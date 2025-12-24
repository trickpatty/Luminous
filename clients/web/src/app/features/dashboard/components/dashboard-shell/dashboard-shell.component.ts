import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router, NavigationEnd } from '@angular/router';
import { filter } from 'rxjs';
import { AuthService, FamilyService } from '../../../../core';
import { AvatarComponent, ButtonComponent } from '../../../../shared';

interface NavItem {
  path: string;
  label: string;
  icon: string;
  exact?: boolean;
}

@Component({
  selector: 'app-dashboard-shell',
  standalone: true,
  imports: [CommonModule, RouterModule, AvatarComponent, ButtonComponent],
  template: `
    <div class="min-h-screen bg-gray-50 flex">
      <!-- Mobile menu overlay -->
      @if (mobileMenuOpen()) {
        <div
          class="fixed inset-0 z-40 bg-gray-600 bg-opacity-75 lg:hidden"
          (click)="closeMobileMenu()"
        ></div>
      }

      <!-- Sidebar -->
      <aside
        class="fixed inset-y-0 left-0 z-50 w-64 bg-white border-r border-gray-200 transform transition-transform duration-200 ease-in-out lg:translate-x-0 lg:static lg:z-auto"
        [class.translate-x-0]="mobileMenuOpen()"
        [class.-translate-x-full]="!mobileMenuOpen()"
      >
        <div class="flex flex-col h-full">
          <!-- Logo -->
          <div class="flex items-center h-16 px-6 border-b border-gray-100">
            <h1 class="text-xl font-bold text-primary-600">Luminous</h1>
          </div>

          <!-- Family name -->
          <div class="px-4 py-3 border-b border-gray-100">
            <p class="text-xs font-medium text-gray-500 uppercase tracking-wider">
              Family
            </p>
            <p class="mt-1 text-sm font-semibold text-gray-900 truncate">
              {{ familyService.familyName() || 'Loading...' }}
            </p>
          </div>

          <!-- Navigation -->
          <nav class="flex-1 px-3 py-4 space-y-1 overflow-y-auto">
            @for (item of navItems; track item.path) {
              <a
                [routerLink]="item.path"
                [routerLinkActive]="'bg-primary-50 text-primary-700 border-primary-500'"
                [routerLinkActiveOptions]="{ exact: item.exact ?? false }"
                class="flex items-center gap-3 px-3 py-2.5 rounded-lg text-sm font-medium text-gray-700 hover:bg-gray-100 border-l-2 border-transparent transition-colors"
                (click)="closeMobileMenu()"
              >
                <span [innerHTML]="getIcon(item.icon)" class="w-5 h-5 flex-shrink-0"></span>
                {{ item.label }}
              </a>
            }
          </nav>

          <!-- User menu -->
          <div class="p-4 border-t border-gray-200">
            @if (user()) {
              <div class="flex items-center gap-3">
                <app-avatar [name]="user()!.displayName" size="sm" />
                <div class="flex-1 min-w-0">
                  <p class="text-sm font-medium text-gray-900 truncate">
                    {{ user()!.displayName }}
                  </p>
                  <p class="text-xs text-gray-500 truncate">
                    {{ user()!.email }}
                  </p>
                </div>
              </div>
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
      <div class="flex-1 flex flex-col min-w-0">
        <!-- Top header (mobile) -->
        <header class="sticky top-0 z-30 bg-white border-b border-gray-200 lg:hidden">
          <div class="flex items-center justify-between h-14 px-4">
            <button
              type="button"
              class="p-2 -ml-2 text-gray-500 hover:text-gray-700 hover:bg-gray-100 rounded-lg"
              (click)="toggleMobileMenu()"
            >
              <svg class="w-6 h-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 6h16M4 12h16M4 18h16" />
              </svg>
            </button>
            <h1 class="text-lg font-semibold text-primary-600">Luminous</h1>
            @if (user()) {
              <app-avatar [name]="user()!.displayName" size="sm" />
            }
          </div>
        </header>

        <!-- Page content -->
        <main class="flex-1 overflow-auto">
          <router-outlet />
        </main>
      </div>
    </div>
  `,
})
export class DashboardShellComponent implements OnInit {
  private readonly authService = inject(AuthService);
  readonly familyService = inject(FamilyService);
  private readonly router = inject(Router);

  user = this.authService.user;
  mobileMenuOpen = signal(false);

  navItems: NavItem[] = [
    { path: '/dashboard', label: 'Home', icon: 'home', exact: true },
    { path: '/dashboard/members', label: 'Family Members', icon: 'users' },
    { path: '/dashboard/devices', label: 'Devices', icon: 'display' },
    { path: '/dashboard/settings', label: 'Settings', icon: 'settings' },
  ];

  ngOnInit(): void {
    // Load family data
    const familyId = this.authService.user()?.familyId;
    if (familyId) {
      this.familyService.getFamily(familyId).subscribe();
    }

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
      home: `<svg fill="none" viewBox="0 0 24 24" stroke="currentColor">
        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3 12l2-2m0 0l7-7 7 7M5 10v10a1 1 0 001 1h3m10-11l2 2m-2-2v10a1 1 0 01-1 1h-3m-6 0a1 1 0 001-1v-4a1 1 0 011-1h2a1 1 0 011 1v4a1 1 0 001 1m-6 0h6" />
      </svg>`,
      users: `<svg fill="none" viewBox="0 0 24 24" stroke="currentColor">
        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4.354a4 4 0 110 5.292M15 21H3v-1a6 6 0 0112 0v1zm0 0h6v-1a6 6 0 00-9-5.197M13 7a4 4 0 11-8 0 4 4 0 018 0z" />
      </svg>`,
      display: `<svg fill="none" viewBox="0 0 24 24" stroke="currentColor">
        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9.75 17L9 20l-1 1h8l-1-1-.75-3M3 13h18M5 17h14a2 2 0 002-2V5a2 2 0 00-2-2H5a2 2 0 00-2 2v10a2 2 0 002 2z" />
      </svg>`,
      settings: `<svg fill="none" viewBox="0 0 24 24" stroke="currentColor">
        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M10.325 4.317c.426-1.756 2.924-1.756 3.35 0a1.724 1.724 0 002.573 1.066c1.543-.94 3.31.826 2.37 2.37a1.724 1.724 0 001.065 2.572c1.756.426 1.756 2.924 0 3.35a1.724 1.724 0 00-1.066 2.573c.94 1.543-.826 3.31-2.37 2.37a1.724 1.724 0 00-2.572 1.065c-.426 1.756-2.924 1.756-3.35 0a1.724 1.724 0 00-2.573-1.066c-1.543.94-3.31-.826-2.37-2.37a1.724 1.724 0 00-1.065-2.572c-1.756-.426-1.756-2.924 0-3.35a1.724 1.724 0 001.066-2.573c-.94-1.543.826-3.31 2.37-2.37.996.608 2.296.07 2.572-1.065z" />
        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 12a3 3 0 11-6 0 3 3 0 016 0z" />
      </svg>`,
    };
    return icons[name] || '';
  }
}
