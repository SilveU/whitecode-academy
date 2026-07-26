using API.Attributes;
using Application.DTOs.Authentication;
using Application.Interfaces.Authentecation;
using Application.Resources;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Localization;

namespace API.Controllers.Authentecation
{
    [ApiController]
    [Route("api/[controller]")]
    [EnableRateLimiting("AuthPolicy")]
    public class PasswordController : ControllerBase
    {
        private readonly IResetPasswordService _resetPasswordService;

        public PasswordController(IResetPasswordService resetPasswordService)
        {
            _resetPasswordService = resetPasswordService;
        }

        // Resolves a MessageKey to the localized string per-request
        private string Resolve(string? key)
        {
            if (string.IsNullOrWhiteSpace(key)) return key ?? string.Empty;
            var localizer = HttpContext.RequestServices
                .GetRequiredService<IStringLocalizer<CommonMessages>>();
            return localizer[key].Value;
        }

        private AuthResponse Translate(AuthResponse response) =>
            response with { Message = Resolve(response.Message) };
            
        [HttpPost("reset-password")]
        [SkipTokenRevocation]
        public async Task<IActionResult> ResetPassword([FromBody] EmailResetPasswordRequest request)
        {
            var result = await _resetPasswordService.ResetPassword(request.Email);

            return Ok(Translate(result));
        }

        [HttpGet("confirm-reset-password")]
        [SkipTokenRevocation]
        public async Task<IActionResult> ConfirmResetPassword([FromQuery] string userId, [FromQuery] string token, [FromBody] NewPasswordRequest request)
        {
            var result = await _resetPasswordService.ConfirmResetPasswordAsync(userId, token, request);

            if (!result.IsAuthenticated)
                return BadRequest(Translate(result));

            return Ok(Translate(result));
        }

        [HttpPost("resend-reset-password")]
        [EnableRateLimiting("VerifyPolicy")]
        [SkipTokenRevocation]
        public async Task<IActionResult> ResendResetPassword([FromBody] EmailResetPasswordRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email))
                return BadRequest();

            var result = await _resetPasswordService.ResendResetPasswordAsync(request.Email);

            return Ok(Translate(result));
        }
    }
}