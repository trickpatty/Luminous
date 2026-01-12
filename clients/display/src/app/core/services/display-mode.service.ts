import { Injectable, signal, computed, inject, OnDestroy } from '@angular/core';
import {
  ElectronService,
  DisplaySettings,
  PrivacyModeSettings,
  SleepModeSettings,
} from './electron.service';

/**
 * Display modes for the Luminous display application.
 *
 * - normal: Full display showing calendar, tasks, widgets, etc.
 * - privacy: Wallpaper/clock only mode - hides family information for privacy
 * - sleep: Screen dimmed/off during scheduled times
 */
export type DisplayMode = 'normal' | 'privacy' | 'sleep';

// Re-export types for convenience
export { PrivacyModeSettings, SleepModeSettings } from './electron.service';

/**
 * Default settings
 */
const DEFAULT_PRIVACY_SETTINGS: PrivacyModeSettings = {
  enabled: true,
  timeoutMinutes: 5,
  showClock: true,
  showWeather: false,
};

const DEFAULT_SLEEP_SETTINGS: SleepModeSettings = {
  enabled: false,
  startTime: '22:00',
  endTime: '06:00',
  dimLevel: 10,
  wakeOnTouch: true,
};

/**
 * Service for managing display modes (Normal, Privacy, Sleep).
 *
 * Features:
 * - Normal mode: Full display with all widgets and data
 * - Privacy mode: One-tap switch to wallpaper/clock only (hides family info)
 * - Sleep mode: Scheduled screen dimming/off with wake on touch
 *
 * Implements requirements:
 * - DSP-001: One-tap privacy mode
 * - DSP-002: Automatic privacy mode after timeout
 * - DSP-010: Scheduled screen-off times
 * - DSP-011: Manual sleep toggle
 * - DSP-012: Wake on touch or schedule
 */
@Injectable({
  providedIn: 'root',
})
export class DisplayModeService implements OnDestroy {
  private readonly electronService = inject(ElectronService);

  // Current display mode
  private readonly _currentMode = signal<DisplayMode>('normal');
  private readonly _previousMode = signal<DisplayMode>('normal');

  // Settings
  private readonly _privacySettings = signal<PrivacyModeSettings>(DEFAULT_PRIVACY_SETTINGS);
  private readonly _sleepSettings = signal<SleepModeSettings>(DEFAULT_SLEEP_SETTINGS);

  // Activity tracking
  private readonly _lastActivityTime = signal<number>(Date.now());
  private readonly _isManualPrivacy = signal(false);
  private readonly _isManualSleep = signal(false);

  // Timers
  private activityTimer?: ReturnType<typeof setInterval>;
  private scheduleTimer?: ReturnType<typeof setInterval>;

  // Public readonly signals
  readonly currentMode = this._currentMode.asReadonly();
  readonly privacySettings = this._privacySettings.asReadonly();
  readonly sleepSettings = this._sleepSettings.asReadonly();
  readonly lastActivityTime = this._lastActivityTime.asReadonly();

  // Computed: Is in privacy mode (manual or automatic)
  readonly isPrivacyMode = computed(() => this._currentMode() === 'privacy');

  // Computed: Is in sleep mode (manual or scheduled)
  readonly isSleepMode = computed(() => this._currentMode() === 'sleep');

  // Computed: Is in normal mode
  readonly isNormalMode = computed(() => this._currentMode() === 'normal');

  // Computed: Minutes until auto-privacy (for UI display)
  readonly minutesUntilPrivacy = computed(() => {
    if (!this._privacySettings().enabled || this._currentMode() !== 'normal') {
      return -1;
    }
    const elapsed = (Date.now() - this._lastActivityTime()) / 60000;
    const remaining = this._privacySettings().timeoutMinutes - elapsed;
    return Math.max(0, Math.ceil(remaining));
  });

  constructor() {
    this.loadSettings();
    this.setupActivityTracking();
    this.startScheduleCheck();
  }

  ngOnDestroy(): void {
    this.stopTimers();
    this.removeActivityListeners();
  }

  /**
   * Initialize the display mode service.
   * Call this from app component after settings are loaded.
   */
  async initialize(): Promise<void> {
    await this.loadSettings();
    this.checkScheduledModes();
  }

  // ============================================
  // Mode Switching Methods
  // ============================================

  /**
   * Switch to normal display mode
   */
  enterNormalMode(): void {
    this._previousMode.set(this._currentMode());
    this._currentMode.set('normal');
    this._isManualPrivacy.set(false);
    this._isManualSleep.set(false);
    this.recordActivity();
    this.applyModeStyles();
  }

  /**
   * Toggle privacy mode on/off
   */
  togglePrivacyMode(): void {
    if (this._currentMode() === 'privacy') {
      this.enterNormalMode();
    } else {
      this.enterPrivacyMode(true);
    }
  }

  /**
   * Enter privacy mode
   * @param manual - Whether this is a manual switch (vs. automatic timeout)
   */
  enterPrivacyMode(manual = false): void {
    if (this._currentMode() === 'sleep') {
      // Don't switch from sleep to privacy automatically
      if (!manual) return;
    }
    this._previousMode.set(this._currentMode());
    this._currentMode.set('privacy');
    this._isManualPrivacy.set(manual);
    this.applyModeStyles();
  }

  /**
   * Toggle sleep mode on/off
   */
  toggleSleepMode(): void {
    if (this._currentMode() === 'sleep') {
      this.wakeFromSleep();
    } else {
      this.enterSleepMode(true);
    }
  }

  /**
   * Enter sleep mode
   * @param manual - Whether this is a manual switch (vs. scheduled)
   */
  enterSleepMode(manual = false): void {
    this._previousMode.set(this._currentMode());
    this._currentMode.set('sleep');
    this._isManualSleep.set(manual);
    this.applyModeStyles();
  }

  /**
   * Wake from sleep mode
   */
  wakeFromSleep(): void {
    const settings = this._sleepSettings();
    if (!settings.wakeOnTouch && !this._isManualSleep()) {
      // Only allow wake if wakeOnTouch is enabled or it was manual sleep
      return;
    }
    this.enterNormalMode();
  }

  /**
   * Handle user interaction (touch/click/key)
   */
  handleUserInteraction(): void {
    this.recordActivity();

    // If in sleep mode and wake on touch is enabled, wake up
    if (this._currentMode() === 'sleep') {
      const settings = this._sleepSettings();
      if (settings.wakeOnTouch || this._isManualSleep()) {
        this.wakeFromSleep();
      }
      return;
    }

    // If in privacy mode (automatic), return to normal
    if (this._currentMode() === 'privacy' && !this._isManualPrivacy()) {
      this.enterNormalMode();
    }
  }

  // ============================================
  // Settings Management
  // ============================================

  /**
   * Update privacy mode settings
   */
  async updatePrivacySettings(settings: Partial<PrivacyModeSettings>): Promise<void> {
    const updated = { ...this._privacySettings(), ...settings };
    this._privacySettings.set(updated);
    await this.saveSettings();
  }

  /**
   * Update sleep mode settings
   */
  async updateSleepSettings(settings: Partial<SleepModeSettings>): Promise<void> {
    const updated = { ...this._sleepSettings(), ...settings };
    this._sleepSettings.set(updated);
    await this.saveSettings();

    // Recheck schedule if settings changed
    this.checkScheduledModes();
  }

  /**
   * Load settings from storage
   */
  private async loadSettings(): Promise<void> {
    try {
      const displaySettings = await this.electronService.getSettings();

      if (displaySettings.privacyModeSettings) {
        this._privacySettings.set({
          ...DEFAULT_PRIVACY_SETTINGS,
          ...displaySettings.privacyModeSettings,
        });
      }

      if (displaySettings.sleepModeSettings) {
        this._sleepSettings.set({
          ...DEFAULT_SLEEP_SETTINGS,
          ...displaySettings.sleepModeSettings,
        });
      }
    } catch (error) {
      console.error('Failed to load display mode settings:', error);
    }
  }

  /**
   * Save settings to storage
   */
  private async saveSettings(): Promise<void> {
    try {
      const currentSettings = await this.electronService.getSettings();
      const updated: DisplaySettings = {
        ...currentSettings,
        privacyModeSettings: this._privacySettings(),
        sleepModeSettings: this._sleepSettings(),
      };
      await this.electronService.setSettings(updated);
    } catch (error) {
      console.error('Failed to save display mode settings:', error);
    }
  }

  // ============================================
  // Activity Tracking
  // ============================================

  /**
   * Record user activity timestamp
   */
  private recordActivity(): void {
    this._lastActivityTime.set(Date.now());
  }

  /**
   * Set up activity tracking for auto-privacy mode
   */
  private setupActivityTracking(): void {
    // Listen for user interactions
    document.addEventListener('touchstart', this.handleActivity, { passive: true });
    document.addEventListener('mousedown', this.handleActivity, { passive: true });
    document.addEventListener('keydown', this.handleActivity, { passive: true });

    // Check activity timeout every minute
    this.activityTimer = setInterval(() => {
      this.checkActivityTimeout();
    }, 60000);
  }

  /**
   * Remove activity listeners
   */
  private removeActivityListeners(): void {
    document.removeEventListener('touchstart', this.handleActivity);
    document.removeEventListener('mousedown', this.handleActivity);
    document.removeEventListener('keydown', this.handleActivity);
  }

  /**
   * Handle activity event
   */
  private handleActivity = (): void => {
    this.handleUserInteraction();
  };

  /**
   * Check if auto-privacy timeout has been reached
   */
  private checkActivityTimeout(): void {
    const settings = this._privacySettings();

    // Skip if privacy mode disabled or not in normal mode
    if (!settings.enabled || this._currentMode() !== 'normal') {
      return;
    }

    const elapsed = (Date.now() - this._lastActivityTime()) / 60000;
    if (elapsed >= settings.timeoutMinutes) {
      this.enterPrivacyMode(false);
    }
  }

  // ============================================
  // Scheduled Mode Management
  // ============================================

  /**
   * Start the schedule checker
   */
  private startScheduleCheck(): void {
    // Check schedule every minute
    this.scheduleTimer = setInterval(() => {
      this.checkScheduledModes();
    }, 60000);

    // Also check immediately
    this.checkScheduledModes();
  }

  /**
   * Check if we should be in scheduled sleep mode
   */
  private checkScheduledModes(): void {
    const settings = this._sleepSettings();

    if (!settings.enabled) {
      // Sleep mode disabled - exit if we're in scheduled sleep
      if (this._currentMode() === 'sleep' && !this._isManualSleep()) {
        this.enterNormalMode();
      }
      return;
    }

    const now = new Date();
    const currentMinutes = now.getHours() * 60 + now.getMinutes();
    const [startHour, startMin] = settings.startTime.split(':').map(Number);
    const [endHour, endMin] = settings.endTime.split(':').map(Number);
    const startMinutes = startHour * 60 + startMin;
    const endMinutes = endHour * 60 + endMin;

    let shouldSleep = false;

    // Handle overnight range (e.g., 22:00 to 06:00)
    if (startMinutes > endMinutes) {
      shouldSleep = currentMinutes >= startMinutes || currentMinutes < endMinutes;
    } else {
      shouldSleep = currentMinutes >= startMinutes && currentMinutes < endMinutes;
    }

    if (shouldSleep && this._currentMode() !== 'sleep') {
      this.enterSleepMode(false);
    } else if (!shouldSleep && this._currentMode() === 'sleep' && !this._isManualSleep()) {
      this.enterNormalMode();
    }
  }

  // ============================================
  // Style Application
  // ============================================

  /**
   * Apply CSS styles based on current mode
   */
  private applyModeStyles(): void {
    const mode = this._currentMode();
    const root = document.documentElement;

    // Set data attribute for CSS selectors
    root.setAttribute('data-display-mode', mode);

    // Apply sleep mode dim level
    if (mode === 'sleep') {
      const dimLevel = this._sleepSettings().dimLevel;
      root.style.setProperty('--sleep-opacity', (dimLevel / 100).toString());
      root.style.setProperty('--sleep-brightness', `${dimLevel}%`);
    } else {
      root.style.setProperty('--sleep-opacity', '1');
      root.style.setProperty('--sleep-brightness', '100%');
    }
  }

  /**
   * Stop all timers
   */
  private stopTimers(): void {
    if (this.activityTimer) {
      clearInterval(this.activityTimer);
      this.activityTimer = undefined;
    }
    if (this.scheduleTimer) {
      clearInterval(this.scheduleTimer);
      this.scheduleTimer = undefined;
    }
  }
}
