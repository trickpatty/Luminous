using System.Security.Cryptography;
using FluentValidation;
using Luminous.Application.Common.Interfaces;
using Luminous.Application.DTOs;
using Luminous.Domain.Entities;
using Luminous.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Luminous.Application.Features.Auth.Commands;

/// <summary>
/// Command to refresh an access token using a refresh token.
/// </summary>
public sealed record RefreshTokenCommand : IRequest<AuthResultDto?>
{
    /// <summary>
    /// The refresh token.
    /// </summary>
    public string RefreshToken { get; init; } = string.Empty;

    /// <summary>
    /// The client IP address.
    /// </summary>
    public string? IpAddress { get; init; }

    /// <summary>
    /// The client user agent.
    /// </summary>
    public string? UserAgent { get; init; }
}

/// <summary>
/// Validator for RefreshTokenCommand.
/// </summary>
public sealed class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage("Refresh token is required.");
    }
}

/// <summary>
/// Handler for RefreshTokenCommand.
/// </summary>
public sealed class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, AuthResultDto?>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITokenService _tokenService;
    private readonly ILogger<RefreshTokenCommandHandler> _logger;

    public RefreshTokenCommandHandler(
        IUnitOfWork unitOfWork,
        ITokenService tokenService,
        ILogger<RefreshTokenCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _tokenService = tokenService;
        _logger = logger;
    }

    public async Task<AuthResultDto?> Handle(
        RefreshTokenCommand request,
        CancellationToken cancellationToken)
    {
        // Hash the incoming token to find it in the database
        var tokenHash = HashToken(request.RefreshToken);
        var existingToken = await _unitOfWork.RefreshTokens.GetByTokenHashAsync(
            tokenHash,
            cancellationToken);

        if (existingToken == null)
        {
            _logger.LogWarning("Refresh token not found");
            return null;
        }

        if (!existingToken.IsActive)
        {
            // Token has been revoked or expired
            // Check if this is a reuse of a revoked token (potential token theft)
            if (existingToken.IsRevoked && existingToken.ReplacedByTokenId != null)
            {
                // Revoke all tokens for this user as a security measure
                await _unitOfWork.RefreshTokens.RevokeAllForUserAsync(
                    existingToken.UserId,
                    "potential token theft detected",
                    cancellationToken);

                _logger.LogWarning(
                    "Potential token theft detected for user {UserId}. All tokens revoked.",
                    existingToken.UserId);
            }
            else
            {
                _logger.LogWarning(
                    "Attempted to use inactive refresh token for user {UserId}",
                    existingToken.UserId);
            }

            return null;
        }

        // Get the user
        var user = await _unitOfWork.Users.GetByIdAsync(
            existingToken.UserId,
            existingToken.FamilyId,
            cancellationToken);

        if (user == null || !user.IsActive)
        {
            _logger.LogWarning(
                "User {UserId} not found or inactive during token refresh",
                existingToken.UserId);
            return null;
        }

        // Rotate the refresh token
        var (newToken, rawNewToken) = existingToken.Rotate(
            request.IpAddress,
            request.UserAgent);

        // Update the old token (now revoked)
        await _unitOfWork.RefreshTokens.UpdateAsync(existingToken, cancellationToken);

        // Store the new token
        await _unitOfWork.RefreshTokens.AddAsync(newToken, cancellationToken);

        // Generate new access token
        var authResult = _tokenService.GenerateToken(user);
        authResult = authResult with { RefreshToken = rawNewToken };

        _logger.LogInformation(
            "Token refreshed for user {UserId}",
            user.Id);

        return authResult;
    }

    private static string HashToken(string token)
    {
        var bytes = System.Text.Encoding.UTF8.GetBytes(token);
        var hash = SHA256.HashData(bytes);
        return Convert.ToBase64String(hash);
    }
}
