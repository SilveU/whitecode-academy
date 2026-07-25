using Domain.Entites.Users;

namespace Application.Interfaces.Authentecation
{
    public interface IRefreshTokenService
    {
        Task<(string RawToken, RefreshToken RefreshToken)> GenerateAsync(string userId, string? ipAddress);
        Task<int> CleanupAsync();
        Task<bool> RevokeAsync(string userId, string rawToken, string? ipAddress);
        string HashToken(string rawToken);
        Task<RefreshToken?> GetByRawTokenAsync(string rawToken);
        Task<List<RefreshToken>?> GetByListTokenUserIdAsync(string userId);
        Task<(string RawToken, RefreshToken NewRefreshToken)> RotateAsync(RefreshToken oldRefreshToken, string userId, string? ipAddress);
    }
} 