# AGENTS.md

Operating manual for AI coding agents working in **Gestor Teocrático**.

> Read this file before changing code. For depth, follow the links into [`docs/`](./docs/README.md).
> This file is the single source of truth for agent-facing rules; keep it short and current.

---

## 1. What this project is

Gestor Teocrático is a **Blazor Server (ASP.NET Core) web app** to manage a religious
congregation: **publishers**, **departments**, **responsibilities**, and **meeting
schedules** (week-by-week assignment of publishers to responsibilities). It authenticates
users with **Google OAuth** (plus an optional "break-glass" local admin), and exports the
monthly schedule as a **PDF**.

- **Domain language:** UI text, seed data, and email copy are in **Spanish (es-ES)**.
- **Code comments and identifiers:** English.
- The app models a **single congregation** (enforced in code — see invariants §4).

---

## 2. Current stack (verified)

| Concern | Technology | Version / notes |
|---|---|---|
| Runtime | .NET | **net10.0** (SDK 10) |
| Web | ASP.NET Core + Blazor | Interactive Server render mode |
| UI library | **Blazor Blueprint** (`BlazorBlueprint.Components`) | `4.1.0` — `Bb*` components |
| Icons | BlazorBlueprint.Icons.Lucide | `2.0.2` — `<LucideIcon Name="..."/>` |
| Styling | **Tailwind CSS v4** (npm) + shadcn CSS variables | `npm run build:css` |
| ORM | EF Core | `10.0.12` |
| DB | PostgreSQL 17 via `Npgsql.EntityFrameworkCore.PostgreSQL` | `10.0.3` |
| Identity | ASP.NET Core Identity (`Identity.EntityFrameworkCore`) + Google OAuth | `10.0.12` |
| Data Protection | `Microsoft.AspNetCore.DataProtection.EntityFrameworkCore` | keys persisted in DB |
| PDF | QuestPDF | `2025.7.0` (Community license) |
| Email | Resend | `0.1.5` (optional; falls back to a no-op sender) |
| Containers | Docker + Docker Compose | .NET 10 base images |
| Tests | xUnit + EF Core SQLite (in-memory) | `GestorTeocratico.Tests` |
| CI | GitHub Actions | builds & pushes image on `main` |

> The root `README.md` and the old `Shared/PRD.md` were historically stale. They have been
> corrected; if you find a fact that contradicts the code, **trust the code** and update the doc.

---

## 3. Commands

```bash
# .NET
dotnet restore                       # restore packages
dotnet build                         # build solution
dotnet test                          # run tests (GestorTeocratico.Tests, xUnit)
dotnet run --project GestorTeocratico # run the app (https://localhost:7095)
dotnet ef migrations add <Name> --project GestorTeocratico
dotnet ef database update --project GestorTeocratico

# Tailwind (required after adding/changing utility classes in .razor files)
npm install
npm run build:css                    # minified build -> wwwroot/css/app.css
npm run watch:css                    # watch mode during development

# Docker
docker compose up --build            # app on :8080, PostgreSQL on :54332
```

- Migrations and seeding run automatically at startup (Dev uses `DataSeederDevelopment`,
  Prod uses `DataSeeder`). See `docs/DEVELOPMENT.md`.
- Tests live in **`GestorTeocratico.Tests`** (xUnit + SQLite in-memory); run with `dotnet test`.
  See `docs/TESTING.md`.

---

## 4. Hard rules / invariants (do not break)

1. **Data access uses `IDbContextFactory<ApplicationDbContext>`.** Handlers never inject a
   scoped `ApplicationDbContext`. Always `await using var context = await _contextFactory.CreateDbContextAsync();`.
   Reason: the app runs long-lived Blazor Server circuits; a scoped context would be shared
   across concurrent circuit operations.
2. **Soft delete.** `Congregation`, `Department`, `Publisher`, `Responsibility` inherit
   `SoftDeleteEntity` (`IsDeleted`). Their `Delete*Handler` sets `IsDeleted = true` — never
   hard-delete these. `MeetingSchedule` and the join rows (`PublisherResponsibility`,
   `ResponsibilityAssignment`) are hard-deleted. Global query filters exclude deleted rows
   automatically (including on the join entities). Use `.IgnoreQueryFilters()` only in
   seeding/lookup as already done.
3. **Primary keys:** `Guid.CreateVersion7()` (UUIDv7). Identity uses composite keys:
   - `PublisherResponsibility` → `(PublisherId, ResponsibilityId)`
   - `ResponsibilityAssignment` → `(MeetingScheduleId, ResponsibilityId)` (`PublisherId` is a
     plain FK column — **one publisher per responsibility per meeting**, enforced by the PK)
4. **Single congregation.** `CreateCongregationHandler` throws `DuplicateEntityException` if a
   congregation already exists; a unique index on `Congregation.Name` backs it up. There is no
   delete path. Do not add multi-congregation CRUD.
5. **UI components:** only **Blazor Blueprint** `Bb*` components + Lucide icons + Tailwind
   utility classes. **Radzen and Bootstrap were removed — do not reintroduce them.**
6. **Authorization:** protect pages with `@attribute [Authorize]` or
   `[Authorize(Roles = "Admin, Manager")]`. A global `FallbackPolicy` requires an authenticated
   user on every endpoint; anonymous surfaces use `[AllowAnonymous]` (landing, not-found, error,
   account pages) or `.AllowAnonymous()` (static assets, login/logout). Define roles via
   `Shared.Roles` constants (`Admin`, `Manager`, `User`) — never hard-code role strings.
7. **UI language is Spanish.** Keep user-facing text in Spanish; keep code/comments in English.
8. **Tailwind scanning:** utility classes are only generated for paths listed in
   `wwwroot/css/app-input.css` (`@source`). If you add a folder with `.razor` files, add a
   `@source` entry, then run `npm run build:css`.

---

## 5. Repository map

```
GestorTeocratico/
├── Components/                 # Blazor UI
│   ├── Account/                # Identity pages + helpers (Google login, manage account)
│   ├── Layout/                 # MainLayout (sidebar shell), NavMenu, IndexLayout (public), AuthLayout (login)
│   ├── Pages/                  # Feature pages: Home(dashboard), Congregation, Department,
│   │                           #   Publisher, Responsibility, MeetingSchedule, Administration
│   ├── Ui/                     # Small shared components (EntityChips)
│   ├── App.razor / Routes.razor / _Imports.razor
├── Features/                   # Vertical slices, one folder per domain
│   ├── Common/                 # Shared domain exceptions (DuplicateEntityException)
│   ├── Congregations/ | Departments/ | Publishers/ | Responsibilities/
│   │                           #   each: Shared/ + one folder per use case (handler)
│   ├── MeetingSchedules/       # schedules + responsibility assignments (+ Endpoints/)
│   ├── PdfExport/ (+ Models/)  # document generation (consumes sliced handlers)
│   └── Roles/                  # thin adapter over RoleManager
├── Data/                       # ApplicationDbContext, ApplicationUser, seeders, Migrations/
├── Entities/                   # Domain models (+ SoftDeleteEntity)
├── Services/                   # Cross-cutting: ResendEmailSender, ResendOptions
├── Shared/                     # Roles.cs, Enums/  (PRD.md is a pointer stub)
├── wwwroot/                    # css (Tailwind in/out), styles/themes, images
└── Program.cs                  # Composition root (DI, auth, pipeline, seeding, endpoints)
```

Feature folder convention (vertical slice): a `Shared/` folder holds contracts (DTOs/records),
mappings and sync helpers; **one folder per use case** holds the query/command record and its
handler (e.g. `CreatePublisher/CreatePublisherHandler.cs`). Read models are DTOs — never expose
EF entities. Register each handler in `Program.cs` as scoped. `Roles` and `PdfExport` are the only
remaining "services" (Identity adapter and document generation respectively).

---

## 6. How to add a feature (recipe)

1. **Entity** — add to `Entities/`; inherit `SoftDeleteEntity` if it is user data.
2. **DbContext** — add `DbSet`, configure constraints/relationships in
   `Data/ApplicationDbContext.cs` (max lengths, `OnDelete(DeleteBehavior.Restrict)`,
   string-converted enums). Query filters are applied automatically for soft-delete types.
3. **Migration** — `dotnet ef migrations add <Name> --project GestorTeocratico` and verify.
4. **Slice** — under `Features/<Domain>/` add a `Shared/` folder (contracts/mappings) and one
   folder per use case with its command/query + handler using `IDbContextFactory` (see
   `Features/Publishers/CreatePublisher/` as the reference). Handlers return DTOs, validate
   inputs, log with `ILogger`, and throw domain exceptions (`DuplicateEntityException`,
   `KeyNotFoundException`) or return `bool` per contract.
5. **DI** — register each handler: `builder.Services.AddScoped<XHandler>();` in `Program.cs`.
6. **UI** — add a page under `Components/Pages/<Domain>/`, `@page "/route"`,
   `@attribute [Authorize(...)]`, inject the handler(s), use `BbDataTable` + `BbDialog` +
   `EditForm`. Own a `CancellationTokenSource` and pass `_cts.Token` to handler calls. Follow
   `docs/UI-GUIDE.md`.
7. **Nav** — add an entry in `Components/Layout/NavMenu.razor` if it is a top-level section.
8. **Docs** — update `docs/FEATURES.md` (and `docs/DATA-MODEL.md` if schema changed).

---

## 7. Known pitfalls

Short list — full detail in [`docs/TROUBLESHOOTING.md`](./docs/TROUBLESHOOTING.md):

- **Detached entities** copied across contexts make EF re-insert the graph. Assign only
  foreign keys (see `CopyWeekAssignmentsHandler`).
- **Unique indexes are partial** (`WHERE "IsDeleted" = false`). Adding one over columns that
  already contain duplicates fails at migration time — dedupe the data first
  (`AddUniqueIndexes`).
- **OAuth `redirect_uri_mismatch` behind a proxy:** `UseForwardedHeaders()` must run before
  `UseAuthentication()`; they are registered explicitly in `Program.cs`.
- **Layout must not query the DB per render**; `public_id` is surfaced via a claim
  (`ApplicationUserClaimsPrincipalFactory`).
- **Tailwind classes not applying:** missing `@source` and/or `npm run build:css` not run.
- **Port trap:** compose exposes the DB on host **54332**, not 5432.

---

## 8. Documentation index

| Doc | Purpose |
|---|---|
| [docs/README.md](./docs/README.md) | Doc map and conventions for these docs |
| [docs/ARCHITECTURE.md](./docs/ARCHITECTURE.md) | Vertical slices, DI, render modes, request pipeline |
| [docs/DATA-MODEL.md](./docs/DATA-MODEL.md) | Entities, keys, relationships, soft delete, migrations |
| [docs/FEATURES.md](./docs/FEATURES.md) | Handler ↔ page ↔ endpoint map and business rules |
| [docs/UI-GUIDE.md](./docs/UI-GUIDE.md) | Blazor Blueprint + Tailwind patterns and page template |
| [docs/AUTH.md](./docs/AUTH.md) | Google OAuth, Identity, roles, claims, emergency admin |
| [docs/DEVELOPMENT.md](./docs/DEVELOPMENT.md) | Setup, env vars, migrations, seeding, Docker, CI |
| [docs/API.md](./docs/API.md) | The real HTTP endpoints |
| [docs/CONVENTIONS.md](./docs/CONVENTIONS.md) | Coding, naming, and error-handling conventions |
| [docs/TESTING.md](./docs/TESTING.md) | Test project, strategy and scenarios |
| [docs/TROUBLESHOOTING.md](./docs/TROUBLESHOOTING.md) | Known issues and fixes |
| [docs/PRD.md](./docs/PRD.md) | Product requirements (refreshed) |
| [docs/ROADMAP.md](./docs/ROADMAP.md) | Current state and next steps |
