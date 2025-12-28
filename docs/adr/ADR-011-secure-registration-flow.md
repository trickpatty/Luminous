# ADR-011: Secure Registration Flow with Email Verification

> **Status:** Accepted
> **Date:** 2025-12-28
> **Deciders:** Luminous Core Team
> **Categories:** Security, Architecture

## Context

The initial registration flow allowed users to register with any email address and immediately create a passkey. This design had a critical security vulnerability: an attacker could register using someone else's email address before the legitimate owner registered, effectively impersonating them.

When the legitimate email owner later tried to register, they would find their email was "already taken" by the attacker. This is a form of identity impersonation attack.

Additionally, the original implementation:
- Trusted the email parameter sent from the client during registration completion
- Did not verify email ownership before account creation
- Did not support invite codes for joining existing families

## Decision Drivers

- **Security**: Prevent identity impersonation attacks
- **Data Integrity**: Ensure users own the email addresses they register with
- **User Experience**: Maintain reasonable registration friction
- **Family Support**: Enable new users to join existing families via invitation
- **Industry Standards**: Follow email verification best practices

## Considered Options

### Option 1: Post-Registration Email Verification

Send verification email after account creation, limit functionality until verified.

**Pros:**
- Simple implementation
- User can start using the app immediately

**Cons:**
- Account already exists before verification
- Attacker can still claim email addresses
- Doesn't prevent the core attack

### Option 2: Pre-Registration Email Verification (Selected)

Verify email ownership via OTP before allowing account creation.

**Pros:**
- Prevents identity impersonation completely
- Email verified before any account data created
- Server-side session storage prevents client tampering
- Industry-standard approach

**Cons:**
- Adds friction to registration
- Requires session management

### Option 3: Magic Link Registration

Send a registration link to the email, user completes registration via link.

**Pros:**
- Very secure
- No OTP to enter

**Cons:**
- Disrupts flow (email context switching)
- Link expiration issues
- Device continuity problems

## Decision

We will implement a **2-step registration flow with pre-registration email verification**:

1. **Registration Start** (`POST /api/auth/register/start`)
   - Client sends: email, displayName, inviteCode (optional)
   - Server validates email is not already registered
   - Server stores email in server-side session
   - Server generates and sends 6-digit OTP
   - Server returns session ID

2. **OTP Verification** (`POST /api/auth/otp/verify`)
   - Client sends: email, code
   - Server validates OTP
   - Server marks email as verified in session

3. **Registration Complete** (`POST /api/auth/register/complete`)
   - Client sends: passkey attestation (NO email parameter)
   - Server retrieves verified email from session (server-side)
   - Server creates user and family (or joins via invite code)
   - Server stores passkey credential
   - Server issues JWT tokens

**Critical Security Measure**: The `register/complete` endpoint does NOT accept an email parameter. It retrieves the verified email from the server-side session only. This prevents attackers from completing registration with a different email than was verified.

## Rationale

1. **Server-side session storage**: Email is stored server-side and never trusted from the client during completion. This is the key security measure.

2. **OTP over magic links**: OTP keeps users in the same device/browser context, providing better UX.

3. **Pre-registration verification**: Verification before account creation means no orphaned or unverified accounts.

4. **Invite code support**: Enables family members to join existing families during registration.

## Consequences

### Positive

- Completely prevents email impersonation attacks
- Server-side session prevents client-side tampering
- Email ownership verified before any data is created
- Supports family invitation workflow
- Follows security best practices

### Negative

- Adds registration friction (OTP step)
- Requires distributed session storage (Redis) for production
- Session expiration requires registration restart

### Neutral

- OTP codes expire after 10 minutes
- Sessions expire after 30 minutes of inactivity
- Rate limiting prevents OTP brute-force (3 attempts per 15 minutes)

## Implementation

### API Endpoints

```csharp
// RegisterStartCommand.cs
public record RegisterStartRequest(
    string Email,
    string DisplayName,
    string? InviteCode
);

// RegisterCompleteCommand.cs - NO email parameter
public record RegisterCompleteRequest(
    AuthenticatorAttestationRawResponse Attestation
);
```

### Session Storage

```csharp
// Server-side session keys
private const string SessionEmailKey = "Registration:Email";
private const string SessionDisplayNameKey = "Registration:DisplayName";
private const string SessionEmailVerifiedKey = "Registration:EmailVerified";
private const string SessionInviteCodeKey = "Registration:InviteCode";
```

### Security Configuration

- OTP: 6 digits, 10-minute expiry
- Session: 30-minute sliding expiration
- Rate limiting: 3 OTP requests per 15 minutes per email
- OTP attempts: 5 per session before lockout

## Related Decisions

- [ADR-010: In-House Passwordless Authentication](./ADR-010-passwordless-authentication.md)
- [ADR-006: Multi-Tenant Architecture](./ADR-006-multi-tenant-architecture.md)

## References

- [OWASP Registration Best Practices](https://cheatsheetseries.owasp.org/cheatsheets/Authentication_Cheat_Sheet.html)
- [NIST Digital Identity Guidelines](https://pages.nist.gov/800-63-3/)
