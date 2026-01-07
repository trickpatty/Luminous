import { Injectable, inject, signal, computed } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';
import { environment } from '../../../environments/environment';
import { DeviceAuthService } from './device-auth.service';
import { CacheService, ScheduleEvent, MemberData } from './cache.service';

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
 * API response for events list
 */
interface EventsResponse {
  events: EventSummaryDto[];
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
 * Integrates with the API and local cache for offline support.
 */
@Injectable({
  providedIn: 'root',
})
export class EventService {
  private readonly http = inject(HttpClient);
  private readonly authService = inject(DeviceAuthService);
  private readonly cacheService = inject(CacheService);

  // State
  private readonly _events = signal<ScheduleEvent[]>([]);
  private readonly _isLoading = signal(false);
  private readonly _error = signal<string | null>(null);
  private readonly _selectedDate = signal<Date>(new Date());
  private readonly _selectedMemberIds = signal<string[]>([]);

  // Public signals
  readonly events = this._events.asReadonly();
  readonly isLoading = this._isLoading.asReadonly();
  readonly error = this._error.asReadonly();
  readonly selectedDate = this._selectedDate.asReadonly();
  readonly selectedMemberIds = this._selectedMemberIds.asReadonly();

  // Computed: filtered events based on selected members
  readonly filteredEvents = computed(() => {
    const events = this._events();
    const memberIds = this._selectedMemberIds();

    if (memberIds.length === 0) {
      return events; // No filter applied, show all
    }

    return events.filter(event =>
      event.memberIds.length === 0 || // Show events without assignees
      event.memberIds.some(id => memberIds.includes(id))
    );
  });

  // Computed: events grouped by date
  readonly eventsByDate = computed(() => {
    const events = this.filteredEvents();
    const grouped = new Map<string, ScheduleEvent[]>();

    for (const event of events) {
      const dateKey = this.getDateKey(new Date(event.startTime));
      const existing = grouped.get(dateKey) || [];
      grouped.set(dateKey, [...existing, event]);
    }

    // Sort events within each date
    for (const [key, dateEvents] of grouped) {
      grouped.set(key, dateEvents.sort((a, b) =>
        new Date(a.startTime).getTime() - new Date(b.startTime).getTime()
      ));
    }

    return grouped;
  });

  /**
   * Set the selected date for calendar views
   */
  setSelectedDate(date: Date): void {
    this._selectedDate.set(date);
  }

  /**
   * Set member filter (empty array = no filter)
   */
  setMemberFilter(memberIds: string[]): void {
    this._selectedMemberIds.set(memberIds);
  }

  /**
   * Toggle a member in the filter
   */
  toggleMemberFilter(memberId: string): void {
    const current = this._selectedMemberIds();
    if (current.includes(memberId)) {
      this._selectedMemberIds.set(current.filter(id => id !== memberId));
    } else {
      this._selectedMemberIds.set([...current, memberId]);
    }
  }

  /**
   * Clear all member filters
   */
  clearMemberFilter(): void {
    this._selectedMemberIds.set([]);
  }

  /**
   * Fetch events for a specific day
   */
  async fetchEventsForDay(date: Date): Promise<ScheduleEvent[]> {
    const startOfDay = new Date(date);
    startOfDay.setHours(0, 0, 0, 0);

    const endOfDay = new Date(date);
    endOfDay.setHours(23, 59, 59, 999);

    return this.fetchEvents({ startDate: startOfDay, endDate: endOfDay });
  }

  /**
   * Fetch events for the next 24 hours from a given date
   */
  async fetchEventsNext24Hours(fromDate: Date = new Date()): Promise<ScheduleEvent[]> {
    const endDate = new Date(fromDate);
    endDate.setHours(endDate.getHours() + 24);

    return this.fetchEvents({ startDate: fromDate, endDate });
  }

  /**
   * Fetch events for a week starting from a given date
   */
  async fetchEventsForWeek(startDate: Date): Promise<ScheduleEvent[]> {
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
  async fetchEventsForMonth(year: number, month: number): Promise<ScheduleEvent[]> {
    const startDate = new Date(year, month, 1);
    const endDate = new Date(year, month + 1, 0, 23, 59, 59, 999);

    return this.fetchEvents({ startDate, endDate });
  }

  /**
   * Fetch events within a date range
   */
  async fetchEvents(range: DateRange): Promise<ScheduleEvent[]> {
    this._isLoading.set(true);
    this._error.set(null);

    try {
      const familyId = this.getFamilyId();
      if (!familyId) {
        throw new Error('No family ID available');
      }

      const params = new HttpParams()
        .set('startDate', range.startDate.toISOString())
        .set('endDate', range.endDate.toISOString());

      const response = await firstValueFrom(
        this.http.get<EventSummaryDto[]>(
          `${environment.apiUrl}/events/family/${familyId}`,
          { params }
        )
      );

      const events = this.mapDtoToScheduleEvents(response);
      this._events.set(events);

      // Cache the events for each day
      await this.cacheEventsByDay(events);

      return events;
    } catch (error) {
      console.error('Failed to fetch events:', error);
      this._error.set('Failed to load events');

      // Try to load from cache
      const cached = await this.loadFromCache(range);
      if (cached.length > 0) {
        this._events.set(cached);
        return cached;
      }

      return [];
    } finally {
      this._isLoading.set(false);
    }
  }

  /**
   * Get events for a specific day from current loaded events
   */
  getEventsForDay(date: Date): ScheduleEvent[] {
    const dateKey = this.getDateKey(date);
    return this.eventsByDate().get(dateKey) || [];
  }

  /**
   * Get events for the next N hours
   */
  getUpcomingEvents(hours: number): ScheduleEvent[] {
    const now = new Date();
    const endTime = new Date(now.getTime() + hours * 60 * 60 * 1000);

    return this.filteredEvents().filter(event => {
      const eventStart = new Date(event.startTime);
      return eventStart >= now && eventStart <= endTime;
    });
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

  /**
   * Get the first event color or member color
   */
  getEventColor(event: ScheduleEvent, members: MemberData[]): string {
    if (event.color) return event.color;
    if (event.memberIds.length > 0) {
      const member = members.find(m => m.id === event.memberIds[0]);
      if (member) return member.color;
    }
    return 'var(--accent-500)';
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

  // ============================================
  // Private Methods
  // ============================================

  private getFamilyId(): string | null {
    const token = this.authService.deviceToken();
    return token?.familyId ?? null;
  }

  private getDateKey(date: Date): string {
    return date.toISOString().split('T')[0];
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
    }));
  }

  private async cacheEventsByDay(events: ScheduleEvent[]): Promise<void> {
    // Group events by day
    const eventsByDay = new Map<string, ScheduleEvent[]>();

    for (const event of events) {
      const dateKey = this.getDateKey(new Date(event.startTime));
      const existing = eventsByDay.get(dateKey) || [];
      eventsByDay.set(dateKey, [...existing, event]);
    }

    // Cache each day
    for (const [date, dayEvents] of eventsByDay) {
      await this.cacheService.cacheSchedule(date, dayEvents);
    }
  }

  private async loadFromCache(range: DateRange): Promise<ScheduleEvent[]> {
    const events: ScheduleEvent[] = [];
    const current = new Date(range.startDate);

    while (current <= range.endDate) {
      const dateKey = this.getDateKey(current);
      const cached = await this.cacheService.getSchedule(dateKey);

      if (cached?.events) {
        events.push(...cached.events);
      }

      current.setDate(current.getDate() + 1);
    }

    return events;
  }
}
