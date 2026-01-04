import { Component, Input, ContentChildren, QueryList, AfterContentInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AvatarComponent, AvatarSize, MemberColorName } from '../avatar/avatar.component';

export interface AvatarGroupItem {
  name?: string;
  src?: string;
  color?: string;
  memberColor?: MemberColorName;
}

@Component({
  selector: 'app-avatar-group',
  standalone: true,
  imports: [CommonModule, AvatarComponent],
  host: {
    'style': 'display: contents;',
  },
  template: `
    <div class="avatar-group" [attr.aria-label]="ariaLabel">
      <!-- Support for items input -->
      @for (item of visibleItems; track $index) {
        <app-avatar
          [name]="item.name"
          [src]="item.src"
          [color]="item.color"
          [memberColor]="item.memberColor"
          [size]="size"
        />
      }
      <!-- Support for content projection -->
      <ng-content></ng-content>
      @if (overflowCount > 0) {
        <div
          [class]="overflowClasses"
          [attr.aria-label]="'+' + overflowCount + ' more'"
        >
          +{{ overflowCount }}
        </div>
      }
    </div>
  `,
})
export class AvatarGroupComponent implements AfterContentInit {
  @Input() items: AvatarGroupItem[] = [];
  @Input() max = 4;
  @Input() size: AvatarSize = 'md';

  @ContentChildren(AvatarComponent) projectedAvatars!: QueryList<AvatarComponent>;

  private projectedCount = 0;

  ngAfterContentInit(): void {
    this.projectedCount = this.projectedAvatars?.length ?? 0;
  }

  get visibleItems(): AvatarGroupItem[] {
    return this.items.slice(0, this.max);
  }

  get overflowCount(): number {
    // Count overflow from either items input or projected content
    const itemsOverflow = Math.max(0, this.items.length - this.max);
    const projectedOverflow = Math.max(0, this.projectedCount - this.max);
    return itemsOverflow || projectedOverflow;
  }

  get ariaLabel(): string {
    const names = this.items.map(i => i.name).filter(Boolean).join(', ');
    return names || 'User avatars';
  }

  get overflowClasses(): string {
    const sizeClasses: Record<AvatarSize, string> = {
      xs: 'avatar-xs text-[8px]',
      sm: 'avatar-sm text-[10px]',
      md: 'avatar-md text-xs',
      lg: 'avatar-lg text-sm',
      xl: 'avatar-xl text-base',
      '2xl': 'avatar-2xl text-xl',
    };

    return `avatar bg-surface-secondary text-text-secondary font-medium ${sizeClasses[this.size]}`;
  }
}
