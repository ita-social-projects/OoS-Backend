using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OutOfSchool.Redis;

public sealed class MultiLayerCache : IMultiLayerCacheService, IDisposable
{
    private readonly ReaderWriterLockSlim cacheLock = new ReaderWriterLockSlim();
    private readonly IMemoryCache _memoryCache;
    private readonly ICacheService _cacheService;
    private readonly MemoryCacheConfig memoryCacheConfig;

    public MultiLayerCache(
        ICacheService externalCache,
        IMemoryCache inMemoryCache,
        IOptions<MemoryCacheConfig> memoryCacheConfig
    )
    {
        this._memoryCache = inMemoryCache;
        this._cacheService = externalCache;
        this.memoryCacheConfig = memoryCacheConfig.Value;
    }

    public Task ClearCacheAsync(string key)
    {
        return ExecuteMultipleAsync(() =>
        {
            cacheLock.EnterWriteLock();
            try
            {
                _memoryCache.Remove(key);
                _cacheService.ClearCacheAsync(key);
            }
            finally
            {
                cacheLock.ExitWriteLock();
            }
        });
    }

    public async Task<T> GetOrAddAsync<T>(string key, Func<Task<T>> newValueFactory, TimeSpan? absoluteExpirationRelativeToNowInterval = null, TimeSpan? slidingExpirationInterval = null)
    {
        T returnValue = default;

        await ExecuteMultipleAsync(() =>
        {
            cacheLock.EnterReadLock();
            
            try
            {
                if (_memoryCache.TryGetValue(key, out T value))
                {
                    returnValue = value;
                }
            }
            finally
            {
                cacheLock.ExitReadLock();
            }
        });

        if (EqualityComparer<T>.Default.Equals(returnValue, default))
        {
            returnValue = await _cacheService.GetOrAddAsync(key, newValueFactory, absoluteExpirationRelativeToNowInterval, slidingExpirationInterval);
            await SetAsync(key, returnValue, absoluteExpirationRelativeToNowInterval, slidingExpirationInterval);
        }

        return returnValue;
    }

    public Task RefreshAsync(string key)
    {
        return ExecuteMultipleAsync(() =>
        {
            cacheLock.EnterWriteLock();
            try
            {
                _cacheService.RefreshAsync(key);
            }
            finally
            {
                cacheLock.ExitWriteLock();
            }
        });
    }

    public Task SetAsync<T>(string key, T value, TimeSpan? absoluteExpirationRelativeToNowInterval = null, TimeSpan? slidingExpirationInterval = null)
    {
        return ExecuteMultipleAsync(() =>
        {
            cacheLock.EnterWriteLock();

            try
            {
                var cacheEntryOptions = new MemoryCacheEntryOptions()
                {
                    SlidingExpiration = slidingExpirationInterval ?? memoryCacheConfig.SlidingExpirationInterval,
                    AbsoluteExpirationRelativeToNow = absoluteExpirationRelativeToNowInterval ?? memoryCacheConfig.AbsoluteExpirationRelativeToNowInterval,
                    Priority = CacheItemPriority.Normal,
                    Size = 1024,
                };

                _memoryCache.Set(key, value, cacheEntryOptions);
            }
            finally
            {
                cacheLock.ExitWriteLock();
            }
        });
    }

    public void Dispose()
    {
        cacheLock?.Dispose();
    }

    private Task ExecuteMultipleAsync(Action operation)
    {
        return Task.Run(operation);
    }
}
