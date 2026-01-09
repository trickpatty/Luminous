import { Component, inject, OnInit, OnDestroy, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { interval, Subscription } from 'rxjs';
import { DeviceAuthService } from '../../core/services/device-auth.service';
import { CacheService, ScheduleEvent, TaskData, MemberData } from '../../core/services/cache.service';
import { EventService } from '../../core/services/event.service';
import { ClockWidgetComponent } from './components/clock-widget/clock-widget.component';
import { WhatsNextWidgetComponent } from './components/whats-next-widget/whats-next-widget.component';
import { CountdownWidgetComponent } from './components/countdown-widget/countdown-widget.component';
import { WeatherWidgetComponent } from './components/weather-widget/weather-widget.component';
import { ScheduleViewComponent } from './components/schedule-view/schedule-view.component';
import { TasksViewComponent } from './components/tasks-view/tasks-view.component';
import {
  DayViewComponent,
  WeekViewComponent,
  MonthViewComponent,
  AgendaViewComponent,
  ProfileFilterComponent
} from './components/calendar-views';
import { environment } from '../../../environments/environment';

type DisplayView = 'schedule' | 'tasks' | 'routines' | 'calendar';
type CalendarViewMode = 'day' | 'week' | 'month' | 'agenda';

@Component({
  selector: 'app-display',
  standalone: true,
  imports: [
    CommonModule,
    ClockWidgetComponent,
    WhatsNextWidgetComponent,
    CountdownWidgetComponent,
    WeatherWidgetComponent,
    ScheduleViewComponent,
    TasksViewComponent,
    DayViewComponent,
    WeekViewComponent,
    MonthViewComponent,
    AgendaViewComponent,
    ProfileFilterComponent,
  ],
  template: `
    <div class="display-container">
      <!-- Header with clock and weather -->
      <header class="display-header">
        <div class="header-left">
          <app-clock-widget />
        </div>
        <div class="header-center">
          <app-weather-widget [compact]="true" />
        </div>
        <div class="header-actions">
          <button
            class="header-nav-btn"
            [class.active]="currentView() === 'schedule'"
            (click)="setView('schedule')"
            aria-label="Today's schedule"
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
            [class.active]="currentView() === 'calendar'"
            (click)="setView('calendar')"
            aria-label="Calendar views"
          >
            <svg xmlns="http://www.w3.org/2000/svg" width="32" height="32" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
              <path d="M8 2v4"/>
              <path d="M16 2v4"/>
              <rect width="18" height="18" x="3" y="4" rx="2"/>
              <path d="M3 10h18"/>
              <path d="M8 14h.01"/>
              <path d="M12 14h.01"/>
              <path d="M16 14h.01"/>
              <path d="M8 18h.01"/>
              <path d="M12 18h.01"/>
              <path d="M16 18h.01"/>
            </svg>
          </button>
          <button
            class="header-nav-btn"
            [class.active]="currentView() === 'tasks'"
            (click)="setView('tasks')"
            aria-label="Tasks"
          >
            <svg xmlns="http://www.w3.org/2000/svg" width="32" height="32" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
              <path d="M12 22c5.523 0 10-4.477 10-10S17.523 2 12 2 2 6.477 2 12s4.477 10 10 10z"/>
              <path d="m9 12 2 2 4-4"/>
            </svg>
          </button>
          <button
            class="header-nav-btn"
            (click)="openSettings()"
            aria-label="Settings"
          >
            <svg xmlns="http://www.w3.org/2000/svg" width="32" height="32" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
              <path d="M12.22 2h-.44a2 2 0 0 0-2 2v.18a2 2 0 0 1-1 1.73l-.43.25a2 2 0 0 1-2 0l-.15-.08a2 2 0 0 0-2.73.73l-.22.38a2 2 0 0 0 .73 2.73l.15.1a2 2 0 0 1 1 1.72v.51a2 2 0 0 1-1 1.74l-.15.09a2 2 0 0 0-.73 2.73l.22.38a2 2 0 0 0 2.73.73l.15-.08a2 2 0 0 1 2 0l.43.25a2 2 0 0 1 1 1.73V20a2 2 0 0 0 2 2h.44a2 2 0 0 0 2-2v-.18a2 2 0 0 1 1-1.73l.43-.25a2 2 0 0 1 2 0l.15.08a2 2 0 0 0 2.73-.73l.22-.39a2 2 0 0 0-.73-2.73l-.15-.08a2 2 0 0 1-1-1.74v-.5a2 2 0 0 1 1-1.74l.15-.09a2 2 0 0 0 .73-2.73l-.22-.38a2 2 0 0 0-2.73-.73l-.15.08a2 2 0 0 1-2 0l-.43-.25a2 2 0 0 1-1-1.73V4a2 2 0 0 0-2-2z"/>
              <circle cx="12" cy="12" r="3"/>
            </svg>
          </button>
        </div>
      </header>

      <!-- Calendar view mode selector (only when in calendar view) -->
      @if (currentView() === 'calendar') {
        <div class="calendar-controls">
          <div class="view-mode-selector">
            <button
              class="mode-btn"
              [class.active]="calendarMode() === 'day'"
              (click)="setCalendarMode('day')"
            >
              Day
            </button>
            <button
              class="mode-btn"
              [class.active]="calendarMode() === 'week'"
              (click)="setCalendarMode('week')"
            >
              Week
            </button>
            <button
              class="mode-btn"
              [class.active]="calendarMode() === 'month'"
              (click)="setCalendarMode('month')"
            >
              Month
            </button>
            <button
              class="mode-btn"
              [class.active]="calendarMode() === 'agenda'"
              (click)="setCalendarMode('agenda')"
            >
              Agenda
            </button>
          </div>

          <!-- Profile filter toggle -->
          <button
            class="filter-toggle-btn"
            [class.active]="showProfileFilter()"
            (click)="toggleProfileFilter()"
            aria-label="Filter by family member"
          >
            <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
              <path d="M16 21v-2a4 4 0 0 0-4-4H6a4 4 0 0 0-4 4v2"/>
              <circle cx="9" cy="7" r="4"/>
              <path d="M22 21v-2a4 4 0 0 0-3-3.87"/>
              <path d="M16 3.13a4 4 0 0 1 0 7.75"/>
            </svg>
            @if (selectedMemberIds().length > 0) {
              <span class="filter-badge">{{ selectedMemberIds().length }}</span>
            }
          </button>
        </div>

        <!-- Profile filter panel -->
        @if (showProfileFilter()) {
          <app-profile-filter
            [members]="members()"
            [selectedMemberIds]="selectedMemberIds()"
            (selectionChange)="onMemberFilterChange($event)"
          />
        }
      }

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
          @case ('calendar') {
            @switch (calendarMode()) {
              @case ('day') {
                <app-day-view
                  [events]="filteredCalendarEvents()"
                  [members]="members()"
                  [isLoading]="isCalendarLoading()"
                  [selectedDate]="selectedDate()"
                  (dateChange)="onDateChange($event)"
                />
              }
              @case ('week') {
                <app-week-view
                  [events]="filteredCalendarEvents()"
                  [members]="members()"
                  [isLoading]="isCalendarLoading()"
                  [selectedDate]="selectedDate()"
                  (dateChange)="onDateChange($event)"
                  (daySelect)="onDaySelect($event)"
                />
              }
              @case ('month') {
                <app-month-view
                  [events]="filteredCalendarEvents()"
                  [members]="members()"
                  [isLoading]="isCalendarLoading()"
                  [selectedDate]="selectedDate()"
                  (dateChange)="onDateChange($event)"
                  (daySelect)="onDaySelect($event)"
                />
              }
              @case ('agenda') {
                <app-agenda-view
                  [events]="filteredCalendarEvents()"
                  [members]="members()"
                  [isLoading]="isCalendarLoading()"
                />
              }
            }
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

      <!-- Widgets Panel (shown on schedule view) -->
      @if (currentView() === 'schedule') {
        <aside class="widgets-panel">
          <app-whats-next-widget
            [events]="todayEvents()"
            [members]="members()"
          />
          <app-countdown-widget
            [events]="allUpcomingEvents()"
            [members]="members()"
            [maxItems]="3"
          />
        </aside>
      }

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
      gap: var(--space-4);
    }

    .header-left {
      flex-shrink: 0;
    }

    .header-center {
      flex: 1;
      max-width: 320px;
    }

    .header-actions {
      display: flex;
      gap: var(--space-3);
      flex-shrink: 0;
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

    .calendar-controls {
      display: flex;
      justify-content: space-between;
      align-items: center;
      gap: var(--space-4);
      padding: 0 var(--space-2);
    }

    .view-mode-selector {
      display: flex;
      background: var(--surface-secondary);
      border-radius: var(--radius-xl);
      padding: var(--space-1);
    }

    .mode-btn {
      padding: var(--space-2) var(--space-4);
      background: transparent;
      border: none;
      border-radius: var(--radius-lg);
      font-size: 1rem;
      font-weight: 500;
      color: var(--text-secondary);
      cursor: pointer;
      transition: all var(--duration-quick) var(--ease-out);
      min-height: var(--touch-min);
    }

    .mode-btn:hover {
      color: var(--text-primary);
    }

    .mode-btn.active {
      background: var(--surface-primary);
      color: var(--accent-600);
      box-shadow: var(--shadow-sm);
    }

    .filter-toggle-btn {
      position: relative;
      width: var(--touch-lg);
      height: var(--touch-lg);
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

    .filter-toggle-btn:active {
      transform: scale(0.95);
    }

    .filter-toggle-btn.active {
      background: var(--accent-100);
      color: var(--accent-600);
    }

    .filter-badge {
      position: absolute;
      top: 4px;
      right: 4px;
      min-width: 18px;
      height: 18px;
      padding: 0 4px;
      background: var(--accent-500);
      color: white;
      border-radius: var(--radius-full);
      font-size: 0.75rem;
      font-weight: 600;
      display: flex;
      align-items: center;
      justify-content: center;
    }

    .display-content {
      flex: 1;
      overflow: hidden;
    }

    .widgets-panel {
      display: grid;
      grid-template-columns: repeat(2, 1fr);
      gap: var(--space-4);
      padding: var(--space-4) 0;
    }

    @media (max-width: 768px) {
      .widgets-panel {
        grid-template-columns: 1fr;
      }
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
  private readonly eventService = inject(EventService);

  private refreshSubscription?: Subscription;

  // State
  protected readonly currentView = signal<DisplayView>('schedule');
  protected readonly calendarMode = signal<CalendarViewMode>('day');
  protected readonly selectedDate = signal<Date>(new Date());
  protected readonly showProfileFilter = signal(false);
  protected readonly selectedMemberIds = signal<string[]>([]);
  protected readonly isLoading = signal(true);
  protected readonly isCalendarLoading = signal(false);
  protected readonly isOffline = signal(false);

  // Data
  protected readonly todayEvents = signal<ScheduleEvent[]>([]);
  protected readonly calendarEvents = signal<ScheduleEvent[]>([]);
  protected readonly upcomingEvents = signal<ScheduleEvent[]>([]); // For countdown widget
  protected readonly tasks = signal<TaskData[]>([]);
  protected readonly members = signal<MemberData[]>([]);

  protected readonly familyName = this.authService.familyName;

  // Computed: All upcoming events for countdown widget (combine today + upcoming)
  protected readonly allUpcomingEvents = computed(() => {
    const today = this.todayEvents();
    const upcoming = this.upcomingEvents();
    // Merge and deduplicate by id
    const all = [...today, ...upcoming];
    const uniqueMap = new Map(all.map(e => [e.id, e]));
    return Array.from(uniqueMap.values());
  });

  // Computed: filtered calendar events based on selected members
  protected readonly filteredCalendarEvents = computed(() => {
    const events = this.calendarEvents();
    const memberIds = this.selectedMemberIds();

    if (memberIds.length === 0) {
      return events; // No filter, show all
    }

    return events.filter(event =>
      event.memberIds.length === 0 || // Show events without assignees
      event.memberIds.some(id => memberIds.includes(id))
    );
  });

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
    this.loadData();
  };

  private handleOffline = (): void => {
    this.isOffline.set(true);
    this.loadFromCache();
  };

  private async loadData(): Promise<void> {
    this.isLoading.set(true);

    try {
      await this.loadFromCache();

      // Load upcoming events for countdown widget (next 90 days)
      await this.loadUpcomingEvents();

      // Load calendar events if in calendar view
      if (this.currentView() === 'calendar') {
        await this.loadCalendarEvents();
      }

    } catch (error) {
      console.error('Failed to load data:', error);
      await this.loadFromCache();
    } finally {
      this.isLoading.set(false);
    }
  }

  private async loadUpcomingEvents(): Promise<void> {
    try {
      const startDate = new Date();
      const endDate = new Date();
      endDate.setDate(endDate.getDate() + 90); // Next 90 days

      const events = await this.eventService.fetchEvents({ startDate, endDate });
      this.upcomingEvents.set(events);
    } catch (error) {
      console.error('Failed to load upcoming events:', error);
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

  private async loadCalendarEvents(): Promise<void> {
    this.isCalendarLoading.set(true);

    try {
      const mode = this.calendarMode();
      const date = this.selectedDate();
      let events: ScheduleEvent[] = [];

      switch (mode) {
        case 'day':
          events = await this.eventService.fetchEventsForDay(date);
          break;
        case 'week':
          const weekStart = this.getWeekStart(date);
          events = await this.eventService.fetchEventsForWeek(weekStart);
          break;
        case 'month':
          events = await this.eventService.fetchEventsForMonth(date.getFullYear(), date.getMonth());
          break;
        case 'agenda':
          const agendaStart = new Date();
          const agendaEnd = new Date();
          agendaEnd.setDate(agendaEnd.getDate() + 14);
          events = await this.eventService.fetchEvents({ startDate: agendaStart, endDate: agendaEnd });
          break;
      }

      this.calendarEvents.set(events);
    } catch (error) {
      console.error('Failed to load calendar events:', error);
    } finally {
      this.isCalendarLoading.set(false);
    }
  }

  private getWeekStart(date: Date): Date {
    const d = new Date(date);
    const day = d.getDay();
    const diff = d.getDate() - day;
    d.setDate(diff);
    d.setHours(0, 0, 0, 0);
    return d;
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

    // Load calendar events when switching to calendar view
    if (view === 'calendar') {
      this.loadCalendarEvents();
    }
  }

  setCalendarMode(mode: CalendarViewMode): void {
    this.calendarMode.set(mode);
    this.loadCalendarEvents();
  }

  toggleProfileFilter(): void {
    this.showProfileFilter.set(!this.showProfileFilter());
  }

  onMemberFilterChange(memberIds: string[]): void {
    this.selectedMemberIds.set(memberIds);
  }

  onDateChange(date: Date): void {
    this.selectedDate.set(date);
    this.loadCalendarEvents();
  }

  onDaySelect(date: Date): void {
    this.selectedDate.set(date);
    this.setCalendarMode('day');
  }

  openSettings(): void {
    this.router.navigate(['/settings']);
  }
}
