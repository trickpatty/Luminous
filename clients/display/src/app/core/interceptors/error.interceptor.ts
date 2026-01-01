import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';
import { DeviceAuthService } from '../services/device-auth.service';

/**
 * Interceptor that handles HTTP errors
 */
export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const router = inject(Router);
  const authService = inject(DeviceAuthService);

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      // Handle 401 Unauthorized - device token invalid
      if (error.status === 401) {
        console.log('Device token invalid, redirecting to linking...');
        authService.unlink();
        return throwError(() => error);
      }

      // Handle 403 Forbidden - device not authorized for this family
      if (error.status === 403) {
        console.log('Device not authorized');
        authService.unlink();
        return throwError(() => error);
      }

      // Log other errors
      console.error('API error:', error.status, error.message);

      return throwError(() => error);
    })
  );
};
