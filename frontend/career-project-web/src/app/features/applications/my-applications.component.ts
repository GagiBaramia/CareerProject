import { DatePipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ApplicationService } from '../../core/services/application.service';
import { ConversationService } from '../../core/services/conversation.service';
import { JobApplication } from '../../core/models/application.models';
import { ConversationSummary } from '../../core/models/conversation.models';
import { StatusBadgeComponent } from '../../shared/components/status-badge/status-badge.component';
import { EmptyStateComponent } from '../../shared/components/empty-state/empty-state.component';
import { SidebarNavComponent } from '../../shared/components/sidebar-nav/sidebar-nav.component';

@Component({
  selector: 'app-my-applications',
  standalone: true,
  imports: [RouterLink, DatePipe, StatusBadgeComponent, EmptyStateComponent, SidebarNavComponent],
  templateUrl: './my-applications.component.html',
  styleUrl: './my-applications.component.css'
})
export class MyApplicationsComponent implements OnInit {
  private readonly applicationService = inject(ApplicationService);
  private readonly conversationService = inject(ConversationService);

  readonly isLoading = signal(true);
  readonly errorMessage = signal<string | null>(null);
  readonly applications = signal<JobApplication[]>([]);
  readonly conversations = signal<ConversationSummary[]>([]);

  ngOnInit(): void {
    this.applicationService.getMyApplications().subscribe({
      next: (applications) => {
        this.applications.set(applications);
        this.isLoading.set(false);
      },
      error: () => {
        this.errorMessage.set('განაცხადების ჩატვირთვისას მოხდა შეცდომა.');
        this.isLoading.set(false);
      }
    });

    this.conversationService.getConversations().subscribe({
      next: (conversations) => this.conversations.set(conversations),
      error: () => this.conversations.set([])
    });
  }

  conversationIdFor(applicationId: string): string | null {
    return this.conversations().find((c) => c.applicationId === applicationId)?.id ?? null;
  }
}
