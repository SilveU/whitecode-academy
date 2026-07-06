using Application.DTOs.Authentication;

namespace Application.Interfaces.Authentecation
{
    public interface IPhoneOtpService
    {
        Task SendOtpAsync(string phoneNumber);
        Task<bool> VerifyOtpAsync(string phoneNumber, string code);
        Task<AuthResponse> ResendAsync(string phone);
    }
} 