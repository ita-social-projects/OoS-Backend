using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OutOfSchool.Redis;

public sealed class MultiLayerCache : IMultiLayerCacheService
{
    private readonly IMemoryCache _memoryCache;
    private readonly ICacheService _cacheService;
    private readonly MemoryCacheConfig _memoryCacheConfig;

    public MultiLayerCache(
        ICacheService externalCache,
        IMemoryCache inMemoryCache,
        IOptions<MemoryCacheConfig> memoryCacheConfig
    )
    {
        _memoryCache = inMemoryCache;
        _cacheService = externalCache;
        _memoryCacheConfig = memoryCacheConfig.Value;
    }

    public async Task RemoveAsync(string key)
    {
        _memoryCache.Remove(key);
        await _cacheService.RemoveAsync(key);
    }

    public async Task<T> GetOrAddAsync<T>(string key, Func<Task<T>> newValueFactory, TimeSpan? absoluteExpirationRelativeToNowInterval = null, TimeSpan? slidingExpirationInterval = null)
    {
        T returnValue = default;

        if (_memoryCache.TryGetValue(key, out T value))
        {
            returnValue = value;
        }

        if (EqualityComparer<T>.Default.Equals(returnValue, default))
        {
            returnValue = await _cacheService.GetOrAddAsync(key, newValueFactory, absoluteExpirationRelativeToNowInterval, slidingExpirationInterval);
            await SetAsync(key, returnValue, absoluteExpirationRelativeToNowInterval, slidingExpirationInterval);
        }

        return returnValue;
    }

    private Task SetAsync<T>(string key, T value, TimeSpan? absoluteExpirationRelativeToNowInterval = null, TimeSpan? slidingExpirationInterval = null)
        => Task.Run(() =>
        {
            var cacheEntryOptions = new MemoryCacheEntryOptions()
            {
                SlidingExpiration = slidingExpirationInterval ?? _memoryCacheConfig.SlidingExpirationInterval,
                AbsoluteExpirationRelativeToNow = absoluteExpirationRelativeToNowInterval ?? _memoryCacheConfig.AbsoluteExpirationRelativeToNowInterval,
                Priority = CacheItemPriority.Normal,
            };

            _memoryCache.Set(key, value, cacheEntryOptions);
        });
}