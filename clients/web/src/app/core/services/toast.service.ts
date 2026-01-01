import { Injectable, signal, computed } from '@angular/core';

export interface Toast {
  id: string;
  message: string;
  variant: 'default' | 'success' | 'error' | 'warning';
  actionLabel?: string;
  action?: () => void;
  duration?: number;
  dismissible?: boolean;
}

@Injectable({
  providedIn: 'root',
})
export class ToastService {
  private toastsSignal = signal<Toast[]>([]);
  private counter = 0;

  /** Observable list of active toasts */
  readonly toasts = computed(() => this.toastsSignal());

  /**
   * Show a toast notification
   */
  show(
    message: string,
    options: Partial<Omit<Toast, 'id' | 'message'>> = {}
  ): string {
    const id = `toast-${++this.counter}`;
    const toast: Toast = {
      id,
      message,
      variant: options.variant ?? 'default',
      actionLabel: options.actionLabel,
      action: options.action,
      duration: options.duration ?? 5000,
      dismissible: options.dismissible ?? true,
    };

    this.toastsSignal.update(toasts => [...toasts, toast]);

    // Auto-dismiss after duration
    if (toast.duration && toast.duration > 0) {
      setTimeout(() => this.dismiss(id), toast.duration);
    }

    return id;
  }

  /**
   * Show a success toast
   */
  success(message: string, options: Partial<Omit<Toast, 'id' | 'message' | 'variant'>> = {}): string {
    return this.show(message, { ...options, variant: 'success' });
  }

  /**
   * Show an error toast
   */
  error(message: string, options: Partial<Omit<Toast, 'id' | 'message' | 'variant'>> = {}): string {
    return this.show(message, { ...options, variant: 'error', duration: options.duration ?? 8000 });
  }

  /**
   * Show a warning toast
   */
  warning(message: string, options: Partial<Omit<Toast, 'id' | 'message' | 'variant'>> = {}): string {
    return this.show(message, { ...options, variant: 'warning' });
  }

  /**
   * Dismiss a toast by ID
   */
  dismiss(id: string): void {
    this.toastsSignal.update(toasts => toasts.filter(t => t.id !== id));
  }

  /**
   * Dismiss all toasts
   */
  dismissAll(): void {
    this.toastsSignal.set([]);
  }
}
