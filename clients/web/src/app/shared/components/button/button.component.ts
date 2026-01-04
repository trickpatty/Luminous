import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';

export type ButtonVariant = 'primary' | 'secondary' | 'ghost' | 'outline' | 'danger';
export type ButtonSize = 'sm' | 'md' | 'lg' | 'xl';

@Component({
  selector: 'app-button',
  standalone: true,
  imports: [CommonModule],
  host: {
    'style': 'display: inline-block;',
  },
  template: `
    <button
      [type]="type"
      [disabled]="disabled || loading"
      [class]="buttonClasses"
      [attr.aria-busy]="loading"
      (click)="onClick.emit($event)"
    >
      @if (loading) {
        <svg
          class="animate-spin h-5 w-5"
          [class.mr-2]="!iconOnly"
          fill="none"
          viewBox="0 0 24 24"
          aria-hidden="true"
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
  @Input() iconOnly = false;

  @Output() onClick = new EventEmitter<MouseEvent>();

  get buttonClasses(): string {
    const base = 'btn';

    const variants: Record<ButtonVariant, string> = {
      primary: 'btn-primary',
      secondary: 'btn-secondary',
      ghost: 'btn-ghost',
      outline: 'btn-outline',
      danger: 'btn-danger',
    };

    const sizes: Record<ButtonSize, string> = {
      sm: 'btn-sm',
      md: 'btn-md',
      lg: 'btn-lg',
      xl: 'btn-xl',
    };

    const width = this.fullWidth ? 'w-full' : '';
    const iconOnlyClass = this.iconOnly ? 'p-0 min-w-0' : '';

    return `${base} ${variants[this.variant]} ${sizes[this.size]} ${width} ${iconOnlyClass}`.trim();
  }
}
