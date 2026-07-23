using System.Security.Claims;
using API.Localization;
using Application.DTOs.Authentication;
using Application.Interfaces.Authentecation;
using Application.Helper;
using Application.Resources;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Localization;
using API.Attributes;

namespace API.Controllers.Authentication
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableRateLimiting("AuthPolicy")]
    public class AuthenticationController : ControllerBase
    {
        private readonly IAuthenticationService auth;
        private readonly IEmailVerificationService _emailVerificationService;
        private readonly IResetPasswordService _resetPasswordService;

        public AuthenticationController(
            IAuthenticationService auth,
            IEmailVerificationService emailVerificationService,
            IResetPasswordService resetPasswordService)
        {
            this.auth = auth;
            _emailVerificationService = emailVerificationService;
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

        [HttpPost("login")]
        [SkipTokenRevocation]
        public async Task<IActionResult> LoginAsync([FromBody] LoginRequest request)
        {
            var ipAddress = await IpAddressHelper.GetRealPublicIpAsync();
            var loginResult = await auth.LoginAsync(request, ipAddress);

            if (!loginResult.IsAuthenticated)
                return Unauthorized(Translate(loginResult));

            return Ok(Translate(loginResult));
        }

        [HttpPost("register")]
        [SkipTokenRevocation]
        public async Task<IActionResult> RegisterAsync([FromBody] RegisterRequest request)
        {
            var registerResult = await auth.RegisterAsync(request);

            if (registerResult.Id is null)
                return BadRequest(Translate(registerResult));

            return Ok(Translate(registerResult));
        }

        [HttpGet("confirm-email")]
        [SkipTokenRevocation]
        public async Task<IActionResult> ConfirmEmail([FromQuery] string userId, [FromQuery] string token)
        {
            var result = await _emailVerificationService.ConfirmEmailAsync(userId, token);

            return Ok(Translate(result));
        }

        [HttpPost("resend-email-confirmation")]
        [SkipTokenRevocation]
        public async Task<IActionResult> ResendEmailConfirmation([FromQuery] ResendEmailConfirmationRequest request)
        {
            var result = await _emailVerificationService.ResendEmailConfirmationAsync(request.Email);

            if (string.IsNullOrWhiteSpace(request.Email))
                return BadRequest(Translate(result));

            return Ok(Translate(result));
        }

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
        [SkipTokenRevocation]
        public async Task<IActionResult> ResendResetPassword([FromQuery] EmailResetPasswordRequest request)
        {
            var result = await _resetPasswordService.ResendResetPasswordAsync(request.Email);

            if (string.IsNullOrWhiteSpace(request.Email))
                return BadRequest(Translate(result));

            return Ok(Translate(result));
        }

        [HttpPost("refresh")]
        [SkipTokenRevocation]
        public async Task<IActionResult> RefreshToken()
        {
            var refreshToken = Request.Cookies["RefreshToken"];

            if (string.IsNullOrWhiteSpace(refreshToken))
                return Unauthorized();

            var ipAddress = await IpAddressHelper.GetRealPublicIpAsync();
            var result = await auth.RefreshAsync(refreshToken, ipAddress);

            if (!result.IsAuthenticated)
                return Unauthorized(Translate(result));

            return Ok(Translate(result));
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var refreshToken = Request.Cookies["RefreshToken"];

            if (string.IsNullOrWhiteSpace(refreshToken))
                return Unauthorized();

            var ipAddress = await IpAddressHelper.GetRealPublicIpAsync();
            var result = await auth.LogoutAsync(refreshToken, ipAddress);

            if (!result)
                return BadRequest(new { Message = Resolve(MessageKeys.Common.Auth_InvalidRefreshToken) });

            return Ok(new { Message = Resolve(MessageKeys.Common.Auth_LoggedOut) });
        }

        [HttpPost("logout-all")]
        public async Task<IActionResult> LogoutAll()
        {
            var currentUserId = GetUserId();
            var ipAddress = await IpAddressHelper.GetRealPublicIpAsync();
            var result = await auth.LogoutAllAsync(currentUserId, ipAddress);

            if (!result)
                return BadRequest(new { Message = Resolve(MessageKeys.Common.Auth_InvalidRefreshToken) });

            return Ok(new { Message = Resolve(MessageKeys.Common.Auth_LoggedOutAll) });
        }

        private string GetUserId()
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            if (string.IsNullOrWhiteSpace(userId))
                throw new UnauthorizedAccessException("User id claim not found.");

            return userId;
        }
    }
}
