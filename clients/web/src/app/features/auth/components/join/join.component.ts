import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, ActivatedRoute, RouterLink } from '@angular/router';
import { InvitationService, WebAuthnService, StorageService } from '../../../../core';
import {
  Invitation,
  InvitationStatus,
  AcceptInvitationRequest,
  AcceptedInvitationResult,
} from '../../../../models';
import { environment } from '../../../../../environments/environment';

type JoinStep = 'loading' | 'invalid' | 'details' | 'passkey' | 'complete';

@Component({
  selector: 'app-join',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './join.component.html',
})
export class JoinComponent implements OnInit {
  private readonly invitationService = inject(InvitationService);
  private readonly webAuthnService = inject(WebAuthnService);
  private readonly storageService = inject(StorageService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);

  // Invitation state
  code = '';
  invitation = signal<Invitation | null>(null);

  // Form state
  displayName = '';
  nickname = '';
  color = '#6366F1'; // Default primary color

  // UI state
  step = signal<JoinStep>('loading');
  isLoading = signal(false);
  error = signal<string | null>(null);
  passkeySupported = signal(false);

  // Auth result for passkey setup
  private authResult: AcceptedInvitationResult | null = null;

  // Available avatar colors
  readonly colors = [
    '#6366F1', // Indigo
    '#8B5CF6', // Violet
    '#EC4899', // Pink
    '#EF4444', // Red
    '#F97316', // Orange
    '#EAB308', // Yellow
    '#22C55E', // Green
    '#06B6D4', // Cyan
    '#3B82F6', // Blue
  ];

  constructor() {
    this.passkeySupported.set(this.webAuthnService.isSupported());
  }

  ngOnInit(): void {
    this.code = this.route.snapshot.paramMap.get('code') || '';
    if (this.code) {
      this.loadInvitation();
    } else {
      this.step.set('invalid');
      this.error.set('No invitation code provided');
    }
  }

  /**
   * Load invitation details from the API
   */
  private loadInvitation(): void {
    this.isLoading.set(true);
    this.error.set(null);

    this.invitationService.getInvitationByCode(this.code).subscribe({
      next: (invitation) => {
        this.isLoading.set(false);
        this.invitation.set(invitation);

        if (!invitation.isValid) {
          this.step.set('invalid');
          this.error.set(this.getInvalidReason(invitation));
        } else {
          this.step.set('details');
        }
      },
      error: (err) => {
        this.isLoading.set(false);
        this.step.set('invalid');
        this.error.set(err.error?.message || 'Invalid or expired invitation code');
      },
    });
  }

  /**
   * Get the reason why an invitation is invalid
   */
  private getInvalidReason(invitation: Invitation): string {
    switch (invitation.status) {
      case InvitationStatus.Accepted:
        return 'This invitation has already been used';
      case InvitationStatus.Declined:
        return 'This invitation was declined';
      case InvitationStatus.Expired:
        return 'This invitation has expired';
      case InvitationStatus.Revoked:
        return 'This invitation was revoked';
      default:
        return 'This invitation is no longer valid';
    }
  }

  /**
   * Accept the invitation and create the account
   */
  acceptInvitation(): void {
    if (!this.displayName.trim()) {
      this.error.set('Please enter your name');
      return;
    }

    this.isLoading.set(true);
    this.error.set(null);

    const request: AcceptInvitationRequest = {
      displayName: this.displayName.trim(),
      nickname: this.nickname.trim() || undefined,
      color: this.color,
    };

    this.invitationService.acceptInvitation(this.code, request).subscribe({
      next: (result) => {
        this.isLoading.set(false);
        this.authResult = result;

        // Store tokens
        this.storageService.set(environment.auth.tokenStorageKey, {
          accessToken: result.accessToken,
          refreshToken: result.refreshToken,
          expiresIn: result.expiresIn,
        });

        // Proceed to passkey setup if supported
        if (this.passkeySupported()) {
          this.step.set('passkey');
        } else {
          this.step.set('complete');
        }
      },
      error: (err) => {
        this.isLoading.set(false);
        this.error.set(err.error?.message || 'Failed to accept invitation');
      },
    });
  }

  /**
   * Set up a passkey for the new account
   */
  async setupPasskey(): Promise<void> {
    if (!this.passkeySupported() || !this.authResult) {
      this.skipPasskey();
      return;
    }

    this.isLoading.set(true);
    this.error.set(null);

    try {
      // Use the invitation email for passkey registration
      const email = this.invitation()?.email;
      if (!email) {
        this.skipPasskey();
        return;
      }

      // Start passkey registration
      const response = await fetch(`${environment.apiUrl}/auth/passkey/register/start`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${this.authResult.accessToken}`,
        },
        body: JSON.stringify({ email }),
      });

      if (!response.ok) {
        throw new Error('Failed to start passkey registration');
      }

      const startResult = await response.json();
      const options = JSON.parse(startResult.options);

      // Create passkey
      const credential = await this.webAuthnService.createPasskey(options);

      // Complete registration
      const completeResponse = await fetch(`${environment.apiUrl}/auth/passkey/register/complete`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${this.authResult.accessToken}`,
        },
        body: JSON.stringify({
          sessionId: startResult.sessionId,
          attestationResponse: credential,
          displayName: 'Primary Passkey',
        }),
      });

      if (completeResponse.ok) {
        this.step.set('complete');
      } else {
        // Allow skip on failure
        console.error('Passkey registration failed');
      }
    } catch (err) {
      console.error('Passkey setup failed:', err);
      // Allow user to skip if passkey fails
    } finally {
      this.isLoading.set(false);
    }
  }

  /**
   * Skip passkey setup
   */
  skipPasskey(): void {
    this.step.set('complete');
  }

  /**
   * Continue to dashboard after completion
   */
  continueToDashboard(): void {
    this.router.navigate(['/dashboard']);
  }

  /**
   * Select a color for the user's avatar
   */
  selectColor(newColor: string): void {
    this.color = newColor;
  }

  /**
   * Get the role display name
   */
  getRoleDisplay(role: string): string {
    const roleDisplays: Record<string, string> = {
      Owner: 'Family Owner',
      Admin: 'Administrator',
      Adult: 'Adult Member',
      Teen: 'Teen Member',
      Child: 'Child Member',
      Caregiver: 'Caregiver',
    };
    return roleDisplays[role] || role;
  }
}
