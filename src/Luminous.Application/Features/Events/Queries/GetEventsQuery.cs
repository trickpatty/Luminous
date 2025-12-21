using FluentValidation;
using Luminous.Application.DTOs;
using Luminous.Domain.Interfaces;
using MediatR;

namespace Luminous.Application.Features.Events.Queries;

/// <summary>
/// Query to get events for a family within a date range.
/// </summary>
public sealed record GetEventsQuery : IRequest<IReadOnlyList<EventSummaryDto>>
{
    public string FamilyId { get; init; } = string.Empty;
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    public string? AssigneeId { get; init; }
}

/// <summary>
/// Validator for GetEventsQuery.
/// </summary>
public sealed class GetEventsQueryValidator : AbstractValidator<GetEventsQuery>
{
    public GetEventsQueryValidator()
    {
        RuleFor(x => x.FamilyId)
            .NotEmpty().WithMessage("Family ID is required.");

        RuleFor(x => x.EndDate)
            .GreaterThan(x => x.StartDate)
            .WithMessage("End date must be after start date.");

        RuleFor(x => x)
            .Must(x => (x.EndDate - x.StartDate).TotalDays <= 90)
            .WithMessage("Date range cannot exceed 90 days.");
    }
}

/// <summary>
/// Handler for GetEventsQuery.
/// </summary>
public sealed class GetEventsQueryHandler : IRequestHandler<GetEventsQuery, IReadOnlyList<EventSummaryDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetEventsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<EventSummaryDto>> Handle(GetEventsQuery request, CancellationToken cancellationToken)
    {
        var events = string.IsNullOrEmpty(request.AssigneeId)
            ? await _unitOfWork.Events.GetByDateRangeAsync(
                request.FamilyId,
                request.StartDate,
                request.EndDate,
                cancellationToken)
            : await _unitOfWork.Events.GetByAssigneeAsync(
                request.FamilyId,
                request.AssigneeId,
                request.StartDate,
                request.EndDate,
                cancellationToken);

        return events.Select(e => new EventSummaryDto
        {
            Id = e.Id,
            Title = e.Title,
            StartTime = e.StartTime,
            EndTime = e.EndTime,
            IsAllDay = e.IsAllDay,
            AssigneeIds = e.Assignees,
            Color = e.Color
        }).ToList();
    }
}
