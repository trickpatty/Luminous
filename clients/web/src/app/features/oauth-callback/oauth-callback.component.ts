import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { CalendarConnectionService, ToastService } from '../../core';

/**
 * OAuth callback component that receives the authorization code
 * from the OAuth provider and either:
 * 1. Sends it back to the parent window (popup flow)
 * 2. Completes the OAuth flow directly (redirect flow - for PWA/popup-blocked scenarios)
 */
@Component({
  selector: 'app-oauth-callback',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="min-h-screen flex items-center justify-center bg-gray-50">
      <div class="text-center max-w-md px-4">
        @if (processing) {
          <div class="animate-spin w-8 h-8 border-4 border-primary-600 border-t-transparent rounded-full mx-auto mb-4"></div>
          <p class="text-gray-600">{{ processingMessage }}</p>
          <p class="text-sm text-gray-400 mt-2">{{ processingSubtext }}</p>
        } @else if (!error) {
          <div class="w-12 h-12 bg-green-100 rounded-full flex items-center justify-center mx-auto mb-4">
            <svg class="w-6 h-6 text-green-600" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 13l4 4L19 7" />
            </svg>
          </div>
          <p class="text-gray-900 font-medium">Authorization Successful</p>
          <p class="text-sm text-gray-500 mt-2">Redirecting you back to the app...</p>
        } @else {
          <div class="w-12 h-12 bg-red-100 rounded-full flex items-center justify-center mx-auto mb-4">
            <svg class="w-6 h-6 text-red-600" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
            </svg>
          </div>
          <p class="text-gray-900 font-medium">Authorization Failed</p>
          <p class="text-sm text-gray-500 mt-2">{{ errorDescription || 'An error occurred during authorization.' }}</p>

          @if (isRedirectFlow) {
            <button
              type="button"
              class="mt-4 px-4 py-2 bg-primary-600 hover:bg-primary-700 rounded-lg text-sm font-medium text-white transition-colors"
              (click)="goToCalendars()"
            >
              Back to Calendars
            </button>
          } @else {
            <button
              type="button"
              class="mt-4 px-4 py-2 bg-gray-200 hover:bg-gray-300 rounded-lg text-sm font-medium text-gray-700 transition-colors"
              (click)="closeWindow()"
            >
              Close Window
            </button>
          }
        }
      </div>
    </div>
  `,
})
export class OAuthCallbackComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly calendarService = inject(CalendarConnectionService);
  private readonly toastService = inject(ToastService);

  error: string | null = null;
  errorDescription: string | null = null;
  processing = true;
  processingMessage = 'Completing authorization...';
  processingSubtext = 'This window will close automatically.';
  isRedirectFlow = false;

  ngOnInit(): void {
    // Check if this is a redirect flow (PWA or popup-blocked scenario)
    this.isRedirectFlow = sessionStorage.getItem('luminous_oauth_redirect') === 'true';

    // Get query parameters
    const params = this.route.snapshot.queryParams;

    const code = params['code'];
    const state = params['state'];
    const error = params['error'];
    const errorDescription = params['error_description'];

    if (error) {
      this.handleError(error, errorDescription);
      return;
    }

    if (code && state) {
      this.handleSuccess(code, state);
    } else {
      this.handleError('missing_params', 'Missing authorization parameters.');
    }
  }

  private handleError(error: string, errorDescription?: string): void {
    this.processing = false;
    this.error = error;
    this.errorDescription = errorDescription ?? null;

    // If in popup flow, also notify parent
    if (window.opener && !this.isRedirectFlow) {
      window.opener.postMessage(
        {
          type: 'oauth_callback',
          error,
          errorDescription,
        },
        window.location.origin
      );
    }

    // Clean up redirect flow markers
    sessionStorage.removeItem('luminous_oauth_redirect');
    sessionStorage.removeItem('luminous_oauth_family_id');
  }

  private handleSuccess(code: string, state: string): void {
    // Check if we have a parent window (popup flow)
    if (window.opener && !this.isRedirectFlow) {
      // Popup flow - send message to parent and close
      window.opener.postMessage(
        {
          type: 'oauth_callback',
          code,
          state,
        },
        window.location.origin
      );

      // Close this window after a brief delay
      setTimeout(() => {
        window.close();
      }, 500);
    } else {
      // Redirect flow - complete OAuth directly and redirect to calendars page
      this.processingMessage = 'Connecting your calendar...';
      this.processingSubtext = 'Please wait while we complete the setup.';

      const redirectUri = `${window.location.origin}/oauth/callback`;
      const familyId = sessionStorage.getItem('luminous_oauth_family_id');

      if (!familyId) {
        this.handleError('missing_family_id', 'Session expired. Please try connecting your calendar again.');
        return;
      }

      this.calendarService.completeOAuth(familyId, { code, state, redirectUri }).subscribe({
        next: () => {
          this.processing = false;
          this.toastService.success('Calendar connected successfully!');

          // Clean up redirect flow markers
          sessionStorage.removeItem('luminous_oauth_redirect');
          sessionStorage.removeItem('luminous_oauth_family_id');

          // Redirect to calendars page
          setTimeout(() => {
            this.router.navigate(['/dashboard'], {
              queryParams: { tab: 'calendars', oauthComplete: 'true' }
            });
          }, 1500);
        },
        error: (err) => {
          this.handleError('oauth_complete_failed', err.message || 'Failed to complete calendar connection.');
        }
      });
    }
  }

  goToCalendars(): void {
    sessionStorage.removeItem('luminous_oauth_redirect');
    sessionStorage.removeItem('luminous_oauth_family_id');
    this.router.navigate(['/dashboard'], { queryParams: { tab: 'calendars' } });
  }

  closeWindow(): void {
    window.close();
  }
}
