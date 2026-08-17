import { Injectable, inject } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from './auth.service';
import { ChatHubService } from './chat-hub.service';

// Small orchestrator so "log out" always does the same three things regardless of who
// triggers it (the sidebar's "გასვლა" button, or the 401 interceptor after a session expires):
// close the SignalR connection, clear auth state, redirect to /login.
@Injectable({ providedIn: 'root' })
export class SessionService {
  private readonly auth = inject(AuthService);
  private readonly chatHub = inject(ChatHubService);
  private readonly router = inject(Router);

  async logout(): Promise<void> {
    await this.chatHub.disconnect();
    this.auth.logout();
    await this.router.navigateByUrl('/login');
  }
}
