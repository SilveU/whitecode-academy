using System.Diagnostics;
using Application.Helper;
using Application.Interfaces.Services;
using Infrastructure.Metrics;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Infrastructure.Services.Server
{
    public class RedisService : ICacheService
    {
        private readonly IDatabase _database;
        private readonly IServer _server;
        private readonly ILogger<RedisService> _logger;

        public RedisService(
            IConnectionMultiplexer connection,
            ILogger<RedisService> logger)
        {
            _database = connection.GetDatabase();
            var endPoint = connection.GetEndPoints().First();
            _server = connection.GetServer(endPoint);
            _logger = logger;
        }

        public async Task<bool> SetAsync<T>(string key, T value, TimeSpan? expiry = null, CancellationToken cancellationToken = default)
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                CustomRedisMetrics.Operation.Add(1, new KeyValuePair<string, object?>("operation", "set"));

                var json = Serializer.Serialize(value);
                var expiration = expiry ?? TimeSpan.FromHours(1);

                return await _database.StringSetAsync(key, json, expiration);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to set cache for key {CacheKey}.", key);

                CustomRedisMetrics.Errors.Add(1, new KeyValuePair<string, object?>("operation", "set"));

                return false;
            }
            finally
            {
                CustomRedisMetrics.OperationDuration.Record(stopwatch.Elapsed.TotalMilliseconds, new KeyValuePair<string, object?>("operation", "set"));
            }
        }

        public async Task<bool> SetIfNotExistsAsync<T>(string key, T value, TimeSpan? expiry = null, CancellationToken cancellationToken = default)
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                CustomRedisMetrics.Operation.Add(1, new KeyValuePair<string, object?>( "operation", "set_if_not_exists"));

                var json = Serializer.Serialize(value);
                var expiration = expiry ?? TimeSpan.FromHours(1);

                return await _database.StringSetAsync(key, json, expiration, When.NotExists);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to set idempotency key {CacheKey}.", key);

                CustomRedisMetrics.Errors.Add(1, new KeyValuePair<string, object?>("operation", "set_if_not_exists"));

                return false;
            }
            finally
            {
                CustomRedisMetrics.OperationDuration.Record(stopwatch.Elapsed.TotalMilliseconds, new KeyValuePair<string, object?>("operation", "set_if_not_exists"));
            }
        }

        public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                CustomRedisMetrics.Operation.Add(1, new KeyValuePair<string, object?>("operation", "delete"));

                await _database.KeyDeleteAsync(key);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to remove cache key {CacheKey}.", key);

                CustomRedisMetrics.Errors.Add(1, new KeyValuePair<string, object?>("operation", "delete"));
            }
            finally
            {
                CustomRedisMetrics.OperationDuration.Record(stopwatch.Elapsed.TotalMilliseconds, new KeyValuePair<string, object?>("operation", "delete"));
            }
        }

        public async Task<(bool Success, T?)> GetAsync<T>(string key, CancellationToken cancellationToken = default)
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                CustomRedisMetrics.Operation.Add(1, new KeyValuePair<string, object?>("operation", "get"));

                var value = await _database.StringGetAsync(key);

                if (value.IsNullOrEmpty)
                {
                    CustomRedisMetrics.CacheMisses.Add(1, new KeyValuePair<string, object?>("operation", "get"));

                    return (false, default(T));
                }

                CustomRedisMetrics.CacheHits.Add(1, new KeyValuePair<string, object?>("operation", "get"));

                return (true, Serializer.Deserialize<T>(value!));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to get cache for key {CacheKey}.", key);

                CustomRedisMetrics.Errors.Add(1, new KeyValuePair<string, object?>("operation", "get"));

                return (false, default(T));
            }
            finally
            {
                CustomRedisMetrics.OperationDuration.Record(stopwatch.Elapsed.TotalMilliseconds, new KeyValuePair<string, object?>("operation", "get"));
            }
        }

        public async Task<(bool Success, bool Exists)> ExistsAsync(string key, CancellationToken cancellationToken = default)
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                CustomRedisMetrics.Operation.Add(1, new KeyValuePair<string, object?>("operation", "exists"));

                return (true, await _database.KeyExistsAsync(key));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to get cache for key {CacheKey}.", key);

                CustomRedisMetrics.Errors.Add(1, new KeyValuePair<string, object?>("operation", "exists"));

                return (false, false);
            }
            finally
            {
                CustomRedisMetrics.OperationDuration.Record(stopwatch.Elapsed.TotalMilliseconds, new KeyValuePair<string, object?>("operation", "exists"));
            }
        }

        public async Task RemoveByPrefixAsync(string prefix, CancellationToken cancellationToken = default)
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                CustomRedisMetrics.Operation.Add(1, new KeyValuePair<string, object?>("operation", "delete_by_prefix"));

                var keys = _server.Keys(_database.Database, $"{prefix}:*").ToArray();

                if (keys.Length == 0)
                    return;

                await _database.KeyDeleteAsync(keys);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to remove cache keys with prefix {Prefix}.", prefix);

                CustomRedisMetrics.Errors.Add(1, new KeyValuePair<string, object?>("operation", "delete_by_prefix"));
            }
            finally
            {
                CustomRedisMetrics.OperationDuration.Record(stopwatch.Elapsed.TotalMilliseconds, new KeyValuePair<string, object?>("operation", "delete_by_prefix"));
            }
        }
    }
}