import { Injectable, inject, signal, computed } from '@angular/core';
import { Observable, of } from 'rxjs';
import { map, catchError, tap } from 'rxjs/operators';
import { HttpParams } from '@angular/common/http';
import { ApiService } from './api.service';
import { AuthService } from './auth.service';

/**
 * Event summary DTO from API
 */
export interface EventSummaryDto {
  id: string;
  title: string;
  startTime: string;
  endTime: string;
  isAllDay: boolean;
  assigneeIds: string[];
  color?: string;
  locationText?: string;
}

/**
 * Schedule event for display
 */
export interface ScheduleEvent {
  id: string;
  title: string;
  startTime: string;
  endTime: string;
  location?: string;
  memberIds: string[];
  color?: string;
  isAllDay?: boolean;
}

/**
 * Date range for fetching events
 */
export interface DateRange {
  startDate: Date;
  endDate: Date;
}

/**
 * Service for fetching and managing calendar events.
 * Integrates with the Luminous API to retrieve events for the user's family.
 */
@Injectable({
  providedIn: 'root',
})
export class EventService {
  private readonly api = inject(ApiService);
  private readonly authService = inject(AuthService);

  // State
  private readonly _events = signal<ScheduleEvent[]>([]);
  private readonly _isLoading = signal(false);
  private readonly _error = signal<string | null>(null);

  // Public signals
  readonly events = this._events.asReadonly();
  readonly isLoading = this._isLoading.asReadonly();
  readonly error = this._error.asReadonly();

  // Computed: events for today
  readonly todayEvents = computed(() => {
    const events = this._events();
    const today = new Date();
    today.setHours(0, 0, 0, 0);
    const tomorrow = new Date(today);
    tomorrow.setDate(tomorrow.getDate() + 1);

    return events
      .filter(event => {
        const eventStart = new Date(event.startTime);
        return eventStart >= today && eventStart < tomorrow;
      })
      .sort((a, b) => new Date(a.startTime).getTime() - new Date(b.startTime).getTime());
  });

  // Computed: upcoming events (next 24 hours)
  readonly upcomingEvents = computed(() => {
    const events = this._events();
    const now = new Date();
    const in24Hours = new Date(now.getTime() + 24 * 60 * 60 * 1000);

    return events
      .filter(event => {
        const eventStart = new Date(event.startTime);
        return eventStart >= now && eventStart <= in24Hours;
      })
      .sort((a, b) => new Date(a.startTime).getTime() - new Date(b.startTime).getTime());
  });

  /**
   * Fetch events for today
   */
  fetchEventsForToday(): Observable<ScheduleEvent[]> {
    const today = new Date();
    today.setHours(0, 0, 0, 0);

    const endOfDay = new Date(today);
    endOfDay.setHours(23, 59, 59, 999);

    return this.fetchEvents({ startDate: today, endDate: endOfDay });
  }

  /**
   * Fetch events for a specific day
   */
  fetchEventsForDay(date: Date): Observable<ScheduleEvent[]> {
    const startOfDay = new Date(date);
    startOfDay.setHours(0, 0, 0, 0);

    const endOfDay = new Date(date);
    endOfDay.setHours(23, 59, 59, 999);

    return this.fetchEvents({ startDate: startOfDay, endDate: endOfDay });
  }

  /**
   * Fetch events for a week starting from a given date
   */
  fetchEventsForWeek(startDate: Date): Observable<ScheduleEvent[]> {
    const start = new Date(startDate);
    start.setHours(0, 0, 0, 0);

    const end = new Date(start);
    end.setDate(end.getDate() + 7);
    end.setHours(23, 59, 59, 999);

    return this.fetchEvents({ startDate: start, endDate: end });
  }

  /**
   * Fetch events for a month
   */
  fetchEventsForMonth(year: number, month: number): Observable<ScheduleEvent[]> {
    const startDate = new Date(year, month, 1);
    const endDate = new Date(year, month + 1, 0, 23, 59, 59, 999);

    return this.fetchEvents({ startDate, endDate });
  }

  /**
   * Fetch events within a date range
   */
  fetchEvents(range: DateRange): Observable<ScheduleEvent[]> {
    const familyId = this.getFamilyId();
    if (!familyId) {
      this._error.set('No family ID available');
      return of([]);
    }

    this._isLoading.set(true);
    this._error.set(null);

    const params = new HttpParams()
      .set('startDate', range.startDate.toISOString())
      .set('endDate', range.endDate.toISOString());

    return this.api.get<EventSummaryDto[]>(`events/family/${familyId}`, { params }).pipe(
      map(dtos => this.mapDtoToScheduleEvents(dtos)),
      tap(events => {
        this._events.set(events);
        this._isLoading.set(false);
      }),
      catchError(error => {
        console.error('Failed to fetch events:', error);
        this._error.set('Failed to load events');
        this._isLoading.set(false);
        return of([]);
      })
    );
  }

  /**
   * Format time for display
   */
  formatTime(isoTime: string, format: '12h' | '24h' = '12h'): string {
    try {
      const date = new Date(isoTime);
      if (format === '24h') {
        return date.toLocaleTimeString('en-GB', {
          hour: '2-digit',
          minute: '2-digit',
        });
      }
      return date.toLocaleTimeString('en-US', {
        hour: 'numeric',
        minute: '2-digit',
        hour12: true,
      });
    } catch {
      return isoTime;
    }
  }

  /**
   * Format date for display
   */
  formatDate(date: Date, format: 'full' | 'short' | 'day' = 'full'): string {
    switch (format) {
      case 'full':
        return date.toLocaleDateString('en-US', {
          weekday: 'long',
          month: 'long',
          day: 'numeric',
        });
      case 'short':
        return date.toLocaleDateString('en-US', {
          month: 'short',
          day: 'numeric',
        });
      case 'day':
        return date.toLocaleDateString('en-US', {
          weekday: 'short',
        });
    }
  }

  /**
   * Check if an event is happening now
   */
  isEventNow(event: ScheduleEvent): boolean {
    const now = new Date();
    const start = new Date(event.startTime);
    const end = event.endTime ? new Date(event.endTime) : new Date(start.getTime() + 60 * 60 * 1000);

    return now >= start && now <= end;
  }

  /**
   * Check if an event is in the past
   */
  isEventPast(event: ScheduleEvent): boolean {
    const now = new Date();
    const end = event.endTime ? new Date(event.endTime) : new Date(event.startTime);
    return end < now;
  }

  // ============================================
  // Private Methods
  // ============================================

  private getFamilyId(): string | null {
    return this.authService.user()?.familyId ?? null;
  }

  private mapDtoToScheduleEvents(dtos: EventSummaryDto[]): ScheduleEvent[] {
    return dtos.map(dto => ({
      id: dto.id,
      title: dto.title,
      startTime: dto.startTime,
      endTime: dto.endTime,
      location: dto.locationText,
      memberIds: dto.assigneeIds || [],
      color: dto.color,
      isAllDay: dto.isAllDay,
    }));
  }
}
