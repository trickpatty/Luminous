import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService, WebAuthnService } from '../../../../core';
import { RegisterStartRequest } from '../../../../models';

type RegisterStep = 'details' | 'verify' | 'passkey' | 'complete';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './register.component.html',
  styleUrl: './register.component.css',
})
export class RegisterComponent {
  private readonly authService = inject(AuthService);
  private readonly webAuthnService = inject(WebAuthnService);
  private readonly router = inject(Router);

  // Form state
  email = '';
  displayName = '';
  familyName = '';

  // Verification state
  sessionId = '';
  maskedEmail = '';
  verificationCode = '';
  remainingAttempts = signal<number | null>(null);
  codeExpiresAt = signal<Date | null>(null);
  canResend = signal(false);
  resendCountdown = signal(0);

  // Passkey registration state
  passkeySessionId = '';

  // UI state
  step = signal<RegisterStep>('details');
  isLoading = this.authService.isLoading;
  error = this.authService.error;
  passkeySupported = signal(false);

  private resendTimer?: ReturnType<typeof setInterval>;

  constructor() {
    this.passkeySupported.set(this.webAuthnService.isSupported());
  }

  /**
   * Step 1: Submit registration details and request email verification
   */
  async startRegistration(): Promise<void> {
    const request: RegisterStartRequest = {
      email: this.email,
      displayName: this.displayName,
      familyName: this.familyName,
      timezone: Intl.DateTimeFormat().resolvedOptions().timeZone,
    };

    this.authService.startRegistration(request).subscribe({
      next: (result) => {
        if (result.success && result.sessionId) {
          this.sessionId = result.sessionId;
          this.maskedEmail = result.maskedEmail;

          if (result.expiresAt) {
            this.codeExpiresAt.set(new Date(result.expiresAt));
          }

          // Start resend countdown
          if (result.retryAfterSeconds) {
            this.startResendCountdown(result.retryAfterSeconds);
          }

          this.step.set('verify');
        }
      },
    });
  }

  /**
   * Step 2: Verify the email with OTP code
   */
  async verifyEmail(): Promise<void> {
    this.authService
      .completeRegistration({
        sessionId: this.sessionId,
        code: this.verificationCode,
      })
      .subscribe({
        next: (result) => {
          if (result.success) {
            // Registration complete - proceed to passkey setup
            if (this.passkeySupported()) {
              this.step.set('passkey');
            } else {
              this.step.set('complete');
            }
          } else {
            // Show remaining attempts if verification failed
            this.remainingAttempts.set(result.remainingAttempts);
          }
        },
      });
  }

  /**
   * Resend verification code
   */
  resendCode(): void {
    if (!this.canResend()) return;

    const request: RegisterStartRequest = {
      email: this.email,
      displayName: this.displayName,
      familyName: this.familyName,
      timezone: Intl.DateTimeFormat().resolvedOptions().timeZone,
    };

    this.authService.startRegistration(request).subscribe({
      next: (result) => {
        if (result.success && result.sessionId) {
          this.sessionId = result.sessionId;
          this.verificationCode = '';
          this.remainingAttempts.set(null);

          if (result.expiresAt) {
            this.codeExpiresAt.set(new Date(result.expiresAt));
          }

          if (result.retryAfterSeconds) {
            this.startResendCountdown(result.retryAfterSeconds);
          }
        }
      },
    });
  }

  /**
   * Go back to details step
   */
  backToDetails(): void {
    this.clearResendTimer();
    this.sessionId = '';
    this.verificationCode = '';
    this.remainingAttempts.set(null);
    this.step.set('details');
  }

  /**
   * Step 3: Set up a passkey for the new account
   */
  async setupPasskey(): Promise<void> {
    if (!this.passkeySupported()) {
      this.skipPasskey();
      return;
    }

    try {
      this.authService.startPasskeyRegistration(this.email).subscribe({
        next: async (data) => {
          try {
            // Store the passkey session ID for completing registration
            this.passkeySessionId = data.sessionId;

            // Create the passkey with the parsed options
            const credential = await this.webAuthnService.createPasskey(data.options);

            // Complete registration with sessionId
            this.authService.completePasskeyRegistration(
              this.passkeySessionId,
              credential,
              'Primary Passkey'
            ).subscribe({
              next: (result) => {
                if (result.success) {
                  this.step.set('complete');
                } else {
                  console.error('Passkey registration failed:', result.error);
                  // Allow user to skip if passkey fails
                }
              },
              error: (err) => {
                console.error('Passkey registration error:', err);
                // Allow user to skip if passkey fails
              }
            });
          } catch (err) {
            console.error('Passkey setup failed:', err);
            // Allow user to skip if passkey fails
          }
        },
        error: (err) => {
          console.error('Failed to start passkey registration:', err);
        }
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

  /**
   * Start countdown timer for resend button
   */
  private startResendCountdown(seconds: number): void {
    this.clearResendTimer();
    this.canResend.set(false);
    this.resendCountdown.set(seconds);

    this.resendTimer = setInterval(() => {
      const current = this.resendCountdown();
      if (current <= 1) {
        this.clearResendTimer();
        this.canResend.set(true);
        this.resendCountdown.set(0);
      } else {
        this.resendCountdown.set(current - 1);
      }
    }, 1000);
  }

  /**
   * Clear the resend countdown timer
   */
  private clearResendTimer(): void {
    if (this.resendTimer) {
      clearInterval(this.resendTimer);
      this.resendTimer = undefined;
    }
  }
}
