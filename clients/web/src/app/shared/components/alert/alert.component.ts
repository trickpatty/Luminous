import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';

export type AlertVariant = 'info' | 'success' | 'warning' | 'error';

@Component({
  selector: 'app-alert',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div [class]="alertClasses" role="alert">
      <div class="flex">
        <div class="flex-shrink-0">
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
        <div class="ml-3 flex-1">
          @if (title) {
            <h3 class="text-sm font-medium">{{ title }}</h3>
          }
          <div [class]="title ? 'mt-1 text-sm' : 'text-sm'">
            <ng-content></ng-content>
          </div>
        </div>
        @if (dismissible) {
          <div class="ml-auto pl-3">
            <button
              type="button"
              (click)="dismiss.emit()"
              class="-mx-1.5 -my-1.5 inline-flex rounded-md p-1.5 focus:outline-none focus:ring-2 focus:ring-offset-2"
              [class]="dismissButtonClasses"
            >
              <span class="sr-only">Dismiss</span>
              <svg class="h-5 w-5" viewBox="0 0 20 20" fill="currentColor">
                <path fill-rule="evenodd" d="M4.293 4.293a1 1 0 011.414 0L10 8.586l4.293-4.293a1 1 0 111.414 1.414L11.414 10l4.293 4.293a1 1 0 01-1.414 1.414L10 11.414l-4.293 4.293a1 1 0 01-1.414-1.414L8.586 10 4.293 5.707a1 1 0 010-1.414z" clip-rule="evenodd" />
              </svg>
            </button>
          </div>
        }
      </div>
    </div>
  `,
})
export class AlertComponent {
  @Input() variant: AlertVariant = 'info';
  @Input() title?: string;
  @Input() dismissible = false;

  @Output() dismiss = new EventEmitter<void>();

  get alertClasses(): string {
    const base = 'rounded-lg p-4 border';

    const variants: Record<AlertVariant, string> = {
      info: 'bg-blue-50 border-blue-200 text-blue-800',
      success: 'bg-green-50 border-green-200 text-green-800',
      warning: 'bg-yellow-50 border-yellow-200 text-yellow-800',
      error: 'bg-red-50 border-red-200 text-red-800',
    };

    return `${base} ${variants[this.variant]}`;
  }

  get dismissButtonClasses(): string {
    const variants: Record<AlertVariant, string> = {
      info: 'hover:bg-blue-100 focus:ring-blue-600',
      success: 'hover:bg-green-100 focus:ring-green-600',
      warning: 'hover:bg-yellow-100 focus:ring-yellow-600',
      error: 'hover:bg-red-100 focus:ring-red-600',
    };

    return variants[this.variant];
  }
}
