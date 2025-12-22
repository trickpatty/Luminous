import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

export type AvatarSize = 'xs' | 'sm' | 'md' | 'lg' | 'xl';

@Component({
  selector: 'app-avatar',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div [class]="avatarClasses" [style.background-color]="backgroundColor">
      @if (src) {
        <img
          [src]="src"
          [alt]="alt || name || 'Avatar'"
          class="h-full w-full object-cover"
          (error)="handleImageError()"
        />
      } @else {
        <span [class]="initialsClasses">
          {{ initials }}
        </span>
      }
    </div>
  `,
})
export class AvatarComponent {
  @Input() src?: string;
  @Input() name?: string;
  @Input() alt?: string;
  @Input() size: AvatarSize = 'md';
  @Input() color?: string;

  imageError = false;

  get initials(): string {
    if (!this.name) return '?';

    const parts = this.name.trim().split(/\s+/);
    if (parts.length === 1) {
      return parts[0].charAt(0).toUpperCase();
    }
    return (parts[0].charAt(0) + parts[parts.length - 1].charAt(0)).toUpperCase();
  }

  get backgroundColor(): string {
    if (this.color) return this.color;
    if (!this.name) return '#e5e7eb'; // gray-200

    // Generate consistent color from name
    const colors = [
      '#3b82f6', // blue
      '#22c55e', // green
      '#eab308', // yellow
      '#f97316', // orange
      '#ef4444', // red
      '#a855f7', // purple
      '#ec4899', // pink
      '#14b8a6', // teal
    ];

    let hash = 0;
    for (let i = 0; i < this.name.length; i++) {
      hash = this.name.charCodeAt(i) + ((hash << 5) - hash);
    }
    return colors[Math.abs(hash) % colors.length];
  }

  get avatarClasses(): string {
    const base = 'inline-flex items-center justify-center rounded-full overflow-hidden flex-shrink-0';

    const sizes: Record<AvatarSize, string> = {
      xs: 'h-6 w-6',
      sm: 'h-8 w-8',
      md: 'h-12 w-12',
      lg: 'h-16 w-16',
      xl: 'h-24 w-24',
    };

    return `${base} ${sizes[this.size]}`;
  }

  get initialsClasses(): string {
    const sizes: Record<AvatarSize, string> = {
      xs: 'text-xs',
      sm: 'text-sm',
      md: 'text-lg',
      lg: 'text-xl',
      xl: 'text-3xl',
    };

    return `font-medium text-white ${sizes[this.size]}`;
  }

  handleImageError(): void {
    this.imageError = true;
    this.src = undefined;
  }
}
