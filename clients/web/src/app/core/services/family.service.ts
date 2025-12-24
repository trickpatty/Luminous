import { Injectable, inject, signal, computed } from '@angular/core';
import { Observable, tap, catchError, of } from 'rxjs';
import { ApiService } from './api.service';
import { Family, FamilySettings, UpdateFamilySettingsRequest } from '../../models';

/**
 * Service for managing family (tenant) operations
 */
@Injectable({
  providedIn: 'root',
})
export class FamilyService {
  private readonly api = inject(ApiService);

  // State
  private readonly _family = signal<Family | null>(null);
  private readonly _loading = signal(false);
  private readonly _error = signal<string | null>(null);

  // Public selectors
  readonly family = this._family.asReadonly();
  readonly loading = this._loading.asReadonly();
  readonly error = this._error.asReadonly();

  // Computed values
  readonly familyName = computed(() => this._family()?.name ?? '');
  readonly memberCount = computed(() => this._family()?.memberCount ?? 0);
  readonly deviceCount = computed(() => this._family()?.deviceCount ?? 0);

  /**
   * Get family details by ID
   */
  getFamily(familyId: string): Observable<Family> {
    this._loading.set(true);
    this._error.set(null);

    return this.api.get<Family>(`families/${familyId}`).pipe(
      tap((family) => {
        this._family.set(family);
        this._loading.set(false);
      }),
      catchError((error) => {
        this._error.set(error.message || 'Failed to load family');
        this._loading.set(false);
        throw error;
      })
    );
  }

  /**
   * Update family settings
   */
  updateSettings(
    familyId: string,
    request: UpdateFamilySettingsRequest
  ): Observable<Family> {
    this._loading.set(true);
    this._error.set(null);

    return this.api.put<Family>(`families/${familyId}/settings`, request).pipe(
      tap((family) => {
        this._family.set(family);
        this._loading.set(false);
      }),
      catchError((error) => {
        this._error.set(error.message || 'Failed to update family settings');
        this._loading.set(false);
        throw error;
      })
    );
  }

  /**
   * Clear family state (on logout)
   */
  clearState(): void {
    this._family.set(null);
    this._loading.set(false);
    this._error.set(null);
  }
}
