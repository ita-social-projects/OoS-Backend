using System;
using Castle.Core.Configuration;
using Google.Cloud.Storage.V1;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using OutOfSchool.Services.Contexts;
using OutOfSchool.Services.Contexts.Configuration;
using OutOfSchool.Services.Extensions;
using OutOfSchool.Services.Repository.Files;
using OutOfSchool.WebApi.Common.QuartzConstants;
using OutOfSchool.WebApi.Config;
using OutOfSchool.WebApi.Config.Quartz;
using OutOfSchool.WebApi.Services.Elasticsearch;
using OutOfSchool.WebApi.Services.Gcp;
using OutOfSchool.WebApi.Services.Images;
using OutOfSchool.WebApi.Util;
using OutOfSchool.WebApi.Util.FakeImplementations;
using Quartz;

namespace OutOfSchool.WebApi.Extensions.Startup;

public static class FileStorageExtensions
{
    /// <summary>
    /// Adds images storage into the services.
    /// </summary>
    /// <param name="services">Service collection.</param>
    /// <param name="turnOnFakeStorage">Parameter that checks whether we should use fake storage.</param>
    /// <returns><see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">Whenever the services collection is null.</exception>
    public static IServiceCollection AddImagesStorage(this IServiceCollection services, bool turnOnFakeStorage = false)
    {
        _ = services ?? throw new ArgumentNullException(nameof(services));

        var isDevelopment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == Environments.Development;

        if (turnOnFakeStorage && isDevelopment)
        {
            return services.AddTransient<IImageFilesStorage, FakeImagesStorage>();
        }

        services.AddSingleton(provider =>
        {
            var config = provider.GetRequiredService<IOptions<GcpStorageImagesSourceConfig>>();
            var googleCredential = config.Value.RetrieveGoogleCredential();
            return StorageClient.Create(googleCredential);
        });

        services.AddSingleton<IGcpStorageContext, GcpStorageContext>(provider =>
        {
            var config = provider.GetRequiredService<IOptions<GcpStorageImagesSourceConfig>>();
            var storageClient = provider.GetRequiredService<StorageClient>();
            return new GcpStorageContext(storageClient, config.Value.BucketName);
        });

        return services.AddScoped<IImageFilesStorage, GcpImagesStorage>(provider
            => new GcpImagesStorage(provider.GetRequiredService<IGcpStorageContext>()));
    }

    /// <summary>
    /// Adds all essential methods to synchronize gcp files with the main database.
    /// </summary>
    /// <param name="quartz">Quartz Configurator.</param>
    /// <param name="services">Service collection.</param>
    /// <param name="quartzConfig">Quartz configuration.</param>
    /// <exception cref="ArgumentNullException">Whenever the services collection is null.</exception>
    public static void AddGcpSynchronization(this IServiceCollectionQuartzConfigurator quartz, IServiceCollection services, QuartzConfig quartzConfig)
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