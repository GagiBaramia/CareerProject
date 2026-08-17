import { DatePipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ApplicationService } from '../../core/services/application.service';
import { APPLICATION_STATUSES, ApplicationStatus, JobApplication } from '../../core/models/application.models';
import { StatusBadgeComponent } from '../../shared/components/status-badge/status-badge.component';
import { AvatarComponent } from '../../shared/components/avatar/avatar.component';
import { EmptyStateComponent } from '../../shared/components/empty-state/empty-state.component';
import { SidebarNavComponent } from '../../shared/components/sidebar-nav/sidebar-nav.component';

@Component({
  selector: 'app-company-applicants',
  standalone: true,
  imports: [
    DatePipe,
    FormsModule,
    StatusBadgeComponent,
    AvatarComponent,
    EmptyStateComponent,
    SidebarNavComponent
  ],
  templateUrl: './company-applicants.component.html',
  styleUrl: './company-applicants.component.css'
})
export class CompanyApplicantsComponent implements OnInit {
  private readonly applicationService = inject(ApplicationService);

  readonly statuses = APPLICATION_STATUSES;

  readonly isLoading = signal(true);
  readonly errorMessage = signal<string | null>(null);
  readonly applications = signal<JobApplication[]>([]);
  readonly updatingId = signal<string | null>(null);
  readonly updateError = signal<string | null>(null);

  ngOnInit(): void {
    this.applicationService.getCompanyApplications().subscribe({
      next: (applications) => {
        this.applications.set(applications);
        this.isLoading.set(false);
      },
      error: () => {
        this.errorMessage.set('კანდიდატების ჩატვირთვისას მოხდა შეცდომა.');
        this.isLoading.set(false);
      }
    });
  }

  changeStatus(application: JobApplication, status: string): void {
    if (status === application.status || this.updatingId()) {
      return;
    }

    this.updatingId.set(application.id);
    this.updateError.set(null);

    this.applicationService.updateStatus(application.id, status as ApplicationStatus).subscribe({
      next: (updated) => {
        this.applications.update((apps) => apps.map((a) => (a.id === updated.id ? updated : a)));
        this.updatingId.set(null);
      },
      error: () => {
        this.updateError.set('სტატუსის განახლებისას მოხდა შეცდომა.');
        this.updatingId.set(null);
      }
    });
  }
}
