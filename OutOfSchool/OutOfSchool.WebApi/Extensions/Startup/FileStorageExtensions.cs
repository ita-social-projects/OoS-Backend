using Google.Cloud.Storage.V1;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OutOfSchool.Services.Contexts;
using OutOfSchool.Services.Contexts.Configuration;
using OutOfSchool.Services.Extensions;
using OutOfSchool.Services.Repository.Files;

namespace OutOfSchool.WebApi.Extensions.Startup
{
    public static class FileStorageExtensions
    {
        public static IServiceCollection AddImagesStorage(this IServiceCollection services)
        {
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