using OutOfSchool.AuthorizationServer.Config;
using OutOfSchool.EmailSender.Quartz;
using Quartz;
using Quartz.Impl.Matchers;

namespace OutOfSchool.AuthorizationServer.Extensions;

public static class EmailSenderExtension
{
    public static void AddEmailSender(
        this IServiceCollectionQuartzConfigurator quartz,
        QuartzConfig quartzConfig)
    {
        _ = quartzConfig ?? throw new ArgumentNullException(nameof(quartzConfig));

        var emailSenderJobKey = new JobKey(EmailSenderConstants.EmailJob, EmailSenderConstants.EmailGroup);

        quartz.AddJob<EmailSenderJob>(j => j.WithIdentity(emailSenderJobKey));
        quartz.AddTrigger(t => t
            .WithIdentity(EmailSenderConstants.EmailSenderJobTrigger, EmailSenderConstants.EmailGroup)
            .StartNow()
            .WithCronSchedule(quartzConfig.CronSchedules.EmailSenderCronScheduleString)
            .ForJob(emailSenderJobKey));

        quartz.AddJobListener<EmailSenderJobListener>(GroupMatcher<JobKey>.GroupEquals(EmailSenderConstants.EmailGroup));
    }
}
