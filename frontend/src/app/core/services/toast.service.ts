import { Injectable, signal } from '@angular/core';

export interface Toast {
  id: number;
  message: string;
  variant: 'success' | 'danger' | 'info' | 'warning';
}

let nextId = 1;

@Injectable({ providedIn: 'root' })
export class ToastService {
  private readonly toastsSignal = signal<Toast[]>([]);
  readonly toasts = this.toastsSignal.asReadonly();

  show(message: string, variant: Toast['variant'] = 'info', durationMs = 4000): void {
    const toast: Toast = { id: nextId++, message, variant };
    this.toastsSignal.update(list => [...list, toast]);
    setTimeout(() => this.dismiss(toast.id), durationMs);
  }

  success(message: string): void {
    this.show(message, 'success');
  }

  error(message: string): void {
    this.show(message, 'danger', 6000);
  }

  dismiss(id: number): void {
    this.toastsSignal.update(list => list.filter(t => t.id !== id));
  }
}
