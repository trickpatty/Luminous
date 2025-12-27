using Azure;
using Azure.Communication.Email;
using Luminous.Api.Configuration;
using Luminous.Application.Common.Interfaces;
using Microsoft.Extensions.Options;

namespace Luminous.Api.Services;

/// <summary>
/// Email service implementation using Azure Communication Services.
/// </summary>
public sealed class AzureEmailService : IEmailService
{
    private readonly EmailClient _emailClient;
    private readonly EmailSettings _settings;
    private readonly IEmailTemplateService _templateService;
    private readonly ILogger<AzureEmailService> _logger;

    public AzureEmailService(
        IOptions<EmailSettings> settings,
        IEmailTemplateService templateService,
        ILogger<AzureEmailService> logger)
    {
        _settings = settings.Value;
        _templateService = templateService;
        _logger = logger;

        if (string.IsNullOrEmpty(_settings.ConnectionString))
        {
            throw new InvalidOperationException("Azure Communication Services connection string is not configured.");
        }

        _emailClient = new EmailClient(_settings.ConnectionString);
    }

    public async Task SendOtpAsync(string email, string code, CancellationToken cancellationToken = default)
    {
        var htmlContent = _templateService.RenderOtpEmail(email, code, 10);

        await SendEmailAsync(
            email,
            "Your Luminous Login Code",
            htmlContent,
            $"Your Luminous login code is: {code}. This code expires in 10 minutes.",
            cancellationToken);

        _logger.LogInformation("OTP email sent to {Email}", MaskEmail(email));
    }

    public async Task SendInvitationAsync(
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

        var plainText = $@"You're Invited!

{inviterName} has invited you to join their family {familyName} on Luminous.

Use this invitation code to join: {invitationCode}

Or visit: {_settings.BaseUrl}/invite/{invitationCode}

This invitation expires in 7 days.";

        await SendEmailAsync(
            email,
            $"You're invited to join {familyName} on Luminous",
            htmlContent,
            plainText,
            cancellationToken);

        _logger.LogInformation("Invitation email sent to {Email} for family {FamilyName}", MaskEmail(email), familyName);
    }

    public async Task SendWelcomeAsync(
        string email,
        string displayName,
        string familyName,
        CancellationToken cancellationToken = default)
    {
        var htmlContent = _templateService.RenderWelcomeEmail(email, displayName, familyName);

        var plainText = $@"Welcome to {familyName}!

Hi {displayName}!

Welcome to Luminous! You've successfully joined {familyName}.

Get started by visiting: {_settings.BaseUrl}/dashboard

Happy organizing!";

        await SendEmailAsync(
            email,
            $"Welcome to {familyName} on Luminous!",
            htmlContent,
            plainText,
            cancellationToken);

        _logger.LogInformation("Welcome email sent to {Email}", MaskEmail(email));
    }

    private async Task SendEmailAsync(
        string toEmail,
        string subject,
        string htmlContent,
        string plainTextContent,
        CancellationToken cancellationToken)
    {
        try
        {
            var emailMessage = new EmailMessage(
                senderAddress: _settings.SenderAddress,
                content: new EmailContent(subject)
                {
                    Html = htmlContent,
                    PlainText = plainTextContent
                },
                recipients: new EmailRecipients(new List<EmailAddress>
                {
                    new(toEmail)
                }));

            var operation = await _emailClient.SendAsync(
                WaitUntil.Started,
                emailMessage,
                cancellationToken);

            _logger.LogDebug(
                "Email send operation started. MessageId: {MessageId}",
                operation.Id);
        }
        catch (RequestFailedException ex)
        {
            _logger.LogError(ex,
                "Failed to send email to {Email}. Status: {Status}, ErrorCode: {ErrorCode}",
                MaskEmail(toEmail),
                ex.Status,
                ex.ErrorCode);
            throw;
        }
    }

    private static string MaskEmail(string email)
    {
        var atIndex = email.IndexOf('@');
        if (atIndex <= 1) return email;

        var prefix = email[..atIndex];
        var domain = email[atIndex..];

        if (prefix.Length <= 2)
        {
            return $"{prefix[0]}*{domain}";
        }

        return $"{prefix[..2]}***{domain}";
    }
}
