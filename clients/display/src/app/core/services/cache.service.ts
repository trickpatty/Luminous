import { Injectable } from '@angular/core';
import { openDB, DBSchema, IDBPDatabase } from 'idb';
import { environment } from '../../../environments/environment';

interface LuminousCacheDB extends DBSchema {
  'schedule': {
    key: string;
    value: CacheEntry<ScheduleData>;
    indexes: { 'by-date': string };
  };
  'tasks': {
    key: string;
    value: CacheEntry<TaskData[]>;
  };
  'family': {
    key: string;
    value: CacheEntry<FamilyData>;
  };
  'members': {
    key: string;
    value: CacheEntry<MemberData[]>;
  };
}

interface CacheEntry<T> {
  data: T;
  timestamp: number;
  expiresAt: number;
}

// Data types (simplified for caching)
export interface ScheduleEvent {
  id: string;
  title: string;
  /** Start time for timed events (ISO 8601). Null for all-day events. */
  startTime?: string | null;
  /** End time for timed events (ISO 8601). Null for all-day events. */
  endTime?: string | null;
  /** Start date for all-day events (YYYY-MM-DD). Null for timed events. */
  startDate?: string | null;
  /** End date for all-day events (YYYY-MM-DD, exclusive). Null for timed events. */
  endDate?: string | null;
  isAllDay: boolean;
  location?: string;
  memberIds: string[];
  color?: string;
}

export interface ScheduleData {
  date: string;
  events: ScheduleEvent[];
}

export interface TaskData {
  id: string;
  title: string;
  dueDate?: string;
  completed: boolean;
  completedAt?: string;
  assigneeId?: string;
  priority?: 'low' | 'medium' | 'high';
}

export interface FamilyData {
  id: string;
  name: string;
  createdAt: string;
}

export interface MemberData {
  id: string;
  name: string;
  color: string;
  avatarUrl?: string;
  initials: string;
}

/**
 * Service for offline caching using IndexedDB.
 * Provides resilient data access when network is unavailable.
 */
@Injectable({
  providedIn: 'root',
})
export class CacheService {
  private db: IDBPDatabase<LuminousCacheDB> | null = null;
  private readonly maxAge = environment.cache.maxAge;
  private readonly storeName = environment.cache.storeName;

  constructor() {
    this.initDatabase();
  }

  /**
   * Initialize IndexedDB database
   */
  private async initDatabase(): Promise<void> {
    try {
      this.db = await openDB<LuminousCacheDB>(this.storeName, 1, {
        upgrade(db) {
          // Schedule store
          if (!db.objectStoreNames.contains('schedule')) {
            const scheduleStore = db.createObjectStore('schedule', { keyPath: 'data.date' });
            scheduleStore.createIndex('by-date', 'data.date');
          }

          // Tasks store
          if (!db.objectStoreNames.contains('tasks')) {
            db.createObjectStore('tasks');
          }

          // Family store
          if (!db.objectStoreNames.contains('family')) {
            db.createObjectStore('family');
          }

          // Members store
          if (!db.objectStoreNames.contains('members')) {
            db.createObjectStore('members');
          }
        },
      });
    } catch (error) {
      console.error('Failed to initialize cache database:', error);
    }
  }

  /**
   * Ensure database is ready
   */
  private async ensureDb(): Promise<IDBPDatabase<LuminousCacheDB>> {
    if (!this.db) {
      await this.initDatabase();
    }
    if (!this.db) {
      throw new Error('Cache database not available');
    }
    return this.db;
  }

  // ============================================
  // Schedule Caching
  // ============================================

  async cacheSchedule(date: string, events: ScheduleEvent[]): Promise<void> {
    try {
      const db = await this.ensureDb();
      const entry: CacheEntry<ScheduleData> = {
        data: { date, events },
        timestamp: Date.now(),
        expiresAt: Date.now() + this.maxAge,
      };
      await db.put('schedule', entry);
    } catch (error) {
      console.error('Failed to cache schedule:', error);
    }
  }

  async getSchedule(date: string): Promise<ScheduleData | null> {
    try {
      const db = await this.ensureDb();
      const entry = await db.get('schedule', date);

      if (!entry) return null;
      if (Date.now() > entry.expiresAt) {
        await db.delete('schedule', date);
        return null;
      }

      return entry.data;
    } catch (error) {
      console.error('Failed to get cached schedule:', error);
      return null;
    }
  }

  // ============================================
  // Tasks Caching
  // ============================================

  async cacheTasks(tasks: TaskData[]): Promise<void> {
    try {
      const db = await this.ensureDb();
      const entry: CacheEntry<TaskData[]> = {
        data: tasks,
        timestamp: Date.now(),
        expiresAt: Date.now() + this.maxAge,
      };
      await db.put('tasks', entry, 'current');
    } catch (error) {
      console.error('Failed to cache tasks:', error);
    }
  }

  async getTasks(): Promise<TaskData[] | null> {
    try {
      const db = await this.ensureDb();
      const entry = await db.get('tasks', 'current');

      if (!entry) return null;
      if (Date.now() > entry.expiresAt) {
        await db.delete('tasks', 'current');
        return null;
      }

      return entry.data;
    } catch (error) {
      console.error('Failed to get cached tasks:', error);
      return null;
    }
  }

  // ============================================
  // Family Caching
  // ============================================

  async cacheFamily(family: FamilyData): Promise<void> {
    try {
      const db = await this.ensureDb();
      const entry: CacheEntry<FamilyData> = {
        data: family,
        timestamp: Date.now(),
        expiresAt: Date.now() + this.maxAge * 24, // Family data expires slower
      };
      await db.put('family', entry, 'current');
    } catch (error) {
      console.error('Failed to cache family:', error);
    }
  }

  async getFamily(): Promise<FamilyData | null> {
    try {
      const db = await this.ensureDb();
      const entry = await db.get('family', 'current');

      if (!entry) return null;
      if (Date.now() > entry.expiresAt) {
        await db.delete('family', 'current');
        return null;
      }

      return entry.data;
    } catch (error) {
      console.error('Failed to get cached family:', error);
      return null;
    }
  }

  // ============================================
  // Members Caching
  // ============================================

  async cacheMembers(members: MemberData[]): Promise<void> {
    try {
      const db = await this.ensureDb();
      const entry: CacheEntry<MemberData[]> = {
        data: members,
        timestamp: Date.now(),
        expiresAt: Date.now() + this.maxAge * 24, // Member data expires slower
      };
      await db.put('members', entry, 'current');
    } catch (error) {
      console.error('Failed to cache members:', error);
    }
  }

  async getMembers(): Promise<MemberData[] | null> {
    try {
      const db = await this.ensureDb();
      const entry = await db.get('members', 'current');

      if (!entry) return null;
      if (Date.now() > entry.expiresAt) {
        await db.delete('members', 'current');
        return null;
      }

      return entry.data;
    } catch (error) {
      console.error('Failed to get cached members:', error);
      return null;
    }
  }

  // ============================================
  // Cache Management
  // ============================================

  /**
   * Clear all cached data
   */
  async clearAll(): Promise<void> {
    try {
      const db = await this.ensureDb();
      await Promise.all([
        db.clear('schedule'),
        db.clear('tasks'),
        db.clear('family'),
        db.clear('members'),
      ]);
    } catch (error) {
      console.error('Failed to clear cache:', error);
    }
  }

  /**
   * Clear expired entries from all stores
   */
  async cleanupExpired(): Promise<void> {
    try {
      const db = await this.ensureDb();
      const now = Date.now();

      // Clean schedule
      const schedules = await db.getAll('schedule');
      for (const entry of schedules) {
        if (now > entry.expiresAt) {
          await db.delete('schedule', entry.data.date);
        }
      }

      // Clean tasks
      const tasksEntry = await db.get('tasks', 'current');
      if (tasksEntry && now > tasksEntry.expiresAt) {
        await db.delete('tasks', 'current');
      }

      // Clean family
      const familyEntry = await db.get('family', 'current');
      if (familyEntry && now > familyEntry.expiresAt) {
        await db.delete('family', 'current');
      }

      // Clean members
      const membersEntry = await db.get('members', 'current');
      if (membersEntry && now > membersEntry.expiresAt) {
        await db.delete('members', 'current');
      }
    } catch (error) {
      console.error('Failed to cleanup expired cache:', error);
    }
  }

  /**
   * Get cache statistics
   */
  async getStats(): Promise<{ scheduleCount: number; hasTasks: boolean; hasFamily: boolean; hasMembers: boolean }> {
    try {
      const db = await this.ensureDb();
      const [schedules, tasks, family, members] = await Promise.all([
        db.count('schedule'),
        db.get('tasks', 'current'),
        db.get('family', 'current'),
        db.get('members', 'current'),
      ]);

      return {
        scheduleCount: schedules,
        hasTasks: !!tasks,
        hasFamily: !!family,
        hasMembers: !!members,
      };
    } catch (error) {
      console.error('Failed to get cache stats:', error);
      return { scheduleCount: 0, hasTasks: false, hasFamily: false, hasMembers: false };
    }
  }
}
