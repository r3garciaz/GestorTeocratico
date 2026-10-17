# Authentication & Authorization

Identity + **Google OAuth** is the primary login. A local password login exists as a
fallback for a "break-glass" admin account. Configuration lives in `Program.cs`; UI lives under
`GestorTeocratico/Components/Account/`.

## Providers and schemes

```csharp
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = IdentityConstants.ApplicationScheme;
    options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
})
.AddGoogle(o => { /* ClientId/Secret from Authentication:Schemes:GoogleOidc */ })
.AddIdentityCookies();

builder.Services.ConfigureApplicationCookie(o =>
{
    o.ExpireTimeSpan = TimeSpan.FromDays(5);
    o.SlidingExpiration = true;
});
```

- Google config keys: `Authentication:Schemes:GoogleOidc:ClientId` / `:ClientSecret`.
- Scopes/claims: `profile`, `email`, and mapped claims `given_name`, `family_name`, `picture`,
  **`email_verified`**, `locale`. Tokens are saved (`SaveTokens = true`).
- `OnCreatingTicket` calls the userinfo endpoint so additional claims are populated.

## Identity core

```csharp
builder.Services.AddIdentityCore<ApplicationUser>(o => o.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();
```

- `ApplicationUser` adds `PublicId` (unique, required, ≤20 chars).
- `RequireConfirmedAccount = true`: password sign-in requires a confirmed email.
- Roles seeded in the DB: `Admin`, `Manager`, `User` (`Shared/Roles.cs`).

## Google sign-in flow (`ExternalLogin.razor`)

1. `Login.razor` renders under `AuthLayout` (split-screen: brand panel + form) and shows
   `ExternalLoginPicker` (Google) plus a collapsible **local** login form.
2. POST `/Account/PerformExternalLogin` → challenge provider → Google → callback
   `/signin-google` → `/Account/ExternalLogin?Action=LoginCallback`.
3. `OnLoginCallbackAsync`:
   - `ExternalLoginSignInAsync` if the external login already exists → sign in.
   - If the Google email is **verified** (`email_verified == true`):
     - No local user → **auto-provision** (create `ApplicationUser`, `EmailConfirmed = true`,
       generated `PublicId`), link the external login, sign in.
     - Existing **confirmed** user → link + sign in.
     - Existing **unconfirmed** user → refuse auto-link (avoid account takeover) and redirect to
       login with a message.
   - If the email is missing/unverified → fall back to the manual form (creates the account and
     sends a confirmation email).
4. **First user becomes Admin.** `CheckAddAdminRole` assigns the `Admin` role to the very first
   account created (when `Users.Count() == 1`).

Public IDs (`PublicIdGenerator` in `ExternalLogin.razor`, and `DataSeeder.GeneratePublicId`)
generate an 8-char uppercase alphanumeric code containing at least one digit, checked for
uniqueness.

## Claims

`ApplicationUserClaimsPrincipalFactory` (extends the role-aware
`UserClaimsPrincipalFactory<ApplicationUser, IdentityRole>`) adds:

```csharp
public const string PublicIdClaim = "public_id";
```

so the layout can display the public ID **without a database query per render** (which would
compete with the page's own `DbContext` usage). `MainLayout.razor` reads
`ClaimTypes.Email`, the user name, and `public_id`.

## Authorization

- Pages declare access explicitly:
  - `[Authorize]` — dashboard and account pages.
  - `[Authorize(Roles = "Admin, Manager")]` — Congregations, Departments, Responsibilities,
    Publishers, Meeting Schedules, Users, Roles.
  - `[AllowAnonymous]` — landing (`/`), NotFound and Error.
- Prefer `Shared.Roles` constants (`Roles.Admin`, `Roles.Manager`, `Roles.User`) over literals
  where possible. Pages currently use the string form in attributes.
- `Routes.razor` uses `AuthorizeRouteView` with `RedirectToLogin` for unauthenticated users.
- A global **`FallbackPolicy`** (`RequireAuthenticatedUser`) protects every endpoint. Anonymous
  surfaces opt out explicitly: `[AllowAnonymous]` on pages (landing, not-found, error, account)
  and `.AllowAnonymous()` on endpoints (static assets, `POST /Account/PerformExternalLogin`,
  `POST /Account/Logout`). The `/api` group additionally calls `.RequireAuthorization()`.

## Email sender

- If `RESEND:APITOKEN` is set → `ResendEmailSender` (`Services/`), Spanish HTML+text templates
  for confirmation, password reset link, and reset code.
- Otherwise → `IdentityNoOpEmailSender` logs/no-ops (confirmation links are shown in-flow).
- From address: `Resend:FromEmail` (default in `appsettings.Development.json`).

## Emergency local admin

`DataSeeder.SeedEmergencyAdminAsync` runs at startup and, when both
`EmergencyAdmin:Email` and `EmergencyAdmin:Password` are set, ensures a confirmed local user
exists and is in the `Admin` role. This is the "break-glass" path when Google is unavailable.
Configure via env vars (see `.env.example`).

## Account pages (`Components/Account/Pages/`)

Register, ConfirmEmail(Change), ExternalLogin, ForgotPassword(+Confirmation), ResetPassword
(+Confirmation), ResendEmailConfirmation, Lockout, LoginWith2fa, LoginWithRecoveryCode,
AccessDenied, InvalidUser/InvalidPasswordReset, and `Manage/*` (profile, email, password, 2FA,
authenticator, recovery codes, external logins, personal data download, delete account).

Additional endpoints are mapped by
`IdentityComponentsEndpointRouteBuilderExtensions.MapAdditionalIdentityEndpoints()`:
`POST /Account/PerformExternalLogin`, `POST /Account/Logout`,
`POST /Account/Manage/LinkExternalLogin`, `POST /Account/Manage/DownloadPersonalData` (GDPR
export of marked personal data as JSON). The `IdentityRedirectManager` centralizes safe
redirects and status-cookie messages.

## Related docs

- Pipeline ordering (`UseForwardedHeaders` before auth): [ARCHITECTURE.md](./ARCHITECTURE.md)
- OAuth/proxy troubleshooting: [TROUBLESHOOTING.md](./TROUBLESHOOTING.md)
- Env vars: [DEVELOPMENT.md](./DEVELOPMENT.md)
