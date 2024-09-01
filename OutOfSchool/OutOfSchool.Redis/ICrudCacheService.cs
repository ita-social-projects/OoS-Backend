using System;
using System.Threading.Tasks;

namespace OutOfSchool.Redis;

public interface ICrudCacheService
{
    Task<string> GetValueAsync(string key);

    public Task UpsertValueAsync<T>(
        string key,
        T value,
        TimeSpan? absoluteExpirationRelativeToNowInterval = null,
        TimeSpan? slidingExpirationInterval = null);

    Task RemoveAsync(string key);
}