using StackExchange.Redis;
using System.Threading.Tasks;
using System;

namespace OutOfSchool.Redis;
public interface IRedisSubscriptionService
{
    IDisposable SubscribeAsync(string channel, Action<RedisChannel, RedisValue> handler);
    Task UnsubscribeAsync(string channel);   
    bool IsConnected();
    IDatabase GetDatabase();
}
