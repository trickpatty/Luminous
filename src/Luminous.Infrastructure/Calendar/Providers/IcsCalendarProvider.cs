using System.Net;
using System.Security.Cryptography;
using System.Text;
using Ical.Net;
using Ical.Net.CalendarComponents;
using Luminous.Application.Common.Interfaces;
using Luminous.Domain.Enums;
using Luminous.Domain.ValueObjects;
using Luminous.Infrastructure.Calendar.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Luminous.Infrastructure.Calendar.Providers;

/// <summary>
/// ICS URL subscription provider implementation.
/// </summary>
public sealed class IcsCalendarProvider : ICalendarProvider
{
    private readonly HttpClient _httpClient;
    private readonly IcsSettings _settings;
    private readonly ILogger<IcsCalendarProvider> _logger;

    public IcsCalendarProvider(
        HttpClient httpClient,
        IOptions<CalendarSettings> settings,
        ILogger<IcsCalendarProvider> logger)
    {
        _httpClient = httpClient;
        _settings = settings.Value.Ics;
        _logger = logger;
    }

    public CalendarProvider ProviderType => CalendarProvider.IcsUrl;
    public bool RequiresOAuth => false;
    public bool SupportsTwoWaySync => false;

    public Task<string> GetAuthorizationUrlAsync(string state, string redirectUri)
    {
        throw new NotSupportedException("ICS subscriptions do not require OAuth");
    }

    public Task<OAuthTokens> ExchangeCodeAsync(string code, string redirectUri)
    {
        throw new NotSupportedException("ICS subscriptions do not require OAuth");
    }

    public Task<OAuthTokens> RefreshTokensAsync(OAuthTokens tokens)
    {
        throw new NotSupportedException("ICS subscriptions do not require OAuth");
    }

    public Task RevokeTokensAsync(OAuthTokens tokens)
    {
        throw new NotSupportedException("ICS subscriptions do not require OAuth");
    }

    public Task<string> GetAccountEmailAsync(OAuthTokens tokens)
    {
        throw new NotSupportedException("ICS subscriptions do not have accounts");
    }

    public Task<IReadOnlyList<ExternalCalendarInfo>> GetCalendarsAsync(OAuthTokens tokens)
    {
        throw new NotSupportedException("ICS subscriptions do not have calendar lists");
    }

    public Task<CalendarSyncResult> FetchEventsAsync(
        OAuthTokens? tokens,
        string calendarId,
        DateTime startDate,
        DateTime endDate,
        string? syncToken = null)
    {
        throw new NotSupportedException("Use FetchIcsEventsAsync for ICS subscriptions");
    }

    public async Task<CalendarSyncResult> FetchIcsEventsAsync(string icsUrl, string? etag = null)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, icsUrl);
        request.Headers.Add("User-Agent", _settings.UserAgent);

        // Add If-None-Match for conditional fetch
        if (!string.IsNullOrEmpty(etag))
        {
            request.Headers.TryAddWithoutValidation("If-None-Match", etag);
        }

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(_settings.TimeoutSeconds));
        var response = await _httpClient.SendAsync(request, cts.Token);

        // 304 Not Modified - no changes
        if (response.StatusCode == HttpStatusCode.NotModified)
        {
            return new CalendarSyncResult([], [], null, etag, false);
        }

        response.EnsureSuccessStatusCode();

        // Check content length
        if (response.Content.Headers.ContentLength > _settings.MaxFileSizeBytes)
        {
            throw new InvalidOperationException($"ICS file exceeds maximum size of {_settings.MaxFileSizeBytes} bytes");
        }

        var content = await response.Content.ReadAsStringAsync(cts.Token);

        // Get new ETag if available
        var newEtag = response.Headers.ETag?.Tag;
        if (string.IsNullOrEmpty(newEtag))
        {
            // Generate a hash-based ETag if none provided
            newEtag = GenerateContentETag(content);
        }

        // If content hasn't changed, return empty result
        if (etag == newEtag)
        {
            return new CalendarSyncResult([], [], null, newEtag, false);
        }

        var events = ParseIcsContent(content);
        return new CalendarSyncResult(events, [], null, newEtag, true);
    }

    public Task<string> CreateEventAsync(OAuthTokens tokens, string calendarId, ExternalCalendarEvent calendarEvent)
    {
        throw new NotSupportedException("ICS subscriptions are read-only");
    }

    public Task UpdateEventAsync(OAuthTokens tokens, string calendarId, string eventId, ExternalCalendarEvent calendarEvent)
    {
        throw new NotSupportedException("ICS subscriptions are read-only");
    }

    public Task DeleteEventAsync(OAuthTokens tokens, string calendarId, string eventId)
    {
        throw new NotSupportedException("ICS subscriptions are read-only");
    }

    private IReadOnlyList<ExternalCalendarEvent> ParseIcsContent(string content)
    {
        var events = new List<ExternalCalendarEvent>();

        try
        {
            var calendar = Ical.Net.Calendar.Load(content);

            if (calendar?.Events is null)
                return events;

            foreach (var calEvent in calendar.Events)
            {
                try
                {
                    var externalEvent = MapToExternalEvent(calEvent);
                    if (externalEvent is not null)
                    {
                        events.Add(externalEvent);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to parse ICS event: {Uid}", calEvent.Uid);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to parse ICS content");
            throw new InvalidOperationException("Failed to parse ICS calendar", ex);
        }

        return events;
    }

    private static ExternalCalendarEvent? MapToExternalEvent(CalendarEvent calEvent)
    {
        if (calEvent.Start is null)
            return null;

        var startTime = calEvent.Start.AsUtc;
        var endTime = calEvent.End?.AsUtc ?? startTime.AddHours(1);
        var isAllDay = calEvent.IsAllDay;

        // Build RRULE string if recurrence exists
        string? rrule = null;
        if (calEvent.RecurrenceRules?.Count > 0)
        {
            var rule = calEvent.RecurrenceRules.First();
            rrule = $"RRULE:{rule}";
        }

        // Parse reminders from VALARM components
        var reminders = new List<int>();
        if (calEvent.Alarms?.Count > 0)
        {
            foreach (var alarm in calEvent.Alarms)
            {
                if (alarm.Trigger?.Duration is { } duration)
                {
                    // Ical.Net 5.x: Duration must be converted to TimeSpan
                    var timeSpan = duration.ToTimeSpanUnspecified();
                    var minutes = (int)Math.Abs(timeSpan.TotalMinutes);
                    if (minutes > 0 && minutes <= 10080) // Max 1 week
                    {
                        reminders.Add(minutes);
                    }
                }
            }
        }

        return new ExternalCalendarEvent
        {
            ExternalId = calEvent.Uid ?? string.Empty,
            Title = calEvent.Summary ?? "(No title)",
            Description = calEvent.Description,
            StartTime = startTime,
            EndTime = endTime,
            IsAllDay = isAllDay,
            Location = calEvent.Location,
            Color = null,
            RecurrenceRule = rrule,
#pragma warning disable CS0618 // RecurrenceId is deprecated but RecurrenceIdentifier lacks AsUtc
            RecurringEventId = calEvent.RecurrenceId?.AsUtc.ToString("yyyyMMddTHHmmssZ"),
#pragma warning restore CS0618
            OriginalStartTime = null,
            IsCancelled = calEvent.Status?.Equals("CANCELLED", StringComparison.OrdinalIgnoreCase) ?? false,
            Reminders = reminders,
            UpdatedAt = calEvent.LastModified?.AsUtc,
            OrganizerEmail = calEvent.Organizer?.Value?.ToString()?.Replace("mailto:", "")
        };
    }

    private static string GenerateContentETag(string content)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(content));
        return $"\"{Convert.ToBase64String(hash)[..16]}\"";
    }
}
