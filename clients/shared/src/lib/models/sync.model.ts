/**
 * Types of sync messages received via SignalR.
 */
export enum SyncMessageType {
  // Event-related messages
  EventCreated = 'EventCreated',
  EventUpdated = 'EventUpdated',
  EventDeleted = 'EventDeleted',
  EventsRefreshed = 'EventsRefreshed',

  // Chore-related messages
  ChoreCreated = 'ChoreCreated',
  ChoreUpdated = 'ChoreUpdated',
  ChoreDeleted = 'ChoreDeleted',
  ChoreCompleted = 'ChoreCompleted',

  // User-related messages
  UserUpdated = 'UserUpdated',
  UserJoined = 'UserJoined',
  UserLeft = 'UserLeft',

  // Family-related messages
  FamilyUpdated = 'FamilyUpdated',
  FamilySettingsUpdated = 'FamilySettingsUpdated',

  // Device-related messages
  DeviceLinked = 'DeviceLinked',
  DeviceUnlinked = 'DeviceUnlinked',
  DeviceUpdated = 'DeviceUpdated',

  // Calendar-related messages
  CalendarConnectionAdded = 'CalendarConnectionAdded',
  CalendarConnectionRemoved = 'CalendarConnectionRemoved',
  CalendarSyncCompleted = 'CalendarSyncCompleted',

  // General sync messages
  FullSyncRequired = 'FullSyncRequired',
  HeartbeatResponse = 'HeartbeatResponse',
}

/**
 * Base sync message from SignalR.
 */
export interface SyncMessage {
  type: SyncMessageType;
  familyId: string;
  entityId?: string;
  triggeredBy?: string;
  timestamp: string;
  payload?: unknown;
}

/**
 * Event-specific sync message.
 */
export interface EventSyncMessage {
  type: SyncMessageType;
  eventId: string;
  event?: EventSummary;
  timestamp: string;
}

/**
 * Chore-specific sync message.
 */
export interface ChoreSyncMessage {
  type: SyncMessageType;
  choreId: string;
  chore?: ChoreData;
  completedBy?: string;
  timestamp: string;
}

/**
 * Calendar sync completed message.
 */
export interface CalendarSyncCompletedMessage {
  calendarConnectionId: string;
  provider: string;
  eventsAdded: number;
  eventsUpdated: number;
  eventsRemoved: number;
  timestamp: string;
}

/**
 * User sync message.
 */
export interface UserSyncMessage {
  type: SyncMessageType;
  userId: string;
  displayName: string;
  timestamp: string;
}

/**
 * Connection state for SignalR.
 */
export enum ConnectionState {
  Disconnected = 'Disconnected',
  Connecting = 'Connecting',
  Connected = 'Connected',
  Reconnecting = 'Reconnecting',
}

/**
 * Simplified event summary for sync messages.
 */
export interface EventSummary {
  id: string;
  title: string;
  startTime?: string;
  endTime?: string;
  startDate?: string;
  endDate?: string;
  isAllDay: boolean;
  assigneeIds: string[];
  color?: string;
  locationText?: string;
}

/**
 * Chore data for sync messages.
 */
export interface ChoreData {
  id: string;
  familyId: string;
  title: string;
  description?: string;
  assignees: string[];
  dueDate?: string;
  isCompleted: boolean;
  completedAt?: string;
  completedBy?: string;
}
