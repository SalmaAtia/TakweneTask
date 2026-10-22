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
  core/auth/               AuthService (login, token in localStorage), authInterceptor
  core/http/               errorInterceptor, ApiError (ProblemDetails), ErrorNotifier (banner)
  tracks/                  models matching the API DTOs, TrackService, page components
```

- `authInterceptor` adds `Authorization: Bearer <token>` to requests for `apiBaseUrl` only.
- `errorInterceptor` turns any failed response into an `ApiError` (message from ProblemDetails `detail`, validation
  `errors`, or `title`), shows it in the banner and rethrows it. A form that shows errors itself passes
  `new HttpContext().set(HANDLES_ERRORS, true)` to skip the banner. A 401 on a request that carried a token logs out.
