import { Component, OnInit, OnDestroy, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService, WebAuthnService } from '../../../../core';

type AuthStep = 'email' | 'otp' | 'passkey';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss',
})
export class LoginComponent implements OnInit, OnDestroy {
  private readonly authService = inject(AuthService);
  private readonly webAuthnService = inject(WebAuthnService);
  private readonly router = inject(Router);

  // Form state
  email = '';
  otpCode = '';

  // UI state
  step = signal<AuthStep>('email');
  isLoading = this.authService.isLoading;
  error = this.authService.error;
  passkeySupported = signal(false);
  conditionalMediationAvailable = signal(false);
  otpSent = signal(false);

  async ngOnInit(): Promise<void> {
    // Check WebAuthn support
    this.passkeySupported.set(this.webAuthnService.isSupported());

    if (this.passkeySupported()) {
      const conditionalAvailable = await this.webAuthnService.isConditionalMediationAvailable();
      this.conditionalMediationAvailable.set(conditionalAvailable);

      // If conditional mediation is available, start listening for passkey autofill
      if (conditionalAvailable) {
        this.startConditionalPasskeyAuth();
      }
    }
  }

  ngOnDestroy(): void {
    // Abort any pending conditional WebAuthn operation to prevent
    // "a pending request already in progress" errors on other pages
    this.webAuthnService.abortConditionalMediation();
  }

  /**
   * Request OTP for the entered email
   */
  async requestOtp(): Promise<void> {
    if (!this.email) return;

    this.authService.requestOtp(this.email).subscribe({
      next: () => {
        this.otpSent.set(true);
        this.step.set('otp');
      },
    });
  }

  /**
   * Verify the OTP code
   */
  async verifyOtp(): Promise<void> {
    if (!this.email || !this.otpCode) return;

    this.authService.verifyOtp(this.email, this.otpCode).subscribe({
      next: (result) => {
        if (result.success && result.auth) {
          this.navigateToDashboard();
        }
        // Error handling is done in the auth service
      },
    });
  }

  /**
   * Start passkey authentication flow
   */
  async authenticateWithPasskey(): Promise<void> {
    if (!this.passkeySupported()) return;

    try {
      // Get authentication options from server
      this.authService.startPasskeyAuthentication(this.email || undefined).subscribe({
        next: async (data) => {
          try {
            // Create credential with browser using parsed options
            const credential = await this.webAuthnService.authenticateWithPasskey(data.options);

            // Complete authentication with server, including sessionId
            this.authService.completePasskeyAuthentication(data.sessionId, credential).subscribe({
              next: (result) => {
                if (result.success) {
                  this.navigateToDashboard();
                }
              },
            });
          } catch (err) {
            console.error('Passkey authentication failed:', err);
          }
        },
      });
    } catch (err) {
      console.error('Failed to start passkey auth:', err);
    }
  }

  /**
   * Start conditional passkey authentication (autofill)
   */
  private async startConditionalPasskeyAuth(): Promise<void> {
    try {
      this.authService.startPasskeyAuthentication().subscribe({
        next: async (data) => {
          try {
            const credential = await this.webAuthnService.authenticateWithPasskey(data.options, true);
            this.authService.completePasskeyAuthentication(data.sessionId, credential).subscribe({
              next: (result) => {
                if (result.success) {
                  this.navigateToDashboard();
                }
              },
            });
          } catch {
            // Conditional UI was dismissed, not an error
          }
        },
      });
    } catch {
      // Failed to start conditional auth, not critical
    }
  }

  /**
   * Go back to email step
   */
  backToEmail(): void {
    this.step.set('email');
    this.otpCode = '';
    this.otpSent.set(false);
  }

  /**
   * Resend OTP code
   */
  resendOtp(): void {
    this.requestOtp();
  }

  /**
   * Navigate to dashboard after successful auth
   */
  private navigateToDashboard(): void {
    const redirectUrl = sessionStorage.getItem('redirectUrl') || '/dashboard';
    sessionStorage.removeItem('redirectUrl');
    this.router.navigate([redirectUrl]);
  }
}
