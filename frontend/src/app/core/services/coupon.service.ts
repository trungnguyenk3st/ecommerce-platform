import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Coupon, ValidateCouponResponse } from '../models/api.models';

export interface CouponCreateUpdateRequest {
  code: string;
  discountType: 'Percentage' | 'FixedAmount';
  discountValue: number;
  minOrderAmount: number | null;
  maxDiscountAmount: number | null;
  maxUsageCount: number | null;
  expiresAt: string | null;
  isActive: boolean;
}

@Injectable({ providedIn: 'root' })
export class CouponService {
  private readonly baseUrl = `${environment.apiUrl}/coupons`;
  private readonly adminBaseUrl = `${environment.apiUrl}/admin/coupons`;

  constructor(private readonly http: HttpClient) {}

  validate(code: string, orderSubtotal: number): Observable<ValidateCouponResponse> {
    return this.http.post<ValidateCouponResponse>(`${this.baseUrl}/validate`, { code, orderSubtotal });
  }

  getAll(): Observable<Coupon[]> {
    return this.http.get<Coupon[]>(this.adminBaseUrl);
  }

  create(request: CouponCreateUpdateRequest): Observable<Coupon> {
    return this.http.post<Coupon>(this.adminBaseUrl, request);
  }

  update(id: number, request: CouponCreateUpdateRequest): Observable<Coupon> {
    return this.http.put<Coupon>(`${this.adminBaseUrl}/${id}`, request);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.adminBaseUrl}/${id}`);
  }
}
