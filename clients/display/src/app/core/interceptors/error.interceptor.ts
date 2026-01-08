import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';
import { DeviceAuthService } from '../services/device-auth.service';

/**
 * URL patterns that indicate device authentication endpoints.
 * Only unlink the device when auth errors occur on these endpoints.
 */
const DEVICE_AUTH_PATTERNS = [
  '/devices/',
  '/device-link',
  '/device/validate',
  '/device/heartbeat',
];

/**
 * Check if the request URL is a device authentication endpoint.
 * We only want to unlink when device-specific auth fails, not when
 * other APIs (like calendar) return 401/403 (e.g., OAuth token expired).
 */
function isDeviceAuthEndpoint(url: string): boolean {
  return DEVICE_AUTH_PATTERNS.some(pattern => url.includes(pattern));
}

/**
 * Check if the error response indicates a device token is invalid.
 * This allows the backend to explicitly signal device auth failure
 * via a specific error code, even on non-device endpoints.
 */
function isDeviceTokenError(error: HttpErrorResponse): boolean {
  const errorCode = error.error?.code || error.error?.errorCode;
  return errorCode === 'DEVICE_TOKEN_INVALID' ||
         errorCode === 'DEVICE_NOT_LINKED' ||
         errorCode === 'DEVICE_UNAUTHORIZED';
}

/**
 * Interceptor that handles HTTP errors.
 *
 * IMPORTANT: Only unlinks the device when:
 * 1. The error comes from a device authentication endpoint, OR
 * 2. The backend explicitly indicates a device token error
 *
 * This prevents calendar API errors (e.g., OAuth token expired)
 * from incorrectly unlinking the device.
 */
export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const router = inject(Router);
  const authService = inject(DeviceAuthService);

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      const shouldUnlink = isDeviceAuthEndpoint(req.url) || isDeviceTokenError(error);

      // Handle 401 Unauthorized
      if (error.status === 401) {
        if (shouldUnlink) {
          console.log('Device token invalid, redirecting to linking...');
          authService.unlink();
        } else {
          // API-specific auth error (e.g., calendar OAuth expired)
          // Log but don't unlink - let the component handle it
          console.warn('API authorization error (not device auth):', req.url);
        }
        return throwError(() => error);
      }

      // Handle 403 Forbidden
      if (error.status === 403) {
        if (shouldUnlink) {
          console.log('Device not authorized for this family');
          authService.unlink();
        } else {
          // API-specific forbidden error
          console.warn('API forbidden error (not device auth):', req.url);
        }
        return throwError(() => error);
      }

      // Log other errors
      console.error('API error:', error.status, error.message);

      return throwError(() => error);
    })
  );
};
