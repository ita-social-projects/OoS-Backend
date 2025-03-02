using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using OutOfSchool.BackgroundJobs.Jobs;
using OutOfSchool.BusinessLogic.Config;
using OutOfSchool.BusinessLogic.Services;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Models.CompetitiveEvents;
using Quartz;

namespace OutOfSchool.WebApi.Tests.QuartzJobs;

[TestFixture]
public class ElasticsearchSynchronizationQuartzTests
{
    [Test]
    public async Task Execute_WhenElasticPingerIsHealthy_ShouldCallElasticsearchWorkshopSynchronizationServiceSynchronize()
    {
        // Arrange
        var elasticsearchWorkshopSynchronizationServiceMock = new Mock<IElasticsearchSynchronizationService<IWorkshopService, Workshop>>();
        var elasticsearchCompetitiveEventSynchronizationServiceMock = new Mock<IElasticsearchSynchronizationService<ICompetitiveEventService, CompetitiveEvent>>();
        var configMock = new Mock<IOptions<ElasticConfig>>();
        var config = new ElasticConfig()
        {
            WorkshopIndexName = "test"
        };

        var elasticHealthServiceMock = new Mock<IElasticsearchHealthService>();
        var jobExecutionContextMock = new Mock<IJobExecutionContext>();

        elasticHealthServiceMock.SetupGet(x => x.IsHealthy).Returns(true);
        jobExecutionContextMock.SetupGet(x => x.CancellationToken).Returns(It.IsAny<CancellationToken>());
        configMock.Setup(x => x.Value).Returns(config);

        var serviceProvider = CreateServiceProvider(
            elasticsearchWorkshopSynchronizationServiceMock.Object,
            elasticsearchCompetitiveEventSynchronizationServiceMock.Object,
            elasticHealthServiceMock.Object,
            configMock.Object);

        var job = new ElasticsearchWorkshopSynchronizationQuartzJob(serviceProvider);

        // Act
        await job.Execute(jobExecutionContextMock.Object);

        // Assert
        elasticsearchWorkshopSynchronizationServiceMock.Verify(x => x.Synchronize(config.WorkshopIndexName, It.IsAny<CancellationToken>()), Times.Once);
    }
    
    [Test]
    public async Task Execute_WhenElasticPingerIsHealthy_ShouldCallElasticsearchCompetitiveEventSynchronizationServiceSynchronize()
    {
        // Arrange
        var elasticsearchWorkshopSynchronizationServiceMock = new Mock<IElasticsearchSynchronizationService<IWorkshopService, Workshop>>();
        var elasticsearchCompetitiveEventSynchronizationServiceMock = new Mock<IElasticsearchSynchronizationService<ICompetitiveEventService, CompetitiveEvent>>();
        var configMock = new Mock<IOptions<ElasticConfig>>();
        var config = new ElasticConfig()
        {
            CompetitiveEventIndexName = "test1"
        };

        var elasticHealthServiceMock = new Mock<IElasticsearchHealthService>();
        var jobExecutionContextMock = new Mock<IJobExecutionContext>();

        elasticHealthServiceMock.SetupGet(x => x.IsHealthy).Returns(true);
        jobExecutionContextMock.SetupGet(x => x.CancellationToken).Returns(It.IsAny<CancellationToken>());
        configMock.Setup(x => x.Value).Returns(config);

        var serviceProvider = CreateServiceProvider(
            elasticsearchWorkshopSynchronizationServiceMock.Object,
            elasticsearchCompetitiveEventSynchronizationServiceMock.Object,
            elasticHealthServiceMock.Object,
            configMock.Object);

        var job = new ElasticsearchCompetitiveEventSynchronizationQuartzJob(serviceProvider);

        // Act
        await job.Execute(jobExecutionContextMock.Object);

        // Assert        
        elasticsearchCompetitiveEventSynchronizationServiceMock.Verify(x => x.Synchronize(config.CompetitiveEventIndexName, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Execute_WhenElasticPingerIsNotHealthy_ShouldNotCallElasticsearchWorkshopSynchronizationServiceSynchronize()
    {
        // Arrange
        var elasticsearchWorkshopSynchronizationServiceMock = new Mock<IElasticsearchSynchronizationService<IWorkshopService, Workshop>>();
        var elasticsearchCompetitiveEventSynchronizationServiceMock = new Mock<IElasticsearchSynchronizationService<ICompetitiveEventService, CompetitiveEvent>>();
        var configMock = new Mock<IOptions<ElasticConfig>>();
        var config = new ElasticConfig()
        {
            WorkshopIndexName = "test"
        };

        var elasticHealthServiceMock = new Mock<IElasticsearchHealthService>();
        var jobExecutionContextMock = new Mock<IJobExecutionContext>();

        elasticHealthServiceMock.SetupGet(x => x.IsHealthy).Returns(false);
        jobExecutionContextMock.SetupGet(x => x.CancellationToken).Returns(It.IsAny<CancellationToken>());
        configMock.Setup(x => x.Value).Returns(config);

        var serviceProvider = CreateServiceProvider(
            elasticsearchWorkshopSynchronizationServiceMock.Object,
            elasticsearchCompetitiveEventSynchronizationServiceMock.Object,
            elasticHealthServiceMock.Object,
            configMock.Object);

        var job = new ElasticsearchWorkshopSynchronizationQuartzJob(serviceProvider);

        // Act
        await job.Execute(jobExecutionContextMock.Object);

        // Assert
        elasticsearchWorkshopSynchronizationServiceMock.Verify(x => x.Synchronize(config.WorkshopIndexName, It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task Execute_WhenElasticPingerIsNotHealthy_ShouldNotCallElasticsearchCompetitiveEventSynchronizationServiceSynchronize()
    {
        // Arrange
        var elasticsearchWorkshopSynchronizationServiceMock = new Mock<IElasticsearchSynchronizationService<IWorkshopService, Workshop>>();
        var elasticsearchCompetitiveEventSynchronizationServiceMock = new Mock<IElasticsearchSynchronizationService<ICompetitiveEventService, CompetitiveEvent>>();
        var configMock = new Mock<IOptions<ElasticConfig>>();
        var config = new ElasticConfig()
        {
            CompetitiveEventIndexName = "test1"
        };

        var elasticHealthServiceMock = new Mock<IElasticsearchHealthService>();
        var jobExecutionContextMock = new Mock<IJobExecutionContext>();

        elasticHealthServiceMock.SetupGet(x => x.IsHealthy).Returns(false);
        jobExecutionContextMock.SetupGet(x => x.CancellationToken).Returns(It.IsAny<CancellationToken>());
        configMock.Setup(x => x.Value).Returns(config);

        var serviceProvider = CreateServiceProvider(
            elasticsearchWorkshopSynchronizationServiceMock.Object,
            elasticsearchCompetitiveEventSynchronizationServiceMock.Object,
            elasticHealthServiceMock.Object,
            configMock.Object);

        var job = new ElasticsearchCompetitiveEventSynchronizationQuartzJob(serviceProvider);

        // Act
        await job.Execute(jobExecutionContextMock.Object);

        // Assert
        elasticsearchCompetitiveEventSynchronizationServiceMock.Verify(x => x.Synchronize(config.CompetitiveEventIndexName, It.IsAny<CancellationToken>()), Times.Never);
    }

    private static IServiceProvider CreateServiceProvider(
        IElasticsearchSynchronizationService<IWorkshopService, Workshop> elasticsearchWorkshopSynchronizationService,
        IElasticsearchSynchronizationService<ICompetitiveEventService, CompetitiveEvent> elasticsearchCompetitiveEventSynchronizationService,
        IElasticsearchHealthService elasticHealthService,
        IOptions<ElasticConfig> config)
    {
        var serviceCollection = new ServiceCollection();

        serviceCollection.AddTransient(sp => elasticsearchWorkshopSynchronizationService);
        serviceCollection.AddTransient(sp => elasticsearchCompetitiveEventSynchronizationService);
        serviceCollection.AddSingleton(sp => elasticHealthService);
        serviceCollection.AddSingleton(sp => config);

        return serviceCollection.BuildServiceProvider();
    }
}
