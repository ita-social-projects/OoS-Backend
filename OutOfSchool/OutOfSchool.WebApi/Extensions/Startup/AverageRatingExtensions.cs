using OutOfSchool.WebApi.Common.QuartzConstants;
using OutOfSchool.WebApi.Services.AverageRatings;
using OutOfSchool.WebApi.Services.StatisticReports;
using Quartz;

namespace OutOfSchool.WebApi.Extensions.Startup;

public static class AverageRatingExtensions
{
    /// <summary>
    /// Adds all essential methods to calculate the average rating for all of the workshops and the providers.
    /// </summary>
    /// <param name="quartz">Quartz Configurator.</param>
    /// <param name="services">Service collection.</param>
    /// <param name="quartzConfig">Quartz configuration.</param>
    /// <exception cref="ArgumentNullException">Whenever the services collection is null.</exception>
    public static void AddAverageRatingCalculating(
        this IServiceCollectionQuartzConfigurator quartz,
        IServiceCollection services,
        QuartzConfig quartzConfig)
    {
        _ = services ?? throw new ArgumentNullException(nameof(services));
        _ = quartzConfig ?? throw new ArgumentNullException(nameof(quartzConfig));

        services.AddScoped<IAverageRatingService, AverageRatingService>();

        var averageRatingCalculatingJobKey = new JobKey(JobConstants.AverageRatingCalculating, GroupConstants.AverageRating);

        quartz.AddJob<AverageRatingQuartzJob>(j => j.WithIdentity(averageRatingCalculatingJobKey));
        quartz.AddTrigger(t => t
            .WithIdentity(JobTriggerConstants.AverageRatingCalculating, GroupConstants.AverageRating)
            .ForJob(averageRatingCalculatingJobKey)
            .StartNow()
            .WithCronSchedule(quartzConfig.CronSchedules.AverageRatingCalculatingCronScheduleString));
    }
}
