/**
 * Event source type
 */
export enum EventSource {
  Internal = 'Internal',
  Google = 'Google',
  Outlook = 'Outlook',
  ICloud = 'ICloud',
  CalDav = 'CalDav',
  IcsUrl = 'IcsUrl',
}

/**
 * Recurrence frequency
 */
export enum RecurrenceFrequency {
  Daily = 'Daily',
  Weekly = 'Weekly',
  Monthly = 'Monthly',
  Yearly = 'Yearly',
}

/**
 * Recurrence rule for recurring events
 */
export interface RecurrenceRule {
  frequency: RecurrenceFrequency;
  interval: number;
  daysOfWeek?: number[];
  until?: string;
  count?: number;
}

/**
 * Event location
 */
export interface EventLocation {
  name?: string;
  address?: string;
  latitude?: number;
  longitude?: number;
}

/**
 * Event reminder
 */
export interface EventReminder {
  minutesBefore: number;
  method: 'push' | 'email' | 'display';
}

/**
 * Calendar event entity
 */
export interface CalendarEvent {
  id: string;
  familyId: string;
  calendarConnectionId?: string;
  externalEventId?: string;
  source: EventSource;
  title: string;
  description?: string;
  location?: EventLocation;
  locationText?: string;
  startTime?: string;
  endTime?: string;
  startDate?: string;
  endDate?: string;
  isAllDay: boolean;
  timezone?: string;
  recurrenceRule?: RecurrenceRule;
  recurrenceId?: string;
  assigneeIds: string[];
  color?: string;
  reminders: EventReminder[];
  isCancelled: boolean;
  createdAt: string;
  updatedAt?: string;
  syncedAt?: string;
}

/**
 * Create event request
 */
export interface CreateEventRequest {
  title: string;
  description?: string;
  location?: EventLocation;
  startTime?: string;
  endTime?: string;
  startDate?: string;
  endDate?: string;
  isAllDay: boolean;
  timezone?: string;
  recurrenceRule?: RecurrenceRule;
  assigneeIds: string[];
  color?: string;
  reminders?: EventReminder[];
}

/**
 * Update event request
 */
export interface UpdateEventRequest {
  title?: string;
  description?: string;
  location?: EventLocation;
  startTime?: string;
  endTime?: string;
  startDate?: string;
  endDate?: string;
  isAllDay?: boolean;
  timezone?: string;
  recurrenceRule?: RecurrenceRule;
  assigneeIds?: string[];
  color?: string;
  reminders?: EventReminder[];
}

/**
 * Event query parameters
 */
export interface EventQueryParams {
  startDate?: string;
  endDate?: string;
  assigneeIds?: string[];
  sources?: EventSource[];
  includeRecurring?: boolean;
}
