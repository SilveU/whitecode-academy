using System.Security.Claims;
using System.Text.Json;
using API.Attributes;
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

        public TokenRevocationMiddleware(RequestDelegate next, ILogger<TokenRevocationMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context, ICacheService cache)
        {
            var endpoint = context.GetEndpoint();

            if (endpoint?.Metadata.GetMetadata<SkipTokenRevocationAttribute>() is null)
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
                    var session = await cache.ExistsAsync(CacheKeys.AuthTokenActive(userId));

                    if (session.Success && !session.Exists)
                    {
                        _logger.LogWarning("Token revocation check failed for user {UserId}. Session not found in Redis.", userId);

                        context.Response.StatusCode  = StatusCodes.Status401Unauthorized;
                        context.Response.ContentType = "application/json";

                        await context.Response.WriteAsync(JsonSerializer.Serialize(new
                        {
                            StatusCode = 401,
                            Message = "Your session has expired or you have been logged out. Please log in again."
                        }));

                        return;
                    }

                    else if (!session.Success)
                    {
                        _logger.LogWarning("Redis is unavailable. Skipping token revocation check for user {UserId}.", userId);
                        await _next(context);
                        return;
                    }
                }
            }

            await _next(context);
        }
    }
}
