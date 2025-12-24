import { Injectable, inject, signal } from '@angular/core';
import { Observable, tap, catchError } from 'rxjs';
import { ApiService } from './api.service';
import {
  Device,
  DeviceType,
  DeviceLinkCode,
  LinkedDevice,
  DeviceHeartbeat,
  LinkDeviceRequest,
  UpdateDeviceRequest,
  GenerateLinkCodeRequest,
} from '../../models';

/**
 * Service for managing family devices
 */
@Injectable({
  providedIn: 'root',
})
export class DeviceService {
  private readonly api = inject(ApiService);

  // State
  private readonly _devices = signal<Device[]>([]);
  private readonly _loading = signal(false);
  private readonly _error = signal<string | null>(null);
  private readonly _linkCode = signal<DeviceLinkCode | null>(null);

  // Public selectors
  readonly devices = this._devices.asReadonly();
  readonly loading = this._loading.asReadonly();
  readonly error = this._error.asReadonly();
  readonly linkCode = this._linkCode.asReadonly();

  /**
   * Get all family devices
   */
  getDevices(familyId: string, activeOnly: boolean = false): Observable<Device[]> {
    this._loading.set(true);
    this._error.set(null);

    const params = activeOnly ? { activeOnly: 'true' } : {};

    return this.api
      .get<Device[]>(`devices/family/${familyId}`, { params })
      .pipe(
        tap((devices) => {
          this._devices.set(devices);
          this._loading.set(false);
        }),
        catchError((error) => {
          this._error.set(error.message || 'Failed to load devices');
          this._loading.set(false);
          throw error;
        })
      );
  }

  /**
   * Get a specific device
   */
  getDevice(familyId: string, deviceId: string): Observable<Device> {
    return this.api.get<Device>(`devices/family/${familyId}/${deviceId}`);
  }

  /**
   * Generate a link code for device pairing
   */
  generateLinkCode(request: GenerateLinkCodeRequest): Observable<DeviceLinkCode> {
    this._loading.set(true);
    this._error.set(null);

    return this.api.post<DeviceLinkCode>('devices/link-code', request).pipe(
      tap((linkCode) => {
        this._linkCode.set(linkCode);
        this._loading.set(false);
      }),
      catchError((error) => {
        this._error.set(error.message || 'Failed to generate link code');
        this._loading.set(false);
        throw error;
      })
    );
  }

  /**
   * Link a device to the family using a code
   */
  linkDevice(request: LinkDeviceRequest): Observable<LinkedDevice> {
    this._loading.set(true);

    return this.api.post<LinkedDevice>('devices/link', request).pipe(
      tap((result) => {
        // Add the newly linked device to the list
        this._devices.update((devices) => [...devices, result.device]);
        this._linkCode.set(null);
        this._loading.set(false);
      }),
      catchError((error) => {
        this._error.set(error.message || 'Failed to link device');
        this._loading.set(false);
        throw error;
      })
    );
  }

  /**
   * Update device settings
   */
  updateDevice(
    familyId: string,
    deviceId: string,
    request: UpdateDeviceRequest
  ): Observable<Device> {
    this._loading.set(true);

    return this.api
      .put<Device>(`devices/family/${familyId}/${deviceId}`, request)
      .pipe(
        tap((device) => {
          this._devices.update((devices) =>
            devices.map((d) => (d.id === device.id ? device : d))
          );
          this._loading.set(false);
        }),
        catchError((error) => {
          this._error.set(error.message || 'Failed to update device');
          this._loading.set(false);
          throw error;
        })
      );
  }

  /**
   * Unlink a device from the family
   */
  unlinkDevice(familyId: string, deviceId: string): Observable<Device> {
    this._loading.set(true);

    return this.api
      .post<Device>(`devices/family/${familyId}/${deviceId}/unlink`, {})
      .pipe(
        tap((device) => {
          this._devices.update((devices) =>
            devices.map((d) => (d.id === device.id ? device : d))
          );
          this._loading.set(false);
        }),
        catchError((error) => {
          this._error.set(error.message || 'Failed to unlink device');
          this._loading.set(false);
          throw error;
        })
      );
  }

  /**
   * Delete a device
   */
  deleteDevice(familyId: string, deviceId: string): Observable<void> {
    this._loading.set(true);

    return this.api
      .delete<void>(`devices/family/${familyId}/${deviceId}`)
      .pipe(
        tap(() => {
          this._devices.update((devices) =>
            devices.filter((d) => d.id !== deviceId)
          );
          this._loading.set(false);
        }),
        catchError((error) => {
          this._error.set(error.message || 'Failed to delete device');
          this._loading.set(false);
          throw error;
        })
      );
  }

  /**
   * Record device heartbeat
   */
  recordHeartbeat(
    familyId: string,
    deviceId: string
  ): Observable<DeviceHeartbeat> {
    return this.api.post<DeviceHeartbeat>(
      `devices/family/${familyId}/${deviceId}/heartbeat`,
      {}
    );
  }

  /**
   * Clear link code state
   */
  clearLinkCode(): void {
    this._linkCode.set(null);
  }

  /**
   * Clear state (on logout)
   */
  clearState(): void {
    this._devices.set([]);
    this._loading.set(false);
    this._error.set(null);
    this._linkCode.set(null);
  }

  /**
   * Get device type display name
   */
  getDeviceTypeDisplayName(type: DeviceType): string {
    const names: Record<DeviceType, string> = {
      [DeviceType.Display]: 'Wall Display',
      [DeviceType.Mobile]: 'Mobile App',
      [DeviceType.Web]: 'Web Browser',
    };
    return names[type] || type;
  }

  /**
   * Get device type icon class
   */
  getDeviceTypeIcon(type: DeviceType): string {
    const icons: Record<DeviceType, string> = {
      [DeviceType.Display]: 'display',
      [DeviceType.Mobile]: 'smartphone',
      [DeviceType.Web]: 'globe',
    };
    return icons[type] || 'device';
  }
}
