using Microsoft.Extensions.Logging;
using OutOfSchool.ExternalFileStore.Models;
using OutOfSchool.ExternalFileStore.NotificationInterfaces;
using StackExchange.Redis;

namespace OutOfSchool.ExternalFileStore.NotificationImplementations;
public class RedisStorageNotificationHandler : IStorageNotificationHandler, IDisposable
{
    private readonly IConnectionMultiplexer _redisConnection;
    private readonly ILogger<RedisStorageNotificationHandler> _logger;
    private readonly Dictionary<string, ISubscriber> _subscribers = new Dictionary<string, ISubscriber>();
    private readonly IProcessNotificationService _processNotificationService;


    public RedisStorageNotificationHandler(
            IConnectionMultiplexer redisConnection,
            ILogger<RedisStorageNotificationHandler> logger,
            IProcessNotificationService processNotificationService)
    {
        _redisConnection = redisConnection ?? throw new ArgumentNullException(nameof(redisConnection));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));        
        _processNotificationService = processNotificationService ?? throw new ArgumentNullException(nameof(processNotificationService));
    }

    public async Task Subscribe(string bucket, NotificationFilter filter)
    {
        try
        {
            _logger.LogInformation("Subscribing to Redis channel for bucket {Bucket} with prefix {Prefix} and suffix {Suffix}",
                bucket, filter.Prefix, filter.Suffix);

            string channelName = GetChannelName(bucket, filter);

            // Create Redis subscriber
            var subscriber = _redisConnection.GetSubscriber();

            // Subscribe to the channel
            await subscriber.SubscribeAsync(channelName, (channel, message) =>
            {
                try
                {
                    _logger.LogDebug("Received Redis message on channel {Channel}: {Message}", channel, message);
                    _processNotificationService.ProcessNotification(message.ToString());
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing message from Redis channel {Channel}", channel);
                }
            });

            // Store the subscriber for later unsubscribing
            _subscribers[channelName] = subscriber;

            _logger.LogInformation("Successfully subscribed to Redis channel {Channel}", channelName);
        }
        catch (RedisConnectionException ex)
        {
            _logger.LogError(ex, "Redis connection error while subscribing to channel for bucket {Bucket}", bucket);            
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error subscribing to Redis channel for bucket {Bucket}", bucket);
            throw;
        }
    }

    public async Task Unsubscribe(string bucket, NotificationFilter filter)
    {
        try
        {
            string channelName = GetChannelName(bucket, filter);

            _logger.LogInformation("Unsubscribing from Redis channel {Channel}", channelName);

            if (_subscribers.TryGetValue(channelName, out var subscriber))
            {
                await subscriber.UnsubscribeAsync(channelName);
                _subscribers.Remove(channelName);
                _logger.LogInformation("Successfully unsubscribed from Redis channel {Channel}", channelName);
            }
            else
            {
                _logger.LogWarning("No active subscription found for channel {Channel}", channelName);
            }
        }
        catch (RedisConnectionException ex)
        {
            _logger.LogError(ex, "Redis connection error while unsubscribing from channel for bucket {Bucket}", bucket);            
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unsubscribing from Redis channel for bucket {Bucket}", bucket);
            throw;
        }
    }

    private string GetChannelName(string bucket, NotificationFilter filter)
    {
        return $"minio-notifications:{bucket}:{filter.Prefix}:{filter.Suffix}";
    }

    public void Dispose()
    {
        foreach (var subscriber in _subscribers.Values)
        {
            try
            {
                foreach (var channel in _subscribers.Keys)
                {
                    subscriber.Unsubscribe(channel);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unsubscribing during disposal");
            }
        }

        _subscribers.Clear();
    }
}
