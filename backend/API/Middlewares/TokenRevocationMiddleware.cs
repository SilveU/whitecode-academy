using System.Security.Claims;
using System.Text.Json;
using API.Attributes;
using API.Resources;
using Application.Common;
using Application.Interfaces.Services;
using Application.Localization;
using Microsoft.Extensions.Localization;

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

            if (context.User.Identity?.IsAuthenticated == true)
            {
                var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (!string.IsNullOrEmpty(userId))
                {
                    var session = await cache.ExistsAsync(CacheKeys.AuthTokenActive(userId));

                    if (session.Success && !session.Exists)
                    {
                        _logger.LogWarning("Token revocation check failed for user {UserId}. Session not found in Redis.", userId);

                        var localizer = context.RequestServices
                            .GetRequiredService<IStringLocalizer<ExceptionMessages>>();

                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        context.Response.ContentType = "application/json";

                        await context.Response.WriteAsync(JsonSerializer.Serialize(new
                        {
                            StatusCode = 401,
                            Message = localizer[MessageKeys.Common.Auth_SessionExpired].Value
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
