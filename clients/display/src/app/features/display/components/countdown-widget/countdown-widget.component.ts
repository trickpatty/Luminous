import { Component, Input, computed, signal, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ScheduleEvent, MemberData } from '../../../../core/services/cache.service';

/**
 * Countdown item with computed days remaining
 */
interface CountdownItem {
  event: ScheduleEvent;
  daysRemaining: number;
  emoji: string;
}

/**
 * Event keywords that trigger special emojis
 */
const EVENT_EMOJIS: Record<string, string> = {
  birthday: '🎂',
  party: '🎉',
  vacation: '✈️',
  trip: '✈️',
  holiday: '🎄',
  christmas: '🎄',
  halloween: '🎃',
  thanksgiving: '🦃',
  easter: '🐰',
  wedding: '💒',
  anniversary: '💕',
  graduation: '🎓',
  concert: '🎵',
  game: '🏀',
  match: '⚽',
  recital: '🎭',
  competition: '🏆',
  camp: '⛺',
  school: '📚',
};

/**
 * Countdown Widget - Shows countdowns to upcoming major events.
 * Displays up to 3 events with the largest days remaining first,
 * then sorted by date for events within the same day count.
 */
@Component({
  selector: 'app-countdown-widget',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="countdown-widget">
      <div class="widget-header">
        <div class="widget-label">Countdowns</div>
      </div>

      @if (countdowns().length > 0) {
        <div class="countdowns-list">
          @for (item of countdowns(); track item.event.id) {
            <div
              class="countdown-item"
              [style.border-left-color]="getEventColor(item.event)"
            >
              <div class="countdown-emoji">{{ item.emoji }}</div>
              <div class="countdown-info">
                <div class="countdown-title">{{ item.event.title }}</div>
                <div class="countdown-date">{{ formatEventDate(item.event) }}</div>
              </div>
              <div class="countdown-days">
                <div class="days-number">{{ item.daysRemaining }}</div>
                <div class="days-label">{{ item.daysRemaining === 1 ? 'day' : 'days' }}</div>
              </div>
            </div>
          }
        </div>
      } @else {
        <div class="empty-state">
          <div class="empty-text">No upcoming events</div>
          <div class="empty-subtext">Add events to see countdowns</div>
        </div>
      }
    </div>
  `,
  styles: [`
    .countdown-widget {
      background: var(--surface-primary);
      border-radius: var(--radius-xl);
      padding: var(--space-5);
      box-shadow: var(--shadow-sm);
    }

    .widget-header {
      margin-bottom: var(--space-4);
    }

    .widget-label {
      font-size: 0.875rem;
      font-weight: 600;
      text-transform: uppercase;
      letter-spacing: 0.05em;
      color: var(--text-tertiary);
    }

    .countdowns-list {
      display: flex;
      flex-direction: column;
      gap: var(--space-3);
    }

    .countdown-item {
      display: flex;
      align-items: center;
      gap: var(--space-3);
      padding: var(--space-3) var(--space-4);
      background: var(--surface-secondary);
      border-radius: var(--radius-lg);
      border-left: 4px solid var(--accent-500);
      transition: transform var(--duration-quick) var(--ease-out);
    }

    .countdown-item:active {
      transform: scale(0.98);
    }

    .countdown-emoji {
      font-size: 2rem;
      line-height: 1;
      flex-shrink: 0;
    }

    .countdown-info {
      flex: 1;
      min-width: 0;
    }

    .countdown-title {
      font-size: 1.125rem;
      font-weight: 500;
      color: var(--text-primary);
      white-space: nowrap;
      overflow: hidden;
      text-overflow: ellipsis;
    }

    .countdown-date {
      font-size: 0.875rem;
      color: var(--text-secondary);
      margin-top: 2px;
    }

    .countdown-days {
      display: flex;
      flex-direction: column;
      align-items: center;
      flex-shrink: 0;
      min-width: 60px;
    }

    .days-number {
      font-size: 2rem;
      font-weight: 600;
      color: var(--accent-600);
      line-height: 1;
      font-variant-numeric: tabular-nums;
    }

    .days-label {
      font-size: 0.75rem;
      font-weight: 500;
      color: var(--text-tertiary);
      text-transform: uppercase;
      letter-spacing: 0.05em;
    }

    .empty-state {
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: center;
      padding: var(--space-6);
      color: var(--text-tertiary);
    }

    .empty-text {
      font-size: 1.125rem;
      font-weight: 500;
      color: var(--text-secondary);
    }

    .empty-subtext {
      font-size: 0.875rem;
      margin-top: var(--space-1);
    }
  `],
})
export class CountdownWidgetComponent implements OnInit, OnDestroy {
  @Input() events: ScheduleEvent[] = [];
  @Input() members: MemberData[] = [];
  @Input() maxItems = 3;
  @Input() maxDaysAhead = 90; // Only show events within 90 days

  protected readonly daysNow = signal(this.getTodayMidnight());

  private intervalId?: ReturnType<typeof setInterval>;

  /**
   * Filter and sort countdown events
   */
  protected readonly countdowns = computed((): CountdownItem[] => {
    const today = this.daysNow();
    const maxDate = new Date(today);
    maxDate.setDate(maxDate.getDate() + this.maxDaysAhead);

    const items: CountdownItem[] = [];

    for (const event of this.events) {
      let eventMidnight: Date;

      if (event.isAllDay && event.startDate) {
        // All-day event: parse the date string
        const [year, month, day] = event.startDate.split('-').map(Number);
        eventMidnight = new Date(year, month - 1, day);
      } else if (event.startTime) {
        // Timed event: use startTime
        const eventDate = new Date(event.startTime);
        eventMidnight = new Date(eventDate);
        eventMidnight.setHours(0, 0, 0, 0);
      } else {
        continue; // Skip events without start info
      }

      // Only include future events within range
      if (eventMidnight >= today && eventMidnight <= maxDate) {
        const daysRemaining = Math.ceil(
          (eventMidnight.getTime() - today.getTime()) / (1000 * 60 * 60 * 24)
        );

        items.push({
          event,
          daysRemaining,
          emoji: this.getEventEmoji(event.title),
        });
      }
    }

    // Sort by days remaining (ascending), then by title
    items.sort((a, b) => {
      if (a.daysRemaining !== b.daysRemaining) {
        return a.daysRemaining - b.daysRemaining;
      }
      return a.event.title.localeCompare(b.event.title);
    });

    return items.slice(0, this.maxItems);
  });

  ngOnInit(): void {
    // Update at midnight to refresh day counts
    this.scheduleNextMidnightUpdate();
  }

  ngOnDestroy(): void {
    if (this.intervalId) {
      clearInterval(this.intervalId);
    }
  }

  protected formatEventDate(event: ScheduleEvent): string {
    let date: Date;

    if (event.isAllDay && event.startDate) {
      // Parse the YYYY-MM-DD date string
      const [year, month, day] = event.startDate.split('-').map(Number);
      date = new Date(year, month - 1, day);
    } else if (event.startTime) {
      date = new Date(event.startTime);
    } else {
      return '';
    }

    return date.toLocaleDateString('en-US', {
      weekday: 'short',
      month: 'short',
      day: 'numeric',
    });
  }

  protected getEventColor(event: ScheduleEvent): string {
    if (event.color) return event.color;
    if (event.memberIds.length > 0) {
      const member = this.members.find(m => m.id === event.memberIds[0]);
      if (member) return member.color;
    }
    return 'var(--accent-500)';
  }

  private getTodayMidnight(): Date {
    const today = new Date();
    today.setHours(0, 0, 0, 0);
    return today;
  }

  private scheduleNextMidnightUpdate(): void {
    const now = new Date();
    const tomorrow = new Date(now);
    tomorrow.setDate(tomorrow.getDate() + 1);
    tomorrow.setHours(0, 0, 0, 0);

    const msUntilMidnight = tomorrow.getTime() - now.getTime();

    // Update at next midnight, then every 24 hours
    setTimeout(() => {
      this.daysNow.set(this.getTodayMidnight());

      this.intervalId = setInterval(() => {
        this.daysNow.set(this.getTodayMidnight());
      }, 24 * 60 * 60 * 1000);
    }, msUntilMidnight);
  }

  private getEventEmoji(title: string): string {
    const lowerTitle = title.toLowerCase();

    for (const [keyword, emoji] of Object.entries(EVENT_EMOJIS)) {
      if (lowerTitle.includes(keyword)) {
        return emoji;
      }
    }

    // Default emoji
    return '📅';
  }
}
