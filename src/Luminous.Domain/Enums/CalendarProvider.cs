namespace Luminous.Domain.Enums;

/// <summary>
/// Defines the supported calendar providers for integration.
/// </summary>
public enum CalendarProvider
{
    /// <summary>
    /// Google Calendar integration.
    /// </summary>
    Google = 0,

    /// <summary>
    /// Microsoft Outlook/Office 365 integration.
    /// </summary>
    Outlook = 1,

    /// <summary>
    /// Apple iCloud Calendar integration.
    /// </summary>
    ICloud = 2,

    /// <summary>
    /// Generic CalDAV server integration.
    /// </summary>
    CalDav = 3,

    /// <summary>
    /// ICS URL subscription (read-only).
    /// </summary>
    IcsUrl = 4,

    /// <summary>
    /// Internal Luminous calendar.
    /// </summary>
    Internal = 5
}
