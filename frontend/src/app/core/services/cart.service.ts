import { HttpClient } from '@angular/common/http';
import { Injectable, computed, signal } from '@angular/core';
import { Observable, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Cart } from '../models/api.models';

const EMPTY_CART: Cart = { id: 0, items: [], subtotal: 0, totalItems: 0 };

@Injectable({ providedIn: 'root' })
export class CartService {
  private readonly baseUrl = `${environment.apiUrl}/cart`;

  private readonly cartSignal = signal<Cart>(EMPTY_CART);
  readonly cart = this.cartSignal.asReadonly();
  readonly itemCount = computed(() => this.cartSignal().totalItems);

  constructor(private readonly http: HttpClient) {}

  refresh(): Observable<Cart> {
    return this.http.get<Cart>(this.baseUrl).pipe(tap(cart => this.cartSignal.set(cart)));
  }

  addItem(productId: number, quantity = 1): Observable<Cart> {
    return this.http.post<Cart>(`${this.baseUrl}/items`, { productId, quantity })
      .pipe(tap(cart => this.cartSignal.set(cart)));
  }

  updateItem(cartItemId: number, quantity: number): Observable<Cart> {
    return this.http.put<Cart>(`${this.baseUrl}/items/${cartItemId}`, { quantity })
      .pipe(tap(cart => this.cartSignal.set(cart)));
  }

  removeItem(cartItemId: number): Observable<Cart> {
    return this.http.delete<Cart>(`${this.baseUrl}/items/${cartItemId}`)
      .pipe(tap(cart => this.cartSignal.set(cart)));
  }

  clear(): Observable<Cart> {
    return this.http.delete<Cart>(this.baseUrl).pipe(tap(cart => this.cartSignal.set(cart)));
  }

  reset(): void {
    this.cartSignal.set(EMPTY_CART);
  }
}
