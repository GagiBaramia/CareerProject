import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../../core/services/auth.service';
import { RecommendationService } from '../../../core/services/recommendation.service';
import { AiChatService } from '../../../core/services/ai-chat.service';
import { JobRecommendation } from '../../../core/models/recommendation.models';
import { AiChatMessage } from '../../../core/models/ai-chat.models';
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
  private readonly aiChatService = inject(AiChatService);
  readonly auth = inject(AuthService);

  readonly employmentTypes = EMPLOYMENT_TYPES;
  readonly workFormats = WORK_FORMATS;

  readonly isLoading = signal(true);
  readonly errorMessage = signal<string | null>(null);
  readonly jobs = signal<JobRecommendation[]>([]);

  readonly locationFilter = signal('');
  readonly employmentTypeFilter = signal('');
  readonly sortOrder = signal<SortOrder>('desc');

  readonly highlightedJobId = signal<string | null>(null);

  readonly chatMessages = signal<AiChatMessage[]>([]);
  readonly chatInput = signal('');
  readonly chatLoading = signal(false);
  readonly chatError = signal<string | null>(null);

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

  sendChatMessage(): void {
    const message = this.chatInput().trim();
    if (!message || this.chatLoading()) {
      return;
    }

    this.chatMessages.update((messages) => [...messages, { role: 'user', content: message }]);
    this.chatInput.set('');
    this.chatLoading.set(true);
    this.chatError.set(null);

    this.aiChatService.sendMessage(message).subscribe({
      next: (response) => {
        this.chatMessages.update((messages) => [
          ...messages,
          { role: 'assistant', content: response.reply, referencedJobs: response.referencedJobs }
        ]);
        this.chatLoading.set(false);
      },
      error: () => {
        this.chatError.set('შეტყობინების გაგზავნისას მოხდა შეცდომა. სცადეთ თავიდან.');
        this.chatLoading.set(false);
      }
    });
  }

  // Vacancy cards live in the same page's job list, so "opening" a job the assistant
  // referenced means clearing filters that might hide it, then scrolling it into view.
  focusJob(jobId: string): void {
    this.locationFilter.set('');
    this.employmentTypeFilter.set('');
    this.highlightedJobId.set(jobId);

    setTimeout(() => {
      document.getElementById('job-' + jobId)?.scrollIntoView({ behavior: 'smooth', block: 'center' });
    });
    setTimeout(() => this.highlightedJobId.set(null), 2000);
  }
}
