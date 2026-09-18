import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { BehaviorSubject, catchError, filter, switchMap, take, throwError } from 'rxjs';
import { AuthService } from '../services/auth.service';
import { ToastService } from '../services/toast.service';
import { ApiProblem } from '../models/api.models';

let isRefreshing = false;
const refreshedToken$ = new BehaviorSubject<string | null>(null);

function isAuthEndpoint(url: string): boolean {
  return url.includes('/auth/login') || url.includes('/auth/register') || url.includes('/auth/refresh');
}

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);
  const toast = inject(ToastService);
  const router = inject(Router);

  return next(req).pipe(
    catchError((error: unknown) => {
      if (!(error instanceof HttpErrorResponse)) return throwError(() => error);

      if (error.status === 401 && !isAuthEndpoint(req.url) && authService.refreshToken) {
        if (!isRefreshing) {
          isRefreshing = true;
          refreshedToken$.next(null);

          return authService.refresh().pipe(
            switchMap(response => {
              isRefreshing = false;
              refreshedToken$.next(response.accessToken);
              return next(req.clone({ setHeaders: { Authorization: `Bearer ${response.accessToken}` } }));
            }),
            catchError(refreshError => {
              isRefreshing = false;
              authService.logout();
              router.navigate(['/login']);
              return throwError(() => refreshError);
            })
          );
        }

        return refreshedToken$.pipe(
          filter(token => token !== null),
          take(1),
          switchMap(token => next(req.clone({ setHeaders: { Authorization: `Bearer ${token}` } })))
        );
      }

      const problem = error.error as ApiProblem | undefined;
      const message = flattenErrorMessage(problem) ?? defaultMessageFor(error.status);

      if (error.status !== 401) toast.error(message);
      return throwError(() => error);
    })
  );
};

function flattenErrorMessage(problem: ApiProblem | undefined): string | null {
  if (!problem) return null;
  if (problem.errors) {
    const messages = Object.values(problem.errors).flat();
    if (messages.length > 0) return messages.join(' ');
  }
  return problem.title ?? null;
}

function defaultMessageFor(status: number): string {
  switch (status) {
    case 0: return 'Cannot reach the server. Please check your connection.';
    case 403: return 'You do not have permission to do that.';
    case 404: return 'The requested resource was not found.';
    default: return 'Something went wrong. Please try again.';
  }
}
