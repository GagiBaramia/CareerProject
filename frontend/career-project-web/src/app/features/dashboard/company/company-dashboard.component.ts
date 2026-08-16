import { Component } from '@angular/core';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-company-dashboard',
  standalone: true,
  template: `
    <div style="padding: 40px; font-family: var(--font-sans);">
      <h1>გამარჯობა, {{ auth.currentUser()?.displayName }} 👋</h1>
      <p>კომპანიის dashboard აქ აშენდება Stage 12-ზე (ვაკანსიის გამოქვეყნება).</p>
    </div>
  `
})
export class CompanyDashboardComponent {
  constructor(readonly auth: AuthService) {}
}
