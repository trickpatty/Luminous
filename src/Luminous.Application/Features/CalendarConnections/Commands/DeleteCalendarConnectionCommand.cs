using FluentValidation;
using Luminous.Application.Common.Exceptions;
using Luminous.Domain.Interfaces;
using MediatR;

namespace Luminous.Application.Features.CalendarConnections.Commands;

/// <summary>
/// Command to delete a calendar connection and its synced events.
/// </summary>
public sealed record DeleteCalendarConnectionCommand : IRequest
{
    public required string FamilyId { get; init; }
    public required string ConnectionId { get; init; }
    public bool DeleteSyncedEvents { get; init; } = true;
}

public sealed class DeleteCalendarConnectionCommandValidator : AbstractValidator<DeleteCalendarConnectionCommand>
{
    public DeleteCalendarConnectionCommandValidator()
    {
        RuleFor(x => x.FamilyId).NotEmpty().WithMessage("Family ID is required");
        RuleFor(x => x.ConnectionId).NotEmpty().WithMessage("Connection ID is required");
    }
}

public sealed class DeleteCalendarConnectionCommandHandler
    : IRequestHandler<DeleteCalendarConnectionCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteCalendarConnectionCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(DeleteCalendarConnectionCommand request, CancellationToken cancellationToken)
    {
        var connection = await _unitOfWork.CalendarConnections
            .GetByIdAsync(request.ConnectionId, request.FamilyId, cancellationToken);

        if (connection is null || connection.FamilyId != request.FamilyId)
            throw new NotFoundException("CalendarConnection", request.ConnectionId);

        // Delete synced events if requested
        if (request.DeleteSyncedEvents)
        {
            var calendarId = connection.ExternalCalendarId ?? connection.Id;
            var events = await _unitOfWork.Events
                .GetByExternalCalendarAsync(request.FamilyId, calendarId, cancellationToken);

            foreach (var evt in events)
            {
                await _unitOfWork.Events.DeleteAsync(evt, cancellationToken);
            }
        }

        // Disconnect and delete the connection
        connection.Disconnect();
        await _unitOfWork.CalendarConnections.DeleteAsync(connection, cancellationToken);
    }
}
