using System.Text;
using Fido2NetLib;
using Fido2NetLib.Objects;
using Luminous.Application.Common.Interfaces;
using Luminous.Domain.Interfaces;
using Microsoft.Extensions.Caching.Distributed;

namespace Luminous.Api.Services;

/// <summary>
/// WebAuthn/FIDO2 service for passkey operations.
/// </summary>
public class WebAuthnService : IWebAuthnService
{
    private readonly IFido2 _fido2;
    private readonly IDistributedCache _cache;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<WebAuthnService> _logger;

    private static readonly TimeSpan SessionTimeout = TimeSpan.FromMinutes(5);

    public WebAuthnService(
        IFido2 fido2,
        IDistributedCache cache,
        IUnitOfWork unitOfWork,
        ILogger<WebAuthnService> logger)
    {
        _fido2 = fido2;
        _cache = cache;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<(CredentialCreateOptions Options, string SessionId)> CreateRegistrationOptionsAsync(
        string userId,
        string email,
        string displayName,
        IEnumerable<byte[]>? existingCredentialIds = null,
        CancellationToken cancellationToken = default)
    {
        var user = new Fido2User
        {
            Id = Encoding.UTF8.GetBytes(userId),
            Name = email,
            DisplayName = displayName
        };

        var excludeCredentials = existingCredentialIds?
            .Select(id => new PublicKeyCredentialDescriptor(id))
            .ToList() ?? [];

        var authenticatorSelection = new AuthenticatorSelection
        {
            ResidentKey = ResidentKeyRequirement.Required,
            UserVerification = UserVerificationRequirement.Preferred
        };

        var options = _fido2.RequestNewCredential(
            user,
            excludeCredentials,
            authenticatorSelection,
            AttestationConveyancePreference.None);

        // Store the options in cache for verification later
        var sessionId = Nanoid.Nanoid.Generate(size: 21);
        var sessionData = new WebAuthnSession
        {
            UserId = userId,
            OptionsJson = options.ToJson()
        };

        await _cache.SetStringAsync(
            $"webauthn:register:{sessionId}",
            System.Text.Json.JsonSerializer.Serialize(sessionData),
            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = SessionTimeout },
            cancellationToken);

        _logger.LogDebug(
            "Created WebAuthn registration options for user {UserId}, session {SessionId}",
            userId,
            sessionId);

        return (options, sessionId);
    }

    public async Task<RegisteredCredentialResult> CompleteRegistrationAsync(
        string sessionId,
        AuthenticatorAttestationRawResponse attestationResponse,
        CancellationToken cancellationToken = default)
    {
        // Retrieve session data
        var sessionJson = await _cache.GetStringAsync(
            $"webauthn:register:{sessionId}",
            cancellationToken);

        if (string.IsNullOrEmpty(sessionJson))
        {
            _logger.LogWarning("WebAuthn registration session not found: {SessionId}", sessionId);
            return new RegisteredCredentialResult
            {
                Success = false,
                Error = "Registration session expired or not found."
            };
        }

        var session = System.Text.Json.JsonSerializer.Deserialize<WebAuthnSession>(sessionJson);
        if (session == null)
        {
            return new RegisteredCredentialResult
            {
                Success = false,
                Error = "Invalid session data."
            };
        }

        var options = CredentialCreateOptions.FromJson(session.OptionsJson);

        try
        {
            // Verify the attestation response
            var result = await _fido2.MakeNewCredentialAsync(
                attestationResponse,
                options,
                async (args, ct) =>
                {
                    // Check if credential ID already exists
                    return !await _unitOfWork.Credentials.CredentialIdExistsAsync(args.CredentialId, ct);
                },
                cancellationToken);

            if (result.Result == null)
            {
                return new RegisteredCredentialResult
                {
                    Success = false,
                    Error = result.ErrorMessage ?? "Registration verification failed."
                };
            }

            // Remove the session from cache
            await _cache.RemoveAsync($"webauthn:register:{sessionId}", cancellationToken);

            _logger.LogInformation(
                "WebAuthn credential registered for user {UserId}",
                session.UserId);

            return new RegisteredCredentialResult
            {
                Success = true,
                CredentialId = result.Result.Id,
                PublicKey = result.Result.PublicKey,
                UserHandle = result.Result.User.Id,
                SignatureCounter = result.Result.Counter,
                AaGuid = result.Result.AaGuid,
                CredentialType = result.Result.Type.ToString(),
                UserId = session.UserId
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "WebAuthn registration failed for session {SessionId}", sessionId);
            return new RegisteredCredentialResult
            {
                Success = false,
                Error = "Registration verification failed."
            };
        }
    }

    public async Task<(AssertionOptions Options, string SessionId)> CreateAuthenticationOptionsAsync(
        string? email = null,
        CancellationToken cancellationToken = default)
    {
        List<PublicKeyCredentialDescriptor>? allowedCredentials = null;
        string? userId = null;

        if (!string.IsNullOrEmpty(email))
        {
            // Get credentials for specific user
            var user = await _unitOfWork.Users.GetByEmailAsync(email, cancellationToken);
            if (user != null)
            {
                userId = user.Id;
                var credentials = await _unitOfWork.Credentials.GetByUserIdAsync(user.Id, cancellationToken);
                allowedCredentials = credentials
                    .Where(c => c.IsActive)
                    .Select(c => new PublicKeyCredentialDescriptor(c.CredentialId))
                    .ToList();
            }
        }

        var options = _fido2.GetAssertionOptions(
            allowedCredentials ?? [],
            UserVerificationRequirement.Preferred);

        // Store options in cache
        var sessionId = Nanoid.Nanoid.Generate(size: 21);
        var sessionData = new WebAuthnSession
        {
            UserId = userId,
            OptionsJson = options.ToJson()
        };

        await _cache.SetStringAsync(
            $"webauthn:auth:{sessionId}",
            System.Text.Json.JsonSerializer.Serialize(sessionData),
            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = SessionTimeout },
            cancellationToken);

        _logger.LogDebug(
            "Created WebAuthn authentication options{Email}, session {SessionId}",
            string.IsNullOrEmpty(email) ? "" : $" for {email}",
            sessionId);

        return (options, sessionId);
    }

    public async Task<AuthenticationResult> CompleteAuthenticationAsync(
        string sessionId,
        AuthenticatorAssertionRawResponse assertionResponse,
        CancellationToken cancellationToken = default)
    {
        // Retrieve session data
        var sessionJson = await _cache.GetStringAsync(
            $"webauthn:auth:{sessionId}",
            cancellationToken);

        if (string.IsNullOrEmpty(sessionJson))
        {
            _logger.LogWarning("WebAuthn authentication session not found: {SessionId}", sessionId);
            return new AuthenticationResult
            {
                Success = false,
                Error = "Authentication session expired or not found."
            };
        }

        var session = System.Text.Json.JsonSerializer.Deserialize<WebAuthnSession>(sessionJson);
        if (session == null)
        {
            return new AuthenticationResult
            {
                Success = false,
                Error = "Invalid session data."
            };
        }

        var options = AssertionOptions.FromJson(session.OptionsJson);

        // Find the credential
        var credential = await _unitOfWork.Credentials.GetByCredentialIdAsync(
            assertionResponse.Id,
            cancellationToken);

        if (credential == null || !credential.IsActive)
        {
            _logger.LogWarning("Credential not found or inactive for authentication");
            return new AuthenticationResult
            {
                Success = false,
                Error = "Credential not found or inactive."
            };
        }

        try
        {
            // Verify the assertion
            var result = await _fido2.MakeAssertionAsync(
                assertionResponse,
                options,
                credential.PublicKey,
                [],
                credential.SignatureCounter,
                async (args, ct) =>
                {
                    // Verify user handle if present
                    if (args.UserHandle != null && args.UserHandle.Length > 0)
                    {
                        var expectedUserId = Encoding.UTF8.GetString(args.UserHandle);
                        return credential.UserId == expectedUserId;
                    }
                    return true;
                },
                cancellationToken);

            if (result.Status != "ok")
            {
                return new AuthenticationResult
                {
                    Success = false,
                    Error = result.ErrorMessage ?? "Authentication verification failed."
                };
            }

            // Remove the session from cache
            await _cache.RemoveAsync($"webauthn:auth:{sessionId}", cancellationToken);

            _logger.LogInformation(
                "WebAuthn authentication successful for user {UserId}",
                credential.UserId);

            return new AuthenticationResult
            {
                Success = true,
                UserId = credential.UserId,
                CredentialId = credential.CredentialId,
                SignatureCounter = result.Counter
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "WebAuthn authentication failed for session {SessionId}", sessionId);
            return new AuthenticationResult
            {
                Success = false,
                Error = "Authentication verification failed."
            };
        }
    }

    private sealed record WebAuthnSession
    {
        public string? UserId { get; init; }
        public string OptionsJson { get; init; } = string.Empty;
    }
}
