using Luminous.Domain.Common;
using Luminous.Domain.Enums;
using Luminous.Domain.ValueObjects;

namespace Luminous.Domain.Entities;

/// <summary>
/// Temporary session that stores OAuth tokens between authorization completion
/// and calendar connection creation. Expires after 15 minutes.
/// </summary>
public class OAuthSession : Entity
{
    private OAuthSession() { } // EF/serialization

    /// <summary>
    /// The family this OAuth session belongs to.
    /// </summary>
    public string FamilyId { get; private set; } = null!;

    /// <summary>
    /// The calendar provider (Google, Microsoft, etc.)
    /// </summary>
    public CalendarProvider Provider { get; private set; }

    /// <summary>
    /// Unique state parameter used during OAuth flow for CSRF protection.
    /// </summary>
    public string State { get; private set; } = null!;

    /// <summary>
    /// The redirect URI used in the OAuth flow.
    /// </summary>
    public string RedirectUri { get; private set; } = null!;

    /// <summary>
    /// The OAuth tokens obtained after authorization.
    /// </summary>
    public OAuthTokens? Tokens { get; private set; }

    /// <summary>
    /// The email/account identifier from the OAuth provider.
    /// </summary>
    public string? AccountEmail { get; private set; }

    /// <summary>
    /// When this session expires. Sessions are valid for 15 minutes after creation.
    /// </summary>
    public DateTime ExpiresAt { get; private set; }

    /// <summary>
    /// Whether the OAuth flow has been completed (tokens obtained).
    /// </summary>
    public bool IsCompleted { get; private set; }

    /// <summary>
    /// Creates a new OAuth session for initiating an OAuth flow.
    /// </summary>
    public static OAuthSession Create(
        string familyId,
        CalendarProvider provider,
        string redirectUri,
        string createdBy)
    {
        return new OAuthSession
        {
            Id = Guid.NewGuid().ToString(),
            FamilyId = familyId,
            Provider = provider,
            State = GenerateState(),
            RedirectUri = redirectUri,
            ExpiresAt = DateTime.UtcNow.AddMinutes(15),
            IsCompleted = false,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Completes the OAuth flow by storing the tokens.
    /// </summary>
    public void Complete(OAuthTokens tokens, string accountEmail)
    {
        if (IsExpired)
            throw new InvalidOperationException("OAuth session has expired");

        Tokens = tokens ?? throw new ArgumentNullException(nameof(tokens));
        AccountEmail = accountEmail ?? throw new ArgumentNullException(nameof(accountEmail));
        IsCompleted = true;
        // Extend expiration after completion to give user time to select calendars
        ExpiresAt = DateTime.UtcNow.AddMinutes(15);
    }

    /// <summary>
    /// Gets whether this session has expired.
    /// </summary>
    public bool IsExpired => DateTime.UtcNow > ExpiresAt;

    /// <summary>
    /// Gets whether this session is valid for creating connections.
    /// </summary>
    public bool IsValidForConnectionCreation => IsCompleted && !IsExpired && Tokens != null;

    /// <summary>
    /// Generates a cryptographically secure state parameter.
    /// </summary>
    private static string GenerateState()
    {
        var bytes = new byte[32];
        using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .TrimEnd('=');
    }
}
