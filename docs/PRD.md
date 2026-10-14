# Product Requirements Document (PRD)

**Product:** Gestor Teocrático
**Status:** Active development (beta)
**Last updated:** 2026 (refreshed from the original `Shared/PRD.md`, which described an obsolete
Radzen/.NET 9 design and was corrupted).

> This PRD describes **what the product is and should do**. For **how it is built today**, read
> [ARCHITECTURE.md](./ARCHITECTURE.md), [DATA-MODEL.md](./DATA-MODEL.md), and
> [FEATURES.md](./FEATURES.md). Scope marked *Planned* is not implemented yet
> ([ROADMAP.md](./ROADMAP.md)).

---

## 1. Overview

Gestor Teocrático is a web application that helps a religious congregation organize its
operational work: who serves in each **department**, which **responsibilities** exist, which
**publishers** are qualified for them, and the **meeting schedule** that assigns publishers to
responsibilities week by week. It replaces spreadsheets with a single, role-aware, printable
system.

### Problem

Congregation scheduling is repetitive and error-prone when done in spreadsheets: qualifications,
department ownership, and the weekly program are easy to desync, and there is no reliable way to
print a clean monthly schedule.

### Goals

- Centralize publishers, departments, responsibilities, and schedules.
- Make weekly assignment fast and safe (assign only *qualified* publishers; one publisher per
  responsibility per meeting).
- Produce a printable monthly schedule (PDF).
- Control access by role and keep data recoverable (soft delete).

### Non-goals (current)

- Multi-tenant / multi-congregation management (the app is a **singleton congregation**).
- Public REST API / third-party integrations (see [API.md](./API.md)).
- Native mobile apps.

## 2. Users and roles

| Role | Description | Can access |
|---|---|---|
| **Admin** | Congregation overseer / system owner. | Everything, including users & roles |
| **Manager** | Department lead. | Configuration + meetings (same pages as Admin in the current UI) |
| **User** | Regular authenticated member. | Dashboard and account only |

- The **first account created becomes Admin** automatically.
- A **break-glass local admin** can be provisioned via configuration when Google is unavailable.
- Authentication is Google OAuth; a local password login exists as a fallback.

## 3. Key features

- **Congregation** — single record with meeting days (separate even/odd-year days for the midweek
  and weekend meetings), address, and city.
- **Departments** — CRUD; one responsible publisher; owns responsibilities.
- **Publishers** — CRUD with contact data, gender, privilege (Elder / Ministerial Servant /
  Pioneer), qualified responsibilities, and departments they oversee.
- **Responsibilities** — CRUD; optionally owned by a department; qualified publishers.
- **Meeting schedules** — one record per week + meeting type (midweek/weekend); weeks are created
  on demand.
- **Assignment** — assign a qualified publisher to a responsibility for a given meeting; copy a
  whole week's assignments to another week.
- **PDF export** — A4-landscape monthly schedule with responsibilities as columns and meetings as
  rows.
- **Dashboard** — KPI cards and the next meetings.
- **Administration** — users (roles, lock state, search) and roles.
- **Account** — registration, login, password reset, 2FA, external logins, personal-data download.
- **Soft delete** — congregations/departments/publishers/responsibilities are logically deleted.

## 4. Functional requirements

### 4.1 Congregation
- Exactly one congregation may exist; attempting to create a second or delete it is rejected.
- Meeting day is configured per even/odd year for both meeting types; the PDF uses these to
  compute the real meeting date for a given week.

### 4.2 Departments
- Names are unique; duplicates are rejected (logged).
- Assigning responsibilities to a department moves them; responsibilities can belong to at most
  one department.

### 4.3 Publishers
- `FirstName` + `Gender` are required; `Privilege` is optional.
- Duplicate full names are rejected (logged).
- Responsible-publisher assignment is exclusive: setting a publisher on departments clears it
  elsewhere.

### 4.4 Responsibilities
- Names unique within a department.
- Maintains the set of qualified publishers.

### 4.5 Meeting scheduling
- A week has at most one midweek and one weekend schedule.
- A responsibility has at most one assigned publisher per meeting.
- Only publishers qualified for a responsibility can be assigned to it.
- Assignments can be copied from a source week to a target week (target is synchronized).

### 4.6 PDF export
- Input: month + year (2020–2030). Output: downloadable PDF named
  `Programacion_{Mes}_{Year}.pdf`.

### 4.7 Users and roles
- Roles: `Admin`, `Manager`, `User`. Users can be searched by email/public ID and filtered by
  role; lock state is visible and manageable.

## 5. Non-functional requirements

- **Security:** OAuth + ASP.NET Identity; role-based page authorization; Data Protection keys in
  the database; antiforgery on forms; safe redirects.
- **Reliability / data safety:** soft delete; no cascading deletes; idempotent seeding.
- **Usability:** responsive Blazor Blueprint UI; Spanish (es-ES); accessible components.
- **Maintainability:** feature-based services behind interfaces; per-operation `DbContext`;
  documented conventions.
- **Portability:** Docker/Compose deployment; PostgreSQL 17.
- **Performance:** server-side Blazor; indexed/limited queries; PDF rendered on demand.

## 6. Data privacy

- Account personal data can be downloaded (`/Account/Manage/DownloadPersonalData`) and the account
  deleted via Identity management pages.
- Publisher records are soft-deleted to preserve history; hard deletion is not exposed.

## 7. Success metrics

- Weekly assignment takes minutes, not hours, and never assigns an unqualified publisher.
- A correct monthly PDF can be produced in one click.
- No data loss from accidental deletion.

## 8. Related documents

- [ROADMAP.md](./ROADMAP.md) — status and planned work
- [FEATURES.md](./FEATURES.md) — implemented behavior in detail
- [ARCHITECTURE.md](./ARCHITECTURE.md) · [DATA-MODEL.md](./DATA-MODEL.md) · [AUTH.md](./AUTH.md)
