import { Component, computed, input } from '@angular/core';

@Component({
  selector: 'app-star-rating',
  standalone: true,
  template: `
    <span class="text-warning" [attr.aria-label]="rating() + ' out of 5 stars'">
      @for (i of stars(); track i) {
        <i class="bi" [class.bi-star-fill]="i <= rounded()" [class.bi-star]="i > rounded()"></i>
      }
    </span>
    @if (showCount()) {
      <span class="text-muted small ms-1">({{ count() }})</span>
    }
  `
})
export class StarRatingComponent {
  readonly rating = input(0);
  readonly count = input(0);
  readonly showCount = input(true);

  readonly stars = computed(() => [1, 2, 3, 4, 5]);
  readonly rounded = computed(() => Math.round(this.rating()));
}
