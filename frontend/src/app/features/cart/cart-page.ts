import { Component, OnInit } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { CartService } from '../../core/services/cart.service';
import { AuthService } from '../../core/services/auth.service';
import { ToastService } from '../../core/services/toast.service';

@Component({
  selector: 'app-cart-page',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './cart-page.html'
})
export class CartPageComponent implements OnInit {
  constructor(
    readonly cart: CartService,
    readonly auth: AuthService,
    private readonly router: Router,
    private readonly toast: ToastService
  ) {}

  ngOnInit(): void {
    if (this.auth.isAuthenticated()) this.cart.refresh().subscribe();
  }

  updateQuantity(cartItemId: number, quantity: number): void {
    if (quantity < 1) return;
    this.cart.updateItem(cartItemId, quantity).subscribe();
  }

  removeItem(cartItemId: number): void {
    this.cart.removeItem(cartItemId).subscribe(() => this.toast.show('Item removed from cart.'));
  }

  goToCheckout(): void {
    if (!this.auth.isAuthenticated()) {
      this.router.navigate(['/login'], { queryParams: { returnUrl: '/checkout' } });
      return;
    }
    this.router.navigate(['/checkout']);
  }

  formatVnd(value: number): string {
    return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(value);
  }
}
