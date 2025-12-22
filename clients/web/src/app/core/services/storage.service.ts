import { Injectable } from '@angular/core';

/**
 * Service for managing local storage with type safety and JSON serialization.
 * Provides secure storage for authentication tokens and user preferences.
 */
@Injectable({
  providedIn: 'root',
})
export class StorageService {
  private readonly storage: Storage = localStorage;

  /**
   * Get an item from storage
   * @param key Storage key
   * @returns Parsed value or null
   */
  get<T>(key: string): T | null {
    try {
      const item = this.storage.getItem(key);
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
      this.storage.setItem(key, JSON.stringify(value));
    } catch (error) {
      console.error('Failed to save to storage:', error);
    }
  }

  /**
   * Remove an item from storage
   * @param key Storage key
   */
  remove(key: string): void {
    this.storage.removeItem(key);
  }

  /**
   * Clear all items from storage
   */
  clear(): void {
    this.storage.clear();
  }

  /**
   * Check if an item exists in storage
   * @param key Storage key
   * @returns true if item exists
   */
  has(key: string): boolean {
    return this.storage.getItem(key) !== null;
  }
}
