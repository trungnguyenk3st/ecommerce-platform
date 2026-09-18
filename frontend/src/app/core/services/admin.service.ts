import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Customer, DashboardSummary } from '../models/api.models';

@Injectable({ providedIn: 'root' })
export class AdminService {
  constructor(private readonly http: HttpClient) {}

  getDashboardSummary(): Observable<DashboardSummary> {
    return this.http.get<DashboardSummary>(`${environment.apiUrl}/admin/dashboard`);
  }

  getCustomers(): Observable<Customer[]> {
    return this.http.get<Customer[]>(`${environment.apiUrl}/admin/customers`);
  }

  setCustomerLock(userId: string, locked: boolean): Observable<Customer> {
    return this.http.put<Customer>(`${environment.apiUrl}/admin/customers/${userId}/lock`, { locked });
  }
}
