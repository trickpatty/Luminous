using FluentValidation;
using Luminous.Application.Common.Interfaces;
using Luminous.Domain.Entities;
using Luminous.Domain.Enums;
using Luminous.Domain.Interfaces;
using MediatR;

namespace Luminous.Application.Features.CalendarConnections.Commands;

/// <summary>
/// Response from initiating an OAuth flow.
/// </summary>
public record OAuthInitiateResponse(
    string SessionId,
    string AuthorizationUrl,
    string State);

/// <summary>
/// Command to initiate an OAuth flow for connecting calendars.
/// Creates a temporary OAuth session that stores tokens between steps.
/// </summary>
public sealed record InitiateOAuthCommand : IRequest<OAuthInitiateResponse>
{
    public required string FamilyId { get; init; }
    public required CalendarProvider Provider { get; init; }
    public required string RedirectUri { get; init; }
}

public sealed class InitiateOAuthCommandValidator : AbstractValidator<InitiateOAuthCommand>
{
    public InitiateOAuthCommandValidator()
    {
        RuleFor(x => x.FamilyId).NotEmpty().WithMessage("Family ID is required");
        RuleFor(x => x.Provider)
            .IsInEnum()
            .Must(p => p != CalendarProvider.IcsUrl)
            .WithMessage("OAuth is not supported for ICS URL subscriptions");
        RuleFor(x => x.RedirectUri)
            .NotEmpty()
            .Must(uri => Uri.TryCreate(uri, UriKind.Absolute, out _))
            .WithMessage("Valid redirect URI is required");
    }
}

public sealed class InitiateOAuthCommandHandler
    : IRequestHandler<InitiateOAuthCommand, OAuthInitiateResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEnumerable<ICalendarProvider> _providers;
    private readonly ICurrentUserService _currentUserService;

    public InitiateOAuthCommandHandler(
        IUnitOfWork unitOfWork,
        IEnumerable<ICalendarProvider> providers,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _providers = providers;
        _currentUserService = currentUserService;
    }

    public async Task<OAuthInitiateResponse> Handle(
        InitiateOAuthCommand request,
        CancellationToken cancellationToken)
    {
        var provider = _providers.FirstOrDefault(p => p.ProviderType == request.Provider)
            ?? throw new InvalidOperationException($"No provider found for {request.Provider}");

        if (!provider.RequiresOAuth)
            throw new InvalidOperationException($"Provider {request.Provider} does not support OAuth");

        var userId = _currentUserService.UserId ?? "system";

        // Create OAuth session
        var session = OAuthSession.Create(
            request.FamilyId,
            request.Provider,
            request.RedirectUri,
            userId);

        await _unitOfWork.OAuthSessions.AddAsync(session, cancellationToken);

        // Generate authorization URL using session state
        var authUrl = await provider.GetAuthorizationUrlAsync(session.State, request.RedirectUri);

        return new OAuthInitiateResponse(session.Id, authUrl, session.State);
    }
}
