using OutOfSchool.WebApi.Common.QuartzConstants;
using OutOfSchool.WebApi.Services.ApplicationStatusChange;
using Quartz;

namespace OutOfSchool.WebApi.Extensions.Startup;

public static class ApplicationStatusChangingExtensions
{
    /// <summary>
    /// Adds all essential methods to change application status from Approved to StudyingForYears.
    /// </summary>
    /// <param name="quartz">Quartz Configurator.</param>
    /// <param name="services">Services collection.</param>
    /// <param name="quartzConfig">Quartz configuration.</param>
    /// <exception cref="ArgumentNullException">Whenever the services collection is null.</exception>
    public static void AddApplicationStatusChanging(this IServiceCollectionQuartzConfigurator quartz, IServiceCollection services, QuartzConfig quartzConfig)
    {
        _ = services ?? throw new ArgumentNullException(nameof(services));
        _ = quartzConfig ?? throw new ArgumentNullException(nameof(quartzConfig));

        var applicationStatusChangingJobKey = new JobKey(JobConstants.ApplicationStatusChanging, GroupConstants.ApplicationStatusChange);

        quartz.AddJob<ApplicationStatusChangingJob>(j => j.WithIdentity(applicationStatusChangingJobKey));
        quartz.AddTrigger(t => t
            .WithIdentity(JobTriggerConstants.ApplicationStatusChanging, GroupConstants.ApplicationStatusChange)
            .ForJob(applicationStatusChangingJobKey)
            .StartNow()
            .WithCronSchedule(quartzConfig.CronSchedules.ApplicationStatusChangingCronScheduleString));
    }
}
