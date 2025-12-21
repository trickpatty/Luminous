import { Routes } from '@angular/router';
import { guestGuard } from '../../core';

export const AUTH_ROUTES: Routes = [
  {
    path: '',
    redirectTo: 'login',
    pathMatch: 'full',
  },
  {
    path: 'login',
    loadComponent: () =>
      import('./components/login/login.component').then((m) => m.LoginComponent),
    canActivate: [guestGuard],
    title: 'Sign In - Luminous',
  },
  {
    path: 'register',
    loadComponent: () =>
      import('./components/register/register.component').then((m) => m.RegisterComponent),
    canActivate: [guestGuard],
    title: 'Create Account - Luminous',
  },
];
