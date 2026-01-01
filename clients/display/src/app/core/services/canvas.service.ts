import { Injectable, signal, computed } from '@angular/core';

type TimeOfDay = 'dawn' | 'morning' | 'afternoon' | 'evening' | 'night';
type Theme = 'light' | 'night';

interface CanvasConfig {
  dawn: string;
  morning: string;
  afternoon: string;
  evening: string;
  night: string;
}

const CANVAS_COLORS: CanvasConfig = {
  dawn: '#FFFCF7',
  morning: '#FEFDFB',
  afternoon: '#FDFCFA',
  evening: '#FDF9F3',
  night: '#FAF7F2',
};

const TIME_PERIODS: { period: TimeOfDay; start: number; end: number }[] = [
  { period: 'dawn', start: 5, end: 7 },
  { period: 'morning', start: 7, end: 12 },
  { period: 'afternoon', start: 12, end: 17 },
  { period: 'evening', start: 17, end: 21 },
  { period: 'night', start: 21, end: 5 },
];

/**
 * Service for managing the adaptive canvas color based on time of day.
 * Creates a warm, comfortable visual rhythm throughout the day.
 */
@Injectable({
  providedIn: 'root',
})
export class CanvasService {
  private readonly _timeOfDay = signal<TimeOfDay>('afternoon');
  private readonly _theme = signal<Theme>('light');
  private readonly _nightDisplayEnabled = signal(false);
  private readonly _nightDisplayStart = signal(21); // 9 PM
  private readonly _nightDisplayEnd = signal(6); // 6 AM

  private adaptationInterval?: ReturnType<typeof setInterval>;

  readonly timeOfDay = this._timeOfDay.asReadonly();
  readonly currentTheme = computed(() => {
    if (this._nightDisplayEnabled()) {
      const hour = new Date().getHours();
      const start = this._nightDisplayStart();
      const end = this._nightDisplayEnd();

      // Handle overnight range (e.g., 21:00 to 06:00)
      if (start > end) {
        if (hour >= start || hour < end) {
          return 'night';
        }
      } else {
        if (hour >= start && hour < end) {
          return 'night';
        }
      }
    }
    return this._theme();
  });

  readonly canvasColor = computed(() => {
    if (this.currentTheme() === 'night') {
      return '#18181B'; // Night display dark canvas
    }
    return CANVAS_COLORS[this._timeOfDay()];
  });

  /**
   * Start canvas color adaptation based on time
   */
  startAdaptation(): void {
    this.updateTimeOfDay();
    this.updateCanvasVariable();

    // Update every minute
    this.adaptationInterval = setInterval(() => {
      this.updateTimeOfDay();
      this.updateCanvasVariable();
    }, 60000);
  }

  /**
   * Stop canvas adaptation
   */
  stopAdaptation(): void {
    if (this.adaptationInterval) {
      clearInterval(this.adaptationInterval);
      this.adaptationInterval = undefined;
    }
  }

  /**
   * Configure night display mode
   */
  setNightDisplay(enabled: boolean, startHour?: number, endHour?: number): void {
    this._nightDisplayEnabled.set(enabled);
    if (startHour !== undefined) {
      this._nightDisplayStart.set(startHour);
    }
    if (endHour !== undefined) {
      this._nightDisplayEnd.set(endHour);
    }
    this.updateCanvasVariable();
  }

  /**
   * Force a specific theme (for testing/preview)
   */
  setTheme(theme: Theme): void {
    this._theme.set(theme);
    this.updateCanvasVariable();
  }

  private updateTimeOfDay(): void {
    const hour = new Date().getHours();
    const period = this.getTimePeriod(hour);
    this._timeOfDay.set(period);
  }

  private getTimePeriod(hour: number): TimeOfDay {
    for (const { period, start, end } of TIME_PERIODS) {
      if (period === 'night') {
        // Night spans midnight
        if (hour >= start || hour < end) {
          return period;
        }
      } else {
        if (hour >= start && hour < end) {
          return period;
        }
      }
    }
    return 'afternoon'; // Default fallback
  }

  private updateCanvasVariable(): void {
    // Update CSS custom property
    document.documentElement.style.setProperty('--canvas', this.canvasColor());
  }
}
