import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { DeviceAuthService } from '../services/device-auth.service';

/**
 * Interceptor that adds device token to API requests.
 * Uses standard Authorization Bearer header for JWT token authentication.
 */
export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(DeviceAuthService);
  const token = authService.getToken();

  if (token) {
    const authReq = req.clone({
      setHeaders: {
        'Authorization': `Bearer ${token}`,
      },
    });
    return next(authReq);
  }

  return next(req);
};
