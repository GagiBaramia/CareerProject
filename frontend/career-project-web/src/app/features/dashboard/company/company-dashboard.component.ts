import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-company-dashboard',
  standalone: true,
  imports: [RouterLink],
  template: `
    <div style="padding: 40px; font-family: var(--font-sans);">
      <h1>გამარჯობა, {{ auth.currentUser()?.displayName }} 👋</h1>
      <p>
        <a routerLink="/jobs/new">გამოაქვეყნე ახალი ვაკანსია</a>
      </p>
      <p>დანარჩენი dashboard აქ აშენდება მომდევნო ეტაპებზე.</p>
    </div>
  `
})
export class CompanyDashboardComponent {
  constructor(readonly auth: AuthService) {}
}
