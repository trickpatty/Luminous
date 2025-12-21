# Luminous Development Roadmap

> **Document Version:** 1.0.0
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
├── Development Environment
├── Monorepo Structure
├── Core Domain Models
└── Design System Foundation

Phase 1: Core Display
├── Display Application Shell
├── Calendar Context
├── Event Display Widgets
└── Offline Data Layer

Phase 2: Task Management
├── Chore/Task Context
├── Routine Context
├── Rewards Context
└── Progress Visualization

Phase 3: Mobile Companion
├── Mobile Application Shell
├── Push Notification Service
├── Sync Engine
└── Remote Data Layer

Phase 4: Household Management
├── Profile Management
├── Meal Planning Context
├── List Management Context
└── Caregiver Portal

Phase 5: Intelligence
├── Magic Import Service
├── AI Suggestion Engine
├── Natural Language Processing
└── Learning Pipeline

Phase 6: Ecosystem
├── Plugin Architecture
├── API Gateway
├── Third-Party Integrations
└── Community Contributions
```

---

## Implementation Phases

### Phase Summary

| Phase | Name | Focus | Key Deliverables |
|-------|------|-------|------------------|
| **0** | Foundation | Infrastructure | Monorepo, CI/CD, design system, domain models |
| **1** | Core Display | Calendar visibility | Display app, calendar integration, offline mode |
| **2** | Task Management | Chores and routines | Task creation, completion tracking, rewards |
| **3** | Mobile Companion | Mobile control | iOS/Android apps, push notifications, sync |
| **4** | Household Management | Expanded features | Profiles, meals, lists, caregiver sharing |
| **5** | Intelligence Layer | AI features | Magic import, suggestions, automation |
| **6** | Ecosystem Expansion | Platform growth | Plugins, APIs, community tools |

---

## Phase 0: Foundation

### Objective

Establish the technical foundation, development environment, and architectural patterns that all subsequent phases will build upon.

### Scope

#### 0.1 Development Environment

- [ ] **0.1.1** Initialize monorepo with Turborepo
- [ ] **0.1.2** Configure TypeScript with strict settings
- [ ] **0.1.3** Set up ESLint and Prettier configurations
- [ ] **0.1.4** Configure Vitest for unit testing
- [ ] **0.1.5** Set up Playwright for E2E testing
- [ ] **0.1.6** Implement pre-commit hooks (Husky + lint-staged)
- [ ] **0.1.7** Configure Changesets for versioning

#### 0.2 CI/CD Pipeline

- [ ] **0.2.1** GitHub Actions workflow for linting
- [ ] **0.2.2** GitHub Actions workflow for testing
- [ ] **0.2.3** GitHub Actions workflow for building
- [ ] **0.2.4** Dependabot/Renovate for dependency updates
- [ ] **0.2.5** Code coverage reporting
- [ ] **0.2.6** Release automation pipeline

#### 0.3 Core Package Structure

- [ ] **0.3.1** Create `packages/core` with domain model stubs
- [ ] **0.3.2** Create `packages/ui` with design system foundation
- [ ] **0.3.3** Create `packages/sync` with sync engine interfaces
- [ ] **0.3.4** Configure package inter-dependencies
- [ ] **0.3.5** Set up path aliases and TypeScript references

#### 0.4 Design System Foundation

- [ ] **0.4.1** Define design tokens (colors, typography, spacing)
- [ ] **0.4.2** Create base theme configuration
- [ ] **0.4.3** Implement primitive components (Button, Input, Text)
- [ ] **0.4.4** Set up Storybook for component development
- [ ] **0.4.5** Document accessibility requirements per component

#### 0.5 Domain Model Implementation

- [ ] **0.5.1** Implement `Household` aggregate
- [ ] **0.5.2** Implement `Profile` entity
- [ ] **0.5.3** Implement `Event` entity
- [ ] **0.5.4** Implement `Chore` entity
- [ ] **0.5.5** Implement shared value objects (DateTime, Color, etc.)
- [ ] **0.5.6** Define domain events
- [ ] **0.5.7** Implement repository interfaces (ports)

#### 0.6 Documentation

- [ ] **0.6.1** Complete PROJECT-OVERVIEW.md
- [ ] **0.6.2** Complete ARCHITECTURE.md
- [ ] **0.6.3** Complete ROADMAP.md
- [ ] **0.6.4** Create initial ADRs
- [ ] **0.6.5** Set up documentation site (optional)

### Exit Criteria

- All packages build and lint successfully
- Unit test framework operational with example tests
- Design system Storybook accessible
- Domain models pass validation tests
- CI pipeline runs on all pull requests

---

## Phase 1: Core Display

### Objective

Deliver a functional wall-mounted display that shows family calendars in a glanceable format with offline capability.

### Scope

#### 1.1 Display Application Shell

- [ ] **1.1.1** Create `apps/display` with Electron/Tauri
- [ ] **1.1.2** Implement kiosk mode (fullscreen, no escape)
- [ ] **1.1.3** Auto-start on system boot
- [ ] **1.1.4** Watchdog for crash recovery
- [ ] **1.1.5** Basic settings screen (PIN-protected)

#### 1.2 Calendar Context Implementation

- [ ] **1.2.1** Implement Google Calendar adapter
- [ ] **1.2.2** Implement ICS URL subscription adapter
- [ ] **1.2.3** OAuth flow for Google authentication
- [ ] **1.2.4** Calendar-to-profile assignment
- [ ] **1.2.5** Recurring event support
- [ ] **1.2.6** Event color mapping

#### 1.3 Calendar Views

- [ ] **1.3.1** Day view (next 24 hours focus)
- [ ] **1.3.2** Week view (7-day overview)
- [ ] **1.3.3** Month view (calendar grid)
- [ ] **1.3.4** Agenda view (chronological list)
- [ ] **1.3.5** View switching and navigation
- [ ] **1.3.6** Profile filtering

#### 1.4 Event Display Widgets

- [ ] **1.4.1** Event card component (color-coded by profile)
- [ ] **1.4.2** "What's Next" widget (next 3 events)
- [ ] **1.4.3** Countdown widget (days until major events)
- [ ] **1.4.4** Today summary widget

#### 1.5 Ambient Widgets

- [ ] **1.5.1** Clock widget (with timezone)
- [ ] **1.5.2** Weather widget (current + forecast)
- [ ] **1.5.3** Weather API integration (configurable provider)

#### 1.6 Display Modes

- [ ] **1.6.1** Normal display mode
- [ ] **1.6.2** Privacy mode (wallpaper/clock only)
- [ ] **1.6.3** Sleep mode (screen off on schedule)
- [ ] **1.6.4** Auto-brightness (if hardware supports)

#### 1.7 Offline Capability

- [ ] **1.7.1** SQLite database setup (sql.js)
- [ ] **1.7.2** Local event cache
- [ ] **1.7.3** Offline indicator
- [ ] **1.7.4** Sync status indicator
- [ ] **1.7.5** Automatic sync on reconnection

#### 1.8 Accessibility

- [ ] **1.8.1** Large text mode
- [ ] **1.8.2** High contrast mode
- [ ] **1.8.3** Touch target sizing (44x44px minimum)
- [ ] **1.8.4** Readable from 10+ feet

### Exit Criteria

- Display shows events from connected Google Calendar
- Display functions offline with cached data
- Privacy and sleep modes functional
- Tested on Raspberry Pi 4 target hardware
- WCAG 2.1 AA compliance verified

---

## Phase 2: Task Management

### Objective

Add chores, routines, and rewards to transform the display from view-only to interactive family coordination.

### Scope

#### 2.1 Chore/Task Context

- [ ] **2.1.1** Chore creation UI
- [ ] **2.1.2** Chore assignment (profiles or "anyone")
- [ ] **2.1.3** Recurrence patterns
- [ ] **2.1.4** Due date/time support
- [ ] **2.1.5** Priority levels (optional)
- [ ] **2.1.6** Chore completion flow (touch to complete)
- [ ] **2.1.7** Completion history

#### 2.2 Routine Context

- [ ] **2.2.1** Routine builder (sequence of steps)
- [ ] **2.2.2** Step icons (kid-friendly library)
- [ ] **2.2.3** Timer per step (optional)
- [ ] **2.2.4** Multiple display modes (list, cards, slideshow)
- [ ] **2.2.5** Routine progress tracking
- [ ] **2.2.6** Streak tracking

#### 2.3 Starter Content

- [ ] **2.3.1** Morning routine template
- [ ] **2.3.2** Bedtime routine template
- [ ] **2.3.3** After-school routine template
- [ ] **2.3.4** Common chore templates
- [ ] **2.3.5** Icon library (100+ kid-friendly icons)

#### 2.4 Rewards System

- [ ] **2.4.1** Point assignment to chores/routines
- [ ] **2.4.2** Per-profile point balance
- [ ] **2.4.3** Custom reward definitions
- [ ] **2.4.4** Reward request/approval workflow
- [ ] **2.4.5** Progress-to-goal visualization
- [ ] **2.4.6** Celebration animations

#### 2.5 Task Display Widgets

- [ ] **2.5.1** "Today's Tasks" widget
- [ ] **2.5.2** Profile task summary
- [ ] **2.5.3** Routine progress widget
- [ ] **2.5.4** Points/rewards widget
- [ ] **2.5.5** Streak display

#### 2.6 Kid Mode

- [ ] **2.6.1** Simplified UI for children
- [ ] **2.6.2** Large touch targets
- [ ] **2.6.3** Emoji/icon-based labels
- [ ] **2.6.4** Non-reader friendly design
- [ ] **2.6.5** Limited destructive actions

### Exit Criteria

- Chores can be created, assigned, and completed on display
- Routines guide children through multi-step processes
- Points accumulate and rewards can be claimed
- Kid mode is intuitive for 6+ year olds
- All task data persists offline

---

## Phase 3: Mobile Companion

### Objective

Deliver iOS and Android companion apps that provide full control and notifications, keeping family members informed on the go.

### Scope

#### 3.1 Mobile Application Shell

- [ ] **3.1.1** React Native project setup
- [ ] **3.1.2** Navigation structure
- [ ] **3.1.3** Authentication flow
- [ ] **3.1.4** Offline capability
- [ ] **3.1.5** Deep linking support

#### 3.2 Sync Engine

- [ ] **3.2.1** CRDT-based sync protocol
- [ ] **3.2.2** Conflict resolution logic
- [ ] **3.2.3** Sync status indicators
- [ ] **3.2.4** Background sync
- [ ] **3.2.5** Manual sync trigger

#### 3.3 Mobile Calendar Features

- [ ] **3.3.1** Calendar views (day, week, month, agenda)
- [ ] **3.3.2** Event creation and editing
- [ ] **3.3.3** Calendar connection management
- [ ] **3.3.4** Profile filtering

#### 3.4 Mobile Task Features

- [ ] **3.4.1** Chore creation and management
- [ ] **3.4.2** Routine creation and management
- [ ] **3.4.3** Task completion (for self)
- [ ] **3.4.4** Reward management

#### 3.5 Push Notifications

- [ ] **3.5.1** Push notification service (APNs + FCM)
- [ ] **3.5.2** Event reminders
- [ ] **3.5.3** Chore due notifications
- [ ] **3.5.4** Completion notifications (kids completed X)
- [ ] **3.5.5** Notification preferences

#### 3.6 Mobile Widgets

- [ ] **3.6.1** iOS Today widget (schedule)
- [ ] **3.6.2** iOS lock screen widget
- [ ] **3.6.3** Android widget support

#### 3.7 Sync Server (Optional)

- [ ] **3.7.1** Server application shell
- [ ] **3.7.2** REST API for sync
- [ ] **3.7.3** WebSocket for real-time updates
- [ ] **3.7.4** User authentication (OAuth)
- [ ] **3.7.5** Push notification dispatch
- [ ] **3.7.6** Docker deployment

### Exit Criteria

- Mobile apps available on iOS and Android
- Changes sync between display and mobile within 60 seconds
- Push notifications delivered reliably
- Offline mode functional on mobile
- App Store / Play Store ready

---

## Phase 4: Household Management

### Objective

Complete the household coordination features including profiles, meal planning, lists, and caregiver access.

### Scope

#### 4.1 Profile Management

- [ ] **4.1.1** Profile creation and editing
- [ ] **4.1.2** Avatar/photo upload
- [ ] **4.1.3** Color selection
- [ ] **4.1.4** Birthday tracking
- [ ] **4.1.5** Caregiver info section
- [ ] **4.1.6** Pet profiles
- [ ] **4.1.7** Role assignment

#### 4.2 Meal Planning Context

- [ ] **4.2.1** Weekly meal plan view
- [ ] **4.2.2** Meal slots (breakfast, lunch, dinner, snacks)
- [ ] **4.2.3** Recipe storage
- [ ] **4.2.4** Recipe import from URLs
- [ ] **4.2.5** Ingredient management
- [ ] **4.2.6** "Add to grocery list" action
- [ ] **4.2.7** Per-profile dietary preferences
- [ ] **4.2.8** Meal plan templates

#### 4.3 List Management Context

- [ ] **4.3.1** Custom list creation
- [ ] **4.3.2** List templates (grocery, packing, etc.)
- [ ] **4.3.3** Checklist view
- [ ] **4.3.4** Board/kanban view
- [ ] **4.3.5** Category grouping
- [ ] **4.3.6** Item quantities and notes
- [ ] **4.3.7** Private lists (mobile only)
- [ ] **4.3.8** List sharing

#### 4.4 Caregiver Access

- [ ] **4.4.1** Caregiver invitation flow
- [ ] **4.4.2** Time-limited access tokens
- [ ] **4.4.3** Caregiver portal (web, no app required)
- [ ] **4.4.4** View-only calendar access
- [ ] **4.4.5** Caregiver info display
- [ ] **4.4.6** Configurable visibility

#### 4.5 Display Enhancements

- [ ] **4.5.1** Meal plan widget on display
- [ ] **4.5.2** Grocery list widget
- [ ] **4.5.3** Profile quick-view
- [ ] **4.5.4** Dashboard customization

#### 4.6 Grocery Integration (Optional)

- [ ] **4.6.1** Instacart API integration (US only)
- [ ] **4.6.2** Send list to Instacart
- [ ] **4.6.3** Alternative delivery services

### Exit Criteria

- Complete profile management across all platforms
- Meal planning functional with recipe storage
- Lists usable for grocery and custom purposes
- Caregivers can access via web link
- All features sync across devices

---

## Phase 5: Intelligence Layer

### Objective

Add AI-powered features that reduce manual data entry and provide intelligent suggestions.

### Scope

#### 5.1 Magic Import Service

- [ ] **5.1.1** Email forwarding inbox
- [ ] **5.1.2** Email parsing (ics attachments, event detection)
- [ ] **5.1.3** Photo/screenshot OCR
- [ ] **5.1.4** PDF parsing (school schedules)
- [ ] **5.1.5** Natural language text parsing
- [ ] **5.1.6** URL scraping (event pages)
- [ ] **5.1.7** Approval queue UI
- [ ] **5.1.8** Batch approval

#### 5.2 Content Extraction

- [ ] **5.2.1** Event extraction (date, time, location, title)
- [ ] **5.2.2** Recurring pattern detection
- [ ] **5.2.3** Meal/recipe extraction from photos
- [ ] **5.2.4** List item extraction from photos
- [ ] **5.2.5** Handwriting recognition

#### 5.3 AI Suggestions

- [ ] **5.3.1** Meal plan suggestions (based on preferences)
- [ ] **5.3.2** Chore assignment suggestions
- [ ] **5.3.3** Schedule conflict detection
- [ ] **5.3.4** Routine optimization tips
- [ ] **5.3.5** Grocery list suggestions (from meal plan)

#### 5.4 Learning Pipeline

- [ ] **5.4.1** Feedback collection (approvals/rejections)
- [ ] **5.4.2** Model fine-tuning (if self-hosted)
- [ ] **5.4.3** Privacy-preserving learning

#### 5.5 Voice Input (Optional, Constrained)

- [ ] **5.5.1** Wake word detection (local)
- [ ] **5.5.2** Voice-to-text for specific commands
- [ ] **5.5.3** "Add event" voice command
- [ ] **5.5.4** "Add to list" voice command
- [ ] **5.5.5** No open-ended queries (zero distraction)

### Exit Criteria

- Emails can be forwarded to create calendar events
- Photos of schedules are parsed with high accuracy
- Approval queue prevents unwanted automatic changes
- AI suggestions are helpful and non-intrusive
- Privacy maintained (local processing preferred)

---

## Phase 6: Ecosystem Expansion

### Objective

Transform Luminous into a platform that supports community contributions, third-party integrations, and extensibility.

### Scope

#### 6.1 Plugin Architecture

- [ ] **6.1.1** Plugin manifest format
- [ ] **6.1.2** Plugin sandboxing
- [ ] **6.1.3** Plugin lifecycle management
- [ ] **6.1.4** Widget plugins (custom display widgets)
- [ ] **6.1.5** Integration plugins (new calendar providers)
- [ ] **6.1.6** Plugin marketplace/registry

#### 6.2 API Gateway

- [ ] **6.2.1** Public REST API
- [ ] **6.2.2** GraphQL API (optional)
- [ ] **6.2.3** API documentation (OpenAPI)
- [ ] **6.2.4** API key management
- [ ] **6.2.5** Rate limiting
- [ ] **6.2.6** Webhook support

#### 6.3 Additional Integrations

- [ ] **6.3.1** Microsoft 365 two-way sync
- [ ] **6.3.2** iCloud CalDAV two-way sync
- [ ] **6.3.3** TeamSnap integration
- [ ] **6.3.4** School calendar integrations
- [ ] **6.3.5** Home automation (Home Assistant)
- [ ] **6.3.6** Smart display (non-Luminous) API

#### 6.4 Community Tools

- [ ] **6.4.1** Routine template sharing
- [ ] **6.4.2** Icon pack contributions
- [ ] **6.4.3** Theme sharing
- [ ] **6.4.4** Translation contributions (i18n)
- [ ] **6.4.5** Community forum/Discord

#### 6.5 Enterprise Features (Optional)

- [ ] **6.5.1** Multi-household support
- [ ] **6.5.2** Centralized administration
- [ ] **6.5.3** SSO/SAML integration
- [ ] **6.5.4** Audit logging

### Exit Criteria

- Third-party developers can create plugins
- Public API enables automation and integrations
- Community can contribute routines, icons, themes
- Internationalization supports major languages
- Platform is self-sustaining with community contributions

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
| SQLite | Durable Objects (Cloudflare) | Managed sync infrastructure |
| React | React 19 Server Components | Improved performance |
| Manual Themes | AI-generated themes | Personalization |

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
