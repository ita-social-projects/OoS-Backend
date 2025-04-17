using Microsoft.Extensions.Logging;
using Minio;
using Minio.DataModel.Args;
using Minio.DataModel.Notification;
using OutOfSchool.ExternalFileStore.Models;
using OutOfSchool.ExternalFileStore.NotificationInterfaces;
using OutOfSchool.Redis;
using System.Collections.Concurrent;
using System.Text.Json;

namespace OutOfSchool.ExternalFileStore.NotificationImplementations;
public class MinioRedisNotificationBridge : IDisposable
{    
    private readonly IMinioClient _minioClient;
    private readonly IRedisSubscriptionService _redisSubscriptionService;
    private readonly ILogger<MinioRedisNotificationBridge> _logger;    
    private readonly IProcessNotificationService _processNotificationService;

    private readonly ConcurrentDictionary<string, IDisposable> _subscriptions = new();

    public MinioRedisNotificationBridge(
            IStorageContext<IMinioClient> storageContext,
            IRedisSubscriptionService redisConnection,
            ILogger<MinioRedisNotificationBridge> logger,
            IProcessNotificationService processNotificationService)
    {
        _minioClient = storageContext?.StorageClient ?? throw new ArgumentNullException(nameof(storageContext));
        _redisSubscriptionService = redisConnection ?? throw new ArgumentNullException(nameof(redisConnection));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _processNotificationService = processNotificationService ?? throw new ArgumentNullException(nameof(processNotificationService));
    }

    public async Task StartBridgeAsync(string bucket, NotificationFilter filter)
    {
        string subscriptionKey = GetSubscriptionKey(bucket, filter);

        if (_subscriptions.ContainsKey(subscriptionKey))
        {
            _logger.LogWarning("Bridge already active for bucket {Bucket} with prefix {Prefix} and suffix {Suffix}",
                bucket, filter.Prefix, filter.Suffix);
            return;
        }

        _logger.LogInformation("Starting MinIO to Redis notification bridge for bucket {Bucket}", bucket);

        try
        {
            var events = new List<EventType>
                {
                    EventType.ObjectCreatedAll
                };

            var args = new ListenBucketNotificationsArgs()
                .WithBucket(bucket)
                .WithEvents(events)
                .WithPrefix(filter.Prefix)
                .WithSuffix(filter.Suffix);

            // Get Redis publisher            
            string channelName = GetChannelName(bucket, filter);
           
            // Subscribe to MinIO notifications
            var observable = _minioClient.ListenBucketNotificationsAsync(args);
            var subscription = observable.Subscribe(
                notification =>
                {
                    try
                    {
                        var storageEvent = _processNotificationService.ParseMinioNotification(notification);
                        if (storageEvent != null)
                        {
                            // Publish the event to Redis
                            string json = JsonSerializer.Serialize(storageEvent);
                            _redisSubscriptionService.PublishNotification(channelName, json);                           
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing MinIO notification in bridge");
                    }
                },
                ex => _logger.LogError(ex, "Error in MinIO notification stream for bucket {Bucket}", bucket),
                () => _logger.LogInformation("MinIO notification stream completed for bucket {Bucket}", bucket)
            );

            _subscriptions[subscriptionKey] = subscription;
            _logger.LogInformation("Successfully started MinIO to Redis notification bridge for bucket {Bucket}", bucket);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start MinIO to Redis notification bridge for bucket {Bucket}", bucket);
            throw;
        }
    }

    public void StopBridge(string bucket, NotificationFilter filter)
    {
        string subscriptionKey = GetSubscriptionKey(bucket, filter);

        if (_subscriptions.TryRemove(subscriptionKey, out var subscription))
        {
            subscription.Dispose();
            _logger.LogInformation("Stopped MinIO to Redis bridge for bucket {Bucket}", bucket);
        }
        else
        {
            _logger.LogWarning("No active bridge found for bucket {Bucket} with prefix {Prefix} and suffix {Suffix}",
                bucket, filter.Prefix, filter.Suffix);
        }
    }

    public void Dispose()
    {
        foreach (var subscription in _subscriptions.Values)
        {
            try
            {
                subscription.Dispose();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error disposing MinIO subscription");
            }
        }

        _subscriptions.Clear();
    }

    private static string GetSubscriptionKey(string bucket, NotificationFilter filter)
        => $"{bucket}:{filter.Prefix}:{filter.Suffix}";

    private static string GetChannelName(string bucket, NotificationFilter filter)
        => $"minio-notifications:{bucket}:{filter.Prefix}:{filter.Suffix}";
}
