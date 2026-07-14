using System.Security.Claims;
using Application.DTOs.Authentication;
using Application.Interfaces.Authentecation;
using Infrastructure.Helper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace API.Controllers.Authentication
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableRateLimiting("AuthPolicy")]
    public class AuthenticationController : ControllerBase
    {
        private readonly IAuthenticationService auth;
        private readonly IEmailVerificationService _emailVerificationService;

        public AuthenticationController(IAuthenticationService auth, IEmailVerificationService emailVerificationService)
        {
            this.auth = auth;
            _emailVerificationService = emailVerificationService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> LoginAsync([FromBody] LoginRequest request)
        {
            var ipAddress = await IpAddressHelper.GetRealPublicIpAsync();
            var loginResult = await auth.LoginAsync(request, ipAddress);

            if (!loginResult.IsAuthenticated)
                return Unauthorized(loginResult);

            return Ok(loginResult);
        }

        [HttpPost("register")]
        public async Task<IActionResult> RegisterAsync([FromBody] RegisterRequest request)
        {
            var registerResult = await auth.RegisterAsync(request);

            if (registerResult.Id is null)
                return BadRequest(registerResult);

            return Ok(registerResult);
        }

        [HttpGet("confirm-email")]
        public async Task<IActionResult> ConfirmEmail([FromQuery] string userId, [FromQuery] string token)
        {
            var result = await _emailVerificationService.ConfirmEmailAsync(userId, token);

            if (!result.IsAuthenticated)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPost("resend-email-confirmation")]
        public async Task<IActionResult> ResendEmailConfirmation([FromQuery] ResendEmailConfirmationRequest request)
        {
            var result = await _emailVerificationService.ResendEmailConfirmationAsync(request.Email);

            if (string.IsNullOrWhiteSpace(request.Email))
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPost("refresh")]
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