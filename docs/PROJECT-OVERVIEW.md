# Luminous Project Overview

> **Document Version:** 2.0.0
> **Last Updated:** 2025-12-21
> **Status:** Draft
> **TOGAF Phase:** Architecture Vision (Phase A)

---

## Table of Contents

1. [Executive Summary](#executive-summary)
2. [Business Context](#business-context)
3. [Product Definition](#product-definition)
4. [Deployment Model](#deployment-model)
5. [Stakeholders and Personas](#stakeholders-and-personas)
6. [Core UX Principles](#core-ux-principles)
7. [Functional Scope](#functional-scope)
8. [Non-Functional Requirements](#non-functional-requirements)
9. [Design Constraints](#design-constraints)
10. [Key Design Decisions](#key-design-decisions)
11. [Success Metrics](#success-metrics)
12. [Glossary](#glossary)

---

## Executive Summary

**Luminous** is a wall-mounted, always-on, distraction-free family hub designed for large portrait-mounted touchscreens. It provides a calm, glanceable view of household schedules, tasks, chores, meal plans, and shared lists, making family coordination visible, actionable, and accessible to all ages.

### Vision Statement

> To create the calm center of household coordination - a single, always-visible source of truth that brings families together through shared awareness and reduced mental load.

### Mission

Luminous aims to eliminate the chaos of family scheduling by providing:
- A unified view of all household activities
- Kid-friendly interfaces that encourage participation
- Caregiver sharing for extended family coordination
- Zero-distraction design that respects attention and focus

---

## Business Context

### Problem Statement

Modern families struggle with:
- **Fragmented information**: Schedules scattered across multiple calendars, apps, and paper
- **Communication gaps**: Caregivers, grandparents, and babysitters lack visibility
- **Chore coordination**: No clear accountability for household tasks
- **Mental load**: One person typically carries the burden of family logistics
- **Screen addiction**: Smart displays often become entertainment devices

### Market Opportunity

The family organization space is served by products like Skylight, Hearth, and Cozi, each with limitations:
- Proprietary hardware lock-in
- Limited integration capabilities
- Subscription fatigue
- Lack of open-source alternatives

### Strategic Goals

| Goal | Description | Priority |
|------|-------------|----------|
| SG-1 | Provide a cloud-hosted family command center with Azure | Critical |
| SG-2 | Support multi-tenant architecture for family sign-up | Critical |
| SG-3 | Enable native mobile apps for iOS and Android | High |
| SG-4 | Support diverse hardware for display (tablets, mini PCs) | High |
| SG-5 | Enable local development alongside cloud deployment | Medium |
| SG-6 | Build a community of contributors and users | Medium |

---

## Product Definition

### Core Value Proposition

A wall-mounted, always-on, distraction-free family hub that makes schedules, chores/routines, meal plans, and shared lists visible, actionable, and kid-friendly, with mobile companion control and caregiver sharing.

### Design Constraint: Zero Distraction

**Critical Design Decision:** Luminous explicitly excludes:
- Web browsing capabilities
- App installation
- Video streaming
- Social media integration
- Voice assistants with open-ended queries
- Games or entertainment features

This constraint is intentional and strategic, aligning with research showing that multi-purpose smart displays often become sources of distraction rather than productivity.

---

## Deployment Model

### Cloud-Hosted Multi-Tenant Platform

Luminous is deployed as a cloud-hosted platform on Microsoft Azure, supporting multiple families (tenants) with complete data isolation.

#### Platform Architecture

| Component | Description | Azure Service |
|-----------|-------------|---------------|
| **Web Application** | Angular-based web interface | Azure Static Web Apps |
| **API Backend** | .NET 10 REST API | Azure App Service |
| **Real-time Sync** | WebSocket-based synchronization | Azure SignalR Service |
| **Data Storage** | Document database per family | Azure Cosmos DB |
| **File Storage** | Media and file uploads | Azure Blob Storage |
| **Identity** | User authentication and authorization | Azure AD B2C |
| **Background Jobs** | Calendar sync, import processing | Azure Functions |

#### Multi-Tenancy Model

Each family represents a **tenant** with complete data isolation:

1. **Sign-Up Flow**: Users register via Azure AD B2C (email/password or social login)
2. **Family Creation**: First user creates a family (tenant) and becomes the Owner
3. **Member Invitation**: Owner invites family members via email or link
4. **Device Linking**: Wall displays are linked to families via 6-digit codes

#### Device Linking Flow

```
+------------------+                    +------------------+
|   DISPLAY APP    |                    |    MOBILE APP    |
+------------------+                    +------------------+
        |                                       |
        | 1. Display shows                      |
        |    6-digit link code                  |
        |    (expires in 15 min)                |
        |                                       |
        |                               2. User logs in
        |                                  via Azure AD B2C
        |                                       |
        |                               3. User enters
        |                                  link code
        |                                       |
        |       4. API validates code           |
        |          and links device             |
        |          to family                    |
        |<--------------------------------------|
        |                                       |
        | 5. Display receives                   |
        |    device token and                   |
        |    syncs family data                  |
        |                                       |
+------------------+                    +------------------+
```

### Native Mobile Applications

Companion apps are built as **native applications** for optimal user experience:

| Platform | Technology | Distribution |
|----------|------------|--------------|
| **iOS** | Swift / SwiftUI | Apple App Store |
| **Android** | Kotlin / Jetpack Compose | Google Play Store |

Native apps provide:
- Push notifications via APNs (iOS) and FCM (Android)
- Biometric authentication
- Offline capability with local caching
- Platform-specific UI/UX patterns

### Local Development Environment

Developers can run the full stack locally using:

| Service | Local Alternative |
|---------|-------------------|
| Cosmos DB | Cosmos DB Emulator |
| Blob Storage | Azurite |
| Redis Cache | Docker Redis |
| Azure Functions | Azure Functions Core Tools |
| Azure AD B2C | Dev B2C tenant or local JWT issuer |

See [Local Development Guide](./DEVELOPMENT.md) for setup instructions.

---

## Stakeholders and Personas

### Primary Stakeholders

| Stakeholder | Role | Concerns | Success Criteria |
|-------------|------|----------|------------------|
| Household Admin(s) | System owner, primary configurer | Setup complexity, data privacy, cost | Easy setup, full control, reliable operation |
| Family Members | Daily users (kids to adults) | Usability, fun factor, autonomy | Intuitive interface, personal accountability |
| External Caregivers | Shared-access users | Limited scope, clear information | View-only access, relevant information only |
| Developers | Contributors | Code quality, documentation | Clean architecture, comprehensive docs |
| Self-Hosters | Advanced users | Control, privacy, customization | Easy deployment, full data ownership |

### Persona Definitions

#### Persona 1: The Household Admin (Primary)

**Name:** Alex (Parent, 35-45)
**Role:** Household Admin
**Technical Skill:** Moderate to High

**Goals:**
- Reduce time spent coordinating family logistics
- Ensure all family members are informed and accountable
- Maintain privacy and control over family data

**Pain Points:**
- Tired of being the "family scheduler"
- Frustrated with subscription costs for family apps
- Wants something the whole family will actually use

**Needs:**
- Quick setup with sensible defaults
- Mobile app for on-the-go management
- Integration with existing calendars
- Caregiver sharing without complexity

---

#### Persona 2: The Family Member (Child)

**Name:** Jordan (Child, 6-12)
**Role:** Family Member
**Technical Skill:** Low to Moderate

**Goals:**
- Know what's happening today
- Complete chores and earn rewards
- Feel included in family activities

**Pain Points:**
- Forgets tasks and appointments
- Can't read complex schedules
- Gets distracted by other screens

**Needs:**
- Big, colorful, simple interface
- Visual routines with icons
- Fun reward system
- Profile with their favorite color

---

#### Persona 3: The Family Member (Teen/Adult)

**Name:** Riley (Teen/Adult, 13+)
**Role:** Family Member
**Technical Skill:** Moderate to High

**Goals:**
- Quick access to personal schedule
- Independence in managing own tasks
- Privacy for personal events

**Pain Points:**
- Doesn't want to be micromanaged
- Needs mobile access, not just wall display
- Wants personal vs. family separation

**Needs:**
- Personal calendar view
- Private task lists
- Notification preferences
- Mobile app access

---

#### Persona 4: The External Caregiver

**Name:** Sam (Grandparent/Babysitter)
**Role:** External Caregiver
**Technical Skill:** Variable

**Goals:**
- Know what needs to happen during care time
- Access emergency information
- Feel confident about the plan

**Pain Points:**
- Often left out of the loop
- Can't remember complex instructions
- Worried about handling emergencies

**Needs:**
- Read-only access to relevant schedules
- Caregiver info (allergies, contacts, routines)
- Simple, uncluttered view
- No app installation required (web access)

---

## Core UX Principles

### Principle 1: Glanceable First

The primary interface must answer "What's next?" within 2 seconds of glancing at the display.

**Implementation:**
- Today's view is always the default
- Next 24 hours prominently displayed
- Color-coded by person
- Large, readable text (viewable from 10+ feet)

### Principle 2: Color + Profile-First

Every item is instantly attributable to a family member through consistent visual coding.

**Implementation:**
- Each profile has a unique color
- Avatar/photo displayed alongside items
- Color-blind friendly palette options
- Consistent color usage across all views

### Principle 3: Two Input Paths

Support both structured and unstructured data entry.

**Path A: Structured Editing**
- Traditional form-based input
- Available on device and mobile app
- Full control over all fields
- Templates for common items

**Path B: Magic Import Assistant**
- Forward emails to create events
- Photo/scan handwritten lists
- Parse PDFs (school schedules, activities)
- AI-assisted extraction with review step
- Nothing published without approval

### Principle 4: Safe by Default

Protect against accidental changes and ensure age-appropriate access.

**Implementation:**
- Kid mode limits destructive actions
- PIN/parental lock for sensitive settings
- Confirmation dialogs for deletions
- Undo available for recent actions
- Audit log for admin review

### Principle 5: Always Recovers

The system must be resilient to failures and require zero manual intervention.

**Implementation:**
- Auto-start kiosk mode on boot
- Watchdog process for UI crashes
- Offline mode with cached "today" view
- Automatic reconnection on network restore
- Graceful degradation of features

---

## Functional Scope

### 1. Calendar and Scheduling

#### 1.1 Calendar Aggregation
- **CAL-001**: Connect and display multiple external calendars (Google, Outlook/Office 365, iCloud, CalDAV)
- **CAL-002**: Support URL-based subscribed calendars (ICS feeds from schools, sports teams, etc.)
- **CAL-003**: Calendar-to-profile mapping (reassign which calendars belong to which family members)
- **CAL-004**: Two-way sync with supported providers (initial: Google Calendar)

#### 1.2 Calendar Views
- **CAL-010**: Day view (current day, including "next 24 hours" mode)
- **CAL-011**: Week view (7-day overview)
- **CAL-012**: Month view (traditional calendar grid)
- **CAL-013**: Agenda view (chronological list)
- **CAL-014**: Family filter (show all or filter by profile)

#### 1.3 Event Management
- **CAL-020**: Create/edit/delete events on device and mobile
- **CAL-021**: Recurring event support (daily, weekly, monthly, yearly, custom)
- **CAL-022**: Event reminders and notifications
- **CAL-023**: Location support with maps link
- **CAL-024**: Attendee management (family members)
- **CAL-025**: Countdown tiles for major events (birthdays, trips, holidays)

#### 1.4 Calendar Sharing
- **CAL-030**: Invite external caregivers with controlled visibility
- **CAL-031**: Permission levels (view-only, can add, full edit)
- **CAL-032**: Share link generation for quick access

---

### 2. Magic Import Assistant

#### 2.1 Input Channels
- **IMP-001**: Email forwarding (dedicated family inbox)
- **IMP-002**: Photo/screenshot upload (OCR processing)
- **IMP-003**: Text paste (natural language parsing)
- **IMP-004**: PDF upload (school schedules, activity calendars)
- **IMP-005**: URL extraction (event pages, schedules)

#### 2.2 Output Types
- **IMP-010**: Calendar events (single and recurring)
- **IMP-011**: Meal plans (weekly with recipes)
- **IMP-012**: List items (grocery, to-do, custom)
- **IMP-013**: Chore assignments

#### 2.3 Approval Workflow
- **IMP-020**: All imports require explicit approval before publishing
- **IMP-021**: Preview with ability to edit before approval
- **IMP-022**: Batch approval for multiple items
- **IMP-023**: Rejection with feedback for learning
- **IMP-024**: Audit trail of all imports

---

### 3. Tasks, Chores, and Routines

#### 3.1 Chores and To-Dos
- **CHR-001**: Create chores with title, description, and icon
- **CHR-002**: Assign to one or multiple profiles
- **CHR-003**: "Anyone" bucket for shared household tasks
- **CHR-004**: Recurrence support (daily, weekly, custom)
- **CHR-005**: Due date and time settings
- **CHR-006**: Completion tracking with timestamps
- **CHR-007**: Priority levels (optional)
- **CHR-008**: "Today's Focus" filter for kid-friendly view

#### 3.2 Routines
- **RTN-001**: Step-by-step routines with kid-friendly icons
- **RTN-002**: Multiple display modes (list, cards, slideshow)
- **RTN-003**: Timer/duration per step (optional)
- **RTN-004**: Audio cues for step completion (optional)
- **RTN-005**: Streak tracking for motivation
- **RTN-006**: Starter routine library (morning, bedtime, homework, etc.)
- **RTN-007**: Custom routine builder
- **RTN-008**: Profile-specific routines

---

### 4. Rewards and Motivation

#### 4.1 Points System
- **RWD-001**: Stars/points assigned to chores and routines
- **RWD-002**: Per-profile point tracking
- **RWD-003**: Point history and statistics

#### 4.2 Rewards
- **RWD-010**: Custom reward definitions (screen time, allowance, privileges)
- **RWD-011**: Point cost per reward
- **RWD-012**: Reward request and approval workflow
- **RWD-013**: Reward history

#### 4.3 Gamification
- **RWD-020**: Progress-to-goal visualization
- **RWD-021**: Celebratory animations on completion
- **RWD-022**: Streaks and achievements
- **RWD-023**: Non-reader-friendly design (emoji, icons)

---

### 5. Meal Planning and Recipes

#### 5.1 Meal Plans
- **MEL-001**: Weekly meal plan view (by day)
- **MEL-002**: Meal slots (breakfast, lunch, dinner, snacks)
- **MEL-003**: Assign meals per family member (dietary needs)
- **MEL-004**: Meal templates for quick planning
- **MEL-005**: Copy/rotate previous weeks

#### 5.2 Recipes
- **MEL-010**: Recipe storage with ingredients and instructions
- **MEL-011**: Recipe import from URLs
- **MEL-012**: Recipe photos
- **MEL-013**: Scaling for servings
- **MEL-014**: Nutritional information (optional)

#### 5.3 Integration
- **MEL-020**: "Add ingredients to grocery list" one-click action
- **MEL-021**: AI-generated meal plans with preferences (allergies, cuisine, budget)
- **MEL-022**: Leftover/ingredient-based suggestions

---

### 6. Lists

#### 6.1 List Management
- **LST-001**: Unlimited custom lists
- **LST-002**: List templates (grocery, packing, school supplies)
- **LST-003**: Private lists in mobile app (e.g., gift lists)
- **LST-004**: Shared vs. personal list designation
- **LST-005**: Archive completed lists

#### 6.2 List Views
- **LST-010**: Checklist view (simple checkbox list)
- **LST-011**: Board/kanban view (columns for categorization)
- **LST-012**: Category grouping (produce, dairy, etc.)
- **LST-013**: Sort options (alphabetical, checked status, custom)

#### 6.3 List Features
- **LST-020**: Quantity and units per item
- **LST-021**: Notes per item
- **LST-022**: Assign items to shoppers
- **LST-023**: Recently added / frequently used suggestions
- **LST-024**: Instacart integration (US only, configurable)

---

### 7. Profiles and Household Directory

#### 7.1 Profile Management
- **PRF-001**: Unlimited profiles (family members and pets)
- **PRF-002**: Photo/avatar support
- **PRF-003**: Nickname and display name
- **PRF-004**: Profile color selection
- **PRF-005**: Birthday and age tracking

#### 7.2 Caregiver Information
- **PRF-010**: Allergies and medical notes
- **PRF-011**: Emergency contact numbers
- **PRF-012**: Doctor/dentist information
- **PRF-013**: School/activity information
- **PRF-014**: Important notes for caregivers

#### 7.3 Permissions
- **PRF-020**: Role assignment (Admin, Member, Child, Caregiver)
- **PRF-021**: Per-role permission matrix
- **PRF-022**: PIN codes per profile (optional)
- **PRF-023**: Caregiver time-limited access

---

### 8. Companion Experience

#### 8.1 Mobile App
- **MOB-001**: Full control of calendars, chores, lists, meals, profiles
- **MOB-002**: Push notifications for events and reminders
- **MOB-003**: Chore completion notifications
- **MOB-004**: Offline capability with sync
- **MOB-005**: iOS and Android support

#### 8.2 Mobile Widgets
- **MOB-010**: Today's schedule widget
- **MOB-011**: Grocery list widget
- **MOB-012**: Chore status widget

#### 8.3 Web Access
- **MOB-020**: Responsive web interface
- **MOB-021**: Caregiver web portal (no app required)
- **MOB-022**: Full admin capabilities via web

---

### 9. Ambient Information

#### 9.1 Weather
- **AMB-001**: Current weather display
- **AMB-002**: Multi-day forecast
- **AMB-003**: Location/address-based
- **AMB-004**: Unit preferences (Fahrenheit/Celsius)
- **AMB-005**: Weather alerts

#### 9.2 Additional Widgets (Future)
- **AMB-010**: Clock with timezone support
- **AMB-011**: Photo frame mode (slideshow of family photos)
- **AMB-012**: Quote of the day
- **AMB-013**: Household announcements/notes

---

### 10. Display Modes and Privacy

#### 10.1 Privacy Mode
- **DSP-001**: One-tap privacy mode (swap to wallpaper/clock)
- **DSP-002**: Automatic privacy mode after timeout
- **DSP-003**: Motion sensor integration (optional, wake on approach)

#### 10.2 Sleep Mode
- **DSP-010**: Scheduled screen-off times
- **DSP-011**: Manual sleep toggle
- **DSP-012**: Wake on touch or schedule

#### 10.3 Display Settings
- **DSP-020**: Auto-brightness with ambient light sensor
- **DSP-021**: Manual brightness override
- **DSP-022**: Night mode color temperature
- **DSP-023**: Screen saver options

---

## Non-Functional Requirements

### Reliability

| Requirement | Description | Target |
|-------------|-------------|--------|
| NFR-REL-001 | Auto-start kiosk shell on boot | 100% |
| NFR-REL-002 | Watchdog recovery from UI crashes | < 30 seconds |
| NFR-REL-003 | Offline resilience (cached today view) | Yes |
| NFR-REL-004 | Uptime target | 99.9% |
| NFR-REL-005 | Data sync recovery on reconnection | Automatic |

### Performance

| Requirement | Description | Target |
|-------------|-------------|--------|
| NFR-PRF-001 | Initial load time | < 5 seconds |
| NFR-PRF-002 | View transition time | < 300ms |
| NFR-PRF-003 | Touch response latency | < 100ms |
| NFR-PRF-004 | Memory usage (display app) | < 512MB |
| NFR-PRF-005 | 24/7 operation without memory leaks | Yes |

### Security

| Requirement | Description | Target |
|-------------|-------------|--------|
| NFR-SEC-001 | Local device lock with PIN | Required |
| NFR-SEC-002 | Role-based access control | Required |
| NFR-SEC-003 | Edit audit logging | Required |
| NFR-SEC-004 | Encrypted data at rest | Required |
| NFR-SEC-005 | Encrypted data in transit (HTTPS) | Required |
| NFR-SEC-006 | OAuth 2.0 for external integrations | Required |

### Accessibility

| Requirement | Description | Target |
|-------------|-------------|--------|
| NFR-A11Y-001 | WCAG 2.1 AA compliance | Required |
| NFR-A11Y-002 | Large text mode | Required |
| NFR-A11Y-003 | High contrast mode | Required |
| NFR-A11Y-004 | Touch target size | 44x44px minimum |
| NFR-A11Y-005 | Dyslexia-friendly font option | Required |
| NFR-A11Y-006 | Screen reader support (mobile) | Required |
| NFR-A11Y-007 | Text readability from 10+ feet | Required |

### Updates and Maintenance

| Requirement | Description | Target |
|-------------|-------------|--------|
| NFR-UPD-001 | OTA update capability | Required |
| NFR-UPD-002 | Automatic background updates | Optional (configurable) |
| NFR-UPD-003 | Rollback capability | Required |
| NFR-UPD-004 | Update notification | Required |

---

## Design Constraints

### Technical Constraints

1. **Platform Agnostic**: Must run on commodity hardware (tablets, mini PCs, Raspberry Pi)
2. **Self-Hostable**: Full functionality without cloud dependency
3. **Offline-First**: Core features must work without internet
4. **Low Power**: Optimized for always-on, low-power devices
5. **Portrait Display**: Primary design for vertical/portrait orientation

### Business Constraints

1. **Open Source**: AGPL-3.0 license for all core functionality
2. **No Vendor Lock-in**: No proprietary hardware requirements
3. **Privacy-First**: No mandatory data collection or telemetry
4. **Community Driven**: Transparent roadmap and contribution process

### Design Constraints

1. **Zero Distraction**: No web browsing, apps, video, or entertainment features
2. **Kid-Friendly**: All interfaces usable by children 6+
3. **Caregiver Simple**: External users need no training
4. **Always Visible**: No screensavers or idle states that hide information

---

## Key Design Decisions

Detailed Architecture Decision Records (ADRs) are maintained in `/docs/adr/`. Key decisions include:

| ADR | Title | Status |
|-----|-------|--------|
| ADR-001 | Use TypeScript for all application code | Accepted |
| ADR-002 | React for display and mobile applications | Accepted |
| ADR-003 | Local-first data architecture with sync | Accepted |
| ADR-004 | Two-way sync initially limited to Google Calendar | Accepted |
| ADR-005 | Magic Import requires explicit approval | Accepted |
| ADR-006 | Zero-distraction principle (no entertainment) | Accepted |
| ADR-007 | Self-hosting as primary deployment model | Accepted |

---

## Success Metrics

### User Adoption

| Metric | Target | Measurement |
|--------|--------|-------------|
| Daily active display usage | 100% of days | Display uptime |
| Family member engagement | 80% of profiles active weekly | Interaction logs |
| Chore completion rate | 70% of assigned chores | Completion records |

### Technical Health

| Metric | Target | Measurement |
|--------|--------|-------------|
| Display uptime | 99.9% | System monitoring |
| Sync latency | < 60 seconds | Sync timestamps |
| Crash-free sessions | 99.5% | Error tracking |

### Community Growth

| Metric | Target | Measurement |
|--------|--------|-------------|
| GitHub stars | 1,000 in year 1 | GitHub API |
| Active contributors | 20 in year 1 | Git history |
| Community deployments | 500 in year 1 | Opt-in telemetry |

---

## Glossary

| Term | Definition |
|------|------------|
| **Chore** | A single task or to-do item assigned to family members |
| **Display** | The wall-mounted device running the Luminous display app |
| **External Caregiver** | Non-household members with shared access (grandparents, babysitters) |
| **Household Admin** | Primary account owner with full configuration access |
| **Magic Import** | AI-powered feature to parse unstructured inputs into structured data |
| **Profile** | A family member or pet represented in the system |
| **Routine** | A sequence of steps that form a repeated activity (morning routine, etc.) |
| **Kiosk Mode** | Locked-down display mode that prevents exit to other apps |
| **Two-Way Sync** | Calendar updates flow both to and from external providers |

---

## Document History

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0.0 | 2025-12-21 | Luminous Team | Initial draft |

---

## Related Documents

- [Architecture Overview](./ARCHITECTURE.md)
- [Development Roadmap](./ROADMAP.md)
- [Architecture Decision Records](./adr/)
- [CLAUDE.md (Development Guidelines)](../CLAUDE.md)
