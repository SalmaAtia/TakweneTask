# Takwene Track Management

Full-stack take-home: a .NET 8 Web API (Clean Architecture, DDD) and an Angular 22 SPA for managing music tracks,
their artists and their distribution to DSPs (Spotify, Apple Music, YouTube Music).

```
backend/    .NET 8 solution: Shared, Domain, Application, Infrastructure, Api + tests
frontend/   Angular 22 app (standalone components, signals)
docker-compose.yml        SQL Server for local development
docker-compose.prod.yml   Production: SQL Server + API + Caddy (HTTPS), see Deployment
```

## Prerequisites

| Tool | Version | Check |
| --- | --- | --- |
| .NET SDK | 8.0.425 or a later 8.0 SDK (`backend/global.json`) | `dotnet --version` |
| Node.js | `^22.22.3` or `>=24.15` (Angular 22) | `node -v` |
| Docker | Docker Desktop or Docker Engine with Compose v2 | `docker compose version` |

Docker is only needed for SQL Server (and the integration tests). On Windows you can use LocalDB instead, see
[below](#alternative-localdb-windows-no-docker).

> The SQL Server image is linux/amd64 only. On an Apple Silicon Mac, turn on Docker Desktop → Settings → General →
> **Use Rosetta for x86_64/amd64 emulation**.

## Quick start

Run everything from the repository root unless a step says otherwise. Start step 1 first: the first pull of the SQL
Server image (~1.5 GB) is the slowest part. Step 2 and the frontend's `npm ci` (step 5) can run while it downloads;
step 3 needs the database to be up. With the image already downloaded, steps 1–5 take about two minutes.

### 1. Start SQL Server

```bash
docker compose up -d --wait
```

`--wait` returns once the database accepts connections. It listens on `localhost,1433` with user `sa` and password
`123456a@` (a local development password; override with `MSSQL_SA_PASSWORD=... docker compose up -d --wait`, or use
another host port with `SQL_PORT=14333`). Data is kept in the `sqlserver-data` volume.

### 2. Configure the API

```bash
cd backend

# Connection string (matches docker-compose.yml). appsettings.Development.json is gitignored.
cp src/TrackManagement.Api/appsettings.Development.example.json src/TrackManagement.Api/appsettings.Development.json

# Demo user: Admin / 123456. The password is stored only as an ASP.NET Core Identity hash.
dotnet user-secrets set "DemoUser:Username" "Admin" --project src/TrackManagement.Api
dotnet user-secrets set "DemoUser:PasswordHash" "AQAAAAIAAYagAAAAEEXdJoFgkLsFx3wB7RZCALUe+su9lSvL32fiPyLPXVtSCekguCiaBoiHrbXQ5CY/gw==" --project src/TrackManagement.Api

# JWT signing key: random, at least 32 bytes
dotnet user-secrets set "Jwt:SigningKey" "$(openssl rand -base64 48)" --project src/TrackManagement.Api
```

<details>
<summary>Windows PowerShell</summary>

```powershell
cd backend
Copy-Item src/TrackManagement.Api/appsettings.Development.example.json src/TrackManagement.Api/appsettings.Development.json
dotnet user-secrets set "DemoUser:Username" "Admin" --project src/TrackManagement.Api
dotnet user-secrets set "DemoUser:PasswordHash" "AQAAAAIAAYagAAAAEEXdJoFgkLsFx3wB7RZCALUe+su9lSvL32fiPyLPXVtSCekguCiaBoiHrbXQ5CY/gw==" --project src/TrackManagement.Api
dotnet user-secrets set "Jwt:SigningKey" ([Convert]::ToBase64String([Security.Cryptography.RandomNumberGenerator]::GetBytes(48))) --project src/TrackManagement.Api
```

</details>

The API refuses to start until all three secrets are set. Check them with
`dotnet user-secrets list --project src/TrackManagement.Api`. Secrets live outside the repository
(`~/.microsoft/usersecrets/<UserSecretsId>/secrets.json`) and are only loaded in the `Development` environment.

To use a different demo password: `dotnet run --project src/TrackManagement.Api -- --hash-password`, type the password,
and store the printed hash with the `DemoUser:PasswordHash` command above.

### 3. Create the database

Still in `backend/`:

```bash
dotnet tool restore   # installs dotnet-ef 8.0.11 from .config/dotnet-tools.json
dotnet ef database update --project src/TrackManagement.Infrastructure --startup-project src/TrackManagement.Api
```

This creates the `TrackManagement` database and seeds it: 4 artists, 3 DSPs and 10 tracks in every status, some
with Pending, Live and Rejected distributions.

### 4. Run the API

```bash
dotnet run --project src/TrackManagement.Api --launch-profile http
```

- API: http://localhost:5206 (health check: http://localhost:5206/health)
- Swagger UI: http://localhost:5206/swagger

### 5. Run the Angular app

In a second terminal, from the repository root:

```bash
cd frontend
npm ci
npm start
```

Open http://localhost:4200. The dev build calls the API at `http://localhost:5206/api`, the only origin the API's CORS
policy allows is `http://localhost:4200`, so keep both ports as they are.

## Demo credentials

| Username | Password |
| --- | --- |
| `Admin` | `123456` |

The username is case-sensitive. Reading (all `GET`s) needs no login; creating and changing data does.

In the app: **Log in** (top right), then open any track to **Distribute** it, **Mark live** / **Reject** a pending
distribution, or **Change status**. A full walk-through: open a Draft track, distribute it to a DSP, mark that
distribution live, then change the track's status to Distributed.

## Getting a token

### With curl

```bash
curl -s -X POST http://localhost:5206/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"Admin","password":"123456"}'
```

```json
{ "accessToken": "eyJhbGciOi...", "expiresAt": "2026-10-22T13:28:07+00:00" }
```

Use it on protected endpoints:

```bash
TOKEN=$(curl -s -X POST http://localhost:5206/api/auth/login -H "Content-Type: application/json" \
  -d '{"username":"Admin","password":"123456"}' | sed -E 's/.*"accessToken":"([^"]+)".*/\1/')

curl -X POST http://localhost:5206/api/tracks \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"title":"My Track","artistId":"a0000000-0000-0000-0000-000000000001","isrc":"EG-ABC-26-00001","releaseDate":"2026-12-01","genre":"Pop"}'
```

Tokens expire after 60 minutes (`Jwt:ExpiryMinutes`); log in again for a new one. A wrong username or password returns
`401` with `"Invalid username or password."`.

### With Swagger

1. Open http://localhost:5206/swagger and expand **POST /api/auth/login** → **Try it out**.
2. Send `{ "username": "Admin", "password": "123456" }` and copy `accessToken` from the response.
3. Click **Authorize** (top right), paste the token **without** the `Bearer ` prefix, and confirm.
4. Endpoints with a padlock now send the token automatically.

## API endpoints

All endpoints are under `http://localhost:5206`. Errors are RFC 7807 ProblemDetails: `400` validation (with per-field
`errors`), `401` missing/invalid token, `404` not found, `409` conflict with the current state (duplicate ISRC or
email, invalid status change, DSP already present), `422` other domain rule violations, `429` too many login
attempts.

| Method | Path | Auth | Description |
| --- | --- | --- | --- |
| `POST` | `/api/auth/login` | – | Log in; returns `{ accessToken, expiresAt }`. Rate limited: 5 attempts per minute per IP |
| `GET` | `/api/artists` | – | All artists, by name |
| `GET` | `/api/artists/{id}` | – | One artist |
| `POST` | `/api/artists` | JWT | Create an artist `{ name, email, country }` (email unique, country ISO alpha-2) |
| `GET` | `/api/dsps` | – | All DSPs, by name |
| `GET` | `/api/tracks?artistId=&genre=&status=` | – | Tracks, newest release first; all filters optional (`status`: `Draft`, `Submitted`, `Distributed`) |
| `GET` | `/api/tracks/{id}` | – | One track with its distributions |
| `POST` | `/api/tracks` | JWT | Create a Draft track `{ title, artistId, isrc, releaseDate, genre }` (ISRC unique, hyphens optional) |
| `POST` | `/api/tracks/{id}/distribute` | JWT | Distribute to DSPs `{ dspIds: [...] }`: adds Pending distributions; Draft → Submitted |
| `PATCH` | `/api/tracks/{id}/distributions/{dspId}` | JWT | Record the DSP's answer for a Pending distribution `{ status: "Live" \| "Rejected" }` |
| `PATCH` | `/api/tracks/{id}/status` | JWT | Change status `{ status }`: Draft → Submitted, Submitted → Draft, Submitted → Distributed (needs a Live distribution) |
| `GET` | `/health` | – | Health check |

Seeded ids for trying things out:

| | Id |
| --- | --- |
| Artist Nour El Sayed | `a0000000-0000-0000-0000-000000000001` |
| DSP Spotify / Apple Music / YouTube Music | `d0000000-0000-0000-0000-000000000001` / `…0002` / `…0003` |
| Track Tok Tok Anthem (Submitted, 2 Pending) | `b0000000-0000-0000-0000-000000000002` |
| Track Cairo Concrete (Draft) | `b0000000-0000-0000-0000-000000000003` |

## Alternative: LocalDB (Windows, no Docker)

Skip step 1. In step 2, instead of copying the example file, point the connection string at LocalDB (installed with
Visual Studio, or the SQL Server Express LocalDB installer):

```powershell
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=(localdb)\MSSQLLocalDB;Database=TrackManagement;Trusted_Connection=True;TrustServerCertificate=True" --project src/TrackManagement.Api
```

User-secrets override `appsettings.Development.json`, so this works whether or not you copied the example file. Then
continue with step 3. (The integration tests still need Docker; see below.)

## Tests

From the repository root:

```bash
dotnet test backend                                                    # all; integration tests need Docker
dotnet test backend --filter "FullyQualifiedName!~Integration"         # domain/manager tests only, no Docker
npm --prefix frontend test -- --watch=false                            # Angular (Vitest)
```

The backend integration tests start their own throwaway SQL Server with Testcontainers and their own JWT key and demo
user, so they don't use the database or the user-secrets above.

## Troubleshooting

| Symptom | Fix |
| --- | --- |
| API exits with `Jwt:SigningKey is required` / `DemoUser:...` | A secret is missing: redo step 2, check with `dotnet user-secrets list` |
| `Connection string 'DefaultConnection' is not configured` | `appsettings.Development.json` is missing (step 2), or the API isn't running in `Development` (use `--launch-profile http`) |
| `A network-related or instance-specific error ... SQL Server` | SQL Server isn't up: `docker compose ps` should show `healthy`; check `docker compose logs sqlserver` |
| `Login failed for user 'sa'` | The password in `appsettings.Development.json` doesn't match `MSSQL_SA_PASSWORD` |
| Port 1433 already in use | `SQL_PORT=14333 docker compose up -d --wait` and change `localhost,1433` to `localhost,14333` in `appsettings.Development.json` |
| The app shows "Cannot reach the server" | The API isn't running on port 5206 (step 4) |
| Stop and reset the database | `docker compose down -v` (removes the volume), then repeat step 3 |

## Deployment

`docker-compose.prod.yml` runs the whole app on one Linux server:

| Service | Image | Role |
| --- | --- | --- |
| `db` | `mcr.microsoft.com/mssql/server:2022-latest` (Express edition) | Database. Named volume `sqlserver-data`, **no published ports**, on an internal network with no internet access |
| `migrator` | `backend/Dockerfile`, target `migrator` | One-off: applies pending migrations, then exits |
| `api` | `backend/Dockerfile` | The API on port 8080, as the non-root `app` user. Reachable only through Caddy |
| `web` | `frontend/Dockerfile` | Caddy: serves the Angular production build, proxies `/api/*`, `/swagger` and `/health` to `api`, and gets a Let's Encrypt certificate for `SITE_HOST` |

The Angular production build calls the API at `/api` on its own origin, so the browser never makes a cross-origin
request and the API needs no CORS setup in production.

### Server requirements

- Linux **x86-64** (SQL Server has no ARM64 image), at least **2 vCPU and 4 GB RAM** (SQL Server alone needs 2 GB),
  10 GB free disk.
- Docker Engine with the Compose plugin (v2.20 or later): `docker compose version`.
- Ports **80 and 443** (TCP, plus 443/UDP for HTTP/3) open to the internet. Let's Encrypt validates on port 80.
- A hostname that resolves to the server. Without a domain, use [sslip.io](https://sslip.io): for the IP
  `203.0.113.10`, `203-0-113-10.sslip.io` already resolves to it, nothing to register.

### Deploy

```bash
git clone https://github.com/SalmaAtia/TakweneTask.git
cd TakweneTask
cp .env.example .env && chmod 600 .env
```

Fill in `.env` (it is gitignored; `.env.example` has only placeholders):

| Variable | Value |
| --- | --- |
| `SITE_HOST` | e.g. `203-0-113-10.sslip.io` |
| `MSSQL_SA_PASSWORD` | `openssl rand -base64 24 \| tr -d '/+='` (no `;` `'` `"` `$`: it goes into a connection string) |
| `JWT_SIGNING_KEY` | `openssl rand -base64 48` |
| `DEMO_USERNAME` | `Admin` |
| `DEMO_PASSWORD_HASH` | For the public demo password `123456`: the hash in [Quick start step 2](#2-configure-the-api). For another password, see below |
| `SWAGGER_ENABLED` | `true` (see [Swagger in production](#swagger-in-production)) |

Then build and start everything:

```bash
docker compose -f docker-compose.prod.yml up -d --build --wait
```

The first run pulls the base images and builds the API and the Angular app (a few minutes). Compose then starts
`db`, waits until it is healthy, runs `migrator`, starts `api` once the migrator has exited successfully and `/health`
answers, and finally `web`. Open `https://<SITE_HOST>`; Swagger is at `https://<SITE_HOST>/swagger`.

To hash a different demo password (no .NET SDK needed on the server):

```bash
docker build -t trackmanagement-api ./backend
echo 'your-password' | docker run --rm -i trackmanagement-api --hash-password 2>/dev/null
```

Check and troubleshoot:

```bash
docker compose -f docker-compose.prod.yml ps -a                # migrator: Exited (0); the rest: healthy
docker compose -f docker-compose.prod.yml logs migrator        # applied migrations
docker compose -f docker-compose.prod.yml logs -f api web      # API errors, certificate issuance
```

If the certificate fails, `SITE_HOST` doesn't resolve to this server or port 80 is blocked (check the cloud firewall /
security group as well as the server's own).

### Update

```bash
git pull
docker compose -f docker-compose.prod.yml up -d --build --wait
```

Compose rebuilds the images, runs `migrator` again (it applies only the new migrations; with none it logs "No
migrations were applied") and recreates `api` and `web`. The database and the certificates are in named volumes and
survive updates. There is a short downtime while `api` restarts (single instance).

Other operations:

| Task | Command |
| --- | --- |
| Stop (keep data) | `docker compose -f docker-compose.prod.yml down` |
| Change a secret in `.env` | Edit it, then `docker compose -f docker-compose.prod.yml up -d` (recreates what uses it) |
| Back up the database | See below |
| Delete everything, including data | `docker compose -f docker-compose.prod.yml down -v` |

Back up the database to `./TrackManagement.bak` (the password is read inside the container):

```bash
docker compose -f docker-compose.prod.yml exec db sh -c '/opt/mssql-tools18/bin/sqlcmd -S localhost -U sa \
  -P "$MSSQL_SA_PASSWORD" -C -b -Q "BACKUP DATABASE TrackManagement TO DISK = N'"'"'/var/opt/mssql/TrackManagement.bak'"'"' WITH INIT"'
docker compose -f docker-compose.prod.yml cp db:/var/opt/mssql/TrackManagement.bak .
```

### Why a migration bundle (and not `Database.Migrate()` at startup)

Migrations run as a separate step: `dotnet ef migrations bundle` builds a single executable (`efbundle`) in
the API image's build stage, and the `migrator` service runs it once per deploy.

- **A failed migration stops the deploy, not the app in a loop.** The API waits for
  `service_completed_successfully`, so it never starts against a half-migrated schema; the error is in
  `logs migrator`. With `Migrate()` in `Program.cs`, a failure becomes a crash/restart loop of the API.
- **No race between instances.** Migrating at startup runs once per instance; two replicas starting together both
  try to migrate. One migrator per deploy avoids that if the API is ever scaled out.
- **The app doesn't need schema rights to run.** The step that changes the schema is separate from the long-running
  process, so the API's login could later be restricted to read/write (here both use `sa`; see below).
- **No `dotnet ef` or SDK on the server.** The bundle is built inside Docker with the exact migrations of that
  commit, and it is idempotent: it applies only what `__EFMigrationsHistory` doesn't have yet.

### Swagger in production

Swagger UI is normally only on in `Development`. Here it is **turned on deliberately** (`SWAGGER_ENABLED=true`, read
as `Swagger:Enabled`) so reviewers can try the API at `https://<SITE_HOST>/swagger` without any tools. It exposes
nothing beyond what the API already serves: GETs are public anyway, and every write still needs a JWT. Set
`SWAGGER_ENABLED=false` and run `up -d` to hide it.

### Login rate limiting

Because the demo credentials are public, `POST /api/auth/login` is limited to **5 attempts per minute per client IP**
(ASP.NET Core's built-in rate limiter, fixed window); further attempts get `429` with a `Retry-After` header and a
ProblemDetails body, which the login page shows. Successful logins count too. The client IP comes from
`X-Forwarded-For`, set by Caddy (the API trusts it via `ASPNETCORE_FORWARDEDHEADERS_ENABLED`; the API is reachable
only through Caddy, and Caddy replaces any `X-Forwarded-For` a client sends). Tune it with `RateLimiting__Login__PermitLimit`
and `RateLimiting__Login__WindowSeconds` on the `api` service.

## More

- `backend/README.md`: authentication settings in detail, test project layout, platform notes for the tests.
- `frontend/README.md`: app structure, interceptors, error handling, why the token is kept in sessionStorage.
