using OutOfSchool.BusinessLogic.Common.QuartzConstants;
using OutOfSchool.EmailSender.Quartz;
using Quartz;
using Quartz.Impl.Matchers;

namespace OutOfSchool.WebApi.Extensions;

public static class EmailSenderExtensions
{
    /// <summary>
    /// Adds all essential methods to send emails.
    /// </summary>
    /// <param name="quartz">Quartz Configurator.</param>
    /// <param name="services">Service collection.</param>
    /// <param name="quartzConfig">Quartz configuration.</param>
    /// <exception cref="ArgumentNullException">Whenever the services collection is null.</exception>
    public static void AddEmailSender(
        this IServiceCollectionQuartzConfigurator quartz,
        IServiceCollection services,
        QuartzConfig quartzConfig)
    {
        _ = services ?? throw new ArgumentNullException(nameof(services));
        _ = quartzConfig ?? throw new ArgumentNullException(nameof(quartzConfig));

        var emailSenderJobKey = new JobKey(JobConstants.EmailSender, GroupConstants.Emails);

        quartz.AddJob<EmailSenderJob>(j => j.WithIdentity(emailSenderJobKey));
        quartz.AddTrigger(t => t
            .WithIdentity(JobTriggerConstants.EmailSender, GroupConstants.Emails)
            .ForJob(emailSenderJobKey)
            .StartNow()
            .WithCronSchedule(quartzConfig.CronSchedules.EmailSenderCronScheduleString));

        quartz.AddJobListener<EmailSenderJobListener>(GroupMatcher<JobKey>.GroupEquals(GroupConstants.Emails));
        quartz.AddTriggerListener<EmailSenderJobTriggerListener>(GroupMatcher<TriggerKey>.GroupEquals(GroupConstants.Emails));
    }
}
