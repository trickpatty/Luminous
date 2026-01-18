import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ToastComponent, ToastVariant } from './toast.component';

/**
 * Toast item interface for the container
 */
export interface ToastItem {
  id: string;
  message: string;
  variant?: ToastVariant;
  actionLabel?: string;
  dismissible?: boolean;
  action?: () => void;
}

@Component({
  selector: 'lum-toast-container',
  standalone: true,
  imports: [CommonModule, ToastComponent],
  template: `
    <div class="fixed bottom-4 right-4 z-toast flex flex-col gap-3 max-w-sm w-full pointer-events-none">
      @for (toast of toasts; track toast.id) {
        <div class="pointer-events-auto">
          <lum-toast
            [message]="toast.message"
            [variant]="toast.variant || 'default'"
            [actionLabel]="toast.actionLabel"
            [dismissible]="toast.dismissible ?? true"
            (action)="handleAction(toast)"
            (dismiss)="handleDismiss(toast)"
          />
        </div>
      }
    </div>
  `,
})
export class ToastContainerComponent {
  @Input() toasts: ToastItem[] = [];
  @Output() toastAction = new EventEmitter<ToastItem>();
  @Output() toastDismiss = new EventEmitter<string>();

  handleAction(toast: ToastItem): void {
    toast.action?.();
    this.toastAction.emit(toast);
    this.toastDismiss.emit(toast.id);
  }

  handleDismiss(toast: ToastItem): void {
    this.toastDismiss.emit(toast.id);
  }
}
