import { Routes } from '@angular/router';
import { deviceAuthGuard } from './core/guards/device-auth.guard';

export const routes: Routes = [
  {
    path: '',
    redirectTo: 'display',
    pathMatch: 'full',
  },
  {
    path: 'linking',
    loadComponent: () =>
      import('./features/linking/linking.component').then((m) => m.LinkingComponent),
  },
  {
    path: 'display',
    canActivate: [deviceAuthGuard],
    loadComponent: () =>
      import('./features/display/display.component').then((m) => m.DisplayComponent),
  },
  {
    path: 'settings',
    canActivate: [deviceAuthGuard],
    loadComponent: () =>
      import('./features/settings/settings.component').then((m) => m.SettingsComponent),
  },
  {
    path: '**',
    redirectTo: 'display',
  },
];
