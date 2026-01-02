import { Injectable, inject, signal, computed } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { Observable, firstValueFrom, interval, Subscription } from 'rxjs';
import { tap, switchMap, takeWhile } from 'rxjs/operators';
import { ElectronService, DeviceTokenData } from './electron.service';
import { environment } from '../../../environments/environment';

export interface LinkCodeRequest {
  deviceType: 'Display' | 'Mobile' | 'Web';
  platform?: string;
}

export interface LinkCodeResponse {
  linkCode: string;
  expiresAt: string;
  deviceId: string;
}

export interface LinkStatusResponse {
  status: 'pending' | 'linked' | 'expired';
  deviceToken?: string;
  familyId?: string;
  familyName?: string;
}

export interface DeviceHeartbeatResponse {
  deviceId: string;
  lastSeenAt: string;
  isActive: boolean;
  appVersion?: string;
}

/**
 * Service for device authentication via 6-digit linking code.
 * Handles the device linking flow and token management.
 */
@Injectable({
  providedIn: 'root',
})
export class DeviceAuthService {
  private readonly http = inject(HttpClient);
  private readonly router = inject(Router);
  private readonly electronService = inject(ElectronService);

  private readonly _deviceToken = signal<DeviceTokenData | null>(null);
  private readonly _linkCode = signal<string | null>(null);
  private readonly _isLinking = signal(false);
  private readonly _linkError = signal<string | null>(null);

  private linkPollingSubscription?: Subscription;
  private heartbeatSubscription?: Subscription;

  readonly deviceToken = this._deviceToken.asReadonly();
  readonly linkCode = this._linkCode.asReadonly();
  readonly isLinking = this._isLinking.asReadonly();
  readonly linkError = this._linkError.asReadonly();

  readonly isAuthenticated = computed(() => !!this._deviceToken()?.token);
  readonly familyName = computed(() => this._deviceToken()?.familyName ?? null);

  constructor() {
    this.loadStoredToken();
  }

  /**
   * Load token from storage on startup
   */
  private async loadStoredToken(): Promise<void> {
    try {
      const tokenData = await this.electronService.getDeviceToken();
      if (tokenData?.token) {
        this._deviceToken.set(tokenData);
        this.startHeartbeat();
      }
    } catch (error) {
      console.error('Failed to load device token:', error);
    }
  }

  /**
   * Request a new link code from the server
   */
  async requestLinkCode(): Promise<string> {
    this._isLinking.set(true);
    this._linkError.set(null);

    try {
      // Build request with device information
      const request: LinkCodeRequest = {
        deviceType: 'Display',
        platform: this.electronService.isElectron()
          ? `Electron/${window.electronAPI?.platform || 'unknown'}`
          : 'Browser',
      };

      const response = await firstValueFrom(
        this.http.post<LinkCodeResponse>(
          `${environment.apiUrl}/devices/link-code`,
          request
        )
      );

      this._linkCode.set(response.linkCode);
      this.startLinkPolling(response.deviceId, response.linkCode);

      return response.linkCode;
    } catch (error) {
      console.error('Failed to request link code:', error);
      this._linkError.set('Failed to generate link code. Please try again.');
      this._isLinking.set(false);
      throw error;
    }
  }

  /**
   * Poll for link status until device is linked or code expires
   */
  private startLinkPolling(deviceId: string, code: string): void {
    this.stopLinkPolling();

    // Poll every 2 seconds
    this.linkPollingSubscription = interval(2000)
      .pipe(
        switchMap(() =>
          this.http.get<LinkStatusResponse>(
            `${environment.apiUrl}/devices/link-status/${deviceId}`
          )
        ),
        takeWhile((response) => response.status === 'pending', true)
      )
      .subscribe({
        next: async (response) => {
          if (response.status === 'linked' && response.deviceToken) {
            await this.handleLinkSuccess(deviceId, response);
          } else if (response.status === 'expired') {
            this._linkError.set('Link code expired. Please request a new one.');
            this._linkCode.set(null);
            this._isLinking.set(false);
          }
        },
        error: (error) => {
          console.error('Link polling error:', error);
          this._linkError.set('Connection error. Please check your network.');
          this._isLinking.set(false);
        },
      });
  }

  /**
   * Handle successful device linking
   */
  private async handleLinkSuccess(
    deviceId: string,
    response: LinkStatusResponse
  ): Promise<void> {
    const tokenData: DeviceTokenData = {
      deviceId,
      token: response.deviceToken!,
      familyId: response.familyId!,
      familyName: response.familyName!,
      linkedAt: new Date().toISOString(),
    };

    await this.electronService.setDeviceToken(tokenData);
    this._deviceToken.set(tokenData);
    this._linkCode.set(null);
    this._isLinking.set(false);

    this.startHeartbeat();
    this.router.navigate(['/display']);
  }

  /**
   * Stop polling for link status
   */
  private stopLinkPolling(): void {
    this.linkPollingSubscription?.unsubscribe();
    this.linkPollingSubscription = undefined;
  }

  /**
   * Cancel ongoing linking process
   */
  cancelLinking(): void {
    this.stopLinkPolling();
    this._linkCode.set(null);
    this._isLinking.set(false);
    this._linkError.set(null);
  }

  /**
   * Start heartbeat to validate token periodically
   */
  private startHeartbeat(): void {
    this.stopHeartbeat();

    const deviceId = this._deviceToken()?.deviceId;
    if (!deviceId) {
      console.warn('Cannot start heartbeat: no device ID');
      return;
    }

    // Heartbeat every 5 minutes
    this.heartbeatSubscription = interval(environment.display.heartbeatInterval)
      .pipe(
        switchMap(() =>
          this.http.post<DeviceHeartbeatResponse>(
            `${environment.apiUrl}/devices/${deviceId}/heartbeat`,
            {}
          )
        )
      )
      .subscribe({
        next: (response) => {
          if (!response.isActive) {
            console.log('Device is inactive, unlinking...');
            this.unlink();
          }
        },
        error: (error) => {
          // Network error - don't unlink, just log
          console.warn('Heartbeat failed:', error);
        },
      });
  }

  /**
   * Stop heartbeat
   */
  private stopHeartbeat(): void {
    this.heartbeatSubscription?.unsubscribe();
    this.heartbeatSubscription = undefined;
  }

  /**
   * Unlink device from family
   */
  async unlink(): Promise<void> {
    this.stopHeartbeat();
    this.stopLinkPolling();

    try {
      const token = this._deviceToken();
      if (token?.token) {
        await firstValueFrom(
          this.http.delete(`${environment.apiUrl}/devices/${token.deviceId}`)
        ).catch(() => {
          // Ignore API errors during unlink
        });
      }
    } catch {
      // Continue with local cleanup even if API fails
    }

    await this.electronService.clearDeviceToken();
    this._deviceToken.set(null);
    this.router.navigate(['/linking']);
  }

  /**
   * Get current device token for API requests
   */
  getToken(): string | null {
    return this._deviceToken()?.token ?? null;
  }

  /**
   * Check if device is authenticated
   */
  async checkAuth(): Promise<boolean> {
    const tokenData = await this.electronService.getDeviceToken();
    if (!tokenData?.token) {
      return false;
    }

    this._deviceToken.set(tokenData);
    return true;
  }
}
