namespace Luminous.Application.Common.Interfaces;

/// <summary>
/// Provides tenant (family) context for the current request.
/// Used to enforce data isolation in multi-tenant scenarios.
/// </summary>
public interface ITenantContext
{
    /// <summary>
    /// Gets the current tenant (family) ID.
    /// </summary>
    string? TenantId { get; }

    /// <summary>
    /// Gets whether a tenant context is available.
    /// </summary>
    bool HasTenant { get; }

    /// <summary>
    /// Validates that the current user has access to the specified family.
    /// </summary>
    /// <param name="familyId">The family ID to validate access for.</param>
    /// <returns>True if the user has access, false otherwise.</returns>
    bool HasAccessToFamily(string familyId);

    /// <summary>
    /// Ensures the current user has access to the specified family.
    /// </summary>
    /// <param name="familyId">The family ID to validate access for.</param>
    /// <exception cref="UnauthorizedAccessException">Thrown if the user does not have access.</exception>
    void EnsureAccessToFamily(string familyId);
}
