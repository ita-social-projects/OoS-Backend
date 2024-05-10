using Microsoft.Extensions.DependencyInjection;
using OutOfSchool.BackgroundJobs.Config;
using OutOfSchool.BackgroundJobs.Jobs;
using OutOfSchool.BusinessLogic.Services.Gcp;
using OutOfSchool.Common.QuartzConstants;
using OutOfSchool.Services.Repository.Files;
using Quartz;

namespace OutOfSchool.BackgroundJobs.Extensions.Startup;

public static class GcpStorageSynchronizationExtensions
{
    /// <summary>
    /// Adds all essential methods to synchronize gcp files with the main database.
    /// </summary>
    /// <param name="quartz">Quartz Configurator.</param>
    /// <param name="services">Service collection.</param>
    /// <param name="quartzConfig">Quartz configuration.</param>
    /// <exception cref="ArgumentNullException">Whenever the services collection is null.</exception>
    public static void AddGcpSynchronization(
        this IServiceCollectionQuartzConfigurator quartz,
        IServiceCollection services,
        QuartzConfig quartzConfig)
    {
        _ = services ?? throw new ArgumentNullException(nameof(services));
        _ = quartzConfig ?? throw new ArgumentNullException(nameof(quartzConfig));

        services.AddScoped<IGcpImagesSyncDataRepository, GcpImagesSyncDataRepository>();
        services.AddScoped<IGcpStorageSynchronizationService, GcpImagesStorageSynchronizationService>();

        var gcpImagesJobKey = new JobKey(JobConstants.GcpImagesSynchronization, GroupConstants.Gcp);

        quartz.AddJob<GcpStorageSynchronizationQuartzJob>(j => j.WithIdentity(gcpImagesJobKey));
        quartz.AddTrigger(t => t
            .WithIdentity(JobTriggerConstants.GcpImagesSynchronization, GroupConstants.Gcp)
            .ForJob(gcpImagesJobKey)
            .StartNow()
            .WithCronSchedule(quartzConfig.CronSchedules.GcpImagesSyncCronScheduleString));
    }
}
