import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Address, AddressInput } from '../models/api.models';

@Injectable({ providedIn: 'root' })
export class AddressService {
  private readonly baseUrl = `${environment.apiUrl}/addresses`;

  constructor(private readonly http: HttpClient) {}

  getAll(): Observable<Address[]> {
    return this.http.get<Address[]>(this.baseUrl);
  }

  create(request: AddressInput): Observable<Address> {
    return this.http.post<Address>(this.baseUrl, request);
  }

  update(id: number, request: AddressInput): Observable<Address> {
    return this.http.put<Address>(`${this.baseUrl}/${id}`, request);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}
