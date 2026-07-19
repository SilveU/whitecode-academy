namespace Application.Interfaces.Services
{
    public interface ICacheService
    {
        Task<bool> SetAsync<T>(string key, T value, TimeSpan? expiry = null);
        Task<bool> SetIfNotExistsAsync<T>(string key, T value, TimeSpan? expiry = null);
        Task RemoveAsync(string key);
        Task<T?> GetAsync<T>(string key);
        Task RemoveByPrefixAsync(string prefix);
    }
}