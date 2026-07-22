using Application.DTOs.Authentication;

namespace Application.Interfaces.Authentecation
{
    public interface IEmailVerificationService
    {
        Task<AuthResponse> SendEmailConfirmationAsync(string userId);
        Task<AuthResponse> ConfirmEmailAsync(string userId, string token);
        Task<AuthResponse> ResendEmailConfirmationAsync(string email);
    }

    public interface IResetPasswordService
    {
        Task<AuthResponse> ResetPassword(string email);
        Task<AuthResponse> ConfirmResetPasswordAsync(string userId, string token, NewPasswordRequest newPassword);
        Task<AuthResponse> ResendResetPasswordAsync(string email);
    }
} 