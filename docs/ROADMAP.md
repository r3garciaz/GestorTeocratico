# Roadmap

Current status and planned work. This replaces the aspirational "phases" in the old README
(which referenced technologies no longer in use).

## Status (implemented)

- **Architecture:** **vertical slices** — one folder per feature, one handler per use case with
  DTO read models (Congregations, Departments, Publishers, Responsibilities, MeetingSchedules +
  assignments). `Roles` and `PdfExport` remain as services.
- **Core management:** Congregation (singleton), Departments, Publishers, Responsibilities —
  CRUD with soft delete where applicable and DB-enforced uniqueness (partial unique indexes).
- **Qualification model:** publishers ↔ responsibilities, department responsibility ownership.
- **Meeting scheduling:** week/month/list views, qualified-publisher assignment (**one publisher
  per responsibility per meeting**, enforced by the PK), copy week, on-demand week creation.
- **PDF export:** monthly schedule (QuestPDF).
- **Dashboard:** KPIs + upcoming meetings.
- **Auth:** Google OAuth (+ emergency local admin), Identity, roles (Admin/Manager/User), 2FA,
  account management, personal-data download, **global `FallbackPolicy`** + explicit
  `[AllowAnonymous]` surfaces.
- **UI:** Blazor Blueprint v4 + Tailwind v4, Spanish, light/dark theme.
- **Tests:** `GestorTeocratico.Tests` (xUnit + SQLite in-memory) covering handlers.
- **Infra:** Docker/Compose, GitHub Actions image build.

## Gaps / tech debt (known)

- **No PostgreSQL integration tests** (Testcontainers) — SQLite covers most relational behavior,
  but provider-specific migrations are only validated on apply — see [TESTING.md](./TESTING.md).
- **CI does not run `dotnet test`** (only builds/pushes the image).
- Forwarded headers accept any network/proxy (`KnownNetworks`/`KnownProxies` cleared) — tighten
  for production.
- Manager vs Admin have identical page access today; role granularity is coarse.
- README badges/metrics were historical and are being corrected in docs.

## Near term

- **Add a `dotnet test` step to CI** (`.github/workflows/`).
- **Differentiate Manager permissions** from Admin where meaningful.
- **Harden production auth/network config** (trusted proxies, cookie/security settings).
- **Add integration tests** against PostgreSQL for the unique indexes and the PK migration.
- Keep `docs/` and `AGENTS.md` current as features change.

## Later / ideas

- Configurable email templates and richer notifications.
- Additional exports (publisher list, department reports).
- Bulk assignment helpers (e.g. auto-suggest qualified publishers, conflict checks).
- Optional read-only API for integrations (would require a real auth story).
- Internationalization beyond Spanish.

## Explicitly out of scope (for now)

- Multi-congregation / multi-tenant support.
- Native mobile apps.

## Related docs

- [PRD.md](./PRD.md) · [FEATURES.md](./FEATURES.md) · [TESTING.md](./TESTING.md)
