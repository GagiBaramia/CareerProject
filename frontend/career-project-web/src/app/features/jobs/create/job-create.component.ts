import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { JobService } from '../../../core/services/job.service';
import { CURRENCIES, EMPLOYMENT_TYPES, WORK_FORMATS } from '../../../core/models/job.models';
import { ProfileSkill } from '../../../core/models/profile.models';
import { SkillsAutocompleteComponent } from '../../../shared/components/skills-autocomplete/skills-autocomplete.component';

const RECENT_CANDIDATES_EXAMPLE = [
  { name: 'თამარ გელაშვილი', vacancy: 'ფრონტენდის დევე...', date: '16 მაისი, 2024', status: 'ახალი' },
  { name: 'ანა ნადირაძე', vacancy: 'ფრონტენდის დევე...', date: '15 მაისი, 2024', status: 'გამხილავში' },
  { name: 'გიორგი ბერიძე', vacancy: 'ფრონტენდის დევე...', date: '14 მაისი, 2024', status: 'ინტერვიუ' }
];

@Component({
  selector: 'app-job-create',
  standalone: true,
  imports: [ReactiveFormsModule, SkillsAutocompleteComponent],
  templateUrl: './job-create.component.html',
  styleUrl: './job-create.component.css'
})
export class JobCreateComponent {
  private readonly fb = inject(FormBuilder);
  private readonly jobService = inject(JobService);
  private readonly router = inject(Router);
  readonly auth = inject(AuthService);

  readonly employmentTypes = EMPLOYMENT_TYPES;
  readonly workFormats = WORK_FORMATS;
  readonly currencies = CURRENCIES;
  readonly recentCandidates = RECENT_CANDIDATES_EXAMPLE;

  readonly skills = signal<ProfileSkill[]>([]);
  readonly isSubmitting = signal(false);
  readonly errorMessage = signal<string | null>(null);

  readonly form = this.fb.group({
    title: ['', [Validators.required, Validators.maxLength(80)]],
    description: ['', [Validators.required, Validators.maxLength(2000)]],
    location: ['', [Validators.required]],
    employmentType: ['FullTime', [Validators.required]],
    workFormat: ['OnSite', [Validators.required]],
    salaryMin: [null as number | null],
    salaryMax: [null as number | null],
    salaryCurrency: ['USD']
  });

  get titleLength(): number {
    return this.form.controls.title.value?.length ?? 0;
  }

  get descriptionLength(): number {
    return this.form.controls.description.value?.length ?? 0;
  }

  onSkillsChange(next: ProfileSkill[]): void {
    this.skills.set(next);
  }

  cancel(): void {
    this.router.navigateByUrl('/dashboard/company');
  }

  submit(): void {
    if (this.form.invalid || this.isSubmitting()) {
      this.form.markAllAsTouched();
      return;
    }

    this.isSubmitting.set(true);
    this.errorMessage.set(null);

    const raw = this.form.getRawValue();

    this.jobService
      .createJob({
        title: raw.title!,
        description: raw.description!,
        employmentType: raw.employmentType!,
        workFormat: raw.workFormat!,
        location: raw.location!,
        salaryMin: raw.salaryMin,
        salaryMax: raw.salaryMax,
        salaryCurrency: raw.salaryCurrency,
        requiredSkills: this.skills().map((s) => ({ skillId: s.skillId, requiredLevel: s.level }))
      })
      .subscribe({
        next: () => this.router.navigateByUrl('/dashboard/company'),
        error: (err: HttpErrorResponse) => {
          this.isSubmitting.set(false);
          this.errorMessage.set(
            err.status === 400
              ? 'ფორმაში არის შეცდომა. გადაამოწმეთ შევსებული ველები.'
              : 'ვაკანსიის გამოქვეყნებისას მოხდა შეცდომა. სცადეთ ხელახლა.'
          );
        }
      });
  }
}
