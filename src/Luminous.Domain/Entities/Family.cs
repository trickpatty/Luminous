using Luminous.Domain.Common;
using Luminous.Domain.ValueObjects;

namespace Luminous.Domain.Entities;

/// <summary>
/// Represents a family household (tenant root aggregate).
/// </summary>
public sealed class Family : AggregateRoot
{
    /// <summary>
    /// Gets or sets the family name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the family's timezone (IANA timezone ID).
    /// </summary>
    public string Timezone { get; set; } = "UTC";

    /// <summary>
    /// Gets or sets the family settings.
    /// </summary>
    public FamilySettings Settings { get; set; } = new();

    /// <summary>
    /// Gets or sets the subscription information.
    /// </summary>
    public SubscriptionInfo? Subscription { get; set; }

    /// <summary>
    /// Gets or sets whether the family is active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Creates a new family with the given name.
    /// </summary>
    public static Family Create(string name, string timezone, string createdBy)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Family name is required.", nameof(name));

        return new Family
        {
            Name = name.Trim(),
            Timezone = timezone,
            CreatedBy = createdBy,
            Subscription = new SubscriptionInfo { Tier = "free", StartedAt = DateTime.UtcNow }
        };
    }

    /// <summary>
    /// Updates the family settings.
    /// </summary>
    public void UpdateSettings(FamilySettings settings, string modifiedBy)
    {
        Settings = settings ?? throw new ArgumentNullException(nameof(settings));
        ModifiedAt = DateTime.UtcNow;
        ModifiedBy = modifiedBy;
    }
}
