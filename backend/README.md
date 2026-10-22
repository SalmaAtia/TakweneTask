# Track Management API

## Authentication (JWT)

All POST/PATCH endpoints require a JWT; GETs are public. There is a single demo account:

| Username | Password |
| --- | --- |
| `Admin` | `123456` |

The username is case-sensitive (`admin` is rejected).

> These are demo credentials for local review only. The password itself is never stored, only its hash.

### 1. One-time setup (each machine)

The credentials and signing key are not in `appsettings.json` (it only holds empty placeholders); they live in
[user-secrets](https://learn.microsoft.com/aspnet/core/security/app-secrets). The API refuses to start until they are set.
Run from `backend/`:

```bash
# Demo user name
dotnet user-secrets set "DemoUser:Username" "Admin" --project src/TrackManagement.Api

# Demo user password hash: the helper reads the password from stdin and prints an ASP.NET Core Identity hash
HASH=$(echo '123456' | dotnet run --project src/TrackManagement.Api -- --hash-password 2>/dev/null)
dotnet user-secrets set "DemoUser:PasswordHash" "$HASH" --project src/TrackManagement.Api

# Token signing key (at least 32 bytes; this generates 64 random bytes)
dotnet user-secrets set "Jwt:SigningKey" "$(openssl rand -base64 64 | tr -d '\n')" --project src/TrackManagement.Api

# Check
dotnet user-secrets list --project src/TrackManagement.Api
```

To use a different password, run `dotnet run --project src/TrackManagement.Api -- --hash-password`, type the password
at the prompt, and store the printed hash with the `DemoUser:PasswordHash` command above.

### 2. Log in

Start the API (`dotnet run --project src/TrackManagement.Api`), then:

```bash
curl -X POST http://localhost:5206/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"Admin","password":"123456"}'
```

Response:

```json
{ "accessToken": "eyJhbGciOi...", "expiresAt": "2026-10-22T13:28:07+00:00" }
```

A wrong username or password returns `401` with `"Invalid username or password."`; empty fields return `400`.

### 3. Call protected endpoints

Send the token in the `Authorization` header:

```bash
curl -X POST http://localhost:5206/api/artists \
  -H "Authorization: Bearer <accessToken>" \
  -H "Content-Type: application/json" \
  -d '{"name":"...","email":"...","country":"..."}'
```

Without a valid token, POST/PATCH endpoints return `401`. The token expires after `Jwt:ExpiryMinutes` (default 60);
log in again to get a new one.

**In Swagger** (`http://localhost:5206/swagger`): call `POST /api/auth/login`, copy `accessToken`, click **Authorize**,
paste the token (without the `Bearer ` prefix) and confirm. Endpoints with a padlock now send it automatically.

### Settings

| Key | Where | Default | Notes |
| --- | --- | --- | --- |
| `DemoUser:Username` | user-secrets | (empty) | Required |
| `DemoUser:PasswordHash` | user-secrets | (empty) | Required, `PasswordHasher` hash |
| `Jwt:SigningKey` | user-secrets | (empty) | Required, at least 32 bytes |
| `Jwt:Issuer` | `appsettings.json` | `TrackManagement.Api` | Validated on every request |
| `Jwt:Audience` | `appsettings.json` | `TrackManagement.Client` | Validated on every request |
| `Jwt:ExpiryMinutes` | `appsettings.json` | `60` | Lifetime validated with 30 s clock skew |
| `Cors:AllowedOrigin` | `appsettings.json` | `http://localhost:4200` | The only allowed origin (Angular dev server) |

User-secrets are only loaded in the `Development` environment. Elsewhere, use environment variables
(`DemoUser__Username`, `DemoUser__PasswordHash`, `Jwt__SigningKey`).

## Tests

```bash
dotnet test
```

`tests/TrackManagement.Tests` (xUnit) contains:

- **Domain unit tests** (`Domain/`): `Track` status transitions and distribution rules, and the `Isrc`, `Email` and
  `CountryCode` value objects. No database, no mocks.
- **Domain manager tests** (`Domain/TrackManagerTests.cs`): `TrackManager` against in-memory fakes of the repository
  interfaces (`Fakes/`).
- **API integration tests** (`Integration/`): the real API via `WebApplicationFactory`, against SQL Server in Docker
  ([Testcontainers](https://dotnet.testcontainers.org/)). **Docker must be running.** The first run pulls
  `mcr.microsoft.com/mssql/server:2022-CU14-ubuntu-22.04` (~1.5 GB). The tests run under the `Testing`
  environment with their own JWT key and demo user, so they don't need your user-secrets.

Run only the fast tests (no Docker): `dotnet test --filter "FullyQualifiedName!~Integration"`.

**Where the integration tests run:** the SQL Server image is published for Linux x86-64 (amd64) only, and needs about
2 GB of memory for the container.

| Machine | Integration tests |
| --- | --- |
| Linux x86-64, Intel Mac, Windows (Docker Desktop with WSL2 / Linux containers) | Supported |
| Apple Silicon Mac | Only with Docker Desktop → Settings → General → **Use Rosetta for x86_64/amd64 emulation** (slower) |
| Linux ARM64, no Docker | Not supported; run the fast tests only (command above) |
