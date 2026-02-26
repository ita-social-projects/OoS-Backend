using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OutOfSchool.ExternalFileStore.Models;

namespace OutOfSchool.ExternalFileStore.NotificationImplementations;
public class MinioNotificationBackgroundService : BackgroundService
{ 
    private readonly ILogger<MinioNotificationBackgroundService> _logger;
    private readonly MinioNotificationListener _listener;
    private readonly string _bucketName;
    private readonly NotificationFilter _filter;  

    public MinioNotificationBackgroundService(     
            ILogger<MinioNotificationBackgroundService> logger,
            MinioNotificationListener listener,
            string bucketName)
    {
        _listener = listener ?? throw new ArgumentNullException(nameof(listener));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));       
        _bucketName = bucketName;
        _filter = new NotificationFilter
        {
            Prefix = "",
            Suffix = "",
            EventTypes = new List<string>
            {
                    "s3:ObjectCreated:Put",
                    "s3:ObjectCreated:Post",
                    "s3:ObjectCreated:Copy",
                    "s3:ObjectCreated:CompleteMultipartUpload"
            }
        };        
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Starting MinIO notification listener for bucket {BucketName}", _bucketName);

        await _listener.StartListeningAsync(_bucketName, _filter);

        _logger.LogInformation("Notification service is running and listening for bucket {BucketName}", _bucketName);

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await _listener.StopListeningAsync(_bucketName, _filter);
        await base.StopAsync(cancellationToken);
    }
}
