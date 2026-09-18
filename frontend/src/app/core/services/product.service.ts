import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { PagedResult, ProductDetail, ProductListItem, ProductQueryParams } from '../models/api.models';

export interface ProductImageInput {
  url: string;
  displayOrder: number;
  isPrimary: boolean;
}

export interface ProductCreateUpdateRequest {
  name: string;
  description: string | null;
  sku: string;
  price: number;
  compareAtPrice: number | null;
  stockQuantity: number;
  isActive: boolean;
  categoryId: number;
  images: ProductImageInput[];
}

@Injectable({ providedIn: 'root' })
export class ProductService {
  private readonly baseUrl = `${environment.apiUrl}/products`;

  constructor(private readonly http: HttpClient) {}

  search(query: ProductQueryParams): Observable<PagedResult<ProductListItem>> {
    let params = new HttpParams();
    Object.entries(query).forEach(([key, value]) => {
      if (value !== undefined && value !== null && value !== '') {
        params = params.set(key, String(value));
      }
    });
    return this.http.get<PagedResult<ProductListItem>>(this.baseUrl, { params });
  }

  getById(id: number): Observable<ProductDetail> {
    return this.http.get<ProductDetail>(`${this.baseUrl}/${id}`);
  }

  getBySlug(slug: string): Observable<ProductDetail> {
    return this.http.get<ProductDetail>(`${this.baseUrl}/slug/${slug}`);
  }

  getRelated(id: number): Observable<ProductListItem[]> {
    return this.http.get<ProductListItem[]>(`${this.baseUrl}/${id}/related`);
  }

  create(request: ProductCreateUpdateRequest): Observable<ProductDetail> {
    return this.http.post<ProductDetail>(this.baseUrl, request);
  }

  update(id: number, request: ProductCreateUpdateRequest): Observable<ProductDetail> {
    return this.http.put<ProductDetail>(`${this.baseUrl}/${id}`, request);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }

  adjustStock(id: number, deltaQuantity: number, reason?: string): Observable<ProductDetail> {
    return this.http.patch<ProductDetail>(`${this.baseUrl}/${id}/stock`, { deltaQuantity, reason });
  }
}
