import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { ProfileService } from '../../../core/services/profile.service';
import { RecommendationService } from '../../../core/services/recommendation.service';
import { ApplicationService } from '../../../core/services/application.service';
import { NotificationService } from '../../../core/services/notification.service';
import { ConversationService } from '../../../core/services/conversation.service';
import { ProfileResponse } from '../../../core/models/profile.models';
import { JobRecommendation } from '../../../core/models/recommendation.models';
import { JobApplication } from '../../../core/models/application.models';
import { AppNotification } from '../../../core/models/notification.models';
import { ConversationSummary } from '../../../core/models/conversation.models';
import { StatCardComponent } from '../../../shared/components/stat-card/stat-card.component';
import { StatusBadgeComponent } from '../../../shared/components/status-badge/status-badge.component';
import { AvatarComponent } from '../../../shared/components/avatar/avatar.component';
import { EmptyStateComponent } from '../../../shared/components/empty-state/empty-state.component';
import { SidebarNavComponent } from '../../../shared/components/sidebar-nav/sidebar-nav.component';

@Component({
  selector: 'app-person-dashboard',
  standalone: true,
  imports: [
    RouterLink,
    StatCardComponent,
    StatusBadgeComponent,
    AvatarComponent,
    EmptyStateComponent,
    SidebarNavComponent
  ],
  templateUrl: './person-dashboard.component.html',
  styleUrl: './person-dashboard.component.css'
})
export class PersonDashboardComponent implements OnInit {
  readonly auth = inject(AuthService);
  private readonly profileService = inject(ProfileService);
  private readonly recommendationService = inject(RecommendationService);
  private readonly applicationService = inject(ApplicationService);
  private readonly notificationService = inject(NotificationService);
  private readonly conversationService = inject(ConversationService);

  readonly profile = signal<ProfileResponse | null>(null);
  readonly recommendations = signal<JobRecommendation[]>([]);
  readonly applications = signal<JobApplication[]>([]);
  readonly notifications = signal<AppNotification[]>([]);
  readonly conversations = signal<ConversationSummary[]>([]);

  readonly recommendationsError = signal(false);
  readonly applicationsError = signal(false);

  readonly profileCompleteness = computed(() => {
    const p = this.profile();
    if (!p) return 0;

    const checks = [!!p.headline, !!p.cvSummary, !!p.location, !!p.photoUrl, p.skills.length > 0];
    return Math.round((checks.filter(Boolean).length / checks.length) * 100);
  });

  readonly topRecommendations = computed(() => this.recommendations().slice(0, 6));
  readonly recentApplications = computed(() => this.applications().slice(0, 5));
  readonly recentNotifications = computed(() => this.notifications().slice(0, 5));

  readonly interviewCount = computed(
    () => this.applications().filter((a) => a.status === 'Interview').length
  );
  readonly acceptedCount = computed(
    () => this.applications().filter((a) => a.status === 'Accepted').length
  );
  readonly unreadMessageCount = computed(() =>
    this.conversations().reduce((sum, c) => sum + c.unreadCount, 0)
  );

  ngOnInit(): void {
    this.profileService.getMyProfile().subscribe({
      next: (profile) => this.profile.set(profile),
      error: () => this.profile.set(null)
    });

    this.recommendationService.getRecommendedJobs().subscribe({
      next: (jobs) => this.recommendations.set(jobs),
      error: () => this.recommendationsError.set(true)
    });

    this.applicationService.getMyApplications().subscribe({
      next: (applications) => this.applications.set(applications),
      error: () => this.applicationsError.set(true)
    });

    this.notificationService.getMyNotifications().subscribe({
      next: (notifications) => this.notifications.set(notifications),
      error: () => this.notifications.set([])
    });

    this.conversationService.getConversations().subscribe({
      next: (conversations) => this.conversations.set(conversations),
      error: () => this.conversations.set([])
    });
  }

  matchPercent(job: JobRecommendation): number {
    return Math.round(job.score * 100);
  }
}
