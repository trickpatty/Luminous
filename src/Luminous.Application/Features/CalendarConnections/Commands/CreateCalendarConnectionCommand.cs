using FluentValidation;
using Luminous.Application.Common.Interfaces;
using Luminous.Application.DTOs;
using Luminous.Domain.Entities;
using Luminous.Domain.Enums;
using Luminous.Domain.Interfaces;
using MediatR;

namespace Luminous.Application.Features.CalendarConnections.Commands;

/// <summary>
/// Command to create a new calendar connection.
/// </summary>
public sealed record CreateCalendarConnectionCommand : IRequest<CalendarConnectionDto>
{
    public required string FamilyId { get; init; }
    public required string Name { get; init; }
    public required CalendarProvider Provider { get; init; }
    public string? IcsUrl { get; init; }
    public List<string> AssignedMemberIds { get; init; } = [];
    public string? Color { get; init; }
}

public sealed class CreateCalendarConnectionCommandValidator : AbstractValidator<CreateCalendarConnectionCommand>
{
    public CreateCalendarConnectionCommandValidator()
    {
        RuleFor(x => x.FamilyId).NotEmpty().WithMessage("Family ID is required");
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100).WithMessage("Name is required and must be 100 characters or less");
        RuleFor(x => x.Provider).IsInEnum().WithMessage("Invalid provider");
        RuleFor(x => x.IcsUrl)
            .NotEmpty()
            .When(x => x.Provider == CalendarProvider.IcsUrl)
            .WithMessage("ICS URL is required for ICS subscriptions");
        RuleFor(x => x.IcsUrl)
            .Must(url => Uri.TryCreate(url, UriKind.Absolute, out var uri) &&
                  (uri.Scheme == "http" || uri.Scheme == "https" || uri.Scheme == "webcal"))
            .When(x => !string.IsNullOrEmpty(x.IcsUrl))
            .WithMessage("Invalid ICS URL format");
        RuleFor(x => x.Color)
            .Matches("^#[0-9A-Fa-f]{6}$")
            .When(x => !string.IsNullOrEmpty(x.Color))
            .WithMessage("Color must be a valid hex color code (e.g., #FF5733)");
    }
}

public sealed class CreateCalendarConnectionCommandHandler
    : IRequestHandler<CreateCalendarConnectionCommand, CalendarConnectionDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public CreateCalendarConnectionCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<CalendarConnectionDto> Handle(
        CreateCalendarConnectionCommand request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId ?? "system";

        CalendarConnection connection;

        if (request.Provider == CalendarProvider.IcsUrl)
        {
            connection = CalendarConnection.CreateIcsSubscription(
                request.FamilyId,
                request.Name,
                request.IcsUrl!,
                userId);
        }
        else
        {
            connection = CalendarConnection.CreateOAuth(
                request.FamilyId,
                request.Provider,
                request.Name,
                userId);
        }

        if (request.AssignedMemberIds.Count > 0)
            connection.AssignToMembers(request.AssignedMemberIds);

        if (!string.IsNullOrEmpty(request.Color))
            connection.Color = request.Color;

        await _unitOfWork.CalendarConnections.AddAsync(connection, cancellationToken);

        return MapToDto(connection);
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
