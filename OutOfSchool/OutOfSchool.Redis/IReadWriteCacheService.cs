using System.Threading.Tasks;
using System;

namespace OutOfSchool.Redis;

public interface IReadWriteCacheService
{
    Task<string> ReadAsync(string key);

    Task WriteAsync(
        string key,
        string value,
        TimeSpan? absoluteExpirationRelativeToNowInterval = null,
        TimeSpan? slidingExpirationInterval = null);

    Task RemoveAsync(string key);

    Task<TimeSpan?> GetTimeToLiveAsync(string key);
}
