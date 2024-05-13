using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.InMemory.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using OutOfSchool.BackgroundJobs.Extensions.Startup;
using OutOfSchool.BusinessLogic.Config;
using OutOfSchool.Common.QuartzConstants;
using Quartz;

namespace OutOfSchool.WebApi.Tests.QuartzJobs.Extensions.Startup;

[TestFixture]
public class ElasticsearchSynchronizationExtensionTests
{
    [Test]
    public void AddElasticsearchSynchronization_WhenConfigurationIsNull_ThrowsArgumentNullException()
    {
        // Arrange
        var servicesRegistering = new ServiceCollection();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => servicesRegistering.AddQuartz(q => q.AddElasticsearchSynchronization(servicesRegistering, null)));
    }

    [Test]
    public async Task AddElasticsearchSynchronization_WhenQuartzConfigIsOk_ShouldRegisterJob()
    {
        // Arrange
        var servicesRegistering = new ServiceCollection();

        servicesRegistering.AddSingleton<ILoggerFactory>(new LoggerFactory());

        var sectionName = ElasticsearchSynchronizationSchedulerConfig.SectionName;
        var operationsPerTaskString = $"{sectionName}:{nameof(ElasticsearchSynchronizationSchedulerConfig.OperationsPerTask)}";
        var delayBetweenTasksInMillisecondsString = $"{sectionName}:{nameof(ElasticsearchSynchronizationSchedulerConfig.DelayBetweenTasksInMilliseconds)}";

        var configDict = new Dictionary<string, string>()
        {
            { operationsPerTaskString, "1" },
            { delayBetweenTasksInMillisecondsString, "1" },
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configDict)
            .Build();

        // Act
        servicesRegistering.AddQuartz(q => q.AddElasticsearchSynchronization(servicesRegistering, configuration));

        using var services = servicesRegistering.BuildServiceProvider();

        // Assert
        var scheduler = await services.GetRequiredService<ISchedulerFactory>().GetScheduler();

        Assert.IsTrue(await scheduler.CheckExists(new JobKey(JobConstants.ElasticSearchSynchronization, GroupConstants.ElasticSearch)));
    }
}
