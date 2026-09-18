import { Component, OnInit } from '@angular/core';
import { RouterLink } from '@angular/router';
import { WishlistService } from '../../core/services/wishlist.service';
import { CartService } from '../../core/services/cart.service';
import { ToastService } from '../../core/services/toast.service';

@Component({
  selector: 'app-wishlist-page',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './wishlist-page.html'
})
export class WishlistPageComponent implements OnInit {
  constructor(
    readonly wishlist: WishlistService,
    private readonly cart: CartService,
    private readonly toast: ToastService
  ) {}

  ngOnInit(): void {
    this.wishlist.refresh().subscribe();
  }

  addToCart(productId: number): void {
    this.cart.addItem(productId).subscribe(() => this.toast.success('Added to cart.'));
  }

  remove(productId: number): void {
    this.wishlist.remove(productId).subscribe();
  }

  formatVnd(value: number): string {
    return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(value);
  }
}
