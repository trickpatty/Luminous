/**
 * Device type enumeration
 */
export enum DeviceType {
  Display = 'Display',
  Mobile = 'Mobile',
  Web = 'Web',
}

/**
 * Device settings configuration
 */
export interface DeviceSettings {
  defaultView: 'day' | 'week' | 'month' | 'agenda';
  brightness: number;
  autoBrightness: boolean;
  orientation: 'portrait' | 'landscape';
  soundEnabled: boolean;
  volume: number;
}

/**
 * Device entity
 */
export interface Device {
  id: string;
  familyId: string;
  type: DeviceType;
  name: string;
  isLinked: boolean;
  linkedAt?: string;
  linkedBy?: string;
  settings: DeviceSettings;
  lastSeenAt: string;
  isActive: boolean;
  platform?: string;
  appVersion?: string;
}

/**
 * Device link code response
 */
export interface DeviceLinkCode {
  deviceId: string;
  linkCode: string;
  expiresAt: string;
}

/**
 * Linked device response (includes tokens)
 */
export interface LinkedDevice {
  device: Device;
  accessToken: string;
  refreshToken?: string;
  tokenType: string;
  expiresIn: number;
}

/**
 * Device heartbeat response
 */
export interface DeviceHeartbeat {
  deviceId: string;
  lastSeenAt: string;
  serverTime: string;
}

/**
 * Request to link a device
 */
export interface LinkDeviceRequest {
  linkCode: string;
}

/**
 * Request to update device settings
 */
export interface UpdateDeviceRequest {
  name?: string;
  settings?: Partial<DeviceSettings>;
}

/**
 * Request to generate a link code
 */
export interface GenerateLinkCodeRequest {
  deviceType: DeviceType;
  name: string;
  platform?: string;
}
