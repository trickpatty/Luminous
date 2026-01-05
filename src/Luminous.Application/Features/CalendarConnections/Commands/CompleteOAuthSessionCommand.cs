using FluentValidation;
using Luminous.Application.Common.Exceptions;
using Luminous.Application.Common.Interfaces;
using Luminous.Domain.Interfaces;
using MediatR;

namespace Luminous.Application.Features.CalendarConnections.Commands;

/// <summary>
/// Response from completing an OAuth flow.
/// </summary>
public record OAuthCompleteResponse(
    string SessionId,
    string AccountEmail,
    IReadOnlyList<ExternalCalendarInfo> Calendars);

/// <summary>
/// Command to complete an OAuth flow by exchanging the authorization code.
/// Returns the list of available calendars for the user to select from.
/// </summary>
public sealed record CompleteOAuthSessionCommand : IRequest<OAuthCompleteResponse>
{
    public required string FamilyId { get; init; }
    public required string Code { get; init; }
    public required string State { get; init; }
    public required string RedirectUri { get; init; }
}

public sealed class CompleteOAuthSessionCommandValidator : AbstractValidator<CompleteOAuthSessionCommand>
{
    public CompleteOAuthSessionCommandValidator()
    {
        RuleFor(x => x.FamilyId).NotEmpty().WithMessage("Family ID is required");
        RuleFor(x => x.Code).NotEmpty().WithMessage("Authorization code is required");
        RuleFor(x => x.State).NotEmpty().WithMessage("State parameter is required");
        RuleFor(x => x.RedirectUri)
            .NotEmpty()
            .Must(uri => Uri.TryCreate(uri, UriKind.Absolute, out _))
            .WithMessage("Valid redirect URI is required");
    }
}

public sealed class CompleteOAuthSessionCommandHandler
    : IRequestHandler<CompleteOAuthSessionCommand, OAuthCompleteResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEnumerable<ICalendarProvider> _providers;

    public CompleteOAuthSessionCommandHandler(
        IUnitOfWork unitOfWork,
        IEnumerable<ICalendarProvider> providers)
    {
        _unitOfWork = unitOfWork;
        _providers = providers;
    }

    public async Task<OAuthCompleteResponse> Handle(
        CompleteOAuthSessionCommand request,
        CancellationToken cancellationToken)
    {
        // Find the session by state
        // Note: The query already filters out expired sessions (expiresAt > now)
        var session = await _unitOfWork.OAuthSessions.GetByStateAsync(request.State, cancellationToken)
            ?? throw new BadRequestException(
                "Invalid or expired OAuth state. Please start the authorization process again.",
                "OAUTH_STATE_INVALID");

        // Validate session belongs to the family
        if (session.FamilyId != request.FamilyId)
            throw new BadRequestException(
                "OAuth session does not belong to this family.",
                "OAUTH_FAMILY_MISMATCH");

        // Defense-in-depth check for expiration (handles clock skew between DB and app server)
        if (session.IsExpired)
            throw new BadRequestException(
                "OAuth session has expired. Please start the authorization process again.",
                "OAUTH_SESSION_EXPIRED");

        if (session.IsCompleted)
            throw new BadRequestException(
                "This authorization has already been completed. Please start a new connection.",
                "OAUTH_SESSION_COMPLETED");

        // Validate redirect URI matches (security check)
        if (session.RedirectUri != request.RedirectUri)
            throw new BadRequestException(
                "Redirect URI mismatch. Please start the authorization process again.",
                "OAUTH_REDIRECT_MISMATCH");

        // Get the provider
        var provider = _providers.FirstOrDefault(p => p.ProviderType == session.Provider)
            ?? throw new BadRequestException(
                $"Calendar provider '{session.Provider}' is not available.",
                "OAUTH_PROVIDER_UNAVAILABLE");

        // Exchange code for tokens
        var tokens = await provider.ExchangeCodeAsync(request.Code, request.RedirectUri);

        // Get account email
        var accountEmail = await provider.GetAccountEmailAsync(tokens);

        // Complete the session (stores tokens)
        session.Complete(tokens, accountEmail);
        await _unitOfWork.OAuthSessions.UpdateAsync(session, cancellationToken);

        // Fetch available calendars
        var calendars = await provider.GetCalendarsAsync(tokens);

        return new OAuthCompleteResponse(session.Id, accountEmail, calendars);
    }
}
