using FluentValidation;
using Luminous.Application.Common.Interfaces;
using Luminous.Domain.Enums;
using MediatR;

namespace Luminous.Application.Features.CalendarConnections.Commands;

/// <summary>
/// Response from validating an ICS URL.
/// </summary>
public record ValidateIcsUrlResponse(
    bool IsValid,
    string? CalendarName,
    int? EventCount,
    string? Error);

/// <summary>
/// Command to validate an ICS URL before creating a connection.
/// </summary>
public sealed record ValidateIcsUrlCommand : IRequest<ValidateIcsUrlResponse>
{
    public required string FamilyId { get; init; }
    public required string Url { get; init; }
}

public sealed class ValidateIcsUrlCommandValidator : AbstractValidator<ValidateIcsUrlCommand>
{
    public ValidateIcsUrlCommandValidator()
    {
        RuleFor(x => x.FamilyId).NotEmpty().WithMessage("Family ID is required");
        RuleFor(x => x.Url)
            .NotEmpty()
            .Must(url =>
            {
                if (string.IsNullOrEmpty(url)) return false;
                // Normalize webcal:// to https:// for validation
                var normalizedUrl = url.StartsWith("webcal://", StringComparison.OrdinalIgnoreCase)
                    ? "https://" + url[9..]
                    : url;
                return Uri.TryCreate(normalizedUrl, UriKind.Absolute, out var uri) &&
                       uri.Scheme is "http" or "https";
            })
            .WithMessage("Valid ICS URL is required (http, https, or webcal)");
    }
}

public sealed class ValidateIcsUrlCommandHandler
    : IRequestHandler<ValidateIcsUrlCommand, ValidateIcsUrlResponse>
{
    private readonly IEnumerable<ICalendarProvider> _providers;

    public ValidateIcsUrlCommandHandler(IEnumerable<ICalendarProvider> providers)
    {
        _providers = providers;
    }

    public async Task<ValidateIcsUrlResponse> Handle(
        ValidateIcsUrlCommand request,
        CancellationToken cancellationToken)
    {
        var provider = _providers.FirstOrDefault(p => p.ProviderType == CalendarProvider.IcsUrl);
        if (provider == null)
        {
            return new ValidateIcsUrlResponse(false, null, null, "ICS provider not available");
        }

        try
        {
            // Normalize webcal:// to https://
            var url = request.Url.StartsWith("webcal://", StringComparison.OrdinalIgnoreCase)
                ? "https://" + request.Url[9..]
                : request.Url;

            // Try to fetch the ICS file
            var result = await provider.FetchIcsEventsAsync(url, null);

            // Extract calendar name from the first event or use a default
            string? calendarName = null;
            if (result.Events.Count > 0)
            {
                // Try to find a sensible name - often the organizer or a common prefix
                var firstEvent = result.Events[0];
                calendarName = !string.IsNullOrEmpty(firstEvent.OrganizerEmail)
                    ? firstEvent.OrganizerEmail.Split('@')[0]
                    : null;
            }

            return new ValidateIcsUrlResponse(
                true,
                calendarName,
                result.Events.Count,
                null);
        }
        catch (HttpRequestException ex)
        {
            return new ValidateIcsUrlResponse(
                false,
                null,
                null,
                ex.StatusCode.HasValue
                    ? $"Failed to fetch calendar: HTTP {(int)ex.StatusCode} {ex.StatusCode}"
                    : "Failed to fetch calendar: Network error");
        }
        catch (Exception ex)
        {
            return new ValidateIcsUrlResponse(
                false,
                null,
                null,
                $"Failed to parse calendar: {ex.Message}");
        }
    }
}
