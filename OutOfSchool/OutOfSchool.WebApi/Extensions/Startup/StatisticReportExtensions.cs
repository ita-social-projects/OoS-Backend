using OutOfSchool.Services.Repository.Files;
using OutOfSchool.WebApi.Common.QuartzConstants;
using OutOfSchool.WebApi.Services.Gcp;
using OutOfSchool.WebApi.Services.StatisticReports;
using Quartz;

namespace OutOfSchool.WebApi.Extensions.Startup;

public static class StatisticReportExtensions
{
    /// <summary>
    /// Adds all essential methods to make statistic reports.
    /// </summary>
    /// <param name="quartz">Quartz Configurator.</param>
    /// <param name="services">Service collection.</param>
    /// <param name="quartzConfig">Quartz configuration.</param>
    /// <exception cref="ArgumentNullException">Whenever the services collection is null.</exception>
    public static void AddStatisticReportsCreating(this IServiceCollectionQuartzConfigurator quartz, IServiceCollection services, QuartzConfig quartzConfig)
    {
        _ = services ?? throw new ArgumentNullException(nameof(services));
        _ = quartzConfig ?? throw new ArgumentNullException(nameof(quartzConfig));

        services.AddScoped<IStatisticReportsMakingService, StatisticReportsMakingService>();

        var statisticReportsMakingJobKey = new JobKey(JobConstants.StatisticReportsMaking, GroupConstants.StatisticReports);

        quartz.AddJob<StatisticReportsMakingQuartsJob>(j => j.WithIdentity(statisticReportsMakingJobKey));
        quartz.AddTrigger(t => t
            .WithIdentity(JobTriggerConstants.StatisticReportsMaking, GroupConstants.StatisticReports)
            .ForJob(statisticReportsMakingJobKey)
            .StartNow()
            .WithCronSchedule(quartzConfig.CronSchedules.StatisticReportsMakingCronScheduleString));
    }
}
