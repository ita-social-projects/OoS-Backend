using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OutOfSchool.ExternalFileStore.Models;
using OutOfSchool.ExternalFileStore.NotificationInterfaces;

namespace OutOfSchool.ExternalFileStore.NotificationImplementations;
public class MinioNotificationBackgroundService : BackgroundService, IStorageNotificationService
{
    private readonly IStorageNotificationHandler _notificationHandler;
    private readonly ILogger<MinioNotificationBackgroundService> _logger;
    private readonly string _bucketName;
    private readonly NotificationFilter _filter;
    private readonly MinioRedisNotificationBridge _bridge;
    private bool _isRunning = false;

    public MinioNotificationBackgroundService(
            IStorageNotificationHandler notificationHandler,
            ILogger<MinioNotificationBackgroundService> logger,
            MinioRedisNotificationBridge bridge,
            string bucketName,
            string prefix = "",
            string suffix = "")
    {
        _notificationHandler = notificationHandler;
        _logger = logger;
        _bucketName = bucketName;
        _filter = new NotificationFilter
        {
            Prefix = prefix,
            Suffix = suffix
        };
        _bridge = bridge ?? throw new ArgumentNullException(nameof(bridge));
    }

    public async Task StartListening()
    {
        if (_isRunning)
        {
            _logger.LogInformation("Notification service is already running");
            return;
        }

        _logger.LogInformation("Starting MinIO notification listener for bucket {BucketName}", _bucketName);
        try
        {
            // First start the bridge to connect MinIO events to Redis
            await _bridge.StartBridgeAsync(_bucketName, _filter);
            // Then subscribe to Redis notifications
            await _notificationHandler.Subscribe(_bucketName, _filter);
            _isRunning = true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting notification service");
            throw;
        }
    }

    public async Task StopListening()
    {
        if (!_isRunning)
        {
            _logger.LogInformation("Notification service is not running");
            return;
        }

        _logger.LogInformation("Stopping MinIO notification service");
        try
        {
            await _notificationHandler.Unsubscribe(_bucketName, _filter);
            _bridge.StopBridge(_bucketName, _filter);
            _isRunning = false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error stopping notification service");
            throw;
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await StartListening();

        try
        {
            while (!stoppingToken.IsCancellationRequested && _isRunning)
            {
                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
            // This is expected when the token is canceled, no action needed
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in MinIO notification background service");
        }
        finally
        {
            if (_isRunning)
            {
                await StopListening();
            }
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Service stop requested");

        if (_isRunning)
        {
            await StopListening();
        }

        await base.StopAsync(cancellationToken);
    }
}
