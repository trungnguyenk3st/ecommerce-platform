import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Order, OrderStatus, OrderSummary, PagedResult, PlaceOrderRequest, PlaceOrderResult } from '../models/api.models';

export interface OrderQueryParams {
  status?: OrderStatus;
  page?: number;
  pageSize?: number;
}

@Injectable({ providedIn: 'root' })
export class OrderService {
  private readonly baseUrl = `${environment.apiUrl}/orders`;
  private readonly adminBaseUrl = `${environment.apiUrl}/admin/orders`;

  constructor(private readonly http: HttpClient) {}

  placeOrder(request: PlaceOrderRequest): Observable<PlaceOrderResult> {
    return this.http.post<PlaceOrderResult>(this.baseUrl, request);
  }

  getMyOrders(query: OrderQueryParams): Observable<PagedResult<OrderSummary>> {
    return this.http.get<PagedResult<OrderSummary>>(this.baseUrl, { params: toHttpParams(query) });
  }

  getMyOrder(id: number): Observable<Order> {
    return this.http.get<Order>(`${this.baseUrl}/${id}`);
  }

  cancelMyOrder(id: number): Observable<Order> {
    return this.http.post<Order>(`${this.baseUrl}/${id}/cancel`, {});
  }

  // Admin
  getAllOrders(query: OrderQueryParams): Observable<PagedResult<OrderSummary>> {
    return this.http.get<PagedResult<OrderSummary>>(this.adminBaseUrl, { params: toHttpParams(query) });
  }

  getOrder(id: number): Observable<Order> {
    return this.http.get<Order>(`${this.adminBaseUrl}/${id}`);
  }

  updateStatus(id: number, status: OrderStatus): Observable<Order> {
    return this.http.put<Order>(`${this.adminBaseUrl}/${id}/status`, { status });
  }
}

function toHttpParams(query: object): HttpParams {
  let params = new HttpParams();
  Object.entries(query).forEach(([key, value]) => {
    if (value !== undefined && value !== null && value !== '') params = params.set(key, String(value));
  });
  return params;
}
