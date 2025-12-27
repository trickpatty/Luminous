using System.Reflection;
using System.Text.RegularExpressions;
using HandlebarsDotNet;
using Luminous.Api.Configuration;
using Microsoft.Extensions.Options;

namespace Luminous.Api.Services;

/// <summary>
/// Service for rendering email templates using Handlebars.
/// </summary>
public interface IEmailTemplateService
{
    /// <summary>
    /// Renders an email template with the provided data.
    /// </summary>
    /// <param name="templateName">The name of the template (without extension).</param>
    /// <param name="data">The data to bind to the template.</param>
    /// <returns>The rendered HTML email content.</returns>
    string RenderTemplate(string templateName, object data);

    /// <summary>
    /// Renders the OTP email template.
    /// </summary>
    string RenderOtpEmail(string email, string code, int expiresInMinutes);

    /// <summary>
    /// Renders the invitation email template.
    /// </summary>
    string RenderInvitationEmail(
        string email,
        string inviterName,
        string familyName,
        string invitationCode,
        string? message,
        DateTime expiresAt);

    /// <summary>
    /// Renders the welcome email template.
    /// </summary>
    string RenderWelcomeEmail(string email, string displayName, string familyName);

    /// <summary>
    /// Converts HTML content to plain text by stripping tags.
    /// </summary>
    string ConvertToPlainText(string htmlContent);

    /// <summary>
    /// Renders the OTP email as plain text.
    /// </summary>
    string RenderOtpEmailPlainText(string email, string code, int expiresInMinutes);

    /// <summary>
    /// Renders the invitation email as plain text.
    /// </summary>
    string RenderInvitationEmailPlainText(
        string email,
        string inviterName,
        string familyName,
        string invitationCode,
        string? message,
        DateTime expiresAt);

    /// <summary>
    /// Renders the welcome email as plain text.
    /// </summary>
    string RenderWelcomeEmailPlainText(string email, string displayName, string familyName);
}

/// <summary>
/// Implementation of email template service using Handlebars.
/// </summary>
public sealed class EmailTemplateService : IEmailTemplateService
{
    private readonly IHandlebars _handlebars;
    private readonly EmailSettings _settings;
    private readonly Dictionary<string, HandlebarsTemplate<object, object>> _compiledTemplates = new();
    private readonly HandlebarsTemplate<object, object>? _baseTemplate;

    public EmailTemplateService(IOptions<EmailSettings> settings)
    {
        _settings = settings.Value;
        _handlebars = Handlebars.Create();

        // Load and compile the base template
        var baseContent = LoadEmbeddedTemplate("base");
        if (!string.IsNullOrEmpty(baseContent))
        {
            _baseTemplate = _handlebars.Compile(baseContent);
        }
    }

    public string RenderTemplate(string templateName, object data)
    {
        var template = GetCompiledTemplate(templateName);
        var content = template(data);

        // If we have a base template, wrap the content
        if (_baseTemplate != null)
        {
            var wrappedData = new Dictionary<string, object>
            {
                ["content"] = content,
                ["subject"] = templateName,
                ["year"] = DateTime.UtcNow.Year,
                ["email"] = GetEmailFromData(data)
            };

            return _baseTemplate(wrappedData);
        }

        return content;
    }

    public string RenderOtpEmail(string email, string code, int expiresInMinutes)
    {
        return RenderTemplate("otp", new
        {
            email,
            code,
            expiresInMinutes,
            year = DateTime.UtcNow.Year
        });
    }

    public string RenderInvitationEmail(
        string email,
        string inviterName,
        string familyName,
        string invitationCode,
        string? message,
        DateTime expiresAt)
    {
        var acceptUrl = $"{_settings.BaseUrl}/invite/{invitationCode}";

        return RenderTemplate("invitation", new
        {
            email,
            inviterName,
            familyName,
            invitationCode,
            message,
            expiresAt = expiresAt.ToString("MMMM d, yyyy 'at' h:mm tt 'UTC'"),
            acceptUrl,
            year = DateTime.UtcNow.Year
        });
    }

    public string RenderWelcomeEmail(string email, string displayName, string familyName)
    {
        var dashboardUrl = $"{_settings.BaseUrl}/dashboard";

        return RenderTemplate("welcome", new
        {
            email,
            displayName,
            familyName,
            dashboardUrl,
            helpUrl = _settings.HelpUrl,
            year = DateTime.UtcNow.Year
        });
    }

    private HandlebarsTemplate<object, object> GetCompiledTemplate(string templateName)
    {
        if (_compiledTemplates.TryGetValue(templateName, out var cached))
        {
            return cached;
        }

        var content = LoadEmbeddedTemplate(templateName);
        if (string.IsNullOrEmpty(content))
        {
            throw new FileNotFoundException($"Email template '{templateName}' not found.");
        }

        var compiled = _handlebars.Compile(content);
        _compiledTemplates[templateName] = compiled;
        return compiled;
    }

    private static string LoadEmbeddedTemplate(string templateName)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = $"Luminous.Api.Templates.Email.{templateName}.hbs";

        using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream == null)
        {
            return string.Empty;
        }

        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    private static string GetEmailFromData(object data)
    {
        var type = data.GetType();
        var emailProperty = type.GetProperty("email") ?? type.GetProperty("Email");
        return emailProperty?.GetValue(data)?.ToString() ?? string.Empty;
    }

    public string ConvertToPlainText(string htmlContent)
    {
        if (string.IsNullOrEmpty(htmlContent))
            return string.Empty;

        // Remove script and style blocks entirely
        var text = Regex.Replace(htmlContent, @"<script[^>]*>[\s\S]*?</script>", "", RegexOptions.IgnoreCase);
        text = Regex.Replace(text, @"<style[^>]*>[\s\S]*?</style>", "", RegexOptions.IgnoreCase);

        // Replace common block elements with newlines
        text = Regex.Replace(text, @"<br\s*/?>", "\n", RegexOptions.IgnoreCase);
        text = Regex.Replace(text, @"</p>", "\n\n", RegexOptions.IgnoreCase);
        text = Regex.Replace(text, @"</div>", "\n", RegexOptions.IgnoreCase);
        text = Regex.Replace(text, @"</tr>", "\n", RegexOptions.IgnoreCase);
        text = Regex.Replace(text, @"</li>", "\n", RegexOptions.IgnoreCase);
        text = Regex.Replace(text, @"<h[1-6][^>]*>", "\n", RegexOptions.IgnoreCase);
        text = Regex.Replace(text, @"</h[1-6]>", "\n\n", RegexOptions.IgnoreCase);

        // Extract href from links and append
        text = Regex.Replace(text, @"<a[^>]*href\s*=\s*[""']([^""']+)[""'][^>]*>([^<]+)</a>",
            "$2 ($1)", RegexOptions.IgnoreCase);

        // Remove all remaining HTML tags
        text = Regex.Replace(text, @"<[^>]+>", "");

        // Decode HTML entities
        text = System.Net.WebUtility.HtmlDecode(text);

        // Normalize whitespace
        text = Regex.Replace(text, @"[ \t]+", " ");
        text = Regex.Replace(text, @"\n{3,}", "\n\n");
        text = text.Trim();

        return text;
    }

    public string RenderOtpEmailPlainText(string email, string code, int expiresInMinutes)
    {
        return $"""
            Your Luminous Login Code

            Your one-time password is: {code}

            This code expires in {expiresInMinutes} minutes.

            If you didn't request this code, you can safely ignore this email.

            ---
            Luminous - Your Family Command Center
            """;
    }

    public string RenderInvitationEmailPlainText(
        string email,
        string inviterName,
        string familyName,
        string invitationCode,
        string? message,
        DateTime expiresAt)
    {
        var acceptUrl = $"{_settings.BaseUrl}/invite/{invitationCode}";
        var messageSection = !string.IsNullOrEmpty(message) ? $"\n\nMessage from {inviterName}:\n\"{message}\"\n" : "";

        return $"""
            You're Invited!

            {inviterName} has invited you to join their family "{familyName}" on Luminous.
            {messageSection}
            Use this invitation code to join: {invitationCode}

            Or visit: {acceptUrl}

            This invitation expires on {expiresAt:MMMM d, yyyy 'at' h:mm tt 'UTC'}.

            ---
            Luminous - Your Family Command Center
            """;
    }

    public string RenderWelcomeEmailPlainText(string email, string displayName, string familyName)
    {
        var dashboardUrl = $"{_settings.BaseUrl}/dashboard";

        return $"""
            Welcome to {familyName}!

            Hi {displayName}!

            Welcome to Luminous! You've successfully joined {familyName}.

            Get started by visiting your dashboard: {dashboardUrl}

            Here's what you can do:
            - View your family calendar
            - Manage chores and routines
            - Connect with family members

            Need help? Visit: {_settings.HelpUrl}

            Happy organizing!

            ---
            Luminous - Your Family Command Center
            """;
    }
}
