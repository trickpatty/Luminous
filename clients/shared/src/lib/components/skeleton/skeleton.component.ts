import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

export type SkeletonVariant = 'text' | 'avatar' | 'card' | 'rectangle';

@Component({
  selector: 'lum-skeleton',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div
      [class]="skeletonClasses"
      [style.width]="width"
      [style.height]="height"
      aria-hidden="true"
    ></div>
  `,
})
export class SkeletonComponent {
  @Input() variant: SkeletonVariant = 'text';
  @Input() width?: string;
  @Input() height?: string;

  get skeletonClasses(): string {
    const base = 'skeleton';

    const variants: Record<SkeletonVariant, string> = {
      text: 'skeleton-text',
      avatar: 'skeleton-avatar',
      card: 'skeleton-card',
      rectangle: '',
    };

    return `${base} ${variants[this.variant]}`;
  }
}
