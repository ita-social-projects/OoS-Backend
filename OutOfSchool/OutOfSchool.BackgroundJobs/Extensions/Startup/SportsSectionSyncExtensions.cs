using OutOfSchool.BackgroundJobs.Config;
using OutOfSchool.BackgroundJobs.Jobs;
using OutOfSchool.Common.QuartzConstants;
using Quartz;

namespace OutOfSchool.BackgroundJobs.Extensions.Startup;

public static class SportsSectionSyncExtensions
{
    public static void AddSportsSectionSync(
        this IServiceCollectionQuartzConfigurator quartz,
        QuartzConfig quartzConfig)
    {
        _ = quartzConfig ?? throw new ArgumentNullException(nameof(quartzConfig));

        var jobKey = new JobKey("SportsSectionSync", GroupConstants.SportsSections);

        quartz.AddJob<SportsSectionSyncQuartzJob>(j => j.WithIdentity(jobKey));

        quartz.AddTrigger(t => t
            .WithIdentity("SportsSectionSyncTrigger", GroupConstants.SportsSections)
            .ForJob(jobKey)
            .StartNow()
            .WithCronSchedule(
                quartzConfig.CronSchedules.SportsSectionSyncCronScheduleString
            ));
    }
}
