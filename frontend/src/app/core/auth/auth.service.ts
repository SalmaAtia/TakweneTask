import { HttpClient } from '@angular/common/http';
import { Injectable, computed, inject, signal } from '@angular/core';
import { Observable, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { LoginRequest, LoginResponse } from './auth.models';

const STORAGE_KEY = 'trackManagement.auth';

/** Holds the JWT from POST /api/auth/login, persisted in localStorage so a reload keeps the session. */
@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly session = signal<LoginResponse | null>(readStoredSession());

  /** Whether a token is held. It may still expire before the next request; see {@link token}. */
  readonly isLoggedIn = computed(() => this.session() !== null);

  login(request: LoginRequest): Observable<LoginResponse> {
    return this.http
      .post<LoginResponse>(`${environment.apiBaseUrl}/auth/login`, request)
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
      if (session) localStorage.setItem(STORAGE_KEY, JSON.stringify(session));
      else localStorage.removeItem(STORAGE_KEY);
    } catch {
      // Storage unavailable (private mode, blocked): the session just won't survive a reload.
    }
  }
}

function readStoredSession(): LoginResponse | null {
  try {
    const raw = localStorage.getItem(STORAGE_KEY);
    const session = raw ? (JSON.parse(raw) as LoginResponse) : null;
    return session?.accessToken && Date.parse(session.expiresAt) > Date.now() ? session : null;
  } catch {
    return null;
  }
}
