import { Injectable, InjectionToken, inject, signal, computed, OnDestroy } from '@angular/core';
import { Subject, Observable, fromEvent, merge } from 'rxjs';
import { filter, takeUntil } from 'rxjs/operators';
import * as signalR from '@microsoft/signalr';

import {
  SyncMessage,
  SyncMessageType,
  EventSyncMessage,
  ChoreSyncMessage,
  CalendarSyncCompletedMessage,
  UserSyncMessage,
  ConnectionState,
} from '../models';

/**
 * Configuration for the sync service
 */
export interface SyncConfig {
  hubUrl: string;
  reconnectIntervalMs: number;
  maxReconnectDelayMs: number;
  maxReconnectAttempts: number;
}

/**
 * Injection token for sync configuration
 */
export const SYNC_CONFIG = new InjectionToken<SyncConfig>('SYNC_CONFIG');

/**
 * Interface for auth token provider
 */
export interface AuthTokenProvider {
  getAccessToken(): string | null;
  isAuthenticated(): boolean;
}

/**
 * Injection token for auth token provider
 */
export const AUTH_TOKEN_PROVIDER = new InjectionToken<AuthTokenProvider>('AUTH_TOKEN_PROVIDER');

/**
 * Base service for real-time synchronization using SignalR.
 * Manages connection to the SignalR hub and broadcasts sync messages.
 *
 * This is a base class that should be extended by app-specific sync services.
 */
@Injectable()
export abstract class SyncBaseService implements OnDestroy {
  private readonly config = inject(SYNC_CONFIG, { optional: true });
  protected readonly authProvider = inject(AUTH_TOKEN_PROVIDER, { optional: true });

  // Connection state
  protected connection: signalR.HubConnection | null = null;
  private readonly _connectionState = signal<ConnectionState>(ConnectionState.Disconnected);
  private readonly _lastError = signal<string | null>(null);
  protected reconnectAttempts = 0;
  protected reconnectTimer: ReturnType<typeof setTimeout> | null = null;

  // Destroy subject for cleanup
  protected readonly destroy$ = new Subject<void>();

  // Message subjects for different message types
  private readonly syncMessage$ = new Subject<SyncMessage>();
  private readonly eventChanged$ = new Subject<EventSyncMessage>();
  private readonly choreChanged$ = new Subject<ChoreSyncMessage>();
  private readonly userChanged$ = new Subject<UserSyncMessage>();
  private readonly calendarSyncCompleted$ = new Subject<CalendarSyncCompletedMessage>();
  private readonly familyChanged$ = new Subject<SyncMessage>();
  private readonly deviceChanged$ = new Subject<SyncMessage>();
  private readonly fullSyncRequired$ = new Subject<SyncMessage>();

  // Public readonly signals
  readonly connectionState = this._connectionState.asReadonly();
  readonly lastError = this._lastError.asReadonly();
  readonly isConnected = computed(() => this._connectionState() === ConnectionState.Connected);

  protected get syncConfig(): SyncConfig {
    if (!this.config) {
      throw new Error('SYNC_CONFIG not provided');
    }
    return this.config;
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
    this.disconnect();
  }

  /**
   * Connect to the SignalR hub.
   * Automatically uses the current user's access token.
   */
  async connect(): Promise<void> {
    // Don't reconnect if already connected
    if (this._connectionState() === ConnectionState.Connected) {
      return;
    }

    // Don't try to connect if not authenticated
    const token = this.authProvider?.getAccessToken();
    if (!token) {
      console.warn('[SyncService] Cannot connect: not authenticated');
      return;
    }

    this._connectionState.set(ConnectionState.Connecting);
    this._lastError.set(null);

    try {
      // Build the connection
      this.connection = new signalR.HubConnectionBuilder()
        .withUrl(this.syncConfig.hubUrl, {
          accessTokenFactory: () => this.authProvider?.getAccessToken() || '',
        })
        .withAutomaticReconnect({
          nextRetryDelayInMilliseconds: (retryContext) => {
            // Exponential backoff with max delay
            const delay = Math.min(
              this.syncConfig.reconnectIntervalMs * Math.pow(2, retryContext.previousRetryCount),
              this.syncConfig.maxReconnectDelayMs
            );
            return delay;
          },
        })
        .configureLogging(signalR.LogLevel.Warning)
        .build();

      // Setup connection event handlers
      this.setupConnectionHandlers();

      // Setup message handlers
      this.setupMessageHandlers();

      // Start the connection
      await this.connection.start();
      this._connectionState.set(ConnectionState.Connected);
      this.reconnectAttempts = 0;
      console.log('[SyncService] Connected to SignalR hub');
    } catch (error) {
      console.error('[SyncService] Failed to connect:', error);
      this._connectionState.set(ConnectionState.Disconnected);
      this._lastError.set(error instanceof Error ? error.message : 'Connection failed');
      this.scheduleReconnect();
    }
  }

  /**
   * Disconnect from the SignalR hub.
   */
  async disconnect(): Promise<void> {
    if (this.reconnectTimer) {
      clearTimeout(this.reconnectTimer);
      this.reconnectTimer = null;
    }

    if (this.connection) {
      try {
        await this.connection.stop();
      } catch (error) {
        console.error('[SyncService] Error disconnecting:', error);
      }
      this.connection = null;
    }

    this._connectionState.set(ConnectionState.Disconnected);
  }

  /**
   * Manually trigger a reconnection attempt.
   */
  async reconnect(): Promise<void> {
    await this.disconnect();
    await this.connect();
  }

  /**
   * Send a ping to verify connection is active.
   */
  async ping(): Promise<string | null> {
    if (!this.connection || this._connectionState() !== ConnectionState.Connected) {
      return null;
    }

    try {
      return await this.connection.invoke<string>('Ping');
    } catch (error) {
      console.error('[SyncService] Ping failed:', error);
      return null;
    }
  }

  // ============================================
  // Observable streams for sync messages
  // ============================================

  onSync(): Observable<SyncMessage> {
    return this.syncMessage$.asObservable();
  }

  onEventChanged(): Observable<EventSyncMessage> {
    return this.eventChanged$.asObservable();
  }

  onEventCreated(): Observable<EventSyncMessage> {
    return this.eventChanged$.pipe(
      filter((m) => m.type === SyncMessageType.EventCreated)
    );
  }

  onEventUpdated(): Observable<EventSyncMessage> {
    return this.eventChanged$.pipe(
      filter((m) => m.type === SyncMessageType.EventUpdated)
    );
  }

  onEventDeleted(): Observable<EventSyncMessage> {
    return this.eventChanged$.pipe(
      filter((m) => m.type === SyncMessageType.EventDeleted)
    );
  }

  onChoreChanged(): Observable<ChoreSyncMessage> {
    return this.choreChanged$.asObservable();
  }

  onUserChanged(): Observable<UserSyncMessage> {
    return this.userChanged$.asObservable();
  }

  onCalendarSyncCompleted(): Observable<CalendarSyncCompletedMessage> {
    return this.calendarSyncCompleted$.asObservable();
  }

  onFamilyChanged(): Observable<SyncMessage> {
    return this.familyChanged$.asObservable();
  }

  onDeviceChanged(): Observable<SyncMessage> {
    return this.deviceChanged$.asObservable();
  }

  onFullSyncRequired(): Observable<SyncMessage> {
    return this.fullSyncRequired$.asObservable();
  }

  // ============================================
  // Protected methods for subclasses
  // ============================================

  protected setupNetworkListener(): void {
    if (typeof window !== 'undefined') {
      const online$ = fromEvent(window, 'online');
      const offline$ = fromEvent(window, 'offline');

      merge(online$, offline$)
        .pipe(takeUntil(this.destroy$))
        .subscribe((event) => {
          if (event.type === 'online') {
            console.log('[SyncService] Network online - attempting reconnect');
            this.connect();
          } else {
            console.log('[SyncService] Network offline');
            this._connectionState.set(ConnectionState.Disconnected);
          }
        });
    }
  }

  // ============================================
  // Private methods
  // ============================================

  private setupConnectionHandlers(): void {
    if (!this.connection) return;

    this.connection.onreconnecting((error) => {
      console.log('[SyncService] Reconnecting...', error);
      this._connectionState.set(ConnectionState.Reconnecting);
      this._lastError.set(error?.message || 'Reconnecting');
    });

    this.connection.onreconnected((connectionId) => {
      console.log('[SyncService] Reconnected:', connectionId);
      this._connectionState.set(ConnectionState.Connected);
      this._lastError.set(null);
      this.reconnectAttempts = 0;
    });

    this.connection.onclose((error) => {
      console.log('[SyncService] Connection closed:', error);
      this._connectionState.set(ConnectionState.Disconnected);
      if (error) {
        this._lastError.set(error.message);
      }

      // Only schedule reconnect if still authenticated
      if (this.authProvider?.isAuthenticated()) {
        this.scheduleReconnect();
      }
    });
  }

  private setupMessageHandlers(): void {
    if (!this.connection) return;

    // Generic sync message handler
    this.connection.on('Sync', (message: SyncMessage) => {
      console.debug('[SyncService] Sync message received:', message.type);
      this.syncMessage$.next(message);

      if (message.type === SyncMessageType.FullSyncRequired) {
        this.fullSyncRequired$.next(message);
      }
    });

    this.connection.on('EventChanged', (message: EventSyncMessage) => {
      console.debug('[SyncService] Event changed:', message.type, message.eventId);
      this.eventChanged$.next(message);
    });

    this.connection.on('ChoreChanged', (message: ChoreSyncMessage) => {
      console.debug('[SyncService] Chore changed:', message.type, message.choreId);
      this.choreChanged$.next(message);
    });

    this.connection.on('UserChanged', (message: UserSyncMessage) => {
      console.debug('[SyncService] User changed:', message.type, message.userId);
      this.userChanged$.next(message);
    });

    this.connection.on('FamilyChanged', (message: SyncMessage) => {
      console.debug('[SyncService] Family changed:', message.type);
      this.familyChanged$.next(message);
    });

    this.connection.on('CalendarSyncCompleted', (message: CalendarSyncCompletedMessage) => {
      console.debug('[SyncService] Calendar sync completed:', message.provider);
      this.calendarSyncCompleted$.next(message);
    });

    this.connection.on('DeviceChanged', (message: SyncMessage) => {
      console.debug('[SyncService] Device changed:', message.type, message.entityId);
      this.deviceChanged$.next(message);
    });

    this.connection.on('FullSyncRequired', (message: SyncMessage) => {
      console.debug('[SyncService] Full sync required');
      this.fullSyncRequired$.next(message);
    });
  }

  private scheduleReconnect(): void {
    if (this.reconnectTimer) {
      return;
    }

    if (this.reconnectAttempts >= this.syncConfig.maxReconnectAttempts) {
      console.warn('[SyncService] Max reconnect attempts reached');
      this._lastError.set('Unable to connect after multiple attempts');
      return;
    }

    const baseDelay = this.syncConfig.reconnectIntervalMs;
    const delay = Math.min(
      baseDelay * Math.pow(2, this.reconnectAttempts) + Math.random() * 1000,
      this.syncConfig.maxReconnectDelayMs
    );

    console.log(`[SyncService] Scheduling reconnect in ${delay}ms (attempt ${this.reconnectAttempts + 1})`);

    this.reconnectTimer = setTimeout(() => {
      this.reconnectTimer = null;
      this.reconnectAttempts++;
      this.connect();
    }, delay);
  }
}
