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
import { User, UserRole, UpdateUserProfileRequest } from '../../../../models';

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

          <!-- Edit Profile Section -->
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
        </div>
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

  // Form fields
  displayName = '';
  nickname = '';
  profileColor = '#4F46E5';
  birthDate = '';

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
}
