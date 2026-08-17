import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { CompanyService } from '../../../core/services/company.service';
import { JobService } from '../../../core/services/job.service';
import { ApplicationService } from '../../../core/services/application.service';
import { ConversationService } from '../../../core/services/conversation.service';
import { CompanyProfile } from '../../../core/models/company.models';
import { JobResponse } from '../../../core/models/job.models';
import { JobApplication } from '../../../core/models/application.models';
import { ConversationSummary } from '../../../core/models/conversation.models';
import { StatCardComponent } from '../../../shared/components/stat-card/stat-card.component';
import { StatusBadgeComponent } from '../../../shared/components/status-badge/status-badge.component';
import { AvatarComponent } from '../../../shared/components/avatar/avatar.component';
import { EmptyStateComponent } from '../../../shared/components/empty-state/empty-state.component';
import { SidebarNavComponent } from '../../../shared/components/sidebar-nav/sidebar-nav.component';

@Component({
  selector: 'app-company-dashboard',
  standalone: true,
  imports: [
    RouterLink,
    StatCardComponent,
    StatusBadgeComponent,
    AvatarComponent,
    EmptyStateComponent,
    SidebarNavComponent
  ],
  templateUrl: './company-dashboard.component.html',
  styleUrl: './company-dashboard.component.css'
})
export class CompanyDashboardComponent implements OnInit {
  readonly auth = inject(AuthService);
  private readonly companyService = inject(CompanyService);
  private readonly jobService = inject(JobService);
  private readonly applicationService = inject(ApplicationService);
  private readonly conversationService = inject(ConversationService);

  readonly company = signal<CompanyProfile | null>(null);
  readonly allJobs = signal<JobResponse[]>([]);
  readonly applications = signal<JobApplication[]>([]);
  readonly conversations = signal<ConversationSummary[]>([]);

  readonly jobsError = signal(false);
  readonly applicationsError = signal(false);

  readonly myJobs = computed(() => {
    const companyId = this.company()?.id;
    if (!companyId) return [];
    return this.allJobs().filter((j) => j.companyId === companyId);
  });

  readonly recentJobs = computed(() =>
    [...this.myJobs()].sort((a, b) => b.createdAt.localeCompare(a.createdAt)).slice(0, 5)
  );

  readonly recentApplicants = computed(() =>
    [...this.applications()].sort((a, b) => b.appliedAt.localeCompare(a.appliedAt)).slice(0, 6)
  );

  readonly newApplicationsCount = computed(
    () => this.applications().filter((a) => a.status === 'Submitted').length
  );
  readonly interviewCount = computed(
    () => this.applications().filter((a) => a.status === 'Interview').length
  );
  readonly acceptedCount = computed(
    () => this.applications().filter((a) => a.status === 'Accepted').length
  );

  ngOnInit(): void {
    this.companyService.getMyCompany().subscribe({
      next: (company) => this.company.set(company),
      error: () => this.company.set(null)
    });

    this.jobService.getJobs().subscribe({
      next: (jobs) => this.allJobs.set(jobs),
      error: () => this.jobsError.set(true)
    });

    this.applicationService.getCompanyApplications().subscribe({
      next: (applications) => this.applications.set(applications),
      error: () => this.applicationsError.set(true)
    });

    this.conversationService.getConversations().subscribe({
      next: (conversations) => this.conversations.set(conversations),
      error: () => this.conversations.set([])
    });
  }
}
