import { Component, OnInit, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { CategoryService } from '../../core/services/category.service';
import { ProductService } from '../../core/services/product.service';
import { CartService } from '../../core/services/cart.service';
import { WishlistService } from '../../core/services/wishlist.service';
import { ToastService } from '../../core/services/toast.service';
import { AuthService } from '../../core/services/auth.service';
import { Category, ProductListItem } from '../../core/models/api.models';
import { ProductCardComponent } from '../../shared/components/product-card/product-card';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [RouterLink, ProductCardComponent],
  templateUrl: './home.html'
})
export class HomeComponent implements OnInit {
  readonly categories = signal<Category[]>([]);
  readonly featuredProducts = signal<ProductListItem[]>([]);

  constructor(
    private readonly categoryService: CategoryService,
    private readonly productService: ProductService,
    readonly cart: CartService,
    readonly wishlist: WishlistService,
    private readonly toast: ToastService,
    private readonly auth: AuthService,
    private readonly router: Router
  ) {}

  ngOnInit(): void {
    this.categoryService.getTree().subscribe(categories => this.categories.set(categories));
    this.productService.search({ sortBy: 'newest', pageSize: 8 }).subscribe(result => this.featuredProducts.set(result.items));
  }

  addToCart(productId: number): void {
    if (!this.auth.isAuthenticated()) {
      this.router.navigate(['/login']);
      return;
    }
    this.cart.addItem(productId).subscribe(() => this.toast.success('Added to cart.'));
  }

  toggleWishlist(productId: number): void {
    if (!this.auth.isAuthenticated()) {
      this.router.navigate(['/login']);
      return;
    }
    const action = this.wishlist.productIds().has(productId)
      ? this.wishlist.remove(productId)
      : this.wishlist.add(productId);
    action.subscribe();
  }
}
