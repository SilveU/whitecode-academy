using System.Net;
using Application.Common;
using Application.DTOs.Authentication;
using Application.Interfaces.Authentecation;
using Application.Interfaces.BackgroundJobs;
using Application.Interfaces.Services;
using Domain.Entites.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Authentecation
{
    public class ResetPasswordService : IResetPasswordService
    {
        private readonly IConfiguration _config;
        private readonly IEmailSender _emailSender;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IApplicationBackgroundJobClient _backgroundJobClient;
        private readonly ICacheService _cache;

        public ResetPasswordService(IConfiguration config, IEmailSender emailSender, UserManager<ApplicationUser> userManager,
        ICacheService cache, IApplicationBackgroundJobClient backgroundJobClient)
        {
            _config = config;
            _emailSender = emailSender;
            _userManager = userManager;
            _cache = cache;
            _backgroundJobClient = backgroundJobClient;
        }

        public async Task<AuthResponse> ResetPassword(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return new AuthResponse
                {
                    Email = email,
                    IsAuthenticated = false,
                    Message = "If this email exists, a confirmation link has been sent."
                };
            }
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var encoded = WebUtility.UrlEncode(token);
            var frontendBaseUrl = _config.GetValue<string>("EmailSettings:FrontendBaseUrl");
            var resetPasswordLink = $"{frontendBaseUrl}/api/authentication/confirm-reset-password?userId={user.Id}&token={encoded}";
            var cooldownMinutes = _config.GetValue<double>("Redis:ResetPasswordResendCooldownMinutes");
            
            var path = Path.Combine(AppContext.BaseDirectory, "Templates", "ResetPassword.html");
            var body = await File.ReadAllTextAsync(path);

            var subject = "Reset Password";
            
            body = body
                .Replace("{{UserName}}", user.UserName!)
                .Replace("{{ResetPasswordUrl}}", resetPasswordLink);

            
            _backgroundJobClient.Enqueue<IEmailSender>(x => x.SendEmailAsync(user.Email!, subject, body));

            // Set cooldown key — prevents resend spam for the configured duration
            var cooldownKey = CacheKeys.ResetPasswordCooldown(user.Id);
            await _cache.SetAsync<bool>(cooldownKey, true, TimeSpan.FromMinutes(cooldownMinutes));

            return new AuthResponse
            {
                Id = user.Id,
                Email = user.Email,
                UserName = user.UserName,
                PhoneNumber = user.PhoneNumber,
                IsAuthenticated = false,
                Message = "Email confirmation link has been sent successfully."
            };
        } 
        public async Task<AuthResponse> ConfirmResetPasswordAsync(string userId, string token, NewPasswordRequest newPassword)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
                return new AuthResponse { Id = userId, IsAuthenticated = false, Message = "User not found." };

            if (string.IsNullOrWhiteSpace(token))
                return new AuthResponse { Id = user.Id, Email = user.Email, UserName = user.UserName,
                PhoneNumber = user.PhoneNumber, IsAuthenticated = false, Message = "Invalid confirmation token." };

            var result = await _userManager.ResetPasswordAsync(user, token, newPassword.NewPassword);

            if (!result.Succeeded)
            {
                return new AuthResponse
                {
                    Id = user.Id,
                    Email = user.Email,
                    UserName = user.UserName,
                    PhoneNumber = user.PhoneNumber,
                    IsAuthenticated = false,
                    Message = string.Join(", ", result.Errors.Select(e => e.Description))
                };
            }

            // Clean up cooldown key once email is confirmed
            await _cache.RemoveAsync(CacheKeys.EmailVerificationCooldown(user.Id));

            return new AuthResponse
            {
                Id = user.Id,
                Email = user.Email,
                UserName = user.UserName,
                PhoneNumber = user.PhoneNumber,
                IsAuthenticated = true,
                Message = "Reset Password Successfully."
            };
        }

        public async Task<AuthResponse> ResendResetPasswordAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return new AuthResponse
                {
                    Email = email,
                    IsAuthenticated = false,
                    Message = "Email is required."
                };
            }

            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                return new AuthResponse
                {
                    Email = email,
                    IsAuthenticated = false,
                    Message = "If this email exists, a confirmation link has been sent."
                };
            }

            if (user.EmailConfirmed)
            {
                return new AuthResponse
                {
                    Id = user.Id,
                    Email = user.Email,
                    UserName = user.UserName,
                    PhoneNumber = user.PhoneNumber,
                    IsAuthenticated = true,
                    Message = "Email is already confirmed."
                };
            }

            // Block resend if cooldown is still active (token was already sent recently)
            var cooldownKey   = CacheKeys.ResetPasswordCooldown(user.Id);
            var cooldownActive = await _cache.GetAsync<bool?>(cooldownKey);

            if (cooldownActive.Item2 is true)
            {
                var cooldownMinutes = _config.GetValue<double>("Redis:EmailVerificationResendCooldownMinutes");
                return new AuthResponse
                {
                    Id = user.Id,
                    Email = user.Email,
                    UserName = user.UserName,
                    PhoneNumber = user.PhoneNumber,
                    IsAuthenticated = false,
                    Message = $"A confirmation email was already sent. Please wait {(int)cooldownMinutes} minutes before requesting a new one."
                };
            }

            return await ResetPassword(user.Email!);
        }
    }
}