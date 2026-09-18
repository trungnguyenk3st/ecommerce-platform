import { Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { ProductService } from '../../../core/services/product.service';
import { CategoryService } from '../../../core/services/category.service';
import { ToastService } from '../../../core/services/toast.service';
import { Category } from '../../../core/models/api.models';

@Component({
  selector: 'app-admin-product-form',
  standalone: true,
  imports: [ReactiveFormsModule, FormsModule, RouterLink],
  templateUrl: './product-form.html'
})
export class AdminProductFormComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly productService = inject(ProductService);
  private readonly categoryService = inject(CategoryService);
  private readonly toast = inject(ToastService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  readonly categories = signal<Category[]>([]);
  readonly productId = signal<number | null>(null);
  readonly saving = signal(false);

  readonly form = this.fb.nonNullable.group({
    name: ['', [Validators.required, Validators.maxLength(200)]],
    description: [''],
    sku: ['', [Validators.required, Validators.maxLength(64)]],
    price: [0, [Validators.required, Validators.min(0.01)]],
    compareAtPrice: this.fb.control<number | null>(null),
    stockQuantity: [0, [Validators.required, Validators.min(0)]],
    isActive: [true],
    categoryId: [0, [Validators.required, Validators.min(1)]],
    images: this.fb.nonNullable.array<string>([])
  });

  get imageControls() {
    return this.form.controls.images;
  }

  ngOnInit(): void {
    this.categoryService.getFlat().subscribe(categories => this.categories.set(categories));

    const idParam = this.route.snapshot.paramMap.get('id');
    if (idParam) {
      const id = Number(idParam);
      this.productId.set(id);
      this.productService.getById(id).subscribe(product => {
        this.form.patchValue({
          name: product.name,
          description: product.description ?? '',
          sku: product.sku,
          price: product.price,
          compareAtPrice: product.compareAtPrice,
          stockQuantity: product.stockQuantity,
          isActive: product.isActive,
          categoryId: product.categoryId
        });
        product.images.forEach(image => this.imageControls.push(this.fb.nonNullable.control(image.url, Validators.required)));
      });
    } else {
      this.addImageField();
    }
  }

  addImageField(): void {
    this.imageControls.push(this.fb.nonNullable.control('', Validators.required));
  }

  removeImageField(index: number): void {
    this.imageControls.removeAt(index);
  }

  save(): void {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }

    this.saving.set(true);
    const value = this.form.getRawValue();
    const request = {
      ...value,
      description: value.description || null,
      images: value.images
        .filter(url => url.trim().length > 0)
        .map((url, index) => ({ url, displayOrder: index, isPrimary: index === 0 }))
    };

    const id = this.productId();
    const request$ = id ? this.productService.update(id, request) : this.productService.create(request);

    request$.subscribe({
      next: () => {
        this.saving.set(false);
        this.toast.success(id ? 'Product updated.' : 'Product created.');
        this.router.navigate(['/admin/products']);
      },
      error: () => this.saving.set(false)
    });
  }
}
