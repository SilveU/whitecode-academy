using API.Attributes;
using API.Controllers.Common;
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
    public class EmailVerificationController : BaseController
    {
        private readonly IEmailVerificationService _emailVerificationService;

        public EmailVerificationController(IEmailVerificationService emailVerificationService)
        {
            _emailVerificationService = emailVerificationService;
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

        [HttpGet("confirm-email")]
        [SkipTokenRevocation]
        public async Task<IActionResult> ConfirmEmail([FromQuery] string userId, [FromQuery] string token)
        {
            var result = await _emailVerificationService.ConfirmEmailAsync(userId, token);

            return Ok(Translate(result));
        }

        [HttpPost("resend-email-confirmation")]
        [EnableRateLimiting("VerifyPolicy")]
        [SkipTokenRevocation]
        public async Task<IActionResult> ResendEmailConfirmation([FromBody] ResendEmailConfirmationRequest request)
        {
            var result = await _emailVerificationService.ResendEmailConfirmationAsync(request.Email);

            if (string.IsNullOrWhiteSpace(request.Email))
                return BadRequest(Translate(result));

            return Ok(Translate(result));
        }
    }
}