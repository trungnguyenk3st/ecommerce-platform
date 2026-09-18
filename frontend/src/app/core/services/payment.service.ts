import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Order } from '../models/api.models';

@Injectable({ providedIn: 'root' })
export class PaymentService {
  constructor(private readonly http: HttpClient) {}

  /** Forwards the vnp_* query params VNPay attached to the browser return URL for signature validation. */
  confirmVnPayReturn(queryParams: Record<string, string>): Observable<Order> {
    const search = new URLSearchParams(queryParams).toString();
    return this.http.get<Order>(`${environment.apiUrl}/payments/vnpay/return?${search}`);
  }
}
