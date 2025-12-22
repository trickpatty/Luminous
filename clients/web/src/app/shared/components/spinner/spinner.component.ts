import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

export type SpinnerSize = 'sm' | 'md' | 'lg' | 'xl';

@Component({
  selector: 'app-spinner',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div [class]="containerClasses" role="status">
      <svg
        [class]="spinnerClasses"
        viewBox="0 0 24 24"
        fill="none"
        xmlns="http://www.w3.org/2000/svg"
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
      @if (label) {
        <span class="ml-2 text-gray-600">{{ label }}</span>
      }
      <span class="sr-only">{{ label || 'Loading...' }}</span>
    </div>
  `,
})
export class SpinnerComponent {
  @Input() size: SpinnerSize = 'md';
  @Input() label?: string;
  @Input() centered = false;

  get containerClasses(): string {
    const base = 'inline-flex items-center';
    const centered = this.centered ? 'justify-center w-full' : '';
    return `${base} ${centered}`;
  }

  get spinnerClasses(): string {
    const base = 'animate-spin text-primary-600';

    const sizes: Record<SpinnerSize, string> = {
      sm: 'h-4 w-4',
      md: 'h-6 w-6',
      lg: 'h-8 w-8',
      xl: 'h-12 w-12',
    };

    return `${base} ${sizes[this.size]}`;
  }
}
