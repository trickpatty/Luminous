import { Component, inject, OnInit, OnDestroy, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DisplayModeService } from '../../../../core/services/display-mode.service';
import { CanvasService } from '../../../../core/services/canvas.service';
import { WeatherWidgetComponent } from '../weather-widget/weather-widget.component';

/**
 * Privacy Mode Component
 *
 * Displays a minimal view with clock and optional weather when privacy mode is active.
 * Hides all family-specific information (calendar, tasks, members).
 *
 * Requirements implemented:
 * - DSP-001: One-tap privacy mode (swap to wallpaper/clock)
 * - DSP-002: Automatic privacy mode after timeout
 *
 * Features:
 * - Large clock display (time and date)
 * - Optional weather display
 * - Subtle animated gradient background
 * - Touch anywhere to return to normal mode
 */
@Component({
  selector: 'app-privacy-mode',
  standalone: true,
  imports: [CommonModule, WeatherWidgetComponent],
  template: `
    <div
      class="privacy-container"
      [style.background]="backgroundGradient()"
      (click)="handleTap()"
      (touchstart)="handleTap()"
    >
      <!-- Ambient gradient overlay -->
      <div class="ambient-overlay"></div>

      <!-- Main content -->
      <div class="privacy-content">
        <!-- Clock display -->
        <div class="clock-section">
          <div class="time-display">
            {{ currentTime() }}
          </div>
          <div class="date-display">
            {{ currentDate() }}
          </div>
        </div>

        <!-- Optional weather display -->
        @if (showWeather()) {
          <div class="weather-section">
            <app-weather-widget [compact]="false" />
          </div>
        }
      </div>

      <!-- Hint text -->
      <div class="hint-text">
        Tap anywhere to return
      </div>
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
      z-index: 100;
    }

    .privacy-container {
      height: 100%;
      width: 100%;
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: center;
      cursor: pointer;
      position: relative;
      transition: background 30s ease-in-out;
    }

    .ambient-overlay {
      position: absolute;
      inset: 0;
      background: radial-gradient(
        ellipse at center,
        transparent 0%,
        rgba(0, 0, 0, 0.1) 100%
      );
      pointer-events: none;
    }

    .privacy-content {
      position: relative;
      z-index: 1;
      display: flex;
      flex-direction: column;
      align-items: center;
      gap: var(--space-12);
      padding: var(--space-8);
    }

    .clock-section {
      text-align: center;
    }

    .time-display {
      font-size: 8rem;
      font-weight: 600;
      line-height: 1;
      letter-spacing: -0.02em;
      color: var(--text-primary);
      font-variant-numeric: tabular-nums;
      text-shadow: 0 2px 20px rgba(0, 0, 0, 0.05);
    }

    .date-display {
      font-size: 2rem;
      font-weight: 500;
      color: var(--text-secondary);
      margin-top: var(--space-4);
      letter-spacing: 0.02em;
    }

    .weather-section {
      max-width: 400px;
      width: 100%;
    }

    .hint-text {
      position: absolute;
      bottom: var(--space-12);
      left: 50%;
      transform: translateX(-50%);
      font-size: 1rem;
      color: var(--text-tertiary);
      opacity: 0;
      animation: fadeInHint 0.5s ease-out 2s forwards;
    }

    @keyframes fadeInHint {
      to {
        opacity: 1;
      }
    }

    /* Responsive adjustments for larger displays */
    @media (min-height: 1200px) {
      .time-display {
        font-size: 12rem;
      }

      .date-display {
        font-size: 3rem;
      }
    }

    /* Smaller screens */
    @media (max-height: 800px) {
      .time-display {
        font-size: 5rem;
      }

      .date-display {
        font-size: 1.5rem;
      }
    }

    /* Night/dark mode adjustments */
    :host-context([data-theme="night"]) .time-display {
      color: #FAFAFA;
      text-shadow: 0 2px 20px rgba(255, 255, 255, 0.1);
    }

    :host-context([data-theme="night"]) .date-display {
      color: #A1A1AA;
    }

    :host-context([data-theme="night"]) .hint-text {
      color: #71717A;
    }
  `],
})
export class PrivacyModeComponent implements OnInit, OnDestroy {
  private readonly displayModeService = inject(DisplayModeService);
  private readonly canvasService = inject(CanvasService);

  private clockTimer?: ReturnType<typeof setInterval>;
  private gradientTimer?: ReturnType<typeof setInterval>;

  // Clock state
  protected readonly currentTime = signal('');
  protected readonly currentDate = signal('');

  // Settings
  protected readonly showWeather = computed(() =>
    this.displayModeService.privacySettings().showWeather
  );

  // Background gradient that slowly shifts
  protected readonly backgroundGradient = signal('');
  private gradientIndex = 0;

  // Gradient colors that complement the time-adaptive canvas
  private readonly gradientColors = [
    ['#FEFDFB', '#FDF9F3'], // Warm cream
    ['#F0F9FF', '#E0F2FE'], // Soft sky
    ['#FDF2F8', '#FCE7F3'], // Gentle rose
    ['#F5F3FF', '#EDE9FE'], // Soft violet
    ['#ECFDF5', '#D1FAE5'], // Fresh mint
  ];

  ngOnInit(): void {
    this.updateClock();
    this.updateGradient();

    // Update clock every second
    this.clockTimer = setInterval(() => {
      this.updateClock();
    }, 1000);

    // Slowly shift gradient every 30 seconds
    this.gradientTimer = setInterval(() => {
      this.updateGradient();
    }, 30000);
  }

  ngOnDestroy(): void {
    if (this.clockTimer) {
      clearInterval(this.clockTimer);
    }
    if (this.gradientTimer) {
      clearInterval(this.gradientTimer);
    }
  }

  /**
   * Handle tap to return to normal mode.
   * Always exits privacy mode when tapped, regardless of whether it was
   * manually activated or triggered by inactivity timeout.
   */
  handleTap(): void {
    this.displayModeService.enterNormalMode();
  }

  /**
   * Update the clock display
   */
  private updateClock(): void {
    const now = new Date();

    // Format time (using canvas service theme could affect this)
    const timeOptions: Intl.DateTimeFormatOptions = {
      hour: 'numeric',
      minute: '2-digit',
      hour12: true,
    };
    this.currentTime.set(now.toLocaleTimeString('en-US', timeOptions));

    // Format date
    const dateOptions: Intl.DateTimeFormatOptions = {
      weekday: 'long',
      month: 'long',
      day: 'numeric',
    };
    this.currentDate.set(now.toLocaleDateString('en-US', dateOptions));
  }

  /**
   * Update the background gradient
   */
  private updateGradient(): void {
    const theme = this.canvasService.currentTheme();

    if (theme === 'night') {
      // Dark mode gradients
      this.backgroundGradient.set('linear-gradient(135deg, #18181B 0%, #27272A 100%)');
      return;
    }

    const colors = this.gradientColors[this.gradientIndex];
    this.backgroundGradient.set(
      `linear-gradient(135deg, ${colors[0]} 0%, ${colors[1]} 100%)`
    );

    this.gradientIndex = (this.gradientIndex + 1) % this.gradientColors.length;
  }
}
