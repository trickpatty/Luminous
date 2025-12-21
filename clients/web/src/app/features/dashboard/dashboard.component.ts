import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { AuthService } from '../../core';
import { CardComponent, AvatarComponent, ButtonComponent } from '../../shared';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, CardComponent, AvatarComponent, ButtonComponent],
  template: `
    <div class="min-h-screen bg-gray-50">
      <!-- Header -->
      <header class="bg-white shadow-sm border-b border-gray-100">
        <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div class="flex justify-between items-center h-16">
            <h1 class="text-2xl font-bold text-primary-600">Luminous</h1>

            <div class="flex items-center gap-4">
              @if (user()) {
                <div class="flex items-center gap-3">
                  <app-avatar
                    [name]="user()!.displayName"
                    size="sm"
                  />
                  <span class="text-sm font-medium text-gray-700 hidden sm:block">
                    {{ user()!.displayName }}
                  </span>
                </div>
              }

              <app-button variant="ghost" size="sm" (onClick)="logout()">
                Sign Out
              </app-button>
            </div>
          </div>
        </div>
      </header>

      <!-- Main Content -->
      <main class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        <div class="mb-8">
          <h2 class="text-2xl font-semibold text-gray-900">
            Welcome{{ user() ? ', ' + user()!.displayName : '' }}!
          </h2>
          <p class="mt-1 text-gray-600">
            Your family command center is ready.
          </p>
        </div>

        <!-- Dashboard Grid -->
        <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
          <!-- Today's Schedule Card -->
          <app-card title="Today's Schedule" size="lg">
            <div class="text-center py-8 text-gray-500">
              <svg class="mx-auto h-12 w-12 text-gray-300" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.5" d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" />
              </svg>
              <p class="mt-2">No events today</p>
              <p class="text-sm mt-1">Connect a calendar to see your schedule</p>
            </div>
          </app-card>

          <!-- Tasks Card -->
          <app-card title="Today's Tasks" size="lg">
            <div class="text-center py-8 text-gray-500">
              <svg class="mx-auto h-12 w-12 text-gray-300" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.5" d="M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2m-6 9l2 2 4-4" />
              </svg>
              <p class="mt-2">No tasks for today</p>
              <p class="text-sm mt-1">Create chores and routines for your family</p>
            </div>
          </app-card>

          <!-- Family Members Card -->
          <app-card title="Family Members" size="lg">
            <div class="text-center py-8 text-gray-500">
              <svg class="mx-auto h-12 w-12 text-gray-300" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.5" d="M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2c0-.656.126-1.283.356-1.857m0 0a5.002 5.002 0 019.288 0M15 7a3 3 0 11-6 0 3 3 0 016 0zm6 3a2 2 0 11-4 0 2 2 0 014 0zM7 10a2 2 0 11-4 0 2 2 0 014 0z" />
              </svg>
              <p class="mt-2">Just you for now</p>
              <p class="text-sm mt-1">Invite family members to join</p>
            </div>
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

            <app-button variant="secondary">
              <svg class="h-5 w-5 mr-2 -ml-1" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M18 9v3m0 0v3m0-3h3m-3 0h-3m-2-5a4 4 0 11-8 0 4 4 0 018 0zM3 20a6 6 0 0112 0v1H3v-1z" />
              </svg>
              Invite Family
            </app-button>

            <app-button variant="secondary">
              <svg class="h-5 w-5 mr-2 -ml-1" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 17V7m0 10a2 2 0 01-2 2H5a2 2 0 01-2-2V7a2 2 0 012-2h2a2 2 0 012 2m0 10a2 2 0 002 2h2a2 2 0 002-2M9 7a2 2 0 012-2h2a2 2 0 012 2m0 10V7m0 10a2 2 0 002 2h2a2 2 0 002-2V7a2 2 0 00-2-2h-2a2 2 0 00-2 2" />
              </svg>
              Link Display
            </app-button>
          </div>
        </div>
      </main>
    </div>
  `,
})
export class DashboardComponent {
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  user = this.authService.user;

  logout(): void {
    this.authService.logout();
  }
}
