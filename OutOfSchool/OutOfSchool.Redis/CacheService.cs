using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace OutOfSchool.Redis
{
    public class CacheService : ICacheService, IDisposable
    {
        private readonly IDistributedCache cache;
        private readonly RedisConfig redisConfig;

        private bool isWorking = true;
        private readonly bool isEnabled = false;
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

        public async Task<T> Get<T>(string key)
        {
            try
            {
                if (isEnabled && isWorking)
                {
                    var value = await cache.GetStringAsync(key);

                    if (value != null)
                    {
                        return JsonConvert.DeserializeObject<T>(value);
                    }
                }
            }
            catch (RedisConnectionException)
            {
                RedisIsBrokenStartCheck();
            }

            return default;
        }

        public async Task Set<T>(string key, T value)
        {
            await ExecuteRedisMethod(async () => {
                var options = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(redisConfig.AbsoluteExpirationRelativeToNowFromHours),
                    SlidingExpiration = TimeSpan.FromMinutes(redisConfig.SlidingExpirationFromMinutes)
                };

                await cache.SetStringAsync(key, JsonConvert.SerializeObject(value), options);
            });
        }

        public async Task ClearCache(string key)
        {
            await ExecuteRedisMethod(async () => {
                await cache.RemoveAsync(key);
            });
        }

        public async Task Refresh(string key)
        {
            await ExecuteRedisMethod(async () => {
                await cache.RefreshAsync(key);
            });
        }

        public void Dispose()
        {
            timer?.Dispose();
        }

        private void RedisIsBrokenStartCheck()
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
                    }
                    catch (RedisConnectionException)
                    {
                        isWorking = false;
                    }
                },
                null,
                TimeSpan.Zero,
                TimeSpan.FromSeconds(redisConfig.SecondsChekingIsWorking));
        }

        private async Task ExecuteRedisMethod(Func<Task> redisMethod)
        {
            if (isEnabled && isWorking)
            {
                try
                {
                    await redisMethod();
                }
                catch (RedisConnectionException)
                {
                    RedisIsBrokenStartCheck();
                }
            }
        }
    }
}
