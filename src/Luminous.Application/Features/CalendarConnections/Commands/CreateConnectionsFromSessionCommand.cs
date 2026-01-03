using FluentValidation;
using Luminous.Application.Common.Interfaces;
using Luminous.Application.DTOs;
using Luminous.Domain.Entities;
using Luminous.Domain.Interfaces;
using MediatR;

namespace Luminous.Application.Features.CalendarConnections.Commands;

/// <summary>
/// Request to create a calendar connection from an OAuth session.
/// </summary>
public record CreateConnectionFromSessionRequest
{
    /// <summary>
    /// External calendar ID from the provider.
    /// </summary>
    public required string ExternalCalendarId { get; init; }

    /// <summary>
    /// Display name for the calendar.
    /// </summary>
    public required string DisplayName { get; init; }

    /// <summary>
    /// Calendar color (hex code).
    /// </summary>
    public string? Color { get; init; }

    /// <summary>
    /// Family member IDs to assign this calendar to.
    /// </summary>
    public List<string> AssignedMemberIds { get; init; } = [];
}

/// <summary>
/// Command to create calendar connections from a completed OAuth session.
/// </summary>
public sealed record CreateConnectionsFromSessionCommand : IRequest<IReadOnlyList<CalendarConnectionDto>>
{
    public required string FamilyId { get; init; }
    public required string SessionId { get; init; }
    public required List<CreateConnectionFromSessionRequest> Calendars { get; init; }
}

public sealed class CreateConnectionsFromSessionCommandValidator
    : AbstractValidator<CreateConnectionsFromSessionCommand>
{
    public CreateConnectionsFromSessionCommandValidator()
    {
        RuleFor(x => x.FamilyId).NotEmpty().WithMessage("Family ID is required");
        RuleFor(x => x.SessionId).NotEmpty().WithMessage("Session ID is required");
        RuleFor(x => x.Calendars).NotEmpty().WithMessage("At least one calendar is required");
        RuleForEach(x => x.Calendars).ChildRules(calendar =>
        {
            calendar.RuleFor(c => c.ExternalCalendarId).NotEmpty();
            calendar.RuleFor(c => c.DisplayName).NotEmpty().MaximumLength(100);
            calendar.RuleFor(c => c.Color)
                .Matches("^#[0-9A-Fa-f]{6}$")
                .When(c => !string.IsNullOrEmpty(c.Color))
                .WithMessage("Color must be a valid hex color code");
        });
    }
}

public sealed class CreateConnectionsFromSessionCommandHandler
    : IRequestHandler<CreateConnectionsFromSessionCommand, IReadOnlyList<CalendarConnectionDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public CreateConnectionsFromSessionCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<IReadOnlyList<CalendarConnectionDto>> Handle(
        CreateConnectionsFromSessionCommand request,
        CancellationToken cancellationToken)
    {
        // Get the OAuth session
        var session = await _unitOfWork.OAuthSessions.GetByIdAsync(
            request.SessionId, request.FamilyId, cancellationToken)
            ?? throw new InvalidOperationException("OAuth session not found");

        if (!session.IsValidForConnectionCreation)
        {
            if (session.IsExpired)
                throw new InvalidOperationException("OAuth session has expired. Please start again.");
            if (!session.IsCompleted)
                throw new InvalidOperationException("OAuth session has not been completed");
            throw new InvalidOperationException("OAuth session is not valid for connection creation");
        }

        var userId = _currentUserService.UserId ?? session.CreatedBy;
        var connections = new List<CalendarConnectionDto>();

        foreach (var calendarRequest in request.Calendars)
        {
            // Check for duplicate - same external calendar ID
            var existing = await _unitOfWork.CalendarConnections.GetByExternalIdAsync(
                request.FamilyId, calendarRequest.ExternalCalendarId, cancellationToken);

            if (existing != null)
            {
                // Skip duplicates, could optionally update instead
                continue;
            }

            // Create the connection with tokens from session
            var connection = CalendarConnection.CreateOAuth(
                request.FamilyId,
                session.Provider,
                calendarRequest.DisplayName,
                userId);

            // Set the external calendar info
            connection.SetExternalCalendarInfo(
                calendarRequest.ExternalCalendarId,
                session.AccountEmail!);

            // Set tokens from session
            connection.SetOAuthTokens(session.Tokens!);

            // Assign to members
            if (calendarRequest.AssignedMemberIds.Count > 0)
                connection.AssignToMembers(calendarRequest.AssignedMemberIds);

            // Set color
            if (!string.IsNullOrEmpty(calendarRequest.Color))
                connection.Color = calendarRequest.Color;

            // Activate the connection
            connection.Activate();

            await _unitOfWork.CalendarConnections.AddAsync(connection, cancellationToken);

            connections.Add(MapToDto(connection));
        }

        // Delete the OAuth session now that connections are created
        await _unitOfWork.OAuthSessions.DeleteAsync(session.Id, session.FamilyId, cancellationToken);

        return connections;
    }

    private static CalendarConnectionDto MapToDto(CalendarConnection connection)
    {
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
