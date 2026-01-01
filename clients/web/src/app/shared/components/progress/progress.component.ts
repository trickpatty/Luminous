import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-progress',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div
      class="progress-track"
      role="progressbar"
      [attr.aria-valuenow]="value"
      [attr.aria-valuemin]="0"
      [attr.aria-valuemax]="max"
      [attr.aria-label]="label"
    >
      <div
        [class]="fillClasses"
        [style.width.%]="percentage"
        [style]="memberColor ? { '--member-color': memberColor } : {}"
      ></div>
    </div>
  `,
})
export class ProgressComponent {
  @Input() value = 0;
  @Input() max = 100;
  @Input() label?: string;
  /** Use member color for personalized progress */
  @Input() memberColor?: string;

  get percentage(): number {
    return Math.min(100, Math.max(0, (this.value / this.max) * 100));
  }

  get fillClasses(): string {
    const base = 'progress-fill';
    return this.memberColor ? `${base} progress-fill-personal` : base;
  }
}
