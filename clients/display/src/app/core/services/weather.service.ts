import { Injectable, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';
import { CacheService } from './cache.service';

/**
 * Weather condition codes mapped to icons and descriptions
 */
export type WeatherCondition =
  | 'clear'
  | 'partly-cloudy'
  | 'cloudy'
  | 'overcast'
  | 'fog'
  | 'drizzle'
  | 'rain'
  | 'snow'
  | 'thunderstorm'
  | 'unknown';

/**
 * Current weather data
 */
export interface CurrentWeather {
  temperature: number;
  feelsLike: number;
  humidity: number;
  condition: WeatherCondition;
  description: string;
  windSpeed: number;
  uvIndex?: number;
  isDay: boolean;
}

/**
 * Daily forecast entry
 */
export interface DailyForecast {
  date: Date;
  highTemp: number;
  lowTemp: number;
  condition: WeatherCondition;
  precipProbability: number;
}

/**
 * Weather alert
 */
export interface WeatherAlert {
  id: string;
  event: string;
  severity: 'minor' | 'moderate' | 'severe' | 'extreme';
  headline: string;
  description: string;
  start: Date;
  end: Date;
}

/**
 * Complete weather data response
 */
export interface WeatherData {
  location: string;
  current: CurrentWeather;
  forecast: DailyForecast[];
  alerts: WeatherAlert[];
  lastUpdated: Date;
}

/**
 * Weather settings stored per-family
 */
export interface WeatherSettings {
  enabled: boolean;
  location?: string; // City name or coordinates
  latitude?: number;
  longitude?: number;
  units: 'fahrenheit' | 'celsius';
}

/**
 * Open-Meteo API response (free, no API key required)
 */
interface OpenMeteoResponse {
  current: {
    time: string;
    temperature_2m: number;
    relative_humidity_2m: number;
    apparent_temperature: number;
    is_day: number;
    weather_code: number;
    wind_speed_10m: number;
  };
  daily: {
    time: string[];
    temperature_2m_max: number[];
    temperature_2m_min: number[];
    weather_code: number[];
    precipitation_probability_max: number[];
  };
}

/**
 * Weather Service - Fetches weather data from Open-Meteo API.
 * Open-Meteo is a free, open-source weather API that requires no API key.
 */
@Injectable({
  providedIn: 'root',
})
export class WeatherService {
  private readonly http = inject(HttpClient);
  private readonly cacheService = inject(CacheService);

  private readonly OPEN_METEO_URL = 'https://api.open-meteo.com/v1/forecast';
  private readonly CACHE_KEY = 'weather_data';
  private readonly CACHE_TTL = 30 * 60 * 1000; // 30 minutes

  // State
  private readonly _weather = signal<WeatherData | null>(null);
  private readonly _isLoading = signal(false);
  private readonly _error = signal<string | null>(null);
  private readonly _settings = signal<WeatherSettings>({
    enabled: true,
    units: 'fahrenheit',
  });

  // Public signals
  readonly weather = this._weather.asReadonly();
  readonly isLoading = this._isLoading.asReadonly();
  readonly error = this._error.asReadonly();
  readonly settings = this._settings.asReadonly();

  /**
   * Update weather settings
   */
  setSettings(settings: Partial<WeatherSettings>): void {
    this._settings.update(current => ({ ...current, ...settings }));
  }

  /**
   * Fetch weather data for a location
   */
  async fetchWeather(
    latitude: number,
    longitude: number,
    locationName?: string
  ): Promise<WeatherData | null> {
    this._isLoading.set(true);
    this._error.set(null);

    try {
      const settings = this._settings();
      const tempUnit = settings.units === 'celsius' ? 'celsius' : 'fahrenheit';
      const windUnit = settings.units === 'celsius' ? 'kmh' : 'mph';

      const params = new URLSearchParams({
        latitude: latitude.toString(),
        longitude: longitude.toString(),
        current: 'temperature_2m,relative_humidity_2m,apparent_temperature,is_day,weather_code,wind_speed_10m',
        daily: 'temperature_2m_max,temperature_2m_min,weather_code,precipitation_probability_max',
        temperature_unit: tempUnit,
        wind_speed_unit: windUnit,
        forecast_days: '5',
        timezone: 'auto',
      });

      const response = await firstValueFrom(
        this.http.get<OpenMeteoResponse>(`${this.OPEN_METEO_URL}?${params}`)
      );

      const weatherData = this.mapOpenMeteoResponse(response, locationName || 'Current Location');
      this._weather.set(weatherData);

      // Cache the data
      await this.cacheWeather(weatherData);

      return weatherData;
    } catch (error) {
      console.error('Failed to fetch weather:', error);
      this._error.set('Unable to load weather');

      // Try to load from cache
      const cached = await this.loadCachedWeather();
      if (cached) {
        this._weather.set(cached);
        return cached;
      }

      return null;
    } finally {
      this._isLoading.set(false);
    }
  }

  /**
   * Fetch weather using browser geolocation.
   * Falls back to cached weather data if geolocation fails.
   */
  async fetchWeatherByGeolocation(): Promise<WeatherData | null> {
    if (!navigator.geolocation) {
      // Geolocation not supported - try to use cached data
      const cached = await this.loadCachedWeather();
      if (cached) {
        this._weather.set(cached);
        return cached;
      }
      this._error.set('Geolocation not supported');
      return null;
    }

    return new Promise((resolve) => {
      navigator.geolocation.getCurrentPosition(
        async (position) => {
          const result = await this.fetchWeather(
            position.coords.latitude,
            position.coords.longitude
          );
          resolve(result);
        },
        async (error) => {
          console.error('Geolocation error:', error);
          // Try to use cached weather data when geolocation fails
          const cached = await this.loadCachedWeather();
          if (cached) {
            this._weather.set(cached);
            // Don't set error if we have cached data
            resolve(cached);
            return;
          }
          this._error.set('Unable to get location');
          resolve(null);
        },
        { timeout: 10000, enableHighAccuracy: false }
      );
    });
  }

  /**
   * Get icon name for a weather condition
   */
  getWeatherIcon(condition: WeatherCondition, isDay: boolean): string {
    const icons: Record<WeatherCondition, { day: string; night: string }> = {
      'clear': { day: '☀️', night: '🌙' },
      'partly-cloudy': { day: '⛅', night: '☁️' },
      'cloudy': { day: '☁️', night: '☁️' },
      'overcast': { day: '☁️', night: '☁️' },
      'fog': { day: '🌫️', night: '🌫️' },
      'drizzle': { day: '🌦️', night: '🌧️' },
      'rain': { day: '🌧️', night: '🌧️' },
      'snow': { day: '🌨️', night: '🌨️' },
      'thunderstorm': { day: '⛈️', night: '⛈️' },
      'unknown': { day: '❓', night: '❓' },
    };

    return icons[condition][isDay ? 'day' : 'night'];
  }

  /**
   * Format temperature with unit symbol
   */
  formatTemperature(temp: number): string {
    const units = this._settings().units;
    const symbol = units === 'celsius' ? '°C' : '°F';
    return `${Math.round(temp)}${symbol}`;
  }

  // ============================================
  // Private Methods
  // ============================================

  private mapOpenMeteoResponse(response: OpenMeteoResponse, location: string): WeatherData {
    const current = response.current;
    const daily = response.daily;

    return {
      location,
      current: {
        temperature: current.temperature_2m,
        feelsLike: current.apparent_temperature,
        humidity: current.relative_humidity_2m,
        condition: this.mapWeatherCode(current.weather_code),
        description: this.getWeatherDescription(current.weather_code),
        windSpeed: current.wind_speed_10m,
        isDay: current.is_day === 1,
      },
      forecast: daily.time.map((time, index) => ({
        date: new Date(time),
        highTemp: daily.temperature_2m_max[index],
        lowTemp: daily.temperature_2m_min[index],
        condition: this.mapWeatherCode(daily.weather_code[index]),
        precipProbability: daily.precipitation_probability_max[index] || 0,
      })),
      alerts: [], // Open-Meteo doesn't provide alerts in free tier
      lastUpdated: new Date(),
    };
  }

  private mapWeatherCode(code: number): WeatherCondition {
    // WMO Weather interpretation codes
    // https://open-meteo.com/en/docs
    if (code === 0) return 'clear';
    if (code === 1 || code === 2) return 'partly-cloudy';
    if (code === 3) return 'overcast';
    if (code >= 45 && code <= 48) return 'fog';
    if (code >= 51 && code <= 55) return 'drizzle';
    if (code >= 56 && code <= 67) return 'rain';
    if (code >= 71 && code <= 77) return 'snow';
    if (code >= 80 && code <= 82) return 'rain';
    if (code >= 85 && code <= 86) return 'snow';
    if (code >= 95 && code <= 99) return 'thunderstorm';
    return 'unknown';
  }

  private getWeatherDescription(code: number): string {
    const descriptions: Record<number, string> = {
      0: 'Clear sky',
      1: 'Mainly clear',
      2: 'Partly cloudy',
      3: 'Overcast',
      45: 'Fog',
      48: 'Depositing rime fog',
      51: 'Light drizzle',
      53: 'Moderate drizzle',
      55: 'Dense drizzle',
      56: 'Light freezing drizzle',
      57: 'Dense freezing drizzle',
      61: 'Slight rain',
      63: 'Moderate rain',
      65: 'Heavy rain',
      66: 'Light freezing rain',
      67: 'Heavy freezing rain',
      71: 'Slight snow fall',
      73: 'Moderate snow fall',
      75: 'Heavy snow fall',
      77: 'Snow grains',
      80: 'Slight rain showers',
      81: 'Moderate rain showers',
      82: 'Violent rain showers',
      85: 'Slight snow showers',
      86: 'Heavy snow showers',
      95: 'Thunderstorm',
      96: 'Thunderstorm with slight hail',
      99: 'Thunderstorm with heavy hail',
    };
    return descriptions[code] || 'Unknown';
  }

  private async cacheWeather(data: WeatherData): Promise<void> {
    try {
      // Use a simple localStorage cache for weather data
      const cacheEntry = {
        data,
        timestamp: Date.now(),
      };
      localStorage.setItem(this.CACHE_KEY, JSON.stringify(cacheEntry));
    } catch (error) {
      console.warn('Failed to cache weather data:', error);
    }
  }

  private async loadCachedWeather(): Promise<WeatherData | null> {
    try {
      const cached = localStorage.getItem(this.CACHE_KEY);
      if (!cached) return null;

      const entry = JSON.parse(cached);
      const age = Date.now() - entry.timestamp;

      // Return cached data if within TTL
      if (age < this.CACHE_TTL) {
        return entry.data;
      }

      return null;
    } catch (error) {
      console.warn('Failed to load cached weather:', error);
      return null;
    }
  }
}
