namespace Showtime.Api.Infrastructure
{
    using System;
    using StackExchange.Redis;

    public interface ICache
    {
        T Get<T>(string key);
        void Set<T>(string key, T value, TimeSpan expirationTime);
    }

    public class RedisCache : ICache
    {
        private readonly IDatabase _redisCache;

        public RedisCache(IConfiguration configuration)
        {
            var redisHost = configuration["Redis:Host"];
            if (string.IsNullOrEmpty(redisHost)) throw new ArgumentNullException(nameof(redisHost), "Redis host not found");
            var connection = ConnectionMultiplexer.Connect(redisHost);
            _redisCache = connection.GetDatabase();
        }

        public T Get<T>(string key)
        {
            var value = _redisCache.StringGet(key);
            return value.HasValue ? Deserialize<T>(value) : default;
        }

        public void Set<T>(string key, T value, TimeSpan expirationTime)
        {
            var serializedValue = Serialize(value);
            _redisCache.StringSet(key, serializedValue, expirationTime);
        }

        private static T Deserialize<T>(RedisValue value)
        {
            var json = value.ToString();
            return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(json);
        }

        private static string Serialize<T>(T value)
        {
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(value);
            return json;
        }
    }
}
