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
        private ReaderWriterLockSlim cacheLock = new ReaderWriterLockSlim();
        private readonly IDistributedCache cache;
        private readonly RedisConfig redisConfig;

        private bool isWorking = true;
        private readonly bool isEnabled = false;
        private static int lockFlagRedisIsBroken = 0;
        private Timer timer;

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

        public async Task<T> GetOrAdd<T>(string key, Func<Task<T>> newValueFactory)
        {
            T returnValue = default;

            ExecuteRedisMethod(() =>
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
                Set(key, returnValue);
            }

            return returnValue;
        }

        public void Set<T>(string key, T value)
        {
            ExecuteRedisMethod(() => {
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
        }
        
        public void ClearCache(string key)
        {
            ExecuteRedisMethod(() => {
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

        public void Refresh(string key)
        {
            ExecuteRedisMethod(() => {
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
        }

        public void Dispose()
        {
            timer?.Dispose();
            cacheLock?.Dispose();
        }

        private void RedisIsBrokenStartCheck()
        {
            if (Interlocked.CompareExchange(ref lockFlagRedisIsBroken, 1, 0) == 0)
            {
                isWorking = false;

                timer = new Timer(
                cb =>
                {
                    try
                    {
                        cache.GetString("_");
                        isWorking = true;
                        timer?.Dispose();
                        Interlocked.Decrement(ref lockFlagRedisIsBroken);
                    }
                    catch (RedisConnectionException)
                    {
                        isWorking = false;
                    }
                },
                null,
                TimeSpan.Zero,
                redisConfig.CheckAlivePollingInterval);
            }
        }

        private void ExecuteRedisMethod(Action redisMethod)
        {
            if (isEnabled && isWorking)
            {
                try
                {
                    redisMethod();
                }
                catch (RedisConnectionException)
                {
                    RedisIsBrokenStartCheck();
                }
            }
        }
    }
}
