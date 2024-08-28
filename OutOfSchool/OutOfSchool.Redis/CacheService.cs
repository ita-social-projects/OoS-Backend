using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace OutOfSchool.Redis;

public class CacheService : ICacheService, ICrudCacheService, IDisposable
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
                var value = GetValue<T>(key);

                if (value != null)
                {
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
                    SetValue(key, returnValue, absoluteExpirationRelativeToNowInterval, slidingExpirationInterval);
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
        => this.ExecuteRedisMethod(() =>
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

    public async Task<T> GetValueAsync<T>(string key)
    {
        T returnValue = default;

        await ExecuteRedisMethod(() =>
        {
            cacheLock.EnterReadLock();
            try
            {
                returnValue = GetValue<T>(key);
            }
            finally
            {
                cacheLock.ExitReadLock();
            }
        });
        return returnValue;
    }

    public async Task SetValueAsync<T>(string key, T value, TimeSpan? absoluteExpirationRelativeToNowInterval = null, TimeSpan? slidingExpirationInterval = null)
    {
        await this.ExecuteRedisMethod(() =>
        {
            SetValue(key, value, absoluteExpirationRelativeToNowInterval, slidingExpirationInterval);
        });
    }

    private T GetValue<T>(string key)
    {
        T returnValue = default;
        string value = cache.GetString(key);

        if (value != null)
        {
            returnValue = JsonConvert.DeserializeObject<T>(value);
        }
        return returnValue;
    }

    private void SetValue<T>(string key, T value, TimeSpan? absoluteExpirationRelativeToNowInterval = null, TimeSpan? slidingExpirationInterval = null)
    {
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = absoluteExpirationRelativeToNowInterval ?? redisConfig.AbsoluteExpirationRelativeToNowInterval,
            SlidingExpiration = slidingExpirationInterval ?? redisConfig.SlidingExpirationInterval,
        };
        cache.SetString(key, JsonConvert.SerializeObject(value), options);
    }

    public async Task<string?> GetValueFromCacheAsync(string key)
    {
        string? returnValue = null;
        await this.ExecuteRedisMethod(() =>
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

    public async Task SetValueToCacheAsync(string key, string value, TimeSpan? absoluteExpirationRelativeToNowInterval = null, TimeSpan? slidingExpirationInterval = null)
    {
        await this.ExecuteRedisMethod(() =>
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

    public Task RemoveFromCacheAsync(string key) => RemoveAsync(key);

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