import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ToastComponent } from './toast.component';
import { ToastService } from '../../../core/services/toast.service';

@Component({
  selector: 'app-toast-container',
  standalone: true,
  imports: [CommonModule, ToastComponent],
  template: `
    <div class="fixed bottom-4 right-4 z-toast flex flex-col gap-3 max-w-sm w-full pointer-events-none">
      @for (toast of toastService.toasts(); track toast.id) {
        <div class="pointer-events-auto">
          <app-toast
            [message]="toast.message"
            [variant]="toast.variant"
            [actionLabel]="toast.actionLabel"
            [dismissible]="toast.dismissible ?? true"
            (action)="handleAction(toast)"
            (dismiss)="toastService.dismiss(toast.id)"
          />
        </div>
      }
    </div>
  `,
})
export class ToastContainerComponent {
  protected toastService = inject(ToastService);

  handleAction(toast: { id: string; action?: () => void }): void {
    toast.action?.();
    this.toastService.dismiss(toast.id);
  }
}
