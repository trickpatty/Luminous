import { Injectable, signal, effect, computed } from '@angular/core';

export type CanvasPeriod = 'dawn' | 'morning' | 'afternoon' | 'evening' | 'night';
export type CanvasPreference = 'auto' | 'cool' | 'warm' | 'night';

interface CanvasColors {
  dawn: string;
  morning: string;
  afternoon: string;
  evening: string;
  night: string;
}

const CANVAS_COLORS: CanvasColors = {
  dawn: '#FFFCF7',
  morning: '#FEFDFB',
  afternoon: '#FDFCFA',
  evening: '#FDF9F3',
  night: '#FAF7F2',
};

const STATIC_PREFERENCES: Record<Exclude<CanvasPreference, 'auto'>, string> = {
  cool: '#FDFCFA',
  warm: '#FDF9F3',
  night: '#18181B',
};

/**
 * Service that manages the time-adaptive canvas background color.
 * Updates the --canvas CSS variable based on time of day.
 */
@Injectable({
  providedIn: 'root',
})
export class CanvasService {
  private preferenceSignal = signal<CanvasPreference>('auto');
  private currentPeriodSignal = signal<CanvasPeriod>(this.calculatePeriod());
  private intervalId?: ReturnType<typeof setInterval>;

  /** Current canvas preference setting */
  readonly preference = computed(() => this.preferenceSignal());

  /** Current time period (only relevant when preference is 'auto') */
  readonly currentPeriod = computed(() => this.currentPeriodSignal());

  /** Current canvas color being applied */
  readonly currentColor = computed(() => {
    const pref = this.preferenceSignal();
    if (pref !== 'auto') {
      return STATIC_PREFERENCES[pref];
    }
    return CANVAS_COLORS[this.currentPeriodSignal()];
  });

  /** Whether night display (dark mode) is active */
  readonly isNightDisplay = computed(() => this.preferenceSignal() === 'night');

  constructor() {
    // Apply canvas color when it changes
    effect(() => {
      this.applyCanvasColor(this.currentColor());
    });

    // Apply night theme attribute
    effect(() => {
      if (this.isNightDisplay()) {
        document.documentElement.setAttribute('data-theme', 'night');
      } else {
        document.documentElement.removeAttribute('data-theme');
      }
    });

    // Start periodic updates for auto mode
    this.startPeriodicUpdate();

    // Load saved preference
    this.loadPreference();
  }

  /**
   * Set the canvas preference
   */
  setPreference(preference: CanvasPreference): void {
    this.preferenceSignal.set(preference);
    this.savePreference(preference);
  }

  /**
   * Calculate the current time period based on hour
   */
  private calculatePeriod(): CanvasPeriod {
    const hour = new Date().getHours();

    if (hour >= 5 && hour < 7) return 'dawn';
    if (hour >= 7 && hour < 12) return 'morning';
    if (hour >= 12 && hour < 17) return 'afternoon';
    if (hour >= 17 && hour < 21) return 'evening';
    return 'night';
  }

  /**
   * Apply the canvas color to the document
   */
  private applyCanvasColor(color: string): void {
    document.documentElement.style.setProperty('--canvas', color);
  }

  /**
   * Start periodic update every minute to check for period changes
   */
  private startPeriodicUpdate(): void {
    // Check every minute
    this.intervalId = setInterval(() => {
      const newPeriod = this.calculatePeriod();
      if (newPeriod !== this.currentPeriodSignal()) {
        this.currentPeriodSignal.set(newPeriod);
      }
    }, 60000);
  }

  /**
   * Save preference to localStorage
   */
  private savePreference(preference: CanvasPreference): void {
    try {
      localStorage.setItem('luminous-canvas-preference', preference);
    } catch {
      // Ignore localStorage errors
    }
  }

  /**
   * Load preference from localStorage
   */
  private loadPreference(): void {
    try {
      const saved = localStorage.getItem('luminous-canvas-preference');
      if (saved && ['auto', 'cool', 'warm', 'night'].includes(saved)) {
        this.preferenceSignal.set(saved as CanvasPreference);
      }
    } catch {
      // Ignore localStorage errors
    }
  }

  /**
   * Cleanup on destroy
   */
  ngOnDestroy(): void {
    if (this.intervalId) {
      clearInterval(this.intervalId);
    }
  }
}
