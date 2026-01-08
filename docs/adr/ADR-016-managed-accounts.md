# ADR-016: Managed Accounts for Children and Non-Email Users

> **Status:** Accepted
> **Date:** 2026-01-08
> **Deciders:** Luminous Core Team
> **Categories:** Architecture, Security, User Experience

## Context

Luminous is a family command center designed to serve all household members, including children who typically don't have email addresses. The current authentication system (ADR-010, ADR-011) requires email verification for account creation, which creates a barrier for:

1. **Young children (ages 6-12)** who need to view schedules, complete chores, and earn rewards
2. **Teens without email** who may not have or want an email account
3. **Elderly family members** who may be uncomfortable with email-based registration
4. **Guest family members** who need temporary household access

These users still need to:
- Be represented on calendars, chores, and lists
- Interact with the display app (mark chores complete, view schedules)
- Potentially use mobile apps or web apps on their own devices
- Have their own identity and permissions within the household

The system must accommodate these users while maintaining security and parental oversight.

## Decision Drivers

- **Family Inclusivity**: All family members must be able to participate, regardless of email availability (aligns with BP-3: Inclusive by Default)
- **Child Safety**: Parents must maintain control over children's accounts and devices (COPPA compliance considerations)
- **Security**: Non-email authentication must be secure and not introduce vulnerabilities
- **User Experience**: Setup must be simple for parents; usage must be intuitive for children
- **Platform Parity**: Solution must work across Display, Web, iOS, and Android platforms
- **Zero Distraction**: Authentication flows should be quick and not disrupt family routines (aligns with ADR-009)

## Considered Options

### Option 1: Email Required for All Users

Maintain current requirement that all users must have email addresses.

**Pros:**
- Simplest implementation (already done)
- Standard identity recovery mechanism
- Consistent security model

**Cons:**
- Excludes young children from the platform
- Forces parents to create email accounts for children
- Poor user experience for families
- Doesn't align with family-centric design principle

### Option 2: Profiles Without Accounts (Display-Only)

Allow profiles to exist without authentication; only authenticated users can perform actions on behalf of profiles.

**Pros:**
- Simple to implement
- Children represented in calendars and chores
- Parents maintain full control

**Cons:**
- Children can't interact with their own chores/tasks
- No mobile app access for children
- Doesn't scale to teen use cases
- Reduces child engagement and ownership

### Option 3: Managed Accounts with Parent-Delegated Authentication (Selected)

Introduce a "Managed Account" concept where parents create and control accounts for children without requiring email. These accounts support multiple authentication methods based on the child's needs and device access.

**Pros:**
- Children can fully participate
- Parents maintain oversight and control
- Flexible authentication options per child's maturity
- Works across all platforms
- Scalable from young children to teens

**Cons:**
- More complex implementation
- New concepts to document and explain
- Requires parent involvement for initial setup

### Option 4: Family-Wide PIN System

Use a single family PIN or individual profile PINs without formal accounts.

**Pros:**
- Very simple user experience
- No account management complexity

**Cons:**
- Weak security (PINs are guessable)
- No device-level isolation
- Can't support mobile app scenarios well
- No audit trail per user

## Decision

We will implement **Managed Accounts** with the following architecture:

### Account Types

| Account Type | Email Required | Created By | Can Authenticate | Platforms | Recovery |
|--------------|---------------|------------|------------------|-----------|----------|
| **Full Account** | Yes | Self | Passkey, Email OTP, Social, Password | All | Email-based |
| **Managed Account** | No | Parent/Admin | PIN, Device Passkey, Parent QR | All | Parent resets |
| **Profile Only** | No | Parent/Admin | None (display-only) | Display | N/A |

### Managed Account Authentication Methods

#### Method 1: Profile PIN (Display & Shared Devices)
- 4-6 digit PIN set by parent for each managed account
- Used on display app and shared family devices
- Provides "who's using this" identification
- Rate-limited (5 attempts, then lockout)

#### Method 2: Device Passkey (Child's Personal Device)
- Parent initiates passkey registration on child's device
- No email required; passkey bound to device
- Biometric unlock (Face ID, fingerprint) for child's convenience
- Parent can revoke device access remotely

#### Method 3: Parent-Delegated Device Linking
- Child's device shows QR code
- Parent scans QR with their authenticated device
- Parent selects which managed account to link
- Child's device receives session token for that account

### Authentication Flows

#### Flow 1: Child Uses Display App (PIN)

```
Display App                    API                      Parent App
     |                          |                           |
     | 1. Child taps their      |                           |
     |    profile avatar        |                           |
     |                          |                           |
     | 2. Enter PIN prompt      |                           |
     |    (4-6 digits)          |                           |
     |                          |                           |
     | 3. Verify PIN ---------->|                           |
     |    (rate limited)        |                           |
     |                          |                           |
     |<---- Session token ------|                           |
     |    (scoped to profile,   |                           |
     |     device, limited      |                           |
     |     permissions)         |                           |
     |                          |                           |
     | 4. Child can now:        |                           |
     |    - View their schedule |                           |
     |    - Mark chores done    |                           |
     |    - Check rewards       |                           |
```

#### Flow 2: Parent Sets Up Child's Mobile Device (Device Passkey)

```
Parent App                     API                    Child Device
     |                          |                           |
     | 1. Parent navigates to   |                           |
     |    "Set Up Child Device" |                           |
     |                          |                           |
     | 2. Selects child profile |                           |
     |                          |                           |
     | 3. Generate setup code ->|                           |
     |                          |                           |
     |<-- 6-digit code (5 min) -|                           |
     |                          |                           |
     |                          |      4. Child downloads app
     |                          |         and enters code   |
     |                          |                           |
     |                          |<---- Validate code -------|
     |                          |                           |
     |                          |-- Begin passkey setup --->|
     |                          |                           |
     |                          |    5. Child uses Face ID/ |
     |                          |       fingerprint to      |
     |                          |       create passkey      |
     |                          |                           |
     |                          |<-- Passkey attestation ---|
     |                          |                           |
     |                          |-- Device token (30 day) ->|
     |                          |                           |
     | 6. Parent notified       |                           |
     |    of new device linked  |                           |
```

#### Flow 3: Quick Device Authorization (QR Code)

```
Child Device                   API                     Parent App
     |                          |                           |
     | 1. Child opens app,      |                           |
     |    not logged in         |                           |
     |                          |                           |
     | 2. Taps "Join Family"    |                           |
     |                          |                           |
     | 3. Request QR code ----->|                           |
     |                          |                           |
     |<---- QR code + token ----|                           |
     |    (displays on screen,  |                           |
     |     10 min expiry)       |                           |
     |                          |                           |
     |                          |      4. Parent opens app  |
     |                          |         taps "Scan Code"  |
     |                          |                           |
     |                          |<---- Scans QR code -------|
     |                          |                           |
     |                          |      5. Parent selects    |
     |                          |         managed account   |
     |                          |         to link           |
     |                          |                           |
     |                          |<---- Authorize device ----|
     |                          |                           |
     | 6. Device receives       |                           |
     |    session via polling   |                           |
     |    or WebSocket          |                           |
     |                          |                           |
     |<---- Device token -------|                           |
     |    (linked to managed    |                           |
     |     account)             |                           |
```

### Permission Model for Managed Accounts

Managed accounts inherit the role-based permission model with additional restrictions:

| Permission | Child Role | Teen Role | Notes |
|------------|-----------|-----------|-------|
| View own calendar | Yes | Yes | Can see events assigned to them |
| View family calendar | Yes | Yes | Filtered view based on sharing settings |
| Mark chores complete | Yes | Yes | Primary interaction for children |
| Create events | No | Own only | Teens can add to their own calendar |
| Edit family events | No | No | Only adults/admins |
| View rewards balance | Yes | Yes | See their points |
| Request rewards | Yes | Yes | Parent approval required |
| Manage profile | View | Limited | Teens can update avatar/name |
| Device management | No | No | Parent-controlled |
| Family settings | No | No | Admin-only |

### Data Model Changes

```csharp
public class User
{
    // ... existing fields ...

    // New fields for managed accounts
    public AccountType AccountType { get; set; } = AccountType.Full;
    public string? ProfilePin { get; set; }   // Hashed PIN (null if not set)
    public bool PinEnabled { get; set; } = false;
    public DateTime? PinLastChanged { get; set; }
    public int PinFailedAttempts { get; set; } = 0;
    public DateTime? PinLockedUntil { get; set; }
}

public enum AccountType
{
    Full,       // Has email, can self-authenticate
    Managed,    // No email, any parent/admin can manage authentication
    ProfileOnly // No authentication, display representation only
}

public class ManagedDevice
{
    public string Id { get; set; } = Nanoid.Generate();
    public string FamilyId { get; set; } = string.Empty;  // Partition key
    public string ManagedAccountId { get; set; } = string.Empty;  // The child's account
    public string AuthorizedById { get; set; } = string.Empty;  // Parent/Admin who authorized (audit)
    public string DeviceIdentifier { get; set; } = string.Empty;  // Device fingerprint
    public string? DeviceName { get; set; }  // "Jordan's iPad"
    public ManagedDeviceType Type { get; set; }
    public string? PasskeyCredentialId { get; set; }  // If device passkey method used
    public DateTime AuthorizedAt { get; set; }
    public DateTime? LastUsedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }  // Optional expiration
    public bool IsRevoked { get; set; } = false;
}

public enum ManagedDeviceType
{
    SharedDisplay,    // Family display, PIN auth
    PersonalMobile,   // Child's phone/tablet, passkey auth
    PersonalWeb,      // Child's browser, passkey or session auth
    TemporarySession  // QR-code authorized session
}
```

**Note:** Managed accounts are not tied to a specific parent. Any user with Owner, Admin, or Adult role can manage all managed accounts in their family. The `AuthorizedById` field in `ManagedDevice` is for audit purposes only, tracking which parent/admin authorized a specific device.

### JWT Claims for Managed Accounts

```json
{
  "sub": "managed-account-id",
  "family_id": "family-guid",
  "role": "child",
  "account_type": "managed",
  "auth_method": "pin|device_passkey|parent_qr",
  "device_id": "device-guid",
  "permissions": ["view_calendar", "complete_chores", "view_rewards"],
  "exp": 1234567890
}
```

### Security Considerations

1. **PIN Security**
   - Hashed with Argon2id (same as passwords)
   - Rate limited: 5 attempts per 15 minutes
   - Lockout after 5 failed attempts (30 minute cooldown)
   - No PIN hints or recovery (parent resets)

2. **Device Passkey Security**
   - Standard WebAuthn security model
   - Device-bound (not synced to cloud keychain)
   - Parent can revoke at any time
   - Optional expiration date

3. **QR Code Authorization Security**
   - Short-lived codes (10 minutes)
   - Single use
   - Requires parent authentication
   - Logged in audit trail

4. **Session Token Security**
   - Shorter expiration for managed accounts (4 hours vs 24 hours)
   - Device binding
   - Parent notification on new device login
   - Remote session revocation

### Parent Controls

Parents/Admins have full control over managed accounts:

| Control | Description |
|---------|-------------|
| Create managed account | Add child to family without email |
| Set/reset PIN | Manage child's display PIN |
| Authorize device | Link child's device via code or QR |
| Revoke device | Remove access from any device |
| View activity | See child's recent actions |
| Upgrade to full | Convert to email account when child is ready |
| Delete account | Remove managed account entirely |

## Rationale

1. **Three-tier account system** (Full, Managed, Profile-Only) provides flexibility for different family situations while maintaining clear security boundaries.

2. **Multiple authentication methods** allow parents to choose the appropriate level of independence for each child based on age and maturity.

3. **Parent-delegated device linking** ensures parents maintain control while allowing children device access without email addresses.

4. **PIN authentication on shared devices** provides a simple "who's using this" mechanism without the complexity of full authentication.

5. **Device passkeys for personal devices** give children a secure, modern authentication experience without email dependency.

## Consequences

### Positive

- All family members can participate regardless of email availability
- Parents maintain appropriate oversight of children's digital presence
- Children can independently interact with chores and schedules
- Flexible system accommodates family evolution (child grows up, gets email)
- Aligns with family-centric and inclusive design principles

### Negative

- Additional complexity in authentication system
- More user states to handle in UI
- Parent burden for initial device setup
- Need to educate parents on managed account concepts

### Neutral

- Managed accounts cannot use social login or email OTP
- Password reset flows only available for full accounts
- Some features may be restricted for managed accounts

## Implementation Notes

### API Endpoints

```
POST   /api/users/family/{familyId}/managed          # Create managed account
PUT    /api/users/family/{familyId}/{userId}/pin     # Set/update PIN
DELETE /api/users/family/{familyId}/{userId}/pin     # Remove PIN
POST   /api/auth/pin/verify                          # Verify PIN, get session

POST   /api/managed-devices/setup-code               # Generate setup code for child device
POST   /api/managed-devices/activate                 # Child device activates with code
POST   /api/managed-devices/qr/generate              # Child device requests QR
POST   /api/managed-devices/qr/authorize             # Parent authorizes via scanned QR
GET    /api/managed-devices/family/{familyId}        # List managed devices
DELETE /api/managed-devices/{id}                     # Revoke device access

POST   /api/users/family/{familyId}/{userId}/upgrade # Convert managed to full account
```

### UI Components Needed

**Parent App:**
- "Add Child" flow (create managed account)
- "Set Up Child's Device" wizard
- QR code scanner for device authorization
- Managed account settings page
- Device management for children

**Child/Display App:**
- Profile selector with PIN entry
- "Join Family" flow (QR code display)
- Device setup wizard (enter code, create passkey)
- Age-appropriate error messages

### Migration Considerations

Existing families with child profiles (role=Child) will:
- Keep functioning as before
- Be prompted to set up managed accounts for enhanced features
- No forced migration; gradual opt-in

## Related Decisions

- [ADR-010: In-House Passwordless Authentication](./ADR-010-passwordless-authentication.md)
- [ADR-011: Secure Registration Flow](./ADR-011-secure-registration-flow.md)
- [ADR-006: Multi-Tenant Architecture](./ADR-006-multi-tenant-architecture.md)
- [ADR-009: Zero-Distraction Design Principle](./ADR-009-zero-distraction-principle.md)

## References

- [COPPA - Children's Online Privacy Protection Act](https://www.ftc.gov/legal-library/browse/rules/childrens-online-privacy-protection-rule-coppa)
- [WebAuthn Specification](https://www.w3.org/TR/webauthn-2/)
- [Apple Family Sharing Model](https://support.apple.com/en-us/HT201088)
- [Google Family Link](https://families.google.com/familylink/)
