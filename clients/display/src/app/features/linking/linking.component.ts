import { Component, inject, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DeviceAuthService } from '../../core/services/device-auth.service';
import { ElectronService } from '../../core/services/electron.service';

@Component({
  selector: 'app-linking',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="display-container linking-page">
      <!-- Header -->
      <header class="linking-header">
        <h1 class="text-display-lg">Luminous</h1>
        <p class="text-glanceable text-secondary">Family Command Center</p>
      </header>

      <!-- Main content -->
      <main class="linking-content">
        <div class="display-card-lg linking-card">
          @if (!linkCode()) {
            <!-- Initial state - Request code -->
            <div class="linking-intro">
              <div class="linking-icon">
                <svg xmlns="http://www.w3.org/2000/svg" width="80" height="80" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round">
                  <path d="M10 13a5 5 0 0 0 7.54.54l3-3a5 5 0 0 0-7.07-7.07l-1.72 1.71"/>
                  <path d="M14 11a5 5 0 0 0-7.54-.54l-3 3a5 5 0 0 0 7.07 7.07l1.71-1.71"/>
                </svg>
              </div>
              <h2 class="text-display-sm">Link This Display</h2>
              <p class="text-glanceable text-secondary">
                Connect this display to your Luminous family account
              </p>
              <button
                class="display-btn display-btn-primary"
                (click)="requestCode()"
                [disabled]="isLinking()"
              >
                @if (isLinking()) {
                  <span class="display-spinner display-spinner-md"></span>
                  <span>Generating...</span>
                } @else {
                  <span>Get Link Code</span>
                }
              </button>
            </div>
          } @else {
            <!-- Link code display -->
            <div class="linking-code-display">
              <p class="text-title-lg text-secondary">Enter this code in the Luminous app:</p>
              <div class="display-link-code">
                {{ linkCode() }}
              </div>
              <p class="text-body-lg text-secondary">
                Waiting for confirmation...
              </p>
              <div class="linking-waiting">
                <span class="display-spinner display-spinner-md"></span>
              </div>
              <button
                class="display-btn display-btn-secondary"
                (click)="cancelLinking()"
              >
                Cancel
              </button>
            </div>
          }

          @if (linkError()) {
            <div class="linking-error">
              <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                <circle cx="12" cy="12" r="10"/>
                <line x1="12" x2="12" y1="8" y2="12"/>
                <line x1="12" x2="12.01" y1="16" y2="16"/>
              </svg>
              <span>{{ linkError() }}</span>
            </div>
          }
        </div>
      </main>

      <!-- Footer -->
      <footer class="linking-footer">
        <p class="text-body-md text-tertiary">
          @if (appInfo()) {
            Luminous Display v{{ appInfo()!.version }}
          }
        </p>
      </footer>
    </div>
  `,
  styles: [`
    .linking-page {
      display: flex;
      flex-direction: column;
      min-height: 100vh;
      padding: var(--display-safe-area);
    }

    .linking-header {
      text-align: center;
      padding: var(--space-12) 0;
    }

    .linking-header h1 {
      color: var(--accent-600);
      margin-bottom: var(--space-2);
    }

    .linking-content {
      flex: 1;
      display: flex;
      align-items: center;
      justify-content: center;
    }

    .linking-card {
      max-width: 600px;
      width: 100%;
      text-align: center;
    }

    .linking-intro {
      display: flex;
      flex-direction: column;
      align-items: center;
      gap: var(--space-6);
    }

    .linking-icon {
      color: var(--accent-500);
    }

    .linking-code-display {
      display: flex;
      flex-direction: column;
      align-items: center;
      gap: var(--space-6);
    }

    .linking-waiting {
      display: flex;
      align-items: center;
      gap: var(--space-3);
      color: var(--text-secondary);
    }

    .linking-error {
      display: flex;
      align-items: center;
      justify-content: center;
      gap: var(--space-3);
      margin-top: var(--space-6);
      padding: var(--space-4);
      background: var(--danger-light);
      color: var(--danger-dark);
      border-radius: var(--radius-lg);
    }

    .linking-footer {
      text-align: center;
      padding: var(--space-6) 0;
    }

    .text-secondary {
      color: var(--text-secondary);
    }

    .text-tertiary {
      color: var(--text-tertiary);
    }
  `],
})
export class LinkingComponent implements OnInit, OnDestroy {
  private readonly authService = inject(DeviceAuthService);
  private readonly electronService = inject(ElectronService);

  protected readonly linkCode = this.authService.linkCode;
  protected readonly isLinking = this.authService.isLinking;
  protected readonly linkError = this.authService.linkError;
  protected readonly appInfo = this.electronService.appInfo;

  ngOnInit(): void {
    // Could auto-request code on load if desired
  }

  ngOnDestroy(): void {
    // Cleanup if needed
  }

  async requestCode(): Promise<void> {
    try {
      await this.authService.requestLinkCode();
    } catch (error) {
      console.error('Failed to request link code:', error);
    }
  }

  cancelLinking(): void {
    this.authService.cancelLinking();
  }
}
