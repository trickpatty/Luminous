import { Component, inject, signal, OnInit, OnDestroy, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { WeatherService, WeatherData, DailyForecast, WeatherCondition } from '../../../../core/services/weather.service';
import { ElectronService } from '../../../../core/services/electron.service';

/**
 * Weather Widget - Displays current weather and multi-day forecast.
 * Uses Open-Meteo API (free, no API key required).
 * Supports Fahrenheit/Celsius unit preferences.
 */
@Component({
  selector: 'app-weather-widget',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="weather-widget" [class.loading]="isLoading()" [class.compact]="compact">
      @if (weather(); as data) {
        <!-- Current Weather -->
        <div class="current-weather">
          <div class="weather-icon">{{ getWeatherIcon(data.current.condition, data.current.isDay) }}</div>
          <div class="weather-main">
            <div class="temperature">{{ formatTemp(data.current.temperature) }}</div>
            <div class="condition">{{ data.current.description }}</div>
          </div>
          @if (!compact) {
            <div class="weather-details">
              <div class="detail-item">
                <span class="detail-label">Feels like</span>
                <span class="detail-value">{{ formatTemp(data.current.feelsLike) }}</span>
              </div>
              <div class="detail-item">
                <span class="detail-label">Humidity</span>
                <span class="detail-value">{{ data.current.humidity }}%</span>
              </div>
              <div class="detail-item">
                <span class="detail-label">Wind</span>
                <span class="detail-value">{{ formatWind(data.current.windSpeed) }}</span>
              </div>
            </div>
          }
        </div>

        <!-- Alerts -->
        @if (data.alerts.length > 0) {
          <div class="weather-alerts">
            @for (alert of data.alerts.slice(0, 1); track alert.id) {
              <div class="alert-item" [class]="'alert-' + alert.severity">
                <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                  <path d="m21.73 18-8-14a2 2 0 0 0-3.48 0l-8 14A2 2 0 0 0 4 21h16a2 2 0 0 0 1.73-3Z"/>
                  <path d="M12 9v4"/>
                  <path d="M12 17h.01"/>
                </svg>
                <span>{{ alert.headline }}</span>
              </div>
            }
          </div>
        }

        <!-- Forecast -->
        @if (!compact && data.forecast.length > 1) {
          <div class="forecast">
            @for (day of data.forecast.slice(1, 5); track day.date.toISOString()) {
              <div class="forecast-day">
                <div class="forecast-day-name">{{ formatDayName(day.date) }}</div>
                <div class="forecast-icon">{{ getWeatherIcon(day.condition, true) }}</div>
                <div class="forecast-temps">
                  <span class="forecast-high">{{ formatTemp(day.highTemp) }}</span>
                  <span class="forecast-low">{{ formatTemp(day.lowTemp) }}</span>
                </div>
                @if (day.precipProbability > 10) {
                  <div class="forecast-precip">
                    <svg xmlns="http://www.w3.org/2000/svg" width="12" height="12" viewBox="0 0 24 24" fill="currentColor">
                      <path d="M12 2.69l5.66 5.66a8 8 0 1 1-11.31 0z"/>
                    </svg>
                    {{ day.precipProbability }}%
                  </div>
                }
              </div>
            }
          </div>
        }

        <!-- Location & Update time -->
        @if (!compact) {
          <div class="weather-footer">
            <span class="location">{{ data.location }}</span>
            <span class="updated">Updated {{ formatTime(data.lastUpdated) }}</span>
          </div>
        }
      } @else if (error()) {
        <div class="weather-error">
          <svg xmlns="http://www.w3.org/2000/svg" width="32" height="32" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
            <circle cx="12" cy="12" r="10"/>
            <line x1="12" x2="12" y1="8" y2="12"/>
            <line x1="12" x2="12.01" y1="16" y2="16"/>
          </svg>
          <span>{{ error() }}</span>
          <button class="retry-btn" (click)="loadWeather()">Retry</button>
        </div>
      } @else if (isLoading()) {
        <div class="weather-loading">
          <div class="loading-spinner"></div>
          <span>Loading weather...</span>
        </div>
      } @else if (!enabled()) {
        <div class="weather-disabled">
          <span>Weather disabled</span>
        </div>
      }
    </div>
  `,
  styles: [`
    .weather-widget {
      background: var(--surface-primary);
      border-radius: var(--radius-xl);
      padding: var(--space-5);
      box-shadow: var(--shadow-sm);
      min-height: 100px;
    }

    .weather-widget.compact {
      padding: var(--space-3) var(--space-4);
      min-height: auto;
    }

    .weather-widget.loading {
      opacity: 0.7;
    }

    .current-weather {
      display: flex;
      align-items: center;
      gap: var(--space-4);
    }

    .weather-icon {
      font-size: 3rem;
      line-height: 1;
    }

    .compact .weather-icon {
      font-size: 2rem;
    }

    .weather-main {
      flex: 1;
    }

    .temperature {
      font-size: var(--font-display-sm);
      font-weight: 600;
      color: var(--text-primary);
      line-height: 1;
    }

    .compact .temperature {
      font-size: 1.5rem;
    }

    .condition {
      font-size: 1rem;
      color: var(--text-secondary);
      margin-top: var(--space-1);
    }

    .compact .condition {
      font-size: 0.875rem;
    }

    .weather-details {
      display: flex;
      flex-direction: column;
      gap: var(--space-1);
      padding-left: var(--space-4);
      border-left: 1px solid var(--border-color-light);
    }

    .detail-item {
      display: flex;
      justify-content: space-between;
      gap: var(--space-3);
      font-size: 0.875rem;
    }

    .detail-label {
      color: var(--text-tertiary);
    }

    .detail-value {
      color: var(--text-secondary);
      font-weight: 500;
    }

    .weather-alerts {
      margin-top: var(--space-3);
    }

    .alert-item {
      display: flex;
      align-items: center;
      gap: var(--space-2);
      padding: var(--space-2) var(--space-3);
      border-radius: var(--radius-md);
      font-size: 0.875rem;
      font-weight: 500;
    }

    .alert-minor {
      background: var(--info-light);
      color: var(--info-dark);
    }

    .alert-moderate {
      background: var(--warning-light);
      color: var(--warning-dark);
    }

    .alert-severe, .alert-extreme {
      background: var(--danger-light);
      color: var(--danger-dark);
    }

    .forecast {
      display: flex;
      gap: var(--space-2);
      margin-top: var(--space-4);
      padding-top: var(--space-4);
      border-top: 1px solid var(--border-color-light);
    }

    .forecast-day {
      flex: 1;
      display: flex;
      flex-direction: column;
      align-items: center;
      gap: var(--space-1);
      padding: var(--space-2);
      background: var(--surface-secondary);
      border-radius: var(--radius-lg);
    }

    .forecast-day-name {
      font-size: 0.75rem;
      font-weight: 600;
      color: var(--text-secondary);
      text-transform: uppercase;
    }

    .forecast-icon {
      font-size: 1.5rem;
    }

    .forecast-temps {
      display: flex;
      gap: var(--space-2);
      font-size: 0.875rem;
    }

    .forecast-high {
      color: var(--text-primary);
      font-weight: 500;
    }

    .forecast-low {
      color: var(--text-tertiary);
    }

    .forecast-precip {
      display: flex;
      align-items: center;
      gap: 2px;
      font-size: 0.75rem;
      color: var(--accent-600);
    }

    .weather-footer {
      display: flex;
      justify-content: space-between;
      margin-top: var(--space-3);
      padding-top: var(--space-3);
      border-top: 1px solid var(--border-color-light);
      font-size: 0.75rem;
      color: var(--text-tertiary);
    }

    .weather-error, .weather-loading, .weather-disabled {
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: center;
      gap: var(--space-2);
      padding: var(--space-4);
      color: var(--text-secondary);
    }

    .retry-btn {
      padding: var(--space-2) var(--space-4);
      background: var(--surface-secondary);
      border: none;
      border-radius: var(--radius-md);
      font-size: 0.875rem;
      font-weight: 500;
      color: var(--text-primary);
      cursor: pointer;
      transition: background var(--duration-quick) var(--ease-out);
    }

    .retry-btn:active {
      background: var(--surface-pressed);
    }

    .loading-spinner {
      width: 24px;
      height: 24px;
      border: 3px solid var(--surface-secondary);
      border-top-color: var(--accent-500);
      border-radius: 50%;
      animation: spin 800ms linear infinite;
    }

    @keyframes spin {
      to { transform: rotate(360deg); }
    }
  `],
})
export class WeatherWidgetComponent implements OnInit, OnDestroy {
  private readonly weatherService = inject(WeatherService);
  private readonly electronService = inject(ElectronService);

  /** Compact mode for header display */
  @Input() compact = false;

  /** Custom latitude (optional - uses geolocation if not provided) */
  @Input() latitude?: number;

  /** Custom longitude (optional - uses geolocation if not provided) */
  @Input() longitude?: number;

  /** Custom location name */
  @Input() locationName?: string;

  protected readonly weather = this.weatherService.weather;
  protected readonly isLoading = this.weatherService.isLoading;
  protected readonly error = this.weatherService.error;
  protected readonly enabled = signal(true);

  private refreshInterval?: ReturnType<typeof setInterval>;
  private units: 'fahrenheit' | 'celsius' = 'fahrenheit';

  async ngOnInit(): Promise<void> {
    // Get settings
    const settings = await this.electronService.getSettings();
    // Use family's preferred units if available
    // Default to fahrenheit for US locale

    await this.loadWeather();

    // Refresh weather every 30 minutes
    this.refreshInterval = setInterval(() => {
      this.loadWeather();
    }, 30 * 60 * 1000);
  }

  ngOnDestroy(): void {
    if (this.refreshInterval) {
      clearInterval(this.refreshInterval);
    }
  }

  async loadWeather(): Promise<void> {
    if (this.latitude !== undefined && this.longitude !== undefined) {
      await this.weatherService.fetchWeather(this.latitude, this.longitude, this.locationName);
    } else {
      await this.weatherService.fetchWeatherByGeolocation();
    }
  }

  protected getWeatherIcon(condition: WeatherCondition, isDay: boolean): string {
    return this.weatherService.getWeatherIcon(condition, isDay);
  }

  protected formatTemp(temp: number): string {
    return this.weatherService.formatTemperature(temp);
  }

  protected formatWind(speed: number): string {
    const units = this.weatherService.settings().units;
    const unit = units === 'celsius' ? 'km/h' : 'mph';
    return `${Math.round(speed)} ${unit}`;
  }

  protected formatDayName(date: Date): string {
    const today = new Date();
    const tomorrow = new Date(today);
    tomorrow.setDate(tomorrow.getDate() + 1);

    if (this.isSameDay(date, today)) return 'Today';
    if (this.isSameDay(date, tomorrow)) return 'Tmrw';

    return date.toLocaleDateString('en-US', { weekday: 'short' });
  }

  protected formatTime(date: Date): string {
    const now = new Date();
    const diffMins = Math.floor((now.getTime() - date.getTime()) / (1000 * 60));

    if (diffMins < 1) return 'just now';
    if (diffMins < 60) return `${diffMins}m ago`;

    const diffHours = Math.floor(diffMins / 60);
    if (diffHours < 24) return `${diffHours}h ago`;

    return date.toLocaleDateString('en-US', { month: 'short', day: 'numeric' });
  }

  private isSameDay(d1: Date, d2: Date): boolean {
    return (
      d1.getFullYear() === d2.getFullYear() &&
      d1.getMonth() === d2.getMonth() &&
      d1.getDate() === d2.getDate()
    );
  }
}
