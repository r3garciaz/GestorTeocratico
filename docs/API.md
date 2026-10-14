# API

> **Reality check:** this is primarily a Blazor Server app. The only HTTP API today is a single
> minimal-API endpoint for PDF export. The README's historical claims of a REST API with JWT
> bearer auth, controllers, and webhooks are **not implemented**. If you need an API, build it
> and update this doc.

## Implemented endpoints

### `GET /api/meeting-schedules/monthly-schedule/{year}/{month}`

Generates and downloads the monthly schedule PDF.

- **Group:** `/api` → `MapGroup("/meeting-schedules")` →
  `Features/MeetingSchedules/Endpoints/MeetingSchedulesEndpoints.cs`.
- **Route values:** `year` int (2020–2030), `month` int (1–12).
- **Auth:** **required** — the `/api` group has `.RequireAuthorization()` and a global
  `FallbackPolicy` requires an authenticated user. Anonymous surfaces opt out explicitly with
  `[AllowAnonymous]`/`.AllowAnonymous()` (login, static assets, landing, error, not-found).
- **Success:** `200` with `application/pdf` and a `Content-Disposition` filename
  `Programacion_{Mes}_{Year}.pdf` (month name in Spanish).
- **Errors:** `400` when month/year are out of range; `Problem(...)` if generation throws.
- **OpenAPI metadata:** `.WithName("GenerateMonthlySchedulePdf")`, summary, `Produces(200)`,
  `Produces(400)`.

Example:

```bash
curl -s -o programacion-12-2025.pdf \
  "https://localhost:7095/api/meeting-schedules/monthly-schedule/2025/12"
```

### Identity endpoints (mapped by Identity, not a public API)

`MapAdditionalIdentityEndpoints()` exposes form-POST endpoints used by the account UI:

| Method & path | Purpose |
|---|---|
| `POST /Account/PerformExternalLogin` | Start an external (Google) login challenge |
| `POST /Account/Logout` | Sign out and redirect home |
| `POST /Account/Manage/LinkExternalLogin` | Link another external login |
| `POST /Account/Manage/DownloadPersonalData` | Returns the user's personal data as `PersonalData.json` |

These are intended for the UI forms (they use cookies/antiforgery), not as a programmatic API.

## Non-endpoint "integration" surfaces

If you need to build on the app programmatically today, these are the stable seams:

- **Slice handlers** (`Features/<Domain>/<UseCase>/<UseCase>Handler.cs`) resolved from DI — use
  them inside the app (a new endpoint, a HostedService, etc.) rather than calling HTTP. `Roles`
  (`IRoleService`) and `PdfExport` (`IPdfExportService`) remain interface-based services.
- **Database** — read/write through `ApplicationDbContext` with the soft-delete filters in mind.

## `PdfExportHttpClient`

`Features/PdfExport/PdfExportHttpClient.cs` is an in-app `HttpClient` wrapper. Its base address
comes from configuration (`PdfExport:BaseAddress`, default `https://localhost:7095`) and it
requests `/api/meeting-schedules/monthly-schedule/{year}/{month}`, matching the implemented
route. It is currently unused by the UI (which navigates to the endpoint); prefer
`IPdfExportService` in-process.

## Not implemented (do not assume)

- REST controllers, JWT bearer tokens, refresh tokens.
- Webhooks, calendar sync, bulk/import endpoints.
- API versioning.

## Related docs

- PDF generation internals: [ARCHITECTURE.md](./ARCHITECTURE.md), [FEATURES.md](./FEATURES.md)
