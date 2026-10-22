import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { Component } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { Router, provideRouter, withComponentInputBinding } from '@angular/router';
import { RouterTestingHarness } from '@angular/router/testing';
import { environment } from '../../environments/environment';
import { AuthService } from '../core/auth/auth.service';
import { ErrorNotifier } from '../core/http/error-notifier';
import { errorInterceptor } from '../core/http/error.interceptor';
import { Login, safeReturnUrl } from './login';

const loginUrl = `${environment.apiBaseUrl}/auth/login`;

@Component({ template: 'page' })
class PageStub {}

describe('Login', () => {
  let harness: RouterTestingHarness;
  let backend: HttpTestingController;

  beforeEach(async () => {
    sessionStorage.clear();
    TestBed.configureTestingModule({
      providers: [
        provideRouter(
          [
            { path: 'login', component: Login },
            { path: '**', component: PageStub },
          ],
          withComponentInputBinding(),
        ),
        provideHttpClient(withInterceptors([errorInterceptor])),
        provideHttpClientTesting(),
      ],
    });
    backend = TestBed.inject(HttpTestingController);
    harness = await RouterTestingHarness.create();
  });

  afterEach(() => backend.verify());

  const el = () => harness.routeNativeElement!;

  async function submit(username: string, password: string) {
    const [user, pass] = Array.from(el().querySelectorAll('input'));
    user.value = username;
    user.dispatchEvent(new Event('input'));
    pass.value = password;
    pass.dispatchEvent(new Event('input'));
    el().querySelector('form')!.dispatchEvent(new Event('submit'));
    return backend.expectOne(loginUrl);
  }

  it('logs in and returns to returnUrl', async () => {
    await harness.navigateByUrl('/login?returnUrl=%2Ftracks%2Ft1%3Fstatus%3DDraft');

    const req = await submit('Admin', '123456');
    expect(req.request.body).toEqual({ username: 'Admin', password: '123456' });
    req.flush({ accessToken: 'jwt', expiresAt: new Date(Date.now() + 60_000).toISOString() });

    await vi.waitFor(() => expect(TestBed.inject(Router).url).toBe('/tracks/t1?status=Draft'));
    expect(TestBed.inject(AuthService).isLoggedIn()).toBe(true);
  });

  it('shows wrong credentials inline, not in the banner', async () => {
    await harness.navigateByUrl('/login');

    (await submit('Admin', 'wrong')).flush(
      { status: 401, detail: 'Invalid username or password.' },
      { status: 401, statusText: 'Unauthorized' },
    );
    await harness.fixture.whenStable();

    expect(el().querySelector('.form-error')?.textContent).toBe('Invalid username or password.');
    expect(TestBed.inject(ErrorNotifier).message()).toBeNull();
    expect(TestBed.inject(Router).url).toBe('/login');
  });

  it('shows API validation errors under their field', async () => {
    await harness.navigateByUrl('/login');

    (await submit('', '')).flush(
      {
        title: 'One or more validation errors occurred.',
        status: 400,
        errors: {
          username: ["'Username' must not be empty."],
          password: ["'Password' must not be empty."],
        },
      },
      { status: 400, statusText: 'Bad Request' },
    );
    await harness.fixture.whenStable();

    const labels = el().querySelectorAll('label');
    expect(labels[0].nextElementSibling?.textContent).toContain("'Username' must not be empty.");
    expect(labels[1].nextElementSibling?.textContent).toContain("'Password' must not be empty.");
    // Field messages aren't repeated in the general slot.
    expect(el().querySelectorAll('.form-error')).toHaveLength(2);
  });
});

describe('safeReturnUrl', () => {
  it.each([
    ['/tracks/1?status=Draft', '/tracks/1?status=Draft'],
    [undefined, '/'],
    ['https://evil.example', '/'],
    ['//evil.example', '/'],
    ['/login?returnUrl=%2F', '/'],
  ])('%s -> %s', (input, expected) => {
    expect(safeReturnUrl(input)).toBe(expected);
  });
});
