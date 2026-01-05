using FluentValidation;
using Luminous.Application.DTOs;
using Luminous.Domain.Entities;
using Luminous.Domain.Interfaces;
using MediatR;

namespace Luminous.Application.Features.CalendarConnections.Queries;

/// <summary>
/// Query to get all calendar connections for a family.
/// </summary>
public sealed record GetCalendarConnectionsQuery : IRequest<IReadOnlyList<CalendarConnectionSummaryDto>>
{
    public required string FamilyId { get; init; }
    public bool? ActiveOnly { get; init; }
}

public sealed class GetCalendarConnectionsQueryValidator : AbstractValidator<GetCalendarConnectionsQuery>
{
    public GetCalendarConnectionsQueryValidator()
    {
        RuleFor(x => x.FamilyId).NotEmpty().WithMessage("Family ID is required");
    }
}

public sealed class GetCalendarConnectionsQueryHandler
    : IRequestHandler<GetCalendarConnectionsQuery, IReadOnlyList<CalendarConnectionSummaryDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetCalendarConnectionsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<CalendarConnectionSummaryDto>> Handle(
        GetCalendarConnectionsQuery request,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<CalendarConnection> connections;

        if (request.ActiveOnly == true)
        {
            connections = await _unitOfWork.CalendarConnections
                .GetActiveByFamilyAsync(request.FamilyId, cancellationToken);
        }
        else
        {
            connections = await _unitOfWork.CalendarConnections
                .GetByFamilyAsync(request.FamilyId, cancellationToken);
        }

        return connections.Select(c => new CalendarConnectionSummaryDto
        {
            Id = c.Id,
            Name = c.Name,
            Provider = c.Provider,
            Status = c.Status,
            IsEnabled = c.IsEnabled,
            Color = c.Color,
            LastSyncedAt = c.LastSyncedAt,
            SyncSettings = new CalendarSyncSettingsDto
            {
                SyncIntervalMinutes = c.SyncSettings.SyncIntervalMinutes,
                SyncPastDays = c.SyncSettings.SyncPastDays,
                SyncFutureDays = c.SyncSettings.SyncFutureDays,
                ImportAllDayEvents = c.SyncSettings.ImportAllDayEvents,
                ImportDeclinedEvents = c.SyncSettings.ImportDeclinedEvents,
                TwoWaySync = c.SyncSettings.TwoWaySync
            },
            AssignedMemberIds = c.AssignedMemberIds.ToList()
        }).ToList();
    }
}
