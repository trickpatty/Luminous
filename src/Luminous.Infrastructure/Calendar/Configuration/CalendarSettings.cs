namespace Luminous.Infrastructure.Calendar.Configuration;

/// <summary>
/// Configuration settings for calendar integrations.
/// </summary>
public sealed class CalendarSettings
{
    /// <summary>
    /// Configuration section name.
    /// </summary>
    public const string SectionName = "Calendar";

    /// <summary>
    /// Google Calendar settings.
    /// </summary>
    public GoogleCalendarSettings Google { get; set; } = new();

    /// <summary>
    /// Microsoft/Outlook Calendar settings.
    /// </summary>
    public MicrosoftCalendarSettings Microsoft { get; set; } = new();

    /// <summary>
    /// ICS subscription settings.
    /// </summary>
    public IcsSettings Ics { get; set; } = new();

    /// <summary>
    /// Default OAuth redirect URI.
    /// </summary>
    public string DefaultRedirectUri { get; set; } = string.Empty;
}

/// <summary>
/// Google Calendar OAuth settings.
/// </summary>
public sealed class GoogleCalendarSettings
{
    /// <summary>
    /// Google OAuth client ID.
    /// </summary>
    public string ClientId { get; set; } = string.Empty;

    /// <summary>
    /// Google OAuth client secret.
    /// </summary>
    public string ClientSecret { get; set; } = string.Empty;

    /// <summary>
    /// OAuth scopes to request.
    /// </summary>
    public List<string> Scopes { get; set; } =
    [
        "https://www.googleapis.com/auth/calendar.readonly",
        "https://www.googleapis.com/auth/calendar.events.readonly"
    ];
}

/// <summary>
/// Microsoft Graph/Outlook Calendar OAuth settings.
/// </summary>
public sealed class MicrosoftCalendarSettings
{
    /// <summary>
    /// Microsoft OAuth client ID.
    /// </summary>
    public string ClientId { get; set; } = string.Empty;

    /// <summary>
    /// Microsoft OAuth client secret.
    /// </summary>
    public string ClientSecret { get; set; } = string.Empty;

    /// <summary>
    /// Azure AD tenant ID (use "common" for multi-tenant).
    /// </summary>
    public string TenantId { get; set; } = "common";

    /// <summary>
    /// OAuth scopes to request.
    /// </summary>
    public List<string> Scopes { get; set; } =
    [
        "offline_access",
        "Calendars.Read",
        "User.Read"
    ];
}

/// <summary>
/// ICS subscription settings.
/// </summary>
public sealed class IcsSettings
{
    /// <summary>
    /// HTTP request timeout in seconds.
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Maximum ICS file size in bytes (default 5MB).
    /// </summary>
    public int MaxFileSizeBytes { get; set; } = 5 * 1024 * 1024;

    /// <summary>
    /// User-Agent header for HTTP requests.
    /// </summary>
    public string UserAgent { get; set; } = "Luminous-Calendar/1.0";
}
