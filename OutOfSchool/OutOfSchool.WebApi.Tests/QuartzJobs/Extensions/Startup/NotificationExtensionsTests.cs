using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using OutOfSchool.BackgroundJobs.Config;
using OutOfSchool.BackgroundJobs.Extensions.Startup;
using OutOfSchool.Common.QuartzConstants;
using Quartz;

namespace OutOfSchool.WebApi.Tests.QuartzJobs.Extensions.Startup;

[TestFixture]
public class NotificationExtensionsTests
{
    [Test]
    public void AddOldNotificationsClearing_WhenQuartzConfigIsNull_ThrowsArgumentNullException()
    {
        // Arrange
        var servicesRegistering = new ServiceCollection();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => servicesRegistering.AddQuartz(q => q.AddOldNotificationsClearing(servicesRegistering, null)));
    }

    [Test]
    public async Task AddOldNotificationsClearing_WhenQuartzConfigIsOk_ShouldRegisterJob()
    {
        // Arrange
        var servicesRegistering = new ServiceCollection();

        servicesRegistering.AddSingleton<ILoggerFactory>(new LoggerFactory());

        var quartzConfig = new QuartzConfig()
        {
            CronSchedules = new QuartzCronScheduleConfig()
            {
                NotificationsClearingCronScheduleString = "0 0 0 1 OCT ? *",
            },
        };

        // Act
        servicesRegistering.AddQuartz(q => q.AddOldNotificationsClearing(servicesRegistering, quartzConfig));

        using var services = servicesRegistering.BuildServiceProvider();

        // Assert
        var scheduler = await services.GetRequiredService<ISchedulerFactory>().GetScheduler();

        Assert.IsTrue(await scheduler.CheckExists(new JobKey(JobConstants.NotificationsClearing, GroupConstants.Notifications)));
    }

    [Test]
    public void AddLicenseApprovalNotificationGenerating_WhenQuartzConfigIsNull_ThrowsArgumentNullException()
    {
        // Arrange
        var servicesRegistering = new ServiceCollection();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => servicesRegistering.AddQuartz(q => q.AddLicenseApprovalNotificationGenerating(servicesRegistering, null)));
    }

    [Test]
    public async Task AddLicenseApprovalNotificationGenerating_WhenQuartzConfigIsOk_ShouldRegisterJob()
    {
        // Arrange
        var servicesRegistering = new ServiceCollection();

        servicesRegistering.AddSingleton<ILoggerFactory>(new LoggerFactory());

        var quartzConfig = new QuartzConfig()
        {
            CronSchedules = new QuartzCronScheduleConfig()
            {
                LicenseApprovalNotificationCronScheduleString = "0 0 0 1 OCT ? *",
            },
        };

        // Act
        servicesRegistering.AddQuartz(q => q.AddLicenseApprovalNotificationGenerating(servicesRegistering, quartzConfig));

        using var services = servicesRegistering.BuildServiceProvider();

        // Assert
        var scheduler = await services.GetRequiredService<ISchedulerFactory>().GetScheduler();

        Assert.IsTrue(await scheduler.CheckExists(new JobKey(JobConstants.LicenseApprovalNotification, GroupConstants.Notifications)));
    }
}
