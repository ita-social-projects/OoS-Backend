using Google.Cloud.Storage.V1;
using Microsoft.Extensions.Options;
using OutOfSchool.BusinessLogic.Util.FakeImplementations;
using OutOfSchool.Services.Contexts;
using OutOfSchool.Services.Repository.Files;

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

        if (turnOnFakeStorage)
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
