using System.Security.Cryptography;
using Luminous.Domain.Common;

namespace Luminous.Domain.Entities;

/// <summary>
/// Represents a one-time password token for email-based authentication.
/// </summary>
public sealed class OtpToken : Entity
{
    /// <summary>
    /// Gets or sets the email address this OTP is for.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the hashed OTP code.
    /// </summary>
    public string CodeHash { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets when the OTP expires.
    /// </summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// Gets or sets whether the OTP has been used.
    /// </summary>
    public bool IsUsed { get; set; }

    /// <summary>
    /// Gets or sets the number of verification attempts.
    /// </summary>
    public int Attempts { get; set; }

    /// <summary>
    /// Gets or sets the maximum allowed attempts.
    /// </summary>
    public int MaxAttempts { get; set; } = 3;

    /// <summary>
    /// Gets or sets the purpose of this OTP (login, registration, password-reset).
    /// </summary>
    public string Purpose { get; set; } = "login";

    /// <summary>
    /// Gets or sets the user ID if this is for an existing user.
    /// </summary>
    public string? UserId { get; set; }

    /// <summary>
    /// Gets or sets the family ID if this is for an existing user.
    /// </summary>
    public string? FamilyId { get; set; }

    /// <summary>
    /// Gets or sets the IP address that requested the OTP.
    /// </summary>
    public string? IpAddress { get; set; }

    /// <summary>
    /// Gets or sets the user agent that requested the OTP.
    /// </summary>
    public string? UserAgent { get; set; }

    /// <summary>
    /// Checks if the OTP is valid (not expired, not used, attempts remaining).
    /// </summary>
    public bool IsValid => !IsUsed && Attempts < MaxAttempts && DateTime.UtcNow < ExpiresAt;

    /// <summary>
    /// Creates a new OTP token for the specified email.
    /// </summary>
    public static (OtpToken Token, string Code) Create(
        string email,
        TimeSpan? validity = null,
        string purpose = "login",
        string? userId = null,
        string? familyId = null,
        string? ipAddress = null,
        string? userAgent = null)
    {
        var code = GenerateCode();
        var token = new OtpToken
        {
            Email = email.ToLowerInvariant(),
            CodeHash = HashCode(code),
            ExpiresAt = DateTime.UtcNow.Add(validity ?? TimeSpan.FromMinutes(10)),
            Purpose = purpose,
            UserId = userId,
            FamilyId = familyId,
            IpAddress = ipAddress,
            UserAgent = userAgent
        };

        return (token, code);
    }

    /// <summary>
    /// Verifies the provided code against the stored hash.
    /// </summary>
    public bool VerifyCode(string code)
    {
        Attempts++;

        if (!IsValid)
        {
            return false;
        }

        var hash = HashCode(code);
        if (hash == CodeHash)
        {
            IsUsed = true;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Marks the OTP as used.
    /// </summary>
    public void MarkAsUsed()
    {
        IsUsed = true;
        ModifiedAt = DateTime.UtcNow;
    }

    private static string GenerateCode()
    {
        // Generate a 6-digit code
        var bytes = RandomNumberGenerator.GetBytes(4);
        var number = Math.Abs(BitConverter.ToInt32(bytes, 0)) % 1000000;
        return number.ToString("D6");
    }

    private static string HashCode(string code)
    {
        var bytes = System.Text.Encoding.UTF8.GetBytes(code);
        var hash = SHA256.HashData(bytes);
        return Convert.ToBase64String(hash);
    }
}
