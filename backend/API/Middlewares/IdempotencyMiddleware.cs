using System.Security.Claims;
using System.Text;
using API.Attributes;
using Application.Common;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entites.System;

namespace API.Middlewares
{
    public class IdempotencyMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<IdempotencyMiddleware> _logger;

        public IdempotencyMiddleware(RequestDelegate next, ILogger<IdempotencyMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context, ICacheService cache, IIdempotencyRepository _idempo)
        {
            var endpoint = context.GetEndpoint();

            if (endpoint?.Metadata.GetMetadata<IdempotentAttribute>() is null)
            {
                await _next(context);
                return;
            }


            var originalBody = context.Response.Body;

            try
            {
                var idempotencyKey = context.Request.Headers[HeaderNames.IdempotencyKey];

                if (string.IsNullOrEmpty(idempotencyKey))
                {
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    await context.Response.WriteAsync("Idempotency-Key header is required.");
                    return;
                }

                var redisKey = CacheKeys.IdempotencyResponseKey(idempotencyKey!);

                var cached = await cache.GetAsync<CachedHttpResponse>(redisKey);

                // Redis Available
                if (cached.Success)
                {
                    if (cached.Item2 != null)
                    {
                        context.Response.StatusCode = cached.Item2.StatusCode;
                        context.Response.ContentType = cached.Item2.ContentType;

                        await context.Response.WriteAsync(cached.Item2.Body);
                        return;
                    }

                    var redisLockKey = CacheKeys.IdempotencyLockKey(idempotencyKey!);

                    var acquired = await cache.SetIfNotExistsAsync(redisLockKey, "processing", TimeSpan.FromSeconds(60));

                    if (!acquired)
                    {
                        for (int i = 0; i < 10; i++)
                        {
                            await Task.Delay(200);

                            var response = await cache.GetAsync<CachedHttpResponse>(redisKey);

                            if (response.Item2 != null)
                            {
                                context.Response.StatusCode = response.Item2.StatusCode;
                                context.Response.ContentType = response.Item2.ContentType;

                                await context.Response.WriteAsync(response.Item2.Body);
                                return;
                            }
                        }

                        context.Response.StatusCode = StatusCodes.Status409Conflict;
                        context.Response.ContentType = "application/json";

                        await context.Response.WriteAsync("Request is still processing");
                        return;
                    }

                    using var memoryStream = new MemoryStream();
                    context.Response.Body = memoryStream;

                    try
                    {
                        await _next(context);

                        memoryStream.Position = 0;

                        using var reader = new StreamReader(memoryStream, Encoding.UTF8, detectEncodingFromByteOrderMarks: false, leaveOpen: true);

                        var text = await reader.ReadToEndAsync();

                        if (context.Response.StatusCode is StatusCodes.Status200OK
                            or StatusCodes.Status201Created
                            or StatusCodes.Status400BadRequest
                            or StatusCodes.Status422UnprocessableEntity)
                        {
                            var response = new CachedHttpResponse
                            {
                                Body = text,
                                ContentType = context.Response.ContentType ?? "application/json",
                                StatusCode = context.Response.StatusCode
                            };

                            await cache.SetAsync(redisKey, response, TimeSpan.FromMinutes(15));
                        }
                    }
                    finally
                    {
                        await cache.RemoveAsync(redisLockKey);
                    }

                    memoryStream.Position = 0;
                    await memoryStream.CopyToAsync(originalBody);
                }

                // Redis Unavailable
                else
                {
                    _logger.LogWarning("Redis is unavailable.");

                    var served = await TryServeFromDatabaseAsync(context, _idempo);

                    if (served)
                        return;

                    using var memoryStream = new MemoryStream();
                    context.Response.Body = memoryStream;

                    await _next(context);

                    memoryStream.Position = 0;

                    using var reader = new StreamReader(
                        memoryStream,
                        Encoding.UTF8,
                        detectEncodingFromByteOrderMarks: false,
                        leaveOpen: true);

                    var text = await reader.ReadToEndAsync();

                    if (context.Response.StatusCode is StatusCodes.Status200OK
                        or StatusCodes.Status201Created
                        or StatusCodes.Status400BadRequest
                        or StatusCodes.Status422UnprocessableEntity)
                    {
                        await SaveToDatabaseAsync(context, text, _idempo);
                    }

                    memoryStream.Position = 0;
                    await memoryStream.CopyToAsync(originalBody);
                }
            }
            finally
            {
                context.Response.Body = originalBody;
            }
        }  

        private async Task<bool> TryServeFromDatabaseAsync(HttpContext context, IIdempotencyRepository _idempo)
        {
            var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);

            var httpMethod = context.Request.Method;
            var path = context.Request.Path.Value ?? string.Empty;

            var idempotencyKey = context.Request.Headers[HeaderNames.IdempotencyKey];
            if(string.IsNullOrEmpty(idempotencyKey))
                return false;

            var idempotency = await _idempo.GetAsync(userId, httpMethod, path, idempotencyKey!);
            if(idempotency == null)
                return false; 

            context.Response.StatusCode = idempotency.StatusCode;
            context.Response.ContentType = idempotency.ContentType;

            await context.Response.WriteAsync(idempotency.ResponseBody);
            return true;
        }
        
        private async Task SaveToDatabaseAsync(HttpContext context, string responseBody, IIdempotencyRepository _idempo)
        {
            var now = DateTimeOffset.UtcNow;
            var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);

            var idempotencyKey = context.Request.Headers[HeaderNames.IdempotencyKey];
            if (string.IsNullOrEmpty(idempotencyKey))
                return ;

            var entity = new Idempotency
            {
                UserId = userId,
                HttpMethod = context.Request.Method,
                Path = context.Request.Path.Value ?? string.Empty,
                IdempotencyKey = idempotencyKey!,
                StatusCode = context.Response.StatusCode,
                ContentType = context.Response.ContentType ?? "application/json",
                ResponseBody = responseBody,
                CreatedAt = now,
                ExpiresAt = now.AddMinutes(15)
            };

            await _idempo.CreateAsync(entity);

            await _idempo.SaveChangesAsync();
        }
    }
}