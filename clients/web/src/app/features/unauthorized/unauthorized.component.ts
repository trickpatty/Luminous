import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { ButtonComponent } from '../../shared';

@Component({
  selector: 'app-unauthorized',
  standalone: true,
  imports: [CommonModule, RouterLink, ButtonComponent],
  template: `
    <div class="min-h-screen flex items-center justify-center bg-gray-50 py-12 px-4">
      <div class="max-w-md w-full text-center">
        <div class="mx-auto w-16 h-16 bg-red-100 rounded-full flex items-center justify-center mb-6">
          <svg class="h-8 w-8 text-red-600" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z" />
          </svg>
        </div>

        <h1 class="text-2xl font-bold text-gray-900 mb-2">Access Denied</h1>
        <p class="text-gray-600 mb-8">
          You don't have permission to access this page. If you believe this is an error, please contact your family administrator.
        </p>

        <div class="flex flex-col sm:flex-row gap-3 justify-center">
          <a routerLink="/dashboard">
            <app-button variant="primary">Go to Dashboard</app-button>
          </a>
          <a routerLink="/auth/login">
            <app-button variant="secondary">Sign In</app-button>
          </a>
        </div>
      </div>
    </div>
  `,
})
export class UnauthorizedComponent {}
