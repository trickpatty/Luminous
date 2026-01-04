using FluentValidation;
using Luminous.Application.Common.Exceptions;
using Luminous.Application.Common.Interfaces;
using Luminous.Application.DTOs;
using Luminous.Domain.Enums;
using Luminous.Domain.Interfaces;
using MediatR;

namespace Luminous.Application.Features.CalendarConnections.Commands;

/// <summary>
/// Command to start the OAuth flow for a calendar connection.
/// </summary>
public sealed record StartOAuthCommand : IRequest<CalendarOAuthStartDto>
{
    public required string FamilyId { get; init; }
    public required string ConnectionId { get; init; }
    public required string RedirectUri { get; init; }
}

public sealed class StartOAuthCommandValidator : AbstractValidator<StartOAuthCommand>
{
    public StartOAuthCommandValidator()
    {
        RuleFor(x => x.FamilyId).NotEmpty().WithMessage("Family ID is required");
        RuleFor(x => x.ConnectionId).NotEmpty().WithMessage("Connection ID is required");
        RuleFor(x => x.RedirectUri)
            .NotEmpty()
            .Must(uri => Uri.TryCreate(uri, UriKind.Absolute, out _))
            .WithMessage("Valid redirect URI is required");
    }
}

public sealed class StartOAuthCommandHandler
    : IRequestHandler<StartOAuthCommand, CalendarOAuthStartDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICalendarSyncService _calendarSyncService;

    public StartOAuthCommandHandler(
        IUnitOfWork unitOfWork,
        ICalendarSyncService calendarSyncService)
    {
        _unitOfWork = unitOfWork;
        _calendarSyncService = calendarSyncService;
    }

    public async Task<CalendarOAuthStartDto> Handle(
        StartOAuthCommand request,
        CancellationToken cancellationToken)
    {
        var connection = await _unitOfWork.CalendarConnections
            .GetByIdAsync(request.ConnectionId, request.FamilyId, cancellationToken);

        if (connection is null || connection.FamilyId != request.FamilyId)
            throw new NotFoundException("CalendarConnection", request.ConnectionId);

        if (!connection.RequiresOAuth)
            throw new InvalidOperationException($"Calendar connection {request.ConnectionId} does not require OAuth");

        if (connection.Status == CalendarConnectionStatus.Active)
            throw new InvalidOperationException($"Calendar connection {request.ConnectionId} is already connected");

        var authUrl = await _calendarSyncService.GetAuthorizationUrlAsync(
            connection.Provider,
            request.ConnectionId,
            request.FamilyId,
            request.RedirectUri);

        return new CalendarOAuthStartDto
        {
            ConnectionId = connection.Id,
            AuthorizationUrl = authUrl
        };
    }
}
