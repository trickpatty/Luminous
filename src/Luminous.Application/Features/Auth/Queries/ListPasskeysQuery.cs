using Luminous.Application.DTOs;
using Luminous.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Luminous.Application.Features.Auth.Queries;

/// <summary>
/// Query to list a user's passkeys.
/// </summary>
public sealed record ListPasskeysQuery : IRequest<PasskeyListResultDto>
{
    /// <summary>
    /// The user ID to list passkeys for.
    /// </summary>
    public string UserId { get; init; } = string.Empty;
}

/// <summary>
/// Handler for ListPasskeysQuery.
/// </summary>
public sealed class ListPasskeysQueryHandler : IRequestHandler<ListPasskeysQuery, PasskeyListResultDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ListPasskeysQueryHandler> _logger;

    public ListPasskeysQueryHandler(
        IUnitOfWork unitOfWork,
        ILogger<ListPasskeysQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<PasskeyListResultDto> Handle(
        ListPasskeysQuery request,
        CancellationToken cancellationToken)
    {
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
            "Listed {Count} passkeys for user {UserId}",
            passkeys.Count,
            request.UserId);

        return new PasskeyListResultDto
        {
            Passkeys = passkeys,
            TotalCount = passkeys.Count
        };
    }
}
