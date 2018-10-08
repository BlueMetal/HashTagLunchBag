using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Redis;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Polly;

namespace LunchBag.Common.Managers
{
    public class RedisCacheService : ICacheService
    {
        private readonly ILogger<RedisCacheService> _logger;
        private readonly RedisCache _redisCache;
        private readonly Policy _retryPolicy;

        private readonly JsonSerializerSettings _settings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.All
        };

        public RedisCacheService(IOptions<RedisCacheOptions> cacheOptions, ILogger<RedisCacheService> logger)
        {
            _logger = logger;
            _redisCache = new RedisCache(cacheOptions);

            _retryPolicy = Policy.Handle<Exception>(e =>
            {
                _logger.LogWarning("Issue while fetching from Redis.\n" + e);
                return true;
            })
            .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromMilliseconds(500));
        }

        public async Task<T> Get<T>(string key)
        {
            var result = await _retryPolicy.ExecuteAsync(async () =>
            {
                await Task.Delay(10);

                var bytes = _redisCache.Get(key);
                if (bytes == null) return default(T);

                return (T)JsonConvert.DeserializeObject(Encoding.UTF8.GetString(bytes), _settings);
            });
            return result;
        }

        public async Task Set(string key, object value, TimeSpan duration)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            if (key == null) throw new ArgumentNullException(nameof(key));

            await _retryPolicy.ExecuteAsync(async () =>
            {
                await Task.Delay(10);

                if (key != null) {
                    _redisCache.Set(key, Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(value, _settings)),
                        new DistributedCacheEntryOptions {
                            AbsoluteExpirationRelativeToNow = duration
                        });
                }
            });
        }

        public async Task Set(string key, object value)
        {
            await Set(key, value, TimeSpan.FromDays(1));
        }
   }
}