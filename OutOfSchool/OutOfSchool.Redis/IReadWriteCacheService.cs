using System;
using System.Threading.Tasks;

namespace OutOfSchool.Redis;

public interface IReadWriteCacheService : ICacheService
{
    Task<string> ReadAsync(string key);

    Task WriteAsync(
        string key,
        string value,
        TimeSpan? absoluteExpirationRelativeToNowInterval = null,
        TimeSpan? slidingExpirationInterval = null);

    Task<TimeSpan?> GetTimeToLiveAsync(string key);
}
