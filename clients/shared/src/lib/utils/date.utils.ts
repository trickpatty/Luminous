/**
 * Date utility functions for Luminous applications.
 */

/**
 * Format a date for display
 * @param date Date to format
 * @param options Intl.DateTimeFormat options
 * @param locale Locale string (defaults to user's locale)
 */
export function formatDate(
  date: Date | string,
  options?: Intl.DateTimeFormatOptions,
  locale?: string
): string {
  const d = typeof date === 'string' ? new Date(date) : date;
  return d.toLocaleDateString(locale, options);
}

/**
 * Format a time for display
 * @param date Date to format
 * @param options Intl.DateTimeFormat options
 * @param locale Locale string
 */
export function formatTime(
  date: Date | string,
  options?: Intl.DateTimeFormatOptions,
  locale?: string
): string {
  const d = typeof date === 'string' ? new Date(date) : date;
  const defaultOptions: Intl.DateTimeFormatOptions = {
    hour: 'numeric',
    minute: '2-digit',
    ...options,
  };
  return d.toLocaleTimeString(locale, defaultOptions);
}

/**
 * Format a date and time for display
 * @param date Date to format
 * @param options Intl.DateTimeFormat options
 * @param locale Locale string
 */
export function formatDateTime(
  date: Date | string,
  options?: Intl.DateTimeFormatOptions,
  locale?: string
): string {
  const d = typeof date === 'string' ? new Date(date) : date;
  const defaultOptions: Intl.DateTimeFormatOptions = {
    month: 'short',
    day: 'numeric',
    hour: 'numeric',
    minute: '2-digit',
    ...options,
  };
  return d.toLocaleDateString(locale, defaultOptions);
}

/**
 * Get relative time string (e.g., "2 hours ago", "in 3 days")
 * @param date Date to compare
 * @param baseDate Base date for comparison (defaults to now)
 * @param locale Locale string
 */
export function getRelativeTime(
  date: Date | string,
  baseDate?: Date,
  locale?: string
): string {
  const d = typeof date === 'string' ? new Date(date) : date;
  const base = baseDate || new Date();
  const diffMs = d.getTime() - base.getTime();
  const diffSec = Math.round(diffMs / 1000);
  const diffMin = Math.round(diffSec / 60);
  const diffHours = Math.round(diffMin / 60);
  const diffDays = Math.round(diffHours / 24);
  const diffWeeks = Math.round(diffDays / 7);
  const diffMonths = Math.round(diffDays / 30);
  const diffYears = Math.round(diffDays / 365);

  const rtf = new Intl.RelativeTimeFormat(locale, { numeric: 'auto' });

  if (Math.abs(diffSec) < 60) {
    return rtf.format(diffSec, 'second');
  } else if (Math.abs(diffMin) < 60) {
    return rtf.format(diffMin, 'minute');
  } else if (Math.abs(diffHours) < 24) {
    return rtf.format(diffHours, 'hour');
  } else if (Math.abs(diffDays) < 7) {
    return rtf.format(diffDays, 'day');
  } else if (Math.abs(diffWeeks) < 4) {
    return rtf.format(diffWeeks, 'week');
  } else if (Math.abs(diffMonths) < 12) {
    return rtf.format(diffMonths, 'month');
  } else {
    return rtf.format(diffYears, 'year');
  }
}

/**
 * Check if two dates are the same day
 */
export function isSameDay(date1: Date | string, date2: Date | string): boolean {
  const d1 = typeof date1 === 'string' ? new Date(date1) : date1;
  const d2 = typeof date2 === 'string' ? new Date(date2) : date2;
  return (
    d1.getFullYear() === d2.getFullYear() &&
    d1.getMonth() === d2.getMonth() &&
    d1.getDate() === d2.getDate()
  );
}

/**
 * Check if a date is today
 */
export function isToday(date: Date | string): boolean {
  return isSameDay(date, new Date());
}

/**
 * Check if a date is tomorrow
 */
export function isTomorrow(date: Date | string): boolean {
  const tomorrow = new Date();
  tomorrow.setDate(tomorrow.getDate() + 1);
  return isSameDay(date, tomorrow);
}

/**
 * Check if a date is yesterday
 */
export function isYesterday(date: Date | string): boolean {
  const yesterday = new Date();
  yesterday.setDate(yesterday.getDate() - 1);
  return isSameDay(date, yesterday);
}

/**
 * Get start of day
 */
export function startOfDay(date: Date | string): Date {
  const d = typeof date === 'string' ? new Date(date) : new Date(date);
  d.setHours(0, 0, 0, 0);
  return d;
}

/**
 * Get end of day
 */
export function endOfDay(date: Date | string): Date {
  const d = typeof date === 'string' ? new Date(date) : new Date(date);
  d.setHours(23, 59, 59, 999);
  return d;
}

/**
 * Add days to a date
 */
export function addDays(date: Date | string, days: number): Date {
  const d = typeof date === 'string' ? new Date(date) : new Date(date);
  d.setDate(d.getDate() + days);
  return d;
}

/**
 * Get the start of the week (respects weekStartDay)
 * @param date Date to get start of week for
 * @param weekStartDay 0 = Sunday, 1 = Monday, etc.
 */
export function startOfWeek(date: Date | string, weekStartDay = 0): Date {
  const d = typeof date === 'string' ? new Date(date) : new Date(date);
  const day = d.getDay();
  const diff = (day - weekStartDay + 7) % 7;
  d.setDate(d.getDate() - diff);
  return startOfDay(d);
}
