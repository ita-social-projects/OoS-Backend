using Microsoft.Extensions.Logging;
using OutOfSchool.ExternalFileStore.Models;
using OutOfSchool.ExternalFileStore.NotificationInterfaces;
using OutOfSchool.Redis;

namespace OutOfSchool.ExternalFileStore.NotificationImplementations;
public class RedisStorageNotificationHandler : IStorageNotificationHandler
{
    private readonly IRedisSubscriptionService _redisSubscriptionService;
    private readonly ILogger<RedisStorageNotificationHandler> _logger;    
    private readonly IProcessNotificationService _processNotificationService;


    public RedisStorageNotificationHandler(
            IRedisSubscriptionService redisConnection,
            ILogger<RedisStorageNotificationHandler> logger,
            IProcessNotificationService processNotificationService)
    {
        _redisSubscriptionService = redisConnection ?? throw new ArgumentNullException(nameof(redisConnection));
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
            
            // Subscribe to the channel
            await _redisSubscriptionService.SubscribeAsync(channelName, (channel, message) =>
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
            
            _logger.LogInformation("Successfully subscribed to Redis channel {Channel}", channelName);
        }        
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error subscribing to Redis channel for bucket {Bucket}", bucket);             
        }
    }

    public async Task Unsubscribe(string bucket, NotificationFilter filter)
    {
        try
        {
            string channelName = GetChannelName(bucket, filter);

            _logger.LogInformation("Unsubscribing from Redis channel {Channel}", channelName);

            await _redisSubscriptionService.UnsubscribeAsync(channelName);
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
}
