import { Component, inject, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { AuthService, UserService, ApiService } from '../../../../core';
import {
  CardComponent,
  ButtonComponent,
  AlertComponent,
  AvatarComponent,
  SpinnerComponent,
} from '../../../../shared';
import { User, UserRole, UpdateUserProfileRequest, PasskeyCredential, PasskeyRegistrationOptions } from '../../../../models';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    RouterModule,
    CardComponent,
    ButtonComponent,
    AlertComponent,
    AvatarComponent,
    SpinnerComponent,
  ],
  template: `
    <div class="p-4 sm:p-6 lg:p-8 max-w-4xl mx-auto">
      <!-- Back button -->
      <div class="mb-6">
        <button
          type="button"
          (click)="goBack()"
          class="inline-flex items-center text-sm text-gray-600 hover:text-gray-900"
        >
          <svg class="w-5 h-5 mr-1" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 19l-7-7 7-7" />
          </svg>
          Back
        </button>
      </div>

      @if (loading()) {
        <div class="flex justify-center py-12">
          <app-spinner size="lg" label="Loading profile..." />
        </div>
      } @else if (user()) {
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
          <!-- Profile Header -->
          <app-card size="lg">
            <div class="flex flex-col sm:flex-row items-center gap-6">
              <!-- Avatar with upload -->
              <div class="relative group">
                <app-avatar
                  [name]="user()!.displayName"
                  [color]="user()!.profile.color"
                  [src]="user()!.profile.avatarUrl"
                  size="xl"
                />
                @if (isOwnProfile() || canEdit()) {
                  <label
                    class="absolute inset-0 flex items-center justify-center bg-black bg-opacity-50 rounded-full opacity-0 group-hover:opacity-100 cursor-pointer transition-opacity"
                  >
                    <input
                      type="file"
                      accept="image/*"
                      class="hidden"
                      (change)="onFileSelected($event)"
                    />
                    <svg class="w-8 h-8 text-white" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3 9a2 2 0 012-2h.93a2 2 0 001.664-.89l.812-1.22A2 2 0 0110.07 4h3.86a2 2 0 011.664.89l.812 1.22A2 2 0 0018.07 7H19a2 2 0 012 2v9a2 2 0 01-2 2H5a2 2 0 01-2-2V9z" />
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 13a3 3 0 11-6 0 3 3 0 016 0z" />
                    </svg>
                  </label>
                }
                @if (uploadingAvatar()) {
                  <div class="absolute inset-0 flex items-center justify-center bg-black bg-opacity-50 rounded-full">
                    <app-spinner size="sm" />
                  </div>
                }
              </div>

              <!-- User info -->
              <div class="flex-1 text-center sm:text-left">
                <h2 class="text-2xl font-semibold text-gray-900">
                  {{ user()!.displayName }}
                  @if (isOwnProfile()) {
                    <span class="text-sm font-normal text-gray-500 ml-2">(You)</span>
                  }
                </h2>
                <p class="text-gray-600 mt-1">{{ user()!.email }}</p>
                <div class="mt-2">
                  <span
                    class="inline-flex items-center px-3 py-1 rounded-full text-sm font-medium"
                    [class]="getRoleBadgeClass(user()!.role)"
                  >
                    {{ user()!.role }}
                  </span>
                </div>
              </div>
            </div>
          </app-card>

          <!-- Edit Profile Section (for users with edit permission) -->
          @if (isOwnProfile() || canEdit()) {
            <app-card title="Edit Profile" subtitle="Update your personal information">
              <div class="space-y-4">
                <div>
                  <label class="block text-sm font-medium text-gray-700 mb-1">
                    Display Name
                  </label>
                  <input
                    type="text"
                    [(ngModel)]="displayName"
                    class="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-primary-500"
                    placeholder="Enter your name"
                  />
                </div>

                <div>
                  <label class="block text-sm font-medium text-gray-700 mb-1">
                    Nickname (optional)
                  </label>
                  <input
                    type="text"
                    [(ngModel)]="nickname"
                    class="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-primary-500"
                    placeholder="Enter a nickname"
                  />
                </div>

                <div>
                  <label class="block text-sm font-medium text-gray-700 mb-1">
                    Profile Color
                  </label>
                  <div class="flex flex-wrap gap-2">
                    @for (color of availableColors; track color) {
                      <button
                        type="button"
                        (click)="profileColor = color"
                        class="w-8 h-8 rounded-full border-2 transition-all"
                        [class.border-gray-900]="profileColor === color"
                        [class.border-transparent]="profileColor !== color"
                        [class.ring-2]="profileColor === color"
                        [class.ring-offset-2]="profileColor === color"
                        [style.backgroundColor]="color"
                      ></button>
                    }
                  </div>
                </div>

                <div>
                  <label class="block text-sm font-medium text-gray-700 mb-1">
                    Birth Date (optional)
                  </label>
                  <input
                    type="date"
                    [(ngModel)]="birthDate"
                    class="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-primary-500"
                  />
                </div>

                <div class="flex justify-end gap-3 pt-4">
                  <app-button variant="secondary" (onClick)="resetForm()">
                    Reset
                  </app-button>
                  <app-button
                    variant="primary"
                    [loading]="saving()"
                    (onClick)="saveProfile()"
                  >
                    Save Changes
                  </app-button>
                </div>
              </div>
            </app-card>
          } @else {
            <!-- Read-only Profile Section (for users without edit permission) -->
            <app-card title="Profile Details" subtitle="Personal information">
              <div class="space-y-4">
                <div>
                  <label class="block text-sm font-medium text-gray-500 mb-1">
                    Display Name
                  </label>
                  <p class="text-gray-900">{{ user()!.displayName }}</p>
                </div>

                @if (user()!.profile.nickname) {
                  <div>
                    <label class="block text-sm font-medium text-gray-500 mb-1">
                      Nickname
                    </label>
                    <p class="text-gray-900">{{ user()!.profile.nickname }}</p>
                  </div>
                }

                <div>
                  <label class="block text-sm font-medium text-gray-500 mb-1">
                    Profile Color
                  </label>
                  <div class="flex items-center gap-2">
                    <span
                      class="w-8 h-8 rounded-full border-2 border-gray-200"
                      [style.backgroundColor]="user()!.profile.color || '#4F46E5'"
                    ></span>
                  </div>
                </div>

                @if (user()!.profile.birthDate) {
                  <div>
                    <label class="block text-sm font-medium text-gray-500 mb-1">
                      Birth Date
                    </label>
                    <p class="text-gray-900">{{ formatDate(user()!.profile.birthDate!) }}</p>
                  </div>
                }
              </div>
            </app-card>
          }

          <!-- Account Info (read-only) -->
          <app-card title="Account Information" subtitle="Details about this account">
            <div class="space-y-3">
              <div class="flex justify-between py-2 border-b border-gray-100">
                <span class="text-sm text-gray-500">Email</span>
                <span class="text-sm text-gray-900">{{ user()!.email }}</span>
              </div>
              <div class="flex justify-between py-2 border-b border-gray-100">
                <span class="text-sm text-gray-500">Role</span>
                <span class="text-sm text-gray-900">{{ user()!.role }}</span>
              </div>
              <div class="flex justify-between py-2 border-b border-gray-100">
                <span class="text-sm text-gray-500">Email Verified</span>
                <span class="text-sm text-gray-900">{{ user()!.emailVerified ? 'Yes' : 'No' }}</span>
              </div>
              <div class="flex justify-between py-2 border-b border-gray-100">
                <span class="text-sm text-gray-500">Member Since</span>
                <span class="text-sm text-gray-900">{{ formatDate(user()!.createdAt) }}</span>
              </div>
              @if (user()!.lastLoginAt) {
                <div class="flex justify-between py-2">
                  <span class="text-sm text-gray-500">Last Login</span>
                  <span class="text-sm text-gray-900">{{ formatDate(user()!.lastLoginAt!) }}</span>
                </div>
              }
            </div>
          </app-card>

          <!-- Passkey Management (for own profile, or admins/owners viewing other profiles) -->
          @if (isOwnProfile() || canEdit()) {
            <app-card
              [title]="isOwnProfile() ? 'Passkeys' : 'Passkeys'"
              [subtitle]="isOwnProfile() ? 'Manage your passkeys for secure, passwordless login' : 'Registered passkeys for this account'"
            >
              @if (loadingPasskeys()) {
                <div class="flex justify-center py-6">
                  <app-spinner size="md" label="Loading passkeys..." />
                </div>
              } @else {
                <div class="space-y-4">
                  <!-- Passkey List -->
                  @if (passkeys().length === 0) {
                    <div class="text-center py-6 text-gray-500">
                      <svg class="mx-auto h-12 w-12 text-gray-300 mb-3" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.5" d="M15 7a2 2 0 012 2m4 0a6 6 0 01-7.743 5.743L11 17H9v2H7v2H4a1 1 0 01-1-1v-2.586a1 1 0 01.293-.707l5.964-5.964A6 6 0 1121 9z" />
                      </svg>
                      <p class="font-medium">No passkeys registered</p>
                      @if (isOwnProfile()) {
                        <p class="text-sm mt-1">Add a passkey for faster, more secure sign-in</p>
                      } @else {
                        <p class="text-sm mt-1">This user has not registered any passkeys</p>
                      }
                    </div>
                  } @else {
                    <div class="divide-y divide-gray-100">
                      @for (passkey of passkeys(); track passkey.id) {
                        <div class="flex items-center justify-between py-3">
                          <div class="flex items-center gap-3">
                            <div class="w-10 h-10 bg-primary-50 rounded-lg flex items-center justify-center">
                              <svg class="w-5 h-5 text-primary-600" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 7a2 2 0 012 2m4 0a6 6 0 01-7.743 5.743L11 17H9v2H7v2H4a1 1 0 01-1-1v-2.586a1 1 0 01.293-.707l5.964-5.964A6 6 0 1121 9z" />
                              </svg>
                            </div>
                            <div>
                              <p class="text-sm font-medium text-gray-900">{{ passkey.name || 'Passkey' }}</p>
                              <p class="text-xs text-gray-500">
                                Added {{ formatDate(passkey.createdAt) }}
                                @if (passkey.lastUsedAt) {
                                  &middot; Last used {{ formatRelativeDate(passkey.lastUsedAt) }}
                                }
                              </p>
                            </div>
                          </div>
                          <!-- Remove button (visible for own profile, OR for admins/owners managing other profiles) -->
                          <button
                            type="button"
                            (click)="confirmRemovePasskey(passkey)"
                            class="p-2 text-gray-400 hover:text-red-600 hover:bg-red-50 rounded-lg transition-colors"
                            title="Remove passkey"
                            [disabled]="removingPasskey()"
                          >
                            <svg class="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" />
                            </svg>
                          </button>
                        </div>
                      }
                    </div>
                  }

                  <!-- Add Passkey Button (only for own profile) -->
                  @if (isOwnProfile()) {
                    @if (passkeySupported()) {
                      <div class="pt-4 border-t border-gray-100">
                        <app-button
                          variant="secondary"
                          [loading]="addingPasskey()"
                          (onClick)="showAddPasskeyModal.set(true)"
                        >
                          <svg class="w-5 h-5 mr-2 -ml-1" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 6v6m0 0v6m0-6h6m-6 0H6" />
                          </svg>
                          Add Passkey
                        </app-button>
                      </div>
                    } @else {
                      <div class="pt-4 border-t border-gray-100">
                        <div class="bg-yellow-50 border border-yellow-200 rounded-lg p-3">
                          <div class="flex items-start gap-2">
                            <svg class="w-5 h-5 text-yellow-600 mt-0.5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z" />
                            </svg>
                            <p class="text-sm text-yellow-800">
                              Your browser doesn't support passkeys. Try using a modern browser like Chrome, Safari, or Edge.
                            </p>
                          </div>
                        </div>
                      </div>
                    }
                  }
                </div>
              }
            </app-card>
          }
        </div>

        <!-- Add Passkey Modal -->
        @if (showAddPasskeyModal()) {
          <div class="fixed inset-0 z-50 overflow-y-auto">
            <div class="flex min-h-full items-center justify-center p-4">
              <div class="fixed inset-0 bg-gray-500 bg-opacity-75" (click)="closeAddPasskeyModal()"></div>
              <div class="relative bg-white rounded-xl shadow-xl max-w-md w-full p-6">
                <h3 class="text-lg font-semibold text-gray-900 mb-4">Add a Passkey</h3>
                <p class="text-sm text-gray-600 mb-4">
                  Passkeys let you sign in securely without a password using your device's built-in authentication (Face ID, Touch ID, Windows Hello, etc.).
                </p>

                <div class="space-y-4">
                  <div>
                    <label class="block text-sm font-medium text-gray-700 mb-1">
                      Passkey Name (optional)
                    </label>
                    <input
                      type="text"
                      [(ngModel)]="newPasskeyName"
                      class="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-primary-500"
                      placeholder="e.g., MacBook Pro, iPhone 15"
                    />
                    <p class="text-xs text-gray-500 mt-1">
                      Give this passkey a name to help you identify it later
                    </p>
                  </div>
                </div>

                <div class="flex justify-end gap-3 mt-6">
                  <app-button variant="secondary" (onClick)="closeAddPasskeyModal()">
                    Cancel
                  </app-button>
                  <app-button
                    variant="primary"
                    [loading]="addingPasskey()"
                    (onClick)="addPasskey()"
                  >
                    Continue
                  </app-button>
                </div>
              </div>
            </div>
          </div>
        }

        <!-- Remove Passkey Confirmation Modal -->
        @if (showRemovePasskeyModal()) {
          <div class="fixed inset-0 z-50 overflow-y-auto">
            <div class="flex min-h-full items-center justify-center p-4">
              <div class="fixed inset-0 bg-gray-500 bg-opacity-75" (click)="closeRemovePasskeyModal()"></div>
              <div class="relative bg-white rounded-xl shadow-xl max-w-md w-full p-6">
                <h3 class="text-lg font-semibold text-gray-900 mb-4">Remove Passkey</h3>
                <p class="text-sm text-gray-600 mb-4">
                  Are you sure you want to remove the passkey <strong>"{{ passkeyToRemove()?.name || 'Passkey' }}"</strong>?
                  You won't be able to use it to sign in anymore.
                </p>
                @if (passkeys().length === 1) {
                  <div class="bg-yellow-50 border border-yellow-200 rounded-lg p-3 mb-4">
                    <div class="flex items-start gap-2">
                      <svg class="w-5 h-5 text-yellow-600 mt-0.5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z" />
                      </svg>
                      <p class="text-sm text-yellow-800">
                        This is your only passkey. After removing it, you'll need to use email verification to sign in.
                      </p>
                    </div>
                  </div>
                }

                <div class="flex justify-end gap-3">
                  <app-button variant="secondary" (onClick)="closeRemovePasskeyModal()">
                    Cancel
                  </app-button>
                  <app-button
                    variant="danger"
                    [loading]="removingPasskey()"
                    (onClick)="removePasskey()"
                  >
                    Remove Passkey
                  </app-button>
                </div>
              </div>
            </div>
          </div>
        }
      } @else {
        <app-card>
          <div class="text-center py-8 text-gray-500">
            <p class="font-medium">Profile not found</p>
            <p class="text-sm mt-1">The requested profile could not be loaded.</p>
          </div>
        </app-card>
      }
    </div>
  `,
})
export class ProfileComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly authService = inject(AuthService);
  private readonly userService = inject(UserService);
  private readonly api = inject(ApiService);

  // State
  user = signal<User | null>(null);
  loading = signal(true);
  saving = signal(false);
  uploadingAvatar = signal(false);
  error = signal<string | null>(null);
  successMessage = signal<string | null>(null);

  // Passkey state
  passkeys = signal<PasskeyCredential[]>([]);
  loadingPasskeys = signal(false);
  addingPasskey = signal(false);
  removingPasskey = signal(false);
  showAddPasskeyModal = signal(false);
  showRemovePasskeyModal = signal(false);
  passkeyToRemove = signal<PasskeyCredential | null>(null);
  passkeySupported = signal(false);

  // Form fields
  displayName = '';
  nickname = '';
  profileColor = '#4F46E5';
  birthDate = '';
  newPasskeyName = '';

  // Available profile colors
  availableColors = [
    '#4F46E5', // Indigo
    '#2563EB', // Blue
    '#0891B2', // Cyan
    '#059669', // Emerald
    '#65A30D', // Lime
    '#CA8A04', // Yellow
    '#EA580C', // Orange
    '#DC2626', // Red
    '#DB2777', // Pink
    '#9333EA', // Purple
    '#6B7280', // Gray
  ];

  // Computed
  isOwnProfile = computed(() => {
    const currentUserId = this.authService.user()?.id;
    const profileUserId = this.user()?.id;
    return currentUserId === profileUserId;
  });

  canEdit = computed(() => {
    const currentRole = this.authService.user()?.role;
    return currentRole === 'Owner' || currentRole === 'Admin';
  });

  ngOnInit(): void {
    // Check if browser supports passkeys
    this.passkeySupported.set(this.authService.isPasskeySupported());

    this.route.paramMap.subscribe((params) => {
      const userId = params.get('id');
      if (userId) {
        this.loadProfile(userId);
      } else {
        // Load own profile if no ID provided
        const currentUserId = this.authService.user()?.id;
        if (currentUserId) {
          this.loadProfile(currentUserId);
        }
      }
    });
  }

  private loadProfile(userId: string): void {
    this.loading.set(true);
    this.error.set(null);

    const familyId = this.authService.user()?.familyId;
    const currentUserId = this.authService.user()?.id;
    if (!familyId) {
      this.error.set('No family ID found');
      this.loading.set(false);
      return;
    }

    this.userService.getUser(familyId, userId).subscribe({
      next: (user) => {
        this.user.set(user);
        this.initializeForm(user);
        this.loading.set(false);

        // Load passkeys if viewing own profile OR if current user is admin/owner
        const currentRole = this.authService.user()?.role;
        const isAdminOrOwner = currentRole === 'Owner' || currentRole === 'Admin';
        if (userId === currentUserId || isAdminOrOwner) {
          this.loadPasskeys(userId);
        }
      },
      error: (err) => {
        this.error.set(err.message || 'Failed to load profile');
        this.loading.set(false);
      },
    });
  }

  private initializeForm(user: User): void {
    this.displayName = user.displayName;
    this.nickname = user.profile?.nickname || '';
    this.profileColor = user.profile?.color || '#4F46E5';
    this.birthDate = user.profile?.birthDate || '';
  }

  resetForm(): void {
    const user = this.user();
    if (user) {
      this.initializeForm(user);
    }
    this.error.set(null);
    this.successMessage.set(null);
  }

  saveProfile(): void {
    const user = this.user();
    const familyId = this.authService.user()?.familyId;
    if (!user || !familyId) return;

    this.saving.set(true);
    this.error.set(null);
    this.successMessage.set(null);

    const request: UpdateUserProfileRequest = {
      displayName: this.displayName,
      profile: {
        color: this.profileColor,
        nickname: this.nickname || undefined,
        birthDate: this.birthDate || undefined,
      },
    };

    this.userService.updateProfile(familyId, user.id, request).subscribe({
      next: (updatedUser) => {
        this.user.set(updatedUser);
        this.saving.set(false);
        this.successMessage.set('Profile updated successfully!');
      },
      error: (err) => {
        this.saving.set(false);
        this.error.set(err.message || 'Failed to update profile');
      },
    });
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (!input.files?.length) return;

    const file = input.files[0];
    if (!file.type.startsWith('image/')) {
      this.error.set('Please select an image file');
      return;
    }

    // Max 5MB
    if (file.size > 5 * 1024 * 1024) {
      this.error.set('Image must be less than 5MB');
      return;
    }

    this.uploadAvatar(file);
  }

  private uploadAvatar(file: File): void {
    const user = this.user();
    const familyId = this.authService.user()?.familyId;
    if (!user || !familyId) return;

    this.uploadingAvatar.set(true);
    this.error.set(null);

    const formData = new FormData();
    formData.append('avatar', file);

    this.api.post<{ avatarUrl: string }>(
      `users/family/${familyId}/${user.id}/avatar`,
      formData
    ).subscribe({
      next: (result) => {
        // Update local user with new avatar URL
        const currentUser = this.user();
        if (currentUser) {
          this.user.set({
            ...currentUser,
            profile: {
              ...currentUser.profile,
              avatarUrl: result.avatarUrl,
            },
          });
        }
        this.uploadingAvatar.set(false);
        this.successMessage.set('Profile photo updated!');
      },
      error: (err) => {
        this.uploadingAvatar.set(false);
        this.error.set(err.message || 'Failed to upload photo');
      },
    });
  }

  getRoleBadgeClass(role: UserRole): string {
    const classes: Record<string, string> = {
      Owner: 'bg-purple-100 text-purple-700',
      Admin: 'bg-blue-100 text-blue-700',
      Adult: 'bg-green-100 text-green-700',
      Teen: 'bg-yellow-100 text-yellow-700',
      Child: 'bg-orange-100 text-orange-700',
      Caregiver: 'bg-gray-100 text-gray-700',
    };
    return classes[role] || 'bg-gray-100 text-gray-700';
  }

  formatDate(dateStr: string): string {
    return new Date(dateStr).toLocaleDateString(undefined, {
      year: 'numeric',
      month: 'long',
      day: 'numeric',
    });
  }

  goBack(): void {
    this.router.navigate(['/dashboard']);
  }

  // ============================================
  // Passkey Management
  // ============================================

  private loadPasskeys(userId?: string): void {
    this.loadingPasskeys.set(true);
    const familyId = this.authService.user()?.familyId;
    const currentUserId = this.authService.user()?.id;
    const targetUserId = userId || currentUserId;

    if (!familyId || !targetUserId) {
      this.loadingPasskeys.set(false);
      return;
    }

    // Use appropriate method based on whether viewing own profile or another user's
    const passkeys$ = targetUserId === currentUserId
      ? this.authService.getPasskeys()
      : this.authService.getPasskeysForUser(familyId, targetUserId);

    passkeys$.subscribe({
      next: (passkeys) => {
        this.passkeys.set(passkeys);
        this.loadingPasskeys.set(false);
      },
      error: (err) => {
        console.error('Failed to load passkeys:', err);
        this.loadingPasskeys.set(false);
      },
    });
  }

  closeAddPasskeyModal(): void {
    this.showAddPasskeyModal.set(false);
    this.newPasskeyName = '';
  }

  async addPasskey(): Promise<void> {
    const email = this.authService.user()?.email;
    if (!email) {
      this.error.set('User email not found');
      return;
    }

    this.addingPasskey.set(true);
    this.error.set(null);

    try {
      // Step 1: Get registration options from server
      const startData = await this.authService
        .startPasskeyRegistration(email)
        .toPromise();

      if (!startData) {
        throw new Error('Failed to get registration options');
      }

      const { options, sessionId } = startData;

      // Step 2: Create credential using browser WebAuthn API
      const credential = await navigator.credentials.create({
        publicKey: this.preparePublicKeyOptions(options),
      }) as PublicKeyCredential;

      if (!credential) {
        throw new Error('Failed to create passkey');
      }

      // Step 3: Prepare the response for the server
      const attestationResponse = credential.response as AuthenticatorAttestationResponse;
      const passkeyResponse = {
        id: credential.id,
        rawId: this.arrayBufferToBase64Url(credential.rawId),
        type: 'public-key' as const,
        response: {
          clientDataJSON: this.arrayBufferToBase64Url(attestationResponse.clientDataJSON),
          attestationObject: this.arrayBufferToBase64Url(attestationResponse.attestationObject),
          transports: attestationResponse.getTransports?.() as AuthenticatorTransport[] | undefined,
        },
      };

      // Step 4: Complete registration on server
      const result = await this.authService
        .completePasskeyRegistration(sessionId, passkeyResponse, this.newPasskeyName || undefined)
        .toPromise();

      if (result?.success) {
        this.addingPasskey.set(false);
        this.closeAddPasskeyModal();
        this.successMessage.set('Passkey added successfully!');
        this.loadPasskeys();
      } else {
        throw new Error(result?.error || 'Failed to register passkey');
      }
    } catch (err: unknown) {
      this.addingPasskey.set(false);
      const errorMessage = err instanceof Error ? err.message : 'Failed to add passkey';

      // Handle user cancellation gracefully
      if (err instanceof DOMException && err.name === 'NotAllowedError') {
        this.error.set('Passkey creation was cancelled');
      } else {
        this.error.set(errorMessage);
      }
    }
  }

  confirmRemovePasskey(passkey: PasskeyCredential): void {
    this.passkeyToRemove.set(passkey);
    this.showRemovePasskeyModal.set(true);
  }

  closeRemovePasskeyModal(): void {
    this.showRemovePasskeyModal.set(false);
    this.passkeyToRemove.set(null);
  }

  removePasskey(): void {
    const passkey = this.passkeyToRemove();
    if (!passkey) return;

    this.removingPasskey.set(true);
    this.error.set(null);

    const familyId = this.authService.user()?.familyId;
    const profileUserId = this.user()?.id;
    const currentUserId = this.authService.user()?.id;

    // Use appropriate method based on whether deleting from own profile or another user's
    const delete$ = profileUserId === currentUserId
      ? this.authService.deletePasskey(passkey.id)
      : (familyId && profileUserId
          ? this.authService.deletePasskeyForUser(familyId, profileUserId, passkey.id)
          : this.authService.deletePasskey(passkey.id));

    delete$.subscribe({
      next: () => {
        this.removingPasskey.set(false);
        this.closeRemovePasskeyModal();
        this.successMessage.set('Passkey removed successfully');
        this.passkeys.update((list) => list.filter((p) => p.id !== passkey.id));
      },
      error: (err) => {
        this.removingPasskey.set(false);
        this.error.set(err.message || 'Failed to remove passkey');
      },
    });
  }

  formatRelativeDate(dateStr: string): string {
    const date = new Date(dateStr);
    const now = new Date();
    const diffMs = now.getTime() - date.getTime();
    const diffDays = Math.floor(diffMs / (1000 * 60 * 60 * 24));

    if (diffDays === 0) {
      const diffHours = Math.floor(diffMs / (1000 * 60 * 60));
      if (diffHours === 0) {
        const diffMinutes = Math.floor(diffMs / (1000 * 60));
        return diffMinutes <= 1 ? 'just now' : `${diffMinutes} minutes ago`;
      }
      return diffHours === 1 ? '1 hour ago' : `${diffHours} hours ago`;
    }
    if (diffDays === 1) return 'yesterday';
    if (diffDays < 7) return `${diffDays} days ago`;
    if (diffDays < 30) return `${Math.floor(diffDays / 7)} weeks ago`;
    return this.formatDate(dateStr);
  }

  // WebAuthn helpers
  private preparePublicKeyOptions(options: PasskeyRegistrationOptions): PublicKeyCredentialCreationOptions {
    // Convert base64url strings to ArrayBuffers where needed
    return {
      ...options,
      challenge: this.base64UrlToArrayBuffer(options.challenge),
      user: {
        ...options.user,
        id: this.base64UrlToArrayBuffer(options.user.id),
      },
    };
  }

  private base64UrlToArrayBuffer(base64url: string): ArrayBuffer {
    const base64 = base64url.replace(/-/g, '+').replace(/_/g, '/');
    const padding = '='.repeat((4 - (base64.length % 4)) % 4);
    const binary = atob(base64 + padding);
    const bytes = new Uint8Array(binary.length);
    for (let i = 0; i < binary.length; i++) {
      bytes[i] = binary.charCodeAt(i);
    }
    return bytes.buffer;
  }

  private arrayBufferToBase64Url(buffer: ArrayBuffer): string {
    const bytes = new Uint8Array(buffer);
    let binary = '';
    for (let i = 0; i < bytes.length; i++) {
      binary += String.fromCharCode(bytes[i]);
    }
    return btoa(binary).replace(/\+/g, '-').replace(/\//g, '_').replace(/=/g, '');
  }
}
