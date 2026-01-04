import { Injectable, inject, signal } from '@angular/core';
import { Observable, tap, catchError } from 'rxjs';
import { ApiService } from './api.service';
import {
  CalendarConnection,
  CalendarProvider,
  CalendarConnectionStatus,
  OAuthStartResponse,
  OAuthCompleteRequest,
  OAuthCompleteResponse,
  CreateCalendarConnectionRequest,
  CreateConnectionsFromSessionRequest,
  UpdateCalendarConnectionRequest,
  SyncCalendarResponse,
  ValidateIcsUrlResponse,
  ExternalCalendarInfo,
} from '../../models';

/**
 * Service for managing calendar connections
 */
@Injectable({
  providedIn: 'root',
})
export class CalendarConnectionService {
  private readonly api = inject(ApiService);

  // State
  private readonly _connections = signal<CalendarConnection[]>([]);
  private readonly _loading = signal(false);
  private readonly _error = signal<string | null>(null);
  private readonly _oauthState = signal<string | null>(null);
  private readonly _pendingSessionId = signal<string | null>(null);
  private readonly _pendingCalendars = signal<ExternalCalendarInfo[]>([]);
  private readonly _pendingAccountEmail = signal<string | null>(null);
  private readonly _syncing = signal<Set<string>>(new Set());

  // Public selectors
  readonly connections = this._connections.asReadonly();
  readonly loading = this._loading.asReadonly();
  readonly error = this._error.asReadonly();
  readonly oauthState = this._oauthState.asReadonly();
  readonly pendingSessionId = this._pendingSessionId.asReadonly();
  readonly pendingCalendars = this._pendingCalendars.asReadonly();
  readonly pendingAccountEmail = this._pendingAccountEmail.asReadonly();
  readonly syncing = this._syncing.asReadonly();

  /**
   * Get all calendar connections for a family
   */
  getConnections(familyId: string): Observable<CalendarConnection[]> {
    this._loading.set(true);
    this._error.set(null);

    return this.api
      .get<CalendarConnection[]>(`calendar-connections/family/${familyId}`)
      .pipe(
        tap((connections) => {
          this._connections.set(connections);
          this._loading.set(false);
        }),
        catchError((error) => {
          this._error.set(error.message || 'Failed to load calendar connections');
          this._loading.set(false);
          throw error;
        })
      );
  }

  /**
   * Get a specific calendar connection
   */
  getConnection(familyId: string, connectionId: string): Observable<CalendarConnection> {
    return this.api.get<CalendarConnection>(
      `calendar-connections/family/${familyId}/${connectionId}`
    );
  }

  /**
   * Start OAuth flow for a calendar provider
   */
  startOAuth(
    familyId: string,
    provider: CalendarProvider,
    redirectUri: string
  ): Observable<OAuthStartResponse> {
    this._loading.set(true);
    this._error.set(null);

    return this.api
      .post<OAuthStartResponse>(
        `calendar-connections/family/${familyId}/oauth/start`,
        { provider, redirectUri }
      )
      .pipe(
        tap((response) => {
          this._oauthState.set(response.state);
          this._loading.set(false);
        }),
        catchError((error) => {
          this._error.set(error.message || 'Failed to start OAuth flow');
          this._loading.set(false);
          throw error;
        })
      );
  }

  /**
   * Complete OAuth flow and get available calendars
   */
  completeOAuth(
    familyId: string,
    request: OAuthCompleteRequest
  ): Observable<OAuthCompleteResponse> {
    this._loading.set(true);
    this._error.set(null);

    return this.api
      .post<OAuthCompleteResponse>(
        `calendar-connections/family/${familyId}/oauth/complete`,
        request
      )
      .pipe(
        tap((response) => {
          this._pendingSessionId.set(response.sessionId);
          this._pendingCalendars.set(response.calendars);
          this._pendingAccountEmail.set(response.accountEmail);
          this._oauthState.set(null);
          this._loading.set(false);
        }),
        catchError((error) => {
          this._error.set(error.message || 'Failed to complete OAuth flow');
          this._oauthState.set(null);
          this._loading.set(false);
          throw error;
        })
      );
  }

  /**
   * Create calendar connections from an OAuth session
   */
  createConnectionsFromSession(
    familyId: string,
    request: CreateConnectionsFromSessionRequest
  ): Observable<CalendarConnection[]> {
    this._loading.set(true);
    this._error.set(null);

    return this.api
      .post<CalendarConnection[]>(
        `calendar-connections/family/${familyId}/oauth/connections`,
        request
      )
      .pipe(
        tap((connections) => {
          this._connections.update((existing) => [...existing, ...connections]);
          this._loading.set(false);
        }),
        catchError((error) => {
          this._error.set(error.message || 'Failed to create calendar connections');
          this._loading.set(false);
          throw error;
        })
      );
  }

  /**
   * Create a new calendar connection
   */
  createConnection(
    familyId: string,
    request: CreateCalendarConnectionRequest
  ): Observable<CalendarConnection> {
    this._loading.set(true);
    this._error.set(null);

    return this.api
      .post<CalendarConnection>(
        `calendar-connections/family/${familyId}`,
        request
      )
      .pipe(
        tap((connection) => {
          this._connections.update((connections) => [...connections, connection]);
          this._loading.set(false);
        }),
        catchError((error) => {
          this._error.set(error.message || 'Failed to create calendar connection');
          this._loading.set(false);
          throw error;
        })
      );
  }

  /**
   * Update a calendar connection
   */
  updateConnection(
    familyId: string,
    connectionId: string,
    request: UpdateCalendarConnectionRequest
  ): Observable<CalendarConnection> {
    this._loading.set(true);
    this._error.set(null);

    return this.api
      .put<CalendarConnection>(
        `calendar-connections/family/${familyId}/${connectionId}`,
        request
      )
      .pipe(
        tap((connection) => {
          this._connections.update((connections) =>
            connections.map((c) => (c.id === connection.id ? connection : c))
          );
          this._loading.set(false);
        }),
        catchError((error) => {
          this._error.set(error.message || 'Failed to update calendar connection');
          this._loading.set(false);
          throw error;
        })
      );
  }

  /**
   * Delete a calendar connection
   */
  deleteConnection(familyId: string, connectionId: string): Observable<void> {
    this._loading.set(true);
    this._error.set(null);

    return this.api
      .delete<void>(`calendar-connections/family/${familyId}/${connectionId}`)
      .pipe(
        tap(() => {
          this._connections.update((connections) =>
            connections.filter((c) => c.id !== connectionId)
          );
          this._loading.set(false);
        }),
        catchError((error) => {
          this._error.set(error.message || 'Failed to delete calendar connection');
          this._loading.set(false);
          throw error;
        })
      );
  }

  /**
   * Trigger a manual sync for a calendar connection
   */
  syncConnection(
    familyId: string,
    connectionId: string
  ): Observable<SyncCalendarResponse> {
    // Add to syncing set
    this._syncing.update((set) => {
      const newSet = new Set(set);
      newSet.add(connectionId);
      return newSet;
    });
    this._error.set(null);

    return this.api
      .post<SyncCalendarResponse>(
        `calendar-connections/family/${familyId}/${connectionId}/sync`,
        {}
      )
      .pipe(
        tap((response) => {
          // Update connection with new sync info
          this._connections.update((connections) =>
            connections.map((c) =>
              c.id === connectionId
                ? {
                    ...c,
                    lastSyncAt: response.syncedAt,
                    lastSyncEventCount:
                      response.eventsAdded +
                      response.eventsUpdated +
                      response.eventsRemoved,
                    lastSyncError: undefined,
                    status: CalendarConnectionStatus.Active,
                  }
                : c
            )
          );
          // Remove from syncing set
          this._syncing.update((set) => {
            const newSet = new Set(set);
            newSet.delete(connectionId);
            return newSet;
          });
        }),
        catchError((error) => {
          this._error.set(error.message || 'Failed to sync calendar');
          // Remove from syncing set
          this._syncing.update((set) => {
            const newSet = new Set(set);
            newSet.delete(connectionId);
            return newSet;
          });
          throw error;
        })
      );
  }

  /**
   * Validate an ICS URL
   */
  validateIcsUrl(
    familyId: string,
    url: string
  ): Observable<ValidateIcsUrlResponse> {
    return this.api.post<ValidateIcsUrlResponse>(
      `calendar-connections/family/${familyId}/validate-ics`,
      { url }
    );
  }

  /**
   * Clear pending OAuth data
   */
  clearPendingOAuth(): void {
    this._oauthState.set(null);
    this._pendingSessionId.set(null);
    this._pendingCalendars.set([]);
    this._pendingAccountEmail.set(null);
  }

  /**
   * Clear error state
   */
  clearError(): void {
    this._error.set(null);
  }

  /**
   * Clear all state (on logout)
   */
  clearState(): void {
    this._connections.set([]);
    this._loading.set(false);
    this._error.set(null);
    this._oauthState.set(null);
    this._pendingSessionId.set(null);
    this._pendingCalendars.set([]);
    this._pendingAccountEmail.set(null);
    this._syncing.set(new Set());
  }

  /**
   * Check if a connection is currently syncing
   */
  isConnectionSyncing(connectionId: string): boolean {
    return this._syncing().has(connectionId);
  }

  /**
   * Get provider display name
   */
  getProviderDisplayName(provider: CalendarProvider): string {
    const names: Partial<Record<CalendarProvider, string>> = {
      [CalendarProvider.Google]: 'Google Calendar',
      [CalendarProvider.Outlook]: 'Microsoft Outlook',
      [CalendarProvider.ICloud]: 'Apple iCloud',
      [CalendarProvider.CalDav]: 'CalDAV',
      [CalendarProvider.IcsUrl]: 'Calendar URL',
      [CalendarProvider.Internal]: 'Luminous Calendar',
    };
    return names[provider] || provider;
  }

  /**
   * Get status display info
   */
  getStatusInfo(status: CalendarConnectionStatus): {
    label: string;
    color: string;
    bgColor: string;
  } {
    const info: Record<
      CalendarConnectionStatus,
      { label: string; color: string; bgColor: string }
    > = {
      [CalendarConnectionStatus.PendingAuth]: {
        label: 'Pending',
        color: 'text-yellow-700',
        bgColor: 'bg-yellow-100',
      },
      [CalendarConnectionStatus.Active]: {
        label: 'Active',
        color: 'text-green-700',
        bgColor: 'bg-green-100',
      },
      [CalendarConnectionStatus.Paused]: {
        label: 'Paused',
        color: 'text-gray-700',
        bgColor: 'bg-gray-100',
      },
      [CalendarConnectionStatus.AuthError]: {
        label: 'Re-auth needed',
        color: 'text-orange-700',
        bgColor: 'bg-orange-100',
      },
      [CalendarConnectionStatus.SyncError]: {
        label: 'Sync error',
        color: 'text-red-700',
        bgColor: 'bg-red-100',
      },
      [CalendarConnectionStatus.Disconnected]: {
        label: 'Disconnected',
        color: 'text-gray-700',
        bgColor: 'bg-gray-100',
      },
    };
    return info[status] || { label: status, color: 'text-gray-700', bgColor: 'bg-gray-100' };
  }
}
