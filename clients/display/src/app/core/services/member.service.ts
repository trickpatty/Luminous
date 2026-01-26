import { Injectable, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { Subject } from 'rxjs';
import { environment } from '../../../environments/environment';
import { DeviceAuthService } from './device-auth.service';
import { CacheService, MemberData } from './cache.service';
import { SyncService } from './sync.service';

/**
 * Profile DTO from API
 */
interface ProfileDto {
  id: string;
  displayName: string;
  color: string;
  avatarUrl?: string;
}

/**
 * API response for profiles
 */
interface ProfilesResponse {
  success: boolean;
  data: ProfileDto[];
}

/**
 * Service for fetching and managing family member data.
 * Provides member information for calendar views and event assignments.
 */
@Injectable({
  providedIn: 'root',
})
export class MemberService {
  private readonly http = inject(HttpClient);
  private readonly authService = inject(DeviceAuthService);
  private readonly cacheService = inject(CacheService);
  private readonly syncService = inject(SyncService);

  private readonly destroy$ = new Subject<void>();

  // State
  private readonly _members = signal<MemberData[]>([]);
  private readonly _isLoading = signal(false);
  private readonly _error = signal<string | null>(null);

  // Public signals
  readonly members = this._members.asReadonly();
  readonly isLoading = this._isLoading.asReadonly();
  readonly error = this._error.asReadonly();

  constructor() {
    this.setupSyncSubscriptions();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  /**
   * Fetch members from the API
   */
  async fetchMembers(): Promise<MemberData[]> {
    this._isLoading.set(true);
    this._error.set(null);

    try {
      const familyId = this.getFamilyId();
      if (!familyId) {
        throw new Error('No family ID available');
      }

      const response = await firstValueFrom(
        this.http.get<ProfilesResponse>(
          `${environment.apiUrl}/families/${familyId}/profiles`
        )
      );

      const members = this.mapProfilesToMembers(response.data || []);
      this._members.set(members);

      // Cache the members
      await this.cacheService.cacheMembers(members);

      return members;
    } catch (error) {
      console.error('Failed to fetch members:', error);
      this._error.set('Failed to load family members');

      // Try to load from cache
      const cached = await this.cacheService.getMembers();
      if (cached && cached.length > 0) {
        this._members.set(cached);
        return cached;
      }

      return [];
    } finally {
      this._isLoading.set(false);
    }
  }

  /**
   * Load members from cache only (for offline scenarios)
   */
  async loadFromCache(): Promise<MemberData[]> {
    const cached = await this.cacheService.getMembers();
    if (cached) {
      this._members.set(cached);
      return cached;
    }
    return [];
  }

  /**
   * Get a member by ID
   */
  getMember(memberId: string): MemberData | undefined {
    return this._members().find(m => m.id === memberId);
  }

  /**
   * Get initials from a name
   */
  private getInitials(name: string): string {
    return name
      .split(' ')
      .map(part => part.charAt(0))
      .join('')
      .toUpperCase()
      .slice(0, 2);
  }

  /**
   * Map profile DTOs to member data
   */
  private mapProfilesToMembers(profiles: ProfileDto[]): MemberData[] {
    return profiles.map(profile => ({
      id: profile.id,
      name: profile.displayName,
      color: profile.color || this.getDefaultColor(profile.id),
      avatarUrl: profile.avatarUrl,
      initials: this.getInitials(profile.displayName),
    }));
  }

  /**
   * Get a default color based on the member ID
   */
  private getDefaultColor(id: string): string {
    const colors = [
      '#3b82f6', // blue
      '#10b981', // emerald
      '#f59e0b', // amber
      '#ef4444', // red
      '#8b5cf6', // violet
      '#ec4899', // pink
      '#06b6d4', // cyan
      '#84cc16', // lime
    ];
    const hash = id.split('').reduce((acc, char) => acc + char.charCodeAt(0), 0);
    return colors[hash % colors.length];
  }

  private getFamilyId(): string | null {
    const token = this.authService.deviceToken();
    return token?.familyId ?? null;
  }

  /**
   * Setup subscriptions to sync service for real-time updates
   */
  private setupSyncSubscriptions(): void {
    // Refresh members when users change
    this.syncService.onUserChanged()
      .pipe(takeUntil(this.destroy$))
      .subscribe(() => this.fetchMembers());

    // Refresh on full sync
    this.syncService.onFullSyncRequired()
      .pipe(takeUntil(this.destroy$))
      .subscribe(() => this.fetchMembers());
  }
}
