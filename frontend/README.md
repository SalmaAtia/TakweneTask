# Track Management – Frontend

Angular 22 SPA (standalone components, signals, zoneless, plain CSS). Talks to the API in `../backend`.

## Prerequisites

Node.js `^22.22.3` or `>=24.15` (Angular 22's requirement). With nvm: `nvm install 22`.

## Run

```bash
npm install
npm start          # http://localhost:4200
```

Start the API first (`dotnet run --project src/TrackManagement.Api` from `../backend`, http profile on port 5206).
The API only allows CORS from `http://localhost:4200`.

## Test / build

```bash
npm test -- --watch=false   # Vitest
npm run build               # production build in dist/
```

## Configuration

| File | `apiBaseUrl` | Used by |
| --- | --- | --- |
| `src/environments/environment.development.ts` | `http://localhost:5206/api` | `ng serve`, development builds |
| `src/environments/environment.ts` | `/api` (same origin) | production build |

## Structure

```
src/app/
  app.ts / app.html        layout shell: header, global error banner, <router-outlet>
  app.routes.ts            '' -> TrackList, 'tracks/:id' -> TrackDetail (lazy)
  core/auth/               AuthService (login, token in memory + sessionStorage), authInterceptor
  core/http/               errorInterceptor, ApiError (ProblemDetails), ErrorNotifier (banner)
  core/http/form-errors.ts inline API error messages for forms
  login/                   login page (?returnUrl=)
  dsps/                    DspService (DSP list for the distribute form)
  tracks/                  models matching the API DTOs, TrackService, list/detail pages,
                           track-actions/ (Distribute, Change status, Mark live / Reject per Pending
                           distribution; shown when logged in)
```

- `authInterceptor` adds `Authorization: Bearer <token>` to requests for `apiBaseUrl` only.
- `errorInterceptor` turns any failed response into an `ApiError` (message from ProblemDetails `detail`, validation
  `errors`, or `title`), shows it in the banner and rethrows it. A form that shows errors itself passes
  `new HttpContext().set(HANDLES_ERRORS, true)` to skip the banner. A 401 on a request that carried a token logs out.

## Authentication

Log in at `/login` (demo account: see `../backend/README.md`). POST/PATCH actions on the track detail page
appear only when logged in; the header has Log in / Log out.

The token lives in memory (a signal read by `authInterceptor`) and is mirrored to **sessionStorage**, not
localStorage:

- Both are readable by any script on the page, so neither protects the token from XSS; that would take an
  httpOnly cookie set by the API.
- sessionStorage is scoped to one tab and cleared when the tab closes: a reload keeps you logged in, but the token
  doesn't linger on a shared machine or spread to other tabs.
- The cost: each new tab, and a restarted browser, needs a new login. localStorage would keep the session across
  tabs and restarts until the token expires (`Jwt:ExpiryMinutes`, 60 by default).
