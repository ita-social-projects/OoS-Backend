using Microsoft.Extensions.Logging;
using Minio;
using Minio.DataModel.Args;
using Minio.DataModel.Notification;
using OutOfSchool.ExternalFileStore.Models;
using OutOfSchool.ExternalFileStore.NotificationInterfaces;
using StackExchange.Redis;
using System.Text.Json;

namespace OutOfSchool.ExternalFileStore.NotificationImplementations;
public class MinioRedisNotificationBridge : IDisposable
{    
    private readonly IMinioClient _minioClient;
    private readonly IConnectionMultiplexer _redisConnection;
    private readonly ILogger<MinioRedisNotificationBridge> _logger;
    private readonly Dictionary<string, IDisposable> _subscriptions = new Dictionary<string, IDisposable>();
    private readonly IProcessNotificationService _processNotificationService;

    public MinioRedisNotificationBridge(
            IStorageContext<IMinioClient> storageContext,
            IConnectionMultiplexer redisConnection,
            ILogger<MinioRedisNotificationBridge> logger,
            IProcessNotificationService processNotificationService)
    {
        _minioClient = storageContext?.StorageClient ?? throw new ArgumentNullException(nameof(storageContext));
        _redisConnection = redisConnection ?? throw new ArgumentNullException(nameof(redisConnection));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _processNotificationService = processNotificationService ?? throw new ArgumentNullException(nameof(processNotificationService));
    }

    public async Task StartBridgeAsync(string bucket, NotificationFilter filter)
    {
        string subscriptionKey = $"{bucket}:{filter.Prefix}:{filter.Suffix}";

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
            var publisher = _redisConnection.GetDatabase();
            string channelName = $"minio-notifications:{bucket}:{filter.Prefix}:{filter.Suffix}";

           
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
                            publisher.Publish(channelName, json);
                            _logger.LogDebug("Published event to Redis channel {Channel}", channelName);
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
        string subscriptionKey = $"{bucket}:{filter.Prefix}:{filter.Suffix}";

        if (_subscriptions.TryGetValue(subscriptionKey, out var subscription))
        {
            _logger.LogInformation("Stopping MinIO to Redis notification bridge for bucket {Bucket}", bucket);
            subscription.Dispose();
            _subscriptions.Remove(subscriptionKey);
            _logger.LogInformation("Successfully stopped MinIO to Redis notification bridge for bucket {Bucket}", bucket);
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
}
