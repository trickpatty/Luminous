using System.Security.Claims;
using Luminous.Application.Common.Interfaces;

namespace Luminous.Api.Services;

/// <summary>
/// Provides tenant (family) context from the current HTTP context.
/// Implements multi-tenant data isolation by ensuring users can only access their family's data.
/// </summary>
public class TenantContext : ITenantContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<TenantContext> _logger;

    public TenantContext(IHttpContextAccessor httpContextAccessor, ILogger<TenantContext> logger)
    {
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

    /// <inheritdoc />
    public string? TenantId => User?.FindFirstValue("family_id");

    /// <inheritdoc />
    public bool HasTenant => !string.IsNullOrEmpty(TenantId);

    /// <inheritdoc />
    public bool HasAccessToFamily(string familyId)
    {
        if (string.IsNullOrEmpty(familyId))
        {
            return false;
        }

        // Users can only access their own family
        return string.Equals(TenantId, familyId, StringComparison.OrdinalIgnoreCase);
    }

    /// <inheritdoc />
    public void EnsureAccessToFamily(string familyId)
    {
        if (!HasAccessToFamily(familyId))
        {
            _logger.LogWarning(
                "User with family {UserFamilyId} attempted to access family {RequestedFamilyId}",
                TenantId ?? "null",
                familyId);

            throw new UnauthorizedAccessException(
                $"User does not have access to family '{familyId}'.");
        }
    }
}
