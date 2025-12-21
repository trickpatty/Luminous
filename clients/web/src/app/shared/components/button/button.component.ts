import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';

export type ButtonVariant = 'primary' | 'secondary' | 'ghost' | 'danger';
export type ButtonSize = 'sm' | 'md' | 'lg';

@Component({
  selector: 'app-button',
  standalone: true,
  imports: [CommonModule],
  template: `
    <button
      [type]="type"
      [disabled]="disabled || loading"
      [class]="buttonClasses"
      (click)="onClick.emit($event)"
    >
      @if (loading) {
        <svg
          class="animate-spin -ml-1 mr-2 h-5 w-5"
          fill="none"
          viewBox="0 0 24 24"
        >
          <circle
            class="opacity-25"
            cx="12"
            cy="12"
            r="10"
            stroke="currentColor"
            stroke-width="4"
          ></circle>
          <path
            class="opacity-75"
            fill="currentColor"
            d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"
          ></path>
        </svg>
      }
      <ng-content></ng-content>
    </button>
  `,
})
export class ButtonComponent {
  @Input() variant: ButtonVariant = 'primary';
  @Input() size: ButtonSize = 'md';
  @Input() type: 'button' | 'submit' | 'reset' = 'button';
  @Input() disabled = false;
  @Input() loading = false;
  @Input() fullWidth = false;

  @Output() onClick = new EventEmitter<MouseEvent>();

  get buttonClasses(): string {
    const base = 'inline-flex items-center justify-center font-medium rounded-lg transition-colors duration-150 ease-in-out focus:ring-4';

    const variants: Record<ButtonVariant, string> = {
      primary: 'bg-primary-600 text-white hover:bg-primary-700 focus:ring-primary-200 disabled:bg-primary-300',
      secondary: 'bg-gray-100 text-gray-700 hover:bg-gray-200 focus:ring-gray-100 disabled:bg-gray-50 disabled:text-gray-400',
      ghost: 'bg-transparent text-gray-700 hover:bg-gray-100 focus:ring-gray-100',
      danger: 'bg-red-600 text-white hover:bg-red-700 focus:ring-red-200 disabled:bg-red-300',
    };

    const sizes: Record<ButtonSize, string> = {
      sm: 'px-4 py-2 text-sm min-h-[36px]',
      md: 'px-6 py-3 text-base min-h-touch',
      lg: 'px-8 py-4 text-lg min-h-touch-lg',
    };

    const width = this.fullWidth ? 'w-full' : '';

    return `${base} ${variants[this.variant]} ${sizes[this.size]} ${width}`;
  }
}
