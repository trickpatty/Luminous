import { Component, Input, Output, EventEmitter, computed, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ScheduleEvent, MemberData } from '../../../../core/services/cache.service';

interface DayColumn {
  date: Date;
  dateKey: string;
  dayName: string;
  dayNumber: number;
  isToday: boolean;
  isPast: boolean;
  events: ScheduleEvent[];
}

@Component({
  selector: 'app-week-view',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="week-view">
      <!-- Header with week navigation -->
      <div class="week-view-header">
        <button class="nav-btn" (click)="navigatePreviousWeek()" aria-label="Previous week">
          <svg xmlns="http://www.w3.org/2000/svg" width="28" height="28" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
            <path d="m15 18-6-6 6-6"/>
          </svg>
        </button>
        <div class="week-view-date">
          <h2 class="date-title">{{ formatWeekTitle() }}</h2>
        </div>
        <button class="nav-btn" (click)="navigateNextWeek()" aria-label="Next week">
          <svg xmlns="http://www.w3.org/2000/svg" width="28" height="28" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
            <path d="m9 18 6-6-6-6"/>
          </svg>
        </button>
      </div>

      <!-- This week button -->
      @if (!isCurrentWeek()) {
        <button class="today-btn" (click)="goToCurrentWeek()">
          This Week
        </button>
      }

      <!-- Loading state -->
      @if (isLoading) {
        <div class="week-view-loading">
          <div class="display-spinner display-spinner-lg"></div>
        </div>
      } @else {
        <!-- Week grid -->
        <div class="week-grid display-card">
          <!-- Day headers -->
          <div class="week-header">
            @for (day of weekDays(); track day.dateKey) {
              <button
                class="day-header"
                [class.today]="day.isToday"
                [class.past]="day.isPast"
                [class.selected]="isSelectedDay(day.date)"
                (click)="selectDay(day.date)"
              >
                <span class="day-name">{{ day.dayName }}</span>
                <span class="day-number" [class.today-badge]="day.isToday">
                  {{ day.dayNumber }}
                </span>
              </button>
            }
          </div>

          <!-- Day columns -->
          <div class="week-body">
            @for (day of weekDays(); track day.dateKey) {
              <div
                class="day-column"
                [class.today]="day.isToday"
                [class.past]="day.isPast"
                [class.selected]="isSelectedDay(day.date)"
                (click)="selectDay(day.date)"
              >
                @if (day.events.length === 0) {
                  <div class="day-empty">
                    <span class="day-empty-text">No events</span>
                  </div>
                } @else {
                  @for (event of day.events.slice(0, maxEventsPerDay); track event.id) {
                    <div
                      class="week-event"
                      [style.--event-color]="getEventColor(event)"
                      [class.all-day]="isAllDayEvent(event)"
                    >
                      <div class="event-time">
                        @if (isAllDayEvent(event)) {
                          All day
                        } @else {
                          {{ formatTime(event.startTime) }}
                        }
                      </div>
                      <div class="event-title">{{ event.title }}</div>
                      @if (event.memberIds.length > 0) {
                        <div class="event-members">
                          @for (memberId of event.memberIds.slice(0, 3); track memberId) {
                            @if (getMember(memberId); as member) {
                              <span
                                class="member-dot"
                                [style.background-color]="member.color"
                                [title]="member.name"
                              ></span>
                            }
                          }
                          @if (event.memberIds.length > 3) {
                            <span class="member-more">+{{ event.memberIds.length - 3 }}</span>
                          }
                        </div>
                      }
                    </div>
                  }
                  @if (day.events.length > maxEventsPerDay) {
                    <div class="more-events">
                      +{{ day.events.length - maxEventsPerDay }} more
                    </div>
                  }
                }
              </div>
            }
          </div>
        </div>
      }
    </div>
  `,
  styles: [`
    .week-view {
      height: 100%;
      display: flex;
      flex-direction: column;
      gap: var(--space-4);
    }

    .week-view-header {
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

    .week-view-date {
      text-align: center;
    }

    .date-title {
      font-size: var(--font-glanceable);
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

    .week-view-loading {
      flex: 1;
      display: flex;
      align-items: center;
      justify-content: center;
    }

    .week-grid {
      flex: 1;
      display: flex;
      flex-direction: column;
      overflow: hidden;
      padding: var(--space-4) !important;
    }

    .week-header {
      display: grid;
      grid-template-columns: repeat(7, 1fr);
      gap: var(--space-2);
      margin-bottom: var(--space-4);
    }

    .day-header {
      display: flex;
      flex-direction: column;
      align-items: center;
      gap: var(--space-1);
      padding: var(--space-2);
      background: transparent;
      border: none;
      border-radius: var(--radius-lg);
      cursor: pointer;
      transition: all var(--duration-quick) var(--ease-out);
    }

    .day-header:active {
      background: var(--surface-pressed);
    }

    .day-header.today .day-number {
      background: var(--accent-500);
      color: white;
    }

    .day-header.past {
      opacity: 0.6;
    }

    .day-header.selected {
      background: var(--accent-100);
    }

    .day-name {
      font-size: 0.875rem;
      font-weight: 500;
      color: var(--text-secondary);
      text-transform: uppercase;
    }

    .day-number {
      font-size: 1.25rem;
      font-weight: 600;
      color: var(--text-primary);
      width: 40px;
      height: 40px;
      display: flex;
      align-items: center;
      justify-content: center;
      border-radius: var(--radius-full);
    }

    .week-body {
      flex: 1;
      display: grid;
      grid-template-columns: repeat(7, 1fr);
      gap: var(--space-2);
      overflow-y: auto;
    }

    .day-column {
      background: var(--surface-secondary);
      border-radius: var(--radius-lg);
      padding: var(--space-2);
      min-height: 200px;
      display: flex;
      flex-direction: column;
      gap: var(--space-2);
      cursor: pointer;
      transition: all var(--duration-quick) var(--ease-out);
    }

    .day-column:hover {
      background: var(--surface-interactive);
    }

    .day-column.today {
      background: var(--accent-50);
      border: 2px solid var(--accent-200);
    }

    .day-column.past {
      opacity: 0.7;
    }

    .day-column.selected {
      border: 2px solid var(--accent-500);
    }

    .day-empty {
      flex: 1;
      display: flex;
      align-items: center;
      justify-content: center;
    }

    .day-empty-text {
      font-size: 0.75rem;
      color: var(--text-tertiary);
    }

    .week-event {
      background: var(--surface-primary);
      border-left: 3px solid var(--event-color, var(--accent-500));
      border-radius: var(--radius-sm);
      padding: var(--space-2);
    }

    .week-event.all-day {
      background: var(--accent-100);
    }

    .event-time {
      font-size: 0.75rem;
      font-weight: 500;
      color: var(--text-secondary);
    }

    .event-title {
      font-size: 0.875rem;
      font-weight: 500;
      color: var(--text-primary);
      line-height: 1.2;
      overflow: hidden;
      text-overflow: ellipsis;
      white-space: nowrap;
    }

    .event-members {
      display: flex;
      gap: 2px;
      margin-top: var(--space-1);
    }

    .member-dot {
      width: 8px;
      height: 8px;
      border-radius: var(--radius-full);
    }

    .member-more {
      font-size: 0.625rem;
      color: var(--text-tertiary);
      margin-left: 2px;
    }

    .more-events {
      font-size: 0.75rem;
      font-weight: 500;
      color: var(--accent-600);
      text-align: center;
      padding: var(--space-1);
    }
  `],
})
export class WeekViewComponent {
  @Input() events: ScheduleEvent[] = [];
  @Input() members: MemberData[] = [];
  @Input() isLoading = false;
  @Input() selectedDate: Date = new Date();

  @Output() dateChange = new EventEmitter<Date>();
  @Output() daySelect = new EventEmitter<Date>();

  readonly maxEventsPerDay = 4;

  readonly weekDays = computed(() => {
    const days: DayColumn[] = [];
    const weekStart = this.getWeekStart(this.selectedDate);
    const today = new Date();
    const todayKey = this.getDateKey(today);

    for (let i = 0; i < 7; i++) {
      const date = new Date(weekStart);
      date.setDate(date.getDate() + i);
      const dateKey = this.getDateKey(date);

      const dayEvents = this.events
        .filter(event => {
          const eventDateKey = this.getDateKey(new Date(event.startTime));
          return eventDateKey === dateKey;
        })
        .sort((a, b) => {
          // All-day events first, then by start time
          const aAllDay = this.isAllDayEvent(a) ? 0 : 1;
          const bAllDay = this.isAllDayEvent(b) ? 0 : 1;
          if (aAllDay !== bAllDay) return aAllDay - bAllDay;
          return new Date(a.startTime).getTime() - new Date(b.startTime).getTime();
        });

      days.push({
        date,
        dateKey,
        dayName: date.toLocaleDateString('en-US', { weekday: 'short' }),
        dayNumber: date.getDate(),
        isToday: dateKey === todayKey,
        isPast: date < today && dateKey !== todayKey,
        events: dayEvents,
      });
    }

    return days;
  });

  isCurrentWeek(): boolean {
    const today = new Date();
    const currentWeekStart = this.getWeekStart(today);
    const selectedWeekStart = this.getWeekStart(this.selectedDate);
    return this.getDateKey(currentWeekStart) === this.getDateKey(selectedWeekStart);
  }

  isSelectedDay(date: Date): boolean {
    return this.getDateKey(date) === this.getDateKey(this.selectedDate);
  }

  formatWeekTitle(): string {
    const weekStart = this.getWeekStart(this.selectedDate);
    const weekEnd = new Date(weekStart);
    weekEnd.setDate(weekEnd.getDate() + 6);

    const startMonth = weekStart.toLocaleDateString('en-US', { month: 'short' });
    const endMonth = weekEnd.toLocaleDateString('en-US', { month: 'short' });

    if (startMonth === endMonth) {
      return `${startMonth} ${weekStart.getDate()} - ${weekEnd.getDate()}, ${weekEnd.getFullYear()}`;
    }
    return `${startMonth} ${weekStart.getDate()} - ${endMonth} ${weekEnd.getDate()}, ${weekEnd.getFullYear()}`;
  }

  navigatePreviousWeek(): void {
    const newDate = new Date(this.selectedDate);
    newDate.setDate(newDate.getDate() - 7);
    this.dateChange.emit(newDate);
  }

  navigateNextWeek(): void {
    const newDate = new Date(this.selectedDate);
    newDate.setDate(newDate.getDate() + 7);
    this.dateChange.emit(newDate);
  }

  goToCurrentWeek(): void {
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
    const start = new Date(event.startTime);
    const end = event.endTime ? new Date(event.endTime) : null;
    if (!end) return false;
    const hours = (end.getTime() - start.getTime()) / (1000 * 60 * 60);
    return hours >= 23;
  }

  private getWeekStart(date: Date): Date {
    const d = new Date(date);
    const day = d.getDay();
    const diff = d.getDate() - day; // Sunday as start of week
    d.setDate(diff);
    d.setHours(0, 0, 0, 0);
    return d;
  }

  private getDateKey(date: Date): string {
    return date.toISOString().split('T')[0];
  }
}
