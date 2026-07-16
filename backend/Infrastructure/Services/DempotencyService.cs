using System.Text.Json;
using Application.Interfaces.Services;

namespace Infrastructure.Services
{
    public class IdempotencyService : IIdempotencyService
    {
        private readonly ICacheService _cache;

        public IdempotencyService(ICacheService cache)
        {
            _cache = cache;
        }

        public async Task<bool> TryAcquireAsync(string key, string entityId, TimeSpan expiration)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Redis key is required.", nameof(key));

            if(string.IsNullOrWhiteSpace(entityId))
                throw new ArgumentException("Payment transaction ID is required.", nameof(entityId));

            return await _cache.SetIfNotExistsAsync(key, entityId, expiration);
        }
    }
}