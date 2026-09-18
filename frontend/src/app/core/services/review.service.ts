import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Review } from '../models/api.models';

@Injectable({ providedIn: 'root' })
export class ReviewService {
  constructor(private readonly http: HttpClient) {}

  getForProduct(productId: number): Observable<Review[]> {
    return this.http.get<Review[]>(`${environment.apiUrl}/products/${productId}/reviews`);
  }

  create(productId: number, rating: number, comment: string | null): Observable<Review> {
    return this.http.post<Review>(`${environment.apiUrl}/reviews`, { productId, rating, comment });
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${environment.apiUrl}/reviews/${id}`);
  }
}
