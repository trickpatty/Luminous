import { Injectable, inject, signal } from '@angular/core';
import { Observable, tap, catchError } from 'rxjs';
import { ApiService } from './api.service';
import {
  User,
  UserRole,
  UpdateUserProfileRequest,
  UpdateUserRoleRequest,
  UpdateCaregiverInfoRequest,
  CaregiverAccessToken,
} from '../../models';

/**
 * Service for managing family members
 */
@Injectable({
  providedIn: 'root',
})
export class UserService {
  private readonly api = inject(ApiService);

  // State
  private readonly _members = signal<User[]>([]);
  private readonly _loading = signal(false);
  private readonly _error = signal<string | null>(null);

  // Public selectors
  readonly members = this._members.asReadonly();
  readonly loading = this._loading.asReadonly();
  readonly error = this._error.asReadonly();

  /**
   * Get all family members
   */
  getMembers(familyId: string): Observable<User[]> {
    this._loading.set(true);
    this._error.set(null);

    return this.api.get<User[]>(`users/family/${familyId}`).pipe(
      tap((members) => {
        this._members.set(members);
        this._loading.set(false);
      }),
      catchError((error) => {
        this._error.set(error.message || 'Failed to load family members');
        this._loading.set(false);
        throw error;
      })
    );
  }

  /**
   * Get a specific user
   */
  getUser(familyId: string, userId: string): Observable<User> {
    return this.api.get<User>(`users/family/${familyId}/${userId}`);
  }

  /**
   * Update user profile
   */
  updateProfile(
    familyId: string,
    userId: string,
    request: UpdateUserProfileRequest
  ): Observable<User> {
    this._loading.set(true);

    return this.api
      .put<User>(`users/family/${familyId}/${userId}/profile`, request)
      .pipe(
        tap((user) => {
          // Update the member in the list
          this._members.update((members) =>
            members.map((m) => (m.id === user.id ? user : m))
          );
          this._loading.set(false);
        }),
        catchError((error) => {
          this._error.set(error.message || 'Failed to update profile');
          this._loading.set(false);
          throw error;
        })
      );
  }

  /**
   * Update user role (admin only)
   */
  updateRole(
    familyId: string,
    userId: string,
    request: UpdateUserRoleRequest
  ): Observable<User> {
    this._loading.set(true);

    return this.api
      .put<User>(`users/family/${familyId}/${userId}/role`, request)
      .pipe(
        tap((user) => {
          this._members.update((members) =>
            members.map((m) => (m.id === user.id ? user : m))
          );
          this._loading.set(false);
        }),
        catchError((error) => {
          this._error.set(error.message || 'Failed to update role');
          this._loading.set(false);
          throw error;
        })
      );
  }

  /**
   * Update caregiver info for a user
   */
  updateCaregiverInfo(
    familyId: string,
    userId: string,
    request: UpdateCaregiverInfoRequest
  ): Observable<User> {
    return this.api
      .put<User>(`users/family/${familyId}/${userId}/caregiver-info`, request)
      .pipe(
        tap((user) => {
          this._members.update((members) =>
            members.map((m) => (m.id === user.id ? user : m))
          );
        })
      );
  }

  /**
   * Generate caregiver access token
   */
  generateCaregiverToken(
    familyId: string,
    userId: string,
    expirationHours: number = 24
  ): Observable<CaregiverAccessToken> {
    return this.api.post<CaregiverAccessToken>(
      `users/family/${familyId}/${userId}/caregiver-token`,
      { expirationHours }
    );
  }

  /**
   * Remove user from family (admin only)
   */
  removeUser(familyId: string, userId: string): Observable<void> {
    this._loading.set(true);

    return this.api.delete<void>(`users/family/${familyId}/${userId}`).pipe(
      tap(() => {
        this._members.update((members) =>
          members.filter((m) => m.id !== userId)
        );
        this._loading.set(false);
      }),
      catchError((error) => {
        this._error.set(error.message || 'Failed to remove user');
        this._loading.set(false);
        throw error;
      })
    );
  }

  /**
   * Clear state (on logout)
   */
  clearState(): void {
    this._members.set([]);
    this._loading.set(false);
    this._error.set(null);
  }

  /**
   * Get role display name
   */
  getRoleDisplayName(role: UserRole): string {
    const names: Record<UserRole, string> = {
      [UserRole.Owner]: 'Owner',
      [UserRole.Admin]: 'Admin',
      [UserRole.Adult]: 'Adult',
      [UserRole.Teen]: 'Teen',
      [UserRole.Child]: 'Child',
      [UserRole.Caregiver]: 'Caregiver',
    };
    return names[role] || role;
  }

  /**
   * Get available roles for assignment (based on current user's role)
   */
  getAssignableRoles(currentUserRole: UserRole): UserRole[] {
    // Owner can assign all roles except Owner
    if (currentUserRole === UserRole.Owner) {
      return [
        UserRole.Admin,
        UserRole.Adult,
        UserRole.Teen,
        UserRole.Child,
        UserRole.Caregiver,
      ];
    }
    // Admin can assign roles below Admin
    if (currentUserRole === UserRole.Admin) {
      return [UserRole.Adult, UserRole.Teen, UserRole.Child, UserRole.Caregiver];
    }
    return [];
  }
}
