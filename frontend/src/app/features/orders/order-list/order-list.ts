import { DatePipe } from '@angular/common';
import { Component, OnInit, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { OrderService } from '../../../core/services/order.service';
import { OrderSummary, PagedResult } from '../../../core/models/api.models';
import { PaginationComponent } from '../../../shared/components/pagination/pagination';

@Component({
  selector: 'app-order-list',
  standalone: true,
  imports: [RouterLink, DatePipe, PaginationComponent],
  templateUrl: './order-list.html'
})
export class OrderListComponent implements OnInit {
  readonly result = signal<PagedResult<OrderSummary> | null>(null);
  readonly loading = signal(false);
  private page = 1;

  constructor(private readonly orderService: OrderService) {}

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.orderService.getMyOrders({ page: this.page, pageSize: 10 }).subscribe(result => {
      this.result.set(result);
      this.loading.set(false);
    });
  }

  onPageChange(page: number): void {
    this.page = page;
    this.load();
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
