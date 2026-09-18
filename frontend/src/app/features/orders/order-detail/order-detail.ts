import { DatePipe } from '@angular/common';
import { Component, OnInit, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { OrderService } from '../../../core/services/order.service';
import { ToastService } from '../../../core/services/toast.service';
import { Order } from '../../../core/models/api.models';

@Component({
  selector: 'app-order-detail',
  standalone: true,
  imports: [RouterLink, DatePipe],
  templateUrl: './order-detail.html'
})
export class OrderDetailComponent implements OnInit {
  readonly order = signal<Order | null>(null);
  readonly cancelling = signal(false);

  constructor(
    private readonly route: ActivatedRoute,
    private readonly orderService: OrderService,
    private readonly toast: ToastService
  ) {}

  ngOnInit(): void {
    const id = Number(this.route.snapshot.paramMap.get('id'));
    this.orderService.getMyOrder(id).subscribe(order => this.order.set(order));
  }

  get canCancel(): boolean {
    const status = this.order()?.status;
    return status === 'PendingPayment' || status === 'Processing';
  }

  cancelOrder(): void {
    const order = this.order();
    if (!order || !confirm('Cancel this order?')) return;

    this.cancelling.set(true);
    this.orderService.cancelMyOrder(order.id).subscribe({
      next: updated => { this.order.set(updated); this.cancelling.set(false); this.toast.show('Order cancelled.'); },
      error: () => this.cancelling.set(false)
    });
  }

  statusVariant(status: string): string {
    switch (status) {
      case 'Delivered': return 'success';
      case 'Paid': case 'Processing': case 'Shipped': return 'primary';
      case 'Cancelled': case 'Refunded': return 'secondary';
      default: return 'warning';
    }
  }

  formatVnd(value: number): string {
    return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(value);
  }
}
