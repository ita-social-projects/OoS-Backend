using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OutOfSchool.Redis
{
    public class CacheService : ICacheService, IDisposable
    {
        private readonly ReaderWriterLockSlim cacheLock = new ReaderWriterLockSlim();
        private readonly IDistributedCache cache;
        private readonly RedisConfig redisConfig;

        private bool isWorking = true;
        private readonly bool isEnabled = false;

        private readonly object lockObject = new object();

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

        public async Task<T> GetOrAddAsync<T>(string key, Func<Task<T>> newValueFactory)
        {
            T returnValue = default;

            await ExecuteRedisMethod(() =>
            {
                string value = null;
                cacheLock.EnterReadLock();
                try
                {
                    value = cache.GetString(key);
                }
                finally
                {
                    cacheLock.ExitReadLock();
                }

                if (value != null)
                {
                    returnValue = JsonConvert.DeserializeObject<T>(value);
                }
            });

            if (EqualityComparer<T>.Default.Equals(returnValue, default))
            {
                returnValue = await newValueFactory();
                await SetAsync(key, returnValue);
            }

            return returnValue;
        }

        public Task SetAsync<T>(string key, T value)
            => ExecuteRedisMethod(() => {
                var options = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = redisConfig.AbsoluteExpirationRelativeToNowInterval,
                    SlidingExpiration = redisConfig.SlidingExpirationInterval
                };

                cacheLock.EnterWriteLock();
                try
                {
                    cache.SetString(key, JsonConvert.SerializeObject(value), options);
                }
                finally
                {
                    cacheLock.ExitWriteLock();
                }
            });
        
        public Task ClearCacheAsync(string key)
            => ExecuteRedisMethod(() => {
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

        public Task RefreshAsync(string key)
            => ExecuteRedisMethod(() => {
                cacheLock.EnterWriteLock();
                try
                {
                    cache.Refresh(key);
                }
                finally
                {
                    cacheLock.ExitWriteLock();
                }
            });

        public void Dispose()
        {
            cacheLock?.Dispose();
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
}
