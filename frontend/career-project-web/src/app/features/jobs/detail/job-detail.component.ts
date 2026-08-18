import { DatePipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { JobService } from '../../../core/services/job.service';
import { ApplicationService } from '../../../core/services/application.service';
import { EMPLOYMENT_TYPES, JobResponse, WORK_FORMATS } from '../../../core/models/job.models';
import { proficiencyLabel } from '../../../core/models/profile.models';
import { SidebarNavComponent } from '../../../shared/components/sidebar-nav/sidebar-nav.component';

@Component({
  selector: 'app-job-detail',
  standalone: true,
  imports: [DatePipe, RouterLink, SidebarNavComponent],
  templateUrl: './job-detail.component.html',
  styleUrl: './job-detail.component.css'
})
export class JobDetailComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly jobService = inject(JobService);
  private readonly applicationService = inject(ApplicationService);
  readonly auth = inject(AuthService);

  readonly employmentTypes = EMPLOYMENT_TYPES;
  readonly workFormats = WORK_FORMATS;
  readonly proficiencyLabel = proficiencyLabel;

  readonly isLoading = signal(true);
  readonly errorMessage = signal<string | null>(null);
  readonly job = signal<JobResponse | null>(null);

  readonly hasApplied = signal(false);
  readonly isApplying = signal(false);
  readonly applyError = signal<string | null>(null);

  ngOnInit(): void {
    const jobId = this.route.snapshot.paramMap.get('id');
    if (!jobId) {
      this.errorMessage.set('ვაკანსია ვერ მოიძებნა.');
      this.isLoading.set(false);
      return;
    }

    this.jobService.getJob(jobId).subscribe({
      next: (job) => {
        this.job.set(job);
        this.isLoading.set(false);
      },
      error: () => {
        this.errorMessage.set('ვაკანსიის ჩატვირთვისას მოხდა შეცდომა.');
        this.isLoading.set(false);
      }
    });

    // Only candidates can apply - check whether they already have, so the button
    // reflects reality even on a fresh page load (not just within the same session).
    if (this.auth.role() === 'Person') {
      this.applicationService.getMyApplications().subscribe({
        next: (applications) => {
          if (applications.some((application) => application.jobId === jobId)) {
            this.hasApplied.set(true);
          }
        },
        error: () => {
          /* Non-critical - apply button just stays in its default state. */
        }
      });
    }
  }

  employmentTypeLabel(value: string): string {
    return this.employmentTypes.find((type) => type.value === value)?.label ?? value;
  }

  workFormatLabel(value: string): string {
    return this.workFormats.find((type) => type.value === value)?.label ?? value;
  }

  apply(): void {
    const job = this.job();
    if (!job || this.hasApplied() || this.isApplying()) {
      return;
    }

    this.isApplying.set(true);
    this.applyError.set(null);

    this.jobService.applyToJob(job.id).subscribe({
      next: () => {
        this.hasApplied.set(true);
        this.isApplying.set(false);
      },
      error: (err) => {
        if (err.status === 409) {
          this.hasApplied.set(true);
        } else {
          this.applyError.set('განაცხადის გაგზავნისას მოხდა შეცდომა. სცადეთ თავიდან.');
        }
        this.isApplying.set(false);
      }
    });
  }
}
