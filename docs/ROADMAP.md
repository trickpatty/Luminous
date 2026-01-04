# Luminous Development Roadmap

> **Document Version:** 3.3.0
> **Last Updated:** 2026-01-01
> **Status:** Active
> **TOGAF Phase:** Phase E/F (Opportunities, Solutions & Migration Planning)

---

## Table of Contents

1. [Roadmap Overview](#roadmap-overview)
2. [Implementation Phases](#implementation-phases)
3. [Phase 0: Foundation](#phase-0-foundation)
4. [Phase 1: Core Platform](#phase-1-core-platform)
5. [Phase 2: Display & Calendar](#phase-2-display--calendar)
6. [Phase 3: Native Mobile Apps](#phase-3-native-mobile-apps)
7. [Phase 4: Task Management](#phase-4-task-management)
8. [Phase 5: Household Features](#phase-5-household-features)
9. [Phase 6: Intelligence & Ecosystem](#phase-6-intelligence--ecosystem)
10. [Future Vision](#future-vision)
11. [Dependency Map](#dependency-map)
12. [Risk Register](#risk-register)

---

## Roadmap Overview

### Strategic Themes

| Theme | Description | Primary Phases |
|-------|-------------|----------------|
| **Foundation** | Establish architecture, tooling, and core infrastructure | Phase 0 |
| **Visibility** | Make family schedules and tasks visible at a glance | Phase 1-2 |
| **Mobility** | Enable management and notifications on the go | Phase 3 |
| **Coordination** | Expand to meal planning, lists, and caregiver sharing | Phase 4 |
| **Intelligence** | Add AI-powered features (Magic Import, suggestions) | Phase 5 |
| **Ecosystem** | Platform extensions, integrations, community | Phase 6+ |

### Release Philosophy

Following TOGAF's principle of iterative architecture development:

1. **Minimum Viable Product (MVP)**: Phases 0-1 deliver a functional calendar display
2. **Minimum Lovable Product (MLP)**: Phase 2 adds task management and rewards
3. **Full Product**: Phases 3-4 complete the companion apps and household features
4. **Platform**: Phases 5-6 enable extensibility and community contributions

### Architecture Building Blocks by Phase

```
Phase 0: Foundation
+-- Azure Infrastructure (Bicep/AVMs)
+-- .NET 10 Solution Structure
+-- Angular Web Application Shell
+-- Passwordless Authentication Setup
+-- Local Development Environment

Phase 1: Core Platform
+-- Multi-Tenant API
+-- CosmosDB Data Layer
+-- User Registration & Family Creation
+-- Device Linking Flow
+-- Web Dashboard MVP

Phase 2: Display & Calendar
+-- Display Application (Angular + Electron)
+-- Calendar Integration (Google, Outlook)
+-- Event Display Widgets
+-- Real-time Sync (SignalR)

Phase 3: Native Mobile Apps
+-- iOS App (Swift/SwiftUI)
+-- Android App (Kotlin/Compose)
+-- Push Notification Service
+-- Mobile-specific UX

Phase 4: Task Management
+-- Chore/Task Context
+-- Routine Context
+-- Rewards System
+-- Progress Visualization

Phase 5: Household Features
+-- Profile Management
+-- Meal Planning
+-- List Management
+-- Caregiver Portal

Phase 6: Intelligence & Ecosystem
+-- Magic Import Service
+-- AI Suggestions
+-- Third-Party Integrations
+-- API for Extensions
```

---

## Implementation Phases

### Phase Summary

| Phase | Name | Focus | Key Deliverables | Status |
|-------|------|-------|------------------|--------|
| **0** | Foundation | Infrastructure | Azure IaC, .NET solution, Angular shell, Passwordless Auth, Local Dev, CI/CD, Docs | ✅ Complete |
| **1** | Core Platform | Multi-tenancy | Family sign-up, device linking, CosmosDB, web MVP | ✅ Complete |
| **2** | Display & Calendar | Calendar visibility | Display app, calendar integration, SignalR sync | 🔄 In Progress |
| **3** | Native Mobile | Mobile apps | iOS (Swift), Android (Kotlin), push notifications | ⬜ Not Started |
| **4** | Task Management | Chores and routines | Task creation, completion tracking, rewards | ⬜ Not Started |
| **5** | Household Features | Expanded features | Profiles, meals, lists, caregiver portal | ⬜ Not Started |
| **6** | Intelligence & Ecosystem | AI and extensions | Magic import, suggestions, third-party APIs | ⬜ Not Started |

---

## Phase 0: Foundation

### Objective

Establish the Azure infrastructure, .NET backend, Angular frontend, and development environment that all subsequent phases will build upon.

### Scope

#### 0.1 Azure Infrastructure (Bicep with AVMs) ✅ COMPLETED

- [x] **0.1.1** Create Bicep modules for all Azure resources using AVMs
  - *Implemented: main.bicep using 13 AVMs directly from the public Bicep registry (br/public:avm/res/...)*
  - *Resources: resource-group, log-analytics, app-insights, key-vault, app-configuration, cosmos-db, storage-account, redis-cache, service-bus, signalr, app-service-plan, web-site, static-web-app*
- [x] **0.1.2** Configure Cosmos DB with required containers
  - *Implemented: 13 containers via AVM (families, users, events, chores, devices, routines, lists, meals, completions, invitations, credentials, otptokens, refreshtokens)*
- [x] **0.1.3** Set up in-house identity service with WebAuthn/passkey support
  - *Implemented: credentials container in Cosmos DB for WebAuthn credential storage; App Service configured for passwordless auth integration*
- [x] **0.1.4** Configure App Service for .NET API
  - *Implemented: Using br/public:avm/res/web/site with .NET 9, managed identity, health checks*
- [x] **0.1.5** Set up Azure Static Web Apps for Angular
  - *Implemented: Using br/public:avm/res/web/static-site with staging environment support*
- [x] **0.1.6** Configure Key Vault for secrets
  - *Implemented: Using br/public:avm/res/key-vault/vault with RBAC authorization, soft delete*
- [x] **0.1.7** Set up environment parameter files (dev, stg, prd)
  - *Implemented: dev.bicepparam, stg.bicepparam, prd.bicepparam with environment-appropriate SKUs and settings*

**Additional deliverables:**
- [x] Deployment scripts (deploy.sh, deploy.ps1) for automated deployment
- [x] Azure Infrastructure documentation (docs/AZURE-INFRASTRUCTURE.md)
- [x] Function Apps for sync and import processing
- [x] Service Bus with queues for async messaging
- [x] SignalR Service for real-time sync
- [x] Redis Cache for session management
- [x] Log Analytics and Application Insights for monitoring

#### 0.2 .NET Solution Structure ✅ COMPLETED

- [x] **0.2.1** Create solution with Clean Architecture layers
  - *Implemented: Luminous.sln with Directory.Build.props, Directory.Packages.props for centralized package management*
- [x] **0.2.2** Set up Luminous.Domain with entities and value objects
  - *Implemented: 10+ entities (Family, User, Device, Event, Chore, Routine, etc.), 8+ value objects, repository interfaces, domain events*
- [x] **0.2.3** Set up Luminous.Application with CQRS handlers
  - *Implemented: MediatR-based CQRS with commands/queries, FluentValidation validators, pipeline behaviors*
- [x] **0.2.4** Set up Luminous.Infrastructure with CosmosDB repositories
  - *Implemented: CosmosDbContext, base repository pattern, concrete repositories for all entities, Unit of Work*
- [x] **0.2.5** Set up Luminous.Api with controllers and middleware
  - *Implemented: ASP.NET Core API with controllers, exception handling middleware, Swagger/OpenAPI*
- [x] **0.2.6** Configure dependency injection and options pattern
  - *Implemented: DependencyInjection.cs in each layer, options pattern for CosmosDB settings*
- [x] **0.2.7** Set up xUnit test projects
  - *Implemented: 4 test projects (Domain.Tests, Application.Tests, Api.Tests, Integration.Tests) with sample tests*

**Additional deliverables:**
- [x] Luminous.Shared project for shared DTOs and contracts
- [x] Centralized package version management (Directory.Packages.props)
- [x] Global.json for SDK version pinning
- [x] Sample unit tests demonstrating testing patterns

#### 0.3 Angular Web Application ✅ COMPLETED

- [x] **0.3.1** Initialize Angular 19+ project with strict mode
  - *Implemented: Angular 19.2 with standalone components, strict TypeScript, and routing*
- [x] **0.3.2** Configure Angular Material or Tailwind CSS
  - *Implemented: Tailwind CSS 3.x with custom theme (primary colors, family colors, touch-friendly spacing)*
- [x] **0.3.3** Set up core module with authentication service
  - *Implemented: Core services (ApiService, AuthService, StorageService, WebAuthnService), guards (authGuard, guestGuard, roleGuard), HTTP interceptors (authInterceptor, errorInterceptor)*
- [x] **0.3.4** Implement WebAuthn/passkey authentication integration
  - *Implemented: Full WebAuthn/passkey support with registration, authentication, conditional UI (autofill), plus email OTP fallback*
- [x] **0.3.5** Create shared component library
  - *Implemented: Reusable components (ButtonComponent, CardComponent, AlertComponent, AvatarComponent, InputComponent, SpinnerComponent) with TypeScript types*
- [x] **0.3.6** Configure environment-based API URLs
  - *Implemented: Environment files (development, stg, production) with API URLs and WebAuthn configuration*

**Additional deliverables:**
- [x] Login and Register pages with passkey/OTP authentication flows
- [x] Dashboard placeholder with family-focused layout
- [x] 404 and Unauthorized pages
- [x] TypeScript models for User, Family, Auth entities
- [x] HTTP interceptors for auth tokens and error handling

#### 0.4 Local Development Environment ✅ COMPLETED

- [x] **0.4.1** Create Docker Compose for local services
  - *Implemented: docker-compose.yml with CosmosDB Emulator, Azurite, Redis, MailHog, and optional Redis Commander*
- [x] **0.4.2** Document Cosmos DB Emulator setup
  - *Implemented: Comprehensive DEVELOPMENT.md with SSL certificate setup, troubleshooting, and configuration*
- [x] **0.4.3** Create local development scripts (PowerShell/Bash)
  - *Implemented: scripts/dev-start.sh and scripts/dev-start.ps1 with service management, status checks, and certificate installation*
- [x] **0.4.4** Configure launch profiles for debugging
  - *Implemented: Properties/launchSettings.json with http, https, Docker, and Watch profiles*
- [x] **0.4.5** Set up local JWT issuer for development
  - *Implemented: LocalJwtTokenService, DevAuthController, JwtSettings configuration, and development token endpoints*

**Additional deliverables:**
- [x] Full appsettings.Development.json with all local service configurations
- [x] Swagger UI with JWT authentication support
- [x] Authorization policies (FamilyMember, FamilyAdmin, FamilyOwner)
- [x] Development authentication endpoint (POST /api/devauth/token)

#### 0.5 CI/CD Pipeline ✅ COMPLETED

- [x] **0.5.1** GitHub Actions for .NET build and test
  - *Implemented: .github/workflows/dotnet.yml with build, test, coverage, and security scanning*
- [x] **0.5.2** GitHub Actions for Angular build and test
  - *Implemented: .github/workflows/angular.yml with lint, typecheck, test, build, and security audit*
- [x] **0.5.3** GitHub Actions for Bicep deployment
  - *Implemented: .github/workflows/infrastructure.yml with validate, what-if, and deploy stages*
- [x] **0.5.4** Configure environment-specific deployments
  - *Implemented: .github/workflows/deploy.yml with dev/stg/prd environments, Azure OIDC auth*
- [x] **0.5.5** Set up Dependabot for dependency updates
  - *Implemented: .github/dependabot.yml for NuGet, npm, GitHub Actions, and Docker ecosystems*

**Additional deliverables:**
- [x] CI/CD documentation (docs/CI-CD.md)
- [x] Environment-specific deployment workflows with approval gates
- [x] Security scanning for .NET (vulnerable packages) and npm (audit)
- [x] Artifact management for build outputs
- [x] Bundle size analysis for Angular builds

#### 0.6 Documentation ✅ COMPLETED

- [x] **0.6.1** Complete PROJECT-OVERVIEW.md
  - *Implemented: Comprehensive product specification with deployment model, personas, UX principles, and functional scope*
- [x] **0.6.2** Complete ARCHITECTURE.md
  - *Implemented: Full architecture document covering all TOGAF phases (business, data, application, technology)*
- [x] **0.6.3** Complete ROADMAP.md
  - *Implemented: Detailed roadmap with phases 0-6, dependencies, and risk register*
- [x] **0.6.4** Create ADRs for technology decisions
  - *Implemented: 10 ADRs covering .NET backend, Angular, Azure, native mobile, CosmosDB, multi-tenancy, Bicep/AVMs, Magic Import, zero-distraction, and passwordless auth*
- [x] **0.6.5** Create local development setup guide
  - *Implemented: DEVELOPMENT.md with Docker Compose setup, VS Code configuration, troubleshooting, and ARM64 support*

**Additional deliverables:**
- [x] AZURE-INFRASTRUCTURE.md with detailed resource documentation
- [x] CI-CD.md with GitHub Actions pipeline documentation
- [x] ADR template (ADR-000-template.md) for future decisions
- [x] CLAUDE.md with comprehensive development guidelines

### Exit Criteria

- Azure infrastructure deploys successfully to dev environment
- .NET API runs locally with Cosmos DB Emulator
- Angular app authenticates via passkey or email OTP
- CI/CD pipeline runs on all pull requests
- Documentation complete and reviewed

---

## Phase 1: Core Platform

### Objective

Deliver the multi-tenant platform with user registration, family creation, device linking, and a basic web dashboard.

### Scope

#### 1.1 Multi-Tenant API ✅ COMPLETED

- [x] **1.1.1** Implement family (tenant) creation endpoint
  - *Implemented: RegisterFamilyCommand creates family and owner atomically, returns auth token*
  - *API: POST /api/auth/register*
- [x] **1.1.2** Implement user registration and profile creation
  - *Implemented: UpdateUserProfileCommand, GetCurrentUserQuery, GetUserQuery*
  - *API: GET /api/auth/me, PUT /api/users/family/{familyId}/{userId}/profile*
- [x] **1.1.3** Implement JWT claims with family context
  - *Implemented: TokenService generates JWT with family_id, role, email_verified claims*
  - *Claims: sub, family_id, role, display_name, email_verified*
- [x] **1.1.4** Add family-scoped authorization policies
  - *Implemented: TenantValidationMiddleware validates family access on routes*
  - *Policies: FamilyMember, FamilyAdmin, FamilyOwner*
- [x] **1.1.5** Implement tenant data isolation in repositories
  - *Implemented: ITenantContext and TenantContext services*
  - *All queries use familyId partition key; tenant access validated in handlers*

#### 1.2 Device Linking ✅ COMPLETED

- [x] **1.2.1** Implement link code generation endpoint
  - *Implemented: GenerateLinkCodeCommand creates unlinked device with 6-digit code (15-min expiry)*
  - *API: POST /api/devices/link-code*
- [x] **1.2.2** Implement link code validation and device registration
  - *Implemented: LinkDeviceCommand validates code, links device to family*
  - *API: POST /api/devices/link*
- [x] **1.2.3** Implement device token issuance
  - *Implemented: LinkDeviceCommand returns LinkedDeviceDto with JWT device token (30-day expiry)*
  - *Token includes: device_id, family_id, device_type, device_name claims*
- [x] **1.2.4** Create device management endpoints
  - *Implemented: GetDeviceQuery, GetFamilyDevicesQuery, UpdateDeviceCommand, UnlinkDeviceCommand, DeleteDeviceCommand*
  - *APIs: GET/PUT/DELETE /api/devices/family/{familyId}/{id}, POST /api/devices/family/{familyId}/{id}/unlink*
- [x] **1.2.5** Implement device heartbeat/status tracking
  - *Implemented: RecordHeartbeatCommand updates LastSeenAt, AppVersion; returns DeviceHeartbeatDto*
  - *API: POST /api/devices/family/{familyId}/{id}/heartbeat*

**Additional deliverables:**
- [x] Device entity methods: Unlink, Rename, UpdateSettings, Activate, Deactivate
- [x] DeviceSettingsDto, DeviceHeartbeatDto, LinkedDeviceDto DTOs
- [x] Unit tests for Device entity and command handlers
- [x] Authorization: FamilyMember for read, FamilyAdmin for write operations

#### 1.3 Family Member Management ✅ COMPLETED

- [x] **1.3.1** Implement member invitation flow
  - *Implemented: CreateInvitationCommand with NanoId-based invitation codes (7-day expiry)*
  - *Implemented: AcceptInvitationCommand creates user and returns auth token*
  - *Implemented: DeclineInvitationCommand, RevokeInvitationCommand*
  - *APIs: POST /api/invitations/family/{familyId}, POST /api/invitations/{code}/accept, POST /api/invitations/{code}/decline, DELETE /api/invitations/family/{familyId}/{id}*
- [x] **1.3.2** Implement role assignment (Owner, Admin, Adult, Teen, Child)
  - *Implemented: UpdateUserRoleCommand with authorization checks*
  - *Role hierarchy: Owner > Admin > Adult > Teen > Child > Caregiver*
  - *Safeguards: Cannot demote Owner, Admin can't promote to Admin*
  - *API: PUT /api/users/family/{familyId}/{userId}/role*
- [x] **1.3.3** Create profile management endpoints
  - *Implemented: UpdateCaregiverInfoCommand for medical/emergency info*
  - *Implemented: RemoveUserFromFamilyCommand with soft delete*
  - *API: PUT /api/users/family/{familyId}/{userId}/caregiver-info, DELETE /api/users/family/{familyId}/{userId}*
- [x] **1.3.4** Implement caregiver access tokens
  - *Implemented: GenerateCaregiverAccessTokenCommand (1-168 hour expiry)*
  - *Token includes: target_user_id, access_type=caregiver, is_read_only=true*
  - *API: POST /api/users/family/{familyId}/{userId}/caregiver-token*

**Additional deliverables:**
- [x] IInvitationRepository interface and InvitationRepository implementation
- [x] InvitationDto, SendInvitationRequestDto, AcceptedInvitationResultDto DTOs
- [x] GetInvitationByCodeQuery, GetFamilyInvitationsQuery, GetPendingInvitationsQuery
- [x] InvitationsController with full CRUD operations
- [x] Unit tests for Invitation entity
- [x] Authorization: FamilyAdmin required for invitation/role management

#### 1.3.5 Authentication Endpoints ✅ COMPLETED

- [x] **OTP Authentication**
  - *Implemented: RequestOtpCommand, VerifyOtpCommand with rate limiting*
  - *OtpToken entity and repository with email partitioning*
  - *DevelopmentEmailService for local testing*
  - *APIs: POST /api/auth/otp/request, POST /api/auth/otp/verify*
- [x] **Passkey/WebAuthn Authentication**
  - *Implemented: PasskeyRegisterStartCommand, PasskeyRegisterCompleteCommand*
  - *Implemented: PasskeyAuthenticateStartCommand, PasskeyAuthenticateCompleteCommand*
  - *WebAuthnService with FIDO2 library integration*
  - *Session management via distributed cache*
  - *APIs: POST /api/auth/passkey/register/start, POST /api/auth/passkey/register/complete*
  - *APIs: POST /api/auth/passkey/authenticate/start, POST /api/auth/passkey/authenticate/complete*
- [x] **Passkey Management**
  - *Implemented: ListPasskeysQuery, DeletePasskeyCommand*
  - *APIs: GET /api/auth/passkey/list, DELETE /api/auth/passkey/{id}*
- [x] **Token Refresh**
  - *Implemented: RefreshTokenCommand with token rotation*
  - *RefreshToken entity and repository with theft detection*
  - *API: POST /api/auth/refresh*

**Auth deliverables:**
- [x] OtpToken and RefreshToken domain entities
- [x] IOtpTokenRepository and IRefreshTokenRepository interfaces
- [x] OtpTokenRepository and RefreshTokenRepository CosmosDB implementations
- [x] IEmailService and IWebAuthnService application interfaces
- [x] WebAuthnService with FIDO2 v4.0 integration
- [x] DevelopmentEmailService for local testing (logs OTPs to console)
- [x] Updated AuthController with all auth endpoints
- [x] CosmosDB containers: otptokens, refreshtokens

**Email service deliverables:**
- [x] AzureEmailService for production email via Azure Communication Services
- [x] EmailTemplateService with Handlebars template rendering
- [x] HTML email templates (base.hbs, otp.hbs, invitation.hbs, welcome.hbs)
- [x] EmailSettings configuration class with UseDevelopmentMode flag
- [x] Config-based email service selection (DevelopmentEmailService vs AzureEmailService)
- [x] Azure Email Service and Communication Service in Bicep (AVMs)
- [x] NanoId session IDs in WebAuthnService (replaces GUIDs)
- [x] CreateInvitationCommand sends invitation emails

#### 1.4 Web Dashboard MVP ✅ COMPLETED

- [x] **1.4.1** Create family dashboard layout
  - *Implemented: DashboardShellComponent with responsive sidebar navigation*
  - *Features: Mobile-friendly hamburger menu, user profile display, family name display*
  - *Routes: /dashboard, /dashboard/members, /dashboard/devices, /dashboard/settings*
- [x] **1.4.2** Implement family settings page
  - *Implemented: SettingsComponent with form-based configuration*
  - *Features: Family name, timezone, default view, privacy mode, sleep mode settings*
  - *API: PUT /api/families/{id}/settings*
- [x] **1.4.3** Create member management UI
  - *Implemented: MembersComponent with full CRUD operations*
  - *Features: Member list, role badges, invite modal, role change modal, remove confirmation*
  - *Features: Pending invitations display with revoke capability*
  - *APIs: GET/PUT/DELETE /api/users/family/{familyId}/*, POST /api/invitations/*
- [x] **1.4.4** Implement device management UI
  - *Implemented: DevicesComponent with device cards and management*
  - *Features: Device list with status indicators, link code generation, edit/unlink modals*
  - *Features: Device type icons, last seen timestamps, platform/version display*
  - *APIs: GET/PUT/DELETE /api/devices/family/{familyId}/*, POST /api/devices/link-code*
- [x] **1.4.5** Create responsive design for mobile web
  - *Implemented: Tailwind CSS responsive breakpoints throughout all components*
  - *Features: Collapsible sidebar on mobile, touch-friendly tap targets (44px minimum)*
  - *Features: Responsive grids, mobile-first layouts, proper spacing on small screens*

**Additional deliverables:**
- [x] FamilyService, UserService, DeviceService, InvitationService in Angular
- [x] Device and Invitation TypeScript models
- [x] DashboardHomeComponent with stats cards and quick actions
- [x] Signal-based reactive state management in all services
- [x] Consistent error handling and success messaging
- [x] Modal dialogs for all CRUD operations

### Exit Criteria

- Users can sign up and create families with passkey or email
- Device linking flow works end-to-end
- Family members can be invited and managed
- Web dashboard displays family information
- All data properly isolated per tenant

---

## Phase 2: Display & Calendar

### Objective

Deliver the display application with calendar integration, real-time sync, and glanceable views.

### Scope

#### 2.1 Display Application (Angular + Electron) ✅ COMPLETED

- [x] **2.1.1** Create Angular Electron project structure
  - *Implemented: clients/display/ with Angular 19 + Electron 33*
  - *Structure: core services, feature modules, shared components*
  - *Build: electron-builder for Linux/Windows/macOS packaging*
- [x] **2.1.2** Implement kiosk mode (fullscreen, no escape)
  - *Implemented: electron/main.js with kiosk and fullscreen options*
  - *Features: Always-on-top, no frame, prevent close, power save blocker*
  - *Admin exit: Ctrl+Shift+Q triggers PIN dialog for authorized exit*
- [x] **2.1.3** Auto-start on system boot
  - *Implemented: scripts/install-autostart.sh (Linux systemd + XDG)*
  - *Implemented: scripts/install-autostart.ps1 (Windows scheduled task)*
  - *Implemented: scripts/kiosk-setup.sh (Raspberry Pi/Linux kiosk mode)*
- [x] **2.1.4** Watchdog for crash recovery
  - *Implemented: Watchdog interval checks window health*
  - *Features: Auto-recreate window on crash, crash count limit*
  - *Crash logging: Local crash-log.json for diagnostics*
  - *Process recovery: Handles renderer crashes and unresponsive states*
- [x] **2.1.5** Device token authentication
  - *Implemented: DeviceAuthService with 6-digit code flow*
  - *Implemented: ElectronService for secure IPC token storage*
  - *Features: Polling for link status, heartbeat validation*
  - *Auth interceptor: Adds X-Device-Token to API requests*
- [x] **2.1.6** Local caching for offline capability
  - *Implemented: CacheService with IndexedDB (idb library)*
  - *Stores: schedule, tasks, family, members with TTL expiration*
  - *Features: Automatic cache cleanup, offline detection, cache stats*

**Additional deliverables:**
- [x] ClockWidgetComponent with time-adaptive display
- [x] ScheduleViewComponent for daily schedule
- [x] TasksViewComponent with progress tracking
- [x] LinkingComponent for device pairing flow
- [x] SettingsComponent for display configuration
- [x] ExitDialogComponent with PIN verification
- [x] CanvasService for time-based canvas color adaptation
- [x] Display-optimized design tokens and typography
- [x] Tailwind CSS configuration for display sizes
- [x] README.md with deployment documentation

#### 2.2 Calendar Integration ✅ COMPLETED

- [x] **2.2.1** Google Calendar OAuth integration
  - *Implemented: GoogleCalendarProvider with OAuth 2.0 flow, token refresh, calendar list, event fetch with delta sync*
  - *API: Full Google Calendar API v3 integration with incremental sync support*
- [x] **2.2.2** Microsoft Graph (Outlook) integration
  - *Implemented: MicrosoftCalendarProvider with OAuth 2.0 flow, token refresh, calendar list, event fetch with delta query*
  - *API: Microsoft Graph API integration with delta sync for incremental updates*
- [x] **2.2.3** ICS URL subscription support
  - *Implemented: IcsCalendarProvider with HTTP fetch, ETag caching, Ical.Net parsing*
  - *Features: Supports webcal:// URLs, handles recurring events, parses VALARM reminders*
- [x] **2.2.4** Calendar-to-profile assignment
  - *Implemented: CalendarConnection.AssignedMemberIds for mapping calendars to family members*
  - *Events from synced calendars automatically assigned to configured members*
- [x] **2.2.5** Azure Function for calendar sync jobs
  - *Implemented: Luminous.Functions.Sync project with timer-triggered CalendarSyncFunction*
  - *Runs every 5 minutes to sync due calendar connections*
- [x] **2.2.6** Recurring event support
  - *Implemented: RecurrenceRule parsing from RRULE format (Daily, Weekly, Monthly, Yearly)*
  - *Supports interval, BYDAY, UNTIL, COUNT parameters*

**Additional deliverables:**
- [x] CalendarConnection domain entity with OAuth token storage
- [x] CalendarSyncSettings and OAuthTokens value objects
- [x] ICalendarProvider and ICalendarSyncService interfaces
- [x] CalendarSyncService for orchestrating sync operations
- [x] CalendarConnectionRepository with CosmosDB container
- [x] Full API: GET/POST/PUT/DELETE /api/calendar-connections/family/{familyId}
- [x] OAuth flow: POST .../oauth/start, POST .../oauth/complete
- [x] Manual sync: POST .../sync
- [x] Calendar settings in App Service configuration

#### 2.3 Calendar Views

- [ ] **2.3.1** Day view (next 24 hours focus)
- [ ] **2.3.2** Week view (7-day overview)
- [ ] **2.3.3** Month view (calendar grid)
- [ ] **2.3.4** Agenda view (chronological list)
- [ ] **2.3.5** Profile filtering

#### 2.4 Display Widgets

- [ ] **2.4.1** "What's Next" widget
- [ ] **2.4.2** Countdown widget for major events
- [ ] **2.4.3** Weather widget
- [ ] **2.4.4** Clock widget

#### 2.5 Real-time Sync (SignalR)

- [ ] **2.5.1** Azure SignalR Service integration
- [ ] **2.5.2** Family-scoped message groups
- [ ] **2.5.3** Push updates to connected displays
- [ ] **2.5.4** Connection recovery handling

#### 2.6 Display Modes

- [ ] **2.6.1** Normal display mode
- [ ] **2.6.2** Privacy mode (wallpaper only)
- [ ] **2.6.3** Sleep mode (scheduled)

### Exit Criteria

- Display app runs in kiosk mode on various hardware
- Calendar events sync from Google and Outlook
- Real-time updates via SignalR
- Offline mode works with cached data
- WCAG 2.1 AA compliance verified

---

## Phase 3: Native Mobile Apps

### Objective

Deliver native iOS and Android apps with full feature access and push notifications.

### Scope

#### 3.1 iOS App (Swift/SwiftUI)

- [ ] **3.1.1** Xcode project with SwiftUI
- [ ] **3.1.2** Passkey integration with ASAuthorizationController
- [ ] **3.1.3** API client with async/await
- [ ] **3.1.4** Core Data for offline caching
- [ ] **3.1.5** Navigation structure (TabView)
- [ ] **3.1.6** Push notifications (APNs)

#### 3.2 Android App (Kotlin/Compose)

- [ ] **3.2.1** Android Studio project with Jetpack Compose
- [ ] **3.2.2** Passkey integration with Credential Manager API
- [ ] **3.2.3** Retrofit API client with coroutines
- [ ] **3.2.4** Room for offline caching
- [ ] **3.2.5** Navigation component
- [ ] **3.2.6** Push notifications (FCM)

#### 3.3 Mobile Calendar Features

- [ ] **3.3.1** Calendar views (day, week, month)
- [ ] **3.3.2** Event creation and editing
- [ ] **3.3.3** Calendar connection management
- [ ] **3.3.4** Profile filtering

#### 3.4 Mobile Device Features

- [ ] **3.4.1** Device linking via code entry
- [ ] **3.4.2** Family member management
- [ ] **3.4.3** Biometric authentication (passkey unlock)
- [ ] **3.4.4** Widget support (iOS/Android)

### Exit Criteria

- iOS app available on App Store (TestFlight initially)
- Android app available on Play Store (Beta track initially)
- Push notifications work for events and reminders
- Apps authenticate via passkey or social login
- Offline mode with local caching

---

## Phase 4: Task Management

### Objective

Add chores, routines, and rewards system across all platforms.

### Scope

#### 4.1 Chore Management

- [ ] **4.1.1** Chore creation API and UI
- [ ] **4.1.2** Assignment to profiles or "anyone"
- [ ] **4.1.3** Recurrence patterns
- [ ] **4.1.4** Due date/time support
- [ ] **4.1.5** Chore completion flow
- [ ] **4.1.6** Completion history

#### 4.2 Routine Management

- [ ] **4.2.1** Routine builder (sequence of steps)
- [ ] **4.2.2** Step icons and images
- [ ] **4.2.3** Timer per step (optional)
- [ ] **4.2.4** Routine progress tracking
- [ ] **4.2.5** Streak tracking

#### 4.3 Rewards System

- [ ] **4.3.1** Point assignment to chores/routines
- [ ] **4.3.2** Per-profile point balance
- [ ] **4.3.3** Custom reward definitions
- [ ] **4.3.4** Reward request/approval workflow
- [ ] **4.3.5** Progress visualization
- [ ] **4.3.6** Celebration animations

#### 4.4 Display Task Widgets

- [ ] **4.4.1** "Today's Tasks" widget
- [ ] **4.4.2** Routine progress widget
- [ ] **4.4.3** Points/rewards widget
- [ ] **4.4.4** Kid-friendly mode

### Exit Criteria

- Chores can be created and completed on all platforms
- Routines guide children through multi-step processes
- Points accumulate and rewards can be claimed
- Real-time sync of task status

---

## Phase 5: Household Features

### Objective

Complete the household coordination features including profiles, meal planning, lists, and caregiver access.

### Scope

#### 5.1 Profile Management

- [ ] **5.1.1** Enhanced profile editing
- [ ] **5.1.2** Avatar/photo upload to Blob Storage
- [ ] **5.1.3** Caregiver info section
- [ ] **5.1.4** Pet profiles

#### 5.2 Meal Planning

- [ ] **5.2.1** Weekly meal plan view
- [ ] **5.2.2** Recipe storage and import
- [ ] **5.2.3** Ingredient management
- [ ] **5.2.4** "Add to grocery list" action
- [ ] **5.2.5** Dietary preferences

#### 5.3 List Management

- [ ] **5.3.1** Custom list creation
- [ ] **5.3.2** List templates
- [ ] **5.3.3** Checklist and board views
- [ ] **5.3.4** List sharing

#### 5.4 Caregiver Portal

- [ ] **5.4.1** Web-based caregiver access
- [ ] **5.4.2** Time-limited access tokens
- [ ] **5.4.3** Read-only calendar view
- [ ] **5.4.4** Caregiver info display

#### 5.5 Family Join Codes

- [ ] **5.5.1** Generate shareable family join code (configurable by owner/admin)
- [ ] **5.5.2** Set default role for join code users (e.g., Adult, Teen, Child)
- [ ] **5.5.3** Join request queue with approval workflow
- [ ] **5.5.4** Owner/admin approval UI with role assignment
- [ ] **5.5.5** Expiration settings for join codes (optional time limit, usage limit)
- [ ] **5.5.6** Revoke/regenerate join code capability

### Exit Criteria

- Complete profile management across all platforms
- Meal planning with recipe storage
- Lists usable for grocery and custom purposes
- Caregivers can access via web link
- Family join codes with approval workflow operational

---

## Phase 6: Intelligence & Ecosystem

### Objective

Add AI-powered features and platform extensibility for community contributions and third-party integrations.

### Scope

#### 6.1 Magic Import Service (Azure Function)

- [ ] **6.1.1** Email forwarding with Azure Logic Apps
- [ ] **6.1.2** Azure Cognitive Services for OCR
- [ ] **6.1.3** PDF parsing with Azure Form Recognizer
- [ ] **6.1.4** Approval queue in web/mobile apps
- [ ] **6.1.5** Azure OpenAI for content extraction

#### 6.2 AI Suggestions

- [ ] **6.2.1** Meal plan suggestions
- [ ] **6.2.2** Schedule conflict detection
- [ ] **6.2.3** Chore assignment suggestions

#### 6.3 Public API

- [ ] **6.3.1** API Management configuration
- [ ] **6.3.2** OpenAPI documentation
- [ ] **6.3.3** API key management
- [ ] **6.3.4** Rate limiting
- [ ] **6.3.5** Webhook support

#### 6.4 Additional Integrations

- [ ] **6.4.1** iCloud CalDAV sync
- [ ] **6.4.2** TeamSnap integration
- [ ] **6.4.3** School calendar integrations
- [ ] **6.4.4** Home automation APIs

#### 6.5 Community Tools

- [ ] **6.5.1** Routine template sharing
- [ ] **6.5.2** Icon pack contributions
- [ ] **6.5.3** Theme sharing
- [ ] **6.5.4** Internationalization (i18n)

### Exit Criteria

- Magic Import processes forwarded emails and photos
- AI suggestions are helpful and privacy-preserving
- Public API enables third-party integrations
- Community can contribute content

---

## Future Vision

### Beyond Phase 6

These features are under consideration for future development:

| Feature | Description | Considerations |
|---------|-------------|----------------|
| **Multiple Displays** | Sync across multiple displays in home | Technical complexity, use case validation |
| **AR/VR Integration** | View schedule in AR glasses | Hardware dependency, limited audience |
| **Wearable Support** | Apple Watch / Wear OS companion | Reduced scope, notification focus |
| **Smart Home Routines** | Trigger home automation from routines | Integration complexity |
| **Family Games** | Educational games as rewards | Conflicts with zero-distraction |
| **Photo Frame Mode** | Display family photos when idle | Scope creep risk |
| **Video Messages** | Short video notes for family | Conflicts with zero-distraction |

### Technology Evolution

| Current | Future Consideration | Rationale |
|---------|---------------------|-----------|
| Electron | Tauri | Smaller bundle, better performance |
| Angular | Angular with Signals | Reactive improvements |
| Cosmos DB | Azure Cosmos DB Serverless | Cost optimization for smaller deployments |
| In-house Auth | External IdP | Consider delegating if maintenance burden increases |
| Azure Functions | Azure Container Apps | More flexibility for long-running jobs |

---

## Dependency Map

### Phase Dependencies

```
Phase 0 (Foundation)
    │
    └──▶ Phase 1 (Core Display)
              │
              ├──▶ Phase 2 (Task Management)
              │         │
              │         └──▶ Phase 3 (Mobile Companion)
              │                   │
              │                   └──▶ Phase 4 (Household)
              │                             │
              │                             └──▶ Phase 5 (Intelligence)
              │                                       │
              │                                       └──▶ Phase 6 (Ecosystem)
              │
              └──▶ (Parallel) Web App MVP
```

### Critical Path

The critical path for MVP delivery:

```
0.1 Dev Environment → 0.3 Core Structure → 0.5 Domain Models →
1.1 Display Shell → 1.2 Calendar Integration → 1.3 Calendar Views →
1.7 Offline → MVP Release
```

### Optional Parallel Tracks

These can be developed in parallel after Phase 0:

- **Web App MVP**: Basic web access (Phase 1 parallel)
- **Additional Calendar Providers**: Outlook, iCloud (Phase 1+)
- **Sync Server**: Multi-device sync (Phase 3 parallel)

---

## Risk Register

### Technical Risks

| Risk | Impact | Probability | Mitigation |
|------|--------|-------------|------------|
| **Electron performance on Pi** | High | Medium | Evaluate Tauri early; optimize bundle |
| **Calendar API rate limits** | Medium | Medium | Aggressive caching; exponential backoff |
| **Sync conflicts** | High | Medium | CRDT-based design; extensive testing |
| **SQLite limitations on web** | Medium | Low | OPFS fallback; sql.js optimization |
| **React Native compatibility** | Medium | Medium | Pin versions; thorough device testing |

### Product Risks

| Risk | Impact | Probability | Mitigation |
|------|--------|-------------|------------|
| **Scope creep (entertainment)** | High | Medium | Strict ADR enforcement; design reviews |
| **Kid UX too complex** | High | Medium | User testing with children; iteration |
| **Caregiver adoption** | Medium | Medium | Simplified web portal; no app required |
| **Calendar sync accuracy** | High | Low | Comprehensive sync testing; monitoring |

### Project Risks

| Risk | Impact | Probability | Mitigation |
|------|--------|-------------|------------|
| **Contributor availability** | Medium | Medium | Comprehensive docs; maintainer diversity |
| **Technology obsolescence** | Medium | Low | Standard tech choices; migration paths |
| **Legal/licensing issues** | High | Low | AGPL clarity; dependency audit |

---

## Related Documents

- [Project Overview](./PROJECT-OVERVIEW.md)
- [Architecture Overview](./ARCHITECTURE.md)
- [Architecture Decision Records](./adr/)
- [CLAUDE.md (Development Guidelines)](../CLAUDE.md)

---

## Document History

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0.0 | 2025-12-21 | Luminous Team | Initial roadmap |
| 2.0.0 | 2025-12-21 | Luminous Team | Updated for Azure/.NET/Angular stack |
| 2.1.0 | 2025-12-21 | Luminous Team | Phase 0.1 Azure Infrastructure completed |
| 2.1.1 | 2025-12-21 | Luminous Team | Refactored to use AVMs directly from public registry |
| 2.2.0 | 2025-12-21 | Luminous Team | Phase 0.2 .NET Solution Structure completed |
| 2.3.0 | 2025-12-21 | Luminous Team | Phase 0.3 Angular Web Application completed |
| 2.4.0 | 2025-12-22 | Luminous Team | Phase 0.4 Local Development Environment completed |
| 2.5.0 | 2025-12-22 | Luminous Team | Phase 0.5 CI/CD Pipeline completed |
| 2.6.0 | 2025-12-23 | Luminous Team | Phase 0.6 Documentation completed; Phase 0 complete |
| 2.7.0 | 2025-12-23 | Luminous Team | Phase 1.1 Multi-Tenant API completed |
| 2.8.0 | 2025-12-23 | Luminous Team | Phase 1.2 Device Linking completed |
| 2.9.0 | 2025-12-24 | Luminous Team | Phase 1.3 Family Member Management completed |
| 3.0.0 | 2025-12-24 | Luminous Team | Phase 1.4 Web Dashboard MVP completed; Phase 1 complete |
| 3.1.0 | 2025-12-27 | Luminous Team | Phase 1.3.5 Authentication Endpoints completed (OTP, Passkey, Token Refresh) |
| 3.2.0 | 2025-12-27 | Luminous Team | Added email services (Azure ACS, Handlebars templates, config-based service selection) |
| 3.3.0 | 2026-01-01 | Luminous Team | Phase 2.1 Display Application (Angular + Electron) completed |
