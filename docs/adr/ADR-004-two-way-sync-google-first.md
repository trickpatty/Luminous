# ADR-004: Two-Way Sync Initially Google-Only

> **Status:** Accepted
> **Date:** 2025-12-21
> **Deciders:** Luminous Core Team
> **Categories:** Architecture, Technology

## Context

Luminous needs to integrate with external calendar providers to aggregate family schedules. The key question is whether to support read-only access (displaying external calendars) or full two-way synchronization (creating/editing events that sync back to the provider).

Two-way sync is significantly more complex than read-only integration due to:
- Conflict resolution with external system
- Different data models and capabilities per provider
- OAuth token management and refresh
- Rate limiting and API quotas
- Handling provider-specific features (Google Meet, Teams, etc.)

## Decision Drivers

- **User expectations**: Users expect changes made in Luminous to appear in their calendar app
- **Implementation complexity**: Each provider has different APIs and behaviors
- **Reliability**: Sync errors can lose or duplicate events
- **Maintenance burden**: APIs change; each provider adds maintenance load
- **Time to market**: Need to ship functional product reasonably quickly
- **Open source sustainability**: Maintainers must be able to support integrations

## Considered Options

### Option 1: Two-Way Sync with All Providers from Start

Full bidirectional sync with Google, Outlook, iCloud, and CalDAV.

**Pros:**
- Complete feature set at launch
- Matches competitor capabilities

**Cons:**
- Very high implementation effort
- Delays initial release significantly
- Multiple complex APIs to maintain
- iCloud has significant limitations
- CalDAV has many edge cases

### Option 2: Read-Only for All, Two-Way for None

Only display calendars; all event creation is in external apps.

**Pros:**
- Simplest implementation
- No sync conflicts possible
- Works with any ICS feed

**Cons:**
- Users must switch apps to add events
- Breaks "single command center" value proposition
- Chore/task completion can't sync to calendars

### Option 3: Two-Way with Google First, Read-Only Others

Full sync with Google Calendar initially; read-only with others.

**Pros:**
- Delivers two-way sync for largest user base
- Manageable implementation scope
- Clear path to add more providers
- Google API is well-documented and stable

**Cons:**
- Users of other providers have limited functionality
- Feature disparity between providers

### Option 4: Internal Calendar with Export

Luminous-only calendar, exportable to other systems.

**Pros:**
- Full control over data model
- No provider dependencies

**Cons:**
- Doesn't aggregate existing calendars
- Users manage two calendar systems
- Misses core product value

## Decision

We will implement **two-way synchronization with Google Calendar first**, with read-only support for other providers (Outlook/Office 365, iCloud, ICS URLs).

Expansion to two-way sync with other providers will be prioritized based on user demand and contribution.

## Rationale

1. **Market coverage**: Google Calendar has the largest market share for personal/family calendars. Supporting it first covers the most users.

2. **API quality**: Google Calendar API is mature, well-documented, and has clear OAuth flows. It's the most straightforward to implement correctly.

3. **Scope management**: Implementing one two-way sync correctly is better than implementing multiple poorly. We can learn from Google integration before adding others.

4. **User value**: Even with read-only access, users get the core value of seeing all calendars in one place. Two-way sync is additive.

5. **Contribution opportunity**: Read-only providers are easier for contributors to add, while core team focuses on sync complexity.

## Consequences

### Positive

- Manageable scope for initial implementation
- High-quality Google integration
- Clear roadmap for additional providers
- Read-only still provides aggregation value
- Lower maintenance burden initially

### Negative

- Outlook/iCloud users have limited functionality at launch
- Potential perception of "Google-only" product
- Must clearly communicate provider capabilities
- Some users may wait for their provider support

### Neutral

- Need to design adapter pattern for easy provider addition
- Documentation must be clear about sync capabilities
- Provider support becomes a contribution opportunity

## Implementation Notes

### Google Calendar Integration

1. **OAuth 2.0 flow** with offline access (refresh tokens)
2. **Incremental sync** using sync tokens
3. **Rate limit handling** with exponential backoff
4. **Webhook support** for real-time updates (where available)

### Read-Only Providers

| Provider | Protocol | Notes |
|----------|----------|-------|
| Outlook/M365 | Microsoft Graph API | OAuth 2.0, JSON |
| iCloud | CalDAV | App-specific passwords required |
| Generic CalDAV | CalDAV | Basic or OAuth |
| ICS URLs | HTTP(S) | No authentication typically |

### Adapter Interface

```typescript
interface CalendarAdapter {
  // All providers
  listCalendars(): Promise<Calendar[]>;
  fetchEvents(calendarId: string, range: DateRange): Promise<Event[]>;

  // Two-way only (Google initially)
  createEvent?(calendarId: string, event: EventInput): Promise<Event>;
  updateEvent?(calendarId: string, eventId: string, event: EventInput): Promise<Event>;
  deleteEvent?(calendarId: string, eventId: string): Promise<void>;
}
```

### Sync Status UI

Users need clear visibility into:
- Which calendars are read-only vs read-write
- Last sync time per calendar
- Sync errors and how to resolve them

## Migration Path

Adding two-way sync for additional providers:

1. **Phase 1** (launch): Google two-way, others read-only
2. **Phase 2**: Microsoft 365/Outlook two-way
3. **Phase 3**: CalDAV two-way (generic)
4. **Future**: iCloud (if Apple improves API access)

## Related Decisions

- [ADR-003: Local-First Data Architecture](./ADR-003-local-first-architecture.md)

## References

- [Google Calendar API](https://developers.google.com/calendar/api)
- [Microsoft Graph Calendar API](https://learn.microsoft.com/en-us/graph/api/resources/calendar)
- [CalDAV RFC 4791](https://datatracker.ietf.org/doc/html/rfc4791)
