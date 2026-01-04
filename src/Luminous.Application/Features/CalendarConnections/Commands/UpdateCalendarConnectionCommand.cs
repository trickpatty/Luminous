using FluentValidation;
using Luminous.Application.Common.Exceptions;
using Luminous.Application.DTOs;
using Luminous.Domain.Interfaces;
using Luminous.Domain.ValueObjects;
using MediatR;

namespace Luminous.Application.Features.CalendarConnections.Commands;

/// <summary>
/// Command to update a calendar connection.
/// </summary>
public sealed record UpdateCalendarConnectionCommand : IRequest<CalendarConnectionDto>
{
    public required string FamilyId { get; init; }
    public required string ConnectionId { get; init; }
    public string? Name { get; init; }
    public List<string>? AssignedMemberIds { get; init; }
    public string? Color { get; init; }
    public bool? IsEnabled { get; init; }
    public CalendarSyncSettingsDto? SyncSettings { get; init; }
}

public sealed class UpdateCalendarConnectionCommandValidator : AbstractValidator<UpdateCalendarConnectionCommand>
{
    public UpdateCalendarConnectionCommandValidator()
    {
        RuleFor(x => x.FamilyId).NotEmpty().WithMessage("Family ID is required");
        RuleFor(x => x.ConnectionId).NotEmpty().WithMessage("Connection ID is required");
        RuleFor(x => x.Name)
            .MaximumLength(100)
            .When(x => !string.IsNullOrEmpty(x.Name))
            .WithMessage("Name must be 100 characters or less");
        RuleFor(x => x.Color)
            .Matches("^#[0-9A-Fa-f]{6}$")
            .When(x => !string.IsNullOrEmpty(x.Color))
            .WithMessage("Color must be a valid hex color code");
    }
}

public sealed class UpdateCalendarConnectionCommandHandler
    : IRequestHandler<UpdateCalendarConnectionCommand, CalendarConnectionDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateCalendarConnectionCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<CalendarConnectionDto> Handle(
        UpdateCalendarConnectionCommand request,
        CancellationToken cancellationToken)
    {
        var connection = await _unitOfWork.CalendarConnections
            .GetByIdAsync(request.ConnectionId, request.FamilyId, cancellationToken);

        if (connection is null || connection.FamilyId != request.FamilyId)
            throw new NotFoundException("CalendarConnection", request.ConnectionId);

        if (!string.IsNullOrEmpty(request.Name))
            connection.Name = request.Name;

        if (request.AssignedMemberIds is not null)
            connection.AssignToMembers(request.AssignedMemberIds);

        if (!string.IsNullOrEmpty(request.Color))
            connection.Color = request.Color;

        if (request.IsEnabled.HasValue)
        {
            if (request.IsEnabled.Value)
                connection.Enable();
            else
                connection.Pause();
        }

        if (request.SyncSettings is not null)
        {
            connection.SyncSettings = new CalendarSyncSettings
            {
                SyncIntervalMinutes = request.SyncSettings.SyncIntervalMinutes,
                SyncPastDays = request.SyncSettings.SyncPastDays,
                SyncFutureDays = request.SyncSettings.SyncFutureDays,
                ImportAllDayEvents = request.SyncSettings.ImportAllDayEvents,
                ImportDeclinedEvents = request.SyncSettings.ImportDeclinedEvents,
                TwoWaySync = request.SyncSettings.TwoWaySync
            };
        }

        connection.ModifiedAt = DateTime.UtcNow;
        await _unitOfWork.CalendarConnections.UpdateAsync(connection, cancellationToken);

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
