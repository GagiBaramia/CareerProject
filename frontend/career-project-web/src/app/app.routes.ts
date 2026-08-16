import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'login' },
  {
    path: 'login',
    loadComponent: () => import('./features/auth/login/login.component').then((m) => m.LoginComponent)
  },
  {
    path: 'register',
    loadComponent: () =>
      import('./features/auth/register/register.component').then((m) => m.RegisterComponent)
  },
  {
    path: 'dashboard/person',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./features/dashboard/person/person-dashboard.component').then(
        (m) => m.PersonDashboardComponent
      )
  },
  {
    path: 'dashboard/company',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./features/dashboard/company/company-dashboard.component').then(
        (m) => m.CompanyDashboardComponent
      )
  },
  { path: '**', redirectTo: 'login' }
];
