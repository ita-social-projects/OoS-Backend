using System;
using System.Threading;
using System.Threading.Tasks;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.IndexManagement;
using Elastic.Transport;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using OutOfSchool.BusinessLogic.Config;
using OutOfSchool.BusinessLogic.Services;
using OutOfSchool.BusinessLogic.Services.Elasticsearch;
using OutOfSchool.ElasticsearchData.Models;

namespace OutOfSchool.WebApi.Tests.Services.Elasticsearch;

[TestFixture]
public class ElasticIndexEnsureCreatedHostedServiceTests
{
    private const string TestIndexName = "test-index";
    private const int TestCheckConnectivityDelayMs = 200;
    private const int TestConnectionWaitingTimeSec = 1;

    private ElasticIndexEnsureCreatedHostedService service;
    private Mock<IServiceProvider> serviceProviderMock;
    private Mock<ElasticsearchClient> elasticClientMock;
    private Mock<IOptions<ElasticConfig>> elasticOptionsMock;
    private Mock<IServiceScopeFactory> serviceScopeFactoryMock;
    private Mock<IServiceScope> serviceScopeMock;
    private Mock<IElasticsearchHealthService> elasticHealthServiceMock;

    [SetUp]
    public void Setup()
    {
        serviceProviderMock = new Mock<IServiceProvider>();
        elasticClientMock = new Mock<ElasticsearchClient>();
        elasticOptionsMock = new Mock<IOptions<ElasticConfig>>();
        serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
        serviceScopeMock = new Mock<IServiceScope>();
        elasticHealthServiceMock = new Mock<IElasticsearchHealthService>();
        elasticOptionsMock.Setup(x => x.Value).Returns(
            new ElasticConfig
            {
                WorkshopIndexName = TestIndexName,
                CheckConnectivityDelayMs = TestCheckConnectivityDelayMs,
                ConnectionWaitingTimeSec = TestConnectionWaitingTimeSec,
            });
        serviceScopeFactoryMock.Setup(x => x.CreateScope()).Returns(serviceScopeMock.Object);
        serviceProviderMock.Setup(x => x.GetService(typeof(IServiceScopeFactory)))
            .Returns(serviceScopeFactoryMock.Object);
        serviceScopeMock.Setup(x => x.ServiceProvider).Returns(serviceProviderMock.Object);
        serviceProviderMock.Setup(x => x.GetService(typeof(IElasticsearchHealthService)))
            .Returns(elasticHealthServiceMock.Object);

        service = new ElasticIndexEnsureCreatedHostedService(
            serviceProviderMock.Object,
            elasticClientMock.Object,
            elasticOptionsMock.Object,
            new Mock<ILogger<ElasticIndexEnsureCreatedHostedService>>().Object);
    }

    [Test]
    public async Task StartAsync_WithCancelledToken_ShouldExit()
    {
        // Arrange
        using var source = new CancellationTokenSource();
        source.Cancel();

        // Act
        await service.StartAsync(source.Token);

        // Assert
        serviceScopeFactoryMock.Verify(x => x.CreateScope(), Times.Never);
    }

    [Test]
    public async Task StartAsync_WhenElasticIsHealthyAndIndexDoesNotExist_ShouldCreateIndex()
    {
        // Arrange
        IndexName expectedIndexName = TestIndexName;
        Indices testIndices = TestIndexName;
        elasticHealthServiceMock.Setup(x => x.IsHealthy).Returns(true);
        var response = TestableResponseFactory
            .CreateResponse<Elastic.Clients.Elasticsearch.IndexManagement.ExistsResponse>(
            new(), StatusCodes.Status404NotFound, false);
        elasticClientMock.Setup(
            x => x.Indices.ExistsAsync(testIndices, CancellationToken.None))
            .Returns(Task.FromResult(response));

        // Act
        await service.StartAsync(CancellationToken.None);

        // Assert
        serviceScopeFactoryMock.Verify(x => x.CreateScope(), Times.Once);
        serviceProviderMock.Verify(
            x => x.GetService(typeof(IElasticsearchHealthService)),
            Times.Once);
        elasticHealthServiceMock.Verify(x => x.IsHealthy, Times.Exactly(2));
        elasticClientMock.Verify(
            x => x.Indices.ExistsAsync(testIndices, CancellationToken.None), Times.Once);
        elasticClientMock.Verify(
            x => x.Indices.CreateAsync(
            expectedIndexName,
            It.IsAny<Action<CreateIndexRequestDescriptor<WorkshopES>>>(),
            CancellationToken.None),
            Times.Once);
    }

    [Test]
    public async Task StartAsync_WhenElasticIsHealthyAndIndexExist_ShouldExit()
    {
        // Arrange
        IndexName expectedIndexName = TestIndexName;
        Indices testIndices = TestIndexName;
        elasticHealthServiceMock.Setup(x => x.IsHealthy).Returns(true);
        var response = TestableResponseFactory
            .CreateSuccessfulResponse<Elastic.Clients.Elasticsearch.IndexManagement.ExistsResponse>(
            new(), StatusCodes.Status200OK);
        elasticClientMock.Setup(
            x => x.Indices.ExistsAsync(testIndices, CancellationToken.None))
            .Returns(Task.FromResult(response));

        // Act
        await service.StartAsync(CancellationToken.None);

        // Assert
        serviceScopeFactoryMock.Verify(x => x.CreateScope(), Times.Once);
        serviceProviderMock.Verify(
            x => x.GetService(typeof(IElasticsearchHealthService)),
            Times.Once);
        elasticClientMock.Verify(
            x => x.Indices.ExistsAsync(testIndices, CancellationToken.None),
            Times.Once);
        elasticClientMock.Verify(
            x => x.Indices.CreateAsync(
            It.IsAny<IndexName>(),
            It.IsAny<Action<CreateIndexRequestDescriptor<WorkshopES>>>(),
            It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task StartAsync_WhenElasticIsUnhealthy_ShouldRetryAndExit()
    {
        // Arrange
        elasticHealthServiceMock.Setup(x => x.IsHealthy).Returns(false);

        // Act
        await service.StartAsync(CancellationToken.None);

        // Assert
        serviceScopeFactoryMock.Verify(x => x.CreateScope(), Times.Once);
        serviceProviderMock.Verify(
            x => x.GetService(typeof(IElasticsearchHealthService)),
            Times.Once);
        elasticHealthServiceMock.Verify(x => x.IsHealthy, Times.AtLeast(2));
        elasticClientMock.Verify(
            x => x.Indices.ExistsAsync(It.IsAny<Indices>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task StopAsync_ShouldCompleteSuccessfully()
    {
        // Arrange
        var cancellationToken = new CancellationTokenSource().Token;

        // Act
        var task = service.StopAsync(cancellationToken);

        // Assert
        await task;
        Assert.That(task.IsCompleted, Is.True);
        Assert.DoesNotThrowAsync(() => service.StopAsync(cancellationToken));
    }

    [Test]
    public void Constructor_WithValidOptions_ShouldInitializeCorrectly()
    {
        // Arrange
        var validConfig = new ElasticConfig
        {
            WorkshopIndexName = "valid-index",
            CheckConnectivityDelayMs = 100,
            ConnectionWaitingTimeSec = 5,
        };
        elasticOptionsMock.Setup(x => x.Value).Returns(validConfig);

        // Act
        var service = new ElasticIndexEnsureCreatedHostedService(
            serviceProviderMock.Object,
            elasticClientMock.Object,
            elasticOptionsMock.Object,
            new Mock<ILogger<ElasticIndexEnsureCreatedHostedService>>().Object);

        // Assert
        Assert.That(service, Is.Not.Null);
    }

    [Test]
    public void Constructor_WithNullOptions_ShouldThrowArgumentNullException()
    {
        // Arrange
        elasticOptionsMock.Setup(x => x.Value).Returns((ElasticConfig)null);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new ElasticIndexEnsureCreatedHostedService(
            serviceProviderMock.Object,
            elasticClientMock.Object,
            elasticOptionsMock.Object,
            new Mock<ILogger<ElasticIndexEnsureCreatedHostedService>>().Object));
    }

    [Test]
    public void Constructor_WithNullWorkshopIndexName_ShouldThrowArgumentNullException()
    {
        // Arrange
        var invalidConfig = new ElasticConfig
        {
            WorkshopIndexName = null,
            CheckConnectivityDelayMs = 100,
            ConnectionWaitingTimeSec = 5,
        };
        elasticOptionsMock.Setup(x => x.Value).Returns(invalidConfig);

        // Act & Assert
        var ex = Assert.Throws<ArgumentNullException>(() => new ElasticIndexEnsureCreatedHostedService(
            serviceProviderMock.Object,
            elasticClientMock.Object,
            elasticOptionsMock.Object,
            new Mock<ILogger<ElasticIndexEnsureCreatedHostedService>>().Object));

        Assert.That(ex.ParamName, Is.EqualTo("elasticOptions"));
        Assert.That(ex.Message, Does.Contain("WorkshopIndexName is null"));
    }
}
