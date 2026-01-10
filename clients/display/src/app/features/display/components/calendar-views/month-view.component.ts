import { Component, Input, Output, EventEmitter, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ScheduleEvent, MemberData } from '../../../../core/services/cache.service';

interface CalendarDay {
  date: Date;
  dateKey: string;
  dayNumber: number;
  isToday: boolean;
  isCurrentMonth: boolean;
  isPast: boolean;
  events: ScheduleEvent[];
}

interface CalendarWeek {
  days: CalendarDay[];
}

@Component({
  selector: 'app-month-view',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="month-view">
      <!-- Header with month navigation -->
      <div class="month-view-header">
        <button class="nav-btn" (click)="navigatePreviousMonth()" aria-label="Previous month">
          <svg xmlns="http://www.w3.org/2000/svg" width="28" height="28" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
            <path d="m15 18-6-6 6-6"/>
          </svg>
        </button>
        <div class="month-view-date">
          <h2 class="date-title">{{ formatMonthTitle() }}</h2>
        </div>
        <button class="nav-btn" (click)="navigateNextMonth()" aria-label="Next month">
          <svg xmlns="http://www.w3.org/2000/svg" width="28" height="28" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
            <path d="m9 18 6-6-6-6"/>
          </svg>
        </button>
      </div>

      <!-- Today button -->
      @if (!isCurrentMonth()) {
        <button class="today-btn" (click)="goToCurrentMonth()">
          Today
        </button>
      }

      <!-- Loading state -->
      @if (isLoading) {
        <div class="month-view-loading">
          <div class="display-spinner display-spinner-lg"></div>
        </div>
      } @else {
        <!-- Calendar grid -->
        <div class="month-grid display-card">
          <!-- Weekday headers -->
          <div class="weekday-header">
            @for (day of weekdayNames; track day) {
              <div class="weekday-name">{{ day }}</div>
            }
          </div>

          <!-- Calendar weeks -->
          <div class="calendar-body">
            @for (week of calendarWeeks(); track $index) {
              <div class="calendar-week">
                @for (day of week.days; track day.dateKey) {
                  <button
                    class="calendar-day"
                    [class.today]="day.isToday"
                    [class.other-month]="!day.isCurrentMonth"
                    [class.past]="day.isPast && day.isCurrentMonth"
                    [class.selected]="isSelectedDay(day.date)"
                    [class.has-events]="day.events.length > 0"
                    (click)="selectDay(day.date)"
                  >
                    <span class="day-number" [class.today-badge]="day.isToday">
                      {{ day.dayNumber }}
                    </span>
                    @if (day.events.length > 0) {
                      <div class="day-events">
                        @for (event of day.events.slice(0, maxEventsPerDay); track event.id) {
                          <div
                            class="event-dot"
                            [style.background-color]="getEventColor(event)"
                            [title]="event.title"
                          ></div>
                        }
                        @if (day.events.length > maxEventsPerDay) {
                          <span class="events-more">+{{ day.events.length - maxEventsPerDay }}</span>
                        }
                      </div>
                    }
                  </button>
                }
              </div>
            }
          </div>
        </div>

        <!-- Selected day preview -->
        @if (selectedDayEvents().length > 0) {
          <div class="selected-day-preview display-card">
            <div class="preview-header">
              <span class="preview-date">{{ formatSelectedDate() }}</span>
              <span class="preview-count">{{ selectedDayEvents().length }} event(s)</span>
            </div>
            <div class="preview-events">
              @for (event of selectedDayEvents().slice(0, 3); track event.id) {
                <div class="preview-event" [style.--event-color]="getEventColor(event)">
                  <span class="event-time">
                    @if (isAllDayEvent(event)) {
                      All day
                    } @else {
                      {{ formatTime(event.startTime) }}
                    }
                  </span>
                  <span class="event-title">{{ event.title }}</span>
                </div>
              }
              @if (selectedDayEvents().length > 3) {
                <div class="preview-more">
                  +{{ selectedDayEvents().length - 3 }} more events
                </div>
              }
            </div>
          </div>
        }
      }
    </div>
  `,
  styles: [`
    .month-view {
      height: 100%;
      display: flex;
      flex-direction: column;
      gap: var(--space-4);
      min-height: 0;
      overflow: hidden;
    }

    .month-view-header {
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

    .month-view-date {
      text-align: center;
    }

    .date-title {
      font-size: var(--font-display-sm);
      font-weight: 600;
      color: var(--text-primary);
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

    .month-view-loading {
      flex: 1;
      display: flex;
      align-items: center;
      justify-content: center;
    }

    .month-grid {
      flex: 1;
      display: flex;
      flex-direction: column;
      padding: var(--space-4) !important;
      overflow: hidden;
    }

    .weekday-header {
      display: grid;
      grid-template-columns: repeat(7, 1fr);
      gap: var(--space-1);
      margin-bottom: var(--space-2);
    }

    .weekday-name {
      text-align: center;
      font-size: 0.875rem;
      font-weight: 600;
      color: var(--text-secondary);
      text-transform: uppercase;
      padding: var(--space-2);
    }

    .calendar-body {
      flex: 1;
      display: flex;
      flex-direction: column;
      gap: var(--space-1);
    }

    .calendar-week {
      display: grid;
      grid-template-columns: repeat(7, 1fr);
      gap: var(--space-1);
      flex: 1;
    }

    .calendar-day {
      display: flex;
      flex-direction: column;
      align-items: center;
      padding: var(--space-2);
      background: transparent;
      border: 2px solid transparent;
      border-radius: var(--radius-lg);
      cursor: pointer;
      transition: all var(--duration-quick) var(--ease-out);
      min-height: 70px;
    }

    .calendar-day:hover {
      background: var(--surface-interactive);
    }

    .calendar-day:active {
      background: var(--surface-pressed);
    }

    .calendar-day.today {
      background: var(--accent-50);
    }

    .calendar-day.today .day-number {
      background: var(--accent-500);
      color: white;
    }

    .calendar-day.other-month {
      opacity: 0.4;
    }

    .calendar-day.past:not(.today) {
      opacity: 0.6;
    }

    .calendar-day.selected {
      border-color: var(--accent-500);
      background: var(--accent-50);
    }

    .day-number {
      font-size: 1.125rem;
      font-weight: 600;
      color: var(--text-primary);
      width: 36px;
      height: 36px;
      display: flex;
      align-items: center;
      justify-content: center;
      border-radius: var(--radius-full);
    }

    .day-events {
      display: flex;
      align-items: center;
      gap: 3px;
      margin-top: var(--space-1);
      flex-wrap: wrap;
      justify-content: center;
    }

    .event-dot {
      width: 8px;
      height: 8px;
      border-radius: var(--radius-full);
    }

    .events-more {
      font-size: 0.625rem;
      color: var(--text-tertiary);
    }

    .selected-day-preview {
      padding: var(--space-4) !important;
    }

    .preview-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: var(--space-3);
    }

    .preview-date {
      font-size: 1.125rem;
      font-weight: 600;
      color: var(--text-primary);
    }

    .preview-count {
      font-size: 0.875rem;
      color: var(--text-secondary);
    }

    .preview-events {
      display: flex;
      flex-direction: column;
      gap: var(--space-2);
    }

    .preview-event {
      display: flex;
      align-items: center;
      gap: var(--space-3);
      padding: var(--space-2);
      background: var(--surface-secondary);
      border-left: 3px solid var(--event-color, var(--accent-500));
      border-radius: var(--radius-md);
    }

    .event-time {
      font-size: 0.875rem;
      font-weight: 500;
      color: var(--text-secondary);
      min-width: 70px;
    }

    .event-title {
      font-size: 1rem;
      font-weight: 500;
      color: var(--text-primary);
      flex: 1;
      overflow: hidden;
      text-overflow: ellipsis;
      white-space: nowrap;
    }

    .preview-more {
      font-size: 0.875rem;
      color: var(--accent-600);
      text-align: center;
      padding: var(--space-2);
    }
  `],
})
export class MonthViewComponent {
  @Input() events: ScheduleEvent[] = [];
  @Input() members: MemberData[] = [];
  @Input() isLoading = false;
  @Input() selectedDate: Date = new Date();

  @Output() dateChange = new EventEmitter<Date>();
  @Output() daySelect = new EventEmitter<Date>();

  readonly maxEventsPerDay = 3;
  readonly weekdayNames = ['Sun', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat'];

  readonly calendarWeeks = computed(() => {
    const weeks: CalendarWeek[] = [];
    const year = this.selectedDate.getFullYear();
    const month = this.selectedDate.getMonth();

    const firstDayOfMonth = new Date(year, month, 1);
    const lastDayOfMonth = new Date(year, month + 1, 0);

    const today = new Date();
    const todayKey = this.getDateKey(today);

    // Start from the Sunday before (or on) the first day of month
    const startDate = new Date(firstDayOfMonth);
    startDate.setDate(startDate.getDate() - startDate.getDay());

    // End on the Saturday after (or on) the last day of month
    const endDate = new Date(lastDayOfMonth);
    endDate.setDate(endDate.getDate() + (6 - endDate.getDay()));

    let currentDate = new Date(startDate);

    while (currentDate <= endDate) {
      const week: CalendarDay[] = [];

      for (let i = 0; i < 7; i++) {
        const date = new Date(currentDate);
        const dateKey = this.getDateKey(date);
        const isCurrentMonth = date.getMonth() === month;

        const dayEvents = this.events
          .filter(event => this.getEventDateKey(event) === dateKey)
          .sort((a, b) => {
            if (a.isAllDay && !b.isAllDay) return -1;
            if (!a.isAllDay && b.isAllDay) return 1;
            if (a.isAllDay && b.isAllDay) {
              return (a.startDate || '') < (b.startDate || '') ? -1 : 1;
            }
            return new Date(a.startTime!).getTime() - new Date(b.startTime!).getTime();
          });

        week.push({
          date,
          dateKey,
          dayNumber: date.getDate(),
          isToday: dateKey === todayKey,
          isCurrentMonth,
          isPast: date < today && dateKey !== todayKey,
          events: dayEvents,
        });

        currentDate.setDate(currentDate.getDate() + 1);
      }

      weeks.push({ days: week });
    }

    return weeks;
  });

  readonly selectedDayEvents = computed(() => {
    const dateKey = this.getDateKey(this.selectedDate);
    return this.events
      .filter(event => this.getEventDateKey(event) === dateKey)
      .sort((a, b) => {
        if (a.isAllDay && !b.isAllDay) return -1;
        if (!a.isAllDay && b.isAllDay) return 1;
        if (a.isAllDay && b.isAllDay) {
          return (a.startDate || '') < (b.startDate || '') ? -1 : 1;
        }
        return new Date(a.startTime!).getTime() - new Date(b.startTime!).getTime();
      });
  });

  isCurrentMonth(): boolean {
    const today = new Date();
    return today.getMonth() === this.selectedDate.getMonth() &&
           today.getFullYear() === this.selectedDate.getFullYear();
  }

  isSelectedDay(date: Date): boolean {
    return this.getDateKey(date) === this.getDateKey(this.selectedDate);
  }

  formatMonthTitle(): string {
    return this.selectedDate.toLocaleDateString('en-US', {
      month: 'long',
      year: 'numeric',
    });
  }

  formatSelectedDate(): string {
    return this.selectedDate.toLocaleDateString('en-US', {
      weekday: 'long',
      month: 'short',
      day: 'numeric',
    });
  }

  navigatePreviousMonth(): void {
    const newDate = new Date(this.selectedDate);
    newDate.setMonth(newDate.getMonth() - 1);
    this.dateChange.emit(newDate);
  }

  navigateNextMonth(): void {
    const newDate = new Date(this.selectedDate);
    newDate.setMonth(newDate.getMonth() + 1);
    this.dateChange.emit(newDate);
  }

  goToCurrentMonth(): void {
    this.dateChange.emit(new Date());
  }

  selectDay(date: Date): void {
    this.daySelect.emit(date);
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

  formatTime(isoTime: string | null | undefined): string {
    if (!isoTime) return '';
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
    return event.isAllDay;
  }

  private getEventDateKey(event: ScheduleEvent): string | null {
    if (event.isAllDay && event.startDate) {
      return event.startDate;
    } else if (event.startTime) {
      return this.getDateKey(new Date(event.startTime));
    }
    return null;
  }

  private getDateKey(date: Date): string {
    return date.toISOString().split('T')[0];
  }
}
