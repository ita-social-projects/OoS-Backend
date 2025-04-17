using StackExchange.Redis;
using System.Threading.Tasks;
using System;

namespace OutOfSchool.Redis;
public interface IRedisSubscriptionService
{
    Task SubscribeAsync(string channel, Action<RedisChannel, RedisValue> handler);
    Task UnsubscribeAsync(string channel);
    void PublishNotification(string channelName, string json);    
}
