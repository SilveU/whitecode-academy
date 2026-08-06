using Application.Helper;
using Application.Interfaces.Services;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Infrastructure.Services.Server
{
    public class RedisService : ICacheService
    {
        private readonly IDatabase _database;
        private readonly IServer _server;
        private readonly ILogger<RedisService> _logger;

        public RedisService(IConnectionMultiplexer connection, ILogger<RedisService> logger)
        {
            _database = connection.GetDatabase();
            var endPoint = connection.GetEndPoints().First();
            _server = connection.GetServer(endPoint);
            _logger = logger;
        }

        public async Task<bool> SetAsync<T>(string key, T value, TimeSpan? expiry = null, CancellationToken cancellationToken = default)
        {
            try
            {
                var json = Serializer.Serialize(value);
                var expiration = expiry ?? TimeSpan.FromHours(1);
                return await _database.StringSetAsync(key, json, expiration);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to set cache for key {CacheKey}.", key);
                return false;
            }
        }

        public async Task<bool> SetIfNotExistsAsync<T>(string key, T value, TimeSpan? expiry = null, CancellationToken cancellationToken = default)
        {
            try
            {
                var json = Serializer.Serialize(value);
                var expiration = expiry ?? TimeSpan.FromHours(1);
                return await _database.StringSetAsync(key, json, expiration, When.NotExists);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to set idempotency key {CacheKey}.", key);
                return false;
            }
        }

        public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
        {
            try
            {
                await _database.KeyDeleteAsync(key);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to remove cache key {CacheKey}.", key);
            }
        }

        public async Task<(bool Success, T?)> GetAsync<T>(string key, CancellationToken cancellationToken = default)
        {
            try
            {
                var value = await _database.StringGetAsync(key);

                if (value.IsNullOrEmpty)
                    return (false, default(T));

                return (true, Serializer.Deserialize<T>(value!));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to get cache for key {CacheKey}.", key);
                return (false, default(T));
            }
        }

        public async Task<(bool Success, bool Exists)> ExistsAsync(string key, CancellationToken cancellationToken = default)
        {
            try
            {
                return (true, await _database.KeyExistsAsync(key));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to get cache for key {CacheKey}.", key);
                return (false, false);
            }
        }

        public async Task RemoveByPrefixAsync(string prefix, CancellationToken cancellationToken = default)
        {
            try
            {
                var keys = _server.Keys(_database.Database, $"{prefix}:*").ToArray();

                if (keys.Length == 0)
                    return;

                await _database.KeyDeleteAsync(keys);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to remove cache keys with prefix {Prefix}.", prefix);
            }
        }
    }
}
