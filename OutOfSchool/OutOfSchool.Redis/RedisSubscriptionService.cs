using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OutOfSchool.Redis;
public class RedisSubscriptionService : IRedisSubscriptionService, IDisposable
{
    private readonly IConnectionMultiplexer _connection;
    private readonly Dictionary<string, ISubscriber> _subscribers = new();
    private readonly ILogger<RedisSubscriptionService> _logger;

    public RedisSubscriptionService(IConnectionMultiplexer connection, ILogger<RedisSubscriptionService> logger)
    {
        _connection = connection ?? throw new ArgumentNullException(nameof(connection));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task SubscribeAsync(string channel, Action<RedisChannel, RedisValue> handler)
    {
        var subscriber = _connection.GetSubscriber();
        await subscriber.SubscribeAsync(channel, handler);
        
        _subscribers[channel] = subscriber;
        _logger.LogDebug("Subscribed to Redis channel {Channel}", channel);
    }

    public async Task UnsubscribeAsync(string channel)
    {
        if (_subscribers.TryGetValue(channel, out var subscriber))
        {
            await subscriber.UnsubscribeAsync(channel);
            _subscribers.Remove(channel);
            _logger.LogDebug("Unsubscribed from Redis channel {Channel}", channel);
        }
    }   

    public void PublishNotification(string channelName, string json)
    {
        var publisher = _connection.GetDatabase();
        publisher.Publish(channelName, json);
        _logger.LogDebug("Published event to Redis channel {Channel}", channelName);
    }

    public void Dispose()
    {
        foreach (var kvp in _subscribers)
        {
            try
            {
                kvp.Value.Unsubscribe(kvp.Key);
            }
            catch (Exception ex)
            {         
                _logger.LogError($"Error unsubscribing from {kvp.Key}: {ex.Message}");
            }
        }
        _subscribers.Clear();
    }
}
