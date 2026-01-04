using FluentValidation;
using Luminous.Application.Common.Interfaces;
using Luminous.Application.DTOs;
using Luminous.Domain.Interfaces;
using MediatR;

namespace Luminous.Application.Features.CalendarConnections.Commands;

/// <summary>
/// Command to complete the OAuth flow for a calendar connection.
/// </summary>
public sealed record CompleteOAuthCommand : IRequest<CalendarConnectionDto>
{
    public required string ConnectionId { get; init; }
    public required string FamilyId { get; init; }
    public required string Code { get; init; }
    public required string RedirectUri { get; init; }
}

public sealed class CompleteOAuthCommandValidator : AbstractValidator<CompleteOAuthCommand>
{
    public CompleteOAuthCommandValidator()
    {
        RuleFor(x => x.ConnectionId).NotEmpty().WithMessage("Connection ID is required");
        RuleFor(x => x.FamilyId).NotEmpty().WithMessage("Family ID is required");
        RuleFor(x => x.Code).NotEmpty().WithMessage("Authorization code is required");
        RuleFor(x => x.RedirectUri)
            .NotEmpty()
            .Must(uri => Uri.TryCreate(uri, UriKind.Absolute, out _))
            .WithMessage("Valid redirect URI is required");
    }
}

public sealed class CompleteOAuthCommandHandler
    : IRequestHandler<CompleteOAuthCommand, CalendarConnectionDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICalendarSyncService _calendarSyncService;

    public CompleteOAuthCommandHandler(
        IUnitOfWork unitOfWork,
        ICalendarSyncService calendarSyncService)
    {
        _unitOfWork = unitOfWork;
        _calendarSyncService = calendarSyncService;
    }

    public async Task<CalendarConnectionDto> Handle(
        CompleteOAuthCommand request,
        CancellationToken cancellationToken)
    {
        await _calendarSyncService.CompleteOAuthAsync(
            request.ConnectionId,
            request.Code,
            request.RedirectUri,
            cancellationToken);

        // Fetch the updated connection
        var connection = await _unitOfWork.CalendarConnections
            .GetByIdAsync(request.ConnectionId, request.FamilyId, cancellationToken);

        if (connection is null)
            throw new InvalidOperationException("Connection not found after OAuth completion");

        return new CalendarConnectionDto
        {
            Id = connection.Id,
            FamilyId = connection.FamilyId,
            Name = connection.Name,
            Provider = connection.Provider,
            Status = connection.Status,
            ExternalAccountId = connection.ExternalAccountId,
            AssignedMemberIds = connection.AssignedMemberIds,
            Color = connection.Color,
            IsEnabled = connection.IsEnabled,
            IsReadOnly = connection.IsReadOnly,
            LastSyncedAt = connection.LastSyncedAt,
            NextSyncAt = connection.NextSyncAt,
            LastSyncError = connection.LastSyncError,
            ConsecutiveFailures = connection.ConsecutiveFailures,
            SyncSettings = new CalendarSyncSettingsDto
            {
                SyncIntervalMinutes = connection.SyncSettings.SyncIntervalMinutes,
                SyncPastDays = connection.SyncSettings.SyncPastDays,
                SyncFutureDays = connection.SyncSettings.SyncFutureDays,
                ImportAllDayEvents = connection.SyncSettings.ImportAllDayEvents,
                ImportDeclinedEvents = connection.SyncSettings.ImportDeclinedEvents,
                TwoWaySync = connection.SyncSettings.TwoWaySync
            },
            CreatedAt = connection.CreatedAt
        };
    }
}
