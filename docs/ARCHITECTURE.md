# Luminous Architecture Document

> **Document Version:** 2.4.0
> **Last Updated:** 2025-12-28
> **Status:** Active
> **TOGAF Phase:** Phase B-D (Architecture Development)

---

## Table of Contents

1. [Architecture Principles](#architecture-principles)
2. [Architecture Vision](#architecture-vision)
3. [Business Architecture](#business-architecture)
4. [Data Architecture](#data-architecture)
5. [Application Architecture](#application-architecture)
6. [Technology Architecture](#technology-architecture)
7. [Security Architecture](#security-architecture)
8. [Integration Architecture](#integration-architecture)
9. [Azure Cloud Architecture](#azure-cloud-architecture)
10. [Multi-Tenancy Architecture](#multi-tenancy-architecture)
11. [Local Development Architecture](#local-development-architecture)
12. [Architecture Decisions](#architecture-decisions)

---

## Architecture Principles

Following TOGAF Enterprise Architecture principles, Luminous adheres to these foundational principles across all architecture domains:

### Business Principles

| Principle | Rationale | Implications |
|-----------|-----------|--------------|
| **BP-1: Family-Centric Design** | All features must serve real family coordination needs | Features are validated against family use cases; no feature creep toward entertainment |
| **BP-2: Zero Distraction** | The product differentiates by what it omits | Explicitly exclude entertainment, browsing, and social features |
| **BP-3: Inclusive by Default** | Families include members of all ages and abilities | Design for youngest (6+) and oldest users; accessibility is mandatory |
| **BP-4: Privacy as a Feature** | Families trust us with sensitive information | Data isolation per tenant; compliance with privacy regulations |

### Data Principles

| Principle | Rationale | Implications |
|-----------|-----------|--------------|
| **DP-1: Cloud-Native Storage** | Scalability and availability requirements | CosmosDB for global distribution; Azure storage for media |
| **DP-2: Tenant Data Isolation** | Multi-family security requirement | Partition strategies per family; no cross-tenant data access |
| **DP-3: Single Source of Truth** | Conflicts create confusion | Clear ownership rules per data type; deterministic conflict resolution |
| **DP-4: Data Minimization** | Collect only what's necessary | No tracking; minimal metadata; privacy by design |

### Application Principles

| Principle | Rationale | Implications |
|-----------|-----------|--------------|
| **AP-1: Modular Composability** | Features evolve independently | Bounded contexts; clean interfaces; microservices where appropriate |
| **AP-2: Cross-Platform Consistency** | Experience must be unified | Shared API; consistent behavior across display/mobile/web |
| **AP-3: Graceful Degradation** | Partial failures shouldn't block usage | Offline mode; fallback behaviors; error boundaries |
| **AP-4: Cloud-First, Local-Ready** | Support both deployment models | Design for Azure; enable local development |

### Technology Principles

| Principle | Rationale | Implications |
|-----------|-----------|--------------|
| **TP-1: Azure-Native Stack** | Enterprise-grade hosting with managed services | Use PaaS services; AVMs for IaC; Azure-native integrations |
| **TP-2: Platform-Native Mobile** | Best user experience on mobile devices | Swift for iOS; Kotlin for Android; shared API contracts |
| **TP-3: Standards-Based Integration** | Interoperability with existing systems | OAuth 2.0/OpenID Connect; CalDAV/ICS; REST APIs |
| **TP-4: Infrastructure as Code** | Reproducible deployments | Bicep with AVMs; environment parity |

---

## Architecture Vision

### Target State Architecture

```
+-----------------------------------------------------------------------------------+
|                           LUMINOUS CLOUD PLATFORM                                  |
+-----------------------------------------------------------------------------------+
|                                                                                    |
|  CLIENT TIER                                                                       |
|  +-------------------+  +-------------------+  +----------------+  +------------+  |
|  |   DISPLAY APP     |  |    iOS APP        |  |  ANDROID APP   |  |  WEB APP   |  |
|  |   (Angular/       |  |    (Swift/        |  |  (Kotlin/      |  |  (Angular) |  |
|  |    Electron)      |  |     SwiftUI)      |  |   Compose)     |  |            |  |
|  +--------+----------+  +--------+----------+  +-------+--------+  +-----+------+  |
|           |                      |                     |                 |         |
|           +----------------------+---------------------+-----------------+         |
|                                  |                                                 |
|  API TIER                        v                                                 |
|  +-------------------------------------------------------------------------+      |
|  |                    AZURE API MANAGEMENT                                  |      |
|  +--------------------------------+----------------------------------------+      |
|                                   |                                                |
|           +------------------+----+----+------------------+                        |
|           |                  |         |                  |                        |
|           v                  v         v                  v                        |
|  +-----------------+ +--------------+ +-------------+ +------------------+         |
|  |  APP SERVICE    | | FUNCTION APP | | FUNCTION APP| |   APP SERVICE    |         |
|  |  (Core API)     | | (Sync Jobs)  | | (Import)    | |  (SignalR)       |         |
|  |  .NET 10        | | .NET 10      | | .NET 10     | |  .NET 10         |         |
|  +-----------------+ +--------------+ +-------------+ +------------------+         |
|           |                  |               |                |                    |
|  DATA TIER                   |               |                |                    |
|  +---------------------------+---------------+----------------+--------------+     |
|  |                                                                           |     |
|  |  +----------------+  +------------------+  +----------------+             |     |
|  |  |   COSMOS DB    |  |  BLOB STORAGE    |  |  SERVICE BUS   |             |     |
|  |  |  (Core Data)   |  |  (Media/Files)   |  |  (Messaging)   |             |     |
|  |  +----------------+  +------------------+  +----------------+             |     |
|  |                                                                           |     |
|  |  +----------------+  +------------------+  +----------------+             |     |
|  |  |   REDIS CACHE  |  |  KEY VAULT       |  |  APP CONFIG    |             |     |
|  |  |  (Sessions)    |  |  (Secrets)       |  |  (Settings)    |             |     |
|  |  +----------------+  +------------------+  +----------------+             |     |
|  |                                                                           |     |
|  +---------------------------------------------------------------------------+     |
|                                                                                    |
|  IDENTITY TIER                                                                     |
|  +-------------------------------------------------------------------------+      |
|  |              IN-HOUSE IDENTITY (Passkeys / WebAuthn / OAuth)             |      |
|  +-------------------------------------------------------------------------+      |
|                                                                                    |
+------------------------------------------------------------------------------------+
```

### Key Architecture Characteristics

| Characteristic | Priority | Description |
|----------------|----------|-------------|
| **Scalability** | Critical | Multi-tenant; handle thousands of families |
| **Reliability** | Critical | 99.9% uptime; auto-recovery; geo-redundancy |
| **Security** | Critical | Tenant isolation; OAuth 2.0; encryption |
| **Usability** | Critical | Child-friendly; glanceable; intuitive |
| **Performance** | High | Fast API response; real-time sync |
| **Maintainability** | High | Clean code; comprehensive testing; IaC |

---

## Business Architecture

### Business Capability Model

```
LUMINOUS FAMILY HUB (MULTI-TENANT)
|
+-- PLATFORM CAPABILITY
|   +-- Tenant Management
|   +-- User Authentication (Passkeys/WebAuthn)
|   +-- Device Registration (Board Linking)
|   +-- Subscription Management
|   +-- Usage Analytics
|
+-- SCHEDULING CAPABILITY
|   +-- Calendar Aggregation
|   +-- Event Management
|   +-- Reminder Management
|   +-- Schedule Sharing
|
+-- TASK MANAGEMENT CAPABILITY
|   +-- Chore Management
|   +-- Routine Management
|   +-- Progress Tracking
|   +-- Reward Management
|
+-- HOUSEHOLD MANAGEMENT CAPABILITY
|   +-- Profile Management
|   +-- Meal Planning
|   +-- List Management
|   +-- Caregiver Coordination
|
+-- INFORMATION DISPLAY CAPABILITY
|   +-- Dashboard Rendering
|   +-- Notification Display
|   +-- Ambient Information
|   +-- Privacy Mode
|
+-- INTEGRATION CAPABILITY
    +-- Calendar Provider Integration
    +-- Push Notification Service
    +-- Magic Import Processing
    +-- External API Access
```

### Multi-Tenant Business Model

```
+---------------------------------------------------------------------+
|                      TENANT LIFECYCLE                                |
+---------------------------------------------------------------------+
|                                                                      |
|  +----------+    +----------+    +----------+    +----------+       |
|  | Sign Up  |--->| Create   |--->| Invite   |--->|  Link    |       |
|  | (User)   |    | Family   |    | Members  |    |  Boards  |       |
|  +----------+    +----------+    +----------+    +----------+       |
|                                                                      |
|  User registers --> Creates family --> Invites family --> Links     |
|  via Passkey/Email   (tenant)          members           display    |
|                                                                      |
+---------------------------------------------------------------------+
```

### Actor-Role Matrix

| Actor | Family Scope | Calendar | Chores | Lists | Profiles | Settings |
|-------|--------------|----------|--------|-------|----------|----------|
| **Family Owner** | Single | Full | Full | Full | Full | Full |
| **Admin Member** | Single | Full | Full | Full | Full | Limited |
| **Adult Member** | Single | Full | Full | Full | Own | View |
| **Teen Member** | Single | Own + Family | Own | Shared | Own | None |
| **Child Member** | Single | View | Complete | View | View | None |
| **External Caregiver** | Limited | View | View | View | View (care) | None |

---

## Data Architecture

### CosmosDB Data Model

```
+---------------------------------------------------------------------+
|                    COSMOS DB CONTAINERS                              |
+---------------------------------------------------------------------+
|                                                                      |
|  CONTAINER: families                                                 |
|  Partition Key: /id                                                  |
|  +---------------------------------------------------------------+  |
|  | { id, name, timezone, settings, createdAt, subscription }     |  |
|  +---------------------------------------------------------------+  |
|                                                                      |
|  CONTAINER: users                                                    |
|  Partition Key: /familyId                                            |
|  +---------------------------------------------------------------+  |
|  | { id, familyId, email, displayName, role, profile, ... }      |  |
|  +---------------------------------------------------------------+  |
|                                                                      |
|  CONTAINER: events                                                   |
|  Partition Key: /familyId                                            |
|  +---------------------------------------------------------------+  |
|  | { id, familyId, title, start, end, assignees, source, ... }   |  |
|  +---------------------------------------------------------------+  |
|                                                                      |
|  CONTAINER: chores                                                   |
|  Partition Key: /familyId                                            |
|  +---------------------------------------------------------------+  |
|  | { id, familyId, title, assignees, recurrence, points, ... }   |  |
|  +---------------------------------------------------------------+  |
|                                                                      |
|  CONTAINER: devices                                                  |
|  Partition Key: /familyId                                            |
|  +---------------------------------------------------------------+  |
|  | { id, familyId, deviceType, linkCode, linkedAt, ... }         |  |
|  +---------------------------------------------------------------+  |
|                                                                      |
|  (Additional: lists, meals, routines, completions, ...)             |
|                                                                      |
+---------------------------------------------------------------------+
```

### Core Entities

#### Family (Tenant Root)

```csharp
// Uses NanoId for URL-friendly, compact unique identifiers
public class Family
{
    public string Id { get; set; } = Nanoid.Generate();  // NanoId: compact, URL-safe
    public string Name { get; set; } = string.Empty;
    public string Timezone { get; set; } = "UTC";
    public FamilySettings Settings { get; set; } = new();
    public SubscriptionInfo? Subscription { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string CreatedBy { get; set; } = string.Empty;
}

public class FamilySettings
{
    public string DefaultView { get; set; } = "day";
    public bool PrivacyModeEnabled { get; set; } = true;
    public TimeSpan PrivacyModeTimeout { get; set; } = TimeSpan.FromMinutes(5);
    public SleepModeSettings SleepMode { get; set; } = new();
}
```

#### User (Family Member)

```csharp
public class User
{
    public string Id { get; set; } = Nanoid.Generate();  // NanoId: compact, URL-safe
    public string FamilyId { get; set; } = string.Empty;  // Partition key
    public string ExternalId { get; set; } = string.Empty; // Internal auth ID
    public string? Email { get; set; }                    // Null for managed accounts
    public string DisplayName { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.Member;
    public UserProfile Profile { get; set; } = new();
    public CaregiverInfo? CaregiverInfo { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Managed Account Support (see ADR-016)
    public AccountType AccountType { get; set; } = AccountType.Full;
    public string? ManagedById { get; set; }              // Parent's user ID (null for full accounts)
    public string? ProfilePinHash { get; set; }          // Argon2id hashed PIN (null if not set)
    public bool PinEnabled { get; set; } = false;
    public DateTime? PinLastChanged { get; set; }
    public int PinFailedAttempts { get; set; } = 0;
    public DateTime? PinLockedUntil { get; set; }
}

public enum UserRole
{
    Owner,      // Can delete family, manage billing
    Admin,      // Full access except billing
    Adult,      // Full feature access
    Teen,       // Limited access
    Child,      // View and complete only
    Caregiver   // External, view only
}

public enum AccountType
{
    Full,        // Has email, can self-authenticate (passkey, email OTP, social, password)
    Managed,     // No email required, parent-controlled auth (PIN, device passkey, parent QR)
    ProfileOnly  // No authentication, display representation only (pets, very young children)
}
```

#### Managed Device (Child Device Authorization)

For devices authorized to access managed accounts (see ADR-016):

```csharp
public class ManagedDevice
{
    public string Id { get; set; } = Nanoid.Generate();
    public string FamilyId { get; set; } = string.Empty;      // Partition key
    public string ManagedAccountId { get; set; } = string.Empty;  // The child's account
    public string AuthorizedById { get; set; } = string.Empty;    // Parent who authorized
    public string DeviceIdentifier { get; set; } = string.Empty;  // Device fingerprint
    public string? DeviceName { get; set; }                       // "Jordan's iPad"
    public ManagedDeviceType Type { get; set; }
    public string? PasskeyCredentialId { get; set; }              // If device passkey method used
    public DateTime AuthorizedAt { get; set; }
    public DateTime? LastUsedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }                      // Optional expiration
    public bool IsRevoked { get; set; } = false;
}

public enum ManagedDeviceType
{
    SharedDisplay,     // Family display, PIN auth
    PersonalMobile,    // Child's phone/tablet, passkey auth
    PersonalWeb,       // Child's browser, passkey or session auth
    TemporarySession   // QR-code authorized session
}
```

#### Device (Linked Board)

```csharp
public class Device
{
    public string Id { get; set; } = Nanoid.Generate();  // NanoId: compact, URL-safe
    public string FamilyId { get; set; } = string.Empty;  // Partition key
    public DeviceType Type { get; set; } = DeviceType.Display;
    public string Name { get; set; } = string.Empty;
    public string? LinkCode { get; set; }  // 6-digit code for linking
    public DateTime? LinkCodeExpiry { get; set; }
    public DateTime? LinkedAt { get; set; }
    public string? LinkedBy { get; set; }
    public DeviceSettings Settings { get; set; } = new();
    public DateTime LastSeenAt { get; set; }
}

public enum DeviceType
{
    Display,    // Wall-mounted display
    Mobile,     // iOS or Android app
    Web         // Web browser
}
```

### Data Storage Strategy

| Data Type | Storage | Partition Strategy | Notes |
|-----------|---------|-------------------|-------|
| **Families** | CosmosDB | By family ID | Tenant root |
| **Users** | CosmosDB | By family ID | Includes auth credentials link, account type |
| **Events** | CosmosDB | By family ID | Calendar events |
| **Chores** | CosmosDB | By family ID | Tasks and assignments |
| **Devices** | CosmosDB | By family ID | Board registrations (display linking) |
| **ManagedDevices** | CosmosDB | By family ID | Child device authorizations (ADR-016) |
| **Media** | Blob Storage | By family ID container | Avatars, recipe photos |
| **Sessions** | Redis Cache | By session ID | Real-time sync state |
| **Secrets** | Key Vault | N/A | API keys, certificates |

---

## Application Architecture

### Bounded Contexts

Following Domain-Driven Design, Luminous is organized into these bounded contexts:

| Context | Entities | Responsibility |
|---------|----------|----------------|
| **Identity** | Family, User, Device, Invitation | Authentication, authorization, multi-tenancy |
| **Scheduling** | Calendar, Event, CalendarConnection, Reminder | Calendar aggregation and management |
| **Task Management** | Chore, Routine, Completion, Reward | Chores, routines, and gamification |
| **Meal Planning** | MealPlan, Recipe, Ingredient, DietaryPref | Meal planning and recipes |
| **List Management** | List, ListItem, Template | Grocery and custom lists |
| **Display** | Dashboard, Widget, PrivacyMode, DeviceConfig | Display rendering and modes |
| **Magic Import** | ImportRequest, ParsedContent, ApprovalQueue | AI-powered content extraction |
| **Notification** | Notification, PushToken, Preference | Push notifications |

### Solution Structure

```
luminous/
|
+-- src/
|   +-- Luminous.Domain/              # Domain models, interfaces
|   +-- Luminous.Application/         # Application services, CQRS
|   +-- Luminous.Infrastructure/      # Data access, external services
|   +-- Luminous.Api/                 # ASP.NET Core Web API
|   +-- Luminous.Functions/           # Azure Functions
|   +-- Luminous.Shared/              # Shared DTOs, contracts
|
+-- clients/
|   +-- shared/                       # Shared Angular library (see below)
|   |   +-- models/                   # TypeScript interfaces (API contracts)
|   |   +-- services/                 # Common services (API client, state)
|   |   +-- components/               # Reusable UI components
|   |   +-- utils/                    # Shared utilities
|   |   +-- styles/                   # Design tokens, base styles
|   +-- web/                          # Angular web application
|   +-- display/                      # Angular + Electron display app
|   +-- ios/                          # Native iOS app (Swift)
|   +-- android/                      # Native Android app (Kotlin)
|
+-- design-tokens/                    # Single source of truth for design tokens
|   +-- tokens.json                   # Master token definitions
|   +-- build/                        # Generated outputs
|       +-- css/                      # CSS custom properties
|       +-- swift/                    # Swift extensions
|       +-- kotlin/                   # Kotlin constants
|
+-- infra/
|   +-- bicep/                        # Bicep IaC with AVMs
|   |   +-- main.bicep
|   |   +-- modules/
|   |   +-- parameters/
|   +-- scripts/
|
+-- tests/
|   +-- Luminous.Domain.Tests/
|   +-- Luminous.Application.Tests/
|   +-- Luminous.Api.Tests/
|   +-- Luminous.Integration.Tests/
|
+-- docs/
|
+-- Luminous.sln
```

### Cross-Platform Client Architecture

Luminous supports four client platforms with a strategy to maximize code sharing while maintaining platform-optimal experiences.

#### Platform Philosophy

| Platform | Primary Purpose | Optimization Focus |
|----------|-----------------|-------------------|
| **Web App** | Admin console, desktop-first management | Complex tasks, keyboard input, large screens |
| **Display App** | Always-on family hub, glanceable info | Touch, readability, 24/7 operation |
| **iOS App** | On-the-go management, notifications | Native UX, push notifications, widgets |
| **Android App** | On-the-go management, notifications | Native UX, push notifications, widgets |

#### Feature Parity Strategy

All platforms support all features, but each is optimized for its context:

```
┌─────────────────────────────────────────────────────────────────────────┐
│                        FEATURE PARITY MODEL                              │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                          │
│  FULL FEATURE ACCESS (All Platforms)                                     │
│  ├── Calendar views (day, week, month, agenda)                          │
│  ├── Event creation and editing                                          │
│  ├── Task/chore management                                               │
│  ├── Family member management                                            │
│  ├── Profile management                                                  │
│  ├── List management                                                     │
│  └── Settings and preferences                                            │
│                                                                          │
│  PLATFORM-OPTIMIZED FEATURES                                             │
│  ├── Web: Complex scheduling, bulk operations, calendar OAuth setup      │
│  ├── Display: Glanceable views, kiosk mode, ambient information         │
│  ├── Mobile: Push notifications, widgets, biometric auth, quick actions │
│  └── Mobile: Device linking via code entry                               │
│                                                                          │
│  PLATFORM-EXCLUSIVE FEATURES                                             │
│  ├── Display: Kiosk mode, watchdog, auto-start                          │
│  ├── iOS: APNs push, iOS widgets, ShareSheet                            │
│  └── Android: FCM push, Android widgets, app shortcuts                  │
│                                                                          │
└─────────────────────────────────────────────────────────────────────────┘
```

#### Phased Feature Rollout

New features follow a phased rollout to reduce development burden:

```
Week 1: Web App (fastest iteration, validation)
    ↓
Week 2: Display App (shared Angular code, ~70% reuse)
    ↓
Week 3: iOS App (native implementation)
    ↓
Week 4: Android App (native implementation)
```

This approach allows:
- Early validation of features on web before native investment
- Shared code between web and display via `clients/shared/`
- Consistent API contracts via OpenAPI-generated clients

#### Shared Angular Library (`clients/shared/`)

The web and display apps share common code via an Angular library:

```
clients/shared/
├── ng-package.json              # Angular library configuration
├── src/
│   ├── lib/
│   │   ├── models/              # TypeScript interfaces
│   │   │   ├── user.model.ts
│   │   │   ├── family.model.ts
│   │   │   ├── event.model.ts
│   │   │   ├── device.model.ts
│   │   │   └── index.ts
│   │   │
│   │   ├── services/            # Common services
│   │   │   ├── api.service.ts           # Base HTTP client
│   │   │   ├── auth.service.ts          # Authentication state
│   │   │   ├── family.service.ts        # Family operations
│   │   │   ├── event.service.ts         # Calendar events
│   │   │   ├── storage.service.ts       # Local storage abstraction
│   │   │   ├── canvas.service.ts        # Time-based theming
│   │   │   └── index.ts
│   │   │
│   │   ├── components/          # Reusable UI components
│   │   │   ├── avatar/
│   │   │   ├── button/
│   │   │   ├── card/
│   │   │   ├── input/
│   │   │   ├── spinner/
│   │   │   ├── alert/
│   │   │   ├── toast/
│   │   │   └── index.ts
│   │   │
│   │   ├── interceptors/        # HTTP interceptors
│   │   │   ├── auth.interceptor.ts
│   │   │   ├── error.interceptor.ts
│   │   │   └── index.ts
│   │   │
│   │   └── utils/               # Shared utilities
│   │       ├── date.utils.ts
│   │       ├── validation.utils.ts
│   │       └── index.ts
│   │
│   └── public-api.ts            # Library exports
```

**Estimated Code Sharing:**
- Models: 100% shared
- Services: 80% shared (display has device-specific services)
- Components: 70% shared (display has larger variants)
- Interceptors: 90% shared

#### OpenAPI Client Generation

API clients are generated from the OpenAPI specification to ensure type safety and contract synchronization:

```
┌─────────────────────────────────────────────────────────────────────────┐
│                      OPENAPI CLIENT GENERATION                           │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                          │
│  Luminous.Api (ASP.NET Core)                                            │
│       │                                                                  │
│       ▼                                                                  │
│  openapi.json (generated at build)                                      │
│       │                                                                  │
│       ├────────────────┬────────────────┬────────────────┐              │
│       ▼                ▼                ▼                ▼              │
│  TypeScript         Swift           Kotlin          C# Client           │
│  (Angular)          (iOS)           (Android)       (Testing)           │
│                                                                          │
│  Tools:                                                                  │
│  - TypeScript: openapi-typescript + openapi-fetch                       │
│  - Swift: swift-openapi-generator                                       │
│  - Kotlin: openapi-generator (kotlin)                                   │
│                                                                          │
└─────────────────────────────────────────────────────────────────────────┘
```

**CI/CD Integration:**
```yaml
# .github/workflows/api-clients.yml
on:
  push:
    paths:
      - 'src/Luminous.Api/**'

jobs:
  generate-clients:
    steps:
      - name: Build API and export OpenAPI spec
        run: dotnet build && dotnet run --project src/Luminous.Api -- --export-openapi

      - name: Generate TypeScript client
        run: npx openapi-typescript openapi.json -o clients/shared/src/lib/api/schema.d.ts

      - name: Generate Swift client
        run: swift-openapi-generator generate openapi.json --output clients/ios/Generated

      - name: Generate Kotlin client
        run: openapi-generator generate -i openapi.json -g kotlin -o clients/android/generated
```

#### Design Token Export

Design tokens are defined once and exported to all platforms:

```
┌─────────────────────────────────────────────────────────────────────────┐
│                       DESIGN TOKEN PIPELINE                              │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                          │
│  design-tokens/tokens.json (Single Source of Truth)                     │
│       │                                                                  │
│       ▼                                                                  │
│  Style Dictionary (build tool)                                          │
│       │                                                                  │
│       ├─────────────────┬─────────────────┬─────────────────┐           │
│       ▼                 ▼                 ▼                 ▼           │
│  CSS Variables      Swift Extensions   Kotlin Object    Tailwind        │
│  (Angular)          (iOS)              (Android)        Config          │
│                                                                          │
│  Output Examples:                                                        │
│                                                                          │
│  CSS:    --color-accent-500: #0EA5E9;                                   │
│  Swift:  static let accent500 = Color(hex: "0EA5E9")                    │
│  Kotlin: val accent500 = Color(0xFF0EA5E9)                              │
│                                                                          │
└─────────────────────────────────────────────────────────────────────────┘
```

**Token Categories:**
| Category | Examples | Platforms |
|----------|----------|-----------|
| Colors | Canvas, surface, text, accent, member colors | All |
| Spacing | 4px base unit scale | All |
| Typography | Font sizes, weights, line heights | All |
| Radii | Border radius scale | All |
| Shadows | Elevation scale | Web, Display |
| Motion | Duration, easing | Web, Display |
| Touch | Minimum touch targets | All |

---

## Technology Architecture

### Technology Stack

| Layer | Technology | Version | Rationale |
|-------|------------|---------|-----------|
| **Backend Runtime** | .NET | 10 | LTS, performance, Azure integration |
| **Backend Framework** | ASP.NET Core | 10 | Web API, SignalR, middleware |
| **Serverless** | Azure Functions | .NET 10 isolated | Event-driven processing |
| **Web Frontend** | Angular | 19+ | Enterprise-grade, TypeScript-first |
| **Display App** | Angular + Electron | Latest | Cross-platform desktop |
| **iOS App** | Swift/SwiftUI | iOS 17+ | Native performance, UX |
| **Android App** | Kotlin/Compose | Android 13+ | Native performance, UX |
| **Database** | Azure Cosmos DB | Latest | Global scale, multi-model |
| **Cache** | Azure Redis Cache | Latest | Session, real-time state |
| **Storage** | Azure Blob Storage | Latest | Media, files |
| **Messaging** | Azure Service Bus | Latest | Async processing |
| **Real-time** | Azure SignalR Service | Latest | WebSocket at scale |
| **Identity** | In-house (FIDO2/WebAuthn) | Latest | Passwordless-first auth |
| **IaC** | Bicep + AVMs | Latest | Azure-native, verified modules |
| **Identifiers** | NanoId | 3.1.0 | URL-friendly, compact unique IDs |
| **OpenAPI** | ASP.NET Core OpenAPI | 10.0.1 | Native document generation |

### Platform Matrix

| Platform | Technology | Distribution |
|----------|------------|--------------|
| **Web App** | Angular, Static Web App | Azure Static Web Apps |
| **Display App** | Angular + Electron | Direct download, auto-update |
| **iOS App** | Swift/SwiftUI | Apple App Store |
| **Android App** | Kotlin/Compose | Google Play Store |
| **API** | .NET 10, App Service | Azure App Service |
| **Functions** | .NET 10 isolated | Azure Functions |

---

## Security Architecture

### Identity and Access Management

```
+---------------------------------------------------------------------+
|                   PASSWORDLESS IDENTITY ARCHITECTURE                 |
+---------------------------------------------------------------------+
|                                                                      |
|  +---------------------------------------------------------------+  |
|  |               IN-HOUSE IDENTITY SERVICE                        |  |
|  |                                                                |  |
|  |  PRIMARY (PASSWORDLESS)           FALLBACK                     |  |
|  |  +--------------+  +--------------+  +--------------+          |  |
|  |  |   Passkeys   |  |  Email OTP   |  |   Password   |          |  |
|  |  |  (WebAuthn)  |  | (Magic Link) |  |  (+ MFA)     |          |  |
|  |  +--------------+  +--------------+  +--------------+          |  |
|  |                                                                |  |
|  |  ADDITIONAL METHODS                                            |  |
|  |  +--------------+  +--------------+  +--------------+          |  |
|  |  |  Hardware    |  |   Social     |  |    TOTP      |          |  |
|  |  |   Tokens     |  |  (Google,    |  |   (MFA)      |          |  |
|  |  | (YubiKey)    |  |   Apple)     |  |              |          |  |
|  |  +--------------+  +--------------+  +--------------+          |  |
|  |                                                                |  |
|  +---------------------------------------------------------------+  |
|                              |                                       |
|                              v                                       |
|  +---------------------------------------------------------------+  |
|  |                    JWT ACCESS TOKEN                            |  |
|  |  { sub, family_id, role, auth_method, mfa_verified, exp, ... } |  |
|  +---------------------------------------------------------------+  |
|                              |                                       |
|                              v                                       |
|  +---------------------------------------------------------------+  |
|  |                    API AUTHORIZATION                           |  |
|  |                                                                |  |
|  |  [Authorize(Policy = "FamilyMember")]                          |  |
|  |  [Authorize(Policy = "FamilyAdmin")]                           |  |
|  |  [Authorize(Policy = "DeviceAccess")]                          |  |
|  |                                                                |  |
|  +---------------------------------------------------------------+  |
|                                                                      |
+---------------------------------------------------------------------+
```

### Secure User Registration Flow

New user registration uses a secure 2-step process with email verification to prevent identity impersonation:

```
+---------------------------------------------------------------------+
|                   SECURE REGISTRATION FLOW                           |
+---------------------------------------------------------------------+
|                                                                      |
|  STEP 1: Registration Start                                          |
|  POST /api/auth/register/start                                       |
|  +---------------------------------------------------------------+  |
|  | Client sends: email, displayName, inviteCode (optional)       |  |
|  | Server:                                                        |  |
|  |   1. Validates email is not already registered                |  |
|  |   2. Stores email in server-side session (NOT trusted later)  |  |
|  |   3. Generates 6-digit OTP                                    |  |
|  |   4. Sends OTP to email address                               |  |
|  |   5. Returns session ID                                       |  |
|  +---------------------------------------------------------------+  |
|                              |                                       |
|                              v                                       |
|  STEP 2: OTP Verification                                            |
|  POST /api/auth/otp/verify                                           |
|  +---------------------------------------------------------------+  |
|  | Client sends: email, code                                      |  |
|  | Server validates OTP and marks email as verified in session    |  |
|  +---------------------------------------------------------------+  |
|                              |                                       |
|                              v                                       |
|  STEP 3: Registration Complete                                       |
|  POST /api/auth/register/complete                                    |
|  +---------------------------------------------------------------+  |
|  | Client sends: passkey attestation (NO email - security!)       |  |
|  | Server:                                                        |  |
|  |   1. Retrieves verified email from session (server-side)      |  |
|  |   2. Creates user with email from session                     |  |
|  |   3. Creates family or joins via invite code                  |  |
|  |   4. Stores passkey credential                                |  |
|  |   5. Issues JWT tokens                                        |  |
|  +---------------------------------------------------------------+  |
|                                                                      |
|  SECURITY NOTES:                                                     |
|  - Email is stored server-side in session, never trusted from client|
|  - OTP must be verified before registration can complete             |
|  - Invite codes allow joining existing families                      |
|  - Prevents attackers from registering with someone else's email     |
|                                                                      |
+---------------------------------------------------------------------+
```

### Device Linking Flow

1. **Display requests link code** - Generates 6-digit code with 15-minute expiry
2. **User opens mobile app** - Logs in with passkey or other method
3. **User enters code** - Associates device with their family
4. **Display receives confirmation** - Gets device token for API access
5. **Display syncs family data** - Pulls events, chores, settings

### Security Requirements

| Category | Requirement | Implementation |
|----------|-------------|----------------|
| **Authentication** | Passwordless-first | Passkeys (WebAuthn), Email OTP, Social OAuth |
| **MFA** | Required for password users | TOTP, hardware tokens, passkey as 2FA |
| **Authorization** | Role-based + Family-scoped | JWT claims, custom policies |
| **Data Isolation** | Tenant data separation | CosmosDB partitioning |
| **Data Encryption** | At rest and in transit | Azure managed encryption, TLS 1.3 |
| **Credential Storage** | Secure key storage | Argon2id hashing, encrypted WebAuthn keys |
| **Secrets** | Secure secret storage | Azure Key Vault |
| **API Security** | Rate limiting, validation | API Management + middleware |
| **Device Auth** | Secure device tokens | JWT with refresh rotation |

---

## Azure Cloud Architecture

### Resource Architecture

```
RESOURCE GROUP: rg-luminous-{env}
|
+-- COMPUTE
|   +-- App Service (API): app-lum-api-{env}
|   +-- Function App (Sync): func-lum-sync-{env}
|   +-- Function App (Import): func-lum-import-{env}
|
+-- DATA
|   +-- Cosmos DB: cosmos-lum-{env}
|   +-- Redis Cache: redis-lum-{env}
|   +-- Storage Account: stlum{env}
|
+-- MESSAGING
|   +-- Service Bus: sb-lum-{env}
|   +-- SignalR Service: sigr-lum-{env}
|
+-- SECURITY
|   +-- Key Vault: kv-lum-{env}
|   +-- App Configuration: appcs-lum-{env}
|
+-- WEB
|   +-- Static Web App: stapp-lum-{env}
|
+-- MONITORING
    +-- Application Insights: appi-lum-{env}
    +-- Log Analytics: log-lum-{env}
```

### Bicep with Azure Verified Modules (AVMs)

```bicep
// main.bicep - Using Azure Verified Modules
targetScope = 'subscription'

@description('Environment name')
@allowed(['dev', 'stg', 'prd'])
param environment string

@description('Azure region')
param location string = 'eastus2'

// Resource Group
module resourceGroup 'br/public:avm/res/resources/resource-group:0.2.0' = {
  name: 'rg-luminous-${environment}'
  params: {
    name: 'rg-luminous-${environment}'
    location: location
  }
}

// Cosmos DB using AVM
module cosmosDb 'br/public:avm/res/document-db/database-account:0.6.0' = {
  name: 'cosmos-luminous-${environment}'
  scope: resourceGroup
  params: {
    name: 'cosmos-lum-${environment}'
    location: location
    sqlDatabases: [
      {
        name: 'luminous'
        containers: [
          { name: 'families', partitionKeyPath: '/id' }
          { name: 'users', partitionKeyPath: '/familyId' }
          { name: 'events', partitionKeyPath: '/familyId' }
          { name: 'chores', partitionKeyPath: '/familyId' }
          { name: 'devices', partitionKeyPath: '/familyId' }
        ]
      }
    ]
  }
}

// App Service using AVM
module appService 'br/public:avm/res/web/site:0.3.0' = {
  name: 'api-luminous-${environment}'
  scope: resourceGroup
  params: {
    name: 'app-lum-api-${environment}'
    location: location
    kind: 'app'
    serverFarmResourceId: appServicePlan.outputs.resourceId
  }
}

// Additional modules...
```

### Environment Strategy

| Environment | Purpose | Resources |
|-------------|---------|-----------|
| **dev** | Development | Minimal SKUs, single region |
| **stg** | Pre-production testing | Production-like, single region |
| **prd** | Production | Full SKUs, multi-region optional |

---

## Multi-Tenancy Architecture

### Tenant Isolation Strategy

| Layer | Isolation Method | Description |
|-------|-----------------|-------------|
| **Data** | CosmosDB Partitioning | Each family's data in separate logical partition |
| **Storage** | Container per family | Blob storage organized by family ID |
| **API** | JWT Claims | Family ID in token, validated on every request |
| **SignalR** | Groups | Real-time updates scoped to family group |

### Family Onboarding Flow

1. **User Registration** - Sign up with passkey, email OTP, or social login
2. **Family Creation** - POST /api/families creates tenant
3. **Profile Setup** - Add family member profiles
4. **Calendar Connection** - OAuth to Google/Outlook
5. **Device Linking** - Link wall display to family

---

## Local Development Architecture

### Local Development Stack

```
+---------------------------------------------------------------------+
|                    LOCAL DEVELOPMENT ENVIRONMENT                     |
+---------------------------------------------------------------------+
|                                                                      |
|  EMULATORS / LOCAL SERVICES                                          |
|  +---------------------------------------------------------------+  |
|  |  +----------------+  +----------------+  +----------------+    |  |
|  |  | Cosmos DB      |  | Azurite        |  | Local Redis    |    |  |
|  |  | Emulator       |  | (Blob/Queue)   |  | (Docker)       |    |  |
|  |  +----------------+  +----------------+  +----------------+    |  |
|  +---------------------------------------------------------------+  |
|                                                                      |
|  DEVELOPMENT SERVERS                                                 |
|  +---------------------------------------------------------------+  |
|  |  +----------------+  +----------------+  +----------------+    |  |
|  |  | .NET API       |  | Angular        |  | Electron       |    |  |
|  |  | (Kestrel)      |  | (ng serve)     |  | (dev mode)     |    |  |
|  |  | :5000          |  | :4200          |  |                |    |  |
|  |  +----------------+  +----------------+  +----------------+    |  |
|  +---------------------------------------------------------------+  |
|                                                                      |
+---------------------------------------------------------------------+
```

### Docker Compose for Local Development

```yaml
# docker-compose.yml
version: '3.8'

services:
  cosmosdb:
    image: mcr.microsoft.com/cosmosdb/linux/azure-cosmos-emulator:latest
    ports:
      - "8081:8081"
      - "10251-10254:10251-10254"
    environment:
      - AZURE_COSMOS_EMULATOR_PARTITION_COUNT=10
      - AZURE_COSMOS_EMULATOR_ENABLE_DATA_PERSISTENCE=true

  redis:
    image: redis:7-alpine
    ports:
      - "6379:6379"

  azurite:
    image: mcr.microsoft.com/azure-storage/azurite:latest
    ports:
      - "10000:10000"  # Blob
      - "10001:10001"  # Queue
      - "10002:10002"  # Table
```

---

## Architecture Decisions

### ADR Index

| ADR | Title | Status | Date |
|-----|-------|--------|------|
| [ADR-001](./adr/ADR-001-dotnet-backend.md) | .NET 10 as Backend Platform | Accepted | 2025-12-21 |
| [ADR-002](./adr/ADR-002-angular-web-framework.md) | Angular as Web Framework | Accepted | 2025-12-21 |
| [ADR-003](./adr/ADR-003-azure-cloud-platform.md) | Azure as Cloud Platform | Accepted | 2025-12-21 |
| [ADR-004](./adr/ADR-004-native-mobile-apps.md) | Native iOS and Android Apps | Accepted | 2025-12-21 |
| [ADR-005](./adr/ADR-005-cosmosdb-data-store.md) | CosmosDB as Primary Data Store | Accepted | 2025-12-21 |
| [ADR-006](./adr/ADR-006-multi-tenant-architecture.md) | Multi-Tenant Architecture | Accepted | 2025-12-21 |
| [ADR-007](./adr/ADR-007-bicep-avm-iac.md) | Bicep with AVMs for IaC | Accepted | 2025-12-21 |
| [ADR-008](./adr/ADR-008-magic-import-approval.md) | Magic Import Requires Approval | Accepted | 2025-12-21 |
| [ADR-009](./adr/ADR-009-zero-distraction-principle.md) | Zero-Distraction Design Principle | Accepted | 2025-12-21 |
| [ADR-010](./adr/ADR-010-passwordless-authentication.md) | In-House Passwordless Auth | Accepted | 2025-12-21 |
| [ADR-011](./adr/ADR-011-secure-registration-flow.md) | Secure Registration with Email Verification | Accepted | 2025-12-28 |
| [ADR-012](./adr/ADR-012-native-openapi.md) | Native ASP.NET Core OpenAPI | Accepted | 2025-12-28 |
| [ADR-013](./adr/ADR-013-shared-angular-library.md) | Shared Angular Library for Web/Display | Accepted | 2026-01-08 |
| [ADR-014](./adr/ADR-014-cross-platform-feature-parity.md) | Cross-Platform Feature Parity Strategy | Accepted | 2026-01-08 |
| [ADR-015](./adr/ADR-015-design-token-pipeline.md) | Design Token Export Pipeline | Accepted | 2026-01-08 |
| [ADR-016](./adr/ADR-016-managed-accounts.md) | Managed Accounts for Children/Non-Email Users | Accepted | 2026-01-08 |

---

## Related Documents

- [Project Overview](./PROJECT-OVERVIEW.md)
- [Development Roadmap](./ROADMAP.md)
- [Architecture Decision Records](./adr/)
- [Azure Infrastructure](./AZURE-INFRASTRUCTURE.md)
- [CI/CD Pipeline](./CI-CD.md)
- [CLAUDE.md (Development Guidelines)](../CLAUDE.md)

---

## Document History

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0.0 | 2025-12-21 | Luminous Team | Initial architecture document |
| 2.0.0 | 2025-12-21 | Luminous Team | Updated for Azure/.NET/Angular stack, multi-tenancy |
| 2.1.0 | 2025-12-21 | Luminous Team | Migrate from Guid to NanoId for unique identifiers |
| 2.2.0 | 2025-12-22 | Luminous Team | Added CI/CD documentation reference |
| 2.3.0 | 2025-12-23 | Luminous Team | Phase 0 complete: Updated status to Active |
| 2.4.0 | 2025-12-28 | Luminous Team | Added secure registration flow, native OpenAPI |
| 2.5.0 | 2026-01-08 | Luminous Team | Added cross-platform client architecture, shared Angular library, OpenAPI client generation, design token pipeline |
| 2.6.0 | 2026-01-08 | Luminous Team | Added managed accounts data model for children/non-email users (ADR-016) |
