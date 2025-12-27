using System.Reflection;
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
}
