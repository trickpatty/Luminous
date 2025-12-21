# ADR-010: Azure AD B2C for Identity

> **Status:** Accepted
> **Date:** 2025-12-21
> **Deciders:** Luminous Core Team
> **Categories:** Architecture, Security

## Context

Luminous is a multi-tenant SaaS platform serving multiple families. We need an identity solution that supports:
- Consumer sign-up and sign-in (not enterprise/corporate)
- Social logins (Google, Apple, Microsoft)
- Multi-factor authentication
- Custom claims (family_id, roles)
- Device linking flow
- Multiple platforms (web, mobile, display)

## Decision Drivers

- **Consumer identity**: Not enterprise Azure AD, but consumer-focused B2C
- **Social logins**: Families expect Google/Apple sign-in options
- **Security**: MFA, secure token handling, password policies
- **Custom claims**: Need family_id and role claims in tokens
- **Azure integration**: Native integration with our Azure services
- **Platform support**: SDKs for .NET, Angular, iOS, Android

## Considered Options

### Option 1: Azure AD B2C

Microsoft's consumer identity platform.

**Pros:**
- Native Azure integration
- Social identity providers built-in
- Custom policies for complex flows
- Custom claims support
- MSAL libraries for all platforms
- Scales automatically
- Compliant (SOC 2, GDPR)

**Cons:**
- Complex custom policy XML
- Learning curve for advanced scenarios
- Per-authentication pricing

### Option 2: Auth0

Third-party identity platform.

**Pros:**
- Developer-friendly
- Good documentation
- Social connections

**Cons:**
- Third-party service (not Azure-native)
- Separate billing
- Integration overhead
- Higher cost at scale

### Option 3: Firebase Authentication

Google's authentication service.

**Pros:**
- Easy to set up
- Good mobile SDKs

**Cons:**
- Not Azure-native
- Limited custom claims
- Google ecosystem dependency

### Option 4: Custom Authentication

Build our own authentication system.

**Pros:**
- Full control

**Cons:**
- Security risk
- Significant development effort
- Maintenance burden
- No social login out of box
- Not recommended for consumer apps

## Decision

We will use **Azure AD B2C** for all identity and authentication in Luminous.

## Rationale

1. **Azure Native**: Seamless integration with Azure App Service, Functions, and Cosmos DB via managed identity and token validation.

2. **Consumer Focus**: B2C is designed for consumer applications, unlike Azure AD which targets enterprises.

3. **Social Logins**: Built-in support for Google, Apple, Microsoft, and Facebook with minimal configuration.

4. **Custom Claims**: JWT tokens can include custom claims like `family_id` and `role` for authorization.

5. **MSAL Libraries**: Microsoft Authentication Library available for .NET, Angular, iOS, and Android.

6. **Compliance**: SOC 2, HIPAA, GDPR compliant out of the box.

## Consequences

### Positive

- Enterprise-grade security without building it
- Social logins reduce friction
- Custom claims enable family-based authorization
- Single identity across all platforms
- MFA built-in
- Automatic scaling

### Negative

- Custom policies have learning curve
- Per-authentication costs (though minimal)
- Azure lock-in for identity
- Complex XML for advanced flows

### Neutral

- Need to design custom claims structure
- Should use user flows for simple scenarios, custom policies for complex
- Token validation required in API

## Implementation

### User Flows

| Flow | Purpose |
|------|---------|
| Sign Up / Sign In | Combined flow for new and returning users |
| Password Reset | Self-service password recovery |
| Profile Edit | Update name, email, preferences |

### Custom Claims

```json
{
  "sub": "user-guid",
  "email": "user@example.com",
  "name": "John Doe",
  "family_id": "family-guid",
  "role": "admin",
  "aud": "luminous-api-client-id",
  "iss": "https://luminous.b2clogin.com/..."
}
```

### Token Validation (.NET API)

```csharp
// Program.cs
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAdB2C"));

// appsettings.json
{
  "AzureAdB2C": {
    "Instance": "https://luminous.b2clogin.com",
    "Domain": "luminous.onmicrosoft.com",
    "ClientId": "api-client-id",
    "SignUpSignInPolicyId": "B2C_1_SignUpSignIn"
  }
}
```

### Angular Integration

```typescript
// app.config.ts
import { MsalModule, MsalInterceptor } from '@azure/msal-angular';

export const appConfig: ApplicationConfig = {
  providers: [
    importProvidersFrom(
      MsalModule.forRoot({
        auth: {
          clientId: 'web-client-id',
          authority: 'https://luminous.b2clogin.com/luminous.onmicrosoft.com/B2C_1_SignUpSignIn',
          redirectUri: 'https://app.luminous.family'
        }
      }, {
        interactionType: InteractionType.Redirect
      }, {
        interactionType: InteractionType.Redirect,
        protectedResourceMap: new Map([
          ['https://api.luminous.family/*', ['api-scope']]
        ])
      })
    ),
    {
      provide: HTTP_INTERCEPTORS,
      useClass: MsalInterceptor,
      multi: true
    }
  ]
};
```

### Mobile Integration (iOS)

```swift
import MSAL

class AuthService {
    private let authority = "https://luminous.b2clogin.com/tfp/luminous.onmicrosoft.com/B2C_1_SignUpSignIn"
    private let clientId = "ios-client-id"

    func signIn() async throws -> MSALResult {
        let config = MSALPublicClientApplicationConfig(clientId: clientId)
        config.authority = try MSALAuthority(url: URL(string: authority)!)

        let application = try MSALPublicClientApplication(configuration: config)
        let parameters = MSALInteractiveTokenParameters(
            scopes: ["api-scope"],
            webviewParameters: MSALWebviewParameters()
        )

        return try await application.acquireToken(with: parameters)
    }
}
```

### Device Linking Flow

For wall displays that don't have keyboards for login:

1. **Display shows 6-digit code**: Generated by API, stored with expiry
2. **User enters code in mobile app**: While logged in on mobile
3. **API validates and links**: Associates device with user's family
4. **Display receives tokens**: Via SignalR, device is authenticated

```csharp
// DeviceLinkingService.cs
public async Task<string> GenerateDeviceCodeAsync(string deviceId)
{
    var code = GenerateSecureCode(); // 6 alphanumeric characters
    await _cache.SetAsync($"device-code:{code}", new DeviceLink {
        DeviceId = deviceId,
        ExpiresAt = DateTime.UtcNow.AddMinutes(10)
    });
    return code;
}

public async Task<bool> LinkDeviceAsync(string code, string familyId, string userId)
{
    var link = await _cache.GetAsync<DeviceLink>($"device-code:{code}");
    if (link == null || link.ExpiresAt < DateTime.UtcNow)
        return false;

    await _deviceRepository.UpdateFamilyAsync(link.DeviceId, familyId, userId);
    await _cache.RemoveAsync($"device-code:{code}");
    return true;
}
```

## Security Considerations

1. **Token Validation**: All APIs validate JWT tokens with proper issuer and audience checks
2. **Refresh Tokens**: Secure storage on mobile (Keychain/Keystore), rotation enabled
3. **MFA**: Enabled for admin operations and sensitive actions
4. **Session Management**: Reasonable token lifetimes, refresh token rotation
5. **Device Codes**: Short expiry (10 minutes), single-use, secure random generation

## Related Decisions

- [ADR-003: Azure as Cloud Platform](./ADR-003-azure-cloud-platform.md)
- [ADR-006: Multi-Tenant Architecture](./ADR-006-multi-tenant-architecture.md)
- [ADR-001: .NET 10 as Backend Platform](./ADR-001-dotnet-backend.md)

## References

- [Azure AD B2C Documentation](https://learn.microsoft.com/azure/active-directory-b2c/)
- [MSAL for .NET](https://learn.microsoft.com/azure/active-directory/develop/msal-net-initializing-client-applications)
- [MSAL Angular](https://github.com/AzureAD/microsoft-authentication-library-for-js)
- [MSAL iOS](https://github.com/AzureAD/microsoft-authentication-library-for-objc)
