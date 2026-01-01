import { Injectable, signal } from '@angular/core';

/**
 * Electron API exposed via preload script
 */
interface ElectronAPI {
  getDeviceToken: () => Promise<DeviceTokenData | null>;
  setDeviceToken: (tokenData: DeviceTokenData) => Promise<boolean>;
  clearDeviceToken: () => Promise<boolean>;
  getSettings: () => Promise<DisplaySettings>;
  setSettings: (settings: DisplaySettings) => Promise<boolean>;
  getAppInfo: () => Promise<AppInfo>;
  getDisplayInfo: () => Promise<DisplayInfo>;
  reloadWindow: () => Promise<void>;
  verifyExitPin: (pin: string) => Promise<boolean>;
  onShowExitDialog: (callback: () => void) => () => void;
  platform: string;
  isElectron: boolean;
}

export interface DeviceTokenData {
  deviceId: string;
  token: string;
  familyId: string;
  familyName: string;
  linkedAt: string;
  expiresAt?: string;
}

export interface DisplaySettings {
  defaultView?: 'schedule' | 'tasks' | 'routines';
  autoRotate?: boolean;
  autoRotateInterval?: number; // minutes
  brightness?: number; // 0-100
  nightDisplayEnabled?: boolean;
  nightDisplayStart?: string; // HH:mm
  nightDisplayEnd?: string; // HH:mm
  timeFormat?: '12h' | '24h';
  dateFormat?: string;
}

export interface AppInfo {
  version: string;
  name: string;
  isKiosk: boolean;
  isDev: boolean;
}

export interface DisplayInfo {
  width: number;
  height: number;
  scaleFactor: number;
  rotation: number;
  bounds: {
    x: number;
    y: number;
    width: number;
    height: number;
  };
}

declare global {
  interface Window {
    electronAPI?: ElectronAPI;
  }
}

/**
 * Service for communicating with Electron main process.
 * Handles device token storage and settings when running in Electron.
 */
@Injectable({
  providedIn: 'root',
})
export class ElectronService {
  private readonly _isElectron = signal(false);
  private readonly _appInfo = signal<AppInfo | null>(null);
  private readonly _displayInfo = signal<DisplayInfo | null>(null);

  readonly isElectron = this._isElectron.asReadonly();
  readonly appInfo = this._appInfo.asReadonly();
  readonly displayInfo = this._displayInfo.asReadonly();

  constructor() {
    this._isElectron.set(!!window.electronAPI?.isElectron);

    if (this._isElectron()) {
      this.loadAppInfo();
      this.loadDisplayInfo();
    }
  }

  private async loadAppInfo(): Promise<void> {
    try {
      const info = await window.electronAPI?.getAppInfo();
      if (info) {
        this._appInfo.set(info);
      }
    } catch (error) {
      console.error('Failed to load app info:', error);
    }
  }

  private async loadDisplayInfo(): Promise<void> {
    try {
      const info = await window.electronAPI?.getDisplayInfo();
      if (info) {
        this._displayInfo.set(info);
      }
    } catch (error) {
      console.error('Failed to load display info:', error);
    }
  }

  /**
   * Get stored device token
   */
  async getDeviceToken(): Promise<DeviceTokenData | null> {
    if (!this._isElectron()) {
      return this.getFromLocalStorage<DeviceTokenData>('luminous_device_token');
    }
    return window.electronAPI!.getDeviceToken();
  }

  /**
   * Store device token
   */
  async setDeviceToken(tokenData: DeviceTokenData): Promise<boolean> {
    if (!this._isElectron()) {
      return this.setToLocalStorage('luminous_device_token', tokenData);
    }
    return window.electronAPI!.setDeviceToken(tokenData);
  }

  /**
   * Clear device token (unlink device)
   */
  async clearDeviceToken(): Promise<boolean> {
    if (!this._isElectron()) {
      localStorage.removeItem('luminous_device_token');
      return true;
    }
    return window.electronAPI!.clearDeviceToken();
  }

  /**
   * Get display settings
   */
  async getSettings(): Promise<DisplaySettings> {
    if (!this._isElectron()) {
      return this.getFromLocalStorage<DisplaySettings>('luminous_display_settings') || {};
    }
    return window.electronAPI!.getSettings();
  }

  /**
   * Save display settings
   */
  async setSettings(settings: DisplaySettings): Promise<boolean> {
    if (!this._isElectron()) {
      return this.setToLocalStorage('luminous_display_settings', settings);
    }
    return window.electronAPI!.setSettings(settings);
  }

  /**
   * Reload the display window
   */
  async reloadWindow(): Promise<void> {
    if (this._isElectron()) {
      await window.electronAPI!.reloadWindow();
    } else {
      window.location.reload();
    }
  }

  /**
   * Verify admin exit PIN
   */
  async verifyExitPin(pin: string): Promise<boolean> {
    if (!this._isElectron()) {
      // In browser, just return true for testing
      return pin === '1234';
    }
    return window.electronAPI!.verifyExitPin(pin);
  }

  /**
   * Listen for exit dialog trigger
   */
  onShowExitDialog(callback: () => void): () => void {
    if (!this._isElectron()) {
      // In browser, listen for keyboard shortcut
      const handler = (e: KeyboardEvent) => {
        if ((e.ctrlKey || e.metaKey) && e.shiftKey && e.key === 'Q') {
          callback();
        }
      };
      window.addEventListener('keydown', handler);
      return () => window.removeEventListener('keydown', handler);
    }
    return window.electronAPI!.onShowExitDialog(callback);
  }

  // Fallback localStorage methods for browser development
  private getFromLocalStorage<T>(key: string): T | null {
    try {
      const item = localStorage.getItem(key);
      return item ? JSON.parse(item) : null;
    } catch {
      return null;
    }
  }

  private setToLocalStorage<T>(key: string, value: T): boolean {
    try {
      localStorage.setItem(key, JSON.stringify(value));
      return true;
    } catch {
      return false;
    }
  }
}
