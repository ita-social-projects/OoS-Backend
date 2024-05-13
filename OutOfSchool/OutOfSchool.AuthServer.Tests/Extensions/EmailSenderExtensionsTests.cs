using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using OutOfSchool.AuthorizationServer.Config;
using OutOfSchool.AuthorizationServer.Extensions;
using OutOfSchool.EmailSender.Quartz;
using OutOfSchool.EmailSender.Services;
using Quartz;

namespace OutOfSchool.AuthServer.Tests.Extensions;

[TestFixture]
public class EmailSenderExtensionsTests
{
    [Test]
    public async Task AddEmailSender_WithValidConfig_AddsJobAndTrigger()
    {
        // Arrange
        var servicesRegistering = new ServiceCollection();
        servicesRegistering.AddSingleton<ILoggerFactory>(new LoggerFactory());
        servicesRegistering.AddSingleton<ISendGridAccessibilityService, SendGridAccessibilityService>();
        servicesRegistering.AddTransient<ILogger<EmailSenderJobListener>, Logger<EmailSenderJobListener>>();
        var quartzConfig = new QuartzConfig { CronSchedules = new QuartzCronScheduleConfig { EmailSenderCronScheduleString = "0 0 0 1 * ? *"} };

        // Act
        servicesRegistering.AddQuartz(q => q.AddEmailSender(quartzConfig));
        using var services = servicesRegistering.BuildServiceProvider();

        // Assert
        var scheduler = await services.GetRequiredService<ISchedulerFactory>().GetScheduler();
        Assert.IsTrue(await scheduler.CheckExists(new JobKey(EmailSenderConstants.EmailJob, EmailSenderConstants.EmailGroup)));
    }

    [Test]
    public void AddEmailSender_WithNullConfig_ThrowsArgumentNullException()
    {
        // Arrange
        var servicesRegistering = new ServiceCollection();

        // Act && Assert
        Assert.Throws<ArgumentNullException>(() => servicesRegistering.AddQuartz(q => q.AddEmailSender(null)));
    }
}
