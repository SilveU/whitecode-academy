namespace Application.Interfaces.Services
{
    public interface ICacheService
    {
        Task<bool> SetAsync<T>(string key, T value, TimeSpan? expiry = null);
        Task<bool> SetIfNotExistsAsync<T>(string key, T value, TimeSpan? expiry = null);
        Task RemoveAsync(string key);
        Task<(bool Success, T?)> GetAsync<T>(string key);
        Task<(bool Success, bool Exists)> ExistsAsync(string key);
        Task RemoveByPrefixAsync(string prefix);
    }
}