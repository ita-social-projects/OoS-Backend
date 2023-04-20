using OutOfSchool.WebApi.Common.QuartzConstants;
using OutOfSchool.WebApi.Services.LicenseApprovalNotification;
using OutOfSchool.WebApi.Services.NotificationsClearing;
using Quartz;

namespace OutOfSchool.WebApi.Extensions.Startup;

public static class NotificationExtensions
{
    /// <summary>
    /// Adds all essential methods to make statistic reports.
    /// </summary>
    /// <param name="quartz">Quartz Configurator.</param>
    /// <param name="services">Service collection.</param>
    /// <param name="quartzConfig">Quartz configuration.</param>
    /// <exception cref="ArgumentNullException">Whenever the services collection is null.</exception>
    public static void AddOldNotificationsClearing(this IServiceCollectionQuartzConfigurator quartz, IServiceCollection services, QuartzConfig quartzConfig)
    {
        _ = services ?? throw new ArgumentNullException(nameof(services));
        _ = quartzConfig ?? throw new ArgumentNullException(nameof(quartzConfig));

        services.AddScoped<INotificationsClearingService, NotificationsClearingService>();

        var notificationsClearingJobKey = new JobKey(JobConstants.NotificationsClearing, GroupConstants.Notifications);

        quartz.AddJob<NotificationsClearingQuartsJob>(j => j.WithIdentity(notificationsClearingJobKey));
        quartz.AddTrigger(t => t
            .WithIdentity(JobTriggerConstants.NotificationsClearing, GroupConstants.Notifications)
            .ForJob(notificationsClearingJobKey)
            .StartNow()
            .WithCronSchedule(quartzConfig.CronSchedules.NotificationsClearingCronScheduleString));
    }

    /// <summary>
    /// Add the method to generate the license approval notification.
    /// </summary>
    /// <param name="quartz">Quartz configurator.</param>
    /// <param name="services">Service collection.</param>
    /// <param name="quartzConfig">Quartz configuration.</param>
    /// <exception cref="ArgumentNullException">Whenever the services collection or the quartzConfig are null.</exception>
    public static void AddLicenseApprovalNotificationGenerating(this IServiceCollectionQuartzConfigurator quartz, IServiceCollection services, QuartzConfig quartzConfig)
    {
        _ = services ?? throw new ArgumentNullException(nameof(services));
        _ = quartzConfig ?? throw new ArgumentNullException(nameof(quartzConfig));

        services.AddScoped<ILicenseApprovalNotificationService, LicenseApprovalNotificationService>();

        var licenseApprovalNotificationJobKey = new JobKey(JobConstants.LicenseApprovalNotification, GroupConstants.Notifications);

        quartz.AddJob<LicenseApprovalNotificationQuartzJob>(j => j.WithIdentity(licenseApprovalNotificationJobKey));
        quartz.AddTrigger(t => t
            .WithIdentity(JobTriggerConstants.LicenseApprovalNotification, GroupConstants.Notifications)
            .ForJob(licenseApprovalNotificationJobKey)
            .StartNow()
            .WithCronSchedule(quartzConfig.CronSchedules.LicenseApprovalNotificationCronScheduleString));
    }

}
