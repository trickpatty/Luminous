import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { AuthService, FamilyService, UserService, DeviceService } from '../../../../core';
import { CardComponent, AvatarComponent, ButtonComponent, SpinnerComponent } from '../../../../shared';

@Component({
  selector: 'app-dashboard-home',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    CardComponent,
    AvatarComponent,
    ButtonComponent,
    SpinnerComponent,
  ],
  template: `
    <div class="p-4 sm:p-6 lg:p-8 max-w-7xl mx-auto">
      <!-- Welcome section -->
      <div class="mb-8">
        <h2 class="text-2xl sm:text-3xl font-semibold text-gray-900">
          Welcome{{ user() ? ', ' + user()!.displayName : '' }}!
        </h2>
        <p class="mt-1 text-gray-600">
          Your family command center is ready.
        </p>
      </div>

      <!-- Stats cards -->
      <div class="grid grid-cols-2 lg:grid-cols-4 gap-4 mb-8">
        <div class="bg-white rounded-xl border border-gray-200 p-4 sm:p-6">
          <p class="text-sm font-medium text-gray-500">Family Members</p>
          <p class="mt-2 text-2xl sm:text-3xl font-bold text-gray-900">
            {{ familyService.memberCount() }}
          </p>
        </div>
        <div class="bg-white rounded-xl border border-gray-200 p-4 sm:p-6">
          <p class="text-sm font-medium text-gray-500">Linked Devices</p>
          <p class="mt-2 text-2xl sm:text-3xl font-bold text-gray-900">
            {{ familyService.deviceCount() }}
          </p>
        </div>
        <div class="bg-white rounded-xl border border-gray-200 p-4 sm:p-6">
          <p class="text-sm font-medium text-gray-500">Today's Events</p>
          <p class="mt-2 text-2xl sm:text-3xl font-bold text-gray-900">0</p>
        </div>
        <div class="bg-white rounded-xl border border-gray-200 p-4 sm:p-6">
          <p class="text-sm font-medium text-gray-500">Pending Tasks</p>
          <p class="mt-2 text-2xl sm:text-3xl font-bold text-gray-900">0</p>
        </div>
      </div>

      <!-- Dashboard Grid -->
      <div class="grid grid-cols-1 lg:grid-cols-2 xl:grid-cols-3 gap-6">
        <!-- Today's Schedule Card -->
        <app-card title="Today's Schedule" size="lg">
          <div class="text-center py-8 text-gray-500">
            <svg class="mx-auto h-12 w-12 text-gray-300" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.5" d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" />
            </svg>
            <p class="mt-2 font-medium">No events today</p>
            <p class="text-sm mt-1">Connect a calendar to see your schedule</p>
          </div>
        </app-card>

        <!-- Tasks Card -->
        <app-card title="Today's Tasks" size="lg">
          <div class="text-center py-8 text-gray-500">
            <svg class="mx-auto h-12 w-12 text-gray-300" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.5" d="M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2m-6 9l2 2 4-4" />
            </svg>
            <p class="mt-2 font-medium">No tasks for today</p>
            <p class="text-sm mt-1">Create chores and routines for your family</p>
          </div>
        </app-card>

        <!-- Family Members Preview -->
        <app-card title="Family Members" size="lg">
          @if (userService.loading()) {
            <div class="flex justify-center py-8">
              <app-spinner size="md" />
            </div>
          } @else if (userService.members().length === 0) {
            <div class="text-center py-8 text-gray-500">
              <svg class="mx-auto h-12 w-12 text-gray-300" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.5" d="M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2c0-.656.126-1.283.356-1.857m0 0a5.002 5.002 0 019.288 0M15 7a3 3 0 11-6 0 3 3 0 016 0zm6 3a2 2 0 11-4 0 2 2 0 014 0zM7 10a2 2 0 11-4 0 2 2 0 014 0z" />
              </svg>
              <p class="mt-2 font-medium">Just you for now</p>
              <p class="text-sm mt-1">Invite family members to join</p>
            </div>
          } @else {
            <div class="space-y-3">
              @for (member of userService.members().slice(0, 5); track member.id) {
                <div class="flex items-center gap-3 p-2 rounded-lg hover:bg-gray-50">
                  <app-avatar [name]="member.displayName" [color]="member.profile.color" size="sm" />
                  <div class="flex-1 min-w-0">
                    <p class="text-sm font-medium text-gray-900 truncate">
                      {{ member.displayName }}
                    </p>
                    <p class="text-xs text-gray-500">{{ member.role }}</p>
                  </div>
                </div>
              }
              @if (userService.members().length > 5) {
                <a
                  routerLink="/dashboard/members"
                  class="block text-center text-sm text-primary-600 hover:text-primary-700 font-medium py-2"
                >
                  View all {{ userService.members().length }} members
                </a>
              }
            </div>
          }
        </app-card>
      </div>

      <!-- Quick Actions -->
      <div class="mt-8">
        <h3 class="text-lg font-semibold text-gray-900 mb-4">Quick Actions</h3>
        <div class="flex flex-wrap gap-3">
          <app-button variant="secondary">
            <svg class="h-5 w-5 mr-2 -ml-1" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" />
            </svg>
            Connect Calendar
          </app-button>

          <a routerLink="/dashboard/members">
            <app-button variant="secondary">
              <svg class="h-5 w-5 mr-2 -ml-1" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M18 9v3m0 0v3m0-3h3m-3 0h-3m-2-5a4 4 0 11-8 0 4 4 0 018 0zM3 20a6 6 0 0112 0v1H3v-1z" />
              </svg>
              Invite Family
            </app-button>
          </a>

          <a routerLink="/dashboard/devices">
            <app-button variant="secondary">
              <svg class="h-5 w-5 mr-2 -ml-1" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9.75 17L9 20l-1 1h8l-1-1-.75-3M3 13h18M5 17h14a2 2 0 002-2V5a2 2 0 00-2-2H5a2 2 0 00-2 2v10a2 2 0 002 2z" />
              </svg>
              Link Display
            </app-button>
          </a>
        </div>
      </div>
    </div>
  `,
})
export class DashboardHomeComponent implements OnInit {
  private readonly authService = inject(AuthService);
  readonly familyService = inject(FamilyService);
  readonly userService = inject(UserService);

  user = this.authService.user;

  ngOnInit(): void {
    const familyId = this.authService.user()?.familyId;
    if (familyId) {
      this.userService.getMembers(familyId).subscribe();
    }
  }
}
