namespace Luminous.Application.Features.Auth.Models;

/// <summary>
/// Session data stored during registration.
/// Used to temporarily store registration details while awaiting email verification.
/// </summary>
public sealed record RegistrationSessionData
{
    /// <summary>
    /// The normalized email address.
    /// </summary>
    public string Email { get; init; } = string.Empty;

    /// <summary>
    /// The user's display name.
    /// </summary>
    public string DisplayName { get; init; } = string.Empty;

    /// <summary>
    /// The family name to create (only used when creating a new family).
    /// </summary>
    public string FamilyName { get; init; } = string.Empty;

    /// <summary>
    /// The timezone for the family (only used when creating a new family).
    /// </summary>
    public string Timezone { get; init; } = string.Empty;

    /// <summary>
    /// Optional invite code to join an existing family instead of creating a new one.
    /// </summary>
    public string? InviteCode { get; init; }

    /// <summary>
    /// When the session was created.
    /// </summary>
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
}
