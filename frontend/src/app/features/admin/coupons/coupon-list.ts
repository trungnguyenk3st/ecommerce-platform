import { DatePipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { CouponService } from '../../../core/services/coupon.service';
import { ToastService } from '../../../core/services/toast.service';
import { Coupon } from '../../../core/models/api.models';

@Component({
  selector: 'app-admin-coupon-list',
  standalone: true,
  imports: [ReactiveFormsModule, DatePipe],
  templateUrl: './coupon-list.html'
})
export class AdminCouponListComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly couponService = inject(CouponService);
  private readonly toast = inject(ToastService);

  readonly coupons = signal<Coupon[]>([]);
  readonly showForm = signal(false);
  readonly editingId = signal<number | null>(null);
  readonly saving = signal(false);

  readonly form = this.fb.nonNullable.group({
    code: ['', Validators.required],
    discountType: this.fb.nonNullable.control<'Percentage' | 'FixedAmount'>('Percentage'),
    discountValue: [10, [Validators.required, Validators.min(0.01)]],
    minOrderAmount: this.fb.control<number | null>(null),
    maxDiscountAmount: this.fb.control<number | null>(null),
    maxUsageCount: this.fb.control<number | null>(null),
    expiresAt: this.fb.control<string | null>(null),
    isActive: [true]
  });

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.couponService.getAll().subscribe(coupons => this.coupons.set(coupons));
  }

  startCreate(): void {
    this.editingId.set(null);
    this.form.reset({ discountType: 'Percentage', discountValue: 10, isActive: true });
    this.showForm.set(true);
  }

  startEdit(coupon: Coupon): void {
    this.editingId.set(coupon.id);
    this.form.reset({ ...coupon, expiresAt: coupon.expiresAt ? coupon.expiresAt.substring(0, 10) : null });
    this.showForm.set(true);
  }

  cancel(): void {
    this.showForm.set(false);
  }

  save(): void {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }

    this.saving.set(true);
    const value = this.form.getRawValue();
    const request = { ...value, expiresAt: value.expiresAt ? new Date(value.expiresAt).toISOString() : null };
    const editingId = this.editingId();
    const request$ = editingId ? this.couponService.update(editingId, request) : this.couponService.create(request);

    request$.subscribe({
      next: () => {
        this.saving.set(false);
        this.showForm.set(false);
        this.toast.success(editingId ? 'Coupon updated.' : 'Coupon created.');
        this.load();
      },
      error: () => this.saving.set(false)
    });
  }

  delete(id: number): void {
    if (!confirm('Delete this coupon?')) return;
    this.couponService.delete(id).subscribe(() => { this.toast.show('Coupon deleted.'); this.load(); });
  }

  formatVnd(value: number): string {
    return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(value);
  }
}
