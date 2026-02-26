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

    public IDisposable SubscribeAsync(string channel, Action<RedisChannel, RedisValue> handler)
    {
        try
        {            
            if (!_connection.IsConnected)
            {
                _logger.LogError("Redis connection is not established");
                throw new InvalidOperationException("Redis connection is not established");
            }

            var subscriber = _connection.GetSubscriber();
            subscriber.Subscribe(channel, handler);

            _subscribers[channel] = subscriber;
            _logger.LogDebug("Subscribed to Redis channel {Channel}", channel);
            
            return new RedisSubscription(subscriber, channel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to subscribe to Redis channel {Channel}", channel);
            throw;
        }
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

    public IDatabase GetDatabase()
    {
        return _connection.GetDatabase();
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

    public bool IsConnected()
    {
        return _connection.IsConnected;
    }    
}
