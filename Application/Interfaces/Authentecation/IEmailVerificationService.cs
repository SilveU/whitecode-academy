using Application.DTOs.Authentication;

namespace Application.Interfaces.Authentecation
{
    public interface IEmailVerificationService
    {
        Task<AuthResponse> SendEmailConfirmationAsync(string userId);
        Task<AuthResponse> ConfirmEmailAsync(string userId, string token);
        Task<AuthResponse> ResendEmailConfirmationAsync(string email);
    }
} 