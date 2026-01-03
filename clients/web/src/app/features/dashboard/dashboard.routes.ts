import { Routes } from '@angular/router';
import { authGuard } from '../../core';

export const DASHBOARD_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./components/dashboard-shell/dashboard-shell.component').then(
        (m) => m.DashboardShellComponent
      ),
    canActivate: [authGuard],
    children: [
      {
        path: '',
        loadComponent: () =>
          import('./components/dashboard-home/dashboard-home.component').then(
            (m) => m.DashboardHomeComponent
          ),
        title: 'Dashboard - Luminous',
      },
      {
        path: 'members',
        loadComponent: () =>
          import('./components/members/members.component').then(
            (m) => m.MembersComponent
          ),
        title: 'Family Members - Luminous',
      },
      {
        path: 'calendars',
        loadComponent: () =>
          import('./components/calendars/calendars.component').then(
            (m) => m.CalendarsComponent
          ),
        title: 'Connected Calendars - Luminous',
      },
      {
        path: 'devices',
        loadComponent: () =>
          import('./components/devices/devices.component').then(
            (m) => m.DevicesComponent
          ),
        title: 'Devices - Luminous',
      },
      {
        path: 'settings',
        loadComponent: () =>
          import('./components/settings/settings.component').then(
            (m) => m.SettingsComponent
          ),
        title: 'Settings - Luminous',
      },
      {
        path: 'profile',
        loadComponent: () =>
          import('./components/profile/profile.component').then(
            (m) => m.ProfileComponent
          ),
        title: 'My Profile - Luminous',
      },
      {
        path: 'profile/:id',
        loadComponent: () =>
          import('./components/profile/profile.component').then(
            (m) => m.ProfileComponent
          ),
        title: 'Profile - Luminous',
      },
    ],
  },
];
