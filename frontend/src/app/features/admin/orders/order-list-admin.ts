import { DatePipe } from '@angular/common';
import { Component, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { OrderService } from '../../../core/services/order.service';
import { OrderStatus, OrderSummary, PagedResult } from '../../../core/models/api.models';
import { PaginationComponent } from '../../../shared/components/pagination/pagination';

@Component({
  selector: 'app-admin-order-list',
  standalone: true,
  imports: [RouterLink, FormsModule, DatePipe, PaginationComponent],
  templateUrl: './order-list-admin.html'
})
export class AdminOrderListComponent implements OnInit {
  readonly result = signal<PagedResult<OrderSummary> | null>(null);
  statusFilter: OrderStatus | '' = '';
  page = 1;

  readonly statuses: OrderStatus[] = ['PendingPayment', 'Paid', 'Processing', 'Shipped', 'Delivered', 'Cancelled', 'Refunded'];

  constructor(private readonly orderService: OrderService) {}

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.orderService.getAllOrders({ status: this.statusFilter || undefined, page: this.page, pageSize: 15 })
      .subscribe(result => this.result.set(result));
  }

  onPageChange(page: number): void {
    this.page = page;
    this.load();
  }

  formatVnd(value: number): string {
    return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(value);
  }
}
