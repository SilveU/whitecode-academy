using System.Security.Claims;
using System.Text.Json;
using Application.Common;
using Application.Interfaces.Services;

namespace API.Middlewares
{
    /// <summary>
    /// Runs after UseAuthentication. If the JWT is valid but the user has no active
    /// session in Redis (logged out or revoked), the request is rejected immediately
    /// without touching the database — this gives us instant token revocation.
    /// </summary>
    public class TokenRevocationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<TokenRevocationMiddleware> _logger;

        // Endpoints that must be reachable without an active session key
        private static readonly HashSet<string> _anonymousPaths = new(StringComparer.OrdinalIgnoreCase)
        {
            "/api/authentication/login",
            "/api/authentication/register",
            "/api/authentication/refresh",
            "/api/authentication/confirm-email",
            "/api/authentication/resend-confirmation",
        };

        public TokenRevocationMiddleware(RequestDelegate next, ILogger<TokenRevocationMiddleware> logger)
        {
            _next   = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context, ICacheService cache)
        {
            // Skip check for anonymous / auth endpoints
            var path = context.Request.Path.Value ?? string.Empty;
            if (_anonymousPaths.Contains(path))
            {
                await _next(context);
                return;
            }

            // Only check authenticated requests (JWT already validated by UseAuthentication)
            if (context.User.Identity?.IsAuthenticated == true)
            {
                var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (!string.IsNullOrEmpty(userId))
                {
                    var isActive = await cache.GetAsync<bool?>(CacheKeys.AuthTokenActive(userId));

                    if (isActive is not true)
                    {
                        _logger.LogWarning("Token revocation check failed for user {UserId}. Session not found in Redis.", userId);

                        context.Response.StatusCode  = StatusCodes.Status401Unauthorized;
                        context.Response.ContentType = "application/json";

                        await context.Response.WriteAsync(JsonSerializer.Serialize(new
                        {
                            StatusCode = 401,
                            Message    = "Your session has expired or you have been logged out. Please log in again."
                        }));

                        return;
                    }
                }
            }

            await _next(context);
        }
    }
}
