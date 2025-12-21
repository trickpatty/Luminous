namespace Luminous.Domain.Enums;

/// <summary>
/// Defines the status of a family invitation.
/// </summary>
public enum InvitationStatus
{
    /// <summary>
    /// Invitation is pending and awaiting response.
    /// </summary>
    Pending = 0,

    /// <summary>
    /// Invitation has been accepted.
    /// </summary>
    Accepted = 1,

    /// <summary>
    /// Invitation has been declined.
    /// </summary>
    Declined = 2,

    /// <summary>
    /// Invitation has expired.
    /// </summary>
    Expired = 3,

    /// <summary>
    /// Invitation was revoked by the inviter.
    /// </summary>
    Revoked = 4
}
