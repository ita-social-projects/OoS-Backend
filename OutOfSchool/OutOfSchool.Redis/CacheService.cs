using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using OutOfSchool.Common;

namespace OutOfSchool.Redis;

public class CacheService : ICacheService, IReadWriteCacheService, IDisposable
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
                    returnValue = JsonSerializerHelper.Deserialize<T>(value);
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
                    var options = GetExpirationIntervalOptions(absoluteExpirationRelativeToNowInterval, slidingExpirationInterval);

                    cache.SetString(key, JsonSerializerHelper.Serialize(returnValue), options);
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
    {
        ArgumentException.ThrowIfNullOrEmpty(key);

        return ExecuteRedisMethod(() =>
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
    }

    public async Task<string> ReadAsync(string key)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);

        string returnValue = string.Empty;

        await ExecuteRedisMethod(() =>
        {
            cacheLock.EnterReadLock();
            try
            {
                returnValue = cache.GetString(key) ?? string.Empty;
            }
            finally
            {
                cacheLock.ExitReadLock();
            }
        });

        return returnValue;
    }

    public async Task WriteAsync(
        string key,
        string value,
        TimeSpan? absoluteExpirationRelativeToNowInterval = null,
        TimeSpan? slidingExpirationInterval = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);
        ArgumentException.ThrowIfNullOrEmpty(value);

        await ExecuteRedisMethod(() =>
        {
            cacheLock.EnterWriteLock();
            try
            {
                var options = GetExpirationIntervalOptions(absoluteExpirationRelativeToNowInterval, slidingExpirationInterval);

                cache.SetString(key, value, options);
            }
            finally
            {
                cacheLock.ExitWriteLock();
            }
        });
    }

    public async Task<TimeSpan?> GetTimeToLiveAsync(string key)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);

        TimeSpan? returnValue = null;

        await ExecuteRedisMethod(() =>
        {
            cacheLock.EnterReadLock();
            try
            {
                var redisConnection = redisConfig.GetRedisConnectionString();
                ConnectionMultiplexer connection = ConnectionMultiplexer.Connect(redisConnection);
                IDatabase db = connection.GetDatabase();
                returnValue = db.KeyTimeToLive(key);
            }
            finally
            {
                cacheLock.ExitReadLock();
            }
        });

        return returnValue;
    }

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

    private DistributedCacheEntryOptions GetExpirationIntervalOptions(
                                                                      TimeSpan? absoluteExpirationRelativeToNowInterval = null,
                                                                      TimeSpan? slidingExpirationInterval = null)
    {
        var absoluteExpiration = absoluteExpirationRelativeToNowInterval
                ?? redisConfig?.AbsoluteExpirationRelativeToNowInterval
                ?? throw new ArgumentNullException(nameof(redisConfig.AbsoluteExpirationRelativeToNowInterval)); // throw exception if null

        TimeSpan? slidingExpiration;
        if (!slidingExpirationInterval.HasValue)
        {
            // Null => use the default from config
            slidingExpiration = redisConfig.SlidingExpirationInterval;
        }
        else if (slidingExpirationInterval.Value <= TimeSpan.Zero)
        {
            // Zero or negative => explicitly disable sliding expiration
            slidingExpiration = null;
        }
        else
        {
            // Positive => use the provided TimeSpan
            slidingExpiration = slidingExpirationInterval;
        }

        return new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = absoluteExpiration,
            SlidingExpiration = slidingExpiration
        };
    }
}