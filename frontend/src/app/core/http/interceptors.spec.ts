import { HttpClient, HttpContext, provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { environment } from '../../../environments/environment';
import { authInterceptor } from '../auth/auth.interceptor';
import { AuthService } from '../auth/auth.service';
import { ApiError } from './api-error';
import { ErrorNotifier } from './error-notifier';
import { HANDLES_ERRORS, errorInterceptor } from './error.interceptor';

const api = environment.apiBaseUrl;

describe('HTTP interceptors', () => {
  let http: HttpClient;
  let backend: HttpTestingController;
  let auth: AuthService;
  let notifier: ErrorNotifier;

  beforeEach(() => {
    sessionStorage.clear();
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(withInterceptors([authInterceptor, errorInterceptor])),
        provideHttpClientTesting(),
      ],
    });
    http = TestBed.inject(HttpClient);
    backend = TestBed.inject(HttpTestingController);
    auth = TestBed.inject(AuthService);
    notifier = TestBed.inject(ErrorNotifier);
  });

  afterEach(() => backend.verify());

  function logIn(expiresAt = new Date(Date.now() + 60_000).toISOString()): void {
    auth.login({ username: 'u', password: 'p' }).subscribe();
    backend.expectOne(`${api}/auth/login`).flush({ accessToken: 'jwt-token', expiresAt });
  }

  /** Subscribes to a GET and returns a holder that receives the rethrown error. */
  function get(url: string, context?: HttpContext): { error?: unknown } {
    const result: { error?: unknown } = {};
    http.get(url, { context }).subscribe({ error: (e) => (result.error = e) });
    return result;
  }

  describe('authInterceptor', () => {
    it('sends no Authorization header when logged out', () => {
      get(`${api}/tracks`);
      expect(backend.expectOne(`${api}/tracks`).request.headers.has('Authorization')).toBe(false);
    });

    it('attaches the bearer token to API requests when logged in', () => {
      logIn();
      get(`${api}/tracks`);
      expect(backend.expectOne(`${api}/tracks`).request.headers.get('Authorization')).toBe(
        'Bearer jwt-token',
      );
    });

    it('never sends the token to another host', () => {
      logIn();
      get('https://example.com/data');
      expect(
        backend.expectOne('https://example.com/data').request.headers.has('Authorization'),
      ).toBe(false);
    });

    it('drops an expired token', () => {
      logIn(new Date(Date.now() - 1_000).toISOString());
      get(`${api}/tracks`);
      expect(backend.expectOne(`${api}/tracks`).request.headers.has('Authorization')).toBe(false);
      expect(auth.isLoggedIn()).toBe(false);
    });
  });

  describe('errorInterceptor', () => {
    it('surfaces the ProblemDetails detail and rethrows an ApiError', () => {
      const result = get(`${api}/tracks/1`);
      backend
        .expectOne(`${api}/tracks/1`)
        .flush(
          { title: 'Not Found', status: 404, detail: "Track '1' was not found." },
          { status: 404, statusText: 'Not Found' },
        );

      expect(notifier.message()).toBe("Track '1' was not found.");
      expect(result.error).toBeInstanceOf(ApiError);
      expect((result.error as ApiError).status).toBe(404);
    });

    it('uses the validation messages for a 400 and keeps them per field', () => {
      const result = get(`${api}/tracks`);
      backend.expectOne(`${api}/tracks`).flush(
        {
          title: 'One or more validation errors occurred.',
          status: 400,
          errors: { status: ["'Status' must be one of: Draft, Submitted, Distributed."] },
        },
        { status: 400, statusText: 'Bad Request' },
      );

      const error = result.error as ApiError;
      expect(error.message).toBe("'Status' must be one of: Draft, Submitted, Distributed.");
      expect(error.fieldErrors['status']).toHaveLength(1);
    });

    it('reports an unreachable server', () => {
      get(`${api}/tracks`);
      backend.expectOne(`${api}/tracks`).error(new ProgressEvent('error'), { status: 0 });

      expect(notifier.message()).toContain('Cannot reach the server');
    });

    it('leaves the banner alone when the caller handles the error', () => {
      const result = get(`${api}/tracks`, new HttpContext().set(HANDLES_ERRORS, true));
      backend
        .expectOne(`${api}/tracks`)
        .flush(
          { title: 'Conflict', detail: 'Duplicate ISRC.' },
          { status: 409, statusText: 'Conflict' },
        );

      expect(notifier.message()).toBeNull();
      expect((result.error as ApiError).message).toBe('Duplicate ISRC.');
    });

    it('logs out when a request carrying a token gets 401', () => {
      logIn();
      get(`${api}/tracks`);
      backend
        .expectOne(`${api}/tracks`)
        .flush({ title: 'Unauthorized', status: 401 }, { status: 401, statusText: 'Unauthorized' });

      expect(auth.isLoggedIn()).toBe(false);
    });
  });
});
