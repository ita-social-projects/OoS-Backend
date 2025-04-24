using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OutOfSchool.ExternalFileStore.Models;
using OutOfSchool.ExternalFileStore.NotificationInterfaces;
using OutOfSchool.Redis;
using StackExchange.Redis;
using System.Collections.Concurrent;

namespace OutOfSchool.ExternalFileStore.NotificationImplementations;
public class MinioNotificationListener
{
    private readonly IRedisSubscriptionService _redisSubscriptionService;
    private readonly ILogger<MinioNotificationListener> _logger;
    private readonly IProcessNotificationService _processNotificationService;
    private readonly ConcurrentDictionary<string, IDisposable> _subscriptions = new();
    private readonly ConcurrentDictionary<string, Timer> _pollers = new();
    private readonly ConcurrentDictionary<string, HashSet<string>> _processedEvents = new();
    private readonly string _hashKey;

    public MinioNotificationListener(
            IRedisSubscriptionService redisConnection,
            ILogger<MinioNotificationListener> logger,
            IProcessNotificationService processNotificationService,
            IConfiguration configuration
            )
    {
        _redisSubscriptionService = redisConnection ?? throw new ArgumentNullException(nameof(redisConnection));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _processNotificationService = processNotificationService ?? throw new ArgumentNullException(nameof(processNotificationService));
        _hashKey = configuration["Redis:HashKey"] ?? throw new InvalidOperationException("Configuration value 'Redis:HashKey' is missing or empty.");
    }

    public async Task StartListeningAsync(string bucketName, NotificationFilter filter)
    {
        try
        {
            if (!_redisSubscriptionService.IsConnected())
            {
                _logger.LogError("Redis connection is not available");
                throw new InvalidOperationException("Redis connection is not available");
            }

            string channelName = GetChannelName(bucketName, filter);
            string pollerKey = GetSubscriptionKey(bucketName, filter);

            if (!_subscriptions.ContainsKey(channelName))
            {
                _logger.LogInformation("Subscribing to Redis channel {Channel}", channelName);
                var subscription = _redisSubscriptionService.SubscribeAsync(channelName, (channel, message) =>
                {
                    _logger.LogDebug("Received notification from channel {Channel}", channel);
                    _processNotificationService.ProcessNotification(message);
                });
            }

            if (!_pollers.ContainsKey(pollerKey))
            {
                _logger.LogInformation("Starting hash poller for bucket {Bucket}", bucketName);
                _processedEvents[pollerKey] = new HashSet<string>();

                // Create a timer to poll for hash changes every 2 seconds
                var timer = new Timer(state => PollForEvents(bucketName, filter), null,
                    TimeSpan.Zero, TimeSpan.FromSeconds(2));

                _pollers[pollerKey] = timer;
                _logger.LogInformation("Successfully started hash poller for bucket {Bucket}", bucketName);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to start listening for MinIO notifications", ex.Message);
            throw;
        }
    }

    private void PollForEvents(string bucketName, NotificationFilter filter)
    {
        try
        {
            var db = _redisSubscriptionService.GetDatabase();
            string pollerKey = GetSubscriptionKey(bucketName, filter);
            
            // Get all entries in the hash
            HashEntry[] entries = db.HashGetAll(_hashKey);

            if (entries.Length > 0)
            {
                _logger.LogDebug("Polling found {Count} entries in hash {HashKey}", entries.Length, _hashKey);
            }

            foreach (var entry in entries)
            {
                string key = entry.Name.ToString();
                string value = entry.Value.ToString();

                // Check if this is for our bucket
                if (key.StartsWith($"{bucketName}/"))
                {
                    // Only process if we haven't seen it before
                    if (!_processedEvents[pollerKey].Contains(key))
                    {
                        try
                        {
                            _processNotificationService.ProcessNotification(value);                           
                            _processedEvents[pollerKey].Add(key);                            
                            db.HashDelete(_hashKey, key);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error processing notification: {Key}", key);
                        }
                    }
                    else
                    {
                        db.HashDelete(_hashKey, key);
                    }
                }
            }

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error polling for MinIO notifications: {Message}", ex.Message);
        }
    }

    public async Task StopListeningAsync(string bucket, NotificationFilter filter)
    {
        string subscriptionKey = GetChannelName(bucket, filter);
        string pollerKey = GetSubscriptionKey(bucket, filter);

        if (_subscriptions.TryRemove(subscriptionKey, out var subscription))
        {
            subscription.Dispose();
            _logger.LogInformation("Stopped Redis notification subscription for bucket {Bucket}", bucket);
        }

        if (_pollers.TryRemove(pollerKey, out var timer))
        {
            timer.Dispose();
            _logger.LogInformation("Stopped Redis hash poller for bucket {Bucket}", bucket);
        }

        if (_processedEvents.TryRemove(pollerKey, out _))
        {
            _logger.LogDebug("Removed processed events cache for bucket {Bucket}", bucket);
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
                _logger.LogError(ex, "Error disposing Redis subscription");
            }
        }

        foreach (var timer in _pollers.Values)
        {
            try
            {
                timer.Dispose();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error disposing polling timer");
            }
        }

        _subscriptions.Clear();
        _pollers.Clear();
        _processedEvents.Clear();
    }

    private static string GetSubscriptionKey(string bucket, NotificationFilter filter)
    => $"{bucket}:{filter.Prefix}:{filter.Suffix}";

    private string GetChannelName(string bucket, NotificationFilter _)    
     =>  $"{_hashKey}:{bucket}";   
}
