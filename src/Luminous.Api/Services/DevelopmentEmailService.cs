using Luminous.Application.Common.Interfaces;

namespace Luminous.Api.Services;

/// <summary>
/// Development email service that logs emails instead of sending them.
/// In development, OTPs are logged to the console for easy testing.
/// </summary>
public class DevelopmentEmailService : IEmailService
{
    private readonly ILogger<DevelopmentEmailService> _logger;

    public DevelopmentEmailService(ILogger<DevelopmentEmailService> logger)
    {
        _logger = logger;
    }

    public Task SendOtpAsync(string email, string code, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "📧 [DEV EMAIL] OTP Code for {Email}: {Code}",
            email,
            code);

        _logger.LogInformation(
            """
            ═══════════════════════════════════════════════════════════════
            Development Email - OTP Authentication
            ═══════════════════════════════════════════════════════════════
            To: {Email}
            Subject: Your Luminous Login Code

            Your one-time password is: {Code}

            This code expires in 10 minutes.
            ═══════════════════════════════════════════════════════════════
            """,
            email,
            code);

        return Task.CompletedTask;
    }

    public Task SendInvitationAsync(
        string email,
        string familyName,
        string inviterName,
        string invitationCode,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            """
            ═══════════════════════════════════════════════════════════════
            Development Email - Family Invitation
            ═══════════════════════════════════════════════════════════════
            To: {Email}
            Subject: You've been invited to join {FamilyName} on Luminous

            {InviterName} has invited you to join their family on Luminous!

            Use this invitation code to join: {InvitationCode}

            This invitation expires in 7 days.
            ═══════════════════════════════════════════════════════════════
            """,
            email,
            familyName,
            inviterName,
            invitationCode);

        return Task.CompletedTask;
    }

    public Task SendWelcomeAsync(
        string email,
        string displayName,
        string familyName,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            """
            ═══════════════════════════════════════════════════════════════
            Development Email - Welcome to Luminous
            ═══════════════════════════════════════════════════════════════
            To: {Email}
            Subject: Welcome to {FamilyName} on Luminous!

            Hi {DisplayName}!

            Welcome to Luminous! You've successfully joined {FamilyName}.

            Get started by:
            1. Setting up your profile
            2. Adding your first calendar
            3. Creating chores and routines

            Happy organizing!
            ═══════════════════════════════════════════════════════════════
            """,
            email,
            familyName,
            displayName);

        return Task.CompletedTask;
    }
}
