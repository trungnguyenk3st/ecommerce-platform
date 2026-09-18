import { HttpClient } from '@angular/common/http';
import { Injectable, computed, signal } from '@angular/core';
import { Observable, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AuthResponse, UserProfile } from '../models/api.models';

const ACCESS_TOKEN_KEY = 'ecom_access_token';
const REFRESH_TOKEN_KEY = 'ecom_refresh_token';
const USER_KEY = 'ecom_user';

interface StoredUser {
  id: string;
  email: string;
  fullName: string;
  roles: string[];
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly baseUrl = `${environment.apiUrl}/auth`;

  private readonly currentUserSignal = signal<StoredUser | null>(this.readStoredUser());

  readonly currentUser = this.currentUserSignal.asReadonly();
  readonly isAuthenticated = computed(() => this.currentUserSignal() !== null);
  readonly isAdmin = computed(() => this.currentUserSignal()?.roles.includes('Admin') ?? false);

  constructor(private readonly http: HttpClient) {}

  get accessToken(): string | null {
    return localStorage.getItem(ACCESS_TOKEN_KEY);
  }

  get refreshToken(): string | null {
    return localStorage.getItem(REFRESH_TOKEN_KEY);
  }

  register(email: string, password: string, fullName: string): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.baseUrl}/register`, { email, password, fullName })
      .pipe(tap(response => this.storeSession(response)));
  }

  login(email: string, password: string): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.baseUrl}/login`, { email, password })
      .pipe(tap(response => this.storeSession(response)));
  }

  refresh(): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.baseUrl}/refresh`, { refreshToken: this.refreshToken })
      .pipe(tap(response => this.storeSession(response)));
  }

  updateProfile(fullName: string): Observable<UserProfile> {
    return this.http.put<UserProfile>(`${this.baseUrl}/me`, { fullName })
      .pipe(tap(profile => this.patchStoredUser({ fullName: profile.fullName })));
  }

  changePassword(currentPassword: string, newPassword: string): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/change-password`, { currentPassword, newPassword });
  }

  logout(): void {
    const refreshToken = this.refreshToken;
    if (refreshToken) {
      this.http.post(`${this.baseUrl}/revoke`, { refreshToken }).subscribe({ error: () => void 0 });
    }
    localStorage.removeItem(ACCESS_TOKEN_KEY);
    localStorage.removeItem(REFRESH_TOKEN_KEY);
    localStorage.removeItem(USER_KEY);
    this.currentUserSignal.set(null);
  }

  private storeSession(response: AuthResponse): void {
    localStorage.setItem(ACCESS_TOKEN_KEY, response.accessToken);
    localStorage.setItem(REFRESH_TOKEN_KEY, response.refreshToken);
    const user: StoredUser = { id: response.userId, email: response.email, fullName: response.fullName, roles: response.roles };
    localStorage.setItem(USER_KEY, JSON.stringify(user));
    this.currentUserSignal.set(user);
  }

  private patchStoredUser(patch: Partial<StoredUser>): void {
    const current = this.currentUserSignal();
    if (!current) return;
    const updated = { ...current, ...patch };
    localStorage.setItem(USER_KEY, JSON.stringify(updated));
    this.currentUserSignal.set(updated);
  }

  private readStoredUser(): StoredUser | null {
    try {
      const raw = localStorage.getItem(USER_KEY);
      return raw ? (JSON.parse(raw) as StoredUser) : null;
    } catch {
      return null;
    }
  }
}
