/**
 * Family settings
 */
export interface FamilySettings {
  defaultView: 'day' | 'week' | 'month' | 'agenda';
  privacyModeEnabled: boolean;
  privacyModeTimeoutMinutes: number;
  sleepMode: SleepModeSettings;
}

/**
 * Sleep mode configuration
 */
export interface SleepModeSettings {
  enabled: boolean;
  startTime: string; // HH:mm format
  endTime: string;   // HH:mm format
  daysOfWeek: number[]; // 0-6, Sunday = 0
}

/**
 * Subscription information
 */
export interface SubscriptionInfo {
  plan: SubscriptionPlan;
  status: SubscriptionStatus;
  expiresAt?: string;
}

/**
 * Available subscription plans
 */
export enum SubscriptionPlan {
  Free = 'free',
  Family = 'family',
  Premium = 'premium',
}

/**
 * Subscription status
 */
export enum SubscriptionStatus {
  Active = 'active',
  Trial = 'trial',
  Expired = 'expired',
  Cancelled = 'cancelled',
}

/**
 * Family entity (tenant)
 */
export interface Family {
  id: string;
  name: string;
  timezone: string;
  settings: FamilySettings;
  subscription?: SubscriptionInfo;
  createdAt: string;
  createdBy: string;
  memberCount?: number;
  deviceCount?: number;
}

/**
 * Request to update family settings
 */
export interface UpdateFamilySettingsRequest {
  name?: string;
  timezone?: string;
  settings?: Partial<FamilySettings>;
}
