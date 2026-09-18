import { DatePipe } from '@angular/common';
import { Component, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { ProductService } from '../../../core/services/product.service';
import { ReviewService } from '../../../core/services/review.service';
import { CartService } from '../../../core/services/cart.service';
import { WishlistService } from '../../../core/services/wishlist.service';
import { AuthService } from '../../../core/services/auth.service';
import { ToastService } from '../../../core/services/toast.service';
import { ProductDetail, ProductListItem, Review } from '../../../core/models/api.models';
import { StarRatingComponent } from '../../../shared/components/star-rating/star-rating';
import { ProductCardComponent } from '../../../shared/components/product-card/product-card';

@Component({
  selector: 'app-product-detail',
  standalone: true,
  imports: [FormsModule, RouterLink, DatePipe, StarRatingComponent, ProductCardComponent],
  templateUrl: './product-detail.html'
})
export class ProductDetailComponent implements OnInit {
  readonly product = signal<ProductDetail | null>(null);
  readonly reviews = signal<Review[]>([]);
  readonly related = signal<ProductListItem[]>([]);
  readonly activeImageIndex = signal(0);
  readonly quantity = signal(1);

  newRating = 5;
  newComment = '';
  submittingReview = false;

  constructor(
    private readonly route: ActivatedRoute,
    private readonly router: Router,
    private readonly productService: ProductService,
    private readonly reviewService: ReviewService,
    readonly cart: CartService,
    readonly wishlist: WishlistService,
    readonly auth: AuthService,
    private readonly toast: ToastService
  ) {}

  ngOnInit(): void {
    this.route.paramMap.subscribe(params => {
      const slug = params.get('slug');
      if (!slug) return;
      this.loadProduct(slug);
    });
  }

  private loadProduct(slug: string): void {
    this.activeImageIndex.set(0);
    this.quantity.set(1);
    this.productService.getBySlug(slug).subscribe(product => {
      this.product.set(product);
      this.reviewService.getForProduct(product.id).subscribe(reviews => this.reviews.set(reviews));
      this.productService.getRelated(product.id).subscribe(related => this.related.set(related));
    });
  }

  addToCart(): void {
    const product = this.product();
    if (!product) return;
    if (!this.auth.isAuthenticated()) { this.router.navigate(['/login']); return; }

    this.cart.addItem(product.id, this.quantity()).subscribe(() => this.toast.success('Added to cart.'));
  }

  toggleWishlist(): void {
    const product = this.product();
    if (!product) return;
    if (!this.auth.isAuthenticated()) { this.router.navigate(['/login']); return; }

    const action = this.wishlist.productIds().has(product.id) ? this.wishlist.remove(product.id) : this.wishlist.add(product.id);
    action.subscribe();
  }

  addRelatedToCart(productId: number): void {
    if (!this.auth.isAuthenticated()) { this.router.navigate(['/login']); return; }
    this.cart.addItem(productId).subscribe(() => this.toast.success('Added to cart.'));
  }

  toggleRelatedWishlist(productId: number): void {
    if (!this.auth.isAuthenticated()) { this.router.navigate(['/login']); return; }
    const action = this.wishlist.productIds().has(productId) ? this.wishlist.remove(productId) : this.wishlist.add(productId);
    action.subscribe();
  }

  submitReview(): void {
    const product = this.product();
    if (!product) return;

    this.submittingReview = true;
    this.reviewService.create(product.id, this.newRating, this.newComment || null).subscribe({
      next: review => {
        this.reviews.update(list => [review, ...list]);
        this.newComment = '';
        this.newRating = 5;
        this.submittingReview = false;
        this.toast.success('Review submitted. Thank you!');
      },
      error: () => { this.submittingReview = false; }
    });
  }

  incrementQuantity(): void {
    const max = this.product()?.stockQuantity ?? 1;
    this.quantity.update(q => Math.min(q + 1, max));
  }

  decrementQuantity(): void {
    this.quantity.update(q => Math.max(q - 1, 1));
  }

  formatVnd(value: number): string {
    return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(value);
  }
}
