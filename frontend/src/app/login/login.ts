import { HttpContext } from '@angular/common/http';
import { Component, inject, input, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../core/auth/auth.service';
import { ApiError } from '../core/http/api-error';
import { HANDLES_ERRORS } from '../core/http/error.interceptor';
import { FormErrors, asApiError } from '../core/http/form-errors';

/**
 * Minimal login form. No client-side rules: the API validates (400, shown under each field) and
 * checks the credentials (401, shown above the button). On success it returns to `?returnUrl=`.
 */
@Component({
  selector: 'app-login',
  imports: [ReactiveFormsModule, FormErrors],
  templateUrl: './login.html',
  styleUrl: './login.css',
})
export class Login {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  /** Where to go after logging in (withComponentInputBinding). */
  readonly returnUrl = input<string>();

  protected readonly form = inject(FormBuilder).nonNullable.group({ username: '', password: '' });
  protected readonly submitting = signal(false);
  protected readonly error = signal<ApiError | null>(null);

  protected submit(): void {
    this.submitting.set(true);
    this.error.set(null);

    this.auth
      .login(this.form.getRawValue(), new HttpContext().set(HANDLES_ERRORS, true))
      .subscribe({
        next: () => this.router.navigateByUrl(safeReturnUrl(this.returnUrl())),
        error: (error: unknown) => {
          this.error.set(asApiError(error));
          this.submitting.set(false);
        },
      });
  }
}

/**
 * Only same-app paths, so a crafted link can't send the user to another site after login; never back
 * to the login page itself (e.g. after clicking the header's "Log in" while already on it).
 */
export function safeReturnUrl(url: string | undefined): string {
  const isAppPath = url?.startsWith('/') && !url.startsWith('//');
  return isAppPath && !url!.startsWith('/login') ? url! : '/';
}
