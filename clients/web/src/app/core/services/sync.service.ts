import { Injectable, inject, signal, computed, OnDestroy } from '@angular/core';
import { Subject, Observable, fromEvent, merge } from 'rxjs';
import { filter, takeUntil } from 'rxjs/operators';
import * as signalR from '@microsoft/signalr';

import { AuthService } from './auth.service';
import { environment } from '../../../environments/environment';
import {
  SyncMessage,
  SyncMessageType,
  EventSyncMessage,
  ChoreSyncMessage,
  CalendarSyncCompletedMessage,
  UserSyncMessage,
  ConnectionState,
} from '../../models';

/**
 * Service for real-time synchronization using SignalR.
 * Manages connection to the SignalR hub and broadcasts sync messages.
 */
@Injectable({
  providedIn: 'root',
})
export class SyncService implements OnDestroy {
  private readonly authService = inject(AuthService);

  // Connection state
  private connection: signalR.HubConnection | null = null;
  private readonly _connectionState = signal<ConnectionState>(ConnectionState.Disconnected);
  private readonly _lastError = signal<string | null>(null);
  private reconnectAttempts = 0;
  private reconnectTimer: ReturnType<typeof setTimeout> | null = null;

  // Destroy subject for cleanup
  private readonly destroy$ = new Subject<void>();

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

  constructor() {
    // Listen for auth changes
    this.setupAuthListener();

    // Listen for online/offline events
    this.setupNetworkListener();
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
    const token = this.authService.getAccessToken();
    if (!token) {
      console.warn('[SyncService] Cannot connect: not authenticated');
      return;
    }

    this._connectionState.set(ConnectionState.Connecting);
    this._lastError.set(null);

    try {
      // Build the connection
      this.connection = new signalR.HubConnectionBuilder()
        .withUrl(environment.signalRUrl, {
          accessTokenFactory: () => this.authService.getAccessToken() || '',
        })
        .withAutomaticReconnect({
          nextRetryDelayInMilliseconds: (retryContext) => {
            // Exponential backoff with max delay
            const delay = Math.min(
              environment.sync.reconnectIntervalMs * Math.pow(2, retryContext.previousRetryCount),
              environment.sync.maxReconnectDelayMs
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

  /**
   * Observable for all sync messages.
   */
  onSync(): Observable<SyncMessage> {
    return this.syncMessage$.asObservable();
  }

  /**
   * Observable for event change messages.
   */
  onEventChanged(): Observable<EventSyncMessage> {
    return this.eventChanged$.asObservable();
  }

  /**
   * Observable for specific event change types.
   */
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

  /**
   * Observable for chore change messages.
   */
  onChoreChanged(): Observable<ChoreSyncMessage> {
    return this.choreChanged$.asObservable();
  }

  /**
   * Observable for user change messages.
   */
  onUserChanged(): Observable<UserSyncMessage> {
    return this.userChanged$.asObservable();
  }

  /**
   * Observable for calendar sync completed messages.
   */
  onCalendarSyncCompleted(): Observable<CalendarSyncCompletedMessage> {
    return this.calendarSyncCompleted$.asObservable();
  }

  /**
   * Observable for family change messages.
   */
  onFamilyChanged(): Observable<SyncMessage> {
    return this.familyChanged$.asObservable();
  }

  /**
   * Observable for device change messages.
   */
  onDeviceChanged(): Observable<SyncMessage> {
    return this.deviceChanged$.asObservable();
  }

  /**
   * Observable for full sync required messages.
   */
  onFullSyncRequired(): Observable<SyncMessage> {
    return this.fullSyncRequired$.asObservable();
  }

  // ============================================
  // Private methods
  // ============================================

  private setupAuthListener(): void {
    // Watch for auth state changes
    // Connect when authenticated, disconnect when logged out
    let wasAuthenticated = this.authService.isAuthenticated();

    // Check periodically for auth changes
    const checkInterval = setInterval(() => {
      const isAuthenticated = this.authService.isAuthenticated();

      if (isAuthenticated && !wasAuthenticated) {
        // User logged in - connect
        this.connect();
      } else if (!isAuthenticated && wasAuthenticated) {
        // User logged out - disconnect
        this.disconnect();
      }

      wasAuthenticated = isAuthenticated;
    }, 1000);

    this.destroy$.subscribe(() => clearInterval(checkInterval));
  }

  private setupNetworkListener(): void {
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
      if (this.authService.isAuthenticated()) {
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

      // Route to specific subjects based on type
      if (message.type === SyncMessageType.FullSyncRequired) {
        this.fullSyncRequired$.next(message);
      }
    });

    // Event change handler
    this.connection.on('EventChanged', (message: EventSyncMessage) => {
      console.debug('[SyncService] Event changed:', message.type, message.eventId);
      this.eventChanged$.next(message);
    });

    // Chore change handler
    this.connection.on('ChoreChanged', (message: ChoreSyncMessage) => {
      console.debug('[SyncService] Chore changed:', message.type, message.choreId);
      this.choreChanged$.next(message);
    });

    // User change handler
    this.connection.on('UserChanged', (message: UserSyncMessage) => {
      console.debug('[SyncService] User changed:', message.type, message.userId);
      this.userChanged$.next(message);
    });

    // Family change handler
    this.connection.on('FamilyChanged', (message: SyncMessage) => {
      console.debug('[SyncService] Family changed:', message.type);
      this.familyChanged$.next(message);
    });

    // Calendar sync completed handler
    this.connection.on('CalendarSyncCompleted', (message: CalendarSyncCompletedMessage) => {
      console.debug('[SyncService] Calendar sync completed:', message.provider);
      this.calendarSyncCompleted$.next(message);
    });

    // Device change handler
    this.connection.on('DeviceChanged', (message: SyncMessage) => {
      console.debug('[SyncService] Device changed:', message.type, message.entityId);
      this.deviceChanged$.next(message);
    });

    // Full sync required handler
    this.connection.on('FullSyncRequired', (message: SyncMessage) => {
      console.debug('[SyncService] Full sync required');
      this.fullSyncRequired$.next(message);
    });
  }

  private scheduleReconnect(): void {
    if (this.reconnectTimer) {
      return; // Already scheduled
    }

    if (this.reconnectAttempts >= environment.sync.maxReconnectAttempts) {
      console.warn('[SyncService] Max reconnect attempts reached');
      this._lastError.set('Unable to connect after multiple attempts');
      return;
    }

    // Exponential backoff with jitter
    const baseDelay = environment.sync.reconnectIntervalMs;
    const delay = Math.min(
      baseDelay * Math.pow(2, this.reconnectAttempts) + Math.random() * 1000,
      environment.sync.maxReconnectDelayMs
    );

    console.log(`[SyncService] Scheduling reconnect in ${delay}ms (attempt ${this.reconnectAttempts + 1})`);

    this.reconnectTimer = setTimeout(() => {
      this.reconnectTimer = null;
      this.reconnectAttempts++;
      this.connect();
    }, delay);
  }
}
