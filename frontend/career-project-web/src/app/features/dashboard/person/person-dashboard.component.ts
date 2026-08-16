import { Component } from '@angular/core';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-person-dashboard',
  standalone: true,
  template: `
    <div style="padding: 40px; font-family: var(--font-sans);">
      <h1>გამარჯობა, {{ auth.currentUser()?.displayName }} 👋</h1>
      <p>კანდიდატის dashboard აქ აშენდება Stage 16-ზე (რეკომენდაციები).</p>
    </div>
  `
})
export class PersonDashboardComponent {
  constructor(readonly auth: AuthService) {}
}
