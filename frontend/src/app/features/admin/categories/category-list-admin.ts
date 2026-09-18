import { Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { CategoryService } from '../../../core/services/category.service';
import { ToastService } from '../../../core/services/toast.service';
import { Category } from '../../../core/models/api.models';

@Component({
  selector: 'app-admin-category-list',
  standalone: true,
  imports: [ReactiveFormsModule],
  templateUrl: './category-list-admin.html'
})
export class AdminCategoryListComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly categoryService = inject(CategoryService);
  private readonly toast = inject(ToastService);

  readonly categories = signal<Category[]>([]);
  readonly showForm = signal(false);
  readonly editingId = signal<number | null>(null);
  readonly saving = signal(false);

  readonly form = this.fb.nonNullable.group({
    name: ['', Validators.required],
    description: [''],
    imageUrl: [''],
    displayOrder: [0],
    isActive: [true],
    parentCategoryId: this.fb.control<number | null>(null)
  });

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.categoryService.getFlat().subscribe(categories => this.categories.set(categories));
  }

  startCreate(): void {
    this.editingId.set(null);
    this.form.reset({ displayOrder: 0, isActive: true, parentCategoryId: null });
    this.showForm.set(true);
  }

  startEdit(category: Category): void {
    this.editingId.set(category.id);
    this.form.reset({ ...category, description: category.description ?? '', imageUrl: category.imageUrl ?? '' });
    this.showForm.set(true);
  }

  cancel(): void {
    this.showForm.set(false);
  }

  save(): void {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }

    this.saving.set(true);
    const value = this.form.getRawValue();
    const editingId = this.editingId();
    const request$ = editingId ? this.categoryService.update(editingId, value) : this.categoryService.create(value);

    request$.subscribe({
      next: () => {
        this.saving.set(false);
        this.showForm.set(false);
        this.toast.success(editingId ? 'Category updated.' : 'Category created.');
        this.load();
      },
      error: () => this.saving.set(false)
    });
  }

  delete(id: number): void {
    if (!confirm('Delete this category?')) return;
    this.categoryService.delete(id).subscribe({
      next: () => { this.toast.show('Category deleted.'); this.load(); }
    });
  }
}
