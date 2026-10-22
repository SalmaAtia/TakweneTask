import { Component, computed, input } from '@angular/core';
import { ApiError } from './api-error';

/**
 * Inline API error messages for a form.
 * - With `field`: that field's validation messages (400), e.g. under its input.
 * - Without: everything not shown by a field-level instance (`shownElsewhere`); the error's
 *   message when there are no field errors at all (401, 404, 409, 422...).
 */
@Component({
  selector: 'app-form-errors',
  template: `
    @for (message of messages(); track $index) {
      <p class="form-error" role="alert">{{ message }}</p>
    }
  `,
  styles: `
    .form-error {
      margin: 0.25rem 0;
      color: #b91c1c;
      font-size: 0.875rem;
    }
  `,
})
export class FormErrors {
  readonly error = input<ApiError | null>(null);
  readonly field = input<string>();
  readonly shownElsewhere = input<string[]>([]);

  protected readonly messages = computed(() => {
    const error = this.error();
    if (!error) return [];

    const field = this.field();
    if (field) return error.fieldErrors[field] ?? [];

    const fieldErrors = Object.entries(error.fieldErrors);
    if (fieldErrors.length === 0) return [error.message];

    return fieldErrors
      .filter(([key]) => !this.shownElsewhere().includes(key))
      .flatMap(([, messages]) => messages);
  });
}

/** Normalizes whatever an HTTP call threw (the error interceptor already makes it an ApiError). */
export function asApiError(error: unknown): ApiError {
  return error instanceof ApiError ? error : new ApiError(String(error), 0);
}
