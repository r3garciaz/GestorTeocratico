# Architecture

Gestor Teocrático is a **Blazor Server** application built as a single ASP.NET Core project
(`GestorTeocratico/`). There is no separate API project; HTTP endpoints, UI, and business
logic live in the same process. Persistence is PostgreSQL through EF Core.

## High-level shape

```
Browser ──(SignalR circuit / SSR)──> ASP.NET Core (Program.cs)
                                       ├─ Blazor components (Components/)
                                       ├─ Feature slices / handlers (Features/) ── IDbContextFactory ──> PostgreSQL
                                       ├─ Identity + Google OAuth
                                       ├─ Minimal API endpoints (/api/...)
                                       └─ QuestPDF / Resend (outbound)
```

- The code is organized as a **Vertical Slice architecture**: one folder per feature, and within
  it one folder per use case (a query/command + handler). "Layers" exist by convention:
  - `Components/` — presentation (Razor).
  - `Features/` — vertical slices: contracts, handlers and data access per domain.
  - `Entities/` — domain models.
  - `Data/` — EF Core context, Identity user, seeders, migrations.
- The legacy PRD called this "Clean Architecture"; an earlier iteration used a per-entity CRUD
  service layer, which was replaced by the slices (only `Roles` and `PdfExport` remain services).

## Composition root — `Program.cs`

Startup order (important parts):

1. `QuestPDF.Settings.License = LicenseType.Community` (before building the app).
2. `AddRazorComponents().AddInteractiveServerComponents()` + `AddBlazorBlueprintComponents()`.
3. Authentication: default scheme `IdentityConstants.ApplicationScheme`, **Google** OAuth
   provider, `AddIdentityCookies()`; cookie lifetime 5 days, sliding.
4. `AddAuthorization` with a global **`FallbackPolicy`** (`RequireAuthenticatedUser`).
5. `AddDbContextFactory<ApplicationDbContext>(... UseNpgsql(connectionString))`.
6. `AddIdentityCore<ApplicationUser>()` + roles + EF stores + sign-in manager + token providers
   (`SignIn.RequireConfirmedAccount = true`).
7. `ApplicationUserClaimsPrincipalFactory` (adds the `public_id` claim).
8. `AddDataProtection().PersistKeysToDbContext<ApplicationDbContext>()`.
9. Vertical slice handler registrations (see table below).
10. Resend email (only if `RESEND:APITOKEN` is set; otherwise `IdentityNoOpEmailSender`).
11. `HttpClient` for `PdfExportHttpClient` (base address from `PdfExport:BaseAddress`).
12. `ForwardedHeadersOptions` (proxy support).
13. Build app → **run migrations + seed** (Dev seed vs Prod seed).
14. `SeedEmergencyAdminAsync`.
15. Pipeline: `UseForwardedHeaders()` → status code pages → `UseHttpsRedirection()` →
    `UseAuthentication()` → `UseAuthorization()` → `UseAntiforgery()` → static assets →
    `MapRazorComponents<App>()` → Identity endpoints → `/api` group.

### Handler registrations (`Program.cs`)

Handlers are registered **by concrete type** (one class per use case), plus two remaining services:

| Registration | Kind |
|---|---|
| `GetPublishersHandler`, `GetPublisherByIdHandler`, `GetPublisherOptionsHandler`, `CreatePublisherHandler`, `UpdatePublisherHandler`, `DeletePublisherHandler` | Publishers slice |
| `GetDepartmentsHandler`, `GetDepartmentByIdHandler`, `GetDepartmentOptionsHandler`, `CreateDepartmentHandler`, `UpdateDepartmentHandler`, `DeleteDepartmentHandler` | Departments slice |
| `GetResponsibilitiesHandler`, `GetResponsibilityByIdHandler`, `GetResponsibilityOptionsHandler`, `CreateResponsibilityHandler`, `UpdateResponsibilityHandler`, `DeleteResponsibilityHandler` | Responsibilities slice |
| `GetCongregationsHandler`, `CreateCongregationHandler`, `UpdateCongregationHandler` | Congregations slice |
| `GetAllMeetingSchedulesHandler`, `GetMeetingSchedulesByMonthHandler`, `GetOrCreateWeekSchedulesHandler`, `GetAssignmentsByMeetingScheduleHandler`, `AssignResponsibilityHandler`, `UnassignResponsibilityHandler`, `CopyWeekAssignmentsHandler` | Meeting schedules + assignments slice |
| `IPdfExportService` → `PdfExportService` | Service (document generation) |
| `IRoleService` → `RoleService` | Service (Identity adapter) |
| `IEmailSender<ApplicationUser>` → `ResendEmailSender` **or** `IdentityNoOpEmailSender` | Scoped / Singleton |

Everything is **scoped** because it is used per Blazor circuit/request. Handlers are safe to be
long-lived *per circuit* precisely because they do **not** hold a `DbContext` — they hold the
singleton factory.

## Data access pattern: `IDbContextFactory`

Every handler follows the same shape:

```csharp
public sealed class GetPublishersHandler(IDbContextFactory<ApplicationDbContext> contextFactory)
{
    public async Task<IReadOnlyList<PublisherDto>> HandleAsync(CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        var publishers = await context.Publishers
            .AsNoTracking()
            .Include(p => p.ResponsibleDepartments)
            .Include(p => p.PublisherResponsibilities).ThenInclude(pr => pr.Responsibility)
            .OrderBy(p => p.FirstName).ThenBy(p => p.LastName)
            .ToListAsync(cancellationToken);

        return publishers.Select(p => p.ToDto()).ToList();
    }
}
```

**Why a factory and not a scoped `DbContext`:** a Blazor Server circuit is long-lived and can
run multiple async operations concurrently. A single scoped `DbContext` is not thread-safe and
would break under concurrency. The factory gives each operation its own short-lived context.

Consequences to remember:

- Read queries use `AsNoTracking()`; handlers map to **DTOs** and never return EF entities.
- Mutating handlers re-load the tracked entity and copy **scalar** values (`CurrentValues.SetValues`
  or field-by-field) instead of `context.Update(detachedGraph)`, so related entities are not
  re-marked.
- Do not pass a context between handlers. Each handler opens its own.
- Accept and propagate a `CancellationToken` (components cancel it on disposal).

## Render modes

- Interactive rendering is enabled via `AddInteractiveServerComponents()`.
- `Components/App.razor` sets `PageRenderMode`: interactive server only when the request
  `AcceptsInteractiveRouting()`, otherwise static SSR.
- `Components/Routes.razor` uses `AuthorizeRouteView` with `MainLayout` and a
  `RedirectToLogin` for unauthenticated users.
- The **public landing** (`Components/Pages/Index.razor`) opts out with
  `[ExcludeFromInteractiveRouting]` and `@layout IndexLayout`. Interactive "islands"
  (theme controls) are mounted explicitly with `@rendermode="InteractiveServer"`.

## Layouts and providers

- `MainLayout.razor` — authenticated shell: `BbSidebarProvider` + sidebar (`NavMenu`) +
  inset with a header (sidebar trigger, page title, theme switcher/dark toggle) and a footer
  showing the user's `public_id`.
- `IndexLayout.razor` — public shell (landing + account pages): header with brand, theme
  controls, login button; footer.
- Interactive providers are rendered **once** at the end of `MainLayout`:
  `BbPortalHost`, `BbToastProvider`, `BbDialogProvider`. Toasts are consumed via the injected
  `ToastService` (`Toasts.Success/Error`).

## Request pipeline notes

- **Forwarded headers first.** `UseForwardedHeaders()` is registered before
  `UseAuthentication()`/`UseAuthorization()` on purpose: behind a TLS-terminating reverse
  proxy the OAuth callback (`/signin-google`) must see `https`, otherwise Google returns
  `redirect_uri_mismatch`.
- A global `FallbackPolicy` requires an authenticated user on **every** endpoint. Anonymous
  surfaces declare intent explicitly: `[AllowAnonymous]` on the landing, not-found, error and
  account pages, and `.AllowAnonymous()` on static assets and the login/logout endpoints.
- Status-code pages re-execute `/not-found` (`Components/Pages/NotFound.razor`); unhandled
  exceptions re-execute `/Error` (`Components/Pages/Error.razor`) — both anonymous.

## PDF generation

`PdfExportService` composes a `MonthlyScheduleDocument` (QuestPDF `IDocument`) from
`MonthlySchedulePdfModel` and returns bytes. It consumes the slice handlers
(`GetMeetingSchedulesByMonthHandler`, `GetResponsibilitiesHandler`, `GetCongregationsHandler`),
then computes the actual meeting day per week from the congregation's even/odd-year day settings
(`GetDayOfMeeting`). The minimal API endpoint (authenticated) streams the bytes.

There is also a `PdfExportHttpClient` wrapper whose base address comes from configuration
(`PdfExport:BaseAddress`, default `https://localhost:7095`); its route matches the endpoint
(`/api/meeting-schedules/monthly-schedule/{year}/{month}`). The UI triggers the endpoint via
navigation instead.

## Cross-cutting concerns

- **Logging:** `ILogger<T>` in handlers and components; startup migration/seed logs.
- **Email:** `ResendEmailSender` (HTML + text templates, Spanish) or no-op fallback.
- **Data Protection:** keys persisted in the database (`DataProtectionKeys` table).
- **Culture:** Spanish formatting is applied locally where needed (`new CultureInfo("es-ES")`).

## Related docs

- Data schema: [DATA-MODEL.md](./DATA-MODEL.md)
- Auth internals: [AUTH.md](./AUTH.md)
- Feature inventory: [FEATURES.md](./FEATURES.md)
