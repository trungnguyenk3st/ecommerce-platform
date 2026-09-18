import { Component, input, output } from '@angular/core';
import { RouterLink } from '@angular/router';
import { StarRatingComponent } from '../star-rating/star-rating';
import { ProductListItem } from '../../../core/models/api.models';

@Component({
  selector: 'app-product-card',
  standalone: true,
  imports: [RouterLink, StarRatingComponent],
  templateUrl: './product-card.html'
})
export class ProductCardComponent {
  readonly product = input.required<ProductListItem>();
  readonly isWishlisted = input(false);
  readonly addToCart = output<number>();
  readonly toggleWishlist = output<number>();

  readonly formatVnd = (value: number) => new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(value);
}
