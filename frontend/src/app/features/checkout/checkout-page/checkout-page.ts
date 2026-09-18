import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormBuilder, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { CartService } from '../../../core/services/cart.service';
import { AddressService } from '../../../core/services/address.service';
import { OrderService } from '../../../core/services/order.service';
import { CouponService } from '../../../core/services/coupon.service';
import { ToastService } from '../../../core/services/toast.service';
import { Address, PaymentMethod } from '../../../core/models/api.models';

@Component({
  selector: 'app-checkout-page',
  standalone: true,
  imports: [ReactiveFormsModule, FormsModule],
  templateUrl: './checkout-page.html'
})
export class CheckoutPageComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  readonly cart = inject(CartService);
  private readonly addressService = inject(AddressService);
  private readonly orderService = inject(OrderService);
  private readonly couponService = inject(CouponService);
  private readonly toast = inject(ToastService);
  private readonly router = inject(Router);

  readonly addresses = signal<Address[]>([]);
  readonly showAddressForm = signal(false);
  readonly selectedAddressId = signal<number | null>(null);
  readonly paymentMethod = signal<PaymentMethod>('CashOnDelivery');
  readonly placing = signal(false);
  readonly savingAddress = signal(false);

  readonly couponCode = signal('');
  readonly couponDiscount = signal(0);
  readonly couponMessage = signal<string | null>(null);
  readonly couponApplied = signal(false);
  readonly validatingCoupon = signal(false);

  customerNote = '';

  readonly shippingFee = computed(() => (this.cart.cart().subtotal >= 1_000_000 ? 0 : 30_000));
  readonly total = computed(() => Math.max(0, this.cart.cart().subtotal - this.couponDiscount() + this.shippingFee()));

  readonly addressForm = this.fb.nonNullable.group({
    fullName: ['', Validators.required],
    phone: ['', Validators.required],
    line1: ['', Validators.required],
    line2: [''],
    city: ['', Validators.required],
    province: ['', Validators.required],
    postalCode: [''],
    country: ['Vietnam', Validators.required],
    isDefault: [true]
  });

  ngOnInit(): void {
    this.cart.refresh().subscribe(cart => {
      if (cart.items.length === 0) this.router.navigate(['/cart']);
    });
    this.loadAddresses();
  }

  private loadAddresses(): void {
    this.addressService.getAll().subscribe(addresses => {
      this.addresses.set(addresses);
      const preferred = addresses.find(a => a.isDefault) ?? addresses[0];
      if (preferred) this.selectedAddressId.set(preferred.id);
      if (addresses.length === 0) this.showAddressForm.set(true);
    });
  }

  saveNewAddress(): void {
    if (this.addressForm.invalid) { this.addressForm.markAllAsTouched(); return; }

    this.savingAddress.set(true);
    this.addressService.create(this.addressForm.getRawValue()).subscribe({
      next: address => {
        this.savingAddress.set(false);
        this.showAddressForm.set(false);
        this.addresses.update(list => [address, ...list]);
        this.selectedAddressId.set(address.id);
      },
      error: () => this.savingAddress.set(false)
    });
  }

  applyCoupon(): void {
    const code = this.couponCode().trim();
    if (!code) return;

    this.validatingCoupon.set(true);
    this.couponService.validate(code, this.cart.cart().subtotal).subscribe({
      next: result => {
        this.validatingCoupon.set(false);
        this.couponMessage.set(result.message);
        if (result.isValid) {
          this.couponDiscount.set(result.discountAmount);
          this.couponApplied.set(true);
          this.toast.success('Coupon applied!');
        } else {
          this.couponDiscount.set(0);
          this.couponApplied.set(false);
        }
      },
      error: () => this.validatingCoupon.set(false)
    });
  }

  removeCoupon(): void {
    this.couponCode.set('');
    this.couponDiscount.set(0);
    this.couponApplied.set(false);
    this.couponMessage.set(null);
  }

  placeOrder(): void {
    const addressId = this.selectedAddressId();
    if (!addressId) { this.toast.error('Please select or add a shipping address.'); return; }

    this.placing.set(true);
    this.orderService.placeOrder({
      addressId,
      paymentMethod: this.paymentMethod(),
      couponCode: this.couponApplied() ? this.couponCode().trim() : null,
      customerNote: this.customerNote || null
    }).subscribe({
      next: result => {
        this.cart.refresh().subscribe();
        if (result.paymentRedirectUrl) {
          window.location.href = result.paymentRedirectUrl;
          return;
        }
        this.router.navigate(['/checkout/payment-result'], { queryParams: { orderId: result.order.id } });
      },
      error: () => this.placing.set(false)
    });
  }

  formatVnd(value: number): string {
    return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(value);
  }
}
