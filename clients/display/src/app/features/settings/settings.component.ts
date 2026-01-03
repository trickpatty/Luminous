import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { ElectronService, DisplaySettings } from '../../core/services/electron.service';
import { DeviceAuthService } from '../../core/services/device-auth.service';
import { CacheService } from '../../core/services/cache.service';
import { CanvasService } from '../../core/services/canvas.service';

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
          <div class="confirm-dialog" (click)="$event.stopPropagation()">
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
      background: var(--border-color-strong);
      border-radius: var(--radius-full);
      border: none;
      padding: 4px;
      cursor: pointer;
      transition: background var(--duration-standard) var(--ease-in-out);
    }

    .toggle[data-checked="true"] {
      background: var(--accent-600);
    }

    .toggle-knob {
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
    }

    .setting-action-btn:active {
      background: var(--surface-pressed);
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
  `],
})
export class SettingsComponent implements OnInit {
  private readonly router = inject(Router);
  private readonly electronService = inject(ElectronService);
  private readonly authService = inject(DeviceAuthService);
  private readonly cacheService = inject(CacheService);
  private readonly canvasService = inject(CanvasService);

  protected readonly settings = signal<DisplaySettings>({
    timeFormat: '12h',
    nightDisplayEnabled: false,
    nightDisplayStart: '21:00',
    nightDisplayEnd: '06:00',
    defaultView: 'schedule',
  });

  protected readonly appInfo = this.electronService.appInfo;
  protected readonly familyName = this.authService.familyName;

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
  }

  goBack(): void {
    this.router.navigate(['/display']);
  }

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
