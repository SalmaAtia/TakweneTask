import { HttpClient, HttpContext } from '@angular/common/http';
import { Injectable, computed, inject, signal } from '@angular/core';
import { Observable, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { LoginRequest, LoginResponse } from './auth.models';

const STORAGE_KEY = 'trackManagement.auth';

/**
 * Holds the JWT from POST /api/auth/login: in memory (a signal, read on every request) and mirrored
 * to sessionStorage so a reload of the same tab keeps the session.
 *
 * Why sessionStorage rather than localStorage: both are readable by any script on the page, so
 * neither protects the token from XSS (only an httpOnly cookie would). The difference is lifetime and
 * reach. sessionStorage is per tab and is cleared when the tab closes, so a token isn't left on a
 * shared machine and doesn't leak into other tabs. The cost is that every new tab (or a restarted
 * browser) needs a new login. localStorage would keep you logged in across tabs and restarts, until
 * the token expires.
 */
@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly session = signal<LoginResponse | null>(readStoredSession());

  /** Whether a token is held. It may still expire before the next request; see {@link token}. */
  readonly isLoggedIn = computed(() => this.session() !== null);

  /** Pass `new HttpContext().set(HANDLES_ERRORS, true)` when the caller shows the error itself. */
  login(request: LoginRequest, context?: HttpContext): Observable<LoginResponse> {
    return this.http
      .post<LoginResponse>(`${environment.apiBaseUrl}/auth/login`, request, { context })
      .pipe(tap((response) => this.setSession(response)));
  }

  logout(): void {
    this.setSession(null);
  }

  /** The current token, or null when there is none or it has expired (an expired one is dropped). */
  token(): string | null {
    const session = this.session();
    if (!session) return null;

    if (Date.parse(session.expiresAt) <= Date.now()) {
      this.logout();
      return null;
    }

    return session.accessToken;
  }

  private setSession(session: LoginResponse | null): void {
    this.session.set(session);
    try {
      if (session) sessionStorage.setItem(STORAGE_KEY, JSON.stringify(session));
      else sessionStorage.removeItem(STORAGE_KEY);
    } catch {
      // Storage unavailable (blocked): the session still works in memory, it just won't survive a reload.
    }
  }
}

function readStoredSession(): LoginResponse | null {
  try {
    const raw = sessionStorage.getItem(STORAGE_KEY);
    const session = raw ? (JSON.parse(raw) as LoginResponse) : null;
    return session?.accessToken && Date.parse(session.expiresAt) > Date.now() ? session : null;
  } catch {
    return null;
  }
}
