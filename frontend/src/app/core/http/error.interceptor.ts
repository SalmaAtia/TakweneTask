import { HttpContextToken, HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, throwError } from 'rxjs';
import { AuthService } from '../auth/auth.service';
import { ApiError } from './api-error';
import { ErrorNotifier } from './error-notifier';

/**
 * Set to true on a request whose caller shows the error itself (e.g. a form showing field errors),
 * so it isn't also shown in the global banner: `{ context: new HttpContext().set(HANDLES_ERRORS, true) }`.
 */
export const HANDLES_ERRORS = new HttpContextToken<boolean>(() => false);

/**
 * Turns failed responses into an {@link ApiError} carrying the ProblemDetails message, shows that
 * message in the global banner, and rethrows it. A 401 on a request that sent a token means the token
 * is no longer valid, so the session is dropped.
 */
export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const notifier = inject(ErrorNotifier);
  const auth = inject(AuthService);

  return next(req).pipe(
    catchError((error: unknown) => {
      if (!(error instanceof HttpErrorResponse)) return throwError(() => error);

      const apiError = ApiError.from(error);

      if (error.status === 401 && req.headers.has('Authorization')) auth.logout();
      if (!req.context.get(HANDLES_ERRORS)) notifier.show(apiError.message);

      return throwError(() => apiError);
    }),
  );
};
