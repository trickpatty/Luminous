using Luminous.Application.DTOs;
using Luminous.Domain.Entities;

namespace Luminous.Application.Common.Interfaces;

/// <summary>
/// Service for generating and validating authentication tokens.
/// </summary>
public interface ITokenService
{
    /// <summary>
    /// Generates an access token for the specified user.
    /// </summary>
    /// <param name="user">The user to generate a token for.</param>
    /// <returns>An authentication result containing the access token.</returns>
    AuthResultDto GenerateToken(User user);

    /// <summary>
    /// Generates an access token with a custom expiration.
    /// </summary>
    /// <param name="user">The user to generate a token for.</param>
    /// <param name="expiration">The token expiration time.</param>
    /// <returns>An authentication result containing the access token.</returns>
    AuthResultDto GenerateToken(User user, TimeSpan expiration);

    /// <summary>
    /// Generates a device token for a linked device.
    /// </summary>
    /// <param name="device">The device to generate a token for.</param>
    /// <param name="familyId">The family ID the device belongs to.</param>
    /// <returns>An authentication result containing the device token.</returns>
    AuthResultDto GenerateDeviceToken(Device device, string familyId);

    /// <summary>
    /// Generates a refresh token.
    /// </summary>
    /// <returns>A refresh token string.</returns>
    string GenerateRefreshToken();

    /// <summary>
    /// Validates a refresh token and returns a new token pair if valid.
    /// </summary>
    /// <param name="refreshToken">The refresh token to validate.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A new authentication result if valid, null otherwise.</returns>
    Task<AuthResultDto?> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
}
