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
    /// The family name to create.
    /// </summary>
    public string FamilyName { get; init; } = string.Empty;

    /// <summary>
    /// The timezone for the family.
    /// </summary>
    public string Timezone { get; init; } = string.Empty;

    /// <summary>
    /// When the session was created.
    /// </summary>
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
}
