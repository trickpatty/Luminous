using FluentValidation;
using Luminous.Application.Common.Exceptions;
using Luminous.Application.Common.Interfaces;
using Luminous.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Luminous.Application.Features.CalendarConnections.Commands;

/// <summary>
/// Command to complete an OAuth flow using only the state parameter.
/// The familyId is extracted from the server-side session, eliminating the need
/// for the client to provide it (useful for redirect flows where client state may be lost).
/// </summary>
public sealed record CompleteOAuthByStateCommand : IRequest<OAuthCompleteResponse>
{
    public required string Code { get; init; }
    public required string State { get; init; }
    public required string RedirectUri { get; init; }
}

public sealed class CompleteOAuthByStateCommandValidator : AbstractValidator<CompleteOAuthByStateCommand>
{
    public CompleteOAuthByStateCommandValidator()
    {
        RuleFor(x => x.Code).NotEmpty().WithMessage("Authorization code is required");
        RuleFor(x => x.State).NotEmpty().WithMessage("State parameter is required");
        RuleFor(x => x.RedirectUri)
            .NotEmpty()
            .Must(uri => Uri.TryCreate(uri, UriKind.Absolute, out _))
            .WithMessage("Valid redirect URI is required");
    }
}

public sealed class CompleteOAuthByStateCommandHandler
    : IRequestHandler<CompleteOAuthByStateCommand, OAuthCompleteResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEnumerable<ICalendarProvider> _providers;
    private readonly ILogger<CompleteOAuthByStateCommandHandler> _logger;

    public CompleteOAuthByStateCommandHandler(
        IUnitOfWork unitOfWork,
        IEnumerable<ICalendarProvider> providers,
        ILogger<CompleteOAuthByStateCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _providers = providers;
        _logger = logger;
    }

    public async Task<OAuthCompleteResponse> Handle(
        CompleteOAuthByStateCommand request,
        CancellationToken cancellationToken)
    {
        // Find the session by state - the state token is the security credential
        // Note: The query already filters out expired sessions (expiresAt > now)
        var session = await _unitOfWork.OAuthSessions.GetByStateAsync(request.State, cancellationToken)
            ?? throw new BadRequestException(
                "Invalid or expired OAuth state. Please start the authorization process again.",
                "OAUTH_STATE_INVALID");

        // Session already contains the familyId - no need to require it from client
        // The state token cryptographically proves ownership of this session

        // Log session details for debugging OAuth issues
        _logger.LogDebug(
            "OAuth session found: Id={SessionId}, FamilyId={FamilyId}, Provider={Provider}, " +
            "ExpiresAt={ExpiresAt:O}, IsExpired={IsExpired}, IsCompleted={IsCompleted}, " +
            "CreatedAt={CreatedAt:O}, Now={Now:O}",
            session.Id, session.FamilyId, session.Provider,
            session.ExpiresAt, session.IsExpired, session.IsCompleted,
            session.CreatedAt, DateTime.UtcNow);

        // Check for potential deserialization issue (ExpiresAt defaulting to DateTime.MinValue)
        if (session.ExpiresAt == DateTime.MinValue)
        {
            _logger.LogError(
                "OAuth session {SessionId} has ExpiresAt=DateTime.MinValue - likely a deserialization issue. " +
                "Session was created at {CreatedAt:O}",
                session.Id, session.CreatedAt);
            throw new BadRequestException(
                "OAuth session data is corrupted. Please start the authorization process again.",
                "OAUTH_SESSION_CORRUPTED");
        }

        // Defense-in-depth check for expiration (handles clock skew between DB and app server)
        if (session.IsExpired)
        {
            _logger.LogWarning(
                "OAuth session {SessionId} expired. ExpiresAt={ExpiresAt:O}, Now={Now:O}, " +
                "Difference={Difference}",
                session.Id, session.ExpiresAt, DateTime.UtcNow,
                DateTime.UtcNow - session.ExpiresAt);
            throw new BadRequestException(
                "OAuth session has expired. Please start the authorization process again.",
                "OAUTH_SESSION_EXPIRED");
        }

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
