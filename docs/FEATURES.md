# Features

Inventory of domains, the handlers that own the logic, the UI page, and the rules.
Routes are Spanish-language UI; see the table at the end.

Convention (vertical slice): `Features/<Domain>/Shared/` holds contracts (DTOs), mappings and
sync helpers; **one folder per use case** holds the query/command record and its handler
(`Features/<Domain>/<UseCase>/<UseCase>Handler.cs`). Handlers use
`IDbContextFactory<ApplicationDbContext>` and return **DTOs**, never EF entities. Each handler is
registered as scoped in `Program.cs`.

## Domains

| Domain | Handlers | Page (route) | Access |
|---|---|---|---|
| Congregations | `GetCongregations`, `CreateCongregation`, `UpdateCongregation` | `Congregation/Congregations.razor` (`/congregations`) | Admin, Manager |
| Departments | `GetDepartments`, `GetDepartmentById`, `GetDepartmentOptions`, `CreateDepartment`, `UpdateDepartment`, `DeleteDepartment` | `Department/Departments.razor` (`/departments`) | Admin, Manager |
| Publishers | `GetPublishers`, `GetPublisherById`, `GetPublisherOptions`, `CreatePublisher`, `UpdatePublisher`, `DeletePublisher` | `Publisher/Publishers.razor` (`/publishers`) | Admin, Manager |
| Responsibilities | `GetResponsibilities`, `GetResponsibilityById`, `GetResponsibilityOptions`, `CreateResponsibility`, `UpdateResponsibility`, `DeleteResponsibility` | `Responsibility/Responsibilities.razor` (`/responsibilities`) | Admin, Manager |
| Meeting schedules (+ assignments) | `GetAllMeetingSchedules`, `GetMeetingSchedulesByMonth`, `GetOrCreateWeekSchedules`, `GetAssignmentsByMeetingSchedule`, `AssignResponsibility`, `UnassignResponsibility`, `CopyWeekAssignments` | `MeetingSchedule/MeetingSchedules.razor` (`/meeting-schedules`) | Admin, Manager |
| PDF export | `IPdfExportService` (consumes the handlers above) | triggered from MeetingSchedules / HTTP | — |
| Roles | `IRoleService` | `Administration/Manage/Role/Roles.razor` (`/administration/roles`) | Admin, Manager |
| Users | `UserManager<ApplicationUser>` + `IRoleService` | `Administration/Manage/User/Users.razor` (`/administration/users`) | Admin, Manager |
| Dashboard | read-only aggregation of the above handlers | `Home.razor` (`/dashboard`) | Authenticated |
| Landing | — | `Index.razor` (`/`) | Anonymous |

## Congregations

- **Singleton.** `CreateCongregationHandler` throws `DuplicateEntityException` if a congregation
  already exists (or a duplicate name exists); a unique index on `Congregation.Name` backs it up.
  There is **no delete** path. The UI only edits the existing one.
- Stores four meeting days: even-year and odd-year for both the midweek and the weekend meeting.
- The PDF layer uses these days (with the meeting's `Year % 2`) to compute the real meeting date.

## Departments

- CRUD (soft delete). Duplicate name → `DuplicateEntityException`.
- A department has **one optional `ResponsiblePublisher`** (a publisher with a `Privilege`).
- `CreateDepartmentHandler`/`UpdateDepartmentHandler` persist the department **and** its
  responsibilities in a single transaction via `DepartmentResponsibilitySync`, which sets/clears
  `Responsibility.DepartmentId` (a responsibility belongs to at most one department).
- `GetDepartmentOptionsHandler` returns responsibilities and only publishers that have a privilege.

## Publishers

- CRUD (soft delete). Duplicate `FirstName`+`LastName` → `DuplicateEntityException`.
- Fields: names, optional mother's last name, phone, email, `Gender` (required), `Privilege`
  (optional).
- `CreatePublisherHandler`/`UpdatePublisherHandler` persist the publisher **and** its assignments
  in a single transaction via `PublisherAssignmentSync`:
  - responsibilities (N:N `PublisherResponsibility`);
  - responsible departments (sets `Department.ResponsiblePublisherId` for selected, clears the rest).
- `UpdatePublisherHandler` copies scalar values onto the tracked entity — it never re-marks
  related entities.
- Deletion is logical (`IsDeleted = true`).

## Responsibilities

- CRUD (soft delete). Duplicate `Name` within the same department → `DuplicateEntityException`.
- Optionally belongs to one department.
- `CreateResponsibilityHandler`/`UpdateResponsibilityHandler` persist the responsibility **and**
  which publishers are qualified via `ResponsibilityPublisherSync` (N:N).

## Publisher ↔ Responsibility (qualification)

- The N:N "qualified for" relation (`PublisherResponsibility`, PK `(PublisherId, ResponsibilityId)`)
  is managed **inside the publisher/responsibility slices** (`PublisherAssignmentSync`,
  `ResponsibilityPublisherSync`). The former `PublisherResponsibilityService` was removed.

## Meeting schedules

- A `MeetingSchedule` is one meeting instance: a **week** (`WeekOfYear` ISO + `Year`) and a
  **type** (`Midweek`/`Weekend`). `Date` is the Monday of the week; `Month`/`Year`/`WeekOfYear`
  derive from it.
- `GetOrCreateWeekSchedulesHandler` returns the week's schedules or creates both the midweek and
  weekend ones (`MeetingScheduleReader.EnsureWeekAsync`).
- Queries: `GetAllMeetingSchedulesHandler`, `GetMeetingSchedulesByMonthHandler`.
- `AssignmentDto` carries the publisher name and short name (`F.L.M`) for the UI/PDF.
- **Copy week:** `CopyWeekAssignmentsHandler` synchronizes the target week's assignments with the
  source week's. It uses **only foreign keys** because source/target DTOs are detached; attaching
  navigations would make EF re-insert the whole graph (this caused a duplicate-key bug, fixed in
  commit `dd0ae05`). Synchronization is **by responsibility** (one publisher per responsibility).

## Responsibility assignments

- Composite identity `(MeetingScheduleId, ResponsibilityId)` — **at most one publisher per
  responsibility per meeting**, enforced by the primary key (`PublisherId` is a plain FK column).
  Migration `OneResponsibilityPerMeeting` changed the PK from the previous 3-column key and
  collapsed duplicates.
- `AssignResponsibilityHandler` updates the existing row in place when one exists (otherwise
  inserts), so it can never create a second assignment for the same responsibility.
- `UnassignResponsibilityHandler` removes by `(MeetingScheduleId, ResponsibilityId)`.
- `GetAssignmentsByMeetingScheduleHandler` returns the assignments of a meeting as DTOs.

## Meeting scheduling UI flow

- View modes: **por semana**, **por mes**, **lista completa** (default: month). DataTables render
  one column per responsibility; each cell shows the assigned publisher or an "Asignar" button.
- Assigning a publisher opens a dialog listing only publishers **qualified** for that
  responsibility (`ResponsibilityDto.PublisherIds`); saving replaces the previous assignment.
- "Copiar semana" copies assignments from a source week/year to a target week/year.
- "Exportar PDF" navigates to `/api/meeting-schedules/monthly-schedule/{year}/{month}` (the
  month containing the selected week, or the selected month).

## PDF export

- `GenerateMonthlySchedulePdfAsync(month, year)` builds a `MonthlySchedulePdfModel`
  (responsibilities as columns, one row per meeting) and renders an A4-landscape QuestPDF
  document with a placeholder logo box, header, table, and footer.
- Publisher names are shown in short form (`F.L.M`), taken from `AssignmentDto.PublisherShortName`.
- `ShowInCompanionAsync` renders via the QuestPDF Companion (development aid).
- Exposed over HTTP (now **requires authentication**) — see [API.md](./API.md).

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
| `/Account/*` | Identity pages | Registration, login (`AuthLayout`), manage account (anonymous) |
| `/api/meeting-schedules/monthly-schedule/{year}/{month}` | Minimal API | PDF download (authenticated) |
| `/not-found` | NotFound | Anonymous |
| `/Error` | Error | Anonymous |

## Related docs

- Schema: [DATA-MODEL.md](./DATA-MODEL.md)
- UI patterns: [UI-GUIDE.md](./UI-GUIDE.md)
- Endpoints: [API.md](./API.md)
