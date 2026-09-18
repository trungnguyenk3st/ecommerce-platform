import { Component, OnInit, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { AdminService } from '../../../core/services/admin.service';
import { DashboardSummary } from '../../../core/models/api.models';

@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './dashboard.html'
})
export class AdminDashboardComponent implements OnInit {
  readonly summary = signal<DashboardSummary | null>(null);

  constructor(private readonly adminService: AdminService) {}

  ngOnInit(): void {
    this.adminService.getDashboardSummary().subscribe(summary => this.summary.set(summary));
  }

  formatVnd(value: number): string {
    return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(value);
  }
}
