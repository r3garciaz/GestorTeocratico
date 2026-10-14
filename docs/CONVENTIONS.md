# Conventions

Follow these when writing or changing code. They describe the patterns already in the codebase;
match them rather than inventing new ones.

## Language

- **Code** (identifiers, comments, `ILogger` messages): **English**.
- **UI text, validation messages, emails, seed data**: **Spanish**.
- Commit messages: Conventional Commits with Spanish subject, e.g. `feat(ui): ...`,
  `fix(meeting-schedules): ...`.

## C#

- Nullable reference types are **enabled**; implicit usings are **enabled**.
- Use file-scoped namespaces (`namespace GestorTeocratico.Features.Publishers;`).
- Async all the way; suffix `Async`. `await using` for contexts.
- Primary constructors are fine for DI-focused classes (the context uses them; services mostly
  use classic constructors with fields — match the surrounding file).
- Prefer `IEnumerable<T>`/`List<T>` return types as the existing interfaces do.
- Use collection expressions (`[]`) for empty collections and initializers.
- Avoid `var` where the type is not obvious from the right-hand side; the codebase mixes both —
  follow the file.
- XML doc comments (`///`) on entities/important methods are welcome; keep them accurate.

## Data access

- Inject `IDbContextFactory<ApplicationDbContext>`; never a scoped `ApplicationDbContext`.
- Open a context per method: `await using var context = await _contextFactory.CreateDbContextAsync();`.
- `Include` every navigation the caller needs (entities are detached after the call).
- Soft-delete services set `IsDeleted = true`; never hard-delete soft entities.
- Guard duplicates: check with `AsNoTracking()` before insert; log a warning and return (or throw
  where the contract says so, e.g. congregation singleton throws).
- Use `.IgnoreQueryFilters()` only when deleted rows must be considered.
- Use `DateOnly`/`ISOWeek` for dates and weeks, consistent with `MeetingSchedule`.

## Services

- One interface + one implementation per domain in `Features/<Domain>/`.
- Register in `Program.cs` as scoped.
- Validate references (schedule/publisher/responsibility exist) and throw
  `ArgumentException`/`InvalidOperationException` or return `bool` to match the interface's
  existing contract. Do not silently swallow except where already done (see below).
- Log notable events/errors with `ILogger<T>`.
- Methods that return `bool` (e.g. `AssignResponsibilityAsync`) currently catch and return
  `false`; keep that behavior unless you deliberately change the contract.

## Blazor components

- Feature pages: `Components/Pages/<Domain>/<Name>.razor`.
- Add `@page`, `@attribute [Authorize(...)]`, and `@inject` the service interface.
- Keep markup + `@code` in one file (no `.razor.cs` split is used).
- Use **Blazor Blueprint** `Bb*` components and Lucide icons exclusively. No Radzen/Bootstrap,
  no ad-hoc `<button>`/`<table>`/`<input>` where a `Bb*` component exists.
- Style with Tailwind utility classes and semantic tokens (`bg-background`,
  `text-muted-foreground`, `border`, `bg-muted`, `text-destructive`, …).
- The CRUD pattern is DataTable + Dialogs + `EditForm`; see [UI-GUIDE.md](./UI-GUIDE.md).
- `_Imports.razor` already brings in `BlazorBlueprint.Components`,
  `BlazorBlueprint.Primitives`, and `BlazorBlueprint.Icons.Lucide.Components`.
- Use `ToastService` (`Toasts.Success/Error`) for user feedback in interactive pages.
- Format dates/months with `CultureInfo("es-ES")`; compare names with a Spanish comparer.

## Error handling

- In services: log then rethrow, or return a documented failure value.
- In pages: wrap service calls in `try/catch`; show an inline error in forms
  (`_formError`) or a toast; never let an exception crash the circuit.
- Minimal API endpoints: return `Results.BadRequest`/`Results.Problem`; log the error.

## Git

- Branch names: `feat/...`, `fix/...`, `refactor/...`, `docs/...`.
- Keep changes focused; update the relevant doc under `docs/` in the same change when behavior,
  routes, schema, or configuration change.
