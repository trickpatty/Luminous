import { Component, inject, OnInit, OnDestroy, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { interval, Subscription } from 'rxjs';
import { DeviceAuthService } from '../../core/services/device-auth.service';
import { CacheService, ScheduleEvent, TaskData, MemberData } from '../../core/services/cache.service';
import { ClockWidgetComponent } from './components/clock-widget/clock-widget.component';
import { ScheduleViewComponent } from './components/schedule-view/schedule-view.component';
import { TasksViewComponent } from './components/tasks-view/tasks-view.component';
import { environment } from '../../../environments/environment';

type DisplayView = 'schedule' | 'tasks' | 'routines';

@Component({
  selector: 'app-display',
  standalone: true,
  imports: [
    CommonModule,
    ClockWidgetComponent,
    ScheduleViewComponent,
    TasksViewComponent,
  ],
  template: `
    <div class="display-container">
      <!-- Header with clock and weather -->
      <header class="display-header">
        <app-clock-widget />
        <div class="header-actions">
          <button
            class="header-nav-btn"
            [class.active]="currentView() === 'schedule'"
            (click)="setView('schedule')"
          >
            <svg xmlns="http://www.w3.org/2000/svg" width="32" height="32" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
              <rect width="18" height="18" x="3" y="4" rx="2" ry="2"/>
              <line x1="16" x2="16" y1="2" y2="6"/>
              <line x1="8" x2="8" y1="2" y2="6"/>
              <line x1="3" x2="21" y1="10" y2="10"/>
            </svg>
          </button>
          <button
            class="header-nav-btn"
            [class.active]="currentView() === 'tasks'"
            (click)="setView('tasks')"
          >
            <svg xmlns="http://www.w3.org/2000/svg" width="32" height="32" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
              <path d="M12 22c5.523 0 10-4.477 10-10S17.523 2 12 2 2 6.477 2 12s4.477 10 10 10z"/>
              <path d="m9 12 2 2 4-4"/>
            </svg>
          </button>
          <button
            class="header-nav-btn"
            (click)="openSettings()"
          >
            <svg xmlns="http://www.w3.org/2000/svg" width="32" height="32" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
              <path d="M12.22 2h-.44a2 2 0 0 0-2 2v.18a2 2 0 0 1-1 1.73l-.43.25a2 2 0 0 1-2 0l-.15-.08a2 2 0 0 0-2.73.73l-.22.38a2 2 0 0 0 .73 2.73l.15.1a2 2 0 0 1 1 1.72v.51a2 2 0 0 1-1 1.74l-.15.09a2 2 0 0 0-.73 2.73l.22.38a2 2 0 0 0 2.73.73l.15-.08a2 2 0 0 1 2 0l.43.25a2 2 0 0 1 1 1.73V20a2 2 0 0 0 2 2h.44a2 2 0 0 0 2-2v-.18a2 2 0 0 1 1-1.73l.43-.25a2 2 0 0 1 2 0l.15.08a2 2 0 0 0 2.73-.73l.22-.39a2 2 0 0 0-.73-2.73l-.15-.08a2 2 0 0 1-1-1.74v-.5a2 2 0 0 1 1-1.74l.15-.09a2 2 0 0 0 .73-2.73l-.22-.38a2 2 0 0 0-2.73-.73l-.15.08a2 2 0 0 1-2 0l-.43-.25a2 2 0 0 1-1-1.73V4a2 2 0 0 0-2-2z"/>
              <circle cx="12" cy="12" r="3"/>
            </svg>
          </button>
        </div>
      </header>

      <!-- Main content area -->
      <main class="display-content">
        @switch (currentView()) {
          @case ('schedule') {
            <app-schedule-view
              [events]="todayEvents()"
              [members]="members()"
              [isLoading]="isLoading()"
            />
          }
          @case ('tasks') {
            <app-tasks-view
              [tasks]="tasks()"
              [members]="members()"
              [isLoading]="isLoading()"
            />
          }
        }
      </main>

      <!-- Footer -->
      <footer class="display-footer">
        <div class="footer-family-name">
          {{ familyName() }}
        </div>
        @if (isOffline()) {
          <div class="footer-offline-badge">
            <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
              <path d="M12 20h.01"/>
              <path d="M8.5 16.429a5 5 0 0 1 7 0"/>
              <path d="M5 12.859a10 10 0 0 1 5.17-2.69"/>
              <path d="M19 12.859a10 10 0 0 0-2.007-1.523"/>
              <line x1="2" x2="22" y1="2" y2="22"/>
            </svg>
            <span>Offline</span>
          </div>
        }
      </footer>
    </div>
  `,
  styles: [`
    .display-header {
      display: flex;
      justify-content: space-between;
      align-items: flex-start;
    }

    .header-actions {
      display: flex;
      gap: var(--space-3);
    }

    .header-nav-btn {
      width: var(--touch-xl);
      height: var(--touch-xl);
      display: flex;
      align-items: center;
      justify-content: center;
      background: var(--surface-secondary);
      border: none;
      border-radius: var(--radius-xl);
      color: var(--text-secondary);
      cursor: pointer;
      transition: all var(--duration-quick) var(--ease-out);
    }

    .header-nav-btn:active {
      transform: scale(0.95);
      background: var(--surface-pressed);
    }

    .header-nav-btn.active {
      background: var(--accent-100);
      color: var(--accent-600);
    }

    .display-content {
      flex: 1;
      overflow: hidden;
    }

    .display-footer {
      display: flex;
      align-items: center;
      justify-content: space-between;
      padding: var(--space-4);
    }

    .footer-family-name {
      font-size: 1.25rem;
      font-weight: 500;
      color: var(--text-secondary);
    }

    .footer-offline-badge {
      display: flex;
      align-items: center;
      gap: var(--space-2);
      padding: var(--space-2) var(--space-4);
      background: var(--warning-light);
      color: var(--warning-dark);
      border-radius: var(--radius-full);
      font-size: 0.875rem;
      font-weight: 500;
    }
  `],
})
export class DisplayComponent implements OnInit, OnDestroy {
  private readonly router = inject(Router);
  private readonly authService = inject(DeviceAuthService);
  private readonly cacheService = inject(CacheService);

  private refreshSubscription?: Subscription;

  // State
  protected readonly currentView = signal<DisplayView>('schedule');
  protected readonly isLoading = signal(true);
  protected readonly isOffline = signal(false);

  // Data
  protected readonly todayEvents = signal<ScheduleEvent[]>([]);
  protected readonly tasks = signal<TaskData[]>([]);
  protected readonly members = signal<MemberData[]>([]);

  protected readonly familyName = this.authService.familyName;

  ngOnInit(): void {
    this.loadData();
    this.startRefreshTimer();

    // Listen for online/offline
    window.addEventListener('online', this.handleOnline);
    window.addEventListener('offline', this.handleOffline);
    this.isOffline.set(!navigator.onLine);
  }

  ngOnDestroy(): void {
    this.refreshSubscription?.unsubscribe();
    window.removeEventListener('online', this.handleOnline);
    window.removeEventListener('offline', this.handleOffline);
  }

  private handleOnline = (): void => {
    this.isOffline.set(false);
    this.loadData(); // Refresh data when back online
  };

  private handleOffline = (): void => {
    this.isOffline.set(true);
    this.loadFromCache(); // Fall back to cache
  };

  private async loadData(): Promise<void> {
    this.isLoading.set(true);

    try {
      // Try to load from API first
      // For now, we'll use cached data as placeholder
      await this.loadFromCache();

      // In production, this would fetch from API and update cache
      // const response = await this.api.getToday();
      // this.todayEvents.set(response.events);
      // this.tasks.set(response.tasks);
      // this.cacheService.cacheSchedule(today, response.events);
      // this.cacheService.cacheTasks(response.tasks);

    } catch (error) {
      console.error('Failed to load data:', error);
      await this.loadFromCache();
    } finally {
      this.isLoading.set(false);
    }
  }

  private async loadFromCache(): Promise<void> {
    const today = new Date().toISOString().split('T')[0];
    const [schedule, tasks, members] = await Promise.all([
      this.cacheService.getSchedule(today),
      this.cacheService.getTasks(),
      this.cacheService.getMembers(),
    ]);

    if (schedule) {
      this.todayEvents.set(schedule.events);
    }
    if (tasks) {
      this.tasks.set(tasks);
    }
    if (members) {
      this.members.set(members);
    }
  }

  private startRefreshTimer(): void {
    this.refreshSubscription = interval(environment.display.refreshInterval)
      .subscribe(() => {
        if (!this.isOffline()) {
          this.loadData();
        }
      });
  }

  setView(view: DisplayView): void {
    this.currentView.set(view);
  }

  openSettings(): void {
    this.router.navigate(['/settings']);
  }
}
