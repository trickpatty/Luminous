using Luminous.Domain.Common;
using Luminous.Domain.Enums;
using Luminous.Domain.ValueObjects;

namespace Luminous.Domain.Entities;

/// <summary>
/// Represents a user (family member) in the system.
/// </summary>
public sealed class User : AggregateRoot
{
    /// <summary>
    /// Gets or sets the family ID this user belongs to (partition key).
    /// </summary>
    public string FamilyId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the external identity provider ID.
    /// </summary>
    public string? ExternalId { get; set; }

    /// <summary>
    /// Gets or sets the user's email address.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the user's display name.
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the user's role within the family.
    /// </summary>
    public UserRole Role { get; set; } = UserRole.Adult;

    /// <summary>
    /// Gets or sets the user's profile information.
    /// </summary>
    public UserProfile Profile { get; set; } = new();

    /// <summary>
    /// Gets or sets caregiver-relevant information about this user.
    /// </summary>
    public CaregiverInfo? CaregiverInfo { get; set; }

    /// <summary>
    /// Gets or sets whether email is verified.
    /// </summary>
    public bool EmailVerified { get; set; }

    /// <summary>
    /// Gets or sets the last login timestamp.
    /// </summary>
    public DateTime? LastLoginAt { get; set; }

    /// <summary>
    /// Gets or sets whether the user account is active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Gets or sets the user's preferred language (ISO 639-1 code).
    /// </summary>
    public string Language { get; set; } = "en";

    /// <summary>
    /// Creates a new user as the family owner.
    /// </summary>
    public static User CreateOwner(string familyId, string email, string displayName, string? externalId = null)
    {
        return Create(familyId, email, displayName, UserRole.Owner, externalId);
    }

    /// <summary>
    /// Creates a new user with the specified role.
    /// </summary>
    public static User Create(string familyId, string email, string displayName, UserRole role, string? externalId = null)
    {
        if (string.IsNullOrWhiteSpace(familyId))
            throw new ArgumentException("Family ID is required.", nameof(familyId));
        if (string.IsNullOrWhiteSpace(displayName))
            throw new ArgumentException("Display name is required.", nameof(displayName));

        return new User
        {
            FamilyId = familyId,
            Email = email?.Trim().ToLowerInvariant() ?? string.Empty,
            DisplayName = displayName.Trim(),
            Role = role,
            ExternalId = externalId
        };
    }

    /// <summary>
    /// Updates the user's profile.
    /// </summary>
    public void UpdateProfile(UserProfile profile, string modifiedBy)
    {
        Profile = profile ?? throw new ArgumentNullException(nameof(profile));
        ModifiedAt = DateTime.UtcNow;
        ModifiedBy = modifiedBy;
    }

    /// <summary>
    /// Records a successful login.
    /// </summary>
    public void RecordLogin()
    {
        LastLoginAt = DateTime.UtcNow;
    }
}
