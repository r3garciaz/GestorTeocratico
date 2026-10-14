# Troubleshooting

Known issues and their fixes, based on real fixes in this repository. Check here before
"rediscovering" a bug.

## Blazor / EF Core

### "The instance of entity type 'X' cannot be tracked…" or duplicate primary keys when copying a week

Copying assignments across weeks reads source schedules from one `DbContext` and writes to a
target schedule from another; the source entities are **detached**. Assigning their navigation
properties (`MeetingSchedule`, `Publisher`, `Responsibility`) to a new join row makes EF treat the
whole graph as `Added` and re-insert existing rows → `duplicate key value violates unique
constraint "PK_MeetingSchedules"` (or similar).

**Fix / pattern:** build join rows from **foreign keys only** (see
`MeetingScheduleService.CopyAssignmentsToWeekAsync`):

```csharp
context.ResponsibilityAssignments.Add(new ResponsibilityAssignment
{
    MeetingScheduleId = targetSchedule.MeetingScheduleId,
    ResponsibilityId = responsibilityId,
    PublisherId = publisherId
});
```

Never attach detached entities you did not fetch from the current context.

### DbContext concurrency / "A second operation was started on this context"

Do not inject a scoped `ApplicationDbContext` into Blazor components or services. Use
`IDbContextFactory<ApplicationDbContext>` and open a context per operation. The layout reads the
user's `public_id` from a **claim** (`ApplicationUserClaimsPrincipalFactory`) precisely to avoid a
per-render DB query.

### Entities are missing after returning from a service

Entities are detached once the context is disposed; lazy loading is off. `Include` the
navigations the UI needs inside the service. If a navigation is unexpectedly `null`, a missing
`Include` is the usual cause.

### Soft-deleted data still (or no longer) appears

Global query filters exclude `IsDeleted == true` for soft-delete entities and their join rows.
If you need deleted rows, use `.IgnoreQueryFilters()` (already used in seeding). If a join row
seems to vanish, remember its filter checks the parent `Publisher`/`Responsibility` soft-delete
flags.

## Authentication / Google OAuth

### `redirect_uri_mismatch` (or OAuth callback fails behind a proxy)

Behind a TLS-terminating reverse proxy the app may see `http`, so the generated callback is
wrong. The app registers the pipeline explicitly:

```csharp
app.UseForwardedHeaders();   // must come before auth
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
```

If you reorder these (or rely on `WebApplication` auto-adding auth at the start), the
`/signin-google` callback breaks. Also set a correct HTTPS redirect URI in the Google console and
(non-default apps) allow the proxy. `Configure<ForwardedHeadersOptions>` clears `KnownNetworks`
and `KnownProxies` — tighten this for production.

### Google login fails at startup

`Program.cs` throws when `Authentication:Schemes:GoogleOidc:ClientId`/`ClientSecret` are missing.
Set the `AUTHENTICATION__SCHEMES__GOOGLEOIDC__*` env vars (see `.env.example`).

### Locked out with no Google access

Configure `EMERGENCYADMIN__EMAIL` / `EMERGENCYADMIN__PASSWORD`; the seeder creates a confirmed
local Admin at startup. You can then sign in via the collapsible **local** login form on
`/Account/Login`.

### Existing account not auto-linked from Google

Auto-linking requires the Google email to be **verified** (`email_verified`). If a local account
already exists with that email and is **not** confirmed, the app refuses to auto-link (anti–account
takeover) and redirects to login with a message. Confirm the account or link Google from
`/Account/Manage/ExternalLogins`.

## Styling (Tailwind / Blazor Blueprint)

### Tailwind classes have no effect

1. The class is in a folder not covered by `@source` in `wwwroot/css/app-input.css` — add it.
2. `npm run build:css` was not run (the output `wwwroot/css/app.css` is generated).
3. You used a raw color instead of the semantic tokens — theming/dark mode then misbehaves.

### Dark mode doesn't follow the toggle

Tailwind v4 defaults `dark:` to `prefers-color-scheme`. The project overrides this with
`@custom-variant dark (&:where(.dark, .dark *))` to match the library's `.dark` class. Don't
remove that line.

### Components look unstyled / theme not applied

Check the `<link>` order in `Components/App.razor`: theme vars → component CSS → themes palette →
`css/app.css` → `theme-init.js`.

## Database

### App can't connect / connection refused

- Verify `ConnectionStrings__DefaultConnection` (startup throws if absent).
- When using `docker compose`, the DB is exposed on **host port 54332** (`54332:${POSTGRES_PORT}`),
  not 5432.
- Inside compose, the app connects to host `database` (the service name), not `localhost`.

### Migration/seed failure at startup

Startup applies migrations and seeds and rethrows on error. In Development, inspect the exception
page; in Production, check logs. The seeders are idempotent, so re-running after fixing the
connection is safe.

## Build / tooling

### `dotnet build` fails on generated CSS/missing assets

Run `npm install` and `npm run build:css` so `wwwroot/css/app.css` exists.

### `dotnet test` finds nothing

There is no test project (see [TESTING.md](./TESTING.md)).

## Related docs

- Pipeline/ordering: [ARCHITECTURE.md](./ARCHITECTURE.md)
- Auth details: [AUTH.md](./AUTH.md)
- Commands/env: [DEVELOPMENT.md](./DEVELOPMENT.md)
