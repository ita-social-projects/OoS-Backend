using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OutOfSchool.ExternalFileStore.Config;
using OutOfSchool.ExternalFileStore.NotificationImplementations;

namespace OutOfSchool.BackgroundJobs.Extensions.Startup;
public static class MinioNotificationExtension
{
    public static IServiceCollection AddMinioNotification(this IServiceCollection services)
    {
        services.AddHostedService(provider => {            
            var storageOptions = provider.GetRequiredService<IOptions<StorageOptions>>().Value;
            var amazonS3 = storageOptions.Providers.AmazonS3;

            return new MinioNotificationBackgroundService(
                provider.GetRequiredService<ILogger<MinioNotificationBackgroundService>>(),
                provider.GetRequiredService<MinioNotificationListener>(),
                amazonS3.AccessKey
            );
        });

        return services;
    }
}