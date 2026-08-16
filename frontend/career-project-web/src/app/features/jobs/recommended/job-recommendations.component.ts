import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../../core/services/auth.service';
import { RecommendationService } from '../../../core/services/recommendation.service';
import { JobRecommendation } from '../../../core/models/recommendation.models';
import { EMPLOYMENT_TYPES, WORK_FORMATS } from '../../../core/models/job.models';

type SortOrder = 'desc' | 'asc';

@Component({
  selector: 'app-job-recommendations',
  standalone: true,
  imports: [FormsModule],
  templateUrl: './job-recommendations.component.html',
  styleUrl: './job-recommendations.component.css'
})
export class JobRecommendationsComponent implements OnInit {
  private readonly recommendationService = inject(RecommendationService);
  readonly auth = inject(AuthService);

  readonly employmentTypes = EMPLOYMENT_TYPES;
  readonly workFormats = WORK_FORMATS;

  readonly isLoading = signal(true);
  readonly errorMessage = signal<string | null>(null);
  readonly jobs = signal<JobRecommendation[]>([]);

  readonly locationFilter = signal('');
  readonly employmentTypeFilter = signal('');
  readonly sortOrder = signal<SortOrder>('desc');

  readonly locations = computed(() =>
    Array.from(new Set(this.jobs().map((j) => j.location))).sort()
  );

  readonly filteredJobs = computed(() => {
    let result = this.jobs();

    if (this.locationFilter()) {
      result = result.filter((j) => j.location === this.locationFilter());
    }
    if (this.employmentTypeFilter()) {
      result = result.filter((j) => j.employmentType === this.employmentTypeFilter());
    }

    result = [...result].sort((a, b) =>
      this.sortOrder() === 'desc' ? b.score - a.score : a.score - b.score
    );

    return result;
  });

  ngOnInit(): void {
    this.recommendationService.getRecommendedJobs().subscribe({
      next: (jobs) => {
        this.jobs.set(jobs);
        this.isLoading.set(false);
      },
      error: () => {
        this.errorMessage.set('რეკომენდაციების ჩატვირთვისას მოხდა შეცდომა.');
        this.isLoading.set(false);
      }
    });
  }

  employmentTypeLabel(value: string): string {
    return this.employmentTypes.find((t) => t.value === value)?.label ?? value;
  }

  workFormatLabel(value: string): string {
    return this.workFormats.find((t) => t.value === value)?.label ?? value;
  }

  matchPercent(job: JobRecommendation): number {
    return Math.round(job.score * 100);
  }

  toggleSortOrder(): void {
    this.sortOrder.set(this.sortOrder() === 'desc' ? 'asc' : 'desc');
  }
}
