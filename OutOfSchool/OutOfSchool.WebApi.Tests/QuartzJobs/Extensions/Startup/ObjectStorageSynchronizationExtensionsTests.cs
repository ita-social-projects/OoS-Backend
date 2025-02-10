using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using OutOfSchool.BackgroundJobs.Config;
using OutOfSchool.BackgroundJobs.Extensions.Startup;
using OutOfSchool.Common.QuartzConstants;
using OutOfSchool.ExternalFileStore;
using Quartz;

namespace OutOfSchool.WebApi.Tests.QuartzJobs.Extensions.Startup;

[TestFixture]
public class ObjectStorageSynchronizationExtensionsTests
{
    [Test]
    public void AddGcpSynchronization_WhenQuartzConfigIsNull_ThrowsArgumentNullException()
    {
        // Arrange
        var servicesRegistering = new ServiceCollection();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            servicesRegistering.AddQuartz(q =>
                q.AddObjectStorageSynchronization(servicesRegistering, StorageProviderType.GoogleCloud, null)));
    }

    [Test]
    public async Task AddGcpSynchronization_WhenQuartzConfigIsOk_ShouldRegisterJob()
    {
        // Arrange
        var servicesRegistering = new ServiceCollection();

        servicesRegistering.AddSingleton<ILoggerFactory>(new LoggerFactory());

        var quartzConfig = new QuartzConfig()
        {
            CronSchedules = new QuartzCronScheduleConfig()
            {
                GcpImagesSyncCronScheduleString = "0 0 0 1 OCT ? *",
            },
        };

        // Act
        servicesRegistering.AddQuartz(q =>
            q.AddObjectStorageSynchronization(servicesRegistering, StorageProviderType.GoogleCloud, quartzConfig));

        using var services = servicesRegistering.BuildServiceProvider();

        // Assert
        var scheduler = await services.GetRequiredService<ISchedulerFactory>().GetScheduler();

        Assert.IsTrue(
            await scheduler.CheckExists(new JobKey(JobConstants.GcpImagesSynchronization, GroupConstants.Gcp)));
    }
}