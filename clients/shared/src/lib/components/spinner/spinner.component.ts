import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

export type SpinnerSize = 'sm' | 'md' | 'lg' | 'xl';

@Component({
  selector: 'lum-spinner',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div [class]="containerClasses" role="status" aria-live="polite">
      <div [class]="spinnerClasses"></div>
      @if (label) {
        <span class="ml-3 text-body-sm text-text-secondary">{{ label }}</span>
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
    const base = 'spinner';

    const sizes: Record<SpinnerSize, string> = {
      sm: 'spinner-sm',
      md: 'spinner-md',
      lg: 'spinner-lg',
      xl: 'spinner-xl',
    };

    return `${base} ${sizes[this.size]}`;
  }
}
