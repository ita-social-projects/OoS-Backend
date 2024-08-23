﻿using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace OutOfSchool.Redis;

public class CacheService : ICacheService, IRedisCacheService, IDisposable
{
    private readonly ReaderWriterLockSlim cacheLock = new ReaderWriterLockSlim();
    private readonly IDistributedCache cache;
    private readonly RedisConfig redisConfig;

    private bool isWorking = true;
    private readonly bool isEnabled = false;

    private readonly object lockObject = new object();
    
    private bool isDisposed;

    public CacheService(
        IDistributedCache cache, 
        IOptions<RedisConfig> redisConfig
    )
    {
        this.cache = cache;
            
        try
        {
            this.redisConfig = redisConfig.Value;
            isEnabled = this.redisConfig.Enabled;
        }
        catch (OptionsValidationException)
        {
            isEnabled = false;
        }
    }

    public async Task<T> GetOrAddAsync<T>(
        string key,
        Func<Task<T>> newValueFactory,
        TimeSpan? absoluteExpirationRelativeToNowInterval = null,
        TimeSpan? slidingExpirationInterval = null)
    {
        T returnValue = default;
        bool isExists = false;

        await ExecuteRedisMethod(() =>
        {
            cacheLock.EnterReadLock();
            try
            {
                var value = cache.GetString(key);

                if (value != null)
                {
                    returnValue = JsonConvert.DeserializeObject<T>(value);
                    isExists = true;
                    return;
                }
            }
            finally
            {
                cacheLock.ExitReadLock();
            }
        });

        if (!isExists)
        {
            returnValue = await newValueFactory();
            await ExecuteRedisMethod(() =>
            {
                cacheLock.EnterWriteLock();
                try
                {
                    var options = new DistributedCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = absoluteExpirationRelativeToNowInterval ?? redisConfig.AbsoluteExpirationRelativeToNowInterval,
                        SlidingExpiration = slidingExpirationInterval ?? redisConfig.SlidingExpirationInterval,
                    };

                    cache.SetString(key, JsonConvert.SerializeObject(returnValue), options);
                }
                finally
                {
                    cacheLock.ExitWriteLock();
                }
            });
        }

        return returnValue;
    }

    public Task RemoveAsync(string key)
        => ExecuteRedisMethod(async () => {
            cacheLock.EnterWriteLock();
            try
            {
                await cache.RemoveAsync(key);
            }
            finally
            {
                cacheLock.ExitWriteLock();
            }
        });

    public async Task<string?> GetValueFromRedisCacheAsync(string key)
    {
        string? returnValue = null;
        await ExecuteRedisMethod(() =>
        {
            cacheLock.EnterReadLock();
            try
            {
                returnValue = cache.GetString(key);
            }
            finally
            {
                cacheLock.ExitReadLock();
            }
        });

        return returnValue;
    }

    public async Task SetValueToRedisCacheAsync(string key, string value, TimeSpan? absoluteExpirationRelativeToNowInterval = null, TimeSpan? slidingExpirationInterval = null)
    {
        await ExecuteRedisMethod(() =>
        {
            cacheLock.EnterWriteLock();
            try
            {
                var options = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = absoluteExpirationRelativeToNowInterval ?? redisConfig.AbsoluteExpirationRelativeToNowInterval,
                    SlidingExpiration = slidingExpirationInterval ?? redisConfig.SlidingExpirationInterval,
                };

                cache.SetString(key, value, options);
            }
            finally
            {
                cacheLock.ExitWriteLock();
            }
        });
    }

    public async Task RemoveValueFromRedisCacheAsync(string key)
       => await ExecuteRedisMethod(() =>
       {
           cacheLock.EnterWriteLock();
           try
           {
               cache.Remove(key);
           }
           finally
           {
               cacheLock.ExitWriteLock();
           }
       });

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    
    protected virtual void Dispose(bool disposing)
    {
        if (disposing && !isDisposed)
        {
            cacheLock?.Dispose();
            isDisposed = true;
        }
    }

    private async Task RedisIsBrokenStartCheck()
    {
        if (Monitor.TryEnter(lockObject))
        {
            isWorking = false;

            try
            {
                while (!isWorking)
                {
                    try
                    {
                        cache.GetString("_");
                        isWorking = true;
                    }
                    catch (RedisConnectionException)
                    {
                        await Task.Delay(redisConfig.CheckAlivePollingInterval);
                    }
                }
            }
            finally
            {
                Monitor.Exit(lockObject);
            }
        }
    }

    private async Task ExecuteRedisMethod(Action redisMethod)
    {
        if (isEnabled && isWorking)
        {
            try
            {
                await Task.Run(redisMethod);
            }
            catch (RedisConnectionException)
            {
                _ = Task.Run(RedisIsBrokenStartCheck);
            }
        }
    }
}