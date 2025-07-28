using OutOfSchool.BackgroundJobs.Config;
using OutOfSchool.Common.QuartzConstants;
using Quartz;

namespace OutOfSchool.BackgroundJobs.Extensions.Startup;
public static class ThumbnailProcessingExtensions
{
    public static void AddThumbnailProcessingJob(
       this IServiceCollectionQuartzConfigurator quartz,
       QuartzConfig quartzConfig)
    {
        _ = quartzConfig ?? throw new ArgumentNullException(nameof(quartzConfig));

        var jobKey = new JobKey(JobConstants.ThumbnailGeneration, GroupConstants.Thumbnails);

        quartz.AddJob<ThumbnailGenerationQuartzJob>(opts => opts.WithIdentity(jobKey));

        quartz.AddTrigger(trigger => trigger
            .WithIdentity(JobTriggerConstants.ThumbnailGeneration, GroupConstants.Thumbnails)
            .ForJob(jobKey)
            .StartNow()
            .WithCronSchedule(quartzConfig.CronSchedules.ThumbnailGenerationCronScheduleString));
    }
}

