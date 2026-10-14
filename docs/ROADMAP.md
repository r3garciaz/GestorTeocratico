# Roadmap

Current status and planned work. This replaces the aspirational "phases" in the old README
(which referenced technologies no longer in use).

## Status (implemented)

- **Core management:** Congregation (singleton), Departments, Publishers, Responsibilities —
  CRUD with soft delete where applicable.
- **Qualification model:** publishers ↔ responsibilities, department responsibility ownership.
- **Meeting scheduling:** week/month/list views, qualified-publisher assignment, copy week,
  on-demand week creation.
- **PDF export:** monthly schedule (QuestPDF).
- **Dashboard:** KPIs + upcoming meetings.
- **Auth:** Google OAuth (+ emergency local admin), Identity, roles (Admin/Manager/User), 2FA,
  account management, personal-data download.
- **UI:** Blazor Blueprint v4 + Tailwind v4, Spanish, light/dark theme.
- **Infra:** Docker/Compose, GitHub Actions image build.

## Gaps / tech debt (known)

- **No test project** in the solution — see [TESTING.md](./TESTING.md).
- `PdfExportHttpClient` points at a non-existent route (`/api/pdf/...`) and is effectively unused
  — see [API.md](./API.md).
- No global fallback authorization policy (per-page attributes only) — see [AUTH.md](./AUTH.md).
- Forwarded headers accept any network/proxy (`KnownNetworks`/`KnownProxies` cleared) — tighten
  for production.
- Manager vs Admin have identical page access today; role granularity is coarse.
- README badges/metrics were historical and are being corrected in docs.

## Near term

- **Re-introduce automated tests** and a `dotnet test` CI step ([TESTING.md](./TESTING.md)).
- **Differentiate Manager permissions** from Admin where meaningful.
- **Harden production auth/network config** (trusted proxies, cookie/security settings).
- **Clean up** the unused `PdfExportHttpClient` or fix its route.
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
