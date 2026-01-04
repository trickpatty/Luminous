/**
 * Calendar provider type (must match backend CalendarProvider enum)
 */
export enum CalendarProvider {
  Google = 'Google',
  Outlook = 'Outlook',
  ICloud = 'ICloud',
  CalDav = 'CalDav',
  IcsUrl = 'IcsUrl',
  Internal = 'Internal',
}

/**
 * Calendar connection status
 */
export enum CalendarConnectionStatus {
  PendingAuth = 'PendingAuth',
  Active = 'Active',
  Paused = 'Paused',
  AuthError = 'AuthError',
  SyncError = 'SyncError',
  Disconnected = 'Disconnected',
}

/**
 * Calendar sync settings (matches CalendarSyncSettingsDto)
 */
export interface CalendarSyncSettings {
  syncIntervalMinutes: number;
  syncPastDays: number;
  syncFutureDays: number;
  importAllDayEvents: boolean;
  importDeclinedEvents: boolean;
  twoWaySync: boolean;
}

/**
 * Calendar connection entity (matches CalendarConnectionDto from backend)
 */
export interface CalendarConnection {
  id: string;
  familyId: string;
  name: string;
  provider: CalendarProvider;
  status: CalendarConnectionStatus;
  externalAccountId?: string;
  assignedMemberIds: string[];
  color?: string;
  isEnabled: boolean;
  isReadOnly: boolean;
  lastSyncedAt?: string;
  nextSyncAt?: string;
  lastSyncError?: string;
  consecutiveFailures: number;
  syncSettings: CalendarSyncSettings;
  createdAt: string;
}

/**
 * Calendar connection summary (lighter version)
 */
export interface CalendarConnectionSummary {
  id: string;
  name: string;
  provider: CalendarProvider;
  status: CalendarConnectionStatus;
  isEnabled: boolean;
  color?: string;
  lastSyncedAt?: string;
}

/**
 * External calendar info (from provider, matches ExternalCalendarInfo from backend)
 */
export interface ExternalCalendarInfo {
  id: string;
  name: string;
  description?: string;
  color?: string;
  isReadOnly: boolean;
  isPrimary: boolean;
  timeZone?: string;
}

/**
 * OAuth initiate response (matches OAuthInitiateResponse from backend)
 */
export interface OAuthStartResponse {
  sessionId: string;
  authorizationUrl: string;
  state: string;
}

/**
 * OAuth complete request
 */
export interface OAuthCompleteRequest {
  code: string;
  state: string;
  redirectUri: string;
}

/**
 * OAuth complete response (matches OAuthCompleteResponse from backend)
 */
export interface OAuthCompleteResponse {
  sessionId: string;
  accountEmail: string;
  calendars: ExternalCalendarInfo[];
}

/**
 * Request to create connections from an OAuth session
 */
export interface CreateConnectionsFromSessionRequest {
  sessionId: string;
  calendars: CreateConnectionFromSessionCalendar[];
}

/**
 * Calendar to create from a session
 */
export interface CreateConnectionFromSessionCalendar {
  externalCalendarId: string;
  displayName: string;
  color?: string;
  assignedMemberIds: string[];
}

/**
 * Create calendar connection request (for ICS only)
 */
export interface CreateCalendarConnectionRequest {
  name: string;
  provider: CalendarProvider;
  icsUrl?: string;
  assignedMemberIds: string[];
  color?: string;
}

/**
 * Update calendar connection request
 */
export interface UpdateCalendarConnectionRequest {
  name?: string;
  assignedMemberIds?: string[];
  color?: string;
  isEnabled?: boolean;
  syncSettings?: Partial<CalendarSyncSettings>;
}

/**
 * Sync calendar connection response
 */
export interface SyncCalendarResponse {
  connectionId: string;
  eventsAdded: number;
  eventsUpdated: number;
  eventsRemoved: number;
  syncedAt: string;
}

/**
 * Validate ICS URL response (matches ValidateIcsUrlResponse from backend)
 */
export interface ValidateIcsUrlResponse {
  isValid: boolean;
  calendarName?: string;
  eventCount?: number;
  error?: string;
}
