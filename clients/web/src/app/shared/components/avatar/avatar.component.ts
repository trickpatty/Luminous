import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

export type AvatarSize = 'xs' | 'sm' | 'md' | 'lg' | 'xl' | '2xl';

/** Design system member colors */
export const MEMBER_COLORS = {
  sky: '#0EA5E9',
  emerald: '#10B981',
  amber: '#F59E0B',
  orange: '#F97316',
  rose: '#F43F5E',
  violet: '#8B5CF6',
  pink: '#EC4899',
  teal: '#14B8A6',
} as const;

export type MemberColorName = keyof typeof MEMBER_COLORS;

@Component({
  selector: 'app-avatar',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div
      [class]="avatarClasses"
      [style.background-color]="backgroundColor"
      [attr.role]="'img'"
      [attr.aria-label]="alt || name || 'User avatar'"
    >
      @if (src && !imageError) {
        <img
          [src]="src"
          [alt]="alt || name || 'Avatar'"
          class="h-full w-full object-cover"
          (error)="handleImageError()"
        />
      } @else {
        <span [class]="initialsClasses" aria-hidden="true">
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
  /** Direct hex color override */
  @Input() color?: string;
  /** Named member color from design system */
  @Input() memberColor?: MemberColorName;
  /** Whether to use text color black instead of white (for light backgrounds like amber) */
  @Input() darkText = false;

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
    // Priority: direct color > memberColor > generated from name
    if (this.color) return this.color;
    if (this.memberColor) return MEMBER_COLORS[this.memberColor];
    if (!this.name) return '#9CA3AF'; // text-tertiary fallback

    // Generate consistent color from name using design system colors
    const colorValues = Object.values(MEMBER_COLORS);
    let hash = 0;
    for (let i = 0; i < this.name.length; i++) {
      hash = this.name.charCodeAt(i) + ((hash << 5) - hash);
    }
    return colorValues[Math.abs(hash) % colorValues.length];
  }

  get textColor(): string {
    // Amber needs dark text for contrast
    if (this.memberColor === 'amber' || this.darkText) {
      return 'text-gray-900';
    }
    return 'text-white';
  }

  get avatarClasses(): string {
    const base = 'avatar';

    const sizes: Record<AvatarSize, string> = {
      xs: 'avatar-xs',
      sm: 'avatar-sm',
      md: 'avatar-md',
      lg: 'avatar-lg',
      xl: 'avatar-xl',
      '2xl': 'avatar-2xl',
    };

    return `${base} ${sizes[this.size]}`;
  }

  get initialsClasses(): string {
    return `font-semibold ${this.textColor}`;
  }

  handleImageError(): void {
    this.imageError = true;
  }
}
