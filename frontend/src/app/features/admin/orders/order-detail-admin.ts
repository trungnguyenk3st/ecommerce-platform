import { DatePipe } from '@angular/common';
import { Component, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { OrderService } from '../../../core/services/order.service';
import { ToastService } from '../../../core/services/toast.service';
import { Order, OrderStatus } from '../../../core/models/api.models';

@Component({
  selector: 'app-admin-order-detail',
  standalone: true,
  imports: [RouterLink, FormsModule, DatePipe],
  templateUrl: './order-detail-admin.html'
})
export class AdminOrderDetailComponent implements OnInit {
  readonly order = signal<Order | null>(null);
  readonly updating = signal(false);
  selectedStatus: OrderStatus = 'PendingPayment';

  readonly statuses: OrderStatus[] = ['PendingPayment', 'Paid', 'Processing', 'Shipped', 'Delivered', 'Cancelled', 'Refunded'];

  constructor(
    private readonly route: ActivatedRoute,
    private readonly orderService: OrderService,
    private readonly toast: ToastService
  ) {}

  ngOnInit(): void {
    const id = Number(this.route.snapshot.paramMap.get('id'));
    this.orderService.getOrder(id).subscribe(order => {
      this.order.set(order);
      this.selectedStatus = order.status;
    });
  }

  updateStatus(): void {
    const order = this.order();
    if (!order) return;

    this.updating.set(true);
    this.orderService.updateStatus(order.id, this.selectedStatus).subscribe({
      next: updated => { this.order.set(updated); this.updating.set(false); this.toast.success('Order status updated.'); },
      error: () => this.updating.set(false)
    });
  }

  formatVnd(value: number): string {
    return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(value);
  }
}
