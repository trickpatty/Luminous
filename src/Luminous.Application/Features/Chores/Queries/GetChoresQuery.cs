using FluentValidation;
using Luminous.Application.DTOs;
using Luminous.Domain.Interfaces;
using MediatR;

namespace Luminous.Application.Features.Chores.Queries;

/// <summary>
/// Query to get chores for a family.
/// </summary>
public sealed record GetChoresQuery : IRequest<IReadOnlyList<ChoreDto>>
{
    public string FamilyId { get; init; } = string.Empty;
    public string? AssigneeId { get; init; }
    public bool IncludeInactive { get; init; }
}

/// <summary>
/// Validator for GetChoresQuery.
/// </summary>
public sealed class GetChoresQueryValidator : AbstractValidator<GetChoresQuery>
{
    public GetChoresQueryValidator()
    {
        RuleFor(x => x.FamilyId)
            .NotEmpty().WithMessage("Family ID is required.");
    }
}

/// <summary>
/// Handler for GetChoresQuery.
/// </summary>
public sealed class GetChoresQueryHandler : IRequestHandler<GetChoresQuery, IReadOnlyList<ChoreDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetChoresQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<ChoreDto>> Handle(GetChoresQuery request, CancellationToken cancellationToken)
    {
        var chores = string.IsNullOrEmpty(request.AssigneeId)
            ? await _unitOfWork.Chores.GetByFamilyIdAsync(
                request.FamilyId,
                request.IncludeInactive,
                cancellationToken)
            : await _unitOfWork.Chores.GetByAssigneeAsync(
                request.FamilyId,
                request.AssigneeId,
                cancellationToken);

        // Get family members for assignee lookups
        var members = await _unitOfWork.Users.GetByFamilyIdAsync(request.FamilyId, cancellationToken);
        var memberLookup = members.ToDictionary(m => m.Id, m => new UserRefDto
        {
            Id = m.Id,
            DisplayName = m.DisplayName,
            AvatarUrl = m.Profile.AvatarUrl,
            Color = m.Profile.Color
        });

        return chores.Select(chore => new ChoreDto
        {
            Id = chore.Id,
            FamilyId = chore.FamilyId,
            Title = chore.Title,
            Description = chore.Description,
            Icon = chore.Icon,
            Assignees = chore.Assignees
                .Where(a => memberLookup.ContainsKey(a))
                .Select(a => memberLookup[a])
                .ToList(),
            IsForAnyone = chore.IsForAnyone,
            Recurrence = chore.Recurrence != null ? new RecurrenceRuleDto
            {
                Pattern = chore.Recurrence.Pattern,
                Interval = chore.Recurrence.Interval,
                DaysOfWeek = chore.Recurrence.DaysOfWeek,
                DayOfMonth = chore.Recurrence.DayOfMonth,
                EndDate = chore.Recurrence.EndDate?.ToString("yyyy-MM-dd"),
                MaxOccurrences = chore.Recurrence.MaxOccurrences
            } : null,
            DueTime = chore.DueTime?.ToString("HH:mm"),
            DueDate = chore.DueDate?.ToString("yyyy-MM-dd"),
            Points = chore.Points,
            Priority = chore.Priority,
            IsActive = chore.IsActive,
            EstimatedMinutes = chore.EstimatedMinutes,
            Room = chore.Room,
            CreatedAt = chore.CreatedAt
        }).ToList();
    }
}
