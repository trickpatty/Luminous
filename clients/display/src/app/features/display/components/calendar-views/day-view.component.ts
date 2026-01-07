import { Component, Input, Output, EventEmitter, computed, signal, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ScheduleEvent, MemberData } from '../../../../core/services/cache.service';

interface HourSlot {
  hour: number;
  label: string;
  events: ScheduleEvent[];
  isCurrentHour: boolean;
}

@Component({
  selector: 'app-day-view',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="day-view">
      <!-- Header with date navigation -->
      <div class="day-view-header">
        <button class="nav-btn" (click)="navigatePrevious()" aria-label="Previous day">
          <svg xmlns="http://www.w3.org/2000/svg" width="28" height="28" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
            <path d="m15 18-6-6 6-6"/>
          </svg>
        </button>
        <div class="day-view-date">
          <h2 class="date-title">{{ formatDateTitle() }}</h2>
          <span class="date-subtitle">{{ formatDateSubtitle() }}</span>
        </div>
        <button class="nav-btn" (click)="navigateNext()" aria-label="Next day">
          <svg xmlns="http://www.w3.org/2000/svg" width="28" height="28" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
            <path d="m9 18 6-6-6-6"/>
          </svg>
        </button>
      </div>

      <!-- Today button -->
      @if (!isToday()) {
        <button class="today-btn" (click)="goToToday()">
          Today
        </button>
      }

      <!-- Loading state -->
      @if (isLoading) {
        <div class="day-view-loading">
          <div class="display-spinner display-spinner-lg"></div>
        </div>
      } @else if (events.length === 0) {
        <!-- Empty state -->
        <div class="display-empty">
          <div class="display-empty-icon">
            <svg xmlns="http://www.w3.org/2000/svg" width="80" height="80" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round">
              <rect width="18" height="18" x="3" y="4" rx="2" ry="2"/>
              <line x1="16" x2="16" y1="2" y2="6"/>
              <line x1="8" x2="8" y1="2" y2="6"/>
              <line x1="3" x2="21" y1="10" y2="10"/>
              <path d="m9 16 2 2 4-4"/>
            </svg>
          </div>
          <h3 class="display-empty-title">No events</h3>
          <p class="display-empty-description">
            Nothing scheduled for {{ isToday() ? 'today' : 'this day' }}.
          </p>
        </div>
      } @else {
        <!-- Timeline view -->
        <div class="day-timeline display-card">
          @for (slot of hourSlots(); track slot.hour) {
            <div
              class="hour-slot"
              [class.current-hour]="slot.isCurrentHour"
              [attr.data-hour]="slot.hour"
            >
              <div class="hour-label">{{ slot.label }}</div>
              <div class="hour-events">
                @if (slot.isCurrentHour) {
                  <div class="current-time-indicator" [style.top.%]="currentMinutePercent()">
                    <div class="current-time-dot"></div>
                    <div class="current-time-line"></div>
                  </div>
                }
                @for (event of slot.events; track event.id) {
                  <div
                    class="day-event"
                    [style.--event-color]="getEventColor(event)"
                    [class.all-day]="isAllDayEvent(event)"
                    [class.past]="isEventPast(event)"
                    [class.current]="isEventNow(event)"
                  >
                    <div class="event-time">
                      @if (isAllDayEvent(event)) {
                        All day
                      } @else {
                        {{ formatTime(event.startTime) }}
                        @if (event.endTime) {
                          - {{ formatTime(event.endTime) }}
                        }
                      }
                    </div>
                    <div class="event-title">{{ event.title }}</div>
                    @if (event.location) {
                      <div class="event-location">{{ event.location }}</div>
                    }
                    @if (event.memberIds.length > 0) {
                      <div class="event-members">
                        @for (memberId of event.memberIds; track memberId) {
                          @if (getMember(memberId); as member) {
                            <span
                              class="member-badge"
                              [style.background-color]="member.color"
                            >
                              {{ member.initials }}
                            </span>
                          }
                        }
                      </div>
                    }
                  </div>
                }
              </div>
            </div>
          }
        </div>
      }
    </div>
  `,
  styles: [`
    .day-view {
      height: 100%;
      display: flex;
      flex-direction: column;
      gap: var(--space-4);
    }

    .day-view-header {
      display: flex;
      align-items: center;
      justify-content: space-between;
      padding: 0 var(--space-2);
    }

    .nav-btn {
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

    .nav-btn:active {
      transform: scale(0.95);
      background: var(--surface-pressed);
    }

    .day-view-date {
      text-align: center;
    }

    .date-title {
      font-size: var(--font-display-sm);
      font-weight: 600;
      color: var(--text-primary);
    }

    .date-subtitle {
      font-size: 1.125rem;
      color: var(--text-secondary);
    }

    .today-btn {
      align-self: center;
      padding: var(--space-2) var(--space-6);
      background: var(--accent-100);
      color: var(--accent-700);
      border: none;
      border-radius: var(--radius-full);
      font-size: 1rem;
      font-weight: 500;
      cursor: pointer;
      transition: all var(--duration-quick) var(--ease-out);
    }

    .today-btn:active {
      background: var(--accent-200);
    }

    .day-view-loading {
      flex: 1;
      display: flex;
      align-items: center;
      justify-content: center;
    }

    .day-timeline {
      flex: 1;
      overflow-y: auto;
      padding: var(--space-4) !important;
    }

    .hour-slot {
      display: flex;
      min-height: 80px;
      border-bottom: 1px solid var(--border-color-light);
      position: relative;
    }

    .hour-slot:last-child {
      border-bottom: none;
    }

    .hour-slot.current-hour {
      background: var(--accent-50);
    }

    .hour-label {
      width: 80px;
      flex-shrink: 0;
      padding: var(--space-3) var(--space-2);
      font-size: 1rem;
      font-weight: 500;
      color: var(--text-secondary);
      font-variant-numeric: tabular-nums;
    }

    .hour-events {
      flex: 1;
      position: relative;
      padding: var(--space-2);
      display: flex;
      flex-direction: column;
      gap: var(--space-2);
    }

    .current-time-indicator {
      position: absolute;
      left: 0;
      right: 0;
      display: flex;
      align-items: center;
      z-index: 10;
      pointer-events: none;
    }

    .current-time-dot {
      width: 12px;
      height: 12px;
      background: var(--danger);
      border-radius: var(--radius-full);
      flex-shrink: 0;
      margin-left: -6px;
    }

    .current-time-line {
      flex: 1;
      height: 2px;
      background: var(--danger);
    }

    .day-event {
      background: var(--member-color-light, var(--surface-secondary));
      border-left: 4px solid var(--event-color, var(--accent-500));
      border-radius: var(--radius-md);
      padding: var(--space-3);
      transition: all var(--duration-quick) var(--ease-out);
    }

    .day-event.past {
      opacity: 0.6;
    }

    .day-event.current {
      box-shadow: var(--shadow-md);
      border-left-width: 6px;
    }

    .day-event.all-day {
      background: var(--accent-100);
    }

    .event-time {
      font-size: 0.875rem;
      font-weight: 500;
      color: var(--text-secondary);
      margin-bottom: var(--space-1);
    }

    .event-title {
      font-size: 1.125rem;
      font-weight: 500;
      color: var(--text-primary);
    }

    .event-location {
      font-size: 0.875rem;
      color: var(--text-secondary);
      margin-top: var(--space-1);
    }

    .event-members {
      display: flex;
      gap: var(--space-1);
      margin-top: var(--space-2);
    }

    .member-badge {
      display: inline-flex;
      align-items: center;
      justify-content: center;
      width: 24px;
      height: 24px;
      border-radius: var(--radius-full);
      font-size: 10px;
      font-weight: 600;
      color: white;
    }
  `],
})
export class DayViewComponent implements OnInit, OnDestroy {
  @Input() events: ScheduleEvent[] = [];
  @Input() members: MemberData[] = [];
  @Input() isLoading = false;
  @Input() selectedDate: Date = new Date();

  @Output() dateChange = new EventEmitter<Date>();

  private currentTimeInterval?: ReturnType<typeof setInterval>;
  private readonly _currentMinute = signal(new Date().getMinutes());

  // Hour slots from 6 AM to 10 PM for typical family schedule
  private readonly startHour = 6;
  private readonly endHour = 22;

  ngOnInit(): void {
    // Update current time every minute
    this.currentTimeInterval = setInterval(() => {
      this._currentMinute.set(new Date().getMinutes());
    }, 60000);
  }

  ngOnDestroy(): void {
    if (this.currentTimeInterval) {
      clearInterval(this.currentTimeInterval);
    }
  }

  readonly hourSlots = computed(() => {
    const slots: HourSlot[] = [];
    const now = new Date();
    const currentHour = now.getHours();
    const selectedDateStr = this.getDateKey(this.selectedDate);

    // Add all-day events slot at the top
    const allDayEvents = this.events.filter(e => this.isAllDayEvent(e));
    if (allDayEvents.length > 0) {
      slots.push({
        hour: -1,
        label: 'All Day',
        events: allDayEvents,
        isCurrentHour: false,
      });
    }

    for (let hour = this.startHour; hour <= this.endHour; hour++) {
      const hourEvents = this.events.filter(event => {
        if (this.isAllDayEvent(event)) return false;
        const eventDate = new Date(event.startTime);
        return eventDate.getHours() === hour &&
               this.getDateKey(eventDate) === selectedDateStr;
      });

      slots.push({
        hour,
        label: this.formatHour(hour),
        events: hourEvents,
        isCurrentHour: this.isToday() && hour === currentHour,
      });
    }

    return slots;
  });

  readonly currentMinutePercent = computed(() => {
    return (this._currentMinute() / 60) * 100;
  });

  isToday(): boolean {
    const today = new Date();
    return this.getDateKey(this.selectedDate) === this.getDateKey(today);
  }

  formatDateTitle(): string {
    if (this.isToday()) {
      return 'Today';
    }
    const tomorrow = new Date();
    tomorrow.setDate(tomorrow.getDate() + 1);
    if (this.getDateKey(this.selectedDate) === this.getDateKey(tomorrow)) {
      return 'Tomorrow';
    }
    const yesterday = new Date();
    yesterday.setDate(yesterday.getDate() - 1);
    if (this.getDateKey(this.selectedDate) === this.getDateKey(yesterday)) {
      return 'Yesterday';
    }
    return this.selectedDate.toLocaleDateString('en-US', { weekday: 'long' });
  }

  formatDateSubtitle(): string {
    return this.selectedDate.toLocaleDateString('en-US', {
      month: 'long',
      day: 'numeric',
      year: 'numeric',
    });
  }

  navigatePrevious(): void {
    const newDate = new Date(this.selectedDate);
    newDate.setDate(newDate.getDate() - 1);
    this.dateChange.emit(newDate);
  }

  navigateNext(): void {
    const newDate = new Date(this.selectedDate);
    newDate.setDate(newDate.getDate() + 1);
    this.dateChange.emit(newDate);
  }

  goToToday(): void {
    this.dateChange.emit(new Date());
  }

  getMember(memberId: string): MemberData | undefined {
    return this.members.find(m => m.id === memberId);
  }

  getEventColor(event: ScheduleEvent): string {
    if (event.color) return event.color;
    if (event.memberIds.length > 0) {
      const member = this.getMember(event.memberIds[0]);
      if (member) return member.color;
    }
    return 'var(--accent-500)';
  }

  formatTime(isoTime: string): string {
    try {
      const date = new Date(isoTime);
      return date.toLocaleTimeString('en-US', {
        hour: 'numeric',
        minute: '2-digit',
        hour12: true,
      });
    } catch {
      return isoTime;
    }
  }

  isAllDayEvent(event: ScheduleEvent): boolean {
    // Check if event spans full day or is marked as all-day
    const start = new Date(event.startTime);
    const end = event.endTime ? new Date(event.endTime) : null;

    if (!end) return false;

    const hours = (end.getTime() - start.getTime()) / (1000 * 60 * 60);
    return hours >= 23;
  }

  isEventNow(event: ScheduleEvent): boolean {
    const now = new Date();
    const start = new Date(event.startTime);
    const end = event.endTime ? new Date(event.endTime) : new Date(start.getTime() + 60 * 60 * 1000);
    return now >= start && now <= end;
  }

  isEventPast(event: ScheduleEvent): boolean {
    const now = new Date();
    const end = event.endTime ? new Date(event.endTime) : new Date(event.startTime);
    return end < now;
  }

  private formatHour(hour: number): string {
    if (hour === 0) return '12 AM';
    if (hour === 12) return '12 PM';
    if (hour < 12) return `${hour} AM`;
    return `${hour - 12} PM`;
  }

  private getDateKey(date: Date): string {
    return date.toISOString().split('T')[0];
  }
}
