import { Injectable, InjectionToken, inject } from '@angular/core';

/**
 * Storage configuration
 */
export interface StorageConfig {
  prefix?: string;
  storage?: 'local' | 'session';
}

/**
 * Injection token for storage configuration
 */
export const STORAGE_CONFIG = new InjectionToken<StorageConfig>('STORAGE_CONFIG');

/**
 * Service for managing browser storage with type safety and JSON serialization.
 * Provides secure storage for authentication tokens and user preferences.
 *
 * Usage:
 * ```typescript
 * // In your app's providers (optional):
 * { provide: STORAGE_CONFIG, useValue: { prefix: 'luminous_', storage: 'local' } }
 * ```
 */
@Injectable({
  providedIn: 'root',
})
export class StorageService {
  private readonly config = inject(STORAGE_CONFIG, { optional: true });

  private get storage(): Storage {
    if (typeof window === 'undefined') {
      // SSR fallback - return a no-op storage
      return {
        length: 0,
        clear: () => {},
        getItem: () => null,
        key: () => null,
        removeItem: () => {},
        setItem: () => {},
      };
    }
    return this.config?.storage === 'session' ? sessionStorage : localStorage;
  }

  private get prefix(): string {
    return this.config?.prefix || '';
  }

  private prefixKey(key: string): string {
    return `${this.prefix}${key}`;
  }

  /**
   * Get an item from storage
   * @param key Storage key
   * @returns Parsed value or null
   */
  get<T>(key: string): T | null {
    try {
      const item = this.storage.getItem(this.prefixKey(key));
      if (item === null) {
        return null;
      }
      return JSON.parse(item) as T;
    } catch {
      return null;
    }
  }

  /**
   * Set an item in storage
   * @param key Storage key
   * @param value Value to store (will be JSON stringified)
   */
  set<T>(key: string, value: T): void {
    try {
      this.storage.setItem(this.prefixKey(key), JSON.stringify(value));
    } catch (error) {
      console.error('Failed to save to storage:', error);
    }
  }

  /**
   * Remove an item from storage
   * @param key Storage key
   */
  remove(key: string): void {
    this.storage.removeItem(this.prefixKey(key));
  }

  /**
   * Clear all items with the configured prefix from storage
   */
  clear(): void {
    if (this.prefix) {
      // Only clear items with our prefix
      const keysToRemove: string[] = [];
      for (let i = 0; i < this.storage.length; i++) {
        const key = this.storage.key(i);
        if (key && key.startsWith(this.prefix)) {
          keysToRemove.push(key);
        }
      }
      keysToRemove.forEach((key) => this.storage.removeItem(key));
    } else {
      this.storage.clear();
    }
  }

  /**
   * Check if an item exists in storage
   * @param key Storage key
   * @returns true if item exists
   */
  has(key: string): boolean {
    return this.storage.getItem(this.prefixKey(key)) !== null;
  }

  /**
   * Get the raw string value from storage without parsing
   * @param key Storage key
   * @returns Raw string value or null
   */
  getRaw(key: string): string | null {
    return this.storage.getItem(this.prefixKey(key));
  }

  /**
   * Set a raw string value in storage without JSON stringification
   * @param key Storage key
   * @param value Raw string value
   */
  setRaw(key: string, value: string): void {
    try {
      this.storage.setItem(this.prefixKey(key), value);
    } catch (error) {
      console.error('Failed to save to storage:', error);
    }
  }
}
