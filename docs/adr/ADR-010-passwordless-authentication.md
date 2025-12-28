# ADR-010: In-House Passwordless Authentication

> **Status:** Accepted
> **Date:** 2025-12-21
> **Deciders:** Luminous Core Team
> **Categories:** Architecture, Security

## Context

Luminous is a multi-tenant SaaS platform serving multiple families. We need an identity solution that supports:
- Consumer sign-up and sign-in
- Modern passwordless authentication (passkeys, hardware tokens)
- Social logins (Google, Apple, Microsoft)
- Multi-factor authentication
- Custom claims (family_id, roles)
- Device linking flow
- Multiple platforms (web, mobile, display)

## Decision Drivers

- **Passwordless-first**: Passkeys and WebAuthn are the future of authentication
- **Security**: Phishing-resistant authentication methods
- **User experience**: Frictionless sign-in with biometrics
- **Flexibility**: Full control over authentication flows
- **Platform support**: Cross-platform passkey support
- **Fallback options**: Email/password for users who need it
- **Cost control**: Avoid per-authentication pricing of external providers

## Considered Options

### Option 1: Azure AD B2C / Entra External ID

Microsoft's consumer identity platform.

**Pros:**
- Managed service
- Social providers built-in
- Compliant (SOC 2, GDPR)

**Cons:**
- Limited passkey support
- Complex custom policy XML
- Per-authentication pricing
- Vendor lock-in
- Limited customization of auth UI

### Option 2: Auth0 / Okta

Third-party identity platforms.

**Pros:**
- Developer-friendly
- Good passkey support
- Social connections

**Cons:**
- Expensive at scale
- External dependency
- Limited control over flows

### Option 3: In-House with Modern Standards (Selected)

Build authentication using established libraries and standards.

**Pros:**
- Full control over authentication experience
- Native passkey/WebAuthn support
- No per-authentication costs
- Customizable UI for family-friendly experience
- Can optimize for our specific use cases

**Cons:**
- Development effort required
- Security responsibility
- Must maintain ourselves

## Decision

We will build **in-house passwordless authentication** using established .NET libraries and security standards, with the following priority order:

1. **Passkeys** (WebAuthn/FIDO2) - Primary, passwordless
2. **Hardware tokens** (YubiKey, etc.) - For security-conscious users
3. **Email OTP** - Passwordless alternative
4. **Social sign-on** - Google, Apple, Microsoft
5. **Email/password** - Fallback only

## Rationale

1. **Passkeys are the future**: Apple, Google, and Microsoft have unified behind passkeys. Building native support positions us well.

2. **Better UX**: Face ID, Touch ID, Windows Hello provide superior experience to passwords.

3. **Phishing resistant**: WebAuthn is cryptographically bound to domains, eliminating phishing attacks.

4. **Cost control**: No per-authentication fees means predictable costs as we scale.

5. **Full customization**: We can build a family-friendly authentication experience without vendor constraints.

6. **Proven libraries**: .NET has excellent support via FIDO2 libraries and ASP.NET Core Identity.

## Authentication Methods

### Priority 1: Passkeys (WebAuthn/FIDO2)

| Feature | Description |
|---------|-------------|
| Platform passkeys | Synced via iCloud Keychain, Google Password Manager, Windows Hello |
| Cross-device | QR code flow for signing in on new devices |
| Biometric | Face ID, Touch ID, fingerprint, Windows Hello |
| Discoverable | Usernameless login where supported |

### Priority 2: Hardware Security Keys

| Feature | Description |
|---------|-------------|
| FIDO2 keys | YubiKey, Google Titan, Feitian |
| USB/NFC/BLE | Multiple transport options |
| Backup method | For recovery when passkey unavailable |

### Priority 3: Email OTP (Passwordless)

| Feature | Description |
|---------|-------------|
| Magic links | Click-to-login email links |
| 6-digit codes | Time-limited verification codes |
| No password needed | True passwordless experience |

### Priority 4: Social Sign-On

| Provider | Implementation |
|----------|----------------|
| Google | OAuth 2.0 / OpenID Connect |
| Apple | Sign in with Apple |
| Microsoft | Microsoft Account OAuth |

### Priority 5: Email/Password (Fallback)

| Feature | Description |
|---------|-------------|
| Strong passwords | Minimum requirements enforced |
| Breach detection | Check against haveibeenpwned |
| Rate limiting | Prevent brute force attacks |
| MFA required | Mandatory second factor |

## Multi-Factor Authentication

MFA is **required** for email/password authentication and **optional** (but encouraged) for passkey users.

| MFA Method | Use Case |
|------------|----------|
| Passkey as 2FA | Add passkey as second factor to password |
| TOTP apps | Authenticator apps (Google, Microsoft, Authy) |
| Email OTP | Fallback verification via email |
| Hardware key | YubiKey or similar FIDO2 device |

## Consequences

### Positive

- Industry-leading passwordless experience
- Phishing-resistant authentication
- No per-authentication costs
- Full control over user experience
- Better mobile experience with biometrics
- Future-proof authentication stack

### Negative

- Initial development investment
- Security responsibility is ours
- Must keep up with WebAuthn spec evolution
- Need to build admin tooling

### Neutral

- Need to design credential recovery flows
- Should implement gradual passkey enrollment
- Must handle cross-device scenarios

## Implementation

### Technology Stack

| Component | Technology |
|-----------|------------|
| WebAuthn Server | FIDO2 library for .NET (fido2-net-lib) |
| Identity Core | ASP.NET Core Identity |
| Token Format | JWT with custom claims |
| Session Store | Redis for distributed sessions |
| Credential Store | CosmosDB (encrypted) |
| Social OAuth | ASP.NET Core Authentication |

### Database Schema

```csharp
// UserCredential.cs - Stores all credential types
public class UserCredential
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string UserId { get; set; } = string.Empty;
    public CredentialType Type { get; set; }
    public string? CredentialId { get; set; }  // For WebAuthn
    public byte[]? PublicKey { get; set; }      // For WebAuthn
    public string? ProviderKey { get; set; }    // For social logins
    public string? ProviderName { get; set; }   // For social logins
    public string? PasswordHash { get; set; }   // For password fallback
    public DateTime CreatedAt { get; set; }
    public DateTime? LastUsedAt { get; set; }
    public string? DeviceName { get; set; }     // "John's iPhone"
    public bool IsBackup { get; set; }          // Recovery credential
}

public enum CredentialType
{
    Passkey,
    HardwareKey,
    Password,
    SocialGoogle,
    SocialApple,
    SocialMicrosoft,
    EmailOtp,
    Totp
}
```

### JWT Claims Structure

```json
{
  "sub": "user-guid",
  "email": "user@example.com",
  "name": "John Doe",
  "family_id": "family-guid",
  "role": "admin",
  "auth_method": "passkey",
  "mfa_verified": true,
  "aud": "luminous-api",
  "iss": "https://auth.luminous.family",
  "exp": 1234567890,
  "iat": 1234567890
}
```

### .NET API Authentication Setup

```csharp
// Program.cs
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.Authority = builder.Configuration["Auth:Authority"];
    options.Audience = builder.Configuration["Auth:Audience"];
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ClockSkew = TimeSpan.FromMinutes(1)
    };
});

// Add FIDO2/WebAuthn support
builder.Services.AddFido2(options =>
{
    options.ServerDomain = "luminous.family";
    options.ServerName = "Luminous";
    options.Origins = new HashSet<string> { "https://app.luminous.family" };
});

// Add social authentication
builder.Services.AddAuthentication()
    .AddGoogle(options =>
    {
        options.ClientId = builder.Configuration["Auth:Google:ClientId"]!;
        options.ClientSecret = builder.Configuration["Auth:Google:ClientSecret"]!;
    })
    .AddApple(options =>
    {
        options.ClientId = builder.Configuration["Auth:Apple:ClientId"]!;
        options.TeamId = builder.Configuration["Auth:Apple:TeamId"]!;
        options.KeyId = builder.Configuration["Auth:Apple:KeyId"]!;
        options.PrivateKey = builder.Configuration["Auth:Apple:PrivateKey"]!;
    })
    .AddMicrosoftAccount(options =>
    {
        options.ClientId = builder.Configuration["Auth:Microsoft:ClientId"]!;
        options.ClientSecret = builder.Configuration["Auth:Microsoft:ClientSecret"]!;
    });
```

### WebAuthn Registration Flow

```csharp
// AuthController.cs
[HttpPost("passkey/register/begin")]
public async Task<ActionResult<CredentialCreateOptions>> BeginPasskeyRegistration()
{
    var user = await GetCurrentUserAsync();

    var options = _fido2.RequestNewCredential(
        new Fido2User
        {
            Id = Encoding.UTF8.GetBytes(user.Id),
            Name = user.Email,
            DisplayName = user.DisplayName
        },
        existingCredentials: await GetExistingCredentialsAsync(user.Id),
        authenticatorSelection: new AuthenticatorSelection
        {
            ResidentKey = ResidentKeyRequirement.Preferred,
            UserVerification = UserVerificationRequirement.Preferred
        },
        attestationPreference: AttestationConveyancePreference.None
    );

    HttpContext.Session.SetString("fido2.attestationOptions", options.ToJson());
    return Ok(options);
}

[HttpPost("passkey/register/complete")]
public async Task<ActionResult> CompletePasskeyRegistration(
    [FromBody] AuthenticatorAttestationRawResponse response)
{
    var options = CredentialCreateOptions.FromJson(
        HttpContext.Session.GetString("fido2.attestationOptions"));

    var result = await _fido2.MakeNewCredentialAsync(
        response, options, IsCredentialIdUniqueAsync);

    await SaveCredentialAsync(result.Result);
    return Ok(new { success = true });
}
```

### Angular Auth Service

```typescript
// auth.service.ts
@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly currentUser = signal<User | null>(null);

  // Passkey registration
  async registerPasskey(): Promise<void> {
    const options = await firstValueFrom(
      this.http.post<PublicKeyCredentialCreationOptions>(
        '/api/auth/passkey/register/begin', {}
      )
    );

    const credential = await navigator.credentials.create({
      publicKey: this.decodeOptions(options)
    });

    await firstValueFrom(
      this.http.post('/api/auth/passkey/register/complete',
        this.encodeCredential(credential))
    );
  }

  // Passkey authentication
  async signInWithPasskey(): Promise<void> {
    const options = await firstValueFrom(
      this.http.post<PublicKeyCredentialRequestOptions>(
        '/api/auth/passkey/authenticate/begin', {}
      )
    );

    const assertion = await navigator.credentials.get({
      publicKey: this.decodeOptions(options)
    });

    const response = await firstValueFrom(
      this.http.post<AuthResponse>('/api/auth/passkey/authenticate/complete',
        this.encodeAssertion(assertion))
    );

    this.setTokens(response);
  }

  // Email OTP
  async requestEmailOtp(email: string): Promise<void> {
    await firstValueFrom(
      this.http.post('/api/auth/email-otp/request', { email })
    );
  }

  async verifyEmailOtp(email: string, code: string): Promise<void> {
    const response = await firstValueFrom(
      this.http.post<AuthResponse>('/api/auth/email-otp/verify',
        { email, code })
    );
    this.setTokens(response);
  }
}
```

### iOS Passkey Integration

```swift
import AuthenticationServices

class AuthService: NSObject, ASAuthorizationControllerDelegate {

    func signInWithPasskey() async throws {
        let challenge = try await fetchChallenge()

        let provider = ASAuthorizationPlatformPublicKeyCredentialProvider(
            relyingPartyIdentifier: "luminous.family"
        )

        let request = provider.createCredentialAssertionRequest(
            challenge: challenge
        )

        let controller = ASAuthorizationController(
            authorizationRequests: [request]
        )
        controller.delegate = self
        controller.performRequests()
    }

    func registerPasskey() async throws {
        let options = try await fetchRegistrationOptions()

        let provider = ASAuthorizationPlatformPublicKeyCredentialProvider(
            relyingPartyIdentifier: "luminous.family"
        )

        let request = provider.createCredentialRegistrationRequest(
            challenge: options.challenge,
            name: options.userName,
            userID: options.userId
        )

        let controller = ASAuthorizationController(
            authorizationRequests: [request]
        )
        controller.delegate = self
        controller.performRequests()
    }
}
```

### Device Linking Flow

For wall displays without keyboards:

1. **Display generates code** - 6-character alphanumeric, 15-minute expiry
2. **User authenticates on mobile** - Using passkey or other method
3. **User enters code** - Associates display with family
4. **Display receives device token** - Long-lived token for API access

```csharp
// DeviceLinkingService.cs
public async Task<DeviceLinkCode> GenerateDeviceCodeAsync(string deviceId)
{
    var code = GenerateSecureCode(); // 6 alphanumeric, case-insensitive

    var linkCode = new DeviceLinkCode
    {
        Code = code,
        DeviceId = deviceId,
        ExpiresAt = DateTime.UtcNow.AddMinutes(15),
        CreatedAt = DateTime.UtcNow
    };

    await _cache.SetAsync($"device-link:{code.ToUpper()}", linkCode,
        TimeSpan.FromMinutes(15));

    return linkCode;
}

public async Task<DeviceToken> LinkDeviceAsync(
    string code,
    string familyId,
    string userId)
{
    var linkCode = await _cache.GetAsync<DeviceLinkCode>(
        $"device-link:{code.ToUpper()}");

    if (linkCode == null || linkCode.ExpiresAt < DateTime.UtcNow)
        throw new InvalidCodeException();

    // Create device record
    var device = new Device
    {
        Id = linkCode.DeviceId,
        FamilyId = familyId,
        LinkedByUserId = userId,
        LinkedAt = DateTime.UtcNow
    };

    await _deviceRepository.UpsertAsync(device);
    await _cache.RemoveAsync($"device-link:{code.ToUpper()}");

    // Generate long-lived device token
    return GenerateDeviceToken(device);
}
```

## Security Considerations

1. **Credential Storage**
   - WebAuthn public keys stored encrypted in CosmosDB
   - Password hashes use Argon2id
   - No plaintext credentials ever stored

2. **Token Security**
   - Short-lived access tokens (15 minutes)
   - Refresh token rotation on each use
   - Device binding for refresh tokens
   - Secure storage (Keychain/Keystore on mobile)

3. **Rate Limiting**
   - Email OTP: 3 requests per 10 minutes per email
   - Password attempts: 5 per 15 minutes, then lockout
   - Registration: 3 per hour per IP

4. **Recovery**
   - Multiple credential types encouraged
   - Recovery codes for account recovery
   - Email verification for sensitive operations

5. **Monitoring**
   - Failed authentication alerts
   - New device notifications
   - Credential addition notifications

## Migration Path

For users transitioning from passwords:

1. **Prompt passkey enrollment** after password sign-in
2. **Gradual migration** - don't force, encourage
3. **Keep password** as backup until passkey registered
4. **Celebrate** passwordless achievement in UI

## Related Decisions

- [ADR-003: Azure as Cloud Platform](./ADR-003-azure-cloud-platform.md)
- [ADR-006: Multi-Tenant Architecture](./ADR-006-multi-tenant-architecture.md)
- [ADR-001: .NET 10 as Backend Platform](./ADR-001-dotnet-backend.md)
- [ADR-011: Secure Registration Flow](./ADR-011-secure-registration-flow.md)

## References

- [WebAuthn Specification](https://www.w3.org/TR/webauthn-2/)
- [FIDO2 .NET Library](https://github.com/passwordless-lib/fido2-net-lib)
- [Apple Passkeys Documentation](https://developer.apple.com/documentation/authenticationservices/public-private_key_authentication/supporting_passkeys)
- [Android Credential Manager](https://developer.android.com/training/sign-in/passkeys)
- [Passkeys.dev](https://passkeys.dev/)
