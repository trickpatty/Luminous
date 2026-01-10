import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AuthService, DeviceService } from '../../../../core';
import {
  CardComponent,
  ButtonComponent,
  AlertComponent,
  SpinnerComponent,
} from '../../../../shared';
import { Device, DeviceType } from '../../../../models';

@Component({
  selector: 'app-devices',
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
    <div class="p-4 sm:p-6 lg:p-8 max-w-6xl mx-auto">
      <div class="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4 mb-8">
        <div>
          <h2 class="text-2xl font-semibold text-gray-900">Devices</h2>
          <p class="mt-1 text-gray-600">
            Manage your linked displays and devices.
          </p>
        </div>
        @if (canManageDevices()) {
          <app-button variant="primary" (onClick)="showLinkModal.set(true)">
            <svg class="h-5 w-5 mr-2 -ml-1" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 6v6m0 0v6m0-6h6m-6 0H6" />
            </svg>
            Link New Device
          </app-button>
        }
      </div>

      @if (error()) {
        <app-alert variant="error" [dismissible]="true" (dismiss)="error.set(null)" class="mb-6">
          {{ error() }}
        </app-alert>
      }

      @if (successMessage()) {
        <app-alert variant="success" [dismissible]="true" (dismiss)="successMessage.set(null)" class="mb-6">
          {{ successMessage() }}
        </app-alert>
      }

      <!-- Devices Grid -->
      @if (deviceService.loading()) {
        <div class="flex justify-center py-12">
          <app-spinner size="lg" label="Loading devices..." />
        </div>
      } @else if (devices().length === 0) {
        <app-card>
          <div class="text-center py-12">
            <svg class="mx-auto h-16 w-16 text-gray-300" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.5" d="M9.75 17L9 20l-1 1h8l-1-1-.75-3M3 13h18M5 17h14a2 2 0 002-2V5a2 2 0 00-2-2H5a2 2 0 00-2 2v10a2 2 0 002 2z" />
            </svg>
            <h3 class="mt-4 text-lg font-medium text-gray-900">No devices linked</h3>
            <p class="mt-1 text-gray-500">
              Link a wall display or other device to your family.
            </p>
            @if (canManageDevices()) {
              <app-button
                variant="primary"
                class="mt-6"
                (onClick)="showLinkModal.set(true)"
              >
                Link Your First Device
              </app-button>
            }
          </div>
        </app-card>
      } @else {
        <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
          @for (device of devices(); track device.id) {
            <div class="bg-white rounded-xl border border-gray-200 overflow-hidden">
              <div class="p-6">
                <div class="flex items-start justify-between">
                  <div class="flex items-center gap-3">
                    <div
                      class="w-12 h-12 rounded-lg flex items-center justify-center"
                      [class]="getDeviceIconBg(device.type)"
                    >
                      <span [innerHTML]="getDeviceIcon(device.type)" class="w-6 h-6"></span>
                    </div>
                    <div>
                      <h3 class="text-sm font-semibold text-gray-900">{{ device.name }}</h3>
                      <p class="text-xs text-gray-500">{{ deviceService.getDeviceTypeDisplayName(device.type) }}</p>
                    </div>
                  </div>
                  <div class="flex items-center gap-2">
                    <span
                      class="inline-flex items-center gap-1 px-2 py-0.5 rounded-full text-xs font-medium"
                      [class]="isDeviceOnline(device) ? 'bg-green-100 text-green-700' : 'bg-gray-100 text-gray-600'"
                    >
                      <span
                        class="w-1.5 h-1.5 rounded-full"
                        [class]="isDeviceOnline(device) ? 'bg-green-500' : 'bg-gray-400'"
                      ></span>
                      {{ isDeviceOnline(device) ? 'Online' : 'Offline' }}
                    </span>
                  </div>
                </div>

                <div class="mt-4 space-y-2 text-sm">
                  @if (device.platform) {
                    <div class="flex justify-between">
                      <span class="text-gray-500">Platform</span>
                      <span class="text-gray-900">{{ device.platform }}</span>
                    </div>
                  }
                  @if (device.appVersion) {
                    <div class="flex justify-between">
                      <span class="text-gray-500">Version</span>
                      <span class="text-gray-900">{{ device.appVersion }}</span>
                    </div>
                  }
                  <div class="flex justify-between">
                    <span class="text-gray-500">Last seen</span>
                    <span class="text-gray-900">{{ formatLastSeen(device.lastSeenAt) }}</span>
                  </div>
                  @if (device.linkedAt) {
                    <div class="flex justify-between">
                      <span class="text-gray-500">Linked</span>
                      <span class="text-gray-900">{{ formatDate(device.linkedAt) }}</span>
                    </div>
                  }
                </div>
              </div>

              @if (canManageDevices()) {
                <div class="px-6 py-3 bg-gray-50 border-t border-gray-100 flex justify-end gap-2">
                  <app-button variant="ghost" size="sm" (onClick)="editDevice(device)">
                    Edit
                  </app-button>
                  <app-button variant="ghost" size="sm" (onClick)="confirmUnlink(device)">
                    Unlink
                  </app-button>
                </div>
              }
            </div>
          }
        </div>
      }

      <!-- Link Device Modal -->
      @if (showLinkModal()) {
        <div class="fixed inset-0 z-50 overflow-y-auto">
          <div class="flex min-h-full items-center justify-center p-4">
            <div class="modal-overlay" (click)="closeLinkModal()"></div>
            <div class="relative bg-white rounded-xl shadow-xl max-w-md w-full p-6">
              <h3 class="text-lg font-semibold text-gray-900 mb-4">Link New Device</h3>

              @if (!linkSuccess()) {
                <div class="space-y-4">
                  <!-- Instructions -->
                  <div class="bg-blue-50 border border-blue-200 rounded-lg p-4 mb-4">
                    <p class="text-sm text-blue-800">
                      <strong>Step 1:</strong> On your display, tap "Get Link Code" to generate a 6-digit code.
                    </p>
                    <p class="text-sm text-blue-800 mt-2">
                      <strong>Step 2:</strong> Enter that code below to link the device to your family.
                    </p>
                  </div>

                  <div>
                    <label class="block text-sm font-medium text-gray-700 mb-1">
                      Link Code from Display
                    </label>
                    <input
                      type="text"
                      [(ngModel)]="enteredLinkCode"
                      class="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-primary-500 text-center text-2xl font-mono tracking-widest"
                      placeholder="000000"
                      maxlength="6"
                      pattern="[0-9]*"
                      inputmode="numeric"
                    />
                  </div>

                  <div>
                    <label class="block text-sm font-medium text-gray-700 mb-1">
                      Device Name
                    </label>
                    <input
                      type="text"
                      [(ngModel)]="newDeviceName"
                      class="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-primary-500"
                      placeholder="e.g., Kitchen Display"
                    />
                    <p class="text-xs text-gray-500 mt-1">Give your display a name so you can identify it later.</p>
                  </div>

                  @if (linkError()) {
                    <div class="bg-red-50 border border-red-200 rounded-lg p-3">
                      <p class="text-sm text-red-700">{{ linkError() }}</p>
                    </div>
                  }

                  <div class="flex justify-end gap-3 mt-6">
                    <app-button variant="secondary" (onClick)="closeLinkModal()">
                      Cancel
                    </app-button>
                    <app-button
                      variant="primary"
                      [loading]="linking()"
                      [disabled]="!canLink()"
                      (onClick)="linkDeviceWithCode()"
                    >
                      Link Device
                    </app-button>
                  </div>
                </div>
              } @else {
                <div class="text-center">
                  <div class="mx-auto w-12 h-12 bg-green-100 rounded-full flex items-center justify-center mb-4">
                    <svg class="w-6 h-6 text-green-600" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 13l4 4L19 7" />
                    </svg>
                  </div>
                  <p class="text-lg font-medium text-gray-900 mb-2">Device Linked!</p>
                  <p class="text-sm text-gray-600 mb-6">
                    The display is now connected to your family and will sync automatically.
                  </p>
                  <app-button variant="primary" (onClick)="closeLinkModal()">
                    Done
                  </app-button>
                </div>
              }
            </div>
          </div>
        </div>
      }

      <!-- Edit Device Modal -->
      @if (showEditModal()) {
        <div class="fixed inset-0 z-50 overflow-y-auto">
          <div class="flex min-h-full items-center justify-center p-4">
            <div class="modal-overlay" (click)="closeEditModal()"></div>
            <div class="relative bg-white rounded-xl shadow-xl max-w-md w-full p-6">
              <h3 class="text-lg font-semibold text-gray-900 mb-4">Edit Device</h3>

              <div class="space-y-4">
                <div>
                  <label class="block text-sm font-medium text-gray-700 mb-1">
                    Device Name
                  </label>
                  <input
                    type="text"
                    [(ngModel)]="editDeviceName"
                    class="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-primary-500"
                  />
                </div>

                <div class="flex justify-end gap-3 mt-6">
                  <app-button variant="secondary" (onClick)="closeEditModal()">
                    Cancel
                  </app-button>
                  <app-button
                    variant="primary"
                    [loading]="updating()"
                    (onClick)="updateDevice()"
                  >
                    Save Changes
                  </app-button>
                </div>
              </div>
            </div>
          </div>
        </div>
      }

      <!-- Unlink Confirmation Modal -->
      @if (showUnlinkModal()) {
        <div class="fixed inset-0 z-50 overflow-y-auto">
          <div class="flex min-h-full items-center justify-center p-4">
            <div class="modal-overlay" (click)="closeUnlinkModal()"></div>
            <div class="relative bg-white rounded-xl shadow-xl max-w-md w-full p-6">
              <h3 class="text-lg font-semibold text-gray-900 mb-4">Unlink Device</h3>
              <p class="text-sm text-gray-600 mb-4">
                Are you sure you want to unlink <strong>{{ selectedDevice()?.name }}</strong>?
                The device will need to be linked again to access your family's data.
              </p>

              <div class="flex justify-end gap-3 mt-6">
                <app-button variant="secondary" (onClick)="closeUnlinkModal()">
                  Cancel
                </app-button>
                <app-button
                  variant="danger"
                  [loading]="unlinking()"
                  (onClick)="unlinkDevice()"
                >
                  Unlink Device
                </app-button>
              </div>
            </div>
          </div>
        </div>
      }
    </div>
  `,
})
export class DevicesComponent implements OnInit {
  private readonly authService = inject(AuthService);
  readonly deviceService = inject(DeviceService);

  // State
  error = signal<string | null>(null);
  successMessage = signal<string | null>(null);
  showLinkModal = signal(false);
  showEditModal = signal(false);
  showUnlinkModal = signal(false);
  linking = signal(false);
  updating = signal(false);
  unlinking = signal(false);
  selectedDevice = signal<Device | null>(null);
  linkSuccess = signal(false);
  linkError = signal<string | null>(null);

  // Form fields
  newDeviceName = '';
  enteredLinkCode = '';
  editDeviceName = '';

  // Computed
  devices = this.deviceService.devices;

  canManageDevices = () => {
    const role = this.authService.user()?.role;
    return role === 'Owner' || role === 'Admin';
  };

  /**
   * Determine if a device is online based on lastSeenAt timestamp.
   * A device is considered online if it was seen within the last 5 minutes.
   */
  isDeviceOnline(device: Device): boolean {
    if (!device.lastSeenAt) return false;
    const lastSeen = new Date(device.lastSeenAt);
    const fiveMinutesAgo = new Date(Date.now() - 5 * 60 * 1000);
    return lastSeen >= fiveMinutesAgo;
  }

  ngOnInit(): void {
    const familyId = this.authService.user()?.familyId;
    if (familyId) {
      this.deviceService.getDevices(familyId).subscribe();
    }
  }

  getDeviceIconBg(type: DeviceType): string {
    const bgs: Record<DeviceType, string> = {
      [DeviceType.Display]: 'bg-purple-100 text-purple-600',
      [DeviceType.Mobile]: 'bg-blue-100 text-blue-600',
      [DeviceType.Web]: 'bg-green-100 text-green-600',
    };
    return bgs[type] || 'bg-gray-100 text-gray-600';
  }

  getDeviceIcon(type: DeviceType): string {
    const icons: Record<DeviceType, string> = {
      [DeviceType.Display]: `<svg fill="none" viewBox="0 0 24 24" stroke="currentColor">
        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9.75 17L9 20l-1 1h8l-1-1-.75-3M3 13h18M5 17h14a2 2 0 002-2V5a2 2 0 00-2-2H5a2 2 0 00-2 2v10a2 2 0 002 2z" />
      </svg>`,
      [DeviceType.Mobile]: `<svg fill="none" viewBox="0 0 24 24" stroke="currentColor">
        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 18h.01M8 21h8a2 2 0 002-2V5a2 2 0 00-2-2H8a2 2 0 00-2 2v14a2 2 0 002 2z" />
      </svg>`,
      [DeviceType.Web]: `<svg fill="none" viewBox="0 0 24 24" stroke="currentColor">
        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 12a9 9 0 01-9 9m9-9a9 9 0 00-9-9m9 9H3m9 9a9 9 0 01-9-9m9 9c1.657 0 3-4.03 3-9s-1.343-9-3-9m0 18c-1.657 0-3-4.03-3-9s1.343-9 3-9m-9 9a9 9 0 019-9" />
      </svg>`,
    };
    return icons[type] || '';
  }

  formatLastSeen(dateStr: string): string {
    const date = new Date(dateStr);
    const now = new Date();
    const diffMs = now.getTime() - date.getTime();
    const diffMins = Math.floor(diffMs / 60000);

    if (diffMins < 1) return 'Just now';
    if (diffMins < 60) return `${diffMins} min ago`;

    const diffHours = Math.floor(diffMins / 60);
    if (diffHours < 24) return `${diffHours} hour${diffHours > 1 ? 's' : ''} ago`;

    const diffDays = Math.floor(diffHours / 24);
    if (diffDays < 7) return `${diffDays} day${diffDays > 1 ? 's' : ''} ago`;

    return date.toLocaleDateString();
  }

  formatDate(dateStr: string): string {
    return new Date(dateStr).toLocaleString();
  }

  // Link Modal
  closeLinkModal(): void {
    this.showLinkModal.set(false);
    this.newDeviceName = '';
    this.enteredLinkCode = '';
    this.linkSuccess.set(false);
    this.linkError.set(null);
  }

  isValidLinkCode(): boolean {
    return /^\d{6}$/.test(this.enteredLinkCode);
  }

  canLink(): boolean {
    return this.isValidLinkCode() && this.newDeviceName.trim().length > 0;
  }

  linkDeviceWithCode(): void {
    if (!this.canLink()) return;

    const familyId = this.authService.user()?.familyId;
    if (!familyId) {
      this.linkError.set('Unable to determine family. Please try again.');
      return;
    }

    this.linking.set(true);
    this.linkError.set(null);

    this.deviceService
      .linkDevice({
        linkCode: this.enteredLinkCode,
        familyId: familyId,
        deviceName: this.newDeviceName.trim(),
      })
      .subscribe({
        next: () => {
          this.linking.set(false);
          this.linkSuccess.set(true);
          this.successMessage.set('Device linked successfully!');
        },
        error: (err) => {
          this.linking.set(false);
          const errorMessage = err.error?.message || err.message || 'Failed to link device';
          if (errorMessage.includes('not found') || errorMessage.includes('invalid')) {
            this.linkError.set('Invalid or expired link code. Please check the code on your display and try again.');
          } else if (errorMessage.includes('expired')) {
            this.linkError.set('This link code has expired. Please generate a new code on your display.');
          } else {
            this.linkError.set(errorMessage);
          }
        },
      });
  }

  // Edit Modal
  editDevice(device: Device): void {
    this.selectedDevice.set(device);
    this.editDeviceName = device.name;
    this.showEditModal.set(true);
  }

  closeEditModal(): void {
    this.showEditModal.set(false);
    this.selectedDevice.set(null);
    this.editDeviceName = '';
  }

  updateDevice(): void {
    const familyId = this.authService.user()?.familyId;
    const device = this.selectedDevice();
    if (!familyId || !device) return;

    this.updating.set(true);
    this.error.set(null);

    this.deviceService
      .updateDevice(familyId, device.id, { name: this.editDeviceName })
      .subscribe({
        next: () => {
          this.updating.set(false);
          this.closeEditModal();
          this.successMessage.set('Device updated successfully!');
        },
        error: (err) => {
          this.updating.set(false);
          this.error.set(err.message || 'Failed to update device');
        },
      });
  }

  // Unlink Modal
  confirmUnlink(device: Device): void {
    this.selectedDevice.set(device);
    this.showUnlinkModal.set(true);
  }

  closeUnlinkModal(): void {
    this.showUnlinkModal.set(false);
    this.selectedDevice.set(null);
  }

  unlinkDevice(): void {
    const familyId = this.authService.user()?.familyId;
    const device = this.selectedDevice();
    if (!familyId || !device) return;

    this.unlinking.set(true);
    this.error.set(null);

    this.deviceService.unlinkDevice(familyId, device.id).subscribe({
      next: () => {
        this.unlinking.set(false);
        this.closeUnlinkModal();
        this.successMessage.set('Device unlinked successfully!');
      },
      error: (err) => {
        this.unlinking.set(false);
        this.error.set(err.message || 'Failed to unlink device');
      },
    });
  }
}
