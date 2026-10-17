# Data Model

Persistence is PostgreSQL 17 via EF Core 10, configured in
`GestorTeocratico/Data/ApplicationDbContext.cs`. The context extends `IdentityDbContext<ApplicationUser>`
(tables for users, roles, claims, tokens, logins) and implements `IDataProtectionKeyContext`
(keys persisted in the DB).

## Entities

| Entity | Table | Soft delete | Key |
|---|---|---|---|
| `Congregation` | `Congregations` | ✅ | `CongregationId` (Guid v7) |
| `Department` | `Departments` | ✅ | `DepartmentId` (Guid v7) |
| `Publisher` | `Publishers` | ✅ | `PublisherId` (Guid v7) |
| `Responsibility` | `Responsibilities` | ✅ | `ResponsibilityId` (Guid v7) |
| `MeetingSchedule` | `MeetingSchedules` | ❌ | `MeetingScheduleId` (Guid v7) |
| `PublisherResponsibility` (join) | `PublisherResponsibilities` | ❌ | `(PublisherId, ResponsibilityId)` |
| `ResponsibilityAssignment` (join) | `ResponsibilityAssignments` | ❌ | `(MeetingScheduleId, ResponsibilityId)` |
| `ApplicationUser` | Identity | ❌ | Identity `Id` (+ unique `PublicId`) |
| `DataProtectionKey` | Identity/DP | ❌ | Data Protection |

IDs are created with `Guid.CreateVersion7()` (UUIDv7 — time-ordered).

### Field notes

- **Congregation** — `Name` (250, required), four meeting-day fields
  (`MidweekMeetingDayEvenYear`, `MidweekMeetingDayOddYear`, `WeekendMeetingDayEvenYear`,
  `WeekendMeetingDayOddYear`, all `DayOfWeek`, stored as string), `Address` (500), `City` (250).
  **Only one congregation is allowed**: `CreateCongregationHandler` throws
  `DuplicateEntityException`, backed by a **unique index on `Congregation.Name`**.
- **Department** — `Name` (250, required), optional `ResponsiblePublisherId` → `Publisher`
  (`OnDelete: Restrict`). Has many `Responsibilities`. Unique index on `Name` (non-deleted).
- **Publisher** — `FirstName`/`LastName` (250, required), `MotherLastName` (250), `Phone` (15),
  `Email` (250), `Gender` (`Gender`, required, string), `Privilege` (`Privilege?`, string).
  Has many `ResponsibleDepartments` and `PublisherResponsibilities`. Unique index on
  `(FirstName, LastName)` (non-deleted).
- **Responsibility** — `Name` (100, required), `Description` (500), optional `DepartmentId`.
  Unique index on `(Name, DepartmentId)` (non-deleted).
- **MeetingSchedule** — `Date` (`DateOnly`), plus derived `Month`, `Year`, `WeekOfYear`
  (ISO-8601 week, computed in the `Date` setter), and `MeetingType` (`MeetingType`, string).
  Has many `ResponsibilityAssignments`. **Not** soft-deletable.
- **PublisherResponsibility** — which publishers are *qualified* for a responsibility (N:N).
- **ResponsibilityAssignment** — an actual assignment for a meeting: links a `MeetingSchedule`,
  a `Responsibility`, and a `Publisher`. The primary key is
  `(MeetingScheduleId, ResponsibilityId)` — **at most one publisher per responsibility per
  meeting**; `PublisherId` is a plain FK column. This is the core of the "programación".

## Enums (`GestorTeocratico/Shared/Enums/`)

```csharp
public enum Gender     { Male = 1, Female = 2 }
public enum MeetingType{ Midweek = 0, Weekend = 1 }
public enum Privilege  { Elder = 1, MinisterialServant = 2, Pioneer = 3 }
```

`Gender`, `MeetingType`, and `Privilege` are persisted as **strings** (`HasConversion<string>()`).

## Relationships

```
Congregation (1)                  (no navigation to Department; logical singleton)
Department   (1) ──< Responsibility
Department   *  ──   Publisher           (ResponsiblePublisher, Restrict)
Publisher    *  ──< PublisherResponsibility >── * Responsibility
MeetingSchedule (1) ──< ResponsibilityAssignment >── (1) Responsibility
                        ResponsibilityAssignment >── (1) Publisher
```

- All FK delete behaviors are **`Restrict`** — deletions are logical, not cascading.
- `ResponsibilityAssignment` uses the **FKs** (non-nullable) to establish required
  relationships, while navigations are declared `null!` for convenience.

## Uniqueness indexes

Uniqueness is enforced in the database with **partial unique indexes** filtered to
non-soft-deleted rows (`WHERE "IsDeleted" = false`), so a deleted row does not block re-creating
the same value:

| Entity | Index |
|---|---|
| `Congregation` | `Name` (unique) |
| `Department` | `Name` (unique, non-deleted) |
| `Publisher` | `(FirstName, LastName)` (unique, non-deleted) |
| `Responsibility` | `(Name, DepartmentId)` (unique, non-deleted) |

The handler layer also checks for duplicates and throws `DuplicateEntityException` (friendly
message) as the first line of defence; the index is the concurrency-proof backstop.

## Soft delete

`Entities/SoftDeleteEntity.cs`:

```csharp
public abstract class SoftDeleteEntity { public bool IsDeleted { get; set; } }
```

`ApplicationDbContext.OnModelCreating` iterates all entity types and, for every
`SoftDeleteEntity` subclass:

1. applies a global query filter `e => e.IsDeleted == false`;
2. marks `IsDeleted` as required.

Because join entities reference soft-deletable parents, matching filters are set explicitly to
avoid EF warning 10622 and to keep joins consistent:

```csharp
modelBuilder.Entity<PublisherResponsibility>()
    .HasQueryFilter(pr => !pr.Publisher.IsDeleted && !pr.Responsibility.IsDeleted);
modelBuilder.Entity<ResponsibilityAssignment>()
    .HasQueryFilter(ra => !ra.Publisher.IsDeleted && !ra.Responsibility.IsDeleted);
```

Implications for agents:

- A soft-deleted `Publisher`/`Responsibility` disappears from its join rows automatically.
- To read deleted rows (e.g. for idempotent seeding) use `.IgnoreQueryFilters()` — already used
  in `DataSeederDevelopment` for Department/Responsibility/Publisher existence checks.
- The soft-delete `Delete*Handler` sets `IsDeleted = true` and saves; it does **not** call
  `Remove`.

## Identity data

- `ApplicationUser : IdentityUser` adds `PublicId` (max length 20, required, unique index) — a
  short human-facing ID surfaced as the `public_id` claim.
- Roles are seeded constants in `GestorTeocratico/Shared/Roles.cs`:
  `Admin`, `Manager`, `User`.

## Migrations (`GestorTeocratico/Data/Migrations/`)

| Migration | Purpose |
|---|---|
| `20250805224840_Initial` | Initial schema |
| `20250805235204_publisher_modify_properties` | Publisher field changes |
| `20250811214431_AddDataProtectionPersistInDatabase` | Persist Data Protection keys |
| `20250819175958_ApplicationUserAddPublicId` | Add `ApplicationUser.PublicId` |
| `AddUniqueIndexes` | Partial unique indexes on Congregation/Department/Publisher/Responsibility |
| `OneResponsibilityPerMeeting` | Change `ResponsibilityAssignment` PK to `(MeetingScheduleId, ResponsibilityId)` and collapse duplicates |

Apply with `dotnet ef database update --project GestorTeocratico`. In Development/Production the
app applies migrations automatically at startup (`dbContext.Database.MigrateAsync()`).

> **Data prerequisite:** `AddUniqueIndexes` fails if existing rows already violate uniqueness
> (same department name, publisher full name, or responsibility name within a department).
> Deduplicate those rows before applying it. `OneResponsibilityPerMeeting` handles its own
> duplicates (it keeps one assignment per responsibility before switching the PK).

## Seeding

| Seeder | Used in | Contents |
|---|---|---|
| `DataSeederDevelopment.SeedDataAsync` | Development | 1 congregation, 3 departments, 8 responsibilities, 31 sample publishers (all male), every publisher qualified for every responsibility, 3 roles |
| `DataSeeder.SeedDataAsync` | Production | 1 congregation, 3 departments (Audio y Video, Acomodación, Literatura), 8 responsibilities, 3 roles |
| `DataSeeder.SeedEmergencyAdminAsync` | Both (if configured) | Local admin from `EmergencyAdmin:Email/Password` |

Seeders are **idempotent**: they check for existing rows by fixed GUID/name before inserting.

## Related docs

- Query/factory rules: [ARCHITECTURE.md](./ARCHITECTURE.md)
- Business rules per entity: [FEATURES.md](./FEATURES.md)
