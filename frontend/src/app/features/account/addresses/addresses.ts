import { Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { AddressService } from '../../../core/services/address.service';
import { ToastService } from '../../../core/services/toast.service';
import { Address } from '../../../core/models/api.models';

@Component({
  selector: 'app-addresses',
  standalone: true,
  imports: [ReactiveFormsModule],
  templateUrl: './addresses.html'
})
export class AddressesComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly addressService = inject(AddressService);
  private readonly toast = inject(ToastService);

  readonly addresses = signal<Address[]>([]);
  readonly showForm = signal(false);
  readonly editingId = signal<number | null>(null);
  readonly saving = signal(false);

  readonly form = this.fb.nonNullable.group({
    fullName: ['', Validators.required],
    phone: ['', Validators.required],
    line1: ['', Validators.required],
    line2: [''],
    city: ['', Validators.required],
    province: ['', Validators.required],
    postalCode: [''],
    country: ['Vietnam', Validators.required],
    isDefault: [false]
  });

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.addressService.getAll().subscribe(addresses => this.addresses.set(addresses));
  }

  startCreate(): void {
    this.editingId.set(null);
    this.form.reset({ country: 'Vietnam', isDefault: false });
    this.showForm.set(true);
  }

  startEdit(address: Address): void {
    this.editingId.set(address.id);
    this.form.reset({ ...address, line2: address.line2 ?? '', postalCode: address.postalCode ?? '' });
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
    const request$ = editingId ? this.addressService.update(editingId, value) : this.addressService.create(value);

    request$.subscribe({
      next: () => {
        this.saving.set(false);
        this.showForm.set(false);
        this.toast.success(editingId ? 'Address updated.' : 'Address added.');
        this.load();
      },
      error: () => this.saving.set(false)
    });
  }

  remove(id: number): void {
    if (!confirm('Delete this address?')) return;
    this.addressService.delete(id).subscribe(() => { this.toast.show('Address deleted.'); this.load(); });
  }
}
