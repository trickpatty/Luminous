import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';

export type AlertVariant = 'info' | 'success' | 'warning' | 'error';

@Component({
  selector: 'lum-alert',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div [class]="alertClasses" role="alert">
      <div class="flex-shrink-0" aria-hidden="true">
        @switch (variant) {
          @case ('info') {
            <svg class="h-5 w-5" viewBox="0 0 20 20" fill="currentColor">
              <path fill-rule="evenodd" d="M18 10a8 8 0 11-16 0 8 8 0 0116 0zm-7-4a1 1 0 11-2 0 1 1 0 012 0zM9 9a1 1 0 000 2v3a1 1 0 001 1h1a1 1 0 100-2v-3a1 1 0 00-1-1H9z" clip-rule="evenodd" />
            </svg>
          }
          @case ('success') {
            <svg class="h-5 w-5" viewBox="0 0 20 20" fill="currentColor">
              <path fill-rule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zm3.707-9.293a1 1 0 00-1.414-1.414L9 10.586 7.707 9.293a1 1 0 00-1.414 1.414l2 2a1 1 0 001.414 0l4-4z" clip-rule="evenodd" />
            </svg>
          }
          @case ('warning') {
            <svg class="h-5 w-5" viewBox="0 0 20 20" fill="currentColor">
              <path fill-rule="evenodd" d="M8.257 3.099c.765-1.36 2.722-1.36 3.486 0l5.58 9.92c.75 1.334-.213 2.98-1.742 2.98H4.42c-1.53 0-2.493-1.646-1.743-2.98l5.58-9.92zM11 13a1 1 0 11-2 0 1 1 0 012 0zm-1-8a1 1 0 00-1 1v3a1 1 0 002 0V6a1 1 0 00-1-1z" clip-rule="evenodd" />
            </svg>
          }
          @case ('error') {
            <svg class="h-5 w-5" viewBox="0 0 20 20" fill="currentColor">
              <path fill-rule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zM8.707 7.293a1 1 0 00-1.414 1.414L8.586 10l-1.293 1.293a1 1 0 101.414 1.414L10 11.414l1.293 1.293a1 1 0 001.414-1.414L11.414 10l1.293-1.293a1 1 0 00-1.414-1.414L10 8.586 8.707 7.293z" clip-rule="evenodd" />
            </svg>
          }
        }
      </div>
      <div class="flex-1">
        @if (title) {
          <h3 class="text-body-sm font-medium">{{ title }}</h3>
        }
        <div [class]="title ? 'mt-1 text-body-sm opacity-90' : 'text-body-sm'">
          <ng-content></ng-content>
        </div>
      </div>
      @if (dismissible) {
        <div class="flex-shrink-0">
          <button
            type="button"
            (click)="dismiss.emit()"
            class="inline-flex rounded-md p-1.5 transition-colors focus:outline-none focus-visible:ring-2 focus-visible:ring-offset-2"
            [class]="dismissButtonClasses"
            aria-label="Dismiss"
          >
            <svg class="h-5 w-5" viewBox="0 0 20 20" fill="currentColor" aria-hidden="true">
              <path fill-rule="evenodd" d="M4.293 4.293a1 1 0 011.414 0L10 8.586l4.293-4.293a1 1 0 111.414 1.414L11.414 10l4.293 4.293a1 1 0 01-1.414 1.414L10 11.414l-4.293 4.293a1 1 0 01-1.414-1.414L8.586 10 4.293 5.707a1 1 0 010-1.414z" clip-rule="evenodd" />
            </svg>
          </button>
        </div>
      }
    </div>
  `,
})
export class AlertComponent {
  @Input() variant: AlertVariant = 'info';
  @Input() title?: string;
  @Input() dismissible = false;

  @Output() dismiss = new EventEmitter<void>();

  get alertClasses(): string {
    const base = 'alert';

    const variants: Record<AlertVariant, string> = {
      info: 'alert-info',
      success: 'alert-success',
      warning: 'alert-warning',
      error: 'alert-danger',
    };

    return `${base} ${variants[this.variant]}`;
  }

  get dismissButtonClasses(): string {
    const variants: Record<AlertVariant, string> = {
      info: 'hover:bg-info/20 focus-visible:ring-info',
      success: 'hover:bg-success/20 focus-visible:ring-success',
      warning: 'hover:bg-warning/20 focus-visible:ring-warning',
      error: 'hover:bg-danger/20 focus-visible:ring-danger',
    };

    return variants[this.variant];
  }
}
