﻿using System;
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
    [Test]
    public async Task Execute_WhenElasticPingerIsHealthy_ShouldCallElasticsearchSynchronizationServiceSynchronize()
    {
        // Arrange
        var elasticsearchSynchronizationServiceMock = new Mock<IElasticsearchSynchronizationService>();
        var elasticHealthServiceMock = new Mock<IElasticsearchHealthService>();
        var jobExecutionContextMock = new Mock<IJobExecutionContext>();

        elasticHealthServiceMock.SetupGet(x => x.IsHealthy).Returns(true);
        jobExecutionContextMock.SetupGet(x => x.CancellationToken).Returns(It.IsAny<CancellationToken>());

        var serviceProvider = CreateServiceProvider(elasticsearchSynchronizationServiceMock.Object, elasticHealthServiceMock.Object);

        var job = new ElasticsearchSynchronizationQuartz(serviceProvider);

        // Act
        await job.Execute(jobExecutionContextMock.Object);

        // Assert
        elasticsearchSynchronizationServiceMock.Verify(x => x.Synchronize(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Execute_WhenElasticPingerIsNotHealthy_ShouldNotCallElasticsearchSynchronizationServiceSynchronize()
    {
        // Arrange
        var elasticsearchSynchronizationServiceMock = new Mock<IElasticsearchSynchronizationService>();
        var elasticHealthServiceMock = new Mock<IElasticsearchHealthService>();
        var jobExecutionContextMock = new Mock<IJobExecutionContext>();

        elasticHealthServiceMock.SetupGet(x => x.IsHealthy).Returns(false);
        jobExecutionContextMock.SetupGet(x => x.CancellationToken).Returns(It.IsAny<CancellationToken>());

        var serviceProvider = CreateServiceProvider(elasticsearchSynchronizationServiceMock.Object, elasticHealthServiceMock.Object);

        var job = new ElasticsearchSynchronizationQuartz(serviceProvider);

        // Act
        await job.Execute(jobExecutionContextMock.Object);

        // Assert
        elasticsearchSynchronizationServiceMock.Verify(x => x.Synchronize(It.IsAny<CancellationToken>()), Times.Never);
    }

    private static IServiceProvider CreateServiceProvider(
        IElasticsearchSynchronizationService elasticsearchSynchronizationService,
        IElasticsearchHealthService elasticHealthService)
    {
        var serviceCollection = new ServiceCollection();

        serviceCollection.AddTransient(sp => elasticsearchSynchronizationService);
        serviceCollection.AddSingleton(sp => elasticHealthService);

        return serviceCollection.BuildServiceProvider();
    }
}
