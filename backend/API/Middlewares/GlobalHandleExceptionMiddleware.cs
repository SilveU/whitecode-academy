using System.Text.Json;
using Application.Localization;
using API.Resources;
using Domain.Exceptions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace API.Middlewares;

public class GlobalHandleExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalHandleExceptionMiddleware> _logger;

    public GlobalHandleExceptionMiddleware(RequestDelegate next, ILogger<GlobalHandleExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            LogException(ex);

            if (context.Response.HasStarted)
            {
                _logger.LogWarning(
                    "The response has already started, the exception middleware will not modify the response.");
                throw;
            }

            // Resolve IStringLocalizer<ExceptionMessages> per-request from Scoped DI
            // (Middleware is Singleton — NEVER inject localizer in constructor)
            var localizer = context.RequestServices
                .GetRequiredService<IStringLocalizer<ExceptionMessages>>();

            var (statusCode, message) = ex switch
            {
                NotFoundException =>
                    (StatusCodes.Status404NotFound,
                        localizer[MessageKeys.Exception.NotFound].Value),

                UnauthorizedAccessException =>
                    (StatusCodes.Status403Forbidden,
                        localizer[MessageKeys.Exception.Unauthorized].Value),

                ArgumentException =>
                    (StatusCodes.Status400BadRequest,
                        localizer[MessageKeys.Exception.InvalidInput].Value),

                BusinessRuleException =>
                    (StatusCodes.Status409Conflict,
                        localizer[MessageKeys.Exception.InvalidOperation].Value),

                InvalidOperationException =>
                    (StatusCodes.Status409Conflict,
                        localizer[MessageKeys.Exception.InvalidOperation].Value),

                DbUpdateConcurrencyException =>
                    (StatusCodes.Status409Conflict,
                        localizer[MessageKeys.Exception.Concurrency].Value),

                DbUpdateException =>
                    (StatusCodes.Status500InternalServerError,
                        localizer[MessageKeys.Exception.DatabaseUpdate].Value),

                SqlException =>
                    (StatusCodes.Status500InternalServerError,
                        localizer[MessageKeys.Exception.Database].Value),

                _ =>
                    (StatusCodes.Status500InternalServerError,
                        localizer[MessageKeys.Exception.Unexpected].Value)
            };

            context.Response.Clear();
            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/json";

            await context.Response.WriteAsync(JsonSerializer.Serialize(new
            {
                StatusCode = statusCode,
                Message = message
            }));
        }
    }

    private void LogException(Exception ex)
    {
        switch (ex)
        {
            case NotFoundException notFoundEx:
                _logger.LogWarning(
                    new EventId(1000),
                    notFoundEx,
                    "{Message}",
                    notFoundEx.Message);
                break;

            case UnauthorizedAccessException unauthorizedEx:
                _logger.LogWarning(
                    new EventId(1001),
                    unauthorizedEx,
                    "{Message}",
                    unauthorizedEx.Message);
                break;

            case ArgumentException argumentEx:
                _logger.LogWarning(
                    new EventId(1002),
                    argumentEx,
                    "{Message}",
                    argumentEx.Message);
                break;

            case BusinessRuleException businessEx:
                _logger.LogWarning(
                    new EventId(1003),
                    businessEx,
                    "{Message}",
                    businessEx.Message);
                break;

            case InvalidOperationException invalidOpEx:
                _logger.LogError(
                    new EventId(1004),
                    invalidOpEx,
                    "Invalid operation.");
                break;

            case DbUpdateConcurrencyException concurrencyEx:
                _logger.LogError(
                    new EventId(1005),
                    concurrencyEx,
                    "A database concurrency conflict occurred.");
                break;

            case SqlException sqlEx:
                _logger.LogError(
                    new EventId(1006),
                    sqlEx,
                    "SQL exception occurred. ErrorNumber: {ErrorNumber}",
                    sqlEx.Number);
                break;

            case DbUpdateException dbUpdateEx:
                _logger.LogError(
                    new EventId(1007),
                    dbUpdateEx,
                    "Database update exception.");
                break;

            case NullReferenceException nullReferenceEx:
                _logger.LogCritical(
                    new EventId(1008),
                    nullReferenceEx,
                    "Null reference exception.");
                break;

            default:
                _logger.LogError(
                    new EventId(1009),
                    ex,
                    "Unhandled exception.");
                break;
        }
    }
}
