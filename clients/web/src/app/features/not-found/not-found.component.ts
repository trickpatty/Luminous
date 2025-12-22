import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { ButtonComponent } from '../../shared';

@Component({
  selector: 'app-not-found',
  standalone: true,
  imports: [CommonModule, RouterLink, ButtonComponent],
  template: `
    <div class="min-h-screen flex items-center justify-center bg-gray-50 py-12 px-4">
      <div class="max-w-md w-full text-center">
        <div class="text-8xl font-bold text-gray-200 mb-4">404</div>

        <h1 class="text-2xl font-bold text-gray-900 mb-2">Page Not Found</h1>
        <p class="text-gray-600 mb-8">
          The page you're looking for doesn't exist or has been moved.
        </p>

        <div class="flex flex-col sm:flex-row gap-3 justify-center">
          <a routerLink="/">
            <app-button variant="primary">Go Home</app-button>
          </a>
          <a routerLink="/dashboard">
            <app-button variant="secondary">Dashboard</app-button>
          </a>
        </div>
      </div>
    </div>
  `,
})
export class NotFoundComponent {}
