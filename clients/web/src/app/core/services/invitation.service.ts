import { Injectable, inject, signal } from '@angular/core';
import { Observable, tap, catchError } from 'rxjs';
import { ApiService } from './api.service';
import {
  Invitation,
  InvitationStatus,
  SendInvitationRequest,
  AcceptInvitationRequest,
  AcceptedInvitationResult,
} from '../../models';

/**
 * Service for managing family invitations
 */
@Injectable({
  providedIn: 'root',
})
export class InvitationService {
  private readonly api = inject(ApiService);

  // State
  private readonly _invitations = signal<Invitation[]>([]);
  private readonly _loading = signal(false);
  private readonly _error = signal<string | null>(null);

  // Public selectors
  readonly invitations = this._invitations.asReadonly();
  readonly loading = this._loading.asReadonly();
  readonly error = this._error.asReadonly();

  /**
   * Get all family invitations
   */
  getInvitations(familyId: string): Observable<Invitation[]> {
    this._loading.set(true);
    this._error.set(null);

    return this.api
      .get<Invitation[]>(`invitations/family/${familyId}`)
      .pipe(
        tap((invitations) => {
          this._invitations.set(invitations);
          this._loading.set(false);
        }),
        catchError((error) => {
          this._error.set(error.message || 'Failed to load invitations');
          this._loading.set(false);
          throw error;
        })
      );
  }

  /**
   * Get pending invitations only
   */
  getPendingInvitations(familyId: string): Observable<Invitation[]> {
    this._loading.set(true);
    this._error.set(null);

    return this.api
      .get<Invitation[]>(`invitations/family/${familyId}/pending`)
      .pipe(
        tap((invitations) => {
          this._invitations.set(invitations);
          this._loading.set(false);
        }),
        catchError((error) => {
          this._error.set(error.message || 'Failed to load invitations');
          this._loading.set(false);
          throw error;
        })
      );
  }

  /**
   * Get invitation by code (public endpoint)
   */
  getInvitationByCode(code: string): Observable<Invitation> {
    return this.api.get<Invitation>(`invitations/${code}`);
  }

  /**
   * Send a new invitation
   */
  sendInvitation(
    familyId: string,
    request: SendInvitationRequest
  ): Observable<Invitation> {
    this._loading.set(true);
    this._error.set(null);

    return this.api
      .post<Invitation>(`invitations/family/${familyId}`, request)
      .pipe(
        tap((invitation) => {
          this._invitations.update((invitations) => [
            ...invitations,
            invitation,
          ]);
          this._loading.set(false);
        }),
        catchError((error) => {
          this._error.set(error.message || 'Failed to send invitation');
          this._loading.set(false);
          throw error;
        })
      );
  }

  /**
   * Accept an invitation (public endpoint)
   */
  acceptInvitation(
    code: string,
    request: AcceptInvitationRequest
  ): Observable<AcceptedInvitationResult> {
    this._loading.set(true);

    return this.api
      .post<AcceptedInvitationResult>(`invitations/${code}/accept`, request)
      .pipe(
        tap(() => {
          this._loading.set(false);
        }),
        catchError((error) => {
          this._error.set(error.message || 'Failed to accept invitation');
          this._loading.set(false);
          throw error;
        })
      );
  }

  /**
   * Decline an invitation (public endpoint)
   */
  declineInvitation(code: string): Observable<void> {
    return this.api.post<void>(`invitations/${code}/decline`, {});
  }

  /**
   * Revoke an invitation (admin only)
   */
  revokeInvitation(familyId: string, invitationId: string): Observable<void> {
    this._loading.set(true);

    return this.api
      .delete<void>(`invitations/family/${familyId}/${invitationId}`)
      .pipe(
        tap(() => {
          this._invitations.update((invitations) =>
            invitations.filter((i) => i.id !== invitationId)
          );
          this._loading.set(false);
        }),
        catchError((error) => {
          this._error.set(error.message || 'Failed to revoke invitation');
          this._loading.set(false);
          throw error;
        })
      );
  }

  /**
   * Clear state (on logout)
   */
  clearState(): void {
    this._invitations.set([]);
    this._loading.set(false);
    this._error.set(null);
  }

  /**
   * Get status display properties
   */
  getStatusDisplay(status: InvitationStatus): {
    label: string;
    color: string;
    bgColor: string;
  } {
    const displays: Record<
      InvitationStatus,
      { label: string; color: string; bgColor: string }
    > = {
      [InvitationStatus.Pending]: {
        label: 'Pending',
        color: 'text-yellow-700',
        bgColor: 'bg-yellow-100',
      },
      [InvitationStatus.Accepted]: {
        label: 'Accepted',
        color: 'text-green-700',
        bgColor: 'bg-green-100',
      },
      [InvitationStatus.Declined]: {
        label: 'Declined',
        color: 'text-red-700',
        bgColor: 'bg-red-100',
      },
      [InvitationStatus.Expired]: {
        label: 'Expired',
        color: 'text-gray-700',
        bgColor: 'bg-gray-100',
      },
      [InvitationStatus.Revoked]: {
        label: 'Revoked',
        color: 'text-gray-700',
        bgColor: 'bg-gray-100',
      },
    };
    return displays[status] || displays[InvitationStatus.Pending];
  }
}
