using System.Text.Json;
using System.Text.RegularExpressions;
using Luminous.Application.Common.Interfaces;
using Luminous.Shared.Contracts;

namespace Luminous.Api.Middleware;

/// <summary>
/// Middleware that validates tenant access for requests that include a family ID in the route.
/// Ensures users can only access data belonging to their own family (tenant).
/// </summary>
public partial class TenantValidationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TenantValidationMiddleware> _logger;

    // Regex pattern to match family-scoped routes
    // Note: devices routes use /api/devices/family/{familyId} pattern, not /api/devices/{familyId}
    [GeneratedRegex(@"/api/(?:families|users/family|devices/family|events/family|chores/family|calendar-connections/family)/([a-zA-Z0-9_-]+)")]
    private static partial Regex FamilyScopedRouteRegex();

    public TenantValidationMiddleware(RequestDelegate next, ILogger<TenantValidationMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, ITenantContext tenantContext)
    {
        var path = context.Request.Path.Value;

        // Skip tenant validation for unauthenticated requests or non-family routes
        if (!context.User.Identity?.IsAuthenticated ?? true)
        {
            await _next(context);
            return;
        }

        // Check if the route contains a family ID
        if (path != null)
        {
            var match = FamilyScopedRouteRegex().Match(path);
            if (match.Success)
            {
                var requestedFamilyId = match.Groups[1].Value;

                // Skip validation for POST /api/families (creating new family)
                if (context.Request.Method == "POST" &&
                    path.Equals("/api/families", StringComparison.OrdinalIgnoreCase))
                {
                    await _next(context);
                    return;
                }

                // Validate that the user has access to the requested family
                if (!tenantContext.HasAccessToFamily(requestedFamilyId))
                {
                    _logger.LogWarning(
                        "Tenant access denied: User with family {UserFamilyId} attempted to access family {RequestedFamilyId} at {Path}",
                        tenantContext.TenantId ?? "null",
                        requestedFamilyId,
                        path);

                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    context.Response.ContentType = "application/json";

                    var response = ApiResponse<object>.Fail(
                        "TENANT_ACCESS_DENIED",
                        "You do not have access to this family's data.");

                    var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    });

                    await context.Response.WriteAsync(json);
                    return;
                }
            }
        }

        await _next(context);
    }
}

/// <summary>
/// Extension methods for TenantValidationMiddleware.
/// </summary>
public static class TenantValidationMiddlewareExtensions
{
    /// <summary>
    /// Adds the tenant validation middleware to the pipeline.
    /// </summary>
    public static IApplicationBuilder UseTenantValidation(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<TenantValidationMiddleware>();
    }
}
