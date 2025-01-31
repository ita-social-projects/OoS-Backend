using System.ComponentModel.DataAnnotations;
using Google.Cloud.Storage.V1;
using Minio;
using OutOfSchool.BusinessLogic.Util.FakeImplementations;
using OutOfSchool.ExternalFileStore;
using OutOfSchool.ExternalFileStore.Config;
using OutOfSchool.ExternalFileStore.Extensions;
using OutOfSchool.ExternalFileStore.Gcs;
using OutOfSchool.ExternalFileStore.S3;

namespace OutOfSchool.WebApi.Extensions.Startup;

public static class FileStorageExtensions
{
    /// <summary>
    /// Adds images storage into the services.
    /// </summary>
    /// <param name="services">Service collection.</param>
    /// <param name="options"><see cref="StorageOptions"/> configuration options</param>
    /// <param name="isImagesFeatureEnabled">Parameter that checks whether we have images feature enabled.</param>
    /// <returns><see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">Whenever the services collection is null.</exception>
    /// <exception cref="ValidationException">Whenever the options is not valid for a given provider.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Whenever the provider is not of a allowed type</exception>
    public static IServiceCollection AddImagesStorage(this IServiceCollection services, StorageOptions options,
        bool isImagesFeatureEnabled = false)
    {
        _ = services ?? throw new ArgumentNullException(nameof(services));
        ValidateOptions(options);

        // Use fake storage if images are disabled or fake provider is configured
        if (!isImagesFeatureEnabled || options.Provider == StorageProviderType.Fake)
        {
            return services.AddTransient<IImageStorage, FakeImagesStorage>();
        }

        switch (options.Provider)
        {
            case StorageProviderType.GoogleCloud:
            {
                var googleCredential = options.Providers.GoogleCloud.RetrieveGoogleCredential();
                var storageClient = StorageClient.Create(googleCredential);

                services.AddSingleton<IStorageContext<StorageClient>, GcpStorageContext>(_ =>
                    new GcpStorageContext(storageClient, options.Containers.Images.BucketName));
            
                services.AddScoped<GcpImagesStorage>();
                services.AddScoped<IImageStorage>(p => p.GetRequiredService<GcpImagesStorage>());
                return services.AddScoped<IObjectImageStorage>(p => p.GetRequiredService<GcpImagesStorage>());
            }
            case StorageProviderType.AmazonS3:
            {
                var storageClient = new MinioClient()
                    .WithEndpoint(options.Providers.AmazonS3.ServiceUrl)
                    .WithCredentials(options.Providers.AmazonS3.AccessKey, options.Providers.AmazonS3.SecretKey)
                    .WithRegion(options.Providers.AmazonS3.Region)
                    .WithSSL()
                    .Build();

                services.AddSingleton<IStorageContext<IMinioClient>, S3StorageContext>(_ =>
                    new S3StorageContext(storageClient, options.Containers.Images.BucketName));
            
                services.AddScoped<S3ImagesStorage>();
                services.AddScoped<IImageStorage>(p => p.GetRequiredService<S3ImagesStorage>());
                return services.AddScoped<IObjectImageStorage>(p => p.GetRequiredService<S3ImagesStorage>());
            }
            default:
                throw new ArgumentOutOfRangeException(nameof(options.Provider), 
                    $"Unsupported storage provider: {options.Provider}");
        }
    }

    private static void ValidateOptions(StorageOptions options)
    {
        var validationContext = new ValidationContext(options);
        Validator.ValidateObject(options, validationContext, true);
    }
}