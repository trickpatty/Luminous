import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService, WebAuthnService } from '../../../../core';
import { RegisterRequest } from '../../../../models';

type RegisterStep = 'details' | 'passkey' | 'complete';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './register.component.html',
})
export class RegisterComponent {
  private readonly authService = inject(AuthService);
  private readonly webAuthnService = inject(WebAuthnService);
  private readonly router = inject(Router);

  // Form state
  email = '';
  displayName = '';
  familyName = '';
  inviteCode = '';
  isJoiningFamily = false;

  // UI state
  step = signal<RegisterStep>('details');
  isLoading = this.authService.isLoading;
  error = this.authService.error;
  passkeySupported = signal(false);

  constructor() {
    this.passkeySupported.set(this.webAuthnService.isSupported());
  }

  /**
   * Toggle between creating new family and joining existing
   */
  toggleJoinFamily(): void {
    this.isJoiningFamily = !this.isJoiningFamily;
    if (!this.isJoiningFamily) {
      this.inviteCode = '';
    } else {
      this.familyName = '';
    }
  }

  /**
   * Register new user
   */
  async register(): Promise<void> {
    const request: RegisterRequest = {
      email: this.email,
      displayName: this.displayName,
    };

    if (this.isJoiningFamily) {
      request.inviteCode = this.inviteCode;
    } else {
      request.familyName = this.familyName;
    }

    this.authService.register(request).subscribe({
      next: () => {
        // If passkey is supported, offer to set one up
        if (this.passkeySupported()) {
          this.step.set('passkey');
        } else {
          this.step.set('complete');
        }
      },
    });
  }

  /**
   * Set up a passkey for the new account
   */
  async setupPasskey(): Promise<void> {
    if (!this.passkeySupported()) {
      this.skipPasskey();
      return;
    }

    try {
      this.authService.startPasskeyRegistration(this.email).subscribe({
        next: async (options) => {
          try {
            const credential = await this.webAuthnService.createPasskey(options);
            this.authService.completePasskeyRegistration(credential).subscribe({
              next: () => {
                this.step.set('complete');
              },
            });
          } catch (err) {
            console.error('Passkey setup failed:', err);
            // Allow user to skip if passkey fails
          }
        },
      });
    } catch (err) {
      console.error('Failed to start passkey registration:', err);
    }
  }

  /**
   * Skip passkey setup
   */
  skipPasskey(): void {
    this.step.set('complete');
  }

  /**
   * Continue to dashboard
   */
  continueToDashboard(): void {
    this.router.navigate(['/dashboard']);
  }
}
