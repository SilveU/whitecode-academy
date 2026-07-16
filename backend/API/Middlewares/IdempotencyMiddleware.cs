using System.Text;
using Application.Common;
using Application.Interfaces.Services;

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

        public async Task Invoke(HttpContext context, ICacheService _cache)
        {
            var originalBody = context.Response.Body;
            try
            {
                var idempotencyKeyHeader = context.Request.Headers[HeaderNames.IdempotencyKey];
                if(string.IsNullOrEmpty(idempotencyKeyHeader))
                {
                    await _next(context);
                    return;
                }

                var redisKey = CacheKeys.IdempotencyResponseKey(idempotencyKeyHeader!);

                var cached = await _cache.GetAsync<CachedHttpResponse>(redisKey);

                if (cached != null)
                {
                    context.Response.StatusCode = cached.StatusCode;
                    context.Response.ContentType = cached.ContentType;

                    await context.Response.WriteAsync(cached.Body);
                    return;
                }
                var redisLockKey = CacheKeys.IdempotencyLockKey(idempotencyKeyHeader!);
                var acquired = await _cache.SetIfNotExistsAsync(redisLockKey,
                $"processing", TimeSpan.FromSeconds(60));

                if (!acquired)
                {
                    for (int i = 0; i < 10; i++)
                    {
                        await Task.Delay(200);

                        var response = await _cache.GetAsync<CachedHttpResponse>(redisKey);

                        if(response != null)
                        {
                            context.Response.StatusCode = response.StatusCode;
                            context.Response.ContentType = response.ContentType;
                            
                            await context.Response.WriteAsync(response.Body);
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
                    using var reader = new StreamReader(memoryStream, Encoding.UTF8, 
                    detectEncodingFromByteOrderMarks: false, leaveOpen: true);

                    string text = await reader.ReadToEndAsync();

                    if (context.Response.StatusCode == StatusCodes.Status200OK || 
                    context.Response.StatusCode == StatusCodes.Status201Created ||
                    context.Response.StatusCode == StatusCodes.Status400BadRequest ||
                    context.Response.StatusCode == StatusCodes.Status422UnprocessableEntity)
                    {
                        var respone = new CachedHttpResponse
                        {
                            Body = text,
                            ContentType = context.Response.ContentType ?? "application/json",
                            StatusCode = context.Response.StatusCode
                        };
                        await _cache.SetAsync(redisKey, respone, TimeSpan.FromMinutes(15));
                    }
                }
                finally
                {
                    await _cache.RemoveAsync(redisLockKey);
                    
                }

                memoryStream.Position = 0;

                await memoryStream.CopyToAsync(originalBody);
            }
            finally
            {
                context.Response.Body = originalBody;
            }
        }
    }
}