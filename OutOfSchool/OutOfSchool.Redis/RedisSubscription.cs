using StackExchange.Redis;
using System;

namespace OutOfSchool.Redis;
internal class RedisSubscription : IDisposable
{
    private readonly ISubscriber _subscriber;
    private readonly string _channel;

    public RedisSubscription(ISubscriber subscriber, string channel)
    {
        _subscriber = subscriber;
        _channel = channel;
    }

    public void Dispose()
    {
        _subscriber.Unsubscribe(_channel);
    }
}
