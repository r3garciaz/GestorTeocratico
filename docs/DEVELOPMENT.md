# Development

## Prerequisites

| Tool | Version | Notes |
|---|---|---|
| .NET SDK | 10.0+ | project targets `net10.0` |
| Node.js + npm | modern LTS | for Tailwind CSS v4 |
| PostgreSQL | 17 | or use Docker |
| Docker + Compose | latest | optional but recommended |
| `dotnet-ef` | 10.x | `dotnet tool install --global dotnet-ef` |

## Local setup

```bash
git clone https://github.com/r3garciaz/GestorTeocratico.git
cd GestorTeocratico

# front-end tooling (Tailwind)
npm install

# configuration
cp .env.example .env        # edit values (see below)

# database (either a local PostgreSQL or the compose one)
dotnet ef database update --project GestorTeocratico

# run
dotnet run --project GestorTeocratico
# http(s) per launchSettings: https://localhost:7095, http://localhost:5265
```

Migrations and seeding run automatically at startup, so the manual `database update` is optional
(but useful to fail fast).

### Tailwind

```bash
npm run watch:css    # during development
npm run build:css    # before committing (minified -> wwwroot/css/app.css)
```

`wwwroot/css/app.css` is generated; do not hand-edit it.

## Configuration

`appsettings.json` holds only logging defaults. Secrets/config come from environment variables
(or user secrets — `UserSecretsId` is set in the csproj).

### Environment variables

| Variable | Required | Purpose |
|---|---|---|
| `ConnectionStrings__DefaultConnection` | ✅ | Npgsql connection string. Startup throws if missing. |
| `ASPNETCORE_ENVIRONMENT` | ✅ | `Development` / `Production` |
| `RESEND__APITOKEN` | ❌ | Enables the real email sender; otherwise no-op |
| `RESEND__FROMEMAIL` | ❌ | From address for emails |
| `AUTHENTICATION__SCHEMES__GOOGLEOIDC__CLIENTID` | ✅* | Google OAuth client id |
| `AUTHENTICATION__SCHEMES__GOOGLEOIDC__CLIENTSECRET` | ✅* | Google OAuth client secret |
| `EMERGENCYADMIN__EMAIL` | ❌ | Break-glass local admin email |
| `EMERGENCYADMIN__PASSWORD` | ❌ | Break-glass local admin password |

\* Required at startup unless you only use the emergency local admin; the app throws when the
Google options are missing from configuration.

`.env.example` (database + connection + email + Google + emergency admin) is the reference; note
it uses `CONNECTIONSTRINGS__DEFAULTCONNECTION`.

### PostgreSQL

`compose.yaml` starts `postgres:17` and maps host **54332** → container `POSTGRES_PORT`:

```yaml
database:
  image: postgres:17
  ports:
    - "54332:${POSTGRES_PORT}"
```

So a host-side connection uses port **54332** when running the DB via compose.

## Docker

```bash
docker compose up --build     # app on http://localhost:8080, DB on host :54332
```

- `GestorTeocratico/Dockerfile` is multi-stage (.NET 10 SDK build → ASP.NET 10 runtime).
- `compose.yaml` builds `gestor-teocratico`, depends on `database`, and reads both services'
  env from `.env`.
- `.dockerignore` excludes build artifacts, node_modules, `.env`, etc.

## Migrations

```bash
dotnet ef migrations add <Name> --project GestorTeocratico
dotnet ef database update --project GestorTeocratico
```

- Applied automatically at startup in both Development and Production.
- The dev exception page (`UseMigrationsEndPoint`) is enabled in Development.
- After changing `ApplicationDbContext`, add a migration and verify the generated file.

## Seeding

| Environment | Seeder | Content |
|---|---|---|
| Development | `DataSeederDevelopment` | Sample congregation, departments, responsibilities, 31 publishers, qualifications, roles |
| Production | `DataSeeder` | Congregation, core departments/responsibilities, roles |
| Both | `DataSeeder.SeedEmergencyAdminAsync` | Break-glass admin if configured |

Seeders are idempotent (fixed GUIDs / existence checks), so restarting is safe.

> In Development, every publisher is qualified for every responsibility to make manual testing
> easier.

## Tests

```bash
dotnet test
```

Tests live in `GestorTeocratico.Tests` (xUnit + SQLite in-memory) and cover the slice handlers;
see [TESTING.md](./TESTING.md).

## CI

`.github/workflows/docker-build.yml` builds a multi-arch image and pushes to Docker Hub
(`r3garciaz/gestor-teocratico`) on pushes to `main` (build only on PRs). It requires
`DOCKER_USERNAME` / `DOCKER_PASSWORD` secrets. It does **not** run the tests yet — run
`dotnet test` locally (or add a test step) — see [TESTING.md](./TESTING.md).

## Repository conventions

- Default branch: `main`. Active work happens on feature branches (e.g. `feat/...`).
- Commits follow **Conventional Commits** with Spanish descriptions, e.g.
  `feat(ui): ...`, `fix(meeting-schedules): ...`, `refactor(ui): ...`.

## Related docs

- Stack/rules: [`../AGENTS.md`](../AGENTS.md)
- Troubleshooting: [TROUBLESHOOTING.md](./TROUBLESHOOTING.md)
