import { Component, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { ProductService } from '../../../core/services/product.service';
import { CategoryService } from '../../../core/services/category.service';
import { CartService } from '../../../core/services/cart.service';
import { WishlistService } from '../../../core/services/wishlist.service';
import { ToastService } from '../../../core/services/toast.service';
import { AuthService } from '../../../core/services/auth.service';
import { Category, PagedResult, ProductListItem, ProductQueryParams } from '../../../core/models/api.models';
import { ProductCardComponent } from '../../../shared/components/product-card/product-card';
import { PaginationComponent } from '../../../shared/components/pagination/pagination';

@Component({
  selector: 'app-product-list',
  standalone: true,
  imports: [FormsModule, ProductCardComponent, PaginationComponent],
  templateUrl: './product-list.html'
})
export class ProductListComponent implements OnInit {
  readonly result = signal<PagedResult<ProductListItem> | null>(null);
  readonly categories = signal<Category[]>([]);
  readonly loading = signal(false);

  filters: ProductQueryParams = { page: 1, pageSize: 12, sortBy: 'newest' };

  constructor(
    private readonly productService: ProductService,
    private readonly categoryService: CategoryService,
    readonly cart: CartService,
    readonly wishlist: WishlistService,
    private readonly toast: ToastService,
    private readonly auth: AuthService,
    private readonly router: Router,
    private readonly route: ActivatedRoute
  ) {}

  ngOnInit(): void {
    this.categoryService.getFlat().subscribe(categories => this.categories.set(categories));

    this.route.queryParamMap.subscribe(params => {
      this.filters = {
        ...this.filters,
        search: params.get('search') ?? undefined,
        categoryId: params.get('categoryId') ? Number(params.get('categoryId')) : undefined,
        page: 1
      };
      this.load();
    });
  }

  load(): void {
    this.loading.set(true);
    this.productService.search(this.filters).subscribe({
      next: result => { this.result.set(result); this.loading.set(false); },
      error: () => this.loading.set(false)
    });
  }

  onPageChange(page: number): void {
    this.filters.page = page;
    this.load();
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }

  clearFilters(): void {
    this.filters = { page: 1, pageSize: 12, sortBy: 'newest' };
    this.router.navigate(['/products']);
  }

  addToCart(productId: number): void {
    if (!this.auth.isAuthenticated()) { this.router.navigate(['/login']); return; }
    this.cart.addItem(productId).subscribe(() => this.toast.success('Added to cart.'));
  }

  toggleWishlist(productId: number): void {
    if (!this.auth.isAuthenticated()) { this.router.navigate(['/login']); return; }
    const action = this.wishlist.productIds().has(productId) ? this.wishlist.remove(productId) : this.wishlist.add(productId);
    action.subscribe();
  }
}
