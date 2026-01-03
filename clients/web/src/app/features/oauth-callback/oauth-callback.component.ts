import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute } from '@angular/router';

/**
 * OAuth callback component that receives the authorization code
 * from the OAuth provider and sends it back to the parent window.
 */
@Component({
  selector: 'app-oauth-callback',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="min-h-screen flex items-center justify-center bg-gray-50">
      <div class="text-center">
        @if (!error) {
          <div class="animate-spin w-8 h-8 border-4 border-primary-600 border-t-transparent rounded-full mx-auto mb-4"></div>
          <p class="text-gray-600">Completing authorization...</p>
          <p class="text-sm text-gray-400 mt-2">This window will close automatically.</p>
        } @else {
          <div class="w-12 h-12 bg-red-100 rounded-full flex items-center justify-center mx-auto mb-4">
            <svg class="w-6 h-6 text-red-600" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
            </svg>
          </div>
          <p class="text-gray-900 font-medium">Authorization Failed</p>
          <p class="text-sm text-gray-500 mt-2">{{ errorDescription || 'An error occurred during authorization.' }}</p>
          <button
            type="button"
            class="mt-4 px-4 py-2 bg-gray-200 hover:bg-gray-300 rounded-lg text-sm font-medium text-gray-700 transition-colors"
            (click)="closeWindow()"
          >
            Close Window
          </button>
        }
      </div>
    </div>
  `,
})
export class OAuthCallbackComponent implements OnInit {
  error: string | null = null;
  errorDescription: string | null = null;

  constructor(private readonly route: ActivatedRoute) {}

  ngOnInit(): void {
    // Get query parameters
    const params = this.route.snapshot.queryParams;

    const code = params['code'];
    const state = params['state'];
    const error = params['error'];
    const errorDescription = params['error_description'];

    if (error) {
      this.error = error;
      this.errorDescription = errorDescription;

      // Send error to parent
      if (window.opener) {
        window.opener.postMessage(
          {
            type: 'oauth_callback',
            error,
            errorDescription,
          },
          window.location.origin
        );
      }
      return;
    }

    if (code && state) {
      // Send success to parent
      if (window.opener) {
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
        // No opener - user navigated directly
        this.error = 'no_opener';
        this.errorDescription = 'Please use the Connect Calendar button in the app.';
      }
    } else {
      this.error = 'missing_params';
      this.errorDescription = 'Missing authorization parameters.';
    }
  }

  closeWindow(): void {
    window.close();
  }
}
