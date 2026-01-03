using Luminous.Domain.Common;

namespace Luminous.Domain.ValueObjects;

/// <summary>
/// Stores OAuth tokens for external calendar integration.
/// Note: Tokens should be encrypted at rest in production.
/// </summary>
public sealed class OAuthTokens : ValueObject
{
    /// <summary>
    /// Gets or sets the access token for API calls.
    /// </summary>
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the refresh token for obtaining new access tokens.
    /// </summary>
    public string? RefreshToken { get; set; }

    /// <summary>
    /// Gets or sets the token type (usually "Bearer").
    /// </summary>
    public string TokenType { get; set; } = "Bearer";

    /// <summary>
    /// Gets or sets the access token expiry time.
    /// </summary>
    public DateTime? ExpiresAt { get; set; }

    /// <summary>
    /// Gets or sets the scope of the access token.
    /// </summary>
    public string? Scope { get; set; }

    /// <summary>
    /// Gets whether the access token has expired.
    /// </summary>
    public bool IsExpired => ExpiresAt.HasValue && ExpiresAt.Value <= DateTime.UtcNow;

    /// <summary>
    /// Gets whether the tokens can be refreshed.
    /// </summary>
    public bool CanRefresh => !string.IsNullOrEmpty(RefreshToken);

    /// <summary>
    /// Creates new OAuth tokens from an OAuth response.
    /// </summary>
    public static OAuthTokens Create(
        string accessToken,
        string? refreshToken,
        int? expiresInSeconds,
        string? scope = null,
        string tokenType = "Bearer")
    {
        return new OAuthTokens
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            TokenType = tokenType,
            ExpiresAt = expiresInSeconds.HasValue
                ? DateTime.UtcNow.AddSeconds(expiresInSeconds.Value)
                : null,
            Scope = scope
        };
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return AccessToken;
        yield return RefreshToken;
        yield return TokenType;
        yield return ExpiresAt;
        yield return Scope;
    }
}
