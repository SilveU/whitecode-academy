namespace Application.Interfaces.Services
{
    public interface ICacheService
    {
        Task<bool> SetAsync<T>(string key, T value, TimeSpan? expiry = null, CancellationToken cancellationToken = default);
        Task<bool> SetIfNotExistsAsync<T>(string key, T value, TimeSpan? expiry = null, CancellationToken cancellationToken = default);
        Task RemoveAsync(string key, CancellationToken cancellationToken = default);
        Task<(bool Success, T?)> GetAsync<T>(string key, CancellationToken cancellationToken = default);
        Task<(bool Success, bool Exists)> ExistsAsync(string key, CancellationToken cancellationToken = default);
        Task RemoveByPrefixAsync(string prefix, CancellationToken cancellationToken = default);
    }
}