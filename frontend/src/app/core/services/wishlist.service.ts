import { HttpClient } from '@angular/common/http';
import { Injectable, computed, signal } from '@angular/core';
import { Observable, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { WishlistItem } from '../models/api.models';

@Injectable({ providedIn: 'root' })
export class WishlistService {
  private readonly baseUrl = `${environment.apiUrl}/wishlist`;

  private readonly itemsSignal = signal<WishlistItem[]>([]);
  readonly items = this.itemsSignal.asReadonly();
  readonly productIds = computed(() => new Set(this.itemsSignal().map(i => i.productId)));

  constructor(private readonly http: HttpClient) {}

  refresh(): Observable<WishlistItem[]> {
    return this.http.get<WishlistItem[]>(this.baseUrl).pipe(tap(items => this.itemsSignal.set(items)));
  }

  add(productId: number): Observable<WishlistItem[]> {
    return this.http.post<WishlistItem[]>(`${this.baseUrl}/${productId}`, {}).pipe(tap(items => this.itemsSignal.set(items)));
  }

  remove(productId: number): Observable<WishlistItem[]> {
    return this.http.delete<WishlistItem[]>(`${this.baseUrl}/${productId}`).pipe(tap(items => this.itemsSignal.set(items)));
  }

  reset(): void {
    this.itemsSignal.set([]);
  }
}
