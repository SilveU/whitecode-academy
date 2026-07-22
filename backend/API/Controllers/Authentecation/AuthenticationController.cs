using System.Security.Claims;
using Application.DTOs.Authentication;
using Application.Interfaces.Authentecation;
using Application.Helper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
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

        public AuthenticationController(IAuthenticationService auth, IEmailVerificationService emailVerificationService, IResetPasswordService resetPasswordService)
        {
            this.auth = auth;
            _emailVerificationService = emailVerificationService;
            _resetPasswordService = resetPasswordService;
        }

        [HttpPost("login")]
        [SkipTokenRevocation]
        public async Task<IActionResult> LoginAsync([FromBody] LoginRequest request)
        {
            var ipAddress = await IpAddressHelper.GetRealPublicIpAsync();
            var loginResult = await auth.LoginAsync(request, ipAddress);

            if (!loginResult.IsAuthenticated)
                return Unauthorized(loginResult);

            return Ok(loginResult);
        }

        [HttpPost("register")]
        [SkipTokenRevocation]
        public async Task<IActionResult> RegisterAsync([FromBody] RegisterRequest request)
        {
            var registerResult = await auth.RegisterAsync(request);

            if (registerResult.Id is null)
                return BadRequest(registerResult);

            return Ok(registerResult);
        }

        [HttpGet("confirm-email")]
        [SkipTokenRevocation]
        public async Task<IActionResult> ConfirmEmail([FromQuery] string userId, [FromQuery] string token)
        {
            var result = await _emailVerificationService.ConfirmEmailAsync(userId, token);

            return Ok(result);
        }

        [HttpPost("resend-email-confirmation")]
        [SkipTokenRevocation]
        public async Task<IActionResult> ResendEmailConfirmation([FromQuery] ResendEmailConfirmationRequest request)
        {
            var result = await _emailVerificationService.ResendEmailConfirmationAsync(request.Email);

            if (string.IsNullOrWhiteSpace(request.Email))
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPost("reset-password")]
        [SkipTokenRevocation]
        public async Task<IActionResult> ResetPassword([FromBody] EmailResetPasswordRequest request)
        {
            var result = await _resetPasswordService.ResetPassword(request.Email);

            return Ok(result);
        }

        [HttpGet("confirm-reset-password")]
        [SkipTokenRevocation]
        public async Task<IActionResult> ConfirmResetPassword([FromQuery] string userId, [FromQuery] string token, [FromBody] NewPasswordRequest request)
        {
            var result = await _resetPasswordService.ConfirmResetPasswordAsync(userId, token, request);

            if (!result.IsAuthenticated)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPost("resend-reset-password")]
        [SkipTokenRevocation]
        public async Task<IActionResult> ResendResetPassword([FromQuery] EmailResetPasswordRequest request)
        {
            var result = await _resetPasswordService.ResendResetPasswordAsync(request.Email);

            if (string.IsNullOrWhiteSpace(request.Email))
                return BadRequest(result);

            return Ok(result);
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
                return Unauthorized(result);

            return Ok(result);
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
                return BadRequest(new { Message = "Failed to logout" });

            return Ok(new { Message = "Logged out successfully" });
        }

        [HttpPost("logout-all")]
        public async Task<IActionResult> LogoutAll()
        {
            var currentUserId = GetUserId();
            var ipAddress = await IpAddressHelper.GetRealPublicIpAsync();
            var result = await auth.LogoutAllAsync(currentUserId, ipAddress);
            if (!result)
                return BadRequest(new { Message = "Failed to logout from all sessions" });

            return Ok(new { Message = "Logged out from all sessions successfully" });
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