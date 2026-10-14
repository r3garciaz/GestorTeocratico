# Testing

## Current state

**There is no test project in the solution today.**

- `GestorTeocratico.Tests/` exists on disk but contains only leftover build artifacts
  (`bin/`, `obj/`, `Features/` empty of sources) — no `.csproj` and no test classes.
- `GestorTeocratico.sln` references **only** `GestorTeocratico/GestorTeocratico.csproj` (plus
  `compose.yaml` as a solution item). It does **not** reference a test project.
- `dotnet test` therefore discovers no tests.
- The old README claimed `dotnet test`, coverage, and an `85%+` metric — those are inaccurate.

Any code that assumes a test project exists (CI test steps, coverage badges) should be updated.

## What is testable today (and how)

The service layer is the easiest and most valuable target because it is plain async C# behind
interfaces that use `IDbContextFactory<ApplicationDbContext>`:

- **Unit-ish/service tests** with EF Core's in-memory or SQLite provider. The project already
  references `Microsoft.EntityFrameworkCore.Sqlite`, so a SQLite in-memory connection is a
  realistic option that exercises relational behavior (including the global soft-delete query
  filters — prefer SQLite over the InMemory provider for that reason).
- To exercise query filters and joins correctly, create an `ApplicationDbContext` on a
  `DbContextOptionsBuilder<ApplicationDbContext>().UseSqlite(connection)` with an open
  `SqliteConnection`, then `EnsureCreated()`.

High-value scenarios:

- **Soft delete:** after `DeleteAsync`, `GetAllAsync` excludes the row; `IgnoreQueryFilters()`
  still sees it; join rows for a deleted publisher/responsibility disappear.
- **Publisher/Department/Responsibility duplicate guards** (add returns without inserting).
- **Congregation singleton:** second `AddAsync` throws; `DeleteAsync` throws
  `NotSupportedException`.
- **Meeting schedule week logic:** `GetOrCreateWeekSchedulesAsync` creates both midweek and
  weekend exactly once; `WeekOfYear`/`Month`/`Year` derive from `Date`.
- **Assignments:** at most one publisher per (meeting, responsibility); `AssignResponsibilityAsync`
  replaces the previous assignment; `CopyAssignmentsToWeekAsync` synchronizes without
  duplicating/re-inserting detached entities.
- **PDF model:** `GetMonthlyScheduleDataAsync` computes the meeting date from the congregation's
  even/odd-year day and maps short names.

## Proposed re-introduction

```bash
dotnet new xunit -n GestorTeocratico.Tests -o GestorTeocratico.Tests
dotnet add GestorTeocratico.Tests reference GestorTeocratico/GestorTeocratico.csproj
dotnet sln add GestorTeocratico.Tests/GestorTeocratico.Tests.csproj
dotnet test
```

Guidelines:

- Mirror the source layout (`Features/<Domain>/...Tests.cs`) — the empty `Features/` folders
  under the test project hint at the intended structure.
- Add a `TestDbFactory` helper that returns an `IDbContextFactory<ApplicationDbContext>` backed
  by SQLite in-memory.
- Keep tests fast and deterministic; avoid touching PostgreSQL, Google, or Resend.
- When tests exist, add a `dotnet test` step to CI (`.github/workflows/`), which currently only
  builds/pushes Docker images.

## Manual test data

In **Development**, `DataSeederDevelopment` seeds a congregation, 3 departments, 8
responsibilities, 31 publishers, and qualifies **every publisher for every responsibility** —
useful to exercise the assignment UI and PDF export quickly.

## Related docs

- Setup/commands: [DEVELOPMENT.md](./DEVELOPMENT.md)
- Data rules to assert: [DATA-MODEL.md](./DATA-MODEL.md), [FEATURES.md](./FEATURES.md)
