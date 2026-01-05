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
/// Google Calendar API provider implementation.
/// </summary>
public sealed class GoogleCalendarProvider : ICalendarProvider
{
    private readonly HttpClient _httpClient;
    private readonly GoogleCalendarSettings _settings;
    private readonly ILogger<GoogleCalendarProvider> _logger;

    private const string AuthEndpoint = "https://accounts.google.com/o/oauth2/v2/auth";
    private const string TokenEndpoint = "https://oauth2.googleapis.com/token";
    private const string RevokeEndpoint = "https://oauth2.googleapis.com/revoke";
    private const string CalendarApiBaseUrl = "https://www.googleapis.com/calendar/v3";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public GoogleCalendarProvider(
        HttpClient httpClient,
        IOptions<CalendarSettings> settings,
        ILogger<GoogleCalendarProvider> logger)
    {
        _httpClient = httpClient;
        _settings = settings.Value.Google;
        _logger = logger;
    }

    public CalendarProvider ProviderType => CalendarProvider.Google;
    public bool RequiresOAuth => true;
    public bool SupportsTwoWaySync => true;

    public Task<string> GetAuthorizationUrlAsync(string state, string redirectUri)
    {
        // Validate OAuth configuration
        if (string.IsNullOrWhiteSpace(_settings.ClientId))
        {
            _logger.LogError("Google Calendar OAuth client_id is not configured. Check Calendar:Google:ClientId in app configuration or Key Vault.");
            throw new InvalidOperationException(
                "Google Calendar integration is not configured. Please contact your administrator to set up Google OAuth credentials.");
        }

        var scopes = string.Join(" ", _settings.Scopes);
        var queryParams = HttpUtility.ParseQueryString(string.Empty);
        queryParams["client_id"] = _settings.ClientId;
        queryParams["redirect_uri"] = redirectUri;
        queryParams["response_type"] = "code";
        queryParams["scope"] = scopes;
        queryParams["access_type"] = "offline";
        queryParams["prompt"] = "consent";
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
            ["grant_type"] = "authorization_code"
        });

        var response = await _httpClient.PostAsync(TokenEndpoint, content);

        // Read the response content before calling EnsureSuccessStatusCode
        // so we can log detailed error information
        var responseContent = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError(
                "Google OAuth token exchange failed. Status={StatusCode}, Response={Response}",
                response.StatusCode, responseContent);
            response.EnsureSuccessStatusCode(); // This will throw with the status code
        }

        // Use dedicated options for token deserialization - no naming policy needed
        // since GoogleTokenResponse has explicit JsonPropertyName attributes
        var tokenOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        var tokenResponse = JsonSerializer.Deserialize<GoogleTokenResponse>(responseContent, tokenOptions);

        if (tokenResponse is null)
        {
            _logger.LogError("Failed to parse Google token response: {Response}", responseContent);
            throw new InvalidOperationException("Failed to parse token response");
        }

        // Validate the access token is present
        if (string.IsNullOrEmpty(tokenResponse.AccessToken))
        {
            _logger.LogError(
                "Google OAuth returned empty access token. HasRefreshToken={HasRefresh}, ExpiresIn={ExpiresIn}, Response={Response}",
                !string.IsNullOrEmpty(tokenResponse.RefreshToken),
                tokenResponse.ExpiresIn,
                responseContent);
            throw new InvalidOperationException("Google OAuth returned an empty access token");
        }

        _logger.LogDebug(
            "Google OAuth token exchange successful. HasRefreshToken={HasRefresh}, ExpiresIn={ExpiresIn}, Scope={Scope}",
            !string.IsNullOrEmpty(tokenResponse.RefreshToken),
            tokenResponse.ExpiresIn,
            tokenResponse.Scope);

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
            ["grant_type"] = "refresh_token"
        });

        var response = await _httpClient.PostAsync(TokenEndpoint, content);
        var responseContent = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError(
                "Google OAuth token refresh failed. Status={StatusCode}, Response={Response}",
                response.StatusCode, responseContent);
            response.EnsureSuccessStatusCode();
        }

        var tokenOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        var tokenResponse = JsonSerializer.Deserialize<GoogleTokenResponse>(responseContent, tokenOptions);

        if (tokenResponse is null)
        {
            _logger.LogError("Failed to parse Google token refresh response: {Response}", responseContent);
            throw new InvalidOperationException("Failed to parse token response");
        }

        if (string.IsNullOrEmpty(tokenResponse.AccessToken))
        {
            _logger.LogError(
                "Google OAuth token refresh returned empty access token. Response={Response}",
                responseContent);
            throw new InvalidOperationException("Google OAuth returned an empty access token");
        }

        // Google may or may not return a new refresh token
        return OAuthTokens.Create(
            tokenResponse.AccessToken,
            tokenResponse.RefreshToken ?? tokens.RefreshToken,
            tokenResponse.ExpiresIn,
            tokenResponse.Scope);
    }

    public async Task RevokeTokensAsync(OAuthTokens tokens)
    {
        var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["token"] = tokens.AccessToken
        });

        try
        {
            await _httpClient.PostAsync(RevokeEndpoint, content);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to revoke Google tokens");
        }
    }

    public async Task<string> GetAccountEmailAsync(OAuthTokens tokens)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "https://www.googleapis.com/oauth2/v2/userinfo");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokens.AccessToken);

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<GoogleUserInfo>(JsonOptions);
        return result?.Email ?? throw new InvalidOperationException("Failed to get account email");
    }

    public async Task<IReadOnlyList<ExternalCalendarInfo>> GetCalendarsAsync(OAuthTokens tokens)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, $"{CalendarApiBaseUrl}/users/me/calendarList");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokens.AccessToken);

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<GoogleCalendarListResponse>(JsonOptions);
        if (result?.Items is null)
            return [];

        return result.Items.Select(c => new ExternalCalendarInfo(
            c.Id,
            c.Summary ?? c.Id,
            c.Description,
            c.BackgroundColor,
            c.AccessRole is "reader" or "freeBusyReader",
            c.Primary ?? false,
            c.TimeZone
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
        string? pageToken = null;
        string? newSyncToken = null;
        bool fullSyncRequired = false;

        do
        {
            var url = BuildEventsUrl(calendarId, startDate, endDate, syncToken, pageToken);

            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokens.AccessToken);

            var response = await _httpClient.SendAsync(request);

            // Handle 410 Gone - sync token is invalid, need full sync
            if (response.StatusCode == System.Net.HttpStatusCode.Gone)
            {
                fullSyncRequired = true;
                syncToken = null;
                pageToken = null;
                events.Clear();
                deletedIds.Clear();
                continue;
            }

            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<GoogleEventsResponse>(JsonOptions);
            if (result is null)
                break;

            if (result.Items is not null)
            {
                foreach (var item in result.Items)
                {
                    if (item.Status == "cancelled")
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

            pageToken = result.NextPageToken;
            newSyncToken = result.NextSyncToken;

        } while (!string.IsNullOrEmpty(pageToken));

        return new CalendarSyncResult(events, deletedIds, newSyncToken, null, fullSyncRequired);
    }

    public Task<CalendarSyncResult> FetchIcsEventsAsync(string icsUrl, string? etag = null)
    {
        throw new NotSupportedException("Google Calendar provider does not support ICS URLs");
    }

    public async Task<string> CreateEventAsync(OAuthTokens tokens, string calendarId, ExternalCalendarEvent calendarEvent)
    {
        var googleEvent = MapToGoogleEvent(calendarEvent);
        var encodedCalendarId = Uri.EscapeDataString(calendarId);

        using var request = new HttpRequestMessage(HttpMethod.Post, $"{CalendarApiBaseUrl}/calendars/{encodedCalendarId}/events");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokens.AccessToken);
        request.Content = JsonContent.Create(googleEvent, options: JsonOptions);

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<GoogleEvent>(JsonOptions);
        return result?.Id ?? throw new InvalidOperationException("Failed to create event");
    }

    public async Task UpdateEventAsync(OAuthTokens tokens, string calendarId, string eventId, ExternalCalendarEvent calendarEvent)
    {
        var googleEvent = MapToGoogleEvent(calendarEvent);
        var encodedCalendarId = Uri.EscapeDataString(calendarId);
        var encodedEventId = Uri.EscapeDataString(eventId);

        using var request = new HttpRequestMessage(HttpMethod.Put, $"{CalendarApiBaseUrl}/calendars/{encodedCalendarId}/events/{encodedEventId}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokens.AccessToken);
        request.Content = JsonContent.Create(googleEvent, options: JsonOptions);

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteEventAsync(OAuthTokens tokens, string calendarId, string eventId)
    {
        var encodedCalendarId = Uri.EscapeDataString(calendarId);
        var encodedEventId = Uri.EscapeDataString(eventId);

        using var request = new HttpRequestMessage(HttpMethod.Delete, $"{CalendarApiBaseUrl}/calendars/{encodedCalendarId}/events/{encodedEventId}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokens.AccessToken);

        var response = await _httpClient.SendAsync(request);
        // 410 Gone is acceptable (already deleted)
        if (response.StatusCode != System.Net.HttpStatusCode.Gone)
            response.EnsureSuccessStatusCode();
    }

    private string BuildEventsUrl(string calendarId, DateTime startDate, DateTime endDate, string? syncToken, string? pageToken)
    {
        var encodedCalendarId = Uri.EscapeDataString(calendarId);
        var baseUrl = $"{CalendarApiBaseUrl}/calendars/{encodedCalendarId}/events";

        var queryParams = HttpUtility.ParseQueryString(string.Empty);
        queryParams["maxResults"] = "250";
        queryParams["singleEvents"] = "false"; // Get recurring events as a series

        if (!string.IsNullOrEmpty(syncToken))
        {
            queryParams["syncToken"] = syncToken;
        }
        else
        {
            queryParams["timeMin"] = startDate.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ");
            queryParams["timeMax"] = endDate.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ");
        }

        if (!string.IsNullOrEmpty(pageToken))
        {
            queryParams["pageToken"] = pageToken;
        }

        return $"{baseUrl}?{queryParams}";
    }

    private static ExternalCalendarEvent? MapToExternalEvent(GoogleEvent item)
    {
        DateTime startTime, endTime;
        bool isAllDay = false;

        if (item.Start?.DateTime is not null)
        {
            startTime = item.Start.DateTime.Value;
            endTime = item.End?.DateTime ?? startTime.AddHours(1);
        }
        else if (item.Start?.Date is not null)
        {
            isAllDay = true;
            startTime = DateTime.Parse(item.Start.Date);
            endTime = item.End?.Date is not null ? DateTime.Parse(item.End.Date) : startTime.AddDays(1);
        }
        else
        {
            return null;
        }

        var reminders = new List<int>();
        if (item.Reminders?.Overrides is not null)
        {
            reminders = item.Reminders.Overrides
                .Where(r => r.Method == "popup")
                .Select(r => r.Minutes)
                .ToList();
        }

        return new ExternalCalendarEvent
        {
            ExternalId = item.Id,
            Title = item.Summary ?? "(No title)",
            Description = item.Description,
            StartTime = startTime,
            EndTime = endTime,
            IsAllDay = isAllDay,
            Location = item.Location,
            Color = item.ColorId is not null ? GoogleColorIdToHex(item.ColorId) : null,
            RecurrenceRule = item.Recurrence?.FirstOrDefault(r => r.StartsWith("RRULE:")),
            RecurringEventId = item.RecurringEventId,
            OriginalStartTime = item.OriginalStartTime?.DateTime,
            IsCancelled = item.Status == "cancelled",
            Reminders = reminders,
            UpdatedAt = item.Updated,
            OrganizerEmail = item.Organizer?.Email
        };
    }

    private static object MapToGoogleEvent(ExternalCalendarEvent calendarEvent)
    {
        object start, end;

        if (calendarEvent.IsAllDay)
        {
            start = new { date = calendarEvent.StartTime.ToString("yyyy-MM-dd") };
            end = new { date = calendarEvent.EndTime.ToString("yyyy-MM-dd") };
        }
        else
        {
            start = new { dateTime = calendarEvent.StartTime.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ") };
            end = new { dateTime = calendarEvent.EndTime.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ") };
        }

        return new
        {
            summary = calendarEvent.Title,
            description = calendarEvent.Description,
            location = calendarEvent.Location,
            start,
            end
        };
    }

    private static string? GoogleColorIdToHex(string colorId)
    {
        // Google Calendar color IDs mapped to hex values
        return colorId switch
        {
            "1" => "#7986CB", // Lavender
            "2" => "#33B679", // Sage
            "3" => "#8E24AA", // Grape
            "4" => "#E67C73", // Flamingo
            "5" => "#F6BF26", // Banana
            "6" => "#F4511E", // Tangerine
            "7" => "#039BE5", // Peacock
            "8" => "#616161", // Graphite
            "9" => "#3F51B5", // Blueberry
            "10" => "#0B8043", // Basil
            "11" => "#D50000", // Tomato
            _ => null
        };
    }

    #region Google API Response Models

    private sealed record GoogleTokenResponse
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

    private sealed record GoogleCalendarListResponse
    {
        public List<GoogleCalendarListEntry>? Items { get; init; }
    }

    private sealed record GoogleCalendarListEntry
    {
        public string Id { get; init; } = string.Empty;
        public string? Summary { get; init; }
        public string? Description { get; init; }
        public string? BackgroundColor { get; init; }
        public string? AccessRole { get; init; }
        public bool? Primary { get; init; }
        public string? TimeZone { get; init; }
    }

    private sealed record GoogleEventsResponse
    {
        public List<GoogleEvent>? Items { get; init; }
        public string? NextPageToken { get; init; }
        public string? NextSyncToken { get; init; }
    }

    private sealed record GoogleEvent
    {
        public string Id { get; init; } = string.Empty;
        public string? Status { get; init; }
        public string? Summary { get; init; }
        public string? Description { get; init; }
        public string? Location { get; init; }
        public string? ColorId { get; init; }
        public GoogleDateTime? Start { get; init; }
        public GoogleDateTime? End { get; init; }
        public List<string>? Recurrence { get; init; }
        public string? RecurringEventId { get; init; }
        public GoogleDateTime? OriginalStartTime { get; init; }
        public GoogleReminders? Reminders { get; init; }
        public DateTime? Updated { get; init; }
        public GoogleOrganizer? Organizer { get; init; }
    }

    private sealed record GoogleDateTime
    {
        public DateTime? DateTime { get; init; }
        public string? Date { get; init; }
        public string? TimeZone { get; init; }
    }

    private sealed record GoogleReminders
    {
        public bool? UseDefault { get; init; }
        public List<GoogleReminder>? Overrides { get; init; }
    }

    private sealed record GoogleReminder
    {
        public string Method { get; init; } = string.Empty;
        public int Minutes { get; init; }
    }

    private sealed record GoogleOrganizer
    {
        public string? Email { get; init; }
        public string? DisplayName { get; init; }
    }

    private sealed record GoogleUserInfo
    {
        public string? Email { get; init; }
    }

    #endregion
}
