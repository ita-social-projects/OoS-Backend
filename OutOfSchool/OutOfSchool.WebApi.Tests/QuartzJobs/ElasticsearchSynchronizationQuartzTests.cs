using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using OutOfSchool.BackgroundJobs.Jobs;
using OutOfSchool.BusinessLogic.Services;
using Quartz;

namespace OutOfSchool.WebApi.Tests.QuartzJobs;

[TestFixture]
public class ElasticsearchSynchronizationQuartzTests
{
    private readonly Mock<IElasticsearchSynchronizationService> elasticsearchSynchronizationServiceMock = new();
    private readonly Mock<IElasticPinger> elasticPingerMock = new();
    private readonly Mock<IJobExecutionContext> jobExecutionContextMock = new();

    [Test]
    public async Task Execute_WhenElasticPingerIsHealthy_ShouldCallElasticsearchSynchronizationServiceSynchronize()
    {
        // Arrange
        elasticPingerMock.SetupGet(x => x.IsHealthy).Returns(true);
        jobExecutionContextMock.SetupGet(x => x.CancellationToken).Returns(It.IsAny<CancellationToken>());

        var job = new ElasticsearchSynchronizationQuartz(GetServiceProvider());

        // Act
        await job.Execute(jobExecutionContextMock.Object);

        // Assert
        elasticsearchSynchronizationServiceMock.Verify(x => x.Synchronize(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Execute_WhenElasticPingerIsNotHealthy_ShouldNotCallElasticsearchSynchronizationServiceSynchronize()
    {
        // Arrange
        elasticPingerMock.SetupGet(x => x.IsHealthy).Returns(false);
        jobExecutionContextMock.SetupGet(x => x.CancellationToken).Returns(It.IsAny<CancellationToken>());

        var job = new ElasticsearchSynchronizationQuartz(GetServiceProvider());

        // Act
        await job.Execute(jobExecutionContextMock.Object);

        // Assert
        elasticsearchSynchronizationServiceMock.Verify(x => x.Synchronize(It.IsAny<CancellationToken>()), Times.Never);
    }

    private IServiceProvider GetServiceProvider()
    {
        var serviceCollection = new ServiceCollection();

        serviceCollection.AddTransient(sp => elasticsearchSynchronizationServiceMock.Object);
        serviceCollection.AddSingleton(sp => elasticPingerMock.Object);

        return serviceCollection.BuildServiceProvider();
    }
}
