import { Component, Input, Output, EventEmitter, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ScheduleEvent, MemberData } from '../../../../core/services/cache.service';

interface AgendaGroup {
  dateKey: string;
  label: string;
  relativeLabel: string;
  events: ScheduleEvent[];
  isToday: boolean;
  isPast: boolean;
}

@Component({
  selector: 'app-agenda-view',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="agenda-view">
      <!-- Header -->
      <div class="agenda-view-header">
        <h2 class="agenda-title">Upcoming Events</h2>
        <div class="agenda-range">
          {{ formatDateRange() }}
        </div>
      </div>

      <!-- Loading state -->
      @if (isLoading) {
        <div class="agenda-view-loading">
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
          <h3 class="display-empty-title">No upcoming events</h3>
          <p class="display-empty-description">
            Your calendar is clear for the next {{ daysToShow }} days.
          </p>
        </div>
      } @else {
        <!-- Agenda list -->
        <div class="agenda-list">
          @for (group of eventGroups(); track group.dateKey) {
            <div class="agenda-group" [class.today]="group.isToday" [class.past]="group.isPast">
              <div class="group-header">
                <div class="group-date">
                  <span class="date-label">{{ group.label }}</span>
                  @if (group.relativeLabel) {
                    <span class="date-relative">{{ group.relativeLabel }}</span>
                  }
                </div>
                <span class="event-count">{{ group.events.length }} event(s)</span>
              </div>

              <div class="group-events display-card">
                @for (event of group.events; track event.id) {
                  <div
                    class="agenda-event"
                    [style.--event-color]="getEventColor(event)"
                    [class.all-day]="isAllDayEvent(event)"
                    [class.past]="isEventPast(event)"
                    [class.current]="isEventNow(event)"
                    (click)="eventClick.emit(event)"
                  >
                    <div class="event-color-bar"></div>
                    <div class="event-time-block">
                      @if (isAllDayEvent(event)) {
                        <span class="event-time all-day">All Day</span>
                      } @else {
                        <span class="event-time">{{ formatTime(event.startTime) }}</span>
                        @if (event.endTime) {
                          <span class="event-time-end">{{ formatTime(event.endTime) }}</span>
                        }
                      }
                    </div>
                    <div class="event-details">
                      <div class="event-title">{{ event.title }}</div>
                      @if (event.location) {
                        <div class="event-location">
                          <svg xmlns="http://www.w3.org/2000/svg" width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                            <path d="M20 10c0 6-8 12-8 12s-8-6-8-12a8 8 0 0 1 16 0Z"/>
                            <circle cx="12" cy="10" r="3"/>
                          </svg>
                          <span>{{ event.location }}</span>
                        </div>
                      }
                    </div>
                    @if (event.memberIds.length > 0) {
                      <div class="event-members">
                        @for (memberId of event.memberIds; track memberId) {
                          @if (getMember(memberId); as member) {
                            <span
                              class="member-badge"
                              [style.background-color]="member.color"
                              [title]="member.name"
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
    .agenda-view {
      height: 100%;
      display: flex;
      flex-direction: column;
      gap: var(--space-4);
    }

    .agenda-view-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      padding: 0 var(--space-2);
    }

    .agenda-title {
      font-size: var(--font-display-sm);
      font-weight: 600;
      color: var(--text-primary);
    }

    .agenda-range {
      font-size: 1rem;
      color: var(--text-secondary);
    }

    .agenda-view-loading {
      flex: 1;
      display: flex;
      align-items: center;
      justify-content: center;
    }

    .agenda-list {
      flex: 1;
      overflow-y: auto;
      display: flex;
      flex-direction: column;
      gap: var(--space-6);
      padding-bottom: var(--space-8);
    }

    .agenda-group {
      display: flex;
      flex-direction: column;
      gap: var(--space-3);
    }

    .agenda-group.past {
      opacity: 0.7;
    }

    .group-header {
      display: flex;
      justify-content: space-between;
      align-items: flex-end;
      padding: 0 var(--space-2);
    }

    .group-date {
      display: flex;
      flex-direction: column;
    }

    .date-label {
      font-size: var(--font-glanceable);
      font-weight: 600;
      color: var(--text-primary);
    }

    .agenda-group.today .date-label {
      color: var(--accent-600);
    }

    .date-relative {
      font-size: 0.875rem;
      color: var(--text-secondary);
    }

    .event-count {
      font-size: 0.875rem;
      color: var(--text-tertiary);
    }

    .group-events {
      padding: 0 !important;
      overflow: hidden;
    }

    .agenda-event {
      display: flex;
      align-items: stretch;
      gap: var(--space-4);
      padding: var(--space-4);
      border-bottom: 1px solid var(--border-color-light);
      cursor: pointer;
      transition: background var(--duration-quick) var(--ease-out);
    }

    .agenda-event:last-child {
      border-bottom: none;
    }

    .agenda-event:hover {
      background: var(--surface-interactive);
    }

    .agenda-event:active {
      background: var(--surface-pressed);
    }

    .agenda-event.past {
      opacity: 0.6;
    }

    .agenda-event.current {
      background: var(--accent-50);
    }

    .event-color-bar {
      width: 4px;
      background: var(--event-color, var(--accent-500));
      border-radius: var(--radius-full);
      flex-shrink: 0;
    }

    .event-time-block {
      display: flex;
      flex-direction: column;
      min-width: 80px;
      flex-shrink: 0;
    }

    .event-time {
      font-size: 1.125rem;
      font-weight: 600;
      color: var(--text-primary);
      font-variant-numeric: tabular-nums;
    }

    .event-time.all-day {
      color: var(--accent-600);
    }

    .event-time-end {
      font-size: 0.875rem;
      color: var(--text-secondary);
      font-variant-numeric: tabular-nums;
    }

    .event-details {
      flex: 1;
      min-width: 0;
    }

    .event-title {
      font-size: 1.125rem;
      font-weight: 500;
      color: var(--text-primary);
      line-height: 1.3;
    }

    .event-location {
      display: flex;
      align-items: center;
      gap: var(--space-1);
      font-size: 0.875rem;
      color: var(--text-secondary);
      margin-top: var(--space-1);
    }

    .event-members {
      display: flex;
      gap: var(--space-1);
      align-items: center;
      flex-shrink: 0;
    }

    .member-badge {
      display: inline-flex;
      align-items: center;
      justify-content: center;
      width: 32px;
      height: 32px;
      border-radius: var(--radius-full);
      font-size: 12px;
      font-weight: 600;
      color: white;
    }
  `],
})
export class AgendaViewComponent {
  @Input() events: ScheduleEvent[] = [];
  @Input() members: MemberData[] = [];
  @Input() isLoading = false;
  @Input() daysToShow = 14;

  @Output() eventClick = new EventEmitter<ScheduleEvent>();

  readonly eventGroups = computed(() => {
    const groups: AgendaGroup[] = [];
    const today = new Date();
    const todayKey = this.getDateKey(today);

    // Group events by date
    const eventsByDate = new Map<string, ScheduleEvent[]>();

    for (const event of this.events) {
      const dateKey = this.getDateKey(new Date(event.startTime));
      const existing = eventsByDate.get(dateKey) || [];
      eventsByDate.set(dateKey, [...existing, event]);
    }

    // Sort each group's events
    for (const [dateKey, dateEvents] of eventsByDate) {
      eventsByDate.set(dateKey, dateEvents.sort((a, b) => {
        // All-day events first
        const aAllDay = this.isAllDayEvent(a) ? 0 : 1;
        const bAllDay = this.isAllDayEvent(b) ? 0 : 1;
        if (aAllDay !== bAllDay) return aAllDay - bAllDay;
        return new Date(a.startTime).getTime() - new Date(b.startTime).getTime();
      }));
    }

    // Sort dates and create groups
    const sortedDates = Array.from(eventsByDate.keys()).sort();

    for (const dateKey of sortedDates) {
      const date = new Date(dateKey + 'T00:00:00');
      const events = eventsByDate.get(dateKey)!;
      const isToday = dateKey === todayKey;
      const isPast = date < today && !isToday;

      groups.push({
        dateKey,
        label: this.formatDateLabel(date),
        relativeLabel: this.getRelativeLabel(date),
        events,
        isToday,
        isPast,
      });
    }

    return groups;
  });

  formatDateRange(): string {
    const start = new Date();
    const end = new Date();
    end.setDate(end.getDate() + this.daysToShow);

    return `${start.toLocaleDateString('en-US', { month: 'short', day: 'numeric' })} - ${end.toLocaleDateString('en-US', { month: 'short', day: 'numeric' })}`;
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

  private formatDateLabel(date: Date): string {
    return date.toLocaleDateString('en-US', {
      weekday: 'long',
      month: 'long',
      day: 'numeric',
    });
  }

  private getRelativeLabel(date: Date): string {
    const today = new Date();
    const todayKey = this.getDateKey(today);
    const dateKey = this.getDateKey(date);

    if (dateKey === todayKey) {
      return 'Today';
    }

    const tomorrow = new Date(today);
    tomorrow.setDate(tomorrow.getDate() + 1);
    if (dateKey === this.getDateKey(tomorrow)) {
      return 'Tomorrow';
    }

    const daysDiff = Math.ceil((date.getTime() - today.getTime()) / (1000 * 60 * 60 * 24));
    if (daysDiff > 0 && daysDiff <= 7) {
      return `In ${daysDiff} days`;
    }

    if (daysDiff < 0) {
      const absDiff = Math.abs(daysDiff);
      return absDiff === 1 ? 'Yesterday' : `${absDiff} days ago`;
    }

    return '';
  }

  private getDateKey(date: Date): string {
    return date.toISOString().split('T')[0];
  }
}
