import { Component, computed, inject, signal } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { SessionService } from '../../../core/services/session.service';

@Component({
  selector: 'app-sidebar-nav',
  standalone: true,
  imports: [RouterLink, RouterLinkActive],
  templateUrl: './sidebar-nav.component.html',
  styleUrl: './sidebar-nav.component.css'
})
export class SidebarNavComponent {
  readonly auth = inject(AuthService);
  private readonly session = inject(SessionService);

  readonly isCompany = computed(() => this.auth.role() === 'Company');

  // Only meaningful on mobile (CSS keeps the sidebar always visible above the
  // breakpoint) - a hamburger button toggles this, nav links/backdrop close it.
  readonly isOpen = signal(false);

  toggle(): void {
    this.isOpen.update((open) => !open);
  }

  close(): void {
    this.isOpen.set(false);
  }

  logout(): void {
    this.close();
    void this.session.logout();
  }
}
