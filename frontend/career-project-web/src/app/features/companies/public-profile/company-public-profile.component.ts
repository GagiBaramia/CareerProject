import { Component, OnInit, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { CompanyService } from '../../../core/services/company.service';
import { JobService } from '../../../core/services/job.service';
import { CompanyProfile } from '../../../core/models/company.models';
import { EMPLOYMENT_TYPES, JobResponse, WORK_FORMATS } from '../../../core/models/job.models';
import { AvatarComponent } from '../../../shared/components/avatar/avatar.component';
import { EmptyStateComponent } from '../../../shared/components/empty-state/empty-state.component';
import { SidebarNavComponent } from '../../../shared/components/sidebar-nav/sidebar-nav.component';

@Component({
  selector: 'app-company-public-profile',
  standalone: true,
  imports: [RouterLink, AvatarComponent, EmptyStateComponent, SidebarNavComponent],
  templateUrl: './company-public-profile.component.html',
  styleUrl: './company-public-profile.component.css'
})
export class CompanyPublicProfileComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly companyService = inject(CompanyService);
  private readonly jobService = inject(JobService);

  readonly employmentTypes = EMPLOYMENT_TYPES;
  readonly workFormats = WORK_FORMATS;

  readonly isLoading = signal(true);
  readonly errorMessage = signal<string | null>(null);
  readonly company = signal<CompanyProfile | null>(null);

  readonly isLoadingJobs = signal(true);
  readonly jobs = signal<JobResponse[]>([]);

  ngOnInit(): void {
    const companyId = this.route.snapshot.paramMap.get('id');
    if (!companyId) {
      this.errorMessage.set('კომპანია ვერ მოიძებნა.');
      this.isLoading.set(false);
      this.isLoadingJobs.set(false);
      return;
    }

    this.companyService.getCompany(companyId).subscribe({
      next: (company) => {
        this.company.set(company);
        this.isLoading.set(false);
      },
      error: () => {
        this.errorMessage.set('კომპანიის ჩატვირთვისას მოხდა შეცდომა.');
        this.isLoading.set(false);
      }
    });

    this.jobService.getJobs(companyId).subscribe({
      next: (jobs) => {
        this.jobs.set(jobs);
        this.isLoadingJobs.set(false);
      },
      error: () => {
        this.isLoadingJobs.set(false);
      }
    });
  }

  employmentTypeLabel(value: string): string {
    return this.employmentTypes.find((type) => type.value === value)?.label ?? value;
  }

  workFormatLabel(value: string): string {
    return this.workFormats.find((type) => type.value === value)?.label ?? value;
  }
}
