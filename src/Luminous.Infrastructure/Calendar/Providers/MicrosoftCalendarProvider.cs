using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Web;
using Luminous.Application.Common.Interfaces;
using Luminous.Domain.Enums;
using Luminous.Domain.ValueObjects;
using Luminous.Infrastructure.Calendar.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Luminous.Infrastructure.Calendar.Providers;

/// <summary>
/// Microsoft Graph Calendar API provider implementation.
/// </summary>
public sealed class MicrosoftCalendarProvider : ICalendarProvider
{
    private readonly HttpClient _httpClient;
    private readonly MicrosoftCalendarSettings _settings;
    private readonly ILogger<MicrosoftCalendarProvider> _logger;

    private const string GraphApiBaseUrl = "https://graph.microsoft.com/v1.0";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public MicrosoftCalendarProvider(
        HttpClient httpClient,
        IOptions<CalendarSettings> settings,
        ILogger<MicrosoftCalendarProvider> logger)
    {
        _httpClient = httpClient;
        _settings = settings.Value.Microsoft;
        _logger = logger;
    }

    public CalendarProvider ProviderType => CalendarProvider.Outlook;
    public bool RequiresOAuth => true;
    public bool SupportsTwoWaySync => true;

    private string AuthEndpoint => $"https://login.microsoftonline.com/{_settings.TenantId}/oauth2/v2.0/authorize";
    private string TokenEndpoint => $"https://login.microsoftonline.com/{_settings.TenantId}/oauth2/v2.0/token";

    public Task<string> GetAuthorizationUrlAsync(string state, string redirectUri)
    {
        var scopes = string.Join(" ", _settings.Scopes);
        var queryParams = HttpUtility.ParseQueryString(string.Empty);
        queryParams["client_id"] = _settings.ClientId;
        queryParams["redirect_uri"] = redirectUri;
        queryParams["response_type"] = "code";
        queryParams["scope"] = scopes;
        queryParams["response_mode"] = "query";
        queryParams["state"] = state;

        var url = $"{AuthEndpoint}?{queryParams}";
        return Task.FromResult(url);
    }

    public async Task<OAuthTokens> ExchangeCodeAsync(string code, string redirectUri)
    {
        var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["code"] = code,
            ["client_id"] = _settings.ClientId,
            ["client_secret"] = _settings.ClientSecret,
            ["redirect_uri"] = redirectUri,
            ["grant_type"] = "authorization_code",
            ["scope"] = string.Join(" ", _settings.Scopes)
        });

        var response = await _httpClient.PostAsync(TokenEndpoint, content);
        response.EnsureSuccessStatusCode();

        var tokenResponse = await response.Content.ReadFromJsonAsync<MicrosoftTokenResponse>(JsonOptions);
        if (tokenResponse is null)
            throw new InvalidOperationException("Failed to parse token response");

        return OAuthTokens.Create(
            tokenResponse.AccessToken,
            tokenResponse.RefreshToken,
            tokenResponse.ExpiresIn,
            tokenResponse.Scope);
    }

    public async Task<OAuthTokens> RefreshTokensAsync(OAuthTokens tokens)
    {
        if (string.IsNullOrEmpty(tokens.RefreshToken))
            throw new InvalidOperationException("No refresh token available");

        var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["refresh_token"] = tokens.RefreshToken,
            ["client_id"] = _settings.ClientId,
            ["client_secret"] = _settings.ClientSecret,
            ["grant_type"] = "refresh_token",
            ["scope"] = string.Join(" ", _settings.Scopes)
        });

        var response = await _httpClient.PostAsync(TokenEndpoint, content);
        response.EnsureSuccessStatusCode();

        var tokenResponse = await response.Content.ReadFromJsonAsync<MicrosoftTokenResponse>(JsonOptions);
        if (tokenResponse is null)
            throw new InvalidOperationException("Failed to parse token response");

        return OAuthTokens.Create(
            tokenResponse.AccessToken,
            tokenResponse.RefreshToken ?? tokens.RefreshToken,
            tokenResponse.ExpiresIn,
            tokenResponse.Scope);
    }

    public Task RevokeTokensAsync(OAuthTokens tokens)
    {
        // Microsoft doesn't have a revoke endpoint - tokens expire naturally
        _logger.LogInformation("Microsoft tokens cannot be explicitly revoked");
        return Task.CompletedTask;
    }

    public async Task<string> GetAccountEmailAsync(OAuthTokens tokens)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, $"{GraphApiBaseUrl}/me");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokens.AccessToken);

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<GraphUser>(JsonOptions);
        return result?.Mail ?? result?.UserPrincipalName ?? throw new InvalidOperationException("Failed to get account email");
    }

    public async Task<IReadOnlyList<ExternalCalendarInfo>> GetCalendarsAsync(OAuthTokens tokens)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, $"{GraphApiBaseUrl}/me/calendars");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokens.AccessToken);

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<GraphCalendarListResponse>(JsonOptions);
        if (result?.Value is null)
            return [];

        return result.Value.Select(c => new ExternalCalendarInfo(
            c.Id,
            c.Name ?? c.Id,
            null,
            c.HexColor,
            !c.CanEdit,
            c.IsDefaultCalendar ?? false,
            null
        )).ToList();
    }

    public async Task<CalendarSyncResult> FetchEventsAsync(
        OAuthTokens? tokens,
        string calendarId,
        DateTime startDate,
        DateTime endDate,
        string? syncToken = null)
    {
        if (tokens is null)
            throw new ArgumentNullException(nameof(tokens));

        var events = new List<ExternalCalendarEvent>();
        var deletedIds = new List<string>();
        string? nextLink = null;
        string? newDeltaToken = null;
        bool fullSyncRequired = false;

        // If we have a delta token, use delta query
        var url = !string.IsNullOrEmpty(syncToken)
            ? syncToken
            : BuildEventsUrl(calendarId, startDate, endDate);

        do
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, nextLink ?? url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokens.AccessToken);
            request.Headers.Add("Prefer", "odata.maxpagesize=100");

            var response = await _httpClient.SendAsync(request);

            // Handle 410 Gone - delta token is invalid
            if (response.StatusCode == System.Net.HttpStatusCode.Gone)
            {
                fullSyncRequired = true;
                url = BuildEventsUrl(calendarId, startDate, endDate);
                nextLink = null;
                events.Clear();
                deletedIds.Clear();
                continue;
            }

            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<GraphEventsResponse>(JsonOptions);
            if (result is null)
                break;

            if (result.Value is not null)
            {
                foreach (var item in result.Value)
                {
                    if (item.Removed is not null)
                    {
                        deletedIds.Add(item.Id);
                    }
                    else if (item.IsCancelled == true)
                    {
                        deletedIds.Add(item.Id);
                    }
                    else
                    {
                        var calEvent = MapToExternalEvent(item);
                        if (calEvent is not null)
                            events.Add(calEvent);
                    }
                }
            }

            nextLink = result.NextLink;
            newDeltaToken = result.DeltaLink;

        } while (!string.IsNullOrEmpty(nextLink));

        return new CalendarSyncResult(events, deletedIds, newDeltaToken, null, fullSyncRequired);
    }

    public Task<CalendarSyncResult> FetchIcsEventsAsync(string icsUrl, string? etag = null)
    {
        throw new NotSupportedException("Microsoft Calendar provider does not support ICS URLs");
    }

    public async Task<string> CreateEventAsync(OAuthTokens tokens, string calendarId, ExternalCalendarEvent calendarEvent)
    {
        var graphEvent = MapToGraphEvent(calendarEvent);

        using var request = new HttpRequestMessage(HttpMethod.Post, $"{GraphApiBaseUrl}/me/calendars/{calendarId}/events");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokens.AccessToken);
        request.Content = JsonContent.Create(graphEvent, options: JsonOptions);

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<GraphEvent>(JsonOptions);
        return result?.Id ?? throw new InvalidOperationException("Failed to create event");
    }

    public async Task UpdateEventAsync(OAuthTokens tokens, string calendarId, string eventId, ExternalCalendarEvent calendarEvent)
    {
        var graphEvent = MapToGraphEvent(calendarEvent);

        using var request = new HttpRequestMessage(HttpMethod.Patch, $"{GraphApiBaseUrl}/me/calendars/{calendarId}/events/{eventId}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokens.AccessToken);
        request.Content = JsonContent.Create(graphEvent, options: JsonOptions);

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteEventAsync(OAuthTokens tokens, string calendarId, string eventId)
    {
        using var request = new HttpRequestMessage(HttpMethod.Delete, $"{GraphApiBaseUrl}/me/calendars/{calendarId}/events/{eventId}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokens.AccessToken);

        var response = await _httpClient.SendAsync(request);
        // 404 is acceptable (already deleted)
        if (response.StatusCode != System.Net.HttpStatusCode.NotFound)
            response.EnsureSuccessStatusCode();
    }

    private static string BuildEventsUrl(string calendarId, DateTime startDate, DateTime endDate)
    {
        var start = startDate.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ");
        var end = endDate.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ");

        // Use delta query for incremental sync support
        return $"{GraphApiBaseUrl}/me/calendars/{calendarId}/calendarView/delta?startDateTime={start}&endDateTime={end}";
    }

    private static ExternalCalendarEvent? MapToExternalEvent(GraphEvent item)
    {
        if (item.Start is null || item.End is null)
            return null;

        DateTime startTime, endTime;
        bool isAllDay = item.IsAllDay ?? false;

        if (isAllDay)
        {
            startTime = DateTime.Parse(item.Start.DateTime);
            endTime = DateTime.Parse(item.End.DateTime);
        }
        else
        {
            startTime = DateTime.Parse(item.Start.DateTime);
            endTime = DateTime.Parse(item.End.DateTime);

            // Convert to UTC if timezone is specified
            if (!string.IsNullOrEmpty(item.Start.TimeZone) && item.Start.TimeZone != "UTC")
            {
                try
                {
                    var tz = TimeZoneInfo.FindSystemTimeZoneById(item.Start.TimeZone);
                    startTime = TimeZoneInfo.ConvertTimeToUtc(startTime, tz);
                }
                catch { /* Use as-is if timezone not found */ }
            }

            if (!string.IsNullOrEmpty(item.End.TimeZone) && item.End.TimeZone != "UTC")
            {
                try
                {
                    var tz = TimeZoneInfo.FindSystemTimeZoneById(item.End.TimeZone);
                    endTime = TimeZoneInfo.ConvertTimeToUtc(endTime, tz);
                }
                catch { /* Use as-is if timezone not found */ }
            }
        }

        var reminders = new List<int>();
        if (item.IsReminderOn == true && item.ReminderMinutesBeforeStart.HasValue)
        {
            reminders.Add(item.ReminderMinutesBeforeStart.Value);
        }

        // Build recurrence rule from pattern
        string? rrule = null;
        if (item.Recurrence?.Pattern is not null)
        {
            rrule = BuildRRuleFromPattern(item.Recurrence);
        }

        return new ExternalCalendarEvent
        {
            ExternalId = item.Id,
            Title = item.Subject ?? "(No title)",
            Description = item.BodyPreview,
            StartTime = startTime,
            EndTime = endTime,
            IsAllDay = isAllDay,
            Location = item.Location?.DisplayName,
            Color = null, // Graph API doesn't provide event colors
            RecurrenceRule = rrule,
            RecurringEventId = item.SeriesMasterId,
            OriginalStartTime = null,
            IsCancelled = item.IsCancelled ?? false,
            Reminders = reminders,
            UpdatedAt = item.LastModifiedDateTime,
            OrganizerEmail = item.Organizer?.EmailAddress?.Address
        };
    }

    private static string? BuildRRuleFromPattern(GraphRecurrence recurrence)
    {
        if (recurrence.Pattern is null)
            return null;

        var parts = new List<string>();

        var freq = recurrence.Pattern.Type?.ToUpperInvariant() switch
        {
            "DAILY" => "DAILY",
            "WEEKLY" => "WEEKLY",
            "ABSOLUTEMONTHLY" or "RELATIVEMONTHLY" => "MONTHLY",
            "ABSOLUTEYEARLY" or "RELATIVEYEARLY" => "YEARLY",
            _ => null
        };

        if (freq is null)
            return null;

        parts.Add($"FREQ={freq}");

        if (recurrence.Pattern.Interval > 1)
            parts.Add($"INTERVAL={recurrence.Pattern.Interval}");

        if (recurrence.Pattern.DaysOfWeek is not null && recurrence.Pattern.DaysOfWeek.Count > 0)
        {
            var days = recurrence.Pattern.DaysOfWeek
                .Select(d => d.ToUpperInvariant() switch
                {
                    "SUNDAY" => "SU",
                    "MONDAY" => "MO",
                    "TUESDAY" => "TU",
                    "WEDNESDAY" => "WE",
                    "THURSDAY" => "TH",
                    "FRIDAY" => "FR",
                    "SATURDAY" => "SA",
                    _ => null
                })
                .Where(d => d is not null);
            parts.Add($"BYDAY={string.Join(",", days)}");
        }

        if (recurrence.Range?.EndDate is not null)
        {
            var endDate = DateTime.Parse(recurrence.Range.EndDate).ToString("yyyyMMdd");
            parts.Add($"UNTIL={endDate}");
        }
        else if (recurrence.Range?.NumberOfOccurrences > 0)
        {
            parts.Add($"COUNT={recurrence.Range.NumberOfOccurrences}");
        }

        return $"RRULE:{string.Join(";", parts)}";
    }

    private static object MapToGraphEvent(ExternalCalendarEvent calendarEvent)
    {
        object start, end;

        if (calendarEvent.IsAllDay)
        {
            start = new { dateTime = calendarEvent.StartTime.ToString("yyyy-MM-dd"), timeZone = "UTC" };
            end = new { dateTime = calendarEvent.EndTime.ToString("yyyy-MM-dd"), timeZone = "UTC" };
        }
        else
        {
            start = new { dateTime = calendarEvent.StartTime.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss"), timeZone = "UTC" };
            end = new { dateTime = calendarEvent.EndTime.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss"), timeZone = "UTC" };
        }

        return new
        {
            subject = calendarEvent.Title,
            body = calendarEvent.Description is not null ? new { contentType = "text", content = calendarEvent.Description } : null,
            location = calendarEvent.Location is not null ? new { displayName = calendarEvent.Location } : null,
            start,
            end,
            isAllDay = calendarEvent.IsAllDay
        };
    }

    #region Microsoft Graph API Response Models

    private sealed record MicrosoftTokenResponse
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; init; } = string.Empty;

        [JsonPropertyName("refresh_token")]
        public string? RefreshToken { get; init; }

        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; init; }

        [JsonPropertyName("scope")]
        public string? Scope { get; init; }

        [JsonPropertyName("token_type")]
        public string TokenType { get; init; } = string.Empty;
    }

    private sealed record GraphCalendarListResponse
    {
        public List<GraphCalendar>? Value { get; init; }
    }

    private sealed record GraphCalendar
    {
        public string Id { get; init; } = string.Empty;
        public string? Name { get; init; }
        public string? HexColor { get; init; }
        public bool CanEdit { get; init; }
        public bool? IsDefaultCalendar { get; init; }
    }

    private sealed record GraphEventsResponse
    {
        public List<GraphEvent>? Value { get; init; }

        [JsonPropertyName("@odata.nextLink")]
        public string? NextLink { get; init; }

        [JsonPropertyName("@odata.deltaLink")]
        public string? DeltaLink { get; init; }
    }

    private sealed record GraphEvent
    {
        public string Id { get; init; } = string.Empty;
        public string? Subject { get; init; }
        public string? BodyPreview { get; init; }
        public GraphDateTimeTimeZone? Start { get; init; }
        public GraphDateTimeTimeZone? End { get; init; }
        public bool? IsAllDay { get; init; }
        public GraphLocation? Location { get; init; }
        public bool? IsCancelled { get; init; }
        public bool? IsReminderOn { get; init; }
        public int? ReminderMinutesBeforeStart { get; init; }
        public GraphRecurrence? Recurrence { get; init; }
        public string? SeriesMasterId { get; init; }
        public DateTime? LastModifiedDateTime { get; init; }
        public GraphOrganizer? Organizer { get; init; }

        [JsonPropertyName("@removed")]
        public GraphRemoved? Removed { get; init; }
    }

    private sealed record GraphDateTimeTimeZone
    {
        public string DateTime { get; init; } = string.Empty;
        public string? TimeZone { get; init; }
    }

    private sealed record GraphLocation
    {
        public string? DisplayName { get; init; }
    }

    private sealed record GraphRecurrence
    {
        public GraphRecurrencePattern? Pattern { get; init; }
        public GraphRecurrenceRange? Range { get; init; }
    }

    private sealed record GraphRecurrencePattern
    {
        public string? Type { get; init; }
        public int Interval { get; init; }
        public List<string>? DaysOfWeek { get; init; }
        public int? DayOfMonth { get; init; }
    }

    private sealed record GraphRecurrenceRange
    {
        public string? Type { get; init; }
        public string? StartDate { get; init; }
        public string? EndDate { get; init; }
        public int? NumberOfOccurrences { get; init; }
    }

    private sealed record GraphOrganizer
    {
        public GraphEmailAddress? EmailAddress { get; init; }
    }

    private sealed record GraphEmailAddress
    {
        public string? Address { get; init; }
        public string? Name { get; init; }
    }

    private sealed record GraphRemoved
    {
        public string? Reason { get; init; }
    }

    private sealed record GraphUser
    {
        public string? Mail { get; init; }
        public string? UserPrincipalName { get; init; }
    }

    #endregion
}
