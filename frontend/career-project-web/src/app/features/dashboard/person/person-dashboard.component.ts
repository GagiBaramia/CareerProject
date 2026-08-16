import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-person-dashboard',
  standalone: true,
  imports: [RouterLink],
  template: `
    <div style="padding: 40px; font-family: var(--font-sans);">
      <h1>გამარჯობა, {{ auth.currentUser()?.displayName }} 👋</h1>
      <p>
        <a routerLink="/profile/edit">შეავსე პროფილი</a> — დაამატე CV-ის დეტალები და უნარები.
      </p>
      <p>დანარჩენი dashboard აქ აშენდება Stage 16-ზე (რეკომენდაციები).</p>
    </div>
  `
})
export class PersonDashboardComponent {
  constructor(readonly auth: AuthService) {}
}
