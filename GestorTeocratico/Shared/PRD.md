# Product Requirement Document (PRD)

## 1. Product Overview

**Name:** Gestor Teocratico  
**Description:**  
Gestor Teocratico is a web application for managing religious congregations, their departments, publishers, responsibilities, and meeting schedules. It enables user administration, task assignment, role control, and management of relevant information for the internal organization of a congregation.

---

## 2. Architecture and Technologies

### 2.1. General Architecture

- **Frontend:**  
  - Based on [Blazor](https://dotnet.microsoft.com/en-us/apps/aspnet/web-apps/blazor) (Radzen Blazor Components).
  - Razor components (`.razor`) for UI and presentation logic.
  - Dependency injection for services and navigation.
  - Form handling, validations, and modals for entity CRUD.

- **Backend:**  
  - ASP.NET Core with Entity Framework Core.
  - Service pattern for business logic (e.g., `DepartmentService`, `PublisherService`).
  - Controllers and services for CRUD operations and business logic.
  - Authentication and authorization with ASP.NET Identity.

- **Persistence:**  
  - PostgreSQL (using `Npgsql` in migrations).
  - Migrations managed with Entity Framework Core.
  - Entities support soft delete (`IsDeleted` property).

- **Infrastructure:**  
  - Docker and Docker Compose for deployment.
  - Environment configuration via `appsettings.json` and environment variables.

---

### 2.2. Main Technologies

- **Language:** C# (.NET 8/9)
- **Web Framework:** ASP.NET Core, Blazor Server
- **ORM:** Entity Framework Core
- **Database:** PostgreSQL
- **UI:** Radzen Blazor Components
- **Authentication:** ASP.NET Identity
- **Containers:** Docker

---

## 3. Main Entities and Relationships

### 3.1. Entities

- **Congregation:** General information about the congregation (name, address, meeting days).
- **Department:** Department within the congregation, with a manager and associated responsibilities.
- **Publisher:** Publisher, with personal data, gender, privilege, and assigned departments/responsibilities.
- **Responsibility:** Task or responsibility, linked to a department and publishers.
- **MeetingSchedule:** Meeting schedule, with responsibility assignments.
- **ResponsibilityAssignment:** Assignment of a responsibility to a publisher for a specific meeting.
- **PublisherResponsibility:** N:N relationship between publishers and responsibilities.

### 3.2. Relationships

- A **Department** has many **Responsibilities**.
- A **Publisher** can be responsible for several **Departments**.
- A **Responsibility** can be assigned to multiple **Publishers** (and vice versa).
- A **MeetingSchedule** has many **ResponsibilityAssignments**.
- **ResponsibilityAssignment** links a **MeetingSchedule**, a **Responsibility**, and a **Publisher**.

---

## 4. Key Features

- **Congregation Management:** CRUD for congregations.
- **Department Management:** CRUD for departments, assignment of manager and responsibilities.
- **Publisher Management:** CRUD for publishers, assignment to departments and responsibilities.
- **Responsibility Management:** CRUD for responsibilities, assignment to departments and publishers.
- **Meeting Scheduling:** Create and manage meetings, assign responsibilities to publishers.
- **Soft Delete:** Logical deletion of entities to prevent data loss.
- **Authentication and Authorization:** Registration, login, user and role management.
- **Personal Data Export:** Download of personal data for privacy compliance.

---

## 5. User Flows

- **Administrator:**  
  - Accesses the admin panel.
  - Manages congregations, departments, publishers, and responsibilities.
  - Assigns tasks and roles.
  - Schedules meetings and assigns responsibilities.

- **Publisher:**  
  - Views their responsibilities and assignments.
  - Accesses contact and department information.

---

## 6. Non-Functional Requirements

- **Security:**  
  - Secure authentication with Identity.
  - Personal data protection.
  - Soft delete to avoid physical data removal.

- **Scalability:**  
  - Use of Docker containers for deployment and scaling.
  - Modular, service-based architecture.

- **Maintainability:**  
  - Clear separation between business logic, data, and presentation.
  - Use of migrations for database changes.

- **Internationalization:**  
  - Support for multiple languages (potential, not explicit in code).

---

## 7. Considerations for AI Agents

- **API Interaction:**  
  - Agents can interact with backend services for entity CRUD.
  - Services are well-defined and decoupled, facilitating integration.

- **Workflow Automation:**  
  - Agents can automate responsibility assignments, report generation, and notifications.

- **Privacy and Compliance:**  
  - Personal data export functionality available.
  - Soft delete implemented for data protection.

---

## 8. Key Files and Components

- **Frontend:**  
  - `/Components/Pages/Department/`, `/Components/Pages/Publisher/`, `/Components/Pages/Responsibility/`
- **Backend:**  
  - `GestorTeocratico/Features/Departments/DepartmentService.cs`
  - `GestorTeocratico/Features/Publishers/PublisherService.cs`
  - `GestorTeocratico/Features/Responsibilities/ResponsibilityService.cs`
  - `GestorTeocratico/Features/MeetingSchedules/MeetingScheduleService.cs`
  - `GestorTeocratico/Features/ResponsibilityAssignments/ResponsibilityAssignmentService.cs`
- **Data:**  
  - `GestorTeocratico/Data/ApplicationDbContext.cs`
  - `/Entities/`
  - `/Data/Migrations/`

---

## 9. Extensibility Summary

- The system is ready to add new entities, relationships, and workflows.
- The service- and component-based architecture facilitates integration with AI agents for automation, analysis, and user