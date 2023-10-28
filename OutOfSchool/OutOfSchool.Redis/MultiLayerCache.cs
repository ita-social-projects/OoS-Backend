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

    public async Task RemoveAsync(string key)
    {
        await ExecuteCacheMethod(async () =>
        {
            cacheLock.EnterWriteLock();
            try
            {
                _memoryCache.Remove(key);
                await _cacheService.RemoveAsync(key);
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

        await ExecuteCacheMethod(() =>
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

    public async Task RefreshAsync(string key)
    {
        await _cacheService.RefreshAsync(key);
    }
    
    public void Dispose()
    {
        cacheLock?.Dispose();
    }

    private Task ExecuteCacheMethod(Action operation)
    {
        return Task.Run(operation);
    }

    private async Task SetAsync<T>(string key, T value, TimeSpan? absoluteExpirationRelativeToNowInterval = null, TimeSpan? slidingExpirationInterval = null)
    {
        await ExecuteCacheMethod(() =>
        {
            cacheLock.EnterWriteLock();

            try
            {
                var cacheEntryOptions = new MemoryCacheEntryOptions()
                {
                    SlidingExpiration = slidingExpirationInterval ?? memoryCacheConfig.SlidingExpirationInterval,
                    AbsoluteExpirationRelativeToNow = absoluteExpirationRelativeToNowInterval ?? memoryCacheConfig.AbsoluteExpirationRelativeToNowInterval,
                    Priority = CacheItemPriority.Normal,
                };

                _memoryCache.Set(key, value, cacheEntryOptions);
            }
            finally
            {
                cacheLock.ExitWriteLock();
            }
        });
    }
}
