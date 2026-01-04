using FluentValidation;
using Luminous.Application.Common.Exceptions;
using Luminous.Application.DTOs;
using Luminous.Domain.Interfaces;
using MediatR;

namespace Luminous.Application.Features.CalendarConnections.Queries;

/// <summary>
/// Query to get a specific calendar connection.
/// </summary>
public sealed record GetCalendarConnectionQuery : IRequest<CalendarConnectionDto>
{
    public required string FamilyId { get; init; }
    public required string ConnectionId { get; init; }
}

public sealed class GetCalendarConnectionQueryValidator : AbstractValidator<GetCalendarConnectionQuery>
{
    public GetCalendarConnectionQueryValidator()
    {
        RuleFor(x => x.FamilyId).NotEmpty().WithMessage("Family ID is required");
        RuleFor(x => x.ConnectionId).NotEmpty().WithMessage("Connection ID is required");
    }
}

public sealed class GetCalendarConnectionQueryHandler
    : IRequestHandler<GetCalendarConnectionQuery, CalendarConnectionDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetCalendarConnectionQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<CalendarConnectionDto> Handle(
        GetCalendarConnectionQuery request,
        CancellationToken cancellationToken)
    {
        var connection = await _unitOfWork.CalendarConnections
            .GetByIdAsync(request.ConnectionId, request.FamilyId, cancellationToken);

        if (connection is null || connection.FamilyId != request.FamilyId)
            throw new NotFoundException("CalendarConnection", request.ConnectionId);

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
