using System.Text.RegularExpressions;
using Application.Common;

namespace API.Middlewares
{
    public class CorrelationIdMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<CorrelationIdMiddleware> _logger;
        private static readonly Regex CorrelationIdRegex = new(@"^[A-Za-z0-9._:-]{1,128}$", RegexOptions.Compiled);

        public CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            var correlationId = context.Request.Headers[HeaderNames.CorrelationId];
            if(correlationId.Count > 1)
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsync("Only one Correlation-Id header is allowed.");
                return;
            }

            else if (string.IsNullOrWhiteSpace(correlationId) || !CorrelationIdRegex.IsMatch(correlationId!))
            {
                correlationId = Guid.NewGuid().ToString();
            }
            using var scope = _logger.BeginScope(new Dictionary<string, object>
            {
                ["CorrelationId"] = correlationId
            });
            _logger.LogInformation("Correlation scope started");
            context.Items[HeaderNames.CorrelationId] = correlationId;
            context.Response.Headers[HeaderNames.CorrelationId] = correlationId;
            await _next(context);
        }
    }
}