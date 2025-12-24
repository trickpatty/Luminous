import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AuthService, FamilyService } from '../../../../core';
import {
  CardComponent,
  ButtonComponent,
  AlertComponent,
  SpinnerComponent,
} from '../../../../shared';
import { FamilySettings, UpdateFamilySettingsRequest } from '../../../../models';

@Component({
  selector: 'app-settings',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    CardComponent,
    ButtonComponent,
    AlertComponent,
    SpinnerComponent,
  ],
  template: `
    <div class="p-4 sm:p-6 lg:p-8 max-w-4xl mx-auto">
      <div class="mb-8">
        <h2 class="text-2xl font-semibold text-gray-900">Family Settings</h2>
        <p class="mt-1 text-gray-600">
          Manage your family's preferences and display settings.
        </p>
      </div>

      @if (loading()) {
        <div class="flex justify-center py-12">
          <app-spinner size="lg" label="Loading settings..." />
        </div>
      } @else if (family()) {
        @if (successMessage()) {
          <app-alert variant="success" [dismissible]="true" (dismiss)="successMessage.set(null)" class="mb-6">
            {{ successMessage() }}
          </app-alert>
        }

        @if (error()) {
          <app-alert variant="error" [dismissible]="true" (dismiss)="error.set(null)" class="mb-6">
            {{ error() }}
          </app-alert>
        }

        <div class="space-y-6">
          <!-- General Settings -->
          <app-card title="General" subtitle="Basic family information">
            <div class="space-y-4">
              <div>
                <label class="block text-sm font-medium text-gray-700 mb-1">
                  Family Name
                </label>
                <input
                  type="text"
                  [(ngModel)]="familyName"
                  class="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-primary-500"
                  placeholder="Enter family name"
                />
              </div>

              <div>
                <label class="block text-sm font-medium text-gray-700 mb-1">
                  Timezone
                </label>
                <select
                  [(ngModel)]="timezone"
                  class="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-primary-500"
                >
                  @for (tz of timezones; track tz) {
                    <option [value]="tz">{{ tz }}</option>
                  }
                </select>
              </div>
            </div>
          </app-card>

          <!-- Display Settings -->
          <app-card title="Display Settings" subtitle="Configure how information is shown">
            <div class="space-y-4">
              <div>
                <label class="block text-sm font-medium text-gray-700 mb-1">
                  Default Calendar View
                </label>
                <select
                  [(ngModel)]="defaultView"
                  class="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-primary-500"
                >
                  <option value="day">Day View</option>
                  <option value="week">Week View</option>
                  <option value="month">Month View</option>
                  <option value="agenda">Agenda View</option>
                </select>
              </div>

              <div>
                <label class="block text-sm font-medium text-gray-700 mb-1">
                  Week Starts On
                </label>
                <select
                  [(ngModel)]="weekStartDay"
                  class="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-primary-500"
                >
                  <option [value]="0">Sunday</option>
                  <option [value]="1">Monday</option>
                  <option [value]="6">Saturday</option>
                </select>
              </div>

              <div class="flex items-center justify-between py-2">
                <div>
                  <p class="text-sm font-medium text-gray-700">Temperature Unit</p>
                  <p class="text-xs text-gray-500">Display temperature in Celsius or Fahrenheit</p>
                </div>
                <div class="flex items-center gap-2">
                  <button
                    type="button"
                    (click)="useCelsius = false"
                    [class]="!useCelsius ? 'bg-primary-100 text-primary-700 border-primary-300' : 'bg-white text-gray-700 border-gray-300'"
                    class="px-3 py-1.5 text-sm font-medium border rounded-l-lg"
                  >
                    °F
                  </button>
                  <button
                    type="button"
                    (click)="useCelsius = true"
                    [class]="useCelsius ? 'bg-primary-100 text-primary-700 border-primary-300' : 'bg-white text-gray-700 border-gray-300'"
                    class="px-3 py-1.5 text-sm font-medium border rounded-r-lg -ml-px"
                  >
                    °C
                  </button>
                </div>
              </div>
            </div>
          </app-card>

          <!-- Privacy Settings -->
          <app-card title="Privacy Mode" subtitle="Protect your family's information when not in use">
            <div class="space-y-4">
              <div class="flex items-center justify-between py-2">
                <div>
                  <p class="text-sm font-medium text-gray-700">Enable Privacy Mode</p>
                  <p class="text-xs text-gray-500">Hide schedule when display is idle</p>
                </div>
                <button
                  type="button"
                  (click)="privacyModeEnabled = !privacyModeEnabled"
                  [class]="privacyModeEnabled ? 'bg-primary-600' : 'bg-gray-200'"
                  class="relative inline-flex h-6 w-11 flex-shrink-0 cursor-pointer rounded-full border-2 border-transparent transition-colors duration-200 ease-in-out focus:outline-none focus:ring-2 focus:ring-primary-500 focus:ring-offset-2"
                >
                  <span
                    [class]="privacyModeEnabled ? 'translate-x-5' : 'translate-x-0'"
                    class="pointer-events-none inline-block h-5 w-5 transform rounded-full bg-white shadow ring-0 transition duration-200 ease-in-out"
                  ></span>
                </button>
              </div>

              @if (privacyModeEnabled) {
                <div>
                  <label class="block text-sm font-medium text-gray-700 mb-1">
                    Privacy Mode Timeout (minutes)
                  </label>
                  <input
                    type="number"
                    [(ngModel)]="privacyModeTimeout"
                    min="1"
                    max="60"
                    class="w-32 px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-primary-500"
                  />
                </div>
              }
            </div>
          </app-card>

          <!-- Sleep Mode Settings -->
          <app-card title="Sleep Mode" subtitle="Automatically turn off display during certain hours">
            <div class="space-y-4">
              <div class="flex items-center justify-between py-2">
                <div>
                  <p class="text-sm font-medium text-gray-700">Enable Sleep Mode</p>
                  <p class="text-xs text-gray-500">Turn off display during scheduled times</p>
                </div>
                <button
                  type="button"
                  (click)="sleepModeEnabled = !sleepModeEnabled"
                  [class]="sleepModeEnabled ? 'bg-primary-600' : 'bg-gray-200'"
                  class="relative inline-flex h-6 w-11 flex-shrink-0 cursor-pointer rounded-full border-2 border-transparent transition-colors duration-200 ease-in-out focus:outline-none focus:ring-2 focus:ring-primary-500 focus:ring-offset-2"
                >
                  <span
                    [class]="sleepModeEnabled ? 'translate-x-5' : 'translate-x-0'"
                    class="pointer-events-none inline-block h-5 w-5 transform rounded-full bg-white shadow ring-0 transition duration-200 ease-in-out"
                  ></span>
                </button>
              </div>

              @if (sleepModeEnabled) {
                <div class="grid grid-cols-2 gap-4">
                  <div>
                    <label class="block text-sm font-medium text-gray-700 mb-1">
                      Start Time
                    </label>
                    <input
                      type="time"
                      [(ngModel)]="sleepStartTime"
                      class="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-primary-500"
                    />
                  </div>
                  <div>
                    <label class="block text-sm font-medium text-gray-700 mb-1">
                      End Time
                    </label>
                    <input
                      type="time"
                      [(ngModel)]="sleepEndTime"
                      class="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-primary-500"
                    />
                  </div>
                </div>
              }
            </div>
          </app-card>

          <!-- Save Button -->
          <div class="flex justify-end gap-3 pt-4">
            <app-button variant="secondary" (onClick)="resetSettings()">
              Reset
            </app-button>
            <app-button
              variant="primary"
              [loading]="saving()"
              (onClick)="saveSettings()"
            >
              Save Changes
            </app-button>
          </div>
        </div>
      }
    </div>
  `,
})
export class SettingsComponent implements OnInit {
  private readonly authService = inject(AuthService);
  private readonly familyService = inject(FamilyService);

  // State
  family = this.familyService.family;
  loading = this.familyService.loading;
  saving = signal(false);
  error = signal<string | null>(null);
  successMessage = signal<string | null>(null);

  // Form fields
  familyName = '';
  timezone = 'UTC';
  defaultView: 'day' | 'week' | 'month' | 'agenda' = 'day';
  weekStartDay = 0;
  useCelsius = false;
  privacyModeEnabled = true;
  privacyModeTimeout = 5;
  sleepModeEnabled = false;
  sleepStartTime = '22:00';
  sleepEndTime = '06:00';

  timezones = [
    'UTC',
    'America/New_York',
    'America/Chicago',
    'America/Denver',
    'America/Los_Angeles',
    'America/Phoenix',
    'America/Anchorage',
    'Pacific/Honolulu',
    'Europe/London',
    'Europe/Paris',
    'Europe/Berlin',
    'Asia/Tokyo',
    'Asia/Shanghai',
    'Asia/Singapore',
    'Australia/Sydney',
  ];

  ngOnInit(): void {
    this.loadSettings();
  }

  private loadSettings(): void {
    const family = this.family();
    if (family) {
      this.familyName = family.name;
      this.timezone = family.timezone;
      this.defaultView = family.settings.defaultView;
      this.privacyModeEnabled = family.settings.privacyModeEnabled;
      this.privacyModeTimeout = family.settings.privacyModeTimeoutMinutes;
      this.sleepModeEnabled = family.settings.sleepMode.enabled;
      this.sleepStartTime = family.settings.sleepMode.startTime;
      this.sleepEndTime = family.settings.sleepMode.endTime;
    }
  }

  resetSettings(): void {
    this.loadSettings();
    this.successMessage.set(null);
    this.error.set(null);
  }

  saveSettings(): void {
    const familyId = this.authService.user()?.familyId;
    if (!familyId) return;

    this.saving.set(true);
    this.error.set(null);
    this.successMessage.set(null);

    const request: UpdateFamilySettingsRequest = {
      name: this.familyName,
      timezone: this.timezone,
      settings: {
        defaultView: this.defaultView,
        privacyModeEnabled: this.privacyModeEnabled,
        privacyModeTimeoutMinutes: this.privacyModeTimeout,
        sleepMode: {
          enabled: this.sleepModeEnabled,
          startTime: this.sleepStartTime,
          endTime: this.sleepEndTime,
          daysOfWeek: [0, 1, 2, 3, 4, 5, 6],
        },
      },
    };

    this.familyService.updateSettings(familyId, request).subscribe({
      next: () => {
        this.saving.set(false);
        this.successMessage.set('Settings saved successfully!');
      },
      error: (err) => {
        this.saving.set(false);
        this.error.set(err.message || 'Failed to save settings');
      },
    });
  }
}
