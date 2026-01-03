using FluentValidation;
using Luminous.Application.Common.Exceptions;
using Luminous.Application.Common.Interfaces;
using Luminous.Domain.Interfaces;
using MediatR;

namespace Luminous.Application.Features.CalendarConnections.Commands;

/// <summary>
/// Command to manually trigger a sync for a calendar connection.
/// </summary>
public sealed record SyncCalendarConnectionCommand : IRequest<CalendarSyncResultDto>
{
    public required string FamilyId { get; init; }
    public required string ConnectionId { get; init; }
}

/// <summary>
/// Result of a calendar sync operation.
/// </summary>
public record CalendarSyncResultDto
{
    public required string ConnectionId { get; init; }
    public bool Success { get; init; }
    public int EventsAdded { get; init; }
    public int EventsUpdated { get; init; }
    public int EventsDeleted { get; init; }
    public string? ErrorMessage { get; init; }
    public TimeSpan Duration { get; init; }
}

public sealed class SyncCalendarConnectionCommandValidator : AbstractValidator<SyncCalendarConnectionCommand>
{
    public SyncCalendarConnectionCommandValidator()
    {
        RuleFor(x => x.FamilyId).NotEmpty().WithMessage("Family ID is required");
        RuleFor(x => x.ConnectionId).NotEmpty().WithMessage("Connection ID is required");
    }
}

public sealed class SyncCalendarConnectionCommandHandler
    : IRequestHandler<SyncCalendarConnectionCommand, CalendarSyncResultDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICalendarSyncService _calendarSyncService;

    public SyncCalendarConnectionCommandHandler(
        IUnitOfWork unitOfWork,
        ICalendarSyncService calendarSyncService)
    {
        _unitOfWork = unitOfWork;
        _calendarSyncService = calendarSyncService;
    }

    public async Task<CalendarSyncResultDto> Handle(
        SyncCalendarConnectionCommand request,
        CancellationToken cancellationToken)
    {
        var connection = await _unitOfWork.CalendarConnections
            .GetByIdAsync(request.ConnectionId, request.FamilyId, cancellationToken);

        if (connection is null || connection.FamilyId != request.FamilyId)
            throw new NotFoundException("CalendarConnection", request.ConnectionId);

        var result = await _calendarSyncService.SyncAsync(connection, cancellationToken);

        return new CalendarSyncResultDto
        {
            ConnectionId = result.ConnectionId,
            Success = result.Success,
            EventsAdded = result.EventsAdded,
            EventsUpdated = result.EventsUpdated,
            EventsDeleted = result.EventsDeleted,
            ErrorMessage = result.ErrorMessage,
            Duration = result.Duration
        };
    }
}
