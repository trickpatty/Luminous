# ADR-003: Local-First Data Architecture

> **Status:** Accepted
> **Date:** 2025-12-21
> **Deciders:** Luminous Core Team
> **Categories:** Architecture

## Context

Luminous is designed for always-on operation on wall-mounted displays, which presents unique requirements:

1. **Reliability**: The display must show current information even during internet outages
2. **Privacy**: Families want control over their personal data
3. **Performance**: UI interactions must be immediate, not dependent on network latency
4. **Self-hosting**: Users want the option to run everything locally

We need to decide on a data architecture that addresses these requirements.

## Decision Drivers

- **Offline functionality**: Display must work without internet connection
- **Data ownership**: Users must be able to self-host and own their data
- **Real-time sync**: Changes should propagate quickly across devices
- **Conflict resolution**: Multiple devices may edit the same data
- **Performance**: No waiting for network on user interactions
- **Simplicity**: Architecture should be understandable and maintainable

## Considered Options

### Option 1: Local-First with CRDT Sync

Data stored locally on each device, synchronized using Conflict-free Replicated Data Types.

**Pros:**
- Works offline by default
- Immediate UI response
- Automatic conflict resolution
- Data stays local (privacy)
- Server is optional for multi-device sync

**Cons:**
- CRDT complexity for some data types
- Storage duplication across devices
- Initial sync can be slow for large datasets
- Debugging sync issues is complex

### Option 2: Cloud-First with Offline Cache

Primary data in cloud, with local caching for offline use.

**Pros:**
- Single source of truth
- Simpler conflict resolution
- Smaller local storage needs

**Cons:**
- Requires internet for full functionality
- Privacy concerns with cloud storage
- Latency on all write operations
- Vendor lock-in with cloud provider
- Ongoing hosting costs

### Option 3: Hybrid (Local Primary, Cloud Backup)

Local storage as primary, cloud as optional backup and sync.

**Pros:**
- Best of both approaches
- Optional cloud dependency
- Privacy-preserving by default

**Cons:**
- Two systems to maintain
- Complex sync logic
- Potential for data divergence

## Decision

We will implement a **Local-First architecture** with optional CRDT-based synchronization.

Core principles:
1. All data is stored locally on each device first
2. UI operations read/write to local storage only
3. Synchronization happens in the background
4. The server is optional and only coordinates sync between devices
5. Conflicts are resolved automatically using CRDTs where possible

## Rationale

Local-first architecture best meets our requirements:

1. **Reliability**: The display always has local data available. Even if the internet is down for days, the calendar shows cached events, chores are visible, and tasks can be completed.

2. **Privacy**: Data never leaves the home network unless the user explicitly configures sync. Self-hosters can run everything locally.

3. **Performance**: All UI interactions are instant because they only touch local storage. Sync happens asynchronously.

4. **User experience**: No loading spinners for basic operations. The app feels native and responsive.

5. **Self-hosting friendly**: The optional sync server can run on a Raspberry Pi or NAS, keeping all data on the local network.

## Consequences

### Positive

- Display works perfectly offline
- Immediate UI feedback on all actions
- No mandatory cloud dependency
- Strong privacy story
- Works well with self-hosting
- Reduced server infrastructure needs

### Negative

- Must implement sync logic (complexity)
- CRDT learning curve for team
- Storage requirements higher (data on all devices)
- Some calendar integrations require internet (unavoidable)
- Conflict resolution edge cases to handle

### Neutral

- Need to choose and implement CRDT library
- Must design data models with CRDTs in mind
- Initial sync may take time for new devices
- Testing requires simulating various sync scenarios

## Implementation Notes

### Storage Stack

- **Display/Desktop**: SQLite via sql.js (WebAssembly)
- **Mobile**: SQLite via expo-sqlite or react-native-sqlite
- **Web**: Origin Private File System (OPFS) or IndexedDB fallback

### Sync Strategy

1. **Event log approach**: Track all changes as events
2. **CRDT types**: Use Yjs or Automerge for collaborative data
3. **Sync protocol**: WebSocket for real-time, REST for batch

### Data Model Considerations

- Design entities with CRDT-friendly IDs (UUIDs)
- Avoid counters (use G-Counters or PN-Counters)
- Prefer append-only where possible (completions, events)
- Use tombstones for deletions (soft delete)

### Conflict Scenarios

| Data Type | Conflict Strategy |
|-----------|------------------|
| Events | Last-write-wins with vector clocks |
| Chores | Merge fields, LWW for each field |
| Completions | Append-only (no conflicts) |
| Lists | CRDT set with tombstones |
| Profiles | Server-authoritative (rare edits) |

## Related Decisions

- [ADR-001: TypeScript as Primary Language](./ADR-001-typescript-primary-language.md)
- [ADR-004: Two-Way Sync Initially Google-Only](./ADR-004-two-way-sync-google-first.md)

## References

- [Local-First Software](https://www.inkandswitch.com/local-first/)
- [Yjs CRDT Library](https://yjs.dev/)
- [Automerge](https://automerge.org/)
- [CRDTs: The Hard Parts](https://www.youtube.com/watch?v=x7drE24geUw)
