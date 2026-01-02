import { Component, EventEmitter, Input, Output, signal } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-exit-dialog',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="exit-dialog-overlay" (click)="close.emit()">
      <div class="exit-dialog" (click)="$event.stopPropagation()">
        <h2 class="text-display-sm">Admin Exit</h2>
        <p class="text-body-lg text-secondary">Enter PIN to exit kiosk mode</p>

        <!-- PIN display -->
        <div class="pin-display">
          @for (digit of [0, 1, 2, 3]; track digit) {
            <div class="pin-dot" [class.filled]="pin().length > digit"></div>
          }
        </div>

        @if (displayError) {
          <div class="pin-error">
            {{ displayError }}
          </div>
        }

        <!-- Numpad -->
        <div class="display-numpad">
          @for (key of numpadKeys; track key) {
            <button
              class="display-numpad-key"
              [class.display-numpad-key-action]="key === 'OK'"
              (click)="handleKey(key)"
            >
              {{ key }}
            </button>
          }
        </div>

        <button class="cancel-btn" (click)="close.emit()">
          Cancel
        </button>
      </div>
    </div>
  `,
  styles: [`
    .exit-dialog-overlay {
      position: fixed;
      inset: 0;
      background: rgba(0, 0, 0, 0.6);
      backdrop-filter: blur(8px);
      display: flex;
      align-items: center;
      justify-content: center;
      z-index: var(--z-modal);
    }

    .exit-dialog {
      background: var(--surface-primary);
      border-radius: var(--radius-2xl);
      padding: var(--space-10);
      text-align: center;
      max-width: 480px;
      width: calc(100% - var(--space-16));
    }

    .exit-dialog h2 {
      margin-bottom: var(--space-2);
    }

    .text-secondary {
      color: var(--text-secondary);
    }

    .pin-display {
      display: flex;
      justify-content: center;
      gap: var(--space-4);
      margin: var(--space-8) 0;
    }

    .pin-dot {
      width: 20px;
      height: 20px;
      border: 3px solid var(--border-color-strong);
      border-radius: var(--radius-full);
      transition: all var(--duration-quick) var(--ease-out);
    }

    .pin-dot.filled {
      background: var(--accent-600);
      border-color: var(--accent-600);
    }

    .pin-error {
      color: var(--danger);
      font-weight: 500;
      margin-bottom: var(--space-4);
    }

    .display-numpad {
      margin-bottom: var(--space-6);
    }

    .cancel-btn {
      background: none;
      border: none;
      color: var(--text-secondary);
      font-size: 1rem;
      cursor: pointer;
      padding: var(--space-3) var(--space-6);
    }

    .cancel-btn:hover {
      color: var(--text-primary);
    }
  `],
})
export class ExitDialogComponent {
  @Input() externalError: string | null = null;
  @Output() close = new EventEmitter<void>();
  @Output() exit = new EventEmitter<string>();

  protected readonly pin = signal('');
  protected readonly internalError = signal<string | null>(null);

  // Combine internal and external errors
  protected get displayError(): string | null {
    return this.externalError || this.internalError();
  }

  readonly numpadKeys = ['1', '2', '3', '4', '5', '6', '7', '8', '9', 'C', '0', 'OK'];

  handleKey(key: string): void {
    if (key === 'C') {
      this.pin.set('');
      this.internalError.set(null);
    } else if (key === 'OK') {
      if (this.pin().length === 4) {
        this.exit.emit(this.pin());
      } else {
        this.internalError.set('Enter 4 digits');
      }
    } else {
      if (this.pin().length < 4) {
        this.pin.update((p) => p + key);
        this.internalError.set(null);
      }
    }
  }
}
