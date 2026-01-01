import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ScheduleEvent, MemberData } from '../../../../core/services/cache.service';

@Component({
  selector: 'app-schedule-view',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="schedule-view">
      <div class="schedule-header">
        <h2 class="text-display-md">Today's Schedule</h2>
      </div>

      @if (isLoading) {
        <div class="schedule-loading">
          <div class="display-spinner display-spinner-lg"></div>
        </div>
      } @else if (events.length === 0) {
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
          <h3 class="display-empty-title">All caught up!</h3>
          <p class="display-empty-description">
            No events scheduled for today. Enjoy your free time.
          </p>
        </div>
      } @else {
        <div class="schedule-list display-card">
          @for (event of events; track event.id) {
            <div class="display-event" [style.--member-color]="getEventColor(event)">
              <div class="display-event-time">
                {{ formatTime(event.startTime) }}
              </div>
              <div class="display-event-content">
                <div class="display-event-title">
                  {{ event.title }}
                </div>
                @if (event.location) {
                  <div class="display-event-location">
                    {{ event.location }}
                  </div>
                }
                @if (event.memberIds.length > 0) {
                  <div class="event-members">
                    @for (memberId of event.memberIds; track memberId) {
                      @if (getMember(memberId); as member) {
                        <span
                          class="event-member-badge"
                          [style.background-color]="member.color"
                        >
                          {{ member.initials }}
                        </span>
                      }
                    }
                  </div>
                }
              </div>
              <div class="event-indicator" [style.background-color]="getEventColor(event)"></div>
            </div>
          }
        </div>
      }
    </div>
  `,
  styles: [`
    .schedule-view {
      height: 100%;
      display: flex;
      flex-direction: column;
      gap: var(--space-6);
    }

    .schedule-header {
      display: flex;
      align-items: center;
      justify-content: space-between;
    }

    .schedule-loading {
      flex: 1;
      display: flex;
      align-items: center;
      justify-content: center;
    }

    .schedule-list {
      flex: 1;
      overflow-y: auto;
    }

    .display-event {
      position: relative;
      padding-right: var(--space-6);
    }

    .event-indicator {
      position: absolute;
      right: 0;
      top: var(--space-4);
      bottom: var(--space-4);
      width: 4px;
      border-radius: var(--radius-full);
      background: var(--accent-500);
    }

    .event-members {
      display: flex;
      gap: var(--space-2);
      margin-top: var(--space-2);
    }

    .event-member-badge {
      display: inline-flex;
      align-items: center;
      justify-content: center;
      width: 28px;
      height: 28px;
      border-radius: var(--radius-full);
      font-size: 11px;
      font-weight: 600;
      color: white;
    }
  `],
})
export class ScheduleViewComponent {
  @Input() events: ScheduleEvent[] = [];
  @Input() members: MemberData[] = [];
  @Input() isLoading = false;

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

  getMember(memberId: string): MemberData | undefined {
    return this.members.find((m) => m.id === memberId);
  }

  getEventColor(event: ScheduleEvent): string {
    if (event.color) return event.color;
    if (event.memberIds.length > 0) {
      const member = this.getMember(event.memberIds[0]);
      if (member) return member.color;
    }
    return 'var(--accent-500)';
  }
}
