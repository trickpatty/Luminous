import { inject } from '@angular/core';
import { Router, CanActivateFn, UrlTree } from '@angular/router';
import { AuthService } from '../services/auth.service';

/**
 * Guard that protects routes requiring authentication.
 * Redirects to login if user is not authenticated.
 */
export const authGuard: CanActivateFn = (): boolean | UrlTree => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (authService.isAuthenticated()) {
    return true;
  }

  // Store attempted URL for redirect after login
  const currentUrl = router.getCurrentNavigation()?.extractedUrl.toString();
  if (currentUrl && currentUrl !== '/auth/login') {
    sessionStorage.setItem('redirectUrl', currentUrl);
  }

  return router.createUrlTree(['/auth/login']);
};

/**
 * Guard that protects guest-only routes (login, register).
 * Redirects to dashboard if user is already authenticated.
 */
export const guestGuard: CanActivateFn = (): boolean | UrlTree => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (!authService.isAuthenticated()) {
    return true;
  }

  return router.createUrlTree(['/dashboard']);
};

/**
 * Guard that requires a specific user role.
 * @param allowedRoles List of roles that can access the route
 */
export const roleGuard = (allowedRoles: string[]): CanActivateFn => {
  return (): boolean | UrlTree => {
    const authService = inject(AuthService);
    const router = inject(Router);

    const user = authService.user();
    if (!user) {
      return router.createUrlTree(['/auth/login']);
    }

    if (allowedRoles.includes(user.role)) {
      return true;
    }

    // User is authenticated but doesn't have required role
    return router.createUrlTree(['/unauthorized']);
  };
};
