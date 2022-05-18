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

            return services.AddTransient<IImageFilesStorage, GcpImagesStorage>(provider =>
            {
                var config = provider.GetRequiredService<IOptions<GcpStorageImagesSourceConfig>>();
                var googleCredential = config.Value.RetrieveGoogleCredential();
                var storageClient = StorageClient.Create(googleCredential);
                var storageContext = new GcpStorageContext(storageClient, config.Value.BucketName);
                return new GcpImagesStorage(storageContext);
            });
        }
    }
}