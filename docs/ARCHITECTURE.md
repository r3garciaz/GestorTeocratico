# Architecture

Gestor Teocrático is a **Blazor Server** application built as a single ASP.NET Core project
(`GestorTeocratico/`). There is no separate API project; HTTP endpoints, UI, and business
logic live in the same process. Persistence is PostgreSQL through EF Core.

## High-level shape

```
Browser ──(SignalR circuit / SSR)──> ASP.NET Core (Program.cs)
                                       ├─ Blazor components (Components/)
                                       ├─ Feature services (Features/)  ── IDbContextFactory ──> PostgreSQL
                                       ├─ Identity + Google OAuth
                                       ├─ Minimal API endpoints (/api/...)
                                       └─ QuestPDF / Resend (outbound)
```

- **Not a layered/clean-architecture solution.** Code is organized by **feature folder**, not
  by technical layer. "Layers" exist by convention:
  - `Components/` — presentation (Razor).
  - `Features/` — business logic + data access per domain.
  - `Entities/` — domain models.
  - `Data/` — EF Core context, Identity user, seeders, migrations.
- The legacy PRD called this "Clean Architecture"; in practice it is **feature-based with a
  service layer**.

## Composition root — `Program.cs`

Startup order (important parts):

1. `QuestPDF.Settings.License = LicenseType.Community` (before building the app).
2. `AddRazorComponents().AddInteractiveServerComponents()` + `AddBlazorBlueprintComponents()`.
3. Authentication: default scheme `IdentityConstants.ApplicationScheme`, **Google** OAuth
   provider, `AddIdentityCookies()`; cookie lifetime 5 days, sliding.
4. `AddDbContextFactory<ApplicationDbContext>(... UseNpgsql(connectionString))`.
5. `AddIdentityCore<ApplicationUser>()` + roles + EF stores + sign-in manager + token providers
   (`SignIn.RequireConfirmedAccount = true`).
6. `ApplicationUserClaimsPrincipalFactory` (adds the `public_id` claim).
7. `AddDataProtection().PersistKeysToDbContext<ApplicationDbContext>()`.
8. Feature service registrations (see table below).
9. Resend email (only if `RESEND:APITOKEN` is set; otherwise `IdentityNoOpEmailSender`).
10. `HttpClient` for `PdfExportHttpClient`.
11. `ForwardedHeadersOptions` (proxy support).
12. Build app → **run migrations + seed** (Dev seed vs Prod seed).
13. `SeedEmergencyAdminAsync`.
14. Pipeline: `UseForwardedHeaders()` → status code pages → `UseHttpsRedirection()` →
    `UseAuthentication()` → `UseAuthorization()` → `UseAntiforgery()` → static assets →
    `MapRazorComponents<App>()` → Identity endpoints → `/api` group.

### Service registrations (`Program.cs`)

| Interface | Implementation | Lifetime |
|---|---|---|
| `ICongregationService` | `CongregationService` | Scoped |
| `IDepartmentService` | `DepartmentService` | Scoped |
| `IPublisherService` | `PublisherService` | Scoped |
| `IResponsibilityService` | `ResponsibilityService` | Scoped |
| `IPublisherResponsibilityService` | `PublisherResponsibilityService` | Scoped |
| `IMeetingScheduleService` | `MeetingScheduleService` | Scoped |
| `IResponsibilityAssignmentService` | `ResponsibilityAssignmentService` | Scoped |
| `IPdfExportService` | `PdfExportService` | Scoped |
| `IRoleService` | `RoleService` | Scoped |
| `IEmailSender<ApplicationUser>` | `ResendEmailSender` **or** `IdentityNoOpEmailSender` | Scoped / Singleton |

All services are **scoped** because they are used per Blazor circuit/request. They are safe to
be long-lived *per circuit* precisely because they do **not** hold a `DbContext` — they hold
the singleton factory.

## Data access pattern: `IDbContextFactory`

Every feature service follows the same shape:

```csharp
public class PublisherService : IPublisherService
{
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
    private readonly ILogger<PublisherService> _logger;

    public PublisherService(IDbContextFactory<ApplicationDbContext> contextFactory,
                            ILogger<PublisherService> logger) { ... }

    public async Task<IEnumerable<Publisher>> GetAllAsync()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Publishers.Include(...).ToListAsync();
    }
}
```

**Why a factory and not a scoped `DbContext`:** a Blazor Server circuit is long-lived and can
run multiple async operations concurrently. A single scoped `DbContext` is not thread-safe and
would break under concurrency. The factory gives each operation its own short-lived context.

Consequences to remember:

- Entities returned are **detached** once the context is disposed. Don't rely on lazy loading
  after the service returns; `Include` the navigations you need. UI code reads navigations the
  service already loaded.
- Do not pass a context between services. Each method opens its own.
- `SaveChangesAsync` is called inside each mutating method.

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
- `IndexLayout.razor` — public shell (landing): header with brand, theme
  controls, login button; footer.
- `AuthLayout.razor` — split-screen shell for the login page: brand panel on the left
  (desktop only, `bg-primary` with a dotted pattern) and the form column on the right, with a
  mobile brand mark, theme controls island, and a "Volver al inicio" link.
- Interactive providers are rendered **once** at the end of `MainLayout`:
  `BbPortalHost`, `BbToastProvider`, `BbDialogProvider`. Toasts are consumed via the injected
  `ToastService` (`Toasts.Success/Error`).

## Request pipeline notes

- **Forwarded headers first.** `UseForwardedHeaders()` is registered before
  `UseAuthentication()`/`UseAuthorization()` on purpose: behind a TLS-terminating reverse
  proxy the OAuth callback (`/signin-google`) must see `https`, otherwise Google returns
  `redirect_uri_mismatch`.
- `MapStaticAssets()` is anonymous; razor components require auth per page attribute.
- Status-code pages re-execute `/not-found` (`Components/Pages/NotFound.razor`).

## PDF generation

`PdfExportService` composes a `MonthlyScheduleDocument` (QuestPDF `IDocument`) from
`MonthlySchedulePdfModel` and returns bytes. `GetMonthlyScheduleDataAsync` gathers schedules,
responsibilities, and the (single) congregation, then computes the actual meeting day per week
from the congregation's even/odd-year day settings (`GetDayOfMeeting`). The minimal API endpoint
streams the bytes.

There is also a `PdfExportHttpClient` wrapper (registered `HttpClient` pointing at
`https://localhost:7095`), but the UI now triggers the endpoint via navigation instead.

## Cross-cutting concerns

- **Logging:** `ILogger<T>` in services and components; startup migration/seed logs.
- **Email:** `ResendEmailSender` (HTML + text templates, Spanish) or no-op fallback.
- **Data Protection:** keys persisted in the database (`DataProtectionKeys` table).
- **Culture:** Spanish formatting is applied locally where needed (`new CultureInfo("es-ES")`).

## Related docs

- Data schema: [DATA-MODEL.md](./DATA-MODEL.md)
- Auth internals: [AUTH.md](./AUTH.md)
- Feature inventory: [FEATURES.md](./FEATURES.md)
