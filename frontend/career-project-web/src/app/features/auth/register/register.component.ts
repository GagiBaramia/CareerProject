import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { UserRole } from '../../../core/models/auth.models';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './register.component.html',
  styleUrl: '../login/login.component.css'
})
export class RegisterComponent {
  private readonly fb = inject(FormBuilder);
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  readonly selectedRole = signal<UserRole>('Person');

  readonly form = this.fb.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(8)]],
    name: ['', [Validators.required, Validators.minLength(1)]]
  });

  readonly isSubmitting = signal(false);
  readonly errorMessage = signal<string | null>(null);

  selectRole(role: UserRole): void {
    this.selectedRole.set(role);
    this.errorMessage.set(null);
  }

  submit(): void {
    if (this.form.invalid || this.isSubmitting()) {
      this.form.markAllAsTouched();
      return;
    }

    this.isSubmitting.set(true);
    this.errorMessage.set(null);

    const { email, password, name } = this.form.getRawValue() as {
      email: string;
      password: string;
      name: string;
    };

    const request$ =
      this.selectedRole() === 'Person'
        ? this.authService.registerPerson({ email, password, fullName: name })
        : this.authService.registerCompany({ email, password, companyName: name });

    request$.subscribe({
      next: () => {
        this.router.navigateByUrl(this.authService.dashboardRouteForCurrentRole());
      },
      error: (err: HttpErrorResponse) => {
        this.isSubmitting.set(false);
        this.errorMessage.set(
          err.status === 409
            ? 'ეს ელ.ფოსტა უკვე რეგისტრირებულია.'
            : 'რეგისტრაციისას მოხდა შეცდომა. სცადეთ ხელახლა.'
        );
      }
    });
  }
}
