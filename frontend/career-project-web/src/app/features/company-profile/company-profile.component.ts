import { Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { CompanyService } from '../../core/services/company.service';
import { UploadService } from '../../core/services/upload.service';
import { AvatarComponent } from '../../shared/components/avatar/avatar.component';
import { SidebarNavComponent } from '../../shared/components/sidebar-nav/sidebar-nav.component';

@Component({
  selector: 'app-company-profile',
  standalone: true,
  imports: [ReactiveFormsModule, AvatarComponent, SidebarNavComponent],
  templateUrl: './company-profile.component.html',
  styleUrl: './company-profile.component.css'
})
export class CompanyProfileComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly companyService = inject(CompanyService);
  private readonly uploadService = inject(UploadService);

  readonly isLoading = signal(true);
  readonly isSaving = signal(false);
  readonly errorMessage = signal<string | null>(null);
  readonly successMessage = signal<string | null>(null);

  readonly logoUrl = signal<string | null>(null);
  readonly isUploadingLogo = signal(false);
  readonly logoError = signal<string | null>(null);

  readonly form = this.fb.group({
    name: ['', [Validators.required, Validators.minLength(1)]],
    description: [''],
    industry: ['']
  });

  ngOnInit(): void {
    this.companyService.getMyCompany().subscribe({
      next: (company) => {
        this.form.patchValue({
          name: company.name,
          description: company.description ?? '',
          industry: company.industry ?? ''
        });
        this.logoUrl.set(company.logoUrl);
        this.isLoading.set(false);
      },
      error: () => {
        this.errorMessage.set('კომპანიის პროფილის ჩატვირთვისას მოხდა შეცდომა.');
        this.isLoading.set(false);
      }
    });
  }

  onLogoSelected(event: Event): void {
    const file = (event.target as HTMLInputElement).files?.[0];
    if (!file) {
      return;
    }

    this.isUploadingLogo.set(true);
    this.logoError.set(null);

    this.uploadService.uploadLogo(file).subscribe({
      next: (result) => {
        this.logoUrl.set(result.logoUrl);
        this.isUploadingLogo.set(false);
      },
      error: (err) => {
        this.logoError.set(err.error?.message || 'ლოგოს ატვირთვისას მოხდა შეცდომა.');
        this.isUploadingLogo.set(false);
      }
    });
  }

  submit(): void {
    if (this.form.invalid || this.isSaving()) {
      this.form.markAllAsTouched();
      return;
    }

    this.isSaving.set(true);
    this.errorMessage.set(null);
    this.successMessage.set(null);

    const { name, description, industry } = this.form.getRawValue();

    this.companyService
      .updateMyCompany({
        name: name!,
        description: description || null,
        industry: industry || null,
        logoUrl: this.logoUrl()
      })
      .subscribe({
        next: () => {
          this.isSaving.set(false);
          this.successMessage.set('პროფილი წარმატებით შენახულია.');
        },
        error: () => {
          this.isSaving.set(false);
          this.errorMessage.set('შენახვისას მოხდა შეცდომა.');
        }
      });
  }
}
