import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { environment } from '../../../environments/environment';
import { AuthService } from './auth.service';

const STORAGE_KEY = 'trackManagement.auth';
const inAMinute = () => new Date(Date.now() + 60_000).toISOString();

describe('AuthService', () => {
  beforeEach(() => {
    sessionStorage.clear();
    localStorage.clear();
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
  });

  it('keeps the token in memory and sessionStorage (never localStorage) after login', () => {
    const auth = TestBed.inject(AuthService);

    auth.login({ username: 'Admin', password: '123456' }).subscribe();
    TestBed.inject(HttpTestingController)
      .expectOne(`${environment.apiBaseUrl}/auth/login`)
      .flush({ accessToken: 'jwt', expiresAt: inAMinute() });

    expect(auth.isLoggedIn()).toBe(true);
    expect(auth.token()).toBe('jwt');
    expect(JSON.parse(sessionStorage.getItem(STORAGE_KEY)!).accessToken).toBe('jwt');
    expect(localStorage.length).toBe(0);
  });

  it('restores the session from sessionStorage (a reload of the same tab)', () => {
    sessionStorage.setItem(
      STORAGE_KEY,
      JSON.stringify({ accessToken: 'jwt', expiresAt: inAMinute() }),
    );

    expect(TestBed.inject(AuthService).token()).toBe('jwt');
  });

  it('ignores an expired or corrupt stored session', () => {
    sessionStorage.setItem(STORAGE_KEY, '{not json');
    expect(TestBed.inject(AuthService).isLoggedIn()).toBe(false);
  });

  it('clears memory and sessionStorage on logout', () => {
    sessionStorage.setItem(
      STORAGE_KEY,
      JSON.stringify({ accessToken: 'jwt', expiresAt: inAMinute() }),
    );
    const auth = TestBed.inject(AuthService);

    auth.logout();

    expect(auth.isLoggedIn()).toBe(false);
    expect(sessionStorage.getItem(STORAGE_KEY)).toBeNull();
  });
});
