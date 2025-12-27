using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Luminous.Api.Configuration;
using Luminous.Application.Common.Interfaces;
using Luminous.Application.DTOs;
using Luminous.Domain.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Luminous.Api.Services;

/// <summary>
/// Service for generating and validating JWT authentication tokens.
/// Implements multi-tenant token generation with family context.
/// </summary>
public class TokenService : ITokenService
{
    private readonly JwtSettings _settings;
    private readonly ILogger<TokenService> _logger;

    public TokenService(IOptions<JwtSettings> settings, ILogger<TokenService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    /// <inheritdoc />
    public AuthResultDto GenerateToken(User user)
    {
        return GenerateToken(user, TimeSpan.FromMinutes(_settings.ExpirationMinutes));
    }

    /// <inheritdoc />
    public AuthResultDto GenerateToken(User user, TimeSpan expiration)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            // Standard JWT claims
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),

            // .NET identity claims
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role.ToString()),
            new(ClaimTypes.Name, user.DisplayName),

            // Custom claims for multi-tenancy
            new("family_id", user.FamilyId),
            new("display_name", user.DisplayName),
            new("email_verified", user.EmailVerified.ToString().ToLowerInvariant())
        };

        var now = DateTime.UtcNow;
        var expires = now.Add(expiration);

        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            notBefore: now,
            expires: expires,
            signingCredentials: credentials);

        var accessToken = new JwtSecurityTokenHandler().WriteToken(token);

        _logger.LogInformation(
            "Generated access token for user {UserId} in family {FamilyId}",
            user.Id, user.FamilyId);

        return new AuthResultDto
        {
            AccessToken = accessToken,
            RefreshToken = GenerateRefreshToken(),
            TokenType = "Bearer",
            ExpiresIn = (int)expiration.TotalSeconds,
            User = new AuthUserDto
            {
                Id = user.Id,
                FamilyId = user.FamilyId,
                Email = user.Email,
                DisplayName = user.DisplayName,
                Role = user.Role.ToString(),
                IsFirstLogin = user.LastLoginAt == null
            }
        };
    }

    /// <inheritdoc />
    public AuthResultDto GenerateDeviceToken(Device device, string familyId)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            // Standard JWT claims
            new(JwtRegisteredClaimNames.Sub, device.Id),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),

            // .NET identity claims for device
            new(ClaimTypes.NameIdentifier, device.Id),
            new(ClaimTypes.Role, "Device"),
            new(ClaimTypes.Name, device.Name),

            // Custom claims for multi-tenancy
            new("family_id", familyId),
            new("device_id", device.Id),
            new("device_type", device.Type.ToString()),
            new("device_name", device.Name)
        };

        // Device tokens have longer expiration (30 days by default)
        var expiration = TimeSpan.FromDays(30);
        var now = DateTime.UtcNow;
        var expires = now.Add(expiration);

        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            notBefore: now,
            expires: expires,
            signingCredentials: credentials);

        var accessToken = new JwtSecurityTokenHandler().WriteToken(token);

        _logger.LogInformation(
            "Generated device token for device {DeviceId} in family {FamilyId}",
            device.Id, familyId);

        return new AuthResultDto
        {
            AccessToken = accessToken,
            RefreshToken = GenerateRefreshToken(),
            TokenType = "Bearer",
            ExpiresIn = (int)expiration.TotalSeconds,
            User = new AuthUserDto
            {
                Id = device.Id,
                FamilyId = familyId,
                DisplayName = device.Name,
                Role = "Device"
            }
        };
    }

    /// <inheritdoc />
    public string GenerateRefreshToken()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }

    /// <inheritdoc />
    public Task<AuthResultDto?> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        // Note: Token refresh with rotation is implemented via RefreshTokenCommand in the Application layer.
        // This method is kept for backward compatibility but redirects to the MediatR handler.
        // Use POST /api/auth/refresh endpoint for proper token refresh with rotation and theft detection.
        _logger.LogWarning("RefreshTokenAsync called on TokenService - use RefreshTokenCommand instead");
        return Task.FromResult<AuthResultDto?>(null);
    }

    /// <inheritdoc />
    public CaregiverAccessTokenDto GenerateCaregiverToken(string familyId, string targetUserId, TimeSpan expiration)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            // Standard JWT claims
            new(JwtRegisteredClaimNames.Sub, $"caregiver:{targetUserId}"),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),

            // .NET identity claims for caregiver
            new(ClaimTypes.NameIdentifier, $"caregiver:{targetUserId}"),
            new(ClaimTypes.Role, "Caregiver"),

            // Custom claims for caregiver access
            new("family_id", familyId),
            new("target_user_id", targetUserId),
            new("access_type", "caregiver"),
            new("is_read_only", "true")
        };

        var now = DateTime.UtcNow;
        var expires = now.Add(expiration);

        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            notBefore: now,
            expires: expires,
            signingCredentials: credentials);

        var accessToken = new JwtSecurityTokenHandler().WriteToken(token);

        _logger.LogInformation(
            "Generated caregiver access token for user {TargetUserId} in family {FamilyId}, expires at {ExpiresAt}",
            targetUserId, familyId, expires);

        // Generate the access URL (this would be configurable in production)
        var baseUrl = _settings.Issuer.TrimEnd('/');
        var accessUrl = $"{baseUrl}/caregiver?token={Uri.EscapeDataString(accessToken)}";

        return new CaregiverAccessTokenDto
        {
            Token = accessToken,
            TokenType = "Bearer",
            ExpiresIn = (int)expiration.TotalSeconds,
            ExpiresAt = expires,
            AccessUrl = accessUrl
        };
    }
}
