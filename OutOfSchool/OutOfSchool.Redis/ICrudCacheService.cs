using System;
using System.Threading.Tasks;

namespace OutOfSchool.Redis;

public interface ICrudCacheService
{
    Task<string?> GetValueFromCacheAsync(string key);

    Task SetValueToCacheAsync(
        string key,
        string value,
        TimeSpan? absoluteExpirationRelativeToNowInterval = null,
        TimeSpan? slidingExpirationInterval = null);

    Task RemoveFromCacheAsync(string key);
}