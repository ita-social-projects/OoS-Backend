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
using OutOfSchool.WebApi.Services.Elasticsearch;
using OutOfSchool.WebApi.Services.Gcp;
using OutOfSchool.WebApi.Services.Images;
using OutOfSchool.WebApi.Util;
using OutOfSchool.WebApi.Util.FakeImplementations;
using Quartz;

namespace OutOfSchool.WebApi.Extensions.Startup
{
    public static class FileStorageExtensions
    {
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

        public static IServiceCollection AddGcpSynchronization(this IServiceCollection services, Microsoft.Extensions.Configuration.IConfiguration configuration)
        {
            _ = services ?? throw new ArgumentNullException(nameof(services));

            services.AddScoped<IGcpImagesSyncDataRepository, GcpImagesSyncDataRepository>();
            services.AddScoped<IGcpStorageSynchronizationService, GcpImagesStorageSynchronizationService>();

            var cronSchedule = configuration.GetValue<string>("Quartz:CronSchedules:GcpImagesSyncCronScheduleString");

            var gcpImagesJobKey = new JobKey("gcpImagesJob", "gcp");

            QuartzPool.AddJob<GcpStorageSynchronizationQuartzJob>(j => j.WithIdentity(gcpImagesJobKey));
            QuartzPool.AddTrigger(t => t
                .WithIdentity("gcpImagesJobTrigger", "gcp")
                .ForJob(gcpImagesJobKey)
                .StartNow()
                .WithCronSchedule(cronSchedule));

            return services;
        }
    }
}