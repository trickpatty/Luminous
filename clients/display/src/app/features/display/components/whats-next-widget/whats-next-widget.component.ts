import { Component, Input, computed, inject, signal, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ScheduleEvent, MemberData } from '../../../../core/services/cache.service';
import { EventService } from '../../../../core/services/event.service';
import { ElectronService } from '../../../../core/services/electron.service';

/**
 * "What's Next" Widget - Shows the next upcoming event at a glance.
 * Displays the event title, time, location, and assigned member(s).
 * Updates automatically as time passes.
 */
@Component({
  selector: 'app-whats-next-widget',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="whats-next-widget" [class.has-event]="nextEvent()">
      @if (nextEvent(); as event) {
        <div class="widget-label">Up Next</div>
        <div class="event-content">
          <div class="event-time-wrapper">
            <div class="event-time">{{ event.isAllDay ? 'All Day' : formatTime(event.startTime) }}</div>
            @if (timeUntil()) {
              <div class="time-until">{{ timeUntil() }}</div>
            }
          </div>
          <div class="event-details">
            <div class="event-title">{{ event.title }}</div>
            @if (event.location) {
              <div class="event-location">
                <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                  <path d="M20 10c0 6-8 12-8 12s-8-6-8-12a8 8 0 0 1 16 0Z"/>
                  <circle cx="12" cy="10" r="3"/>
                </svg>
                <span>{{ event.location }}</span>
              </div>
            }
          </div>
          @if (eventMembers().length > 0) {
            <div class="event-members">
              @for (member of eventMembers(); track member.id) {
                <div
                  class="member-avatar"
                  [style.background-color]="member.color"
                  [attr.aria-label]="member.name"
                >
                  {{ member.initials }}
                </div>
              }
            </div>
          }
        </div>
        <div
          class="event-indicator"
          [style.background-color]="eventColor()"
        ></div>
      } @else {
        <div class="empty-state">
          <svg xmlns="http://www.w3.org/2000/svg" width="48" height="48" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round">
            <circle cx="12" cy="12" r="10"/>
            <polyline points="12 6 12 12 16 14"/>
          </svg>
          <div class="empty-text">All caught up!</div>
          <div class="empty-subtext">No upcoming events</div>
        </div>
      }
    </div>
  `,
  styles: [`
    .whats-next-widget {
      position: relative;
      background: var(--surface-primary);
      border-radius: var(--radius-xl);
      padding: var(--space-5);
      min-height: 140px;
      display: flex;
      flex-direction: column;
      overflow: hidden;
      box-shadow: var(--shadow-sm);
    }

    .whats-next-widget.has-event {
      padding-left: var(--space-6);
    }

    .widget-label {
      font-size: 0.875rem;
      font-weight: 600;
      text-transform: uppercase;
      letter-spacing: 0.05em;
      color: var(--text-tertiary);
      margin-bottom: var(--space-2);
    }

    .event-content {
      display: flex;
      flex-direction: column;
      gap: var(--space-2);
      flex: 1;
    }

    .event-time-wrapper {
      display: flex;
      align-items: baseline;
      gap: var(--space-3);
    }

    .event-time {
      font-size: var(--font-display-sm);
      font-weight: 600;
      color: var(--text-primary);
      font-variant-numeric: tabular-nums;
    }

    .time-until {
      font-size: 1rem;
      font-weight: 500;
      color: var(--accent-600);
      background: var(--accent-100);
      padding: var(--space-1) var(--space-3);
      border-radius: var(--radius-full);
    }

    .event-details {
      display: flex;
      flex-direction: column;
      gap: var(--space-1);
    }

    .event-title {
      font-size: var(--font-glanceable);
      font-weight: 500;
      color: var(--text-primary);
      line-height: 1.3;
    }

    .event-location {
      display: flex;
      align-items: center;
      gap: var(--space-1);
      font-size: 1rem;
      color: var(--text-secondary);
    }

    .event-location svg {
      flex-shrink: 0;
    }

    .event-members {
      display: flex;
      gap: var(--space-1);
      margin-top: var(--space-2);
    }

    .member-avatar {
      width: 32px;
      height: 32px;
      border-radius: var(--radius-full);
      display: flex;
      align-items: center;
      justify-content: center;
      color: white;
      font-size: 0.75rem;
      font-weight: 600;
    }

    .event-indicator {
      position: absolute;
      left: 0;
      top: 0;
      bottom: 0;
      width: 6px;
      border-radius: var(--radius-xl) 0 0 var(--radius-xl);
    }

    .empty-state {
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: center;
      flex: 1;
      gap: var(--space-2);
      color: var(--text-tertiary);
    }

    .empty-state svg {
      opacity: 0.5;
    }

    .empty-text {
      font-size: 1.25rem;
      font-weight: 500;
      color: var(--text-secondary);
    }

    .empty-subtext {
      font-size: 1rem;
      color: var(--text-tertiary);
    }
  `],
})
export class WhatsNextWidgetComponent implements OnInit, OnDestroy {
  private readonly eventService = inject(EventService);
  private readonly electronService = inject(ElectronService);

  @Input() events: ScheduleEvent[] = [];
  @Input() members: MemberData[] = [];

  protected readonly timeUntil = signal<string>('');

  private intervalId?: ReturnType<typeof setInterval>;
  private use24Hour = false;

  /**
   * Get the next upcoming event (not in the past, sorted by start time)
   */
  protected readonly nextEvent = computed(() => {
    const now = new Date();
    const todayStr = this.formatDateStr(now);

    const upcoming = this.events
      .filter(event => {
        if (event.isAllDay && event.startDate) {
          // All-day event: check if end date is today or later
          const endDate = event.endDate || event.startDate;
          return endDate > todayStr;
        } else if (event.startTime) {
          // Timed event: check if end time is in the future
          const endTime = event.endTime ? new Date(event.endTime) : new Date(event.startTime);
          return endTime > now;
        }
        return false;
      })
      .sort((a, b) => {
        // All-day events first, then by start time/date
        if (a.isAllDay && !b.isAllDay) return -1;
        if (!a.isAllDay && b.isAllDay) return 1;
        if (a.isAllDay && b.isAllDay) {
          return (a.startDate || '') < (b.startDate || '') ? -1 : 1;
        }
        return new Date(a.startTime!).getTime() - new Date(b.startTime!).getTime();
      });

    return upcoming[0] || null;
  });

  /**
   * Format a Date to YYYY-MM-DD string (local date)
   */
  private formatDateStr(date: Date): string {
    const year = date.getFullYear();
    const month = String(date.getMonth() + 1).padStart(2, '0');
    const day = String(date.getDate()).padStart(2, '0');
    return `${year}-${month}-${day}`;
  }

  /**
   * Get the members assigned to the next event
   */
  protected readonly eventMembers = computed(() => {
    const event = this.nextEvent();
    if (!event || event.memberIds.length === 0) return [];

    return this.members.filter(m => event.memberIds.includes(m.id));
  });

  /**
   * Get the color for the event indicator
   */
  protected readonly eventColor = computed(() => {
    const event = this.nextEvent();
    if (!event) return 'var(--accent-500)';

    return this.eventService.getEventColor(event, this.members);
  });

  async ngOnInit(): Promise<void> {
    const settings = await this.electronService.getSettings();
    this.use24Hour = settings.timeFormat === '24h';

    this.updateTimeUntil();
    this.intervalId = setInterval(() => this.updateTimeUntil(), 60000); // Update every minute
  }

  ngOnDestroy(): void {
    if (this.intervalId) {
      clearInterval(this.intervalId);
    }
  }

  protected formatTime(isoTime: string | null | undefined): string {
    return this.eventService.formatTime(isoTime, this.use24Hour ? '24h' : '12h');
  }

  private updateTimeUntil(): void {
    const event = this.nextEvent();
    if (!event) {
      this.timeUntil.set('');
      return;
    }

    // For all-day events, don't show "time until"
    if (event.isAllDay) {
      this.timeUntil.set('');
      return;
    }

    if (!event.startTime) {
      this.timeUntil.set('');
      return;
    }

    const now = new Date();
    const eventStart = new Date(event.startTime);
    const diffMs = eventStart.getTime() - now.getTime();

    if (diffMs <= 0) {
      // Event is happening now
      this.timeUntil.set('Now');
      return;
    }

    const diffMins = Math.floor(diffMs / (1000 * 60));
    const diffHours = Math.floor(diffMins / 60);
    const diffDays = Math.floor(diffHours / 24);

    if (diffDays > 0) {
      this.timeUntil.set(`in ${diffDays}d ${diffHours % 24}h`);
    } else if (diffHours > 0) {
      this.timeUntil.set(`in ${diffHours}h ${diffMins % 60}m`);
    } else if (diffMins > 0) {
      this.timeUntil.set(`in ${diffMins}m`);
    } else {
      this.timeUntil.set('Starting soon');
    }
  }
}
