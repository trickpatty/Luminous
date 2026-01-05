using FluentValidation;
using Luminous.Application.Common.Interfaces;
using Luminous.Domain.Interfaces;
using MediatR;

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

    public CompleteOAuthByStateCommandHandler(
        IUnitOfWork unitOfWork,
        IEnumerable<ICalendarProvider> providers)
    {
        _unitOfWork = unitOfWork;
        _providers = providers;
    }

    public async Task<OAuthCompleteResponse> Handle(
        CompleteOAuthByStateCommand request,
        CancellationToken cancellationToken)
    {
        // Find the session by state - the state token is the security credential
        var session = await _unitOfWork.OAuthSessions.GetByStateAsync(request.State, cancellationToken)
            ?? throw new InvalidOperationException("Invalid or expired OAuth state. Please try again.");

        // Session already contains the familyId - no need to require it from client
        // The state token cryptographically proves ownership of this session

        if (session.IsExpired)
            throw new InvalidOperationException("OAuth session has expired. Please start again.");

        if (session.IsCompleted)
            throw new InvalidOperationException("OAuth session has already been completed");

        // Validate redirect URI matches
        if (session.RedirectUri != request.RedirectUri)
            throw new InvalidOperationException("Redirect URI mismatch");

        // Get the provider
        var provider = _providers.FirstOrDefault(p => p.ProviderType == session.Provider)
            ?? throw new InvalidOperationException($"No provider found for {session.Provider}");

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
