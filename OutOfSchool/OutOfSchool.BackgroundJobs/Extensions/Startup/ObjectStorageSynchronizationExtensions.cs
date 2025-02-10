using Microsoft.Extensions.DependencyInjection;
using OutOfSchool.BackgroundJobs.Config;
using OutOfSchool.BackgroundJobs.Jobs;
using OutOfSchool.Common.QuartzConstants;
using OutOfSchool.ExternalFileStore;
using OutOfSchool.ExternalFileStore.Gcs;
using OutOfSchool.ExternalFileStore.S3;
using OutOfSchool.Services.Repository.Files;
using Quartz;

namespace OutOfSchool.BackgroundJobs.Extensions.Startup;

public static class ObjectStorageSynchronizationExtensions
{
    /// <summary>
    /// Adds all essential methods to synchronize object storage files with the main database.
    /// </summary>
    /// <param name="quartz">Quartz Configurator.</param>
    /// <param name="services">Service collection.</param>
    /// <param name="providerType">Storage provider implementation.</param>
    /// <param name="quartzConfig">Quartz configuration.</param>
    /// <exception cref="ArgumentNullException">Whenever the services collection is null.</exception>
    public static void AddObjectStorageSynchronization(
        this IServiceCollectionQuartzConfigurator quartz,
        IServiceCollection services,
        StorageProviderType providerType,
        QuartzConfig quartzConfig)
    {
        _ = services ?? throw new ArgumentNullException(nameof(services));
        _ = quartzConfig ?? throw new ArgumentNullException(nameof(quartzConfig));

        services.AddScoped<IObjectImagesSyncDataRepository, ObjectImagesSyncDataRepository>();
        switch (providerType)
        {
            case StorageProviderType.GoogleCloud:
                services.AddScoped<IObjectStorageSynchronizationService, GcsImagesStorageSynchronizationService>();
                break;
            case StorageProviderType.AmazonS3:
                services.AddScoped<IObjectStorageSynchronizationService, S3ImagesStorageSynchronizationService>();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(providerType), 
                    $"Unsupported storage provider: {providerType}");
        }

        var gcpImagesJobKey = new JobKey(JobConstants.GcpImagesSynchronization, GroupConstants.Gcp);

        quartz.AddJob<ObjectStorageSynchronizationQuartzJob>(j => j.WithIdentity(gcpImagesJobKey));
        quartz.AddTrigger(t => t
            .WithIdentity(JobTriggerConstants.GcpImagesSynchronization, GroupConstants.Gcp)
            .ForJob(gcpImagesJobKey)
            .StartNow()
            .WithCronSchedule(quartzConfig.CronSchedules.GcpImagesSyncCronScheduleString));
    }
}
