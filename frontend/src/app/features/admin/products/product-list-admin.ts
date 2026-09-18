import { Component, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { ProductService } from '../../../core/services/product.service';
import { ToastService } from '../../../core/services/toast.service';
import { PagedResult, ProductListItem } from '../../../core/models/api.models';
import { PaginationComponent } from '../../../shared/components/pagination/pagination';

@Component({
  selector: 'app-admin-product-list',
  standalone: true,
  imports: [RouterLink, FormsModule, PaginationComponent],
  templateUrl: './product-list-admin.html'
})
export class AdminProductListComponent implements OnInit {
  readonly result = signal<PagedResult<ProductListItem> | null>(null);
  search = '';
  private page = 1;

  constructor(private readonly productService: ProductService, private readonly toast: ToastService) {}

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.productService.search({ search: this.search || undefined, page: this.page, pageSize: 15, includeInactive: true })
      .subscribe(result => this.result.set(result));
  }

  onPageChange(page: number): void {
    this.page = page;
    this.load();
  }

  delete(id: number): void {
    if (!confirm('Delete this product? Products with existing orders will be deactivated instead.')) return;
    this.productService.delete(id).subscribe(() => { this.toast.show('Product removed.'); this.load(); });
  }

  formatVnd(value: number): string {
    return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(value);
  }
}
