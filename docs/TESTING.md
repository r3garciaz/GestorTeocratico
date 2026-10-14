# Testing

## Current state

**There is a test project in the solution: `GestorTeocratico.Tests`** (xUnit).

- It references `GestorTeocratico/GestorTeocratico.csproj` and `Microsoft.EntityFrameworkCore.Sqlite`.
- Uses an in-memory **SQLite** `IDbContextFactory<ApplicationDbContext>` (open connection +
  `EnsureCreated`) so relational behavior is exercised: transactions, unique indexes and the
  global soft-delete query filters.
- 15 tests today, all green (`dotnet test`).
- Tests target the **vertical slice handlers** (not a service layer).

```bash
dotnet test
```

## Test infrastructure

`Infrastructure/SqliteTestDbContextFactory.cs` implements
`IDbContextFactory<ApplicationDbContext>` backed by a single open `Data Source=:memory:`
connection, running `EnsureCreated()` once. Every `CreateDbContext()` shares the same schema, so
unique indexes and query filters behave like production.

Tests instantiate handlers directly with that factory and a `NullLogger<T>.Instance`, e.g.:

```csharp
using var factory = new SqliteTestDbContextFactory();
var create = new CreatePublisherHandler(factory, NullLogger<CreatePublisherHandler>.Instance);
var id = await create.HandleAsync(new CreatePublisherCommand("Ana", "Lopez", null, null, null,
    Gender.Female, null, [], []));
```

## Covered scenarios

- **`Update` must not modify related entities** (the detached-graph regression): updating a
  publisher does not overwrite its responsibilities/departments; same for departments.
- **Duplicate guards** now throw `DuplicateEntityException` (publisher full name, department
  name, responsibility within a department) and are also backed by the unique indexes.
- **Soft delete:** after `Delete*Handler`, `Get*` excludes the row; `IgnoreQueryFilters()` still
  sees it.
- **Create persists assignments** for publishers (responsibilities + departments), responsibilities
  (qualified publishers) and departments (responsibilities).
- **Assignments:** `AssignResponsibilityHandler` replaces the previous assignment for the same
  responsibility; the **primary key rejects a second assignment** for the same
  `(MeetingScheduleId, ResponsibilityId)` (`DbUpdateException`).
- **Unassign** removes the assignment.
- **Copy week** copies assignments to the target week (synchronizing by responsibility).

## Gaps / next steps

- No **integration tests against real PostgreSQL** (Testcontainers) yet — SQLite covers most
  relational behavior, but provider-specific SQL (e.g. the `AddUniqueIndexes` filter and the
  `OneResponsibilityPerMeeting` dedupe) is only validated when migrations run.
- No component tests (bUnit) for the pages.
- CI (`.github/workflows/`) builds/pushes the Docker image but does **not** run `dotnet test`
  yet — add a test step.

Guidelines:

- Mirror the source layout: `GestorTeocratico.Tests/Features/<Domain>/<X>Tests.cs`.
- Keep tests fast and deterministic; avoid touching PostgreSQL, Google, or Resend.

## Manual test data

In **Development**, `DataSeederDevelopment` seeds a congregation, 3 departments, 8
responsibilities, 31 publishers, and qualifies **every publisher for every responsibility** —
useful to exercise the assignment UI and PDF export quickly.

## Related docs

- Setup/commands: [DEVELOPMENT.md](./DEVELOPMENT.md)
- Data rules to assert: [DATA-MODEL.md](./DATA-MODEL.md), [FEATURES.md](./FEATURES.md)
