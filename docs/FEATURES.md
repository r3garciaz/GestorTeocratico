# Features

Inventory of domains, the service that owns the logic, the UI page, and the rules.
Routes are Spanish-language UI; see the table at the end.

Convention: `Features/<Domain>/I<X>Service.cs` (contract) + `<X>Service.cs` (implementation),
registered in `Program.cs`. All services use `IDbContextFactory<ApplicationDbContext>`.

## Domains

| Domain | Service | Page (route) | Access |
|---|---|---|---|
| Congregations | `ICongregationService` | `Congregation/Congregations.razor` (`/congregations`) | Admin, Manager |
| Departments | `IDepartmentService` | `Department/Departments.razor` (`/departments`) | Admin, Manager |
| Publishers | `IPublisherService` | `Publisher/Publishers.razor` (`/publishers`) | Admin, Manager |
| Responsibilities | `IResponsibilityService` | `Responsibility/Responsibilities.razor` (`/responsibilities`) | Admin, Manager |
| Publisher ↔ Responsibility | `IPublisherResponsibilityService` | (used inside Publisher/Responsibility pages) | — |
| Meeting schedules | `IMeetingScheduleService` | `MeetingSchedule/MeetingSchedules.razor` (`/meeting-schedules`) | Admin, Manager |
| Responsibility assignments | `IResponsibilityAssignmentService` | (used inside MeetingSchedules page) | — |
| PDF export | `IPdfExportService` | triggered from MeetingSchedules / HTTP | — |
| Roles | `IRoleService` | `Administration/Manage/Role/Roles.razor` (`/administration/roles`) | Admin, Manager |
| Users | `UserManager<ApplicationUser>` + `IRoleService` | `Administration/Manage/User/Users.razor` (`/administration/users`) | Admin, Manager |
| Dashboard | read-only aggregation of the above | `Home.razor` (`/dashboard`) | Authenticated |
| Landing | — | `Index.razor` (`/`) | Anonymous |

## Congregations

- **Singleton.** `AddAsync` throws `InvalidOperationException` if any congregation exists (or a
  duplicate name exists), and the UI only edits the existing one. `DeleteAsync` throws
  `NotSupportedException`.
- Stores four meeting days: even-year and odd-year for both the midweek and the weekend meeting.
- The PDF layer uses these days (with the meeting's `Year % 2`) to compute the real meeting date.

## Departments

- CRUD (soft delete). Names must be unique (duplicate add is logged and ignored).
- A department has **one optional `ResponsiblePublisher`** (a publisher with a `Privilege`).
- `UpdateDepartmentResponsibilitiesAsync` sets/clears `Responsibility.DepartmentId` so a
  responsibility belongs to at most one department.
- `GetAvailablePublishersAsync` returns only publishers that have a privilege.

## Publishers

- CRUD (soft delete). Duplicate `FirstName`+`LastName` add is logged and ignored.
- Fields: names, optional mother's last name, phone, email, `Gender` (required), `Privilege`
  (optional).
- `UpdatePublisherResponsibilitiesAsync` replaces the publisher's qualified responsibilities.
- `UpdatePublisherDepartmentsAsync` sets the publisher as `ResponsiblePublisher` for selected
  departments and clears it for the rest.
- Deletion is logical (`IsDeleted = true`).

## Responsibilities

- CRUD (soft delete). Duplicate `Name` within the same department is logged and ignored.
- Optionally belongs to one department.
- `UpdateResponsibilityPublishersAsync` replaces which publishers are qualified.

## Publisher ↔ Responsibility (qualification)

- `PublisherResponsibilityService` manages the N:N "qualified for" relation (composite key).
- `AssignResponsibilityToPublisherAsync` / `RemoveResponsibilityFromPublisherAsync` wrap
  create/delete; failures return `false` instead of throwing.

## Meeting schedules

- A `MeetingSchedule` is one meeting instance: a **week** (`WeekOfYear` ISO + `Year`) and a
  **type** (`Midweek`/`Weekend`). `Date` is the Monday of the week; `Month`/`Year`/`WeekOfYear`
  derive from it.
- `GetOrCreateWeekSchedulesAsync(week, year)` returns existing schedules for the week or creates
  both the midweek and weekend ones.
- `GetOrCreateMeetingScheduleAsync(week, year, type)` is the single-item version.
- Queries: by date range, by month, by week, by type, by (date + type).
- **Copy week:** `CopyAssignmentsToWeekAsync(source…, target…)` synchronizes the target week's
  assignments with the source week's. It uses **only foreign keys** (`.Select(a => (a.ResponsibilityId, a.PublisherId))`)
  because source/target entities come from different contexts and are detached; attaching their
  navigations would make EF re-insert the whole graph (this caused a duplicate-key bug, fixed in
  commit `dd0ae05`).

## Responsibility assignments

- Composite identity `(MeetingScheduleId, ResponsibilityId, PublisherId)`.
- Constraint enforced in `AssignResponsibilityAsync`: **at most one publisher per
  responsibility per meeting** — it removes any existing assignment for that
  meeting/responsibility before inserting.
- `CreateAsync` validates that schedule, publisher, and responsibility exist and that the exact
  assignment does not already exist.
- `RemoveAssignmentAsync` aliases `DeleteAsync` (hard delete of the join row).

## Meeting scheduling UI flow

- View modes: **por semana**, **por mes**, **lista completa** (default: month). DataTables render
  one column per responsibility; each cell shows the assigned publisher or an "Asignar" button.
- Assigning a publisher opens a dialog listing only publishers **qualified** for that
  responsibility (`responsibility.PublisherResponsibilities`); saving removes the previous
  assignment and creates the new one.
- "Copiar semana" copies assignments from a source week/year to a target week/year.
- "Exportar PDF" navigates to `/api/meeting-schedules/monthly-schedule/{year}/{month}` (the
  month containing the selected week, or the selected month).

## PDF export

- `GenerateMonthlySchedulePdfAsync(month, year)` builds a `MonthlySchedulePdfModel`
  (responsibilities as columns, one row per meeting) and renders an A4-landscape QuestPDF
  document with a placeholder logo box, header, table, and footer.
- Publisher names are shown in short form (`F.L.M`).
- `ShowInCompanionAsync` renders via the QuestPDF Companion (development aid).
- Exposed over HTTP — see [API.md](./API.md).

## Roles

- `IRoleService` wraps `RoleManager<IdentityRole>`: list/get/create/update/delete.
- Errors from Identity surface as `InvalidOperationException` with joined error descriptions.
- Roles seeded: `Admin`, `Manager`, `User`.

## Users

- Page uses `UserManager<ApplicationUser>` and `IRoleService`; shows counts, search by email /
  public ID, role filter, lock state, and role management per user.

## Dashboard

- KPIs: publishers (with elder/servant breakdown), departments, responsibilities, congregations.
- "Próximas reuniones": the next 5 schedules by date.
- Quick actions to the main sections.

## Routes

| Route | Page | Notes |
|---|---|---|
| `/` | Index | Public landing |
| `/dashboard` | Home | Authenticated |
| `/congregations` | Congregations | Admin, Manager |
| `/departments` | Departments | Admin, Manager |
| `/responsibilities` | Responsibilities | Admin, Manager |
| `/publishers` | Publishers | Admin, Manager |
| `/meeting-schedules` | MeetingSchedules | Admin, Manager |
| `/administration/users` | Users | Admin, Manager |
| `/administration/roles` | Roles | Admin, Manager |
| `/Account/*` | Identity pages | Registration, login, manage account |
| `/api/meeting-schedules/monthly-schedule/{year}/{month}` | Minimal API | PDF download |
| `/not-found` | NotFound | Anonymous |

## Related docs

- Schema: [DATA-MODEL.md](./DATA-MODEL.md)
- UI patterns: [UI-GUIDE.md](./UI-GUIDE.md)
- Endpoints: [API.md](./API.md)
