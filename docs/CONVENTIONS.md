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
- Primary constructors are used for DI-focused classes (the context and the slice handlers);
  some services use classic constructors with fields — match the surrounding file.
- Handlers return `IReadOnlyList<T>` / DTOs; `IEnumerable<T>`/`List<T>` remain where they read
  naturally.
- Use collection expressions (`[]`) for empty collections and initializers.
- Avoid `var` where the type is not obvious from the right-hand side; the codebase mixes both —
  follow the file.
- XML doc comments (`///`) on entities/important methods are welcome; keep them accurate.

## Data access

- Inject `IDbContextFactory<ApplicationDbContext>`; never a scoped `ApplicationDbContext`.
- Open a context per method: `await using var context = await _contextFactory.CreateDbContextAsync();`.
- Read paths use `.AsNoTracking()` and map to **DTOs** — never return EF entities to the UI.
- `Include` every navigation the DTO needs (entities are detached after the call).
- Soft-delete handlers set `IsDeleted = true`; never hard-delete soft entities.
- Guard duplicates: check with `AsNoTracking()` before insert and **throw
  `DuplicateEntityException`** (partial unique indexes back it up in the DB).
- Mutating handlers copy scalar values onto the **tracked** entity (field-by-field or
  `CurrentValues.SetValues`) instead of `context.Update(detachedGraph)`.
- Use `.IgnoreQueryFilters()` only when deleted rows must be considered.
- Use `DateOnly`/`ISOWeek` for dates and weeks, consistent with `MeetingSchedule`.

## Handlers / slices

- Vertical slice layout: `Features/<Domain>/Shared/` (DTOs, mappings, sync helpers) + one folder
  per use case (`<UseCase>/<UseCase>Handler.cs`).
- Handlers receive `IDbContextFactory` and return **DTOs**, never EF entities.
- Register each handler by concrete type in `Program.cs` (`builder.Services.AddScoped<XHandler>()`).
- Validate references (schedule/publisher/responsibility exist); throw domain exceptions
  (`DuplicateEntityException`, `KeyNotFoundException`, `ArgumentException`) or return `bool` per
  contract.
- Log notable events/errors with `ILogger<T>`. Never swallow exceptions with a bare `catch`;
  re-throw `OperationCanceledException` explicitly.
- Accept and propagate a `CancellationToken` on async methods.

## Blazor components

- Feature pages: `Components/Pages/<Domain>/<Name>.razor`.
- Add `@page`, `@attribute [Authorize(...)]`, and `@inject` the handler(s).
- Keep markup + `@code` in one file (no `.razor.cs` split is used).
- Use **Blazor Blueprint** `Bb*` components and Lucide icons exclusively. No Radzen/Bootstrap,
  no ad-hoc `<button>`/`<table>`/`<input>` where a `Bb*` component exists.
- Style with Tailwind utility classes and semantic tokens (`bg-background`,
  `text-muted-foreground`, `border`, `bg-muted`, `text-destructive`, …).
- The CRUD pattern is DataTable + Dialogs + `EditForm`; see [UI-GUIDE.md](./UI-GUIDE.md).
- `_Imports.razor` already brings in `BlazorBlueprint.Components`,
  `BlazorBlueprint.Primitives`, and `BlazorBlueprint.Icons.Lucide.Components`.
- Data-loading pages own a `CancellationTokenSource`, pass `_cts.Token` to handler calls, and
  cancel/dispose it in `Dispose()`.
- Use `ToastService` (`Toasts.Success/Error`) for user feedback in interactive pages.
- Format dates/months with `CultureInfo("es-ES")`; compare names with a Spanish comparer.

## Error handling

- In handlers: log then rethrow, or return a documented failure value.
- In pages: wrap handler calls in `try/catch`; show an inline error in forms
  (`_formError`) or a toast; never let an exception crash the circuit.
- Minimal API endpoints: return `Results.BadRequest`/`Results.Problem`; log the error.

## Git

- Branch names: `feat/...`, `fix/...`, `refactor/...`, `docs/...`.
- Keep changes focused; update the relevant doc under `docs/` in the same change when behavior,
  routes, schema, or configuration change.
