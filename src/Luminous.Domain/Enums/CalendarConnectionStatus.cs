namespace Luminous.Domain.Enums;

/// <summary>
/// Defines the status of a calendar connection.
/// </summary>
public enum CalendarConnectionStatus
{
    /// <summary>
    /// Connection is pending initial authentication.
    /// </summary>
    PendingAuth = 0,

    /// <summary>
    /// Connection is active and syncing.
    /// </summary>
    Active = 1,

    /// <summary>
    /// Connection is paused by user.
    /// </summary>
    Paused = 2,

    /// <summary>
    /// Connection failed due to authentication error (needs re-auth).
    /// </summary>
    AuthError = 3,

    /// <summary>
    /// Connection failed due to sync error.
    /// </summary>
    SyncError = 4,

    /// <summary>
    /// Connection has been disconnected/revoked.
    /// </summary>
    Disconnected = 5
}
