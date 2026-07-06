using Application.DTOs.Authentication;
using Domain.Entites.Users;

namespace Application.Interfaces.Authentecation
{
    public interface IAuthenticationService
    {
        Task<string> GenerateJwtToken(ApplicationUser user);
        Task<AuthResponse> LoginAsync(LoginRequest request, string? ipAddress);
        Task<AuthResponse> RegisterAsync(RegisterRequest request);
        Task<AuthResponse> RefreshAsync(string refreshToken, string? ipAddress);
        Task<bool> LogoutAsync(string refreshToken, string? ipAddress);
        Task<bool> LogoutAllAsync(string userId, string? ipAddress);
    }
} 