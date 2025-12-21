# Luminous Development Roadmap

> **Document Version:** 2.0.0
> **Last Updated:** 2025-12-21
> **Status:** Draft
> **TOGAF Phase:** Phase E/F (Opportunities, Solutions & Migration Planning)

---

## Table of Contents

1. [Roadmap Overview](#roadmap-overview)
2. [Implementation Phases](#implementation-phases)
3. [Phase 0: Foundation](#phase-0-foundation)
4. [Phase 1: Core Display](#phase-1-core-display)
5. [Phase 2: Task Management](#phase-2-task-management)
6. [Phase 3: Mobile Companion](#phase-3-mobile-companion)
7. [Phase 4: Household Management](#phase-4-household-management)
8. [Phase 5: Intelligence Layer](#phase-5-intelligence-layer)
9. [Phase 6: Ecosystem Expansion](#phase-6-ecosystem-expansion)
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
+-- Azure AD B2C Configuration
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

| Phase | Name | Focus | Key Deliverables |
|-------|------|-------|------------------|
| **0** | Foundation | Infrastructure | Azure IaC, .NET solution, Angular shell, Azure AD B2C |
| **1** | Core Platform | Multi-tenancy | Family sign-up, device linking, CosmosDB, web MVP |
| **2** | Display & Calendar | Calendar visibility | Display app, calendar integration, SignalR sync |
| **3** | Native Mobile | Mobile apps | iOS (Swift), Android (Kotlin), push notifications |
| **4** | Task Management | Chores and routines | Task creation, completion tracking, rewards |
| **5** | Household Features | Expanded features | Profiles, meals, lists, caregiver portal |
| **6** | Intelligence & Ecosystem | AI and extensions | Magic import, suggestions, third-party APIs |

---

## Phase 0: Foundation

### Objective

Establish the Azure infrastructure, .NET backend, Angular frontend, and development environment that all subsequent phases will build upon.

### Scope

#### 0.1 Azure Infrastructure (Bicep with AVMs)

- [ ] **0.1.1** Create Bicep modules for all Azure resources using AVMs
- [ ] **0.1.2** Configure Cosmos DB with required containers
- [ ] **0.1.3** Set up Azure AD B2C tenant and user flows
- [ ] **0.1.4** Configure App Service for .NET API
- [ ] **0.1.5** Set up Azure Static Web Apps for Angular
- [ ] **0.1.6** Configure Key Vault for secrets
- [ ] **0.1.7** Set up environment parameter files (dev, staging, prod)

#### 0.2 .NET Solution Structure

- [ ] **0.2.1** Create solution with Clean Architecture layers
- [ ] **0.2.2** Set up Luminous.Domain with entities and value objects
- [ ] **0.2.3** Set up Luminous.Application with CQRS handlers
- [ ] **0.2.4** Set up Luminous.Infrastructure with CosmosDB repositories
- [ ] **0.2.5** Set up Luminous.Api with controllers and middleware
- [ ] **0.2.6** Configure dependency injection and options pattern
- [ ] **0.2.7** Set up xUnit test projects

#### 0.3 Angular Web Application

- [ ] **0.3.1** Initialize Angular 19+ project with strict mode
- [ ] **0.3.2** Configure Angular Material or Tailwind CSS
- [ ] **0.3.3** Set up core module with authentication service
- [ ] **0.3.4** Implement MSAL integration for Azure AD B2C
- [ ] **0.3.5** Create shared component library
- [ ] **0.3.6** Configure environment-based API URLs

#### 0.4 Local Development Environment

- [ ] **0.4.1** Create Docker Compose for local services
- [ ] **0.4.2** Document Cosmos DB Emulator setup
- [ ] **0.4.3** Create local development scripts (PowerShell/Bash)
- [ ] **0.4.4** Configure launch profiles for debugging
- [ ] **0.4.5** Set up local JWT issuer for development

#### 0.5 CI/CD Pipeline

- [ ] **0.5.1** GitHub Actions for .NET build and test
- [ ] **0.5.2** GitHub Actions for Angular build and test
- [ ] **0.5.3** GitHub Actions for Bicep deployment
- [ ] **0.5.4** Configure environment-specific deployments
- [ ] **0.5.5** Set up Dependabot for dependency updates

#### 0.6 Documentation

- [ ] **0.6.1** Complete PROJECT-OVERVIEW.md
- [ ] **0.6.2** Complete ARCHITECTURE.md
- [ ] **0.6.3** Complete ROADMAP.md
- [ ] **0.6.4** Create ADRs for technology decisions
- [ ] **0.6.5** Create local development setup guide

### Exit Criteria

- Azure infrastructure deploys successfully to dev environment
- .NET API runs locally with Cosmos DB Emulator
- Angular app authenticates via Azure AD B2C
- CI/CD pipeline runs on all pull requests
- Documentation complete and reviewed

---

## Phase 1: Core Platform

### Objective

Deliver the multi-tenant platform with user registration, family creation, device linking, and a basic web dashboard.

### Scope

#### 1.1 Multi-Tenant API

- [ ] **1.1.1** Implement family (tenant) creation endpoint
- [ ] **1.1.2** Implement user registration and profile creation
- [ ] **1.1.3** Implement JWT claims with family context
- [ ] **1.1.4** Add family-scoped authorization policies
- [ ] **1.1.5** Implement tenant data isolation in repositories

#### 1.2 Device Linking

- [ ] **1.2.1** Implement link code generation endpoint
- [ ] **1.2.2** Implement link code validation and device registration
- [ ] **1.2.3** Implement device token issuance
- [ ] **1.2.4** Create device management endpoints
- [ ] **1.2.5** Implement device heartbeat/status tracking

#### 1.3 Family Member Management

- [ ] **1.3.1** Implement member invitation flow
- [ ] **1.3.2** Implement role assignment (Owner, Admin, Adult, Teen, Child)
- [ ] **1.3.3** Create profile management endpoints
- [ ] **1.3.4** Implement caregiver access tokens

#### 1.4 Web Dashboard MVP

- [ ] **1.4.1** Create family dashboard layout
- [ ] **1.4.2** Implement family settings page
- [ ] **1.4.3** Create member management UI
- [ ] **1.4.4** Implement device management UI
- [ ] **1.4.5** Create responsive design for mobile web

### Exit Criteria

- Users can sign up and create families via Azure AD B2C
- Device linking flow works end-to-end
- Family members can be invited and managed
- Web dashboard displays family information
- All data properly isolated per tenant

---

## Phase 2: Display & Calendar

### Objective

Deliver the display application with calendar integration, real-time sync, and glanceable views.

### Scope

#### 2.1 Display Application (Angular + Electron)

- [ ] **2.1.1** Create Angular Electron project structure
- [ ] **2.1.2** Implement kiosk mode (fullscreen, no escape)
- [ ] **2.1.3** Auto-start on system boot
- [ ] **2.1.4** Watchdog for crash recovery
- [ ] **2.1.5** Device token authentication
- [ ] **2.1.6** Local caching for offline capability

#### 2.2 Calendar Integration

- [ ] **2.2.1** Google Calendar OAuth integration
- [ ] **2.2.2** Microsoft Graph (Outlook) integration
- [ ] **2.2.3** ICS URL subscription support
- [ ] **2.2.4** Calendar-to-profile assignment
- [ ] **2.2.5** Azure Function for calendar sync jobs
- [ ] **2.2.6** Recurring event support

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
- [ ] **3.1.2** MSAL integration for Azure AD B2C
- [ ] **3.1.3** API client with async/await
- [ ] **3.1.4** Core Data for offline caching
- [ ] **3.1.5** Navigation structure (TabView)
- [ ] **3.1.6** Push notifications (APNs)

#### 3.2 Android App (Kotlin/Compose)

- [ ] **3.2.1** Android Studio project with Jetpack Compose
- [ ] **3.2.2** MSAL integration for Azure AD B2C
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
- [ ] **3.4.3** Biometric authentication
- [ ] **3.4.4** Widget support (iOS/Android)

### Exit Criteria

- iOS app available on App Store (TestFlight initially)
- Android app available on Play Store (Beta track initially)
- Push notifications work for events and reminders
- Apps authenticate via Azure AD B2C
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

### Exit Criteria

- Complete profile management across all platforms
- Meal planning with recipe storage
- Lists usable for grocery and custom purposes
- Caregivers can access via web link

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
| Azure AD B2C | Entra External ID | Unified identity platform |
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
