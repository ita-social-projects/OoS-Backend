using OutOfSchool.BackgroundJobs.Config;
using OutOfSchool.BackgroundJobs.Jobs;
using OutOfSchool.Common.QuartzConstants;
using Quartz;

namespace OutOfSchool.BackgroundJobs.Extensions.Startup;

public static class SportKindSyncExtensions
{
    public static void AddSportKindSync(
        this IServiceCollectionQuartzConfigurator quartz,
        QuartzConfig quartzConfig)
    {
        _ = quartzConfig ?? throw new ArgumentNullException(nameof(quartzConfig));

        var sportKindSyncJobKey = new JobKey("SportKindSync", GroupConstants.SportKinds);

        quartz.AddJob<SportKindSyncQuartzJob>(j => j.WithIdentity(sportKindSyncJobKey));

        quartz.AddTrigger(t => t
            .WithIdentity("SportKindSyncTrigger", GroupConstants.SportKinds)
            .ForJob(sportKindSyncJobKey)
            .StartNow()
            .WithCronSchedule(
                quartzConfig.CronSchedules.SportKindSyncCronScheduleString,
                x => x.InTimeZone(TimeZoneInfo.FindSystemTimeZoneById("Europe/Kyiv"))
                    .WithMisfireHandlingInstructionDoNothing()
            ));
    }
}