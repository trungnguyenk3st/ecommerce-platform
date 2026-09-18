import { Component, OnInit } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { HeaderComponent } from './shared/components/header/header';
import { FooterComponent } from './shared/components/footer/footer';
import { ToastContainerComponent } from './shared/components/toast-container/toast-container';
import { AuthService } from './core/services/auth.service';
import { CartService } from './core/services/cart.service';
import { WishlistService } from './core/services/wishlist.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, HeaderComponent, FooterComponent, ToastContainerComponent],
  templateUrl: './app.html'
})
export class App implements OnInit {
  constructor(
    private readonly auth: AuthService,
    private readonly cart: CartService,
    private readonly wishlist: WishlistService
  ) {}

  ngOnInit(): void {
    if (this.auth.isAuthenticated()) {
      this.cart.refresh().subscribe({ error: () => void 0 });
      this.wishlist.refresh().subscribe({ error: () => void 0 });
    }
  }
}
