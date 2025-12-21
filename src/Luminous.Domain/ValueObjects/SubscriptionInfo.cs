using Luminous.Domain.Common;

namespace Luminous.Domain.ValueObjects;

/// <summary>
/// Information about a family's subscription status.
/// </summary>
public sealed class SubscriptionInfo : ValueObject
{
    /// <summary>
    /// Gets or sets the subscription tier (free, premium, etc.).
    /// </summary>
    public string Tier { get; set; } = "free";

    /// <summary>
    /// Gets or sets when the subscription started.
    /// </summary>
    public DateTime? StartedAt { get; set; }

    /// <summary>
    /// Gets or sets when the subscription expires.
    /// </summary>
    public DateTime? ExpiresAt { get; set; }

    /// <summary>
    /// Gets or sets the external subscription ID from payment provider.
    /// </summary>
    public string? ExternalSubscriptionId { get; set; }

    /// <summary>
    /// Gets whether the subscription is currently active.
    /// </summary>
    public bool IsActive => ExpiresAt == null || ExpiresAt > DateTime.UtcNow;

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Tier;
        yield return StartedAt;
        yield return ExpiresAt;
        yield return ExternalSubscriptionId;
    }
}
