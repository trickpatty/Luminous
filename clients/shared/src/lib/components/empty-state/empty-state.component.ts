import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ButtonComponent } from '../button/button.component';

@Component({
  selector: 'lum-empty-state',
  standalone: true,
  imports: [CommonModule, ButtonComponent],
  template: `
    <div class="empty-state">
      @if (icon) {
        <div class="empty-state-icon" [innerHTML]="icon"></div>
      } @else {
        <!-- Default empty state icon -->
        <svg class="empty-state-icon" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="1.5">
          <path stroke-linecap="round" stroke-linejoin="round" d="M20 13V6a2 2 0 00-2-2H6a2 2 0 00-2 2v7m16 0v5a2 2 0 01-2 2H6a2 2 0 01-2-2v-5m16 0h-2.586a1 1 0 00-.707.293l-2.414 2.414a1 1 0 01-.707.293h-3.172a1 1 0 01-.707-.293l-2.414-2.414A1 1 0 006.586 13H4" />
        </svg>
      }

      <h3 class="empty-state-title">{{ title }}</h3>

      @if (description) {
        <p class="empty-state-description">{{ description }}</p>
      }

      @if (actionLabel) {
        <lum-button
          [variant]="actionVariant"
          (onClick)="action.emit()"
        >
          {{ actionLabel }}
        </lum-button>
      }
    </div>
  `,
})
export class EmptyStateComponent {
  @Input() title = 'No items found';
  @Input() description?: string;
  @Input() icon?: string;
  @Input() actionLabel?: string;
  @Input() actionVariant: 'primary' | 'secondary' | 'outline' = 'primary';

  @Output() action = new EventEmitter<void>();
}
