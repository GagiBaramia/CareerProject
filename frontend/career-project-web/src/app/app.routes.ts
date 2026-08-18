import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';
import { companyGuard } from './core/guards/company.guard';
import { personGuard } from './core/guards/person.guard';

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
    path: 'profile/edit',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./features/profile-wizard/profile-wizard.component').then(
        (m) => m.ProfileWizardComponent
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
  {
    path: 'jobs/new',
    canActivate: [companyGuard],
    loadComponent: () =>
      import('./features/jobs/create/job-create.component').then((m) => m.JobCreateComponent)
  },
  {
    path: 'jobs/recommended',
    canActivate: [personGuard],
    loadComponent: () =>
      import('./features/jobs/recommended/job-recommendations.component').then(
        (m) => m.JobRecommendationsComponent
      )
  },
  {
    path: 'jobs/:id',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./features/jobs/detail/job-detail.component').then((m) => m.JobDetailComponent)
  },
  {
    path: 'companies/:id',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./features/companies/public-profile/company-public-profile.component').then(
        (m) => m.CompanyPublicProfileComponent
      )
  },
  {
    path: 'applications',
    canActivate: [personGuard],
    loadComponent: () =>
      import('./features/applications/my-applications.component').then(
        (m) => m.MyApplicationsComponent
      )
  },
  {
    path: 'company/applicants',
    canActivate: [companyGuard],
    loadComponent: () =>
      import('./features/applicants/company-applicants.component').then(
        (m) => m.CompanyApplicantsComponent
      )
  },
  {
    path: 'company/profile',
    canActivate: [companyGuard],
    loadComponent: () =>
      import('./features/company-profile/company-profile.component').then(
        (m) => m.CompanyProfileComponent
      )
  },
  {
    path: 'messages',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./features/messages/messages.component').then((m) => m.MessagesComponent)
  },
  { path: '**', redirectTo: 'login' }
];
