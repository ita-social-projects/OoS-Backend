using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Minio;
using OutOfSchool.ExternalFileStore.Config;

namespace OutOfSchool.ExternalFileStore.Extensions.Startup;
public static class MinioClientServiceExtensions
{
    public static IServiceCollection AddMinioClient(this IServiceCollection services)
    {        
        services.AddSingleton(sp =>
        {

            var storageOptions = sp.GetRequiredService<IOptions<StorageOptions>>().Value;
            var amazonS3 = storageOptions.Providers.AmazonS3;

            var uri = new Uri($"https://{amazonS3.ServiceUrl}");

            var minioClient = new MinioClient()
                .WithEndpoint(uri.Host)
                .WithCredentials(amazonS3.AccessKey, amazonS3.SecretKey)
                .WithSSL(uri.Scheme == "https")
                .Build();
            return minioClient;
        });

        services.AddSingleton(provider =>
        {
            var client = provider.GetRequiredService<IMinioClient>();
            if (client is MinioClient minioClient)
                return minioClient;

            throw new InvalidOperationException("IMinioClient is not of type MinioClient");
        });

        return services;
    }
}
