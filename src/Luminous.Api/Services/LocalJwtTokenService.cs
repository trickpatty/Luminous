using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Luminous.Api.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Luminous.Api.Services;

/// <summary>
/// Service for generating JWT tokens for local development purposes.
/// This should NOT be used in production - tokens should come from the identity provider.
/// </summary>
public interface ILocalJwtTokenService
{
    /// <summary>
    /// Generates a JWT token for a development user.
    /// </summary>
    /// <param name="userId">The user ID to include in the token.</param>
    /// <param name="familyId">The family ID to include in the token.</param>
    /// <param name="email">The email to include in the token.</param>
    /// <param name="role">The role to include in the token.</param>
    /// <param name="displayName">The display name to include in the token.</param>
    /// <returns>A JWT access token.</returns>
    string GenerateToken(string userId, string familyId, string email, string role, string displayName);

    /// <summary>
    /// Generates a development user token with default test values.
    /// </summary>
    /// <returns>A JWT access token for a test user.</returns>
    string GenerateDevToken();
}

/// <summary>
/// Implementation of local JWT token generation for development.
/// </summary>
public class LocalJwtTokenService : ILocalJwtTokenService
{
    private readonly JwtSettings _settings;
    private readonly ILogger<LocalJwtTokenService> _logger;

    public LocalJwtTokenService(IOptions<JwtSettings> settings, ILogger<LocalJwtTokenService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public string GenerateToken(string userId, string familyId, string email, string role, string displayName)
    {
        if (!_settings.EnableLocalTokenGeneration)
        {
            throw new InvalidOperationException("Local JWT token generation is disabled. This feature is only available in development.");
        }

        _logger.LogWarning(
            "Generating local development JWT token for user {UserId} in family {FamilyId}. " +
            "This should only happen in development environments.",
            userId, familyId);

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId),
            new(JwtRegisteredClaimNames.Email, email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
            new(ClaimTypes.NameIdentifier, userId),
            new(ClaimTypes.Email, email),
            new(ClaimTypes.Role, role),
            new(ClaimTypes.Name, displayName),
            new("family_id", familyId),
            new("display_name", displayName),
            new("auth_method", "local_dev")
        };

        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: DateTime.UtcNow.AddMinutes(_settings.ExpirationMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateDevToken()
    {
        // Default development user credentials
        return GenerateToken(
            userId: "dev-user-001",
            familyId: "dev-family-001",
            email: "dev@luminous.local",
            role: "Owner",
            displayName: "Developer");
    }
}
