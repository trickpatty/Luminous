using FluentValidation;
using Luminous.Application.Common.Interfaces;
using Luminous.Application.DTOs;
using Luminous.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Luminous.Application.Features.Users.Queries;

/// <summary>
/// Query to list a specific user's passkeys (admin only).
/// </summary>
public sealed record ListUserPasskeysQuery : IRequest<PasskeyListResultDto>
{
    /// <summary>
    /// The family ID the user belongs to.
    /// </summary>
    public string FamilyId { get; init; } = string.Empty;

    /// <summary>
    /// The user ID to list passkeys for.
    /// </summary>
    public string UserId { get; init; } = string.Empty;
}

/// <summary>
/// Validator for ListUserPasskeysQuery.
/// </summary>
public sealed class ListUserPasskeysQueryValidator : AbstractValidator<ListUserPasskeysQuery>
{
    public ListUserPasskeysQueryValidator()
    {
        RuleFor(x => x.FamilyId)
            .NotEmpty().WithMessage("Family ID is required.");

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");
    }
}

/// <summary>
/// Handler for ListUserPasskeysQuery.
/// </summary>
public sealed class ListUserPasskeysQueryHandler : IRequestHandler<ListUserPasskeysQuery, PasskeyListResultDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<ListUserPasskeysQueryHandler> _logger;

    public ListUserPasskeysQueryHandler(
        IUnitOfWork unitOfWork,
        ITenantContext tenantContext,
        ILogger<ListUserPasskeysQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
        _logger = logger;
    }

    public async Task<PasskeyListResultDto> Handle(
        ListUserPasskeysQuery request,
        CancellationToken cancellationToken)
    {
        // Validate tenant access
        _tenantContext.EnsureAccessToFamily(request.FamilyId);

        // Verify user exists in the family
        var user = await _unitOfWork.Users.GetByIdAsync(
            request.UserId,
            request.FamilyId,
            cancellationToken);

        if (user == null)
        {
            _logger.LogWarning(
                "User {UserId} not found in family {FamilyId}",
                request.UserId,
                request.FamilyId);

            return new PasskeyListResultDto
            {
                Passkeys = [],
                TotalCount = 0
            };
        }

        // Get passkeys for the user
        var credentials = await _unitOfWork.Credentials.GetByUserIdAsync(
            request.UserId,
            cancellationToken);

        var passkeys = credentials
            .Select(c => new PasskeyDto
            {
                Id = c.Id,
                CredentialId = Convert.ToBase64String(c.CredentialId),
                DisplayName = c.DisplayName,
                RegisteredAt = c.RegisteredAt,
                LastUsedAt = c.LastUsedAt,
                AaGuid = c.AaGuid.ToString(),
                Transports = c.Transports,
                IsActive = c.IsActive
            })
            .ToList();

        _logger.LogDebug(
            "Admin listed {Count} passkeys for user {UserId} in family {FamilyId}",
            passkeys.Count,
            request.UserId,
            request.FamilyId);

        return new PasskeyListResultDto
        {
            Passkeys = passkeys,
            TotalCount = passkeys.Count
        };
    }
}
