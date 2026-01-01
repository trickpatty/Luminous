import { Routes } from '@angular/router';
import { guestGuard } from '../../core';

export const AUTH_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./components/auth-layout/auth-layout.component').then(
        (m) => m.AuthLayoutComponent
      ),
    canActivate: [guestGuard],
    children: [
      {
        path: '',
        redirectTo: 'login',
        pathMatch: 'full',
      },
      {
        path: 'login',
        loadComponent: () =>
          import('./components/login/login.component').then((m) => m.LoginComponent),
        title: 'Sign In - Luminous',
      },
      {
        path: 'register',
        loadComponent: () =>
          import('./components/register/register.component').then(
            (m) => m.RegisterComponent
          ),
        title: 'Create Account - Luminous',
      },
      {
        path: 'join/:code',
        loadComponent: () =>
          import('./components/join/join.component').then((m) => m.JoinComponent),
        title: 'Join Family - Luminous',
      },
    ],
  },
];
