import { HttpErrorResponse } from '@angular/common/http';

/** RFC 7807 body returned by the API for every error (ValidationProblemDetails adds `errors`). */
export interface ProblemDetails {
  type?: string;
  title?: string;
  status?: number;
  detail?: string;
  instance?: string;
  traceId?: string;
  /** 400 only: camelCase field name -> messages. */
  errors?: Record<string, string[]>;
}

/** What the error interceptor rethrows, so callers get one shape whatever went wrong. */
export class ApiError extends Error {
  constructor(
    message: string,
    /** HTTP status; 0 when the server couldn't be reached. */
    readonly status: number,
    /** Per-field validation messages (400), keyed by camelCase field name. */
    readonly fieldErrors: Record<string, string[]> = {},
    readonly problem: ProblemDetails | null = null,
  ) {
    super(message);
    this.name = 'ApiError';
  }

  static from(response: HttpErrorResponse): ApiError {
    if (response.status === 0) {
      return new ApiError('Cannot reach the server. Check that the API is running.', 0);
    }

    const problem = isProblemDetails(response.error) ? response.error : null;
    const fieldErrors = problem?.errors ?? {};
    const validationMessages = Object.values(fieldErrors).flat();

    const message =
      problem?.detail ??
      (validationMessages.length > 0 ? validationMessages.join(' ') : null) ??
      problem?.title ??
      `Request failed (${response.status} ${response.statusText}).`;

    return new ApiError(message, response.status, fieldErrors, problem);
  }
}

function isProblemDetails(body: unknown): body is ProblemDetails {
  return typeof body === 'object' && body !== null && ('title' in body || 'detail' in body);
}
