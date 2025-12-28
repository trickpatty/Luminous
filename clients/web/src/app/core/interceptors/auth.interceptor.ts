import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, switchMap, throwError } from 'rxjs';

import { AuthService } from '../services/auth.service';

/**
 * Auth endpoints that should NOT receive Authorization headers.
 * These are anonymous endpoints that don't require authentication.
 */
const ANONYMOUS_AUTH_ENDPOINTS = [
  '/auth/register',
  '/auth/check-email',
  '/auth/otp/request',
  '/auth/otp/verify',
  '/auth/passkey/authenticate/start',
  '/auth/passkey/authenticate/complete',
  '/auth/refresh',
];

/**
 * Check if the URL is an anonymous auth endpoint that should not receive auth headers.
 */
function isAnonymousAuthEndpoint(url: string): boolean {
  return ANONYMOUS_AUTH_ENDPOINTS.some((endpoint) => url.includes(endpoint));
}

/**
 * HTTP interceptor that adds authentication token to requests
 * and handles token refresh on 401 errors.
 */
export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);
  const token = authService.getAccessToken();

  // Skip auth header only for specific anonymous auth endpoints
  const skipAuth = isAnonymousAuthEndpoint(req.url);

  if (token && !skipAuth) {
    req = req.clone({
      setHeaders: {
        Authorization: `Bearer ${token}`,
      },
    });
  }

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error.status === 401 && !skipAuth) {
        // Token expired, try to refresh
        return authService.refreshToken().pipe(
          switchMap(() => {
            const newToken = authService.getAccessToken();
            const clonedReq = req.clone({
              setHeaders: {
                Authorization: `Bearer ${newToken}`,
              },
            });
            return next(clonedReq);
          }),
          catchError(() => {
            authService.logout();
            return throwError(() => error);
          })
        );
      }
      return throwError(() => error);
    })
  );
};
