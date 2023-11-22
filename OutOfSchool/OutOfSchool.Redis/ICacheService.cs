using System;
using System.Threading.Tasks;

namespace OutOfSchool.Redis;

public interface ICacheService
{
    Task<T> GetOrAddAsync<T>(
        string key,
        Func<Task<T>> newValueFactory,
        TimeSpan? absoluteExpirationRelativeToNowInterval = null,
        TimeSpan? slidingExpirationInterval = null);

    Task RemoveAsync(string key);
}

public interface IMultiLayerCacheService : ICacheService
{

}