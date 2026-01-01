import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { DeviceAuthService } from '../services/device-auth.service';

/**
 * Interceptor that adds device token to API requests
 */
export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(DeviceAuthService);
  const token = authService.getToken();

  if (token) {
    const authReq = req.clone({
      setHeaders: {
        'X-Device-Token': token,
      },
    });
    return next(authReq);
  }

  return next(req);
};
