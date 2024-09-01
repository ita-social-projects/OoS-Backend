using System;
using System.Threading.Tasks;

namespace OutOfSchool.Redis;

public interface ICrudCacheService
{
    Task<string> GetValueAsync(string key);

    public Task UpsertValueAsync(
        string key,
        string value,
        TimeSpan? absoluteExpirationRelativeToNowInterval = null,
        TimeSpan? slidingExpirationInterval = null);

    Task RemoveAsync(string key);
}