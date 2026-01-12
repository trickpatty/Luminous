import { Component, inject, OnInit, OnDestroy, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DisplayModeService } from '../../../../core/services/display-mode.service';

/**
 * Sleep Mode Component
 *
 * Displays a minimal, dimmed view during scheduled sleep hours.
 * Allows optional wake-on-touch to return to normal mode.
 *
 * Requirements implemented:
 * - DSP-010: Scheduled screen-off times
 * - DSP-011: Manual sleep toggle
 * - DSP-012: Wake on touch or schedule
 *
 * Features:
 * - Dimmed display based on configured dim level
 * - Optional small clock display at very low brightness
 * - Touch anywhere to wake (if enabled)
 * - Smooth transition animations
 */
@Component({
  selector: 'app-sleep-mode',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div
      class="sleep-container"
      [style.--dim-opacity]="dimOpacity()"
      (click)="handleTap($event)"
      (touchstart)="handleTap($event)"
    >
      <!-- Dark overlay for dimming effect -->
      <div class="dim-overlay"></div>

      <!-- Minimal clock (only visible at higher dim levels) -->
      @if (showMinimalClock()) {
        <div class="minimal-clock">
          {{ currentTime() }}
        </div>
      }

      <!-- Wake hint (appears after a few seconds) -->
      @if (wakeOnTouch()) {
        <div class="wake-hint" [class.visible]="showWakeHint()">
          Tap to wake
        </div>
      }
    </div>
  `,
  styles: [`
    :host {
      display: block;
      height: 100vh;
      width: 100vw;
      position: fixed;
      top: 0;
      left: 0;
      z-index: 200;
    }

    .sleep-container {
      height: 100%;
      width: 100%;
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: center;
      background: #000000;
      position: relative;
      cursor: pointer;
    }

    .dim-overlay {
      position: absolute;
      inset: 0;
      background: #000000;
      opacity: calc(1 - var(--dim-opacity, 0.1));
      pointer-events: none;
      transition: opacity 1s ease-in-out;
    }

    .minimal-clock {
      position: relative;
      z-index: 1;
      font-size: 4rem;
      font-weight: 300;
      color: #3F3F46;
      font-variant-numeric: tabular-nums;
      letter-spacing: 0.1em;
      opacity: var(--dim-opacity, 0.1);
      transition: opacity 1s ease-in-out;
    }

    .wake-hint {
      position: absolute;
      bottom: var(--space-12);
      left: 50%;
      transform: translateX(-50%);
      font-size: 0.875rem;
      color: #52525B;
      opacity: 0;
      transition: opacity 0.3s ease-out;
    }

    .wake-hint.visible {
      opacity: calc(var(--dim-opacity, 0.1) * 0.5);
    }

    /* Ensure the screen can dim to near-black */
    @media (prefers-color-scheme: dark) {
      .sleep-container {
        background: #000000;
      }
    }

    /* Responsive adjustments */
    @media (min-height: 1200px) {
      .minimal-clock {
        font-size: 6rem;
      }
    }

    @media (max-height: 600px) {
      .minimal-clock {
        font-size: 2.5rem;
      }
    }
  `],
})
export class SleepModeComponent implements OnInit, OnDestroy {
  private readonly displayModeService = inject(DisplayModeService);

  private clockTimer?: ReturnType<typeof setInterval>;
  private hintTimer?: ReturnType<typeof setTimeout>;

  // Clock state
  protected readonly currentTime = signal('');

  // UI state
  protected readonly showWakeHint = signal(false);

  // Computed values from settings
  protected readonly dimOpacity = computed(() => {
    const dimLevel = this.displayModeService.sleepSettings().dimLevel;
    return dimLevel / 100;
  });

  protected readonly wakeOnTouch = computed(() =>
    this.displayModeService.sleepSettings().wakeOnTouch
  );

  // Show minimal clock only if dim level > 5%
  protected readonly showMinimalClock = computed(() => {
    return this.displayModeService.sleepSettings().dimLevel > 5;
  });

  ngOnInit(): void {
    this.updateClock();

    // Update clock every minute (no need for seconds in sleep mode)
    this.clockTimer = setInterval(() => {
      this.updateClock();
    }, 60000);

    // Show wake hint after 3 seconds
    if (this.wakeOnTouch()) {
      this.hintTimer = setTimeout(() => {
        this.showWakeHint.set(true);
      }, 3000);
    }
  }

  ngOnDestroy(): void {
    if (this.clockTimer) {
      clearInterval(this.clockTimer);
    }
    if (this.hintTimer) {
      clearTimeout(this.hintTimer);
    }
  }

  /**
   * Handle tap to wake from sleep
   */
  handleTap(event: Event): void {
    event.preventDefault();
    event.stopPropagation();

    if (this.wakeOnTouch()) {
      this.displayModeService.wakeFromSleep();
    }
  }

  /**
   * Update the minimal clock display
   */
  private updateClock(): void {
    const now = new Date();
    const timeOptions: Intl.DateTimeFormatOptions = {
      hour: 'numeric',
      minute: '2-digit',
      hour12: false, // Use 24h format for minimal display
    };
    this.currentTime.set(now.toLocaleTimeString('en-US', timeOptions));
  }
}
