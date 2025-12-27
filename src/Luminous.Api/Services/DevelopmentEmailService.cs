using Luminous.Application.Common.Interfaces;

namespace Luminous.Api.Services;

/// <summary>
/// Development email service that logs emails to the console instead of sending them.
/// Uses the same Handlebars templates as production for consistent email content.
/// </summary>
public class DevelopmentEmailService : IEmailService
{
    private readonly IEmailTemplateService _templateService;
    private readonly ILogger<DevelopmentEmailService> _logger;

    public DevelopmentEmailService(
        IEmailTemplateService templateService,
        ILogger<DevelopmentEmailService> logger)
    {
        _templateService = templateService;
        _logger = logger;
    }

    public Task SendOtpAsync(string email, string code, CancellationToken cancellationToken = default)
    {
        var htmlContent = _templateService.RenderOtpEmail(email, code, 10);

        _logger.LogInformation(
            "📧 [DEV EMAIL] OTP Code for {Email}: {Code}",
            email,
            code);

        LogEmailContent("Your Luminous Login Code", email, htmlContent);

        return Task.CompletedTask;
    }

    public Task SendInvitationAsync(
        string email,
        string familyName,
        string inviterName,
        string invitationCode,
        CancellationToken cancellationToken = default)
    {
        var htmlContent = _templateService.RenderInvitationEmail(
            email,
            inviterName,
            familyName,
            invitationCode,
            null,
            DateTime.UtcNow.AddDays(7));

        _logger.LogInformation(
            "📧 [DEV EMAIL] Invitation sent to {Email} for family {FamilyName}",
            email,
            familyName);

        LogEmailContent($"You've been invited to join {familyName} on Luminous", email, htmlContent);

        return Task.CompletedTask;
    }

    public Task SendWelcomeAsync(
        string email,
        string displayName,
        string familyName,
        CancellationToken cancellationToken = default)
    {
        var htmlContent = _templateService.RenderWelcomeEmail(email, displayName, familyName);

        _logger.LogInformation(
            "📧 [DEV EMAIL] Welcome email sent to {Email}",
            email);

        LogEmailContent($"Welcome to {familyName} on Luminous!", email, htmlContent);

        return Task.CompletedTask;
    }

    private void LogEmailContent(string subject, string toEmail, string htmlContent)
    {
        _logger.LogDebug(
            """
            ═══════════════════════════════════════════════════════════════
            Development Email
            ═══════════════════════════════════════════════════════════════
            To: {ToEmail}
            Subject: {Subject}

            [HTML Content - {Length} characters]
            ═══════════════════════════════════════════════════════════════
            """,
            toEmail,
            subject,
            htmlContent.Length);
    }
}
