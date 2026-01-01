import { Component, OnInit, OnDestroy, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ElectronService } from '../../../../core/services/electron.service';

@Component({
  selector: 'app-clock-widget',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="clock-widget">
      <div class="clock-time text-time">
        {{ time() }}
      </div>
      <div class="clock-date text-display-sm">
        {{ date() }}
      </div>
    </div>
  `,
  styles: [`
    .clock-widget {
      display: flex;
      flex-direction: column;
      gap: var(--space-2);
    }

    .clock-time {
      color: var(--text-primary);
    }

    .clock-date {
      color: var(--text-secondary);
    }
  `],
})
export class ClockWidgetComponent implements OnInit, OnDestroy {
  private readonly electronService = inject(ElectronService);

  protected readonly time = signal('');
  protected readonly date = signal('');

  private intervalId?: ReturnType<typeof setInterval>;
  private use24Hour = false;

  async ngOnInit(): Promise<void> {
    // Get settings for time format
    const settings = await this.electronService.getSettings();
    this.use24Hour = settings.timeFormat === '24h';

    this.updateClock();
    this.intervalId = setInterval(() => this.updateClock(), 1000);
  }

  ngOnDestroy(): void {
    if (this.intervalId) {
      clearInterval(this.intervalId);
    }
  }

  private updateClock(): void {
    const now = new Date();

    // Format time
    if (this.use24Hour) {
      this.time.set(
        now.toLocaleTimeString('en-US', {
          hour: '2-digit',
          minute: '2-digit',
          hour12: false,
        })
      );
    } else {
      const hours = now.getHours();
      const minutes = now.getMinutes();
      const ampm = hours >= 12 ? 'PM' : 'AM';
      const displayHours = hours % 12 || 12;
      const displayMinutes = minutes.toString().padStart(2, '0');
      this.time.set(`${displayHours}:${displayMinutes} ${ampm}`);
    }

    // Format date
    this.date.set(
      now.toLocaleDateString('en-US', {
        weekday: 'long',
        month: 'long',
        day: 'numeric',
      })
    );
  }
}
