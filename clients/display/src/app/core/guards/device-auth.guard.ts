import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { DeviceAuthService } from '../services/device-auth.service';

/**
 * Guard that ensures device is authenticated before allowing access.
 * Redirects to linking page if not authenticated.
 */
export const deviceAuthGuard: CanActivateFn = async () => {
  const authService = inject(DeviceAuthService);
  const router = inject(Router);

  const isAuthenticated = await authService.checkAuth();

  if (!isAuthenticated) {
    router.navigate(['/linking']);
    return false;
  }

  return true;
};
