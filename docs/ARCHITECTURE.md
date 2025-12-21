# Luminous Architecture Document

> **Document Version:** 1.0.0
> **Last Updated:** 2025-12-21
> **Status:** Draft
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
9. [Deployment Architecture](#deployment-architecture)
10. [Architecture Decisions](#architecture-decisions)

---

## Architecture Principles

Following TOGAF Enterprise Architecture principles, Luminous adheres to these foundational principles across all architecture domains:

### Business Principles

| Principle | Rationale | Implications |
|-----------|-----------|--------------|
| **BP-1: Family-Centric Design** | All features must serve real family coordination needs | Features are validated against family use cases; no feature creep toward entertainment |
| **BP-2: Zero Distraction** | The product differentiates by what it omits | Explicitly exclude entertainment, browsing, and social features |
| **BP-3: Inclusive by Default** | Families include members of all ages and abilities | Design for youngest (6+) and oldest users; accessibility is mandatory |
| **BP-4: Privacy as a Feature** | Families trust us with sensitive information | Self-hosting option; no mandatory telemetry; data minimization |

### Data Principles

| Principle | Rationale | Implications |
|-----------|-----------|--------------|
| **DP-1: Local-First Data** | Reliability requires offline capability | Data stored locally first; sync is secondary; no cloud dependency |
| **DP-2: User Data Ownership** | Families own their information | Export in open formats; no lock-in; self-hosting support |
| **DP-3: Single Source of Truth** | Conflicts create confusion | Clear ownership rules per data type; deterministic conflict resolution |
| **DP-4: Data Minimization** | Collect only what's necessary | No tracking; minimal metadata; privacy by design |

### Application Principles

| Principle | Rationale | Implications |
|-----------|-----------|--------------|
| **AP-1: Modular Composability** | Features evolve independently | Bounded contexts; clean interfaces; plugin architecture |
| **AP-2: Cross-Platform Consistency** | Experience must be unified | Shared design system; consistent behavior across display/mobile/web |
| **AP-3: Graceful Degradation** | Partial failures shouldn't block usage | Offline mode; fallback behaviors; error boundaries |
| **AP-4: Performance on Constraints** | Target hardware is modest | Optimize for Raspberry Pi 4; minimize memory; efficient rendering |

### Technology Principles

| Principle | Rationale | Implications |
|-----------|-----------|--------------|
| **TP-1: Open Source Stack** | Aligned with project values; community contribution | No proprietary dependencies in core; permissive or copyleft licenses |
| **TP-2: Commodity Hardware** | Accessibility and affordability | ARM and x86 support; no specialized hardware requirements |
| **TP-3: Standards-Based Integration** | Interoperability with existing systems | OAuth 2.0; CalDAV/ICS; REST/GraphQL APIs; OpenAPI specs |
| **TP-4: Operational Simplicity** | Self-hosters need low maintenance | Auto-updates; self-healing; minimal configuration |

---

## Architecture Vision

### Target State Architecture

```
+------------------------------------------------------------------+
|                      LUMINOUS ECOSYSTEM                           |
+------------------------------------------------------------------+
|                                                                    |
|  +-------------------+  +-------------------+  +----------------+  |
|  |   DISPLAY APP     |  |    MOBILE APP     |  |    WEB APP     |  |
|  |   (Kiosk Mode)    |  |   (iOS/Android)   |  |   (Browser)    |  |
|  +--------+----------+  +--------+----------+  +-------+--------+  |
|           |                      |                     |           |
|           +----------------------+---------------------+           |
|                                  |                                 |
|                    +-------------v--------------+                  |
|                    |       LUMINOUS CORE        |                  |
|                    |    (Shared Domain Logic)   |                  |
|                    +-------------+--------------+                  |
|                                  |                                 |
|           +----------------------+---------------------+           |
|           |                      |                     |           |
|  +--------v----------+  +--------v----------+  +-------v--------+  |
|  |   LOCAL STORAGE   |  |    SYNC ENGINE    |  |  INTEGRATIONS  |  |
|  |   (SQLite/OPFS)   |  |   (CRDT-based)    |  |   (Calendar,   |  |
|  +-------------------+  +-------------------+  |    Weather)    |  |
|                                                +----------------+  |
|                                                                    |
+------------------------------------------------------------------+
                                  |
                                  v
+------------------------------------------------------------------+
|                    OPTIONAL SYNC SERVER                           |
|  +-------------------+  +-------------------+  +----------------+  |
|  |   AUTH SERVICE    |  |   SYNC SERVICE    |  |  PUSH SERVICE  |  |
|  +-------------------+  +-------------------+  +----------------+  |
+------------------------------------------------------------------+
```

### Key Architecture Characteristics

| Characteristic | Priority | Description |
|----------------|----------|-------------|
| **Reliability** | Critical | 24/7 operation; auto-recovery; crash resilience |
| **Usability** | Critical | Child-friendly; glanceable; intuitive |
| **Performance** | High | Fast rendering; low latency; efficient memory |
| **Scalability** | Medium | Support large families; many calendars |
| **Portability** | High | Cross-platform; commodity hardware |
| **Maintainability** | High | Clean code; comprehensive testing; documentation |

---

## Business Architecture

### Business Capability Model

```
LUMINOUS FAMILY HUB
├── SCHEDULING CAPABILITY
│   ├── Calendar Aggregation
│   ├── Event Management
│   ├── Reminder Management
│   └── Schedule Sharing
│
├── TASK MANAGEMENT CAPABILITY
│   ├── Chore Management
│   ├── Routine Management
│   ├── Progress Tracking
│   └── Reward Management
│
├── HOUSEHOLD MANAGEMENT CAPABILITY
│   ├── Profile Management
│   ├── Meal Planning
│   ├── List Management
│   └── Caregiver Coordination
│
├── INFORMATION DISPLAY CAPABILITY
│   ├── Dashboard Rendering
│   ├── Notification Display
│   ├── Ambient Information
│   └── Privacy Mode
│
└── PLATFORM CAPABILITY
    ├── Authentication & Authorization
    ├── Data Synchronization
    ├── External Integration
    └── System Administration
```

### Business Process Map

#### BP-001: Daily Family Coordination

```
┌─────────────────────────────────────────────────────────────────────┐
│                    DAILY FAMILY COORDINATION                         │
├─────────────────────────────────────────────────────────────────────┤
│                                                                      │
│  ┌──────────┐    ┌──────────┐    ┌──────────┐    ┌──────────┐      │
│  │ Morning  │───▶│  View    │───▶│ Complete │───▶│ Receive  │      │
│  │  Wake    │    │  Today   │    │ Routines │    │ Rewards  │      │
│  └──────────┘    └──────────┘    └──────────┘    └──────────┘      │
│       │                                                              │
│       ▼                                                              │
│  ┌──────────┐    ┌──────────┐    ┌──────────┐                      │
│  │  Check   │───▶│  Handle  │───▶│  Update  │                      │
│  │ Calendar │    │  Events  │    │  Status  │                      │
│  └──────────┘    └──────────┘    └──────────┘                      │
│                                                                      │
└─────────────────────────────────────────────────────────────────────┘
```

### Actor-Role Matrix

| Actor | Calendar | Chores | Lists | Profiles | Settings |
|-------|----------|--------|-------|----------|----------|
| **Household Admin** | Full | Full | Full | Full | Full |
| **Adult Member** | Full | Full | Full | Own | View |
| **Teen Member** | Own + Family | Own | Shared | Own | None |
| **Child Member** | View | Complete | View | View | None |
| **External Caregiver** | View (limited) | View | View | View (care info) | None |

---

## Data Architecture

### Domain Model

```
┌─────────────────────────────────────────────────────────────────────┐
│                         DOMAIN MODEL                                 │
├─────────────────────────────────────────────────────────────────────┤
│                                                                      │
│  ┌─────────────────┐         ┌─────────────────┐                    │
│  │    HOUSEHOLD    │────────▶│     PROFILE     │                    │
│  │    (Aggregate)  │  1   *  │    (Entity)     │                    │
│  └────────┬────────┘         └────────┬────────┘                    │
│           │                           │                              │
│           │ 1                         │ *                            │
│           ▼                           ▼                              │
│  ┌─────────────────┐         ┌─────────────────┐                    │
│  │   CALENDAR      │         │     CHORE       │                    │
│  │   CONNECTION    │         │   (Entity)      │                    │
│  │   (Entity)      │         └────────┬────────┘                    │
│  └────────┬────────┘                  │                              │
│           │                           │                              │
│           │ *                         │ *                            │
│           ▼                           ▼                              │
│  ┌─────────────────┐         ┌─────────────────┐                    │
│  │     EVENT       │         │   COMPLETION    │                    │
│  │   (Entity)      │         │   (Value Obj)   │                    │
│  └─────────────────┘         └─────────────────┘                    │
│                                                                      │
│  ┌─────────────────┐         ┌─────────────────┐                    │
│  │    ROUTINE      │────────▶│   ROUTINE STEP  │                    │
│  │   (Aggregate)   │  1   *  │  (Value Obj)    │                    │
│  └─────────────────┘         └─────────────────┘                    │
│                                                                      │
│  ┌─────────────────┐         ┌─────────────────┐                    │
│  │      LIST       │────────▶│   LIST ITEM     │                    │
│  │   (Aggregate)   │  1   *  │   (Entity)      │                    │
│  └─────────────────┘         └─────────────────┘                    │
│                                                                      │
│  ┌─────────────────┐         ┌─────────────────┐                    │
│  │   MEAL PLAN     │────────▶│     RECIPE      │                    │
│  │   (Aggregate)   │  *   *  │   (Entity)      │                    │
│  └─────────────────┘         └─────────────────┘                    │
│                                                                      │
└─────────────────────────────────────────────────────────────────────┘
```

### Core Entities

#### Household (Aggregate Root)

```typescript
interface Household {
  id: HouseholdId;
  name: string;
  timezone: Timezone;
  createdAt: DateTime;
  settings: HouseholdSettings;
  profiles: Profile[];
  calendarConnections: CalendarConnection[];
}
```

#### Profile

```typescript
interface Profile {
  id: ProfileId;
  householdId: HouseholdId;
  displayName: string;
  avatarUrl?: string;
  color: HexColor;
  role: ProfileRole; // ADMIN | ADULT | TEEN | CHILD | PET
  birthday?: LocalDate;
  caregiverInfo?: CaregiverInfo;
  permissions: Permission[];
  pinHash?: string;
  createdAt: DateTime;
}

interface CaregiverInfo {
  allergies: string[];
  medications: string[];
  emergencyContacts: Contact[];
  doctorInfo?: Contact;
  notes: string;
}
```

#### Event

```typescript
interface Event {
  id: EventId;
  householdId: HouseholdId;
  calendarConnectionId?: CalendarConnectionId;
  externalId?: string;
  title: string;
  description?: string;
  location?: Location;
  startTime: DateTime;
  endTime: DateTime;
  allDay: boolean;
  recurrence?: RecurrenceRule;
  assignedProfiles: ProfileId[];
  reminders: Reminder[];
  isCountdown: boolean;
  createdAt: DateTime;
  updatedAt: DateTime;
  syncStatus: SyncStatus;
}
```

#### Chore

```typescript
interface Chore {
  id: ChoreId;
  householdId: HouseholdId;
  title: string;
  description?: string;
  icon: IconId;
  assignedProfiles: ProfileId[]; // empty = "anyone"
  recurrence?: RecurrenceRule;
  dueTime?: LocalTime;
  pointValue: number;
  priority?: Priority;
  completions: ChoreCompletion[];
  createdAt: DateTime;
}

interface ChoreCompletion {
  date: LocalDate;
  completedBy: ProfileId;
  completedAt: DateTime;
  pointsAwarded: number;
}
```

### Data Storage Strategy

| Data Type | Storage | Sync Strategy | Conflict Resolution |
|-----------|---------|---------------|---------------------|
| **Household Config** | Local SQLite | Full sync | Last-write-wins with vector clock |
| **Profiles** | Local SQLite | Full sync | Server authority |
| **Events (internal)** | Local SQLite | CRDT merge | Operational transform |
| **Events (external)** | Cached | Pull from source | Source authority |
| **Chores/Routines** | Local SQLite | CRDT merge | Operational transform |
| **Lists** | Local SQLite | CRDT merge | Append-only with tombstones |
| **Completions** | Local SQLite | Append-only | No conflicts (immutable) |
| **Media (avatars)** | Local filesystem | Hash-addressed | Content-addressed (no conflicts) |

### Data Flow Diagram

```
┌─────────────────────────────────────────────────────────────────────┐
│                       DATA FLOW ARCHITECTURE                         │
├─────────────────────────────────────────────────────────────────────┤
│                                                                      │
│                         EXTERNAL SOURCES                             │
│    ┌──────────┐  ┌──────────┐  ┌──────────┐  ┌──────────┐          │
│    │  Google  │  │  Outlook │  │  iCloud  │  │   ICS    │          │
│    │ Calendar │  │ Calendar │  │ Calendar │  │  Feeds   │          │
│    └────┬─────┘  └────┬─────┘  └────┬─────┘  └────┬─────┘          │
│         │             │             │             │                  │
│         └─────────────┴──────┬──────┴─────────────┘                  │
│                              ▼                                       │
│                    ┌─────────────────┐                              │
│                    │   SYNC ENGINE   │                              │
│                    │   (Adapter per  │                              │
│                    │    provider)    │                              │
│                    └────────┬────────┘                              │
│                             │                                        │
│                             ▼                                        │
│    ┌────────────────────────────────────────────────────────┐       │
│    │                   LOCAL DATA STORE                      │       │
│    │  ┌──────────┐  ┌──────────┐  ┌──────────┐             │       │
│    │  │  Events  │  │  Chores  │  │   Lists  │  ...        │       │
│    │  └──────────┘  └──────────┘  └──────────┘             │       │
│    └────────────────────────┬───────────────────────────────┘       │
│                             │                                        │
│         ┌───────────────────┼───────────────────┐                   │
│         ▼                   ▼                   ▼                   │
│  ┌────────────┐     ┌────────────┐     ┌────────────┐              │
│  │  DISPLAY   │     │   MOBILE   │     │    WEB     │              │
│  │    APP     │◀───▶│    APP     │◀───▶│    APP     │              │
│  └────────────┘     └────────────┘     └────────────┘              │
│         │                   │                   │                   │
│         └───────────────────┴───────────────────┘                   │
│                             │                                        │
│                             ▼                                        │
│                    ┌─────────────────┐                              │
│                    │   SYNC SERVER   │ (Optional)                   │
│                    │  (Multi-device) │                              │
│                    └─────────────────┘                              │
│                                                                      │
└─────────────────────────────────────────────────────────────────────┘
```

---

## Application Architecture

### Bounded Contexts

Following Domain-Driven Design, Luminous is organized into these bounded contexts:

```
┌─────────────────────────────────────────────────────────────────────┐
│                      BOUNDED CONTEXTS                                │
├─────────────────────────────────────────────────────────────────────┤
│                                                                      │
│  ┌──────────────────┐  ┌──────────────────┐  ┌──────────────────┐  │
│  │    SCHEDULING    │  │  TASK MANAGEMENT │  │    HOUSEHOLD     │  │
│  │     CONTEXT      │  │     CONTEXT      │  │     CONTEXT      │  │
│  │                  │  │                  │  │                  │  │
│  │ - Calendar       │  │ - Chore          │  │ - Profile        │  │
│  │ - Event          │  │ - Routine        │  │ - Household      │  │
│  │ - CalendarConn   │  │ - Completion     │  │ - CaregiverInfo  │  │
│  │ - Reminder       │  │ - Reward         │  │ - Permission     │  │
│  └──────────────────┘  └──────────────────┘  └──────────────────┘  │
│                                                                      │
│  ┌──────────────────┐  ┌──────────────────┐  ┌──────────────────┐  │
│  │   MEAL PLANNING  │  │  LIST MANAGEMENT │  │     DISPLAY      │  │
│  │     CONTEXT      │  │     CONTEXT      │  │     CONTEXT      │  │
│  │                  │  │                  │  │                  │  │
│  │ - MealPlan       │  │ - List           │  │ - Dashboard      │  │
│  │ - Recipe         │  │ - ListItem       │  │ - Widget         │  │
│  │ - Ingredient     │  │ - Template       │  │ - PrivacyMode    │  │
│  │ - DietaryPref    │  │                  │  │ - SleepMode      │  │
│  └──────────────────┘  └──────────────────┘  └──────────────────┘  │
│                                                                      │
│  ┌──────────────────┐  ┌──────────────────┐                        │
│  │   MAGIC IMPORT   │  │   INTEGRATION    │                        │
│  │     CONTEXT      │  │     CONTEXT      │                        │
│  │                  │  │                  │                        │
│  │ - ImportRequest  │  │ - CalendarSync   │                        │
│  │ - ParsedContent  │  │ - WeatherFetch   │                        │
│  │ - ApprovalQueue  │  │ - InstacartLink  │                        │
│  │                  │  │ - PushNotify     │                        │
│  └──────────────────┘  └──────────────────┘                        │
│                                                                      │
└─────────────────────────────────────────────────────────────────────┘
```

### Application Component Model

```
┌─────────────────────────────────────────────────────────────────────┐
│                    APPLICATION COMPONENTS                            │
├─────────────────────────────────────────────────────────────────────┤
│                                                                      │
│  PRESENTATION LAYER                                                  │
│  ┌─────────────────────────────────────────────────────────────┐    │
│  │  ┌─────────┐  ┌─────────┐  ┌─────────┐  ┌─────────────┐    │    │
│  │  │ Display │  │ Mobile  │  │   Web   │  │  Caregiver  │    │    │
│  │  │   App   │  │   App   │  │   App   │  │   Portal    │    │    │
│  │  └────┬────┘  └────┬────┘  └────┬────┘  └──────┬──────┘    │    │
│  └───────┼────────────┼────────────┼──────────────┼───────────┘    │
│          │            │            │              │                  │
│          └────────────┴─────┬──────┴──────────────┘                  │
│                             │                                        │
│  SHARED UI LAYER            ▼                                        │
│  ┌─────────────────────────────────────────────────────────────┐    │
│  │                    DESIGN SYSTEM                             │    │
│  │  ┌─────────┐  ┌─────────┐  ┌─────────┐  ┌─────────────┐    │    │
│  │  │  Theme  │  │  Core   │  │  Form   │  │  Composite  │    │    │
│  │  │ Tokens  │  │ Widgets │  │ Elements│  │  Components │    │    │
│  │  └─────────┘  └─────────┘  └─────────┘  └─────────────┘    │    │
│  └─────────────────────────────────────────────────────────────┘    │
│                             │                                        │
│  DOMAIN LAYER               ▼                                        │
│  ┌─────────────────────────────────────────────────────────────┐    │
│  │                     LUMINOUS CORE                            │    │
│  │  ┌───────────┐  ┌───────────┐  ┌───────────┐               │    │
│  │  │  Domain   │  │  Domain   │  │  Domain   │  ...          │    │
│  │  │  Models   │  │ Services  │  │   Events  │               │    │
│  │  └───────────┘  └───────────┘  └───────────┘               │    │
│  └─────────────────────────────────────────────────────────────┘    │
│                             │                                        │
│  INFRASTRUCTURE LAYER       ▼                                        │
│  ┌─────────────────────────────────────────────────────────────┐    │
│  │  ┌───────────┐  ┌───────────┐  ┌───────────┐               │    │
│  │  │   Data    │  │   Sync    │  │  External │               │    │
│  │  │Repository │  │  Engine   │  │ Adapters  │               │    │
│  │  └───────────┘  └───────────┘  └───────────┘               │    │
│  └─────────────────────────────────────────────────────────────┘    │
│                                                                      │
└─────────────────────────────────────────────────────────────────────┘
```

### Module Structure

```
luminous/
├── packages/
│   ├── core/                    # Shared domain logic
│   │   ├── src/
│   │   │   ├── domain/          # Domain models and entities
│   │   │   │   ├── household/
│   │   │   │   ├── scheduling/
│   │   │   │   ├── tasks/
│   │   │   │   ├── lists/
│   │   │   │   └── meals/
│   │   │   ├── services/        # Domain services
│   │   │   ├── events/          # Domain events
│   │   │   └── ports/           # Repository interfaces
│   │   └── package.json
│   │
│   ├── ui/                      # Shared design system
│   │   ├── src/
│   │   │   ├── tokens/          # Design tokens
│   │   │   ├── primitives/      # Base components
│   │   │   ├── components/      # Composite components
│   │   │   └── themes/          # Theme definitions
│   │   └── package.json
│   │
│   ├── sync/                    # Synchronization engine
│   │   ├── src/
│   │   │   ├── crdt/            # CRDT implementations
│   │   │   ├── adapters/        # Calendar provider adapters
│   │   │   └── engine/          # Sync orchestration
│   │   └── package.json
│   │
│   └── import/                  # Magic import service
│       ├── src/
│       │   ├── parsers/         # Input parsers (email, photo, etc.)
│       │   ├── extractors/      # Content extractors
│       │   └── queue/           # Approval queue
│       └── package.json
│
├── apps/
│   ├── display/                 # Wall display application
│   │   ├── src/
│   │   │   ├── views/           # Main display views
│   │   │   ├── widgets/         # Display widgets
│   │   │   ├── kiosk/           # Kiosk mode management
│   │   │   └── store/           # State management
│   │   └── package.json
│   │
│   ├── mobile/                  # Mobile application
│   │   ├── src/
│   │   │   ├── screens/         # App screens
│   │   │   ├── navigation/      # Navigation config
│   │   │   ├── push/            # Push notification handling
│   │   │   └── store/           # State management
│   │   └── package.json
│   │
│   ├── web/                     # Web application
│   │   ├── src/
│   │   │   ├── pages/           # Page components
│   │   │   ├── layouts/         # Layout components
│   │   │   └── store/           # State management
│   │   └── package.json
│   │
│   └── server/                  # Optional sync server
│       ├── src/
│       │   ├── api/             # REST/GraphQL API
│       │   ├── auth/            # Authentication
│       │   ├── sync/            # Sync coordination
│       │   └── push/            # Push notification service
│       └── package.json
│
├── tools/                       # Development tools
│   ├── cli/                     # CLI tooling
│   └── scripts/                 # Build scripts
│
└── docs/                        # Documentation
    ├── adr/                     # Architecture Decision Records
    └── api/                     # API documentation
```

---

## Technology Architecture

### Technology Stack

| Layer | Technology | Rationale |
|-------|------------|-----------|
| **Language** | TypeScript | Type safety, tooling, ecosystem |
| **UI Framework** | React | Component model, ecosystem, cross-platform (React Native) |
| **State Management** | Zustand | Lightweight, TypeScript-first, easy testing |
| **Local Database** | SQLite (via sql.js) / OPFS | Offline-first, portable, performant |
| **Sync Protocol** | Custom CRDT-based | Conflict-free offline operation |
| **Mobile** | React Native | Code sharing with display/web |
| **Desktop/Display** | Electron or Tauri | Native kiosk capabilities |
| **Server (Optional)** | Node.js + Fastify | TypeScript consistency, performance |
| **Build System** | Turborepo | Monorepo management, caching |
| **Testing** | Vitest + Playwright | Fast unit tests, E2E coverage |

### Technology Reference Model

```
┌─────────────────────────────────────────────────────────────────────┐
│                  TECHNOLOGY REFERENCE MODEL                          │
├─────────────────────────────────────────────────────────────────────┤
│                                                                      │
│  DEVELOPMENT PLATFORM                                                │
│  ┌────────────────────────────────────────────────────────────────┐ │
│  │  TypeScript 5.x │ Node.js 20 LTS │ pnpm │ Turborepo           │ │
│  └────────────────────────────────────────────────────────────────┘ │
│                                                                      │
│  APPLICATION FRAMEWORKS                                              │
│  ┌────────────────────────────────────────────────────────────────┐ │
│  │  React 19 │ React Native │ Electron/Tauri │ Fastify            │ │
│  └────────────────────────────────────────────────────────────────┘ │
│                                                                      │
│  UI/UX INFRASTRUCTURE                                                │
│  ┌────────────────────────────────────────────────────────────────┐ │
│  │  Tailwind CSS │ Radix UI │ Framer Motion │ React Aria          │ │
│  └────────────────────────────────────────────────────────────────┘ │
│                                                                      │
│  DATA INFRASTRUCTURE                                                 │
│  ┌────────────────────────────────────────────────────────────────┐ │
│  │  SQLite (sql.js) │ OPFS │ IndexedDB │ Yjs (CRDT)              │ │
│  └────────────────────────────────────────────────────────────────┘ │
│                                                                      │
│  INTEGRATION INFRASTRUCTURE                                          │
│  ┌────────────────────────────────────────────────────────────────┐ │
│  │  OAuth 2.0 │ CalDAV │ REST │ WebSocket │ Web Push             │ │
│  └────────────────────────────────────────────────────────────────┘ │
│                                                                      │
│  QUALITY INFRASTRUCTURE                                              │
│  ┌────────────────────────────────────────────────────────────────┐ │
│  │  Vitest │ Playwright │ ESLint │ Prettier │ TypeScript ESLint  │ │
│  └────────────────────────────────────────────────────────────────┘ │
│                                                                      │
│  DEPLOYMENT INFRASTRUCTURE                                           │
│  ┌────────────────────────────────────────────────────────────────┐ │
│  │  Docker │ GitHub Actions │ Renovate │ Changesets              │ │
│  └────────────────────────────────────────────────────────────────┘ │
│                                                                      │
└─────────────────────────────────────────────────────────────────────┘
```

### Platform Matrix

| Platform | Runtime | Database | Notes |
|----------|---------|----------|-------|
| **Display (Linux)** | Electron | SQLite | Kiosk mode, ARM64 support |
| **Display (Raspberry Pi)** | Electron/Chromium | SQLite | Optimized for Pi 4/5 |
| **Mobile (iOS)** | React Native | SQLite | App Store distribution |
| **Mobile (Android)** | React Native | SQLite | Play Store + sideload |
| **Web** | Browser | OPFS/IndexedDB | Progressive Web App |
| **Server** | Node.js | PostgreSQL/SQLite | Docker deployment |

---

## Security Architecture

### Security Model

```
┌─────────────────────────────────────────────────────────────────────┐
│                       SECURITY ARCHITECTURE                          │
├─────────────────────────────────────────────────────────────────────┤
│                                                                      │
│  AUTHENTICATION                                                      │
│  ┌────────────────────────────────────────────────────────────────┐ │
│  │                                                                │  │
│  │  ┌──────────────┐   ┌──────────────┐   ┌──────────────┐      │  │
│  │  │ Local PIN    │   │  OAuth 2.0   │   │   Passkey    │      │  │
│  │  │ (Display)    │   │  (Mobile)    │   │  (Future)    │      │  │
│  │  └──────────────┘   └──────────────┘   └──────────────┘      │  │
│  │                                                                │  │
│  └────────────────────────────────────────────────────────────────┘ │
│                                                                      │
│  AUTHORIZATION                                                       │
│  ┌────────────────────────────────────────────────────────────────┐ │
│  │                                                                │  │
│  │  Role-Based Access Control (RBAC)                             │  │
│  │  ┌────────────────────────────────────────────────────────┐   │  │
│  │  │  ADMIN → ADULT → TEEN → CHILD → CAREGIVER (External)  │   │  │
│  │  └────────────────────────────────────────────────────────┘   │  │
│  │                                                                │  │
│  └────────────────────────────────────────────────────────────────┘ │
│                                                                      │
│  DATA PROTECTION                                                     │
│  ┌────────────────────────────────────────────────────────────────┐ │
│  │                                                                │  │
│  │  ┌──────────────┐   ┌──────────────┐   ┌──────────────┐      │  │
│  │  │ Encryption   │   │ Encryption   │   │   Secure     │      │  │
│  │  │ at Rest      │   │ in Transit   │   │   Backup     │      │  │
│  │  │ (SQLCipher)  │   │ (TLS 1.3)    │   │              │      │  │
│  │  └──────────────┘   └──────────────┘   └──────────────┘      │  │
│  │                                                                │  │
│  └────────────────────────────────────────────────────────────────┘ │
│                                                                      │
└─────────────────────────────────────────────────────────────────────┘
```

### Role Permission Matrix

| Capability | Admin | Adult | Teen | Child | Caregiver |
|------------|-------|-------|------|-------|-----------|
| View all calendars | Yes | Yes | Limited | Limited | Limited |
| Edit events | Yes | Yes | Own | No | No |
| Create chores | Yes | Yes | No | No | No |
| Complete chores | Yes | Yes | Own | Own | No |
| Manage profiles | Yes | No | No | No | No |
| View caregiver info | Yes | Yes | No | No | Own-assigned |
| System settings | Yes | No | No | No | No |
| Invite caregivers | Yes | Yes | No | No | No |

### Security Requirements

| Category | Requirement | Implementation |
|----------|-------------|----------------|
| **Authentication** | Multi-factor for admin actions | PIN + device auth |
| **Session** | Automatic timeout on mobile | 15-minute idle logout |
| **Data** | Encryption at rest | SQLCipher for local data |
| **Transit** | TLS for all connections | TLS 1.3 minimum |
| **Secrets** | No secrets in code | Environment variables, secret manager |
| **Dependencies** | Regular vulnerability scans | Renovate + npm audit |
| **Audit** | Log security-relevant actions | Immutable audit log |

---

## Integration Architecture

### External Integration Points

```
┌─────────────────────────────────────────────────────────────────────┐
│                    INTEGRATION ARCHITECTURE                          │
├─────────────────────────────────────────────────────────────────────┤
│                                                                      │
│                          LUMINOUS                                    │
│                             │                                        │
│       ┌─────────────────────┼─────────────────────┐                 │
│       │                     │                     │                 │
│       ▼                     ▼                     ▼                 │
│  ┌─────────────┐     ┌─────────────┐     ┌─────────────┐           │
│  │  CALENDAR   │     │   WEATHER   │     │   GROCERY   │           │
│  │ PROVIDERS   │     │   SERVICE   │     │  DELIVERY   │           │
│  ├─────────────┤     ├─────────────┤     ├─────────────┤           │
│  │ Google Cal  │     │ OpenWeather │     │  Instacart  │           │
│  │ Outlook/M365│     │   NWS API   │     │   (Future)  │           │
│  │ iCloud      │     │   Pirate    │     │             │           │
│  │ CalDAV      │     │   Weather   │     │             │           │
│  │ ICS URLs    │     │             │     │             │           │
│  └─────────────┘     └─────────────┘     └─────────────┘           │
│                                                                      │
│       ┌─────────────────────┼─────────────────────┐                 │
│       │                     │                     │                 │
│       ▼                     ▼                     ▼                 │
│  ┌─────────────┐     ┌─────────────┐     ┌─────────────┐           │
│  │    PUSH     │     │   MAGIC     │     │    MAPS     │           │
│  │   NOTIFY    │     │   IMPORT    │     │   SERVICE   │           │
│  ├─────────────┤     ├─────────────┤     ├─────────────┤           │
│  │ Apple APNS  │     │ OCR Service │     │ OpenStreet  │           │
│  │ Google FCM  │     │ Email Parse │     │   Map       │           │
│  │ Web Push    │     │ LLM Extract │     │             │           │
│  └─────────────┘     └─────────────┘     └─────────────┘           │
│                                                                      │
└─────────────────────────────────────────────────────────────────────┘
```

### Calendar Integration Patterns

| Provider | Protocol | Sync Direction | Auth Method |
|----------|----------|----------------|-------------|
| **Google Calendar** | REST API | Two-way | OAuth 2.0 |
| **Microsoft 365** | Graph API | Two-way | OAuth 2.0 |
| **iCloud** | CalDAV | Read-only (initially) | App-specific password |
| **CalDAV Generic** | CalDAV | Read-only | Basic / OAuth |
| **ICS URL** | HTTP(S) | Read-only | None / Basic |

### Integration API Design

All external integrations follow the Adapter pattern:

```typescript
interface CalendarAdapter {
  readonly providerId: CalendarProviderId;

  authenticate(credentials: AuthCredentials): Promise<AuthSession>;
  refreshToken(session: AuthSession): Promise<AuthSession>;

  listCalendars(session: AuthSession): Promise<ExternalCalendar[]>;

  fetchEvents(
    session: AuthSession,
    calendarId: string,
    range: DateRange
  ): Promise<ExternalEvent[]>;

  createEvent?(
    session: AuthSession,
    calendarId: string,
    event: EventCreateInput
  ): Promise<ExternalEvent>;

  updateEvent?(
    session: AuthSession,
    calendarId: string,
    eventId: string,
    event: EventUpdateInput
  ): Promise<ExternalEvent>;

  deleteEvent?(
    session: AuthSession,
    calendarId: string,
    eventId: string
  ): Promise<void>;
}
```

---

## Deployment Architecture

### Self-Hosted Deployment

```
┌─────────────────────────────────────────────────────────────────────┐
│                   SELF-HOSTED DEPLOYMENT                             │
├─────────────────────────────────────────────────────────────────────┤
│                                                                      │
│  HOME NETWORK                                                        │
│  ┌────────────────────────────────────────────────────────────────┐ │
│  │                                                                │  │
│  │  ┌────────────────┐         ┌────────────────┐                │  │
│  │  │  WALL DISPLAY  │         │  SYNC SERVER   │  (Optional)   │  │
│  │  │  (Kiosk Mode)  │◀───────▶│  (Raspberry Pi │                │  │
│  │  │                │   LAN   │   or NAS)      │                │  │
│  │  └────────────────┘         └────────┬───────┘                │  │
│  │                                      │                         │  │
│  │                                      │ LAN                     │  │
│  │  ┌────────────────┐                  │                         │  │
│  │  │  MOBILE APPS   │◀─────────────────┘                         │  │
│  │  │  (on WiFi)     │                                            │  │
│  │  └────────────────┘                                            │  │
│  │                                                                │  │
│  └────────────────────────────────────────────────────────────────┘ │
│                            │                                         │
│                            │ Internet (for calendar sync)           │
│                            ▼                                         │
│  ┌────────────────────────────────────────────────────────────────┐ │
│  │  EXTERNAL SERVICES                                              │ │
│  │  ┌──────────┐  ┌──────────┐  ┌──────────┐  ┌──────────┐       │ │
│  │  │ Google   │  │ Outlook  │  │ Weather  │  │  Push    │       │ │
│  │  │ Calendar │  │ Calendar │  │   API    │  │ Services │       │ │
│  │  └──────────┘  └──────────┘  └──────────┘  └──────────┘       │ │
│  └────────────────────────────────────────────────────────────────┘ │
│                                                                      │
└─────────────────────────────────────────────────────────────────────┘
```

### Display Hardware Specifications

| Tier | Device | RAM | Storage | Notes |
|------|--------|-----|---------|-------|
| **Budget** | Raspberry Pi 4 | 4GB | 32GB SD | Minimum viable |
| **Recommended** | Raspberry Pi 5 | 8GB | 64GB SD | Smooth experience |
| **Premium** | Intel NUC / Mini PC | 8GB+ | 128GB SSD | Best performance |
| **Tablet** | iPad / Android Tablet | 4GB+ | 64GB+ | No kiosk hardware needed |

### Container Deployment (Server)

```yaml
# docker-compose.yml (example)
version: '3.8'
services:
  luminous-server:
    image: luminous/server:latest
    ports:
      - "3000:3000"
    volumes:
      - luminous-data:/data
    environment:
      - DATABASE_URL=file:/data/luminous.db
      - PUSH_VAPID_PUBLIC_KEY=${VAPID_PUBLIC}
      - PUSH_VAPID_PRIVATE_KEY=${VAPID_PRIVATE}
    restart: unless-stopped

volumes:
  luminous-data:
```

---

## Architecture Decisions

All significant architecture decisions are documented as Architecture Decision Records (ADRs) in the `/docs/adr/` directory.

### ADR Index

| ADR | Title | Status | Date |
|-----|-------|--------|------|
| [ADR-001](./adr/ADR-001-typescript-primary-language.md) | TypeScript as Primary Language | Accepted | 2025-12-21 |
| [ADR-002](./adr/ADR-002-react-ui-framework.md) | React as UI Framework | Accepted | 2025-12-21 |
| [ADR-003](./adr/ADR-003-local-first-architecture.md) | Local-First Data Architecture | Accepted | 2025-12-21 |
| [ADR-004](./adr/ADR-004-two-way-sync-google-first.md) | Two-Way Sync Initially Google-Only | Accepted | 2025-12-21 |
| [ADR-005](./adr/ADR-005-magic-import-approval.md) | Magic Import Requires Approval | Accepted | 2025-12-21 |
| [ADR-006](./adr/ADR-006-zero-distraction-principle.md) | Zero-Distraction Design Principle | Accepted | 2025-12-21 |
| [ADR-007](./adr/ADR-007-self-hosting-first.md) | Self-Hosting as Primary Model | Accepted | 2025-12-21 |

---

## Related Documents

- [Project Overview](./PROJECT-OVERVIEW.md)
- [Development Roadmap](./ROADMAP.md)
- [Architecture Decision Records](./adr/)
- [CLAUDE.md (Development Guidelines)](../CLAUDE.md)

---

## Document History

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0.0 | 2025-12-21 | Luminous Team | Initial architecture document |
