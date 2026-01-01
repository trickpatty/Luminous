import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

export type CardVariant = 'default' | 'elevated' | 'interactive' | 'outlined' | 'personal';
export type CardSize = 'sm' | 'md' | 'lg';

@Component({
  selector: 'app-card',
  standalone: true,
  imports: [CommonModule],
  host: {
    'style': 'display: block;'
  },
  template: `
    <div [class]="cardClasses" [style]="cardStyles">
      @if (title || subtitle) {
        <div class="mb-4">
          @if (title) {
            <h3 class="text-title-md text-text-primary">{{ title }}</h3>
          }
          @if (subtitle) {
            <p class="text-body-sm text-text-secondary mt-1">{{ subtitle }}</p>
          }
        </div>
      }
      <ng-content></ng-content>
    </div>
  `,
})
export class CardComponent {
  @Input() title?: string;
  @Input() subtitle?: string;
  @Input() variant: CardVariant = 'default';
  @Input() size: CardSize = 'md';
  /** @deprecated Use variant='interactive' instead */
  @Input() hoverable = false;
  /** Member color for personal cards (hex color) */
  @Input() memberColor?: string;

  get cardClasses(): string {
    // Handle legacy hoverable prop
    const effectiveVariant = this.hoverable ? 'interactive' : this.variant;

    const variants: Record<CardVariant, string> = {
      default: 'card',
      elevated: 'card-elevated',
      interactive: 'card-interactive',
      outlined: 'card-outlined',
      personal: 'card-personal',
    };

    const sizes: Record<CardSize, string> = {
      sm: 'p-4',
      md: '', // Default padding handled by CSS
      lg: 'p-6 rounded-2xl',
    };

    return `${variants[effectiveVariant]} ${sizes[this.size]}`.trim();
  }

  get cardStyles(): Record<string, string> {
    if (this.variant === 'personal' && this.memberColor) {
      return {
        '--member-color': this.memberColor,
        '--member-color-light': `${this.memberColor}1F`, // 12% opacity
      };
    }
    return {};
  }
}
