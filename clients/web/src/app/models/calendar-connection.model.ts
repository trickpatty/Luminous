/**
 * Calendar provider type
 */
export enum CalendarProvider {
  Google = 'Google',
  Microsoft = 'Microsoft',
  Apple = 'Apple',
  IcsUrl = 'IcsUrl',
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
 * Calendar sync settings
 */
export interface CalendarSyncSettings {
  syncEnabled: boolean;
  syncIntervalMinutes: number;
  syncPastDays: number;
  syncFutureDays: number;
  includeAllDayEvents: boolean;
  includePrivateEvents: boolean;
}

/**
 * Calendar connection entity
 */
export interface CalendarConnection {
  id: string;
  familyId: string;
  provider: CalendarProvider;
  status: CalendarConnectionStatus;
  displayName: string;
  accountEmail?: string;
  externalCalendarId?: string;
  externalCalendarName?: string;
  calendarColor?: string;
  icsUrl?: string;
  assignedMemberIds: string[];
  syncSettings: CalendarSyncSettings;
  lastSyncAt?: string;
  lastSyncEventCount?: number;
  lastSyncError?: string;
  syncToken?: string;
  createdAt: string;
  createdBy: string;
  updatedAt?: string;
}

/**
 * External calendar info (from provider)
 */
export interface ExternalCalendarInfo {
  id: string;
  name: string;
  color?: string;
  isPrimary: boolean;
  accessRole: string;
}

/**
 * OAuth start response
 */
export interface OAuthStartResponse {
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
 * OAuth complete response with available calendars
 */
export interface OAuthCompleteResponse {
  accountEmail: string;
  calendars: ExternalCalendarInfo[];
}

/**
 * Create calendar connection request
 */
export interface CreateCalendarConnectionRequest {
  provider: CalendarProvider;
  displayName: string;
  externalCalendarId?: string;
  externalCalendarName?: string;
  calendarColor?: string;
  icsUrl?: string;
  assignedMemberIds: string[];
  syncSettings?: Partial<CalendarSyncSettings>;
}

/**
 * Update calendar connection request
 */
export interface UpdateCalendarConnectionRequest {
  displayName?: string;
  assignedMemberIds?: string[];
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
 * Validate ICS URL request
 */
export interface ValidateIcsUrlRequest {
  url: string;
}

/**
 * Validate ICS URL response
 */
export interface ValidateIcsUrlResponse {
  valid: boolean;
  calendarName?: string;
  eventCount?: number;
  error?: string;
}
