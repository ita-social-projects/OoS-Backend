using System;
using System.Threading.Tasks;

namespace OutOfSchool.Redis;
public interface IRedisCacheService
{
    Task<string?> GetValueFromRedisCacheAsync(string key);

    Task SetValueToRedisCacheAsync(
        string key,
        string value,
        TimeSpan? absoluteExpirationRelativeToNowInterval = null,
        TimeSpan? slidingExpirationInterval = null);

    Task RemoveValueFromRedisCacheAsync(string key);
}
