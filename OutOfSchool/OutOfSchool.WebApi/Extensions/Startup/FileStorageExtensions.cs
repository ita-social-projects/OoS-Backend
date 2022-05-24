using System;
using Google.Cloud.Storage.V1;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using OutOfSchool.Services.Contexts;
using OutOfSchool.Services.Contexts.Configuration;
using OutOfSchool.Services.Extensions;
using OutOfSchool.Services.Repository.Files;
using OutOfSchool.WebApi.Util.FakeImplementations;

namespace OutOfSchool.WebApi.Extensions.Startup
{
    public static class FileStorageExtensions
    {
        public static IServiceCollection AddImagesStorage(this IServiceCollection services, bool turnOnFakeStorage = false)
        {
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
    }
}