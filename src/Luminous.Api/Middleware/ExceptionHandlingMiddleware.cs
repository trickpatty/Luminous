using System.Net;
using System.Text.Json;
using Luminous.Application.Common.Exceptions;
using Luminous.Shared.Contracts;

namespace Luminous.Api.Middleware;

/// <summary>
/// Middleware for global exception handling.
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, response) = exception switch
        {
            ValidationException validationEx => (
                HttpStatusCode.BadRequest,
                ApiResponse<object>.Fail("VALIDATION_ERROR", "Validation failed.", validationEx.Errors)
            ),
            NotFoundException notFoundEx => (
                HttpStatusCode.NotFound,
                ApiResponse<object>.Fail("NOT_FOUND", notFoundEx.Message)
            ),
            ForbiddenAccessException forbiddenEx => (
                HttpStatusCode.Forbidden,
                ApiResponse<object>.Fail("FORBIDDEN", forbiddenEx.Message)
            ),
            ConflictException conflictEx => (
                HttpStatusCode.Conflict,
                ApiResponse<object>.Fail("CONFLICT", conflictEx.Message)
            ),
            UnauthorizedAccessException => (
                HttpStatusCode.Unauthorized,
                ApiResponse<object>.Fail("UNAUTHORIZED", "Authentication required.")
            ),
            _ => (
                HttpStatusCode.InternalServerError,
                ApiResponse<object>.Fail("INTERNAL_ERROR", "An unexpected error occurred.")
            )
        };

        if (statusCode == HttpStatusCode.InternalServerError)
        {
            _logger.LogError(exception, "Unhandled exception occurred");
        }
        else
        {
            _logger.LogWarning(exception, "Handled exception: {Message}", exception.Message);
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        var json = JsonSerializer.Serialize(response, options);

        await context.Response.WriteAsync(json);
    }
}
