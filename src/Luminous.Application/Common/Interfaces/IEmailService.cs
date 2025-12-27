namespace Luminous.Application.Common.Interfaces;

/// <summary>
/// Service for sending emails.
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Sends an OTP code to the specified email address.
    /// </summary>
    /// <param name="email">The recipient email address.</param>
    /// <param name="code">The OTP code to send.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task SendOtpAsync(string email, string code, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a family invitation email.
    /// </summary>
    /// <param name="email">The recipient email address.</param>
    /// <param name="familyName">The name of the family.</param>
    /// <param name="inviterName">The name of the person inviting.</param>
    /// <param name="invitationCode">The invitation code.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task SendInvitationAsync(
        string email,
        string familyName,
        string inviterName,
        string invitationCode,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a welcome email to a new user.
    /// </summary>
    /// <param name="email">The recipient email address.</param>
    /// <param name="displayName">The user's display name.</param>
    /// <param name="familyName">The name of the family.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task SendWelcomeAsync(
        string email,
        string displayName,
        string familyName,
        CancellationToken cancellationToken = default);
}
