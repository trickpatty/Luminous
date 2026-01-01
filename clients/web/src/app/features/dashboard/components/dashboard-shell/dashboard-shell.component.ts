import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router, NavigationEnd } from '@angular/router';
import { filter } from 'rxjs';
import { AuthService, FamilyService } from '../../../../core';
import { AvatarComponent, ButtonComponent, ToastContainerComponent } from '../../../../shared';
import { CanvasService } from '../../../../core/services/canvas.service';

interface NavItem {
  path: string;
  label: string;
  icon: string;
  exact?: boolean;
}

@Component({
  selector: 'app-dashboard-shell',
  standalone: true,
  imports: [CommonModule, RouterModule, AvatarComponent, ButtonComponent, ToastContainerComponent],
  template: `
    <div class="min-h-screen bg-canvas flex">
      <!-- Mobile menu overlay -->
      @if (mobileMenuOpen()) {
        <div
          class="modal-overlay lg:hidden"
          (click)="closeMobileMenu()"
          aria-hidden="true"
        ></div>
      }

      <!-- Sidebar -->
      <aside
        class="fixed inset-y-0 left-0 z-modal w-64 bg-surface-primary border-r border-light transform transition-transform duration-moderate ease-out lg:translate-x-0 lg:sticky lg:top-0 lg:z-auto lg:h-screen"
        [class.translate-x-0]="mobileMenuOpen()"
        [class.-translate-x-full]="!mobileMenuOpen()"
        role="navigation"
        aria-label="Main navigation"
      >
        <div class="flex flex-col h-full">
          <!-- Logo -->
          <div class="flex items-center h-16 px-6 border-b border-light">
            <h1 class="text-title-md text-accent-600">Luminous</h1>
          </div>

          <!-- Family name -->
          <div class="px-4 py-3 border-b border-light">
            <p class="text-overline text-text-tertiary">
              Family
            </p>
            <p class="mt-1 text-body-sm font-semibold text-text-primary truncate">
              {{ familyService.familyName() || 'Loading...' }}
            </p>
          </div>

          <!-- Navigation -->
          <nav class="flex-1 px-3 py-4 space-y-1 overflow-y-auto">
            @for (item of navItems; track item.path) {
              <a
                [routerLink]="item.path"
                [routerLinkActive]="'bg-accent-50 text-accent-700 border-accent-500'"
                [routerLinkActiveOptions]="{ exact: item.exact ?? false }"
                class="flex items-center gap-3 px-3 py-2.5 rounded-lg text-body-sm font-medium text-text-secondary hover:bg-surface-interactive border-l-2 border-transparent transition-default"
                (click)="closeMobileMenu()"
              >
                <span [innerHTML]="getIcon(item.icon)" class="w-5 h-5 flex-shrink-0"></span>
                {{ item.label }}
              </a>
            }
          </nav>

          <!-- User menu (pinned to bottom) -->
          <div class="flex-shrink-0 p-4 border-t border-light">
            @if (user()) {
              <a routerLink="/dashboard/profile" class="flex items-center gap-3 p-2 -m-2 rounded-lg hover:bg-surface-interactive transition-default">
                <app-avatar [name]="user()!.displayName" [color]="user()!.profile?.color" [src]="user()!.profile?.avatarUrl" size="sm" />
                <div class="flex-1 min-w-0">
                  <p class="text-body-sm font-medium text-text-primary truncate">
                    {{ user()!.displayName }}
                  </p>
                  <p class="text-caption text-text-secondary truncate">
                    {{ user()!.email }}
                  </p>
                </div>
              </a>
              <app-button
                variant="ghost"
                size="sm"
                [fullWidth]="true"
                class="mt-3"
                (onClick)="logout()"
              >
                Sign Out
              </app-button>
            }
          </div>
        </div>
      </aside>

      <!-- Main content area -->
      <div class="flex-1 flex flex-col min-w-0 pb-14 lg:pb-0">
        <!-- Top header (mobile) -->
        <header class="sticky top-0 z-sticky bg-surface-primary border-b border-light lg:hidden">
          <div class="flex items-center justify-between h-14 px-4">
            <button
              type="button"
              class="p-2 -ml-2 text-text-secondary hover:text-text-primary hover:bg-surface-interactive rounded-lg min-h-touch-min min-w-touch-min flex items-center justify-center transition-default"
              (click)="toggleMobileMenu()"
              aria-label="Open menu"
            >
              <svg class="w-6 h-6" fill="none" viewBox="0 0 24 24" stroke="currentColor" aria-hidden="true">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 6h16M4 12h16M4 18h16" />
              </svg>
            </button>
            <h1 class="text-title-sm text-accent-600">Luminous</h1>
            @if (user()) {
              <a routerLink="/dashboard/profile" class="block" aria-label="View profile">
                <app-avatar [name]="user()!.displayName" [color]="user()!.profile?.color" [src]="user()!.profile?.avatarUrl" size="sm" />
              </a>
            }
          </div>
        </header>

        <!-- Page content -->
        <main class="flex-1 overflow-auto">
          <router-outlet />
        </main>
      </div>

      <!-- Mobile Tab Bar -->
      <nav class="tab-bar lg:hidden" aria-label="Mobile navigation">
        @for (item of mobileNavItems; track item.path) {
          <a
            [routerLink]="item.path"
            [routerLinkActive]="'tab-bar-item-active'"
            [routerLinkActiveOptions]="{ exact: item.exact ?? false }"
            class="tab-bar-item"
          >
            <span [innerHTML]="getIcon(item.icon)" aria-hidden="true"></span>
            <span>{{ item.label }}</span>
          </a>
        }
      </nav>

      <!-- Toast container -->
      <app-toast-container />
    </div>
  `,
})
export class DashboardShellComponent implements OnInit {
  private readonly authService = inject(AuthService);
  readonly familyService = inject(FamilyService);
  private readonly router = inject(Router);
  private readonly canvasService = inject(CanvasService);

  user = this.authService.user;
  mobileMenuOpen = signal(false);

  navItems: NavItem[] = [
    { path: '/dashboard', label: 'Home', icon: 'home', exact: true },
    { path: '/dashboard/members', label: 'Family Members', icon: 'users' },
    { path: '/dashboard/devices', label: 'Devices', icon: 'display' },
    { path: '/dashboard/settings', label: 'Settings', icon: 'settings' },
  ];

  mobileNavItems: NavItem[] = [
    { path: '/dashboard', label: 'Home', icon: 'home', exact: true },
    { path: '/dashboard/members', label: 'Family', icon: 'users' },
    { path: '/dashboard/devices', label: 'Devices', icon: 'display' },
    { path: '/dashboard/profile', label: 'Profile', icon: 'user', exact: true },
    { path: '/dashboard/settings', label: 'Settings', icon: 'settings' },
  ];

  ngOnInit(): void {
    // Load family data
    const familyId = this.authService.user()?.familyId;
    if (familyId) {
      this.familyService.getFamily(familyId).subscribe();
    }

    // Load user profile to get profile color and avatar
    this.authService.loadUserProfile().subscribe();

    // Close mobile menu on navigation
    this.router.events
      .pipe(filter((event) => event instanceof NavigationEnd))
      .subscribe(() => this.closeMobileMenu());
  }

  toggleMobileMenu(): void {
    this.mobileMenuOpen.update((open) => !open);
  }

  closeMobileMenu(): void {
    this.mobileMenuOpen.set(false);
  }

  logout(): void {
    this.authService.logout();
    this.familyService.clearState();
  }

  getIcon(name: string): string {
    const icons: Record<string, string> = {
      home: `<svg fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="1.5">
        <path stroke-linecap="round" stroke-linejoin="round" d="M3 12l2-2m0 0l7-7 7 7M5 10v10a1 1 0 001 1h3m10-11l2 2m-2-2v10a1 1 0 01-1 1h-3m-6 0a1 1 0 001-1v-4a1 1 0 011-1h2a1 1 0 011 1v4a1 1 0 001 1m-6 0h6" />
      </svg>`,
      users: `<svg fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="1.5">
        <path stroke-linecap="round" stroke-linejoin="round" d="M12 4.354a4 4 0 110 5.292M15 21H3v-1a6 6 0 0112 0v1zm0 0h6v-1a6 6 0 00-9-5.197M13 7a4 4 0 11-8 0 4 4 0 018 0z" />
      </svg>`,
      user: `<svg fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="1.5">
        <path stroke-linecap="round" stroke-linejoin="round" d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z" />
      </svg>`,
      display: `<svg fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="1.5">
        <path stroke-linecap="round" stroke-linejoin="round" d="M9.75 17L9 20l-1 1h8l-1-1-.75-3M3 13h18M5 17h14a2 2 0 002-2V5a2 2 0 00-2-2H5a2 2 0 00-2 2v10a2 2 0 002 2z" />
      </svg>`,
      settings: `<svg fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="1.5">
        <path stroke-linecap="round" stroke-linejoin="round" d="M10.325 4.317c.426-1.756 2.924-1.756 3.35 0a1.724 1.724 0 002.573 1.066c1.543-.94 3.31.826 2.37 2.37a1.724 1.724 0 001.065 2.572c1.756.426 1.756 2.924 0 3.35a1.724 1.724 0 00-1.066 2.573c.94 1.543-.826 3.31-2.37 2.37a1.724 1.724 0 00-2.572 1.065c-.426 1.756-2.924 1.756-3.35 0a1.724 1.724 0 00-2.573-1.066c-1.543.94-3.31-.826-2.37-2.37a1.724 1.724 0 00-1.065-2.572c-1.756-.426-1.756-2.924 0-3.35a1.724 1.724 0 001.066-2.573c-.94-1.543.826-3.31 2.37-2.37.996.608 2.296.07 2.572-1.065z" />
        <path stroke-linecap="round" stroke-linejoin="round" d="M15 12a3 3 0 11-6 0 3 3 0 016 0z" />
      </svg>`,
    };
    return icons[name] || '';
  }
}
