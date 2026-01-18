import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';

export type ToastVariant = 'default' | 'success' | 'error' | 'warning';

@Component({
  selector: 'lum-toast',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div
      class="toast animate-toast-enter"
      [class.animate-toast-exit]="exiting"
      role="alert"
      aria-live="polite"
    >
      @if (showIcon) {
        <div class="flex-shrink-0" aria-hidden="true">
          @switch (variant) {
            @case ('success') {
              <svg class="h-5 w-5 text-success" viewBox="0 0 20 20" fill="currentColor">
                <path fill-rule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zm3.707-9.293a1 1 0 00-1.414-1.414L9 10.586 7.707 9.293a1 1 0 00-1.414 1.414l2 2a1 1 0 001.414 0l4-4z" clip-rule="evenodd" />
              </svg>
            }
            @case ('error') {
              <svg class="h-5 w-5 text-danger" viewBox="0 0 20 20" fill="currentColor">
                <path fill-rule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zM8.707 7.293a1 1 0 00-1.414 1.414L8.586 10l-1.293 1.293a1 1 0 101.414 1.414L10 11.414l1.293 1.293a1 1 0 001.414-1.414L11.414 10l1.293-1.293a1 1 0 00-1.414-1.414L10 8.586 8.707 7.293z" clip-rule="evenodd" />
              </svg>
            }
            @case ('warning') {
              <svg class="h-5 w-5 text-warning" viewBox="0 0 20 20" fill="currentColor">
                <path fill-rule="evenodd" d="M8.257 3.099c.765-1.36 2.722-1.36 3.486 0l5.58 9.92c.75 1.334-.213 2.98-1.742 2.98H4.42c-1.53 0-2.493-1.646-1.743-2.98l5.58-9.92zM11 13a1 1 0 11-2 0 1 1 0 012 0zm-1-8a1 1 0 00-1 1v3a1 1 0 002 0V6a1 1 0 00-1-1z" clip-rule="evenodd" />
              </svg>
            }
            @default {
              <svg class="h-5 w-5 text-info" viewBox="0 0 20 20" fill="currentColor">
                <path fill-rule="evenodd" d="M18 10a8 8 0 11-16 0 8 8 0 0116 0zm-7-4a1 1 0 11-2 0 1 1 0 012 0zM9 9a1 1 0 000 2v3a1 1 0 001 1h1a1 1 0 100-2v-3a1 1 0 00-1-1H9z" clip-rule="evenodd" />
              </svg>
            }
          }
        </div>
      }

      <div class="flex-1">
        <p class="text-body-sm">{{ message }}</p>
      </div>

      @if (actionLabel) {
        <button
          type="button"
          class="flex-shrink-0 text-body-sm font-medium text-accent-400 hover:text-accent-300 transition-colors"
          (click)="action.emit()"
        >
          {{ actionLabel }}
        </button>
      }

      @if (dismissible) {
        <button
          type="button"
          class="flex-shrink-0 text-gray-400 hover:text-white transition-colors ml-2"
          (click)="dismiss.emit()"
          aria-label="Dismiss"
        >
          <svg class="h-5 w-5" viewBox="0 0 20 20" fill="currentColor">
            <path fill-rule="evenodd" d="M4.293 4.293a1 1 0 011.414 0L10 8.586l4.293-4.293a1 1 0 111.414 1.414L11.414 10l4.293 4.293a1 1 0 01-1.414 1.414L10 11.414l-4.293 4.293a1 1 0 01-1.414-1.414L8.586 10 4.293 5.707a1 1 0 010-1.414z" clip-rule="evenodd" />
          </svg>
        </button>
      }
    </div>
  `,
})
export class ToastComponent {
  @Input() message = '';
  @Input() variant: ToastVariant = 'default';
  @Input() actionLabel?: string;
  @Input() dismissible = true;
  @Input() showIcon = true;
  @Input() exiting = false;

  @Output() action = new EventEmitter<void>();
  @Output() dismiss = new EventEmitter<void>();
}
