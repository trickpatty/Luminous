import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

export type CardSize = 'sm' | 'md' | 'lg';

@Component({
  selector: 'app-card',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div [class]="cardClasses">
      @if (title || subtitle) {
        <div class="mb-4">
          @if (title) {
            <h3 class="text-lg font-semibold text-gray-900">{{ title }}</h3>
          }
          @if (subtitle) {
            <p class="text-sm text-gray-600 mt-1">{{ subtitle }}</p>
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
  @Input() size: CardSize = 'md';
  @Input() hoverable = false;

  get cardClasses(): string {
    const base = 'bg-white rounded-xl shadow-sm border border-gray-100';

    const sizes: Record<CardSize, string> = {
      sm: 'p-4',
      md: 'p-5',
      lg: 'p-6 rounded-2xl shadow-md',
    };

    const hover = this.hoverable
      ? 'hover:shadow-md hover:border-gray-200 transition-shadow cursor-pointer'
      : '';

    return `${base} ${sizes[this.size]} ${hover}`;
  }
}
