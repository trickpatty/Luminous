using System.Security.Cryptography;
using Luminous.Domain.Common;

namespace Luminous.Domain.Entities;

/// <summary>
/// Represents a refresh token for JWT token renewal.
/// </summary>
public sealed class RefreshToken : Entity
{
    /// <summary>
    /// Gets or sets the user ID this token belongs to.
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the family ID for multi-tenancy.
    /// </summary>
    public string FamilyId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the hashed token value.
    /// </summary>
    public string TokenHash { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets when the token expires.
    /// </summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// Gets or sets whether the token has been revoked.
    /// </summary>
    public bool IsRevoked { get; set; }

    /// <summary>
    /// Gets or sets when the token was revoked.
    /// </summary>
    public DateTime? RevokedAt { get; set; }

    /// <summary>
    /// Gets or sets the reason for revocation.
    /// </summary>
    public string? RevokedReason { get; set; }

    /// <summary>
    /// Gets or sets the token that replaced this one (for rotation).
    /// </summary>
    public string? ReplacedByTokenId { get; set; }

    /// <summary>
    /// Gets or sets the IP address that created this token.
    /// </summary>
    public string? IpAddress { get; set; }

    /// <summary>
    /// Gets or sets the user agent that created this token.
    /// </summary>
    public string? UserAgent { get; set; }

    /// <summary>
    /// Gets or sets the device type (web, mobile, device).
    /// </summary>
    public string DeviceType { get; set; } = "web";

    /// <summary>
    /// Gets or sets the device ID if this is a device token.
    /// </summary>
    public string? DeviceId { get; set; }

    /// <summary>
    /// Checks if the token is active (not expired and not revoked).
    /// </summary>
    public bool IsActive => !IsRevoked && DateTime.UtcNow < ExpiresAt;

    /// <summary>
    /// Creates a new refresh token for a user.
    /// </summary>
    public static (RefreshToken Token, string RawToken) Create(
        string userId,
        string familyId,
        TimeSpan? validity = null,
        string? ipAddress = null,
        string? userAgent = null,
        string deviceType = "web",
        string? deviceId = null)
    {
        var rawToken = GenerateToken();
        var token = new RefreshToken
        {
            UserId = userId,
            FamilyId = familyId,
            TokenHash = HashToken(rawToken),
            ExpiresAt = DateTime.UtcNow.Add(validity ?? TimeSpan.FromDays(7)),
            IpAddress = ipAddress,
            UserAgent = userAgent,
            DeviceType = deviceType,
            DeviceId = deviceId,
            CreatedBy = userId
        };

        return (token, rawToken);
    }

    /// <summary>
    /// Verifies the provided raw token against the stored hash.
    /// </summary>
    public bool VerifyToken(string rawToken)
    {
        if (!IsActive)
        {
            return false;
        }

        return HashToken(rawToken) == TokenHash;
    }

    /// <summary>
    /// Revokes the token.
    /// </summary>
    public void Revoke(string? reason = null, string? replacedByTokenId = null)
    {
        IsRevoked = true;
        RevokedAt = DateTime.UtcNow;
        RevokedReason = reason;
        ReplacedByTokenId = replacedByTokenId;
        ModifiedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Rotates the token by revoking this one and creating a new one.
    /// </summary>
    public (RefreshToken NewToken, string RawToken) Rotate(
        string? ipAddress = null,
        string? userAgent = null)
    {
        var (newToken, rawToken) = Create(
            UserId,
            FamilyId,
            ExpiresAt - CreatedAt, // Same validity period
            ipAddress ?? IpAddress,
            userAgent ?? UserAgent,
            DeviceType,
            DeviceId);

        Revoke("rotated", newToken.Id);

        return (newToken, rawToken);
    }

    private static string GenerateToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(bytes);
    }

    private static string HashToken(string token)
    {
        var bytes = System.Text.Encoding.UTF8.GetBytes(token);
        var hash = SHA256.HashData(bytes);
        return Convert.ToBase64String(hash);
    }
}
