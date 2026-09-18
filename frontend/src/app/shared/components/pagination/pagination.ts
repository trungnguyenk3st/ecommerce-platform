import { Component, computed, input, output } from '@angular/core';

@Component({
  selector: 'app-pagination',
  standalone: true,
  templateUrl: './pagination.html'
})
export class PaginationComponent {
  readonly page = input.required<number>();
  readonly totalPages = input.required<number>();
  readonly pageChange = output<number>();

  readonly pages = computed(() => {
    const total = this.totalPages();
    const current = this.page();
    const windowSize = 2;
    const start = Math.max(1, current - windowSize);
    const end = Math.min(total, current + windowSize);
    return Array.from({ length: end - start + 1 }, (_, i) => start + i);
  });

  go(page: number): void {
    if (page < 1 || page > this.totalPages() || page === this.page()) return;
    this.pageChange.emit(page);
  }
}
