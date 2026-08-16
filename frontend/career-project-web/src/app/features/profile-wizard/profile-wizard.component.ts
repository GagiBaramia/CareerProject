import { Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { forkJoin } from 'rxjs';
import { AuthService } from '../../core/services/auth.service';
import { ProfileService } from '../../core/services/profile.service';
import { ProfileSkill } from '../../core/models/profile.models';
import { SkillsAutocompleteComponent } from './components/skills-autocomplete/skills-autocomplete.component';

interface WizardStep {
  step: number;
  label: string;
}

const STEPS: WizardStep[] = [
  { step: 1, label: 'პირადი ინფორმაცია' },
  { step: 2, label: 'გამოცდილება' },
  { step: 3, label: 'განათლება' },
  { step: 4, label: 'დამატებითი ინფორმაცია' }
];

@Component({
  selector: 'app-profile-wizard',
  standalone: true,
  imports: [ReactiveFormsModule, SkillsAutocompleteComponent],
  templateUrl: './profile-wizard.component.html',
  styleUrl: './profile-wizard.component.css'
})
export class ProfileWizardComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly profileService = inject(ProfileService);
  private readonly router = inject(Router);
  readonly auth = inject(AuthService);

  readonly steps = STEPS;
  readonly currentStep = signal(1);
  readonly isLoading = signal(true);
  readonly isSaving = signal(false);
  readonly errorMessage = signal<string | null>(null);
  readonly skills = signal<ProfileSkill[]>([]);

  readonly form = this.fb.group({
    fullName: ['', [Validators.required, Validators.minLength(1)]],
    headline: ['', [Validators.maxLength(120)]],
    cvSummary: ['', [Validators.maxLength(600)]],
    location: ['']
  });

  get headlineLength(): number {
    return this.form.controls.headline.value?.length ?? 0;
  }

  get summaryLength(): number {
    return this.form.controls.cvSummary.value?.length ?? 0;
  }

  ngOnInit(): void {
    this.profileService.getMyProfile().subscribe({
      next: (profile) => {
        this.form.patchValue({
          fullName: profile.fullName,
          headline: profile.headline ?? '',
          cvSummary: profile.cvSummary ?? '',
          location: profile.location ?? ''
        });
        this.skills.set(profile.skills);
        this.isLoading.set(false);
      },
      error: () => {
        this.errorMessage.set('პროფილის ჩატვირთვისას მოხდა შეცდომა.');
        this.isLoading.set(false);
      }
    });
  }

  onSkillsChange(next: ProfileSkill[]): void {
    this.skills.set(next);
  }

  goToStep(step: number): void {
    if (step === 1) {
      this.currentStep.set(1);
    }
  }

  submitStepOne(): void {
    if (this.form.invalid || this.isSaving()) {
      this.form.markAllAsTouched();
      return;
    }

    this.isSaving.set(true);
    this.errorMessage.set(null);

    const { fullName, headline, cvSummary, location } = this.form.getRawValue();

    forkJoin([
      this.profileService.updateMyProfile({
        fullName: fullName!,
        headline: headline || null,
        cvSummary: cvSummary || null,
        location: location || null
      }),
      this.profileService.updateMySkills({
        skills: this.skills().map((s) => ({ skillId: s.skillId, level: s.level }))
      })
    ]).subscribe({
      next: () => {
        this.isSaving.set(false);
        this.currentStep.set(2);
      },
      error: () => {
        this.isSaving.set(false);
        this.errorMessage.set('შენახვისას მოხდა შეცდომა. სცადეთ ხელახლა.');
      }
    });
  }

  cancel(): void {
    this.router.navigateByUrl(this.auth.dashboardRouteForCurrentRole());
  }
}
