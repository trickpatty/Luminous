import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';
import {
  ElectronService,
  DisplaySettings,
  PrivacyModeSettings,
  SleepModeSettings,
  WeatherLocationSettings,
} from '../../core/services/electron.service';
import { DeviceAuthService } from '../../core/services/device-auth.service';
import { CacheService } from '../../core/services/cache.service';
import { CanvasService } from '../../core/services/canvas.service';
import { DisplayModeService } from '../../core/services/display-mode.service';

/**
 * Default privacy mode settings
 */
const DEFAULT_PRIVACY_SETTINGS: PrivacyModeSettings = {
  enabled: true,
  timeoutMinutes: 5,
  showClock: true,
  showWeather: false,
};

/**
 * Default sleep mode settings
 */
const DEFAULT_SLEEP_SETTINGS: SleepModeSettings = {
  enabled: false,
  startTime: '22:00',
  endTime: '06:00',
  dimLevel: 10,
  wakeOnTouch: true,
};

/**
 * Default weather location settings
 */
const DEFAULT_WEATHER_SETTINGS: WeatherLocationSettings = {
  useAutoLocation: true,
  temperatureUnit: 'fahrenheit',
};

/**
 * Location search result from geocoding API
 */
interface LocationSearchResult {
  name: string;
  latitude: number;
  longitude: number;
  country: string;
  region: string;
}

/**
 * Open-Meteo Geocoding API response
 */
interface GeocodingResponse {
  results?: Array<{
    id: number;
    name: string;
    latitude: number;
    longitude: number;
    country: string;
    admin1?: string;
    admin2?: string;
  }>;
}

@Component({
  selector: 'app-settings',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="display-container settings-page">
      <!-- Header -->
      <header class="settings-header">
        <button class="back-btn" (click)="goBack()">
          <svg xmlns="http://www.w3.org/2000/svg" width="32" height="32" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
            <path d="m15 18-6-6 6-6"/>
          </svg>
        </button>
        <h1 class="text-display-md">Settings</h1>
        <div class="header-spacer"></div>
      </header>

      <!-- Settings content -->
      <main class="settings-content">
        <!-- Display settings -->
        <section class="settings-section">
          <h2 class="settings-section-title">Display</h2>

          <div class="settings-card">
            <div class="setting-item">
              <div class="setting-info">
                <span class="setting-label">Time Format</span>
                <span class="setting-description">Choose 12 or 24 hour display</span>
              </div>
              <select
                class="setting-select"
                [ngModel]="settings().timeFormat"
                (ngModelChange)="updateSetting('timeFormat', $event)"
              >
                <option value="12h">12 Hour</option>
                <option value="24h">24 Hour</option>
              </select>
            </div>

            <div class="setting-item">
              <div class="setting-info">
                <span class="setting-label">Night Display</span>
                <span class="setting-description">Dark theme during night hours</span>
              </div>
              <button
                class="toggle"
                [attr.data-checked]="settings().nightDisplayEnabled"
                (click)="toggleNightDisplay()"
              >
                <div class="toggle-knob"></div>
              </button>
            </div>

            @if (settings().nightDisplayEnabled) {
              <div class="setting-item">
                <div class="setting-info">
                  <span class="setting-label">Night Hours</span>
                </div>
                <div class="time-range">
                  <input
                    type="time"
                    class="time-input"
                    [value]="settings().nightDisplayStart"
                    (change)="updateSetting('nightDisplayStart', $any($event.target).value)"
                  />
                  <span>to</span>
                  <input
                    type="time"
                    class="time-input"
                    [value]="settings().nightDisplayEnd"
                    (change)="updateSetting('nightDisplayEnd', $any($event.target).value)"
                  />
                </div>
              </div>
            }

            <div class="setting-item">
              <div class="setting-info">
                <span class="setting-label">Default View</span>
                <span class="setting-description">View to show on startup</span>
              </div>
              <select
                class="setting-select"
                [ngModel]="settings().defaultView"
                (ngModelChange)="updateSetting('defaultView', $event)"
              >
                <option value="schedule">Schedule</option>
                <option value="tasks">Tasks</option>
                <option value="routines">Routines</option>
              </select>
            </div>
          </div>
        </section>

        <!-- Weather Location Settings -->
        <section class="settings-section">
          <h2 class="settings-section-title">Weather Location</h2>

          <div class="settings-card">
            <div class="setting-item">
              <div class="setting-info">
                <span class="setting-label">Auto-Detect Location</span>
                <span class="setting-description">Use device location for weather</span>
              </div>
              <button
                class="toggle"
                [attr.data-checked]="weatherSettings().useAutoLocation"
                (click)="toggleAutoLocation()"
              >
                <div class="toggle-knob"></div>
              </button>
            </div>

            @if (!weatherSettings().useAutoLocation) {
              <div class="setting-item">
                <div class="setting-info">
                  <span class="setting-label">Location</span>
                  <span class="setting-description">Enter city name to search</span>
                </div>
                <div class="location-input-container">
                  <input
                    type="text"
                    class="location-input"
                    placeholder="Enter city name..."
                    [value]="locationSearchQuery"
                    (input)="onLocationSearchInput($any($event.target).value)"
                    (keyup.enter)="searchLocation()"
                  />
                  <button class="search-btn" (click)="searchLocation()" [disabled]="isSearchingLocation()">
                    @if (isSearchingLocation()) {
                      <div class="search-spinner"></div>
                    } @else {
                      <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                        <circle cx="11" cy="11" r="8"/>
                        <path d="m21 21-4.3-4.3"/>
                      </svg>
                    }
                  </button>
                </div>
              </div>

              @if (locationSearchResults().length > 0) {
                <div class="setting-item location-results">
                  <div class="setting-info full-width">
                    <span class="setting-label">Search Results</span>
                    <div class="location-list">
                      @for (result of locationSearchResults(); track result.name + result.latitude) {
                        <button
                          class="location-result-item"
                          (click)="selectLocation(result)"
                        >
                          <span class="location-name">{{ result.name }}</span>
                          <span class="location-region">{{ result.region }}, {{ result.country }}</span>
                        </button>
                      }
                    </div>
                  </div>
                </div>
              }

              @if (weatherSettings().manualLocationName) {
                <div class="setting-item">
                  <div class="setting-info">
                    <span class="setting-label">Current Location</span>
                  </div>
                  <span class="setting-value">{{ weatherSettings().manualLocationName }}</span>
                </div>
              }
            }

            <div class="setting-item">
              <div class="setting-info">
                <span class="setting-label">Temperature Unit</span>
              </div>
              <select
                class="setting-select"
                [ngModel]="weatherSettings().temperatureUnit"
                (ngModelChange)="updateWeatherSetting('temperatureUnit', $event)"
              >
                <option value="fahrenheit">Fahrenheit</option>
                <option value="celsius">Celsius</option>
              </select>
            </div>

            @if (locationError()) {
              <div class="setting-item error-item">
                <span class="error-text">{{ locationError() }}</span>
              </div>
            }
          </div>
        </section>

        <!-- Privacy Mode Settings -->
        <section class="settings-section">
          <h2 class="settings-section-title">Privacy Mode</h2>

          <div class="settings-card">
            <div class="setting-item">
              <div class="setting-info">
                <span class="setting-label">Enable Privacy Mode</span>
                <span class="setting-description">Show clock/wallpaper after inactivity</span>
              </div>
              <button
                class="toggle"
                [attr.data-checked]="privacySettings().enabled"
                (click)="togglePrivacyModeEnabled()"
              >
                <div class="toggle-knob"></div>
              </button>
            </div>

            @if (privacySettings().enabled) {
              <div class="setting-item">
                <div class="setting-info">
                  <span class="setting-label">Auto-Privacy Timeout</span>
                  <span class="setting-description">Switch to privacy mode after inactivity</span>
                </div>
                <select
                  class="setting-select"
                  [ngModel]="privacySettings().timeoutMinutes"
                  (ngModelChange)="updatePrivacySetting('timeoutMinutes', $event)"
                >
                  <option [ngValue]="1">1 minute</option>
                  <option [ngValue]="2">2 minutes</option>
                  <option [ngValue]="5">5 minutes</option>
                  <option [ngValue]="10">10 minutes</option>
                  <option [ngValue]="15">15 minutes</option>
                  <option [ngValue]="30">30 minutes</option>
                </select>
              </div>

              <div class="setting-item">
                <div class="setting-info">
                  <span class="setting-label">Show Clock</span>
                  <span class="setting-description">Display time in privacy mode</span>
                </div>
                <button
                  class="toggle"
                  [attr.data-checked]="privacySettings().showClock"
                  (click)="togglePrivacyShowClock()"
                >
                  <div class="toggle-knob"></div>
                </button>
              </div>

              <div class="setting-item">
                <div class="setting-info">
                  <span class="setting-label">Show Weather</span>
                  <span class="setting-description">Display weather in privacy mode</span>
                </div>
                <button
                  class="toggle"
                  [attr.data-checked]="privacySettings().showWeather"
                  (click)="togglePrivacyShowWeather()"
                >
                  <div class="toggle-knob"></div>
                </button>
              </div>
            }

            <!-- Quick action to toggle privacy mode now -->
            <div class="setting-item">
              <div class="setting-info">
                <span class="setting-label">Privacy Mode</span>
                <span class="setting-description">{{ isPrivacyMode() ? 'Currently active' : 'Activate now' }}</span>
              </div>
              <button
                class="setting-action-btn"
                [class.active]="isPrivacyMode()"
                (click)="togglePrivacyModeNow()"
              >
                {{ isPrivacyMode() ? 'Exit' : 'Activate' }}
              </button>
            </div>
          </div>
        </section>

        <!-- Sleep Mode Settings -->
        <section class="settings-section">
          <h2 class="settings-section-title">Sleep Mode</h2>

          <div class="settings-card">
            <div class="setting-item">
              <div class="setting-info">
                <span class="setting-label">Scheduled Sleep</span>
                <span class="setting-description">Dim display during scheduled hours</span>
              </div>
              <button
                class="toggle"
                [attr.data-checked]="sleepSettings().enabled"
                (click)="toggleSleepModeEnabled()"
              >
                <div class="toggle-knob"></div>
              </button>
            </div>

            @if (sleepSettings().enabled) {
              <div class="setting-item">
                <div class="setting-info">
                  <span class="setting-label">Sleep Hours</span>
                </div>
                <div class="time-range">
                  <input
                    type="time"
                    class="time-input"
                    [value]="sleepSettings().startTime"
                    (change)="updateSleepSetting('startTime', $any($event.target).value)"
                  />
                  <span>to</span>
                  <input
                    type="time"
                    class="time-input"
                    [value]="sleepSettings().endTime"
                    (change)="updateSleepSetting('endTime', $any($event.target).value)"
                  />
                </div>
              </div>

              <div class="setting-item">
                <div class="setting-info">
                  <span class="setting-label">Dim Level</span>
                  <span class="setting-description">{{ sleepSettings().dimLevel }}% brightness</span>
                </div>
                <input
                  type="range"
                  class="dim-slider"
                  min="0"
                  max="50"
                  step="5"
                  [value]="sleepSettings().dimLevel"
                  (input)="updateSleepSetting('dimLevel', +$any($event.target).value)"
                />
              </div>

              <div class="setting-item">
                <div class="setting-info">
                  <span class="setting-label">Wake on Touch</span>
                  <span class="setting-description">Tap screen to exit sleep mode</span>
                </div>
                <button
                  class="toggle"
                  [attr.data-checked]="sleepSettings().wakeOnTouch"
                  (click)="toggleSleepWakeOnTouch()"
                >
                  <div class="toggle-knob"></div>
                </button>
              </div>
            }

            <!-- Quick action to toggle sleep mode now -->
            <div class="setting-item">
              <div class="setting-info">
                <span class="setting-label">Sleep Mode</span>
                <span class="setting-description">{{ isSleepMode() ? 'Currently active' : 'Activate now' }}</span>
              </div>
              <button
                class="setting-action-btn"
                [class.active]="isSleepMode()"
                (click)="toggleSleepModeNow()"
              >
                {{ isSleepMode() ? 'Wake' : 'Sleep' }}
              </button>
            </div>
          </div>
        </section>

        <!-- Device info -->
        <section class="settings-section">
          <h2 class="settings-section-title">Device</h2>

          <div class="settings-card">
            <div class="setting-item">
              <div class="setting-info">
                <span class="setting-label">Family</span>
              </div>
              <span class="setting-value">{{ familyName() }}</span>
            </div>

            <div class="setting-item">
              <div class="setting-info">
                <span class="setting-label">Cache Status</span>
              </div>
              <button class="setting-action-btn" (click)="clearCache()">
                Clear Cache
              </button>
            </div>

            <div class="setting-item">
              <div class="setting-info">
                <span class="setting-label">Reload Display</span>
              </div>
              <button class="setting-action-btn" (click)="reloadDisplay()">
                Reload
              </button>
            </div>
          </div>
        </section>

        <!-- Danger zone -->
        <section class="settings-section">
          <h2 class="settings-section-title danger-title">Danger Zone</h2>

          <div class="settings-card danger-card">
            <div class="setting-item">
              <div class="setting-info">
                <span class="setting-label">Unlink Device</span>
                <span class="setting-description">Disconnect this display from your family</span>
              </div>
              <button class="setting-danger-btn" (click)="confirmUnlink()">
                Unlink
              </button>
            </div>
          </div>
        </section>

        <!-- Version info -->
        <div class="version-info">
          @if (appInfo()) {
            <p>Luminous Display v{{ appInfo()!.version }}</p>
          }
          <p>{{ isKiosk() ? 'Kiosk Mode' : 'Development Mode' }}</p>
        </div>
      </main>

      <!-- Unlink confirmation dialog -->
      @if (showUnlinkConfirm) {
        <div class="confirm-overlay" (click)="showUnlinkConfirm = false">
          <div class="confirm-dialog z-modal" (click)="$event.stopPropagation()">
            <h3 class="text-display-sm">Unlink Device?</h3>
            <p class="text-body-lg">
              This will disconnect this display from your family account.
              You'll need to link it again to use it.
            </p>
            <div class="confirm-actions">
              <button class="display-btn display-btn-secondary" (click)="showUnlinkConfirm = false">
                Cancel
              </button>
              <button class="display-btn danger-btn" (click)="unlink()">
                Unlink Device
              </button>
            </div>
          </div>
        </div>
      }
    </div>
  `,
  styles: [`
    .settings-page {
      height: 100vh;
      overflow-y: auto;
      display: flex;
      flex-direction: column;
    }

    .settings-header {
      display: flex;
      align-items: center;
      gap: var(--space-4);
      padding: var(--space-4) 0;
    }

    .back-btn {
      width: var(--touch-lg);
      height: var(--touch-lg);
      display: flex;
      align-items: center;
      justify-content: center;
      background: var(--surface-secondary);
      border: none;
      border-radius: var(--radius-xl);
      cursor: pointer;
    }

    .back-btn:active {
      background: var(--surface-pressed);
    }

    .header-spacer {
      width: var(--touch-lg);
    }

    .settings-content {
      flex: 1;
      overflow-y: auto;
      padding-bottom: var(--space-12);
    }

    .settings-section {
      margin-bottom: var(--space-8);
    }

    .settings-section-title {
      font-size: 1.25rem;
      font-weight: 600;
      color: var(--text-secondary);
      text-transform: uppercase;
      letter-spacing: 0.05em;
      margin-bottom: var(--space-4);
      padding-left: var(--space-4);
    }

    .danger-title {
      color: var(--danger);
    }

    .settings-card {
      background: var(--surface-primary);
      border-radius: var(--radius-xl);
      box-shadow: var(--shadow-sm);
      overflow: hidden;
    }

    .danger-card {
      border: 1px solid var(--danger-light);
    }

    .setting-item {
      display: flex;
      align-items: center;
      justify-content: space-between;
      padding: var(--space-5) var(--space-6);
      border-bottom: 1px solid var(--border-color-light);
    }

    .setting-item:last-child {
      border-bottom: none;
    }

    .setting-info {
      display: flex;
      flex-direction: column;
      gap: var(--space-1);
    }

    .setting-label {
      font-size: 1.125rem;
      font-weight: 500;
      color: var(--text-primary);
    }

    .setting-description {
      font-size: 0.875rem;
      color: var(--text-secondary);
    }

    .setting-value {
      font-size: 1rem;
      color: var(--text-secondary);
    }

    .setting-select {
      height: 44px;
      padding: 0 var(--space-4);
      font-size: 1rem;
      border: 1px solid var(--border-color);
      border-radius: var(--radius-md);
      background: var(--surface-primary);
    }

    .time-range {
      display: flex;
      align-items: center;
      gap: var(--space-3);
    }

    .time-input {
      height: 44px;
      padding: 0 var(--space-3);
      font-size: 1rem;
      border: 1px solid var(--border-color);
      border-radius: var(--radius-md);
      background: var(--surface-primary);
    }

    .toggle {
      position: relative;
      width: 52px;
      height: 32px;
      /* Override global button min-height/min-width */
      min-width: 52px;
      min-height: 32px;
      background: var(--border-color-strong);
      border-radius: var(--radius-full);
      border: none;
      padding: 4px;
      cursor: pointer;
      transition: background var(--duration-standard) var(--ease-in-out);
      /* Ensure the toggle maintains its pill shape */
      flex-shrink: 0;
    }

    .toggle[data-checked="true"] {
      background: var(--accent-600);
    }

    .toggle-knob {
      display: block;
      width: 24px;
      height: 24px;
      background: white;
      border-radius: var(--radius-full);
      box-shadow: var(--shadow-sm);
      transition: transform var(--duration-standard) var(--ease-spring);
    }

    .toggle[data-checked="true"] .toggle-knob {
      transform: translateX(20px);
    }

    .setting-action-btn {
      height: 40px;
      padding: 0 var(--space-5);
      font-size: 0.875rem;
      font-weight: 500;
      background: var(--surface-secondary);
      border: none;
      border-radius: var(--radius-md);
      cursor: pointer;
      transition: all var(--duration-quick) var(--ease-out);
    }

    .setting-action-btn:active {
      background: var(--surface-pressed);
    }

    .setting-action-btn.active {
      background: var(--accent-100);
      color: var(--accent-700);
    }

    .setting-danger-btn {
      height: 40px;
      padding: 0 var(--space-5);
      font-size: 0.875rem;
      font-weight: 500;
      background: var(--danger-light);
      color: var(--danger-dark);
      border: none;
      border-radius: var(--radius-md);
      cursor: pointer;
    }

    .setting-danger-btn:active {
      background: var(--danger);
      color: white;
    }

    .dim-slider {
      width: 120px;
      height: 8px;
      -webkit-appearance: none;
      appearance: none;
      background: var(--surface-secondary);
      border-radius: var(--radius-full);
      outline: none;
    }

    .dim-slider::-webkit-slider-thumb {
      -webkit-appearance: none;
      appearance: none;
      width: 24px;
      height: 24px;
      background: var(--accent-600);
      border-radius: var(--radius-full);
      cursor: pointer;
      box-shadow: var(--shadow-sm);
    }

    .dim-slider::-moz-range-thumb {
      width: 24px;
      height: 24px;
      background: var(--accent-600);
      border-radius: var(--radius-full);
      cursor: pointer;
      border: none;
      box-shadow: var(--shadow-sm);
    }

    .version-info {
      text-align: center;
      color: var(--text-tertiary);
      font-size: 0.875rem;
      margin-top: var(--space-8);
    }

    .confirm-overlay {
      position: fixed;
      inset: 0;
      background: rgba(0, 0, 0, 0.5);
      backdrop-filter: blur(4px);
      display: flex;
      align-items: center;
      justify-content: center;
      z-index: var(--z-modal);
    }

    .confirm-dialog {
      background: var(--surface-primary);
      border-radius: var(--radius-2xl);
      padding: var(--space-8);
      max-width: 400px;
      text-align: center;
    }

    .confirm-dialog h3 {
      margin-bottom: var(--space-3);
    }

    .confirm-dialog p {
      color: var(--text-secondary);
      margin-bottom: var(--space-6);
    }

    .confirm-actions {
      display: flex;
      gap: var(--space-4);
      justify-content: center;
    }

    .danger-btn {
      background: var(--danger) !important;
      color: white !important;
    }

    /* Weather Location Styles */
    .location-input-container {
      display: flex;
      gap: var(--space-2);
    }

    .location-input {
      flex: 1;
      height: 44px;
      padding: 0 var(--space-3);
      font-size: 1rem;
      border: 1px solid var(--border-color);
      border-radius: var(--radius-md);
      background: var(--surface-primary);
      min-width: 180px;
    }

    .location-input:focus {
      outline: none;
      border-color: var(--accent-500);
    }

    .search-btn {
      width: 44px;
      height: 44px;
      display: flex;
      align-items: center;
      justify-content: center;
      background: var(--accent-500);
      color: white;
      border: none;
      border-radius: var(--radius-md);
      cursor: pointer;
      transition: all var(--duration-quick) var(--ease-out);
    }

    .search-btn:hover:not(:disabled) {
      background: var(--accent-600);
    }

    .search-btn:active:not(:disabled) {
      transform: scale(0.95);
    }

    .search-btn:disabled {
      opacity: 0.7;
      cursor: not-allowed;
    }

    .search-spinner {
      width: 16px;
      height: 16px;
      border: 2px solid rgba(255, 255, 255, 0.3);
      border-top-color: white;
      border-radius: 50%;
      animation: spin 800ms linear infinite;
    }

    .location-results {
      flex-direction: column;
      align-items: stretch !important;
    }

    .location-results .setting-info.full-width {
      width: 100%;
    }

    .location-list {
      display: flex;
      flex-direction: column;
      gap: var(--space-2);
      margin-top: var(--space-3);
    }

    .location-result-item {
      display: flex;
      flex-direction: column;
      align-items: flex-start;
      padding: var(--space-3);
      background: var(--surface-secondary);
      border: 1px solid var(--border-color-light);
      border-radius: var(--radius-md);
      cursor: pointer;
      transition: all var(--duration-quick) var(--ease-out);
    }

    .location-result-item:hover {
      background: var(--surface-interactive);
      border-color: var(--accent-300);
    }

    .location-result-item:active {
      background: var(--surface-pressed);
    }

    .location-name {
      font-size: 1rem;
      font-weight: 500;
      color: var(--text-primary);
    }

    .location-region {
      font-size: 0.875rem;
      color: var(--text-secondary);
    }

    .error-item {
      justify-content: center !important;
    }

    .error-text {
      color: var(--danger);
      font-size: 0.875rem;
    }
  `],
})
export class SettingsComponent implements OnInit {
  private readonly router = inject(Router);
  private readonly http = inject(HttpClient);
  private readonly electronService = inject(ElectronService);
  private readonly authService = inject(DeviceAuthService);
  private readonly cacheService = inject(CacheService);
  private readonly canvasService = inject(CanvasService);
  private readonly displayModeService = inject(DisplayModeService);

  protected readonly settings = signal<DisplaySettings>({
    timeFormat: '12h',
    nightDisplayEnabled: false,
    nightDisplayStart: '21:00',
    nightDisplayEnd: '06:00',
    defaultView: 'schedule',
  });

  protected readonly privacySettings = signal<PrivacyModeSettings>(DEFAULT_PRIVACY_SETTINGS);
  protected readonly sleepSettings = signal<SleepModeSettings>(DEFAULT_SLEEP_SETTINGS);
  protected readonly weatherSettings = signal<WeatherLocationSettings>(DEFAULT_WEATHER_SETTINGS);
  protected readonly isSearchingLocation = signal(false);
  protected readonly locationSearchResults = signal<LocationSearchResult[]>([]);
  protected readonly locationError = signal<string | null>(null);
  protected locationSearchQuery = '';

  protected readonly appInfo = this.electronService.appInfo;
  protected readonly familyName = this.authService.familyName;

  // Display mode status from service
  protected readonly isPrivacyMode = this.displayModeService.isPrivacyMode;
  protected readonly isSleepMode = this.displayModeService.isSleepMode;

  protected showUnlinkConfirm = false;

  isKiosk(): boolean {
    return this.appInfo()?.isKiosk ?? false;
  }

  async ngOnInit(): Promise<void> {
    const savedSettings = await this.electronService.getSettings();
    this.settings.set({
      ...this.settings(),
      ...savedSettings,
    });

    // Load privacy and sleep settings
    if (savedSettings.privacyModeSettings) {
      this.privacySettings.set({
        ...DEFAULT_PRIVACY_SETTINGS,
        ...savedSettings.privacyModeSettings,
      });
    }
    if (savedSettings.sleepModeSettings) {
      this.sleepSettings.set({
        ...DEFAULT_SLEEP_SETTINGS,
        ...savedSettings.sleepModeSettings,
      });
    }
    // Load weather location settings
    if (savedSettings.weatherLocationSettings) {
      this.weatherSettings.set({
        ...DEFAULT_WEATHER_SETTINGS,
        ...savedSettings.weatherLocationSettings,
      });
    }
  }

  goBack(): void {
    this.router.navigate(['/display']);
  }

  // ============================================
  // Display Settings
  // ============================================

  async updateSetting<K extends keyof DisplaySettings>(
    key: K,
    value: DisplaySettings[K]
  ): Promise<void> {
    const updated = { ...this.settings(), [key]: value };
    this.settings.set(updated);
    await this.electronService.setSettings(updated);

    // Apply night display setting immediately
    if (key === 'nightDisplayEnabled' || key === 'nightDisplayStart' || key === 'nightDisplayEnd') {
      const startHour = parseInt(updated.nightDisplayStart?.split(':')[0] || '21', 10);
      const endHour = parseInt(updated.nightDisplayEnd?.split(':')[0] || '6', 10);
      this.canvasService.setNightDisplay(updated.nightDisplayEnabled ?? false, startHour, endHour);
    }
  }

  toggleNightDisplay(): void {
    const current = this.settings().nightDisplayEnabled ?? false;
    this.updateSetting('nightDisplayEnabled', !current);
  }

  // ============================================
  // Privacy Mode Settings
  // ============================================

  async updatePrivacySetting<K extends keyof PrivacyModeSettings>(
    key: K,
    value: PrivacyModeSettings[K]
  ): Promise<void> {
    const updated = { ...this.privacySettings(), [key]: value };
    this.privacySettings.set(updated);
    await this.displayModeService.updatePrivacySettings({ [key]: value });
  }

  togglePrivacyModeEnabled(): void {
    const current = this.privacySettings().enabled;
    this.updatePrivacySetting('enabled', !current);
  }

  togglePrivacyShowClock(): void {
    const current = this.privacySettings().showClock;
    this.updatePrivacySetting('showClock', !current);
  }

  togglePrivacyShowWeather(): void {
    const current = this.privacySettings().showWeather;
    this.updatePrivacySetting('showWeather', !current);
  }

  togglePrivacyModeNow(): void {
    this.displayModeService.togglePrivacyMode();
  }

  // ============================================
  // Sleep Mode Settings
  // ============================================

  async updateSleepSetting<K extends keyof SleepModeSettings>(
    key: K,
    value: SleepModeSettings[K]
  ): Promise<void> {
    const updated = { ...this.sleepSettings(), [key]: value };
    this.sleepSettings.set(updated);
    await this.displayModeService.updateSleepSettings({ [key]: value });
  }

  toggleSleepModeEnabled(): void {
    const current = this.sleepSettings().enabled;
    this.updateSleepSetting('enabled', !current);
  }

  toggleSleepWakeOnTouch(): void {
    const current = this.sleepSettings().wakeOnTouch;
    this.updateSleepSetting('wakeOnTouch', !current);
  }

  toggleSleepModeNow(): void {
    this.displayModeService.toggleSleepMode();
  }

  // ============================================
  // Weather Location Settings
  // ============================================

  async updateWeatherSetting<K extends keyof WeatherLocationSettings>(
    key: K,
    value: WeatherLocationSettings[K]
  ): Promise<void> {
    const updated = { ...this.weatherSettings(), [key]: value };
    this.weatherSettings.set(updated);
    await this.saveWeatherSettings(updated);
  }

  toggleAutoLocation(): void {
    const current = this.weatherSettings().useAutoLocation;
    this.updateWeatherSetting('useAutoLocation', !current);
    // Clear search results when toggling
    this.locationSearchResults.set([]);
    this.locationError.set(null);
  }

  onLocationSearchInput(value: string): void {
    this.locationSearchQuery = value;
    // Clear error when typing
    this.locationError.set(null);
  }

  async searchLocation(): Promise<void> {
    const query = this.locationSearchQuery.trim();
    if (!query) {
      this.locationError.set('Please enter a city name');
      return;
    }

    this.isSearchingLocation.set(true);
    this.locationError.set(null);
    this.locationSearchResults.set([]);

    try {
      const response = await firstValueFrom(
        this.http.get<GeocodingResponse>(
          `https://geocoding-api.open-meteo.com/v1/search?name=${encodeURIComponent(query)}&count=5&language=en&format=json`
        )
      );

      if (response.results && response.results.length > 0) {
        this.locationSearchResults.set(
          response.results.map(r => ({
            name: r.name,
            latitude: r.latitude,
            longitude: r.longitude,
            country: r.country,
            region: r.admin1 || r.admin2 || '',
          }))
        );
      } else {
        this.locationError.set('No locations found. Try a different search term.');
      }
    } catch (error) {
      console.error('Location search failed:', error);
      this.locationError.set('Failed to search for location. Please try again.');
    } finally {
      this.isSearchingLocation.set(false);
    }
  }

  async selectLocation(result: LocationSearchResult): Promise<void> {
    const locationName = result.region
      ? `${result.name}, ${result.region}`
      : `${result.name}, ${result.country}`;

    const updated: WeatherLocationSettings = {
      ...this.weatherSettings(),
      manualLocationName: locationName,
      manualLatitude: result.latitude,
      manualLongitude: result.longitude,
    };

    this.weatherSettings.set(updated);
    await this.saveWeatherSettings(updated);

    // Clear search results
    this.locationSearchResults.set([]);
    this.locationSearchQuery = '';
  }

  private async saveWeatherSettings(settings: WeatherLocationSettings): Promise<void> {
    const currentSettings = await this.electronService.getSettings();
    await this.electronService.setSettings({
      ...currentSettings,
      weatherLocationSettings: settings,
    });
  }

  // ============================================
  // Device Actions
  // ============================================

  async clearCache(): Promise<void> {
    await this.cacheService.clearAll();
    // Show feedback
  }

  async reloadDisplay(): Promise<void> {
    await this.electronService.reloadWindow();
  }

  confirmUnlink(): void {
    this.showUnlinkConfirm = true;
  }

  async unlink(): Promise<void> {
    this.showUnlinkConfirm = false;
    await this.authService.unlink();
  }
}
