import { Component, OnInit, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { OrderService } from '../../../core/services/order.service';
import { PaymentService } from '../../../core/services/payment.service';
import { Order } from '../../../core/models/api.models';

@Component({
  selector: 'app-payment-result',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './payment-result.html'
})
export class PaymentResultComponent implements OnInit {
  readonly loading = signal(true);
  readonly order = signal<Order | null>(null);
  readonly errorMessage = signal<string | null>(null);

  constructor(
    private readonly route: ActivatedRoute,
    private readonly orderService: OrderService,
    private readonly paymentService: PaymentService
  ) {}

  ngOnInit(): void {
    const params = this.route.snapshot.queryParamMap;
    const hasVnPayParams = params.keys.some(key => key.startsWith('vnp_'));

    if (hasVnPayParams) {
      const queryParams: Record<string, string> = {};
      params.keys.forEach(key => { queryParams[key] = params.get(key) ?? ''; });

      this.paymentService.confirmVnPayReturn(queryParams).subscribe({
        next: order => { this.order.set(order); this.loading.set(false); },
        error: () => { this.errorMessage.set('We could not verify your payment. Please check your order history.'); this.loading.set(false); }
      });
      return;
    }

    const orderId = params.get('orderId');
    if (!orderId) { this.loading.set(false); this.errorMessage.set('No order reference found.'); return; }

    this.orderService.getMyOrder(Number(orderId)).subscribe({
      next: order => { this.order.set(order); this.loading.set(false); },
      error: () => { this.errorMessage.set('Order not found.'); this.loading.set(false); }
    });
  }

  formatVnd(value: number): string {
    return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(value);
  }
}
