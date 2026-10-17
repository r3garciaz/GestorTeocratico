# Product Requirements Document (PRD)

**Product:** Gestor Teocrático
**Status:** Active development (beta)
**Last updated:** 2026-10-14 (refreshed for the .NET 10 / vertical slice refactor)

> This PRD describes **what the product is and should do**. For **how it is built today**, read
> [ARCHITECTURE.md](./ARCHITECTURE.md), [DATA-MODEL.md](./DATA-MODEL.md) and
> [FEATURES.md](./FEATURES.md); agent-facing rules live in [AGENTS.md](../AGENTS.md).

---

## 1. Product Overview

**Name:** Gestor Teocrático
**Description:** A web application for managing religious congregations, their departments, publishers, responsibilities and meeting schedules. It enables user administration, task assignment, role control and management of the internal organization of a congregation.

---

## 2. Architecture and Technologies

### 2.1. General Architecture

- **Frontend:**
  - Blazor Web App with **Interactive Server** render mode (ASP.NET Core).
  - **BlazorBlueprint** component library (`BlazorBlueprint.Components` + `BlazorBlueprint.Icons.Lucide`) with Tailwind CSS.
  - Razor components (`.razor`) for UI and presentation logic with reactive state.
  - Dependency injection for handlers and navigation.
  - Advanced form handling, real-time validation and interactive dialogs for entity CRUD.
  - Responsive design with modern UI/UX patterns.
  - Feature-based component organization (`Components/Pages/<Feature>`).

- **Backend:**
  - **.NET 10** with **Entity Framework Core 10**.
  - **Vertical Slice architecture**: each feature lives under `Features/<Feature>/` and exposes one
    handler per use case (query/command) instead of a single "god service".
  - Read models are **DTOs / records**; EF entities are never exposed to the UI.
  - Minimal API endpoints for specialized operations (monthly schedule PDF).
  - Authentication and authorization with ASP.NET Identity, roles and a global fallback policy.
  - Automated email notifications with Resend.
  - PDF generation with QuestPDF.

- **Persistence:**
  - **PostgreSQL** via the `Npgsql` provider.
  - EF Core migrations for schema management.
  - Entities support **soft delete** (`SoftDeleteEntity.IsDeleted`) enforced through global query filters.
  - **Unique indexes** (partial, `WHERE "IsDeleted" = false`) protect domain uniqueness (department name,
    publisher full name, responsibility name within a department, congregation name).
  - Data Protection keys stored in the database.

- **Infrastructure:**
  - Docker and Docker Compose for containerized deployment.
  - Multi-stage Dockerfile for optimized production builds.
  - Environment-based configuration.
  - Forwarded headers configuration for reverse proxy / load balancer support.

### 2.2. Main Technologies

| Concern | Technology |
|---|---|
| Language | C# / .NET 10 |
| Web | ASP.NET Core 10, Blazor Web App (Interactive Server) |
| UI | BlazorBlueprint.Components 4.x, BlazorBlueprint.Icons.Lucide, Tailwind CSS |
| ORM | Entity Framework Core 10 |
| Database | PostgreSQL |
| Auth | ASP.NET Core Identity + Google OAuth |
| PDF | QuestPDF |
| Email | Resend |

---

## 3. Domain Entities and Relationships

### 3.1. Entities

- **Congregation:** congregation data (name, address, meeting days). Only one is allowed.
- **Department:** department within the congregation, with a responsible publisher and responsibilities.
- **Publisher:** publisher with personal data, gender, privilege and assignments.
- **Responsibility:** task linked to a department and to publishers.
- **MeetingSchedule:** meeting schedule (date, type, week/month/year) with responsibility assignments.
- **ResponsibilityAssignment:** assignment of a responsibility to a publisher for a specific meeting.
- **PublisherResponsibility:** N:N relationship between publishers and responsibilities.

All entities except the join/meeting ones derive from `SoftDeleteEntity` (soft delete with query filters).

### 3.2. Relationships

- A **Department** has many **Responsibilities**.
- A **Publisher** can be responsible for several **Departments**.
- A **Responsibility** can be assigned to multiple **Publishers** (and vice versa).
- A **MeetingSchedule** has many **ResponsibilityAssignments**.
- **ResponsibilityAssignment** links a **MeetingSchedule**, a **Responsibility** and a **Publisher**.

---

## 4. Vertical Slices (feature map)

Each feature owns its DTOs, queries, commands and handlers under `Features/<Feature>/`:

| Feature | Handlers |
|---|---|
| Congregations | Get, Create, Update |
| Departments | Get, GetById, GetOptions, Create, Update, Delete |
| Publishers | Get, GetById, GetOptions, Create, Update, Delete |
| Responsibilities | Get, GetById, GetOptions, Create, Update, Delete |
| MeetingSchedules (+ assignments) | GetAll, GetByMonth, GetOrCreateWeek, GetAssignments, AssignResponsibility, UnassignResponsibility, CopyWeekAssignments |
| Roles | Thin adapter over `RoleManager<IdentityRole>` (Identity, not an EF entity) |
| PdfExport | Document generation; consumes the Congregations/Responsibilities/MeetingSchedules handlers |

Shared building blocks live in `Features/Common` (e.g. `DuplicateEntityException`) and in each feature's
`Shared/` folder (contracts, mappings, synchronization helpers).

---

## 5. Key Features

- **Congregation Management:** CRUD for the single congregation configuration, including meeting days.
- **Department Management:** CRUD, assignment of a responsible publisher and responsibilities.
- **Publisher Management:** CRUD, assignment to departments and responsibilities, privilege management.
- **Responsibility Management:** CRUD, department and publisher assignment.
- **Meeting Scheduling:** per week/month management of meetings and responsibility assignments, including
  "copy week" to clone assignments between weeks.
- **PDF Export System:** professional monthly schedule reports with QuestPDF.
- **Email Notification System:** account confirmation and password reset via Resend.
- **Dashboard & Analytics:** metrics, statistics and upcoming meetings.
- **Role-Based Access Control:** Admin, Manager and User roles.
- **Soft Delete:** logical deletion with global query filters.
- **Authentication and Authorization:** ASP.NET Identity with Google OAuth, global fallback policy.
- **Personal Data Export:** GDPR-compliant data export.

---

## 6. User Flows

- **Administrator:**
  - Manages congregations, departments, publishers and responsibilities.
  - Schedules meetings and assigns responsibilities, with "copy week" support.
  - Generates and downloads monthly schedule PDFs.
  - Manages users and roles.
  - Monitors the dashboard.

- **Manager:**
  - Manages publishers and responsibilities within their domains.
  - Schedules meetings and assigns responsibilities.
  - Generates reports and exports data.

- **Publisher:**
  - Views assigned responsibilities and upcoming meetings.
  - Receives email notifications for account events.
  - Can export personal data.

---

## 7. Non-Functional Requirements

- **Security:**
  - ASP.NET Core Identity with role-based authorization.
  - Global authorization fallback policy: every endpoint requires authentication unless it explicitly
    allows anonymous access (login, static assets, landing page, not-found).
  - Data Protection keys persisted in the database.
  - Soft delete to prevent accidental data loss.

- **Scalability:**
  - Containerized deployment with Docker and Docker Compose.
  - Feature-based (vertical slice) organization for maintainability and horizontal scaling.
  - Efficient EF Core queries with `AsNoTracking` on read paths and cancellation-token propagation.

- **Maintainability:**
  - Clear separation between UI, use-case handlers and data access.
  - One handler per use case; DTO boundaries between persistence and UI.
  - EF Core migrations for controlled schema evolution.
  - Automated tests backed by SQLite in-memory.

- **Performance:**
  - Server-side Blazor for rendering performance.
  - Efficient PDF generation with QuestPDF.
  - Cancellation-aware queries.

- **Usability / Internationalization:**
  - Responsive UI with BlazorBlueprint components.
  - Spanish (`es-ES`) culture support for dates and numbers.

---

## 8. Testing & Quality

- Test project `GestorTeocratico.Tests` (xUnit) with a SQLite in-memory `IDbContextFactory` so relational
  features (transactions, unique indexes, query filters) behave like production.
- Coverage focused on handlers: create/update/delete, duplicate detection, soft delete, assignment
  synchronization and the "update must not modify related entities" regression.
- Build is expected to be warning-aware and produce 0 errors.

---

## 9. Deployment

- `Dockerfile` (multi-stage) and `compose.yaml` (PostgreSQL + app).
- Environment variables / user secrets for connection strings and provider secrets
  (`RESEND:APITOKEN`, `Authentication:Schemes:GoogleOidc`, `PdfExport:BaseAddress`, emergency admin).
- Migrations and seeding run at startup per environment.

---

## 10. Extensibility

- **Modular architecture:** add a new domain by creating a new feature folder with its contracts and handlers.
- **Handler-based extensibility:** each use case is independent and easily testable/replaceable.
- **UI component system:** BlazorBlueprint components enable rapid new screens.
- **Database evolution:** EF Core migrations support schema changes.
- **Security framework:** role-based access can be extended with custom policies.
- **Notifications:** email infrastructure can be extended to SMS/push.
- **Reporting:** the PDF framework can be extended to more document formats.

The system is well-suited for:
- Integration with AI agents and automation.
- Adding new business features and workflows.
- Scaling to larger congregations and future multi-tenant scenarios.
- Advanced analytics and reporting.
