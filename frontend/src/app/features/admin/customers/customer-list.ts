import { Component, OnInit, signal } from '@angular/core';
import { AdminService } from '../../../core/services/admin.service';
import { ToastService } from '../../../core/services/toast.service';
import { Customer } from '../../../core/models/api.models';

@Component({
  selector: 'app-admin-customer-list',
  standalone: true,
  templateUrl: './customer-list.html'
})
export class AdminCustomerListComponent implements OnInit {
  readonly customers = signal<Customer[]>([]);

  constructor(private readonly adminService: AdminService, private readonly toast: ToastService) {}

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.adminService.getCustomers().subscribe(customers => this.customers.set(customers));
  }

  toggleLock(customer: Customer): void {
    this.adminService.setCustomerLock(customer.id, !customer.isLocked).subscribe(updated => {
      this.customers.update(list => list.map(c => (c.id === updated.id ? updated : c)));
      this.toast.show(updated.isLocked ? 'Customer locked.' : 'Customer unlocked.');
    });
  }
}
