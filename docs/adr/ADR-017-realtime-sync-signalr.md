# ADR-017: Real-time Sync with SignalR

> **Status:** Accepted
> **Date:** 2026-01-09
> **Deciders:** Architecture Team
> **Categories:** Architecture | Technology

## Context

Luminous is a family command center with multiple clients (web, display, mobile) that need to stay synchronized with each other. When a family member adds an event on their phone, the wall-mounted display should update immediately without requiring a manual refresh.

The current architecture relies on periodic polling (1-minute intervals on the display), which introduces:
- Latency between data changes and UI updates
- Unnecessary API calls when no data has changed
- Poor user experience for collaborative features
- Increased server load from polling

We need a real-time synchronization mechanism that:
- Pushes updates immediately when data changes
- Scales across multiple connected devices per family
- Works with our existing Azure infrastructure
- Supports offline/reconnection scenarios
- Maintains family data isolation (multi-tenancy)

## Decision Drivers

- **Low latency updates**: Display screens should reflect changes within seconds
- **Scalability**: Must support many concurrent family connections
- **Azure-native**: Leverage existing Azure investment
- **Multi-tenancy**: Family data must remain isolated
- **Reliability**: Handle network interruptions gracefully
- **Cost efficiency**: Minimize Azure resource usage

## Considered Options

### Option 1: Azure SignalR Service

Azure-managed SignalR service with ASP.NET Core SignalR hub.

**Pros:**
- Fully managed, scales automatically
- Native integration with ASP.NET Core
- Built-in connection management and groups
- Supports WebSocket, Server-Sent Events, Long Polling
- Free tier available for development

**Cons:**
- Additional Azure resource cost in production
- Slightly more complex than pure WebSocket

### Option 2: Azure Web PubSub

Azure's newer WebSocket-based real-time messaging service.

**Pros:**
- Lower latency than SignalR
- More lightweight protocol

**Cons:**
- Less integration with ASP.NET Core
- Requires more custom code
- Less mature SDK support

### Option 3: Firebase Realtime Database

Google's real-time sync solution.

**Pros:**
- Simple client SDK
- Built-in offline support

**Cons:**
- Not Azure-native (vendor split)
- Additional cost and complexity
- Data model doesn't fit CosmosDB patterns

## Decision

We will use **Azure SignalR Service** with family-scoped SignalR groups for real-time synchronization.

### Architecture

```
┌─────────────────────────────────────────────────────────────────────────┐
│                         SIGNALR ARCHITECTURE                            │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│  ┌─────────┐    ┌─────────┐    ┌─────────┐    ┌─────────┐              │
│  │  Web    │    │ Display │    │   iOS   │    │ Android │              │
│  │  App    │    │   App   │    │   App   │    │   App   │              │
│  └────┬────┘    └────┬────┘    └────┬────┘    └────┬────┘              │
│       │              │              │              │                    │
│       └──────────────┴──────────────┴──────────────┘                    │
│                             │                                           │
│                             ▼                                           │
│                   ┌───────────────────┐                                │
│                   │  Azure SignalR    │                                │
│                   │     Service       │                                │
│                   └────────┬──────────┘                                │
│                            │                                           │
│                            ▼                                           │
│                   ┌───────────────────┐                                │
│                   │    SyncHub        │                                │
│                   │  (ASP.NET Core)   │                                │
│                   └────────┬──────────┘                                │
│                            │                                           │
│        ┌───────────────────┼───────────────────┐                       │
│        │                   │                   │                       │
│        ▼                   ▼                   ▼                       │
│  ┌───────────┐      ┌───────────┐      ┌───────────┐                  │
│  │ family:A  │      │ family:B  │      │ family:C  │   SignalR        │
│  │  (group)  │      │  (group)  │      │  (group)  │   Groups         │
│  └───────────┘      └───────────┘      └───────────┘                  │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

### Message Flow

1. Client makes API request (e.g., create event)
2. API processes request and updates database
3. Command handler calls `ISyncNotificationService`
4. `SyncNotificationService` broadcasts to SignalR group
5. All connected family clients receive the update
6. Clients update their local state immediately

### Group Strategy

Each family has a dedicated SignalR group named `family:{familyId}`. When a client connects:

1. JWT token is validated (contains `family_id` claim)
2. Client is automatically added to their family's group
3. All broadcasts are scoped to the family group
4. When client disconnects, they're removed from the group

### Message Types

```
EventCreated, EventUpdated, EventDeleted, EventsRefreshed
ChoreCreated, ChoreUpdated, ChoreDeleted, ChoreCompleted
UserUpdated, UserJoined, UserLeft
FamilyUpdated, FamilySettingsUpdated
DeviceLinked, DeviceUnlinked
CalendarSyncCompleted
FullSyncRequired
```

## Rationale

Azure SignalR Service was chosen because:

1. **Native ASP.NET Core integration**: Works seamlessly with our existing .NET backend
2. **Managed scaling**: No infrastructure to manage; scales with connections
3. **Groups feature**: Perfect for family-scoped multi-tenancy
4. **Reconnection handling**: Built-in automatic reconnect with exponential backoff
5. **Azure alignment**: Fits our Azure-first strategy (TP-1)
6. **Cost effective**: Free tier for dev/test, predictable pricing in production

## Consequences

### Positive

- Real-time updates across all family devices
- Reduced API polling and server load
- Better user experience for collaborative features
- Built-in offline recovery handling
- Simpler client code (reactive streams)

### Negative

- Additional Azure resource to manage
- Slightly increased infrastructure cost
- More complex debugging (distributed events)
- Need to handle duplicate messages in edge cases

### Neutral

- Requires clients to implement SignalR connection management
- Message ordering not guaranteed across groups
- Need to decide when to use push vs. poll

## Implementation Notes

### Backend

1. **SyncHub** (`/hubs/sync`): SignalR hub with JWT authentication
2. **ISyncNotificationService**: Interface for broadcasting to family groups
3. **SyncNotificationService**: Implementation using `IHubContext<SyncHub>`
4. **JWT Events**: `OnMessageReceived` extracts token from query string for WebSocket

### Frontend (Angular)

1. **SyncService**: Manages SignalR connection lifecycle
2. **Exponential backoff**: Reconnect with 2^n delay (max 30s)
3. **Observable streams**: RxJS subjects for each message type
4. **Cache invalidation**: Automatically invalidates cache on sync messages

### Configuration

- Development: Self-hosted SignalR (no Azure dependency)
- Production: Azure SignalR Service Standard tier

### Security

- All connections require valid JWT token
- Family ID extracted from `family_id` claim
- Clients can only receive messages for their own family
- No client-to-client messaging (server-only broadcast)

## Related Decisions

- [ADR-001: .NET Backend](ADR-001-dotnet-backend.md) - Backend framework
- [ADR-003: Azure Cloud Platform](ADR-003-azure-cloud-platform.md) - Azure-first strategy
- [ADR-006: Multi-Tenant Architecture](ADR-006-multi-tenant-architecture.md) - Family isolation

## References

- [Azure SignalR Service Documentation](https://learn.microsoft.com/en-us/azure/azure-signalr/)
- [ASP.NET Core SignalR](https://learn.microsoft.com/en-us/aspnet/core/signalr/introduction)
- [SignalR Groups](https://learn.microsoft.com/en-us/aspnet/core/signalr/groups)
