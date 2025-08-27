# Product Requirement Document (PRD)

## 1. Product Overview

**Name:** Gestor Teocratico  
**Description:**  
Gestor Teocratico is a web application for managing religious congregations, their departments, publishers, responsibilities, and meeting schedules. It enables user administ## 10. Extensibility Summary

- **Modular Architecture:** Feature-based organization enables easy addition of new business domains and capabilities.
- **Service Extensibility:** Well-defined interfaces allow for service enhancement and replacement without affecting dependent components.
- **API Extensibility:** Both minimal APIs and traditional controllers provide flexible integration points for external systems and AI agents.
- **UI Component System:** Radzen Blazor Components foundation allows for rapid development of new user interfaces and interactive features.
- **Database Evolution:** Entity Framework migrations support seamless schema changes and new entity addition.
- **Security Framework:** Role-based system can be extended with custom permissions and authorization policies.
- **Notification System:** Email infrastructure can be extended to support SMS, push notifications, and other communication channels.
- **Reporting System:** PDF generation framework can be extended to support additional document formats and custom templates.
- **Configuration Management:** Environment-based configuration supports easy deployment across different environments and cloud platforms.
- **Monitoring & Analytics:** Dashboard infrastructure provides foundation for advanced analytics, monitoring, and business intelligence features.

The system architecture prioritizes maintainability, testability, and extensibility, making it well-suited for:
- Integration with AI agents and automation systems
- Addition of new business features and workflows  
- Scaling to support larger congregations and multi-tenant scenarios
- Enhancement with advanced analytics and reporting capabilities
- Migration to cloud platforms and distributed architectureson, task assignment, role control, and management of relevant information for the internal organization of a congregation.

---

## 2. Architecture and Technologies

### 2.1. General Architecture

- **Frontend:**  
  - Based on [Blazor Server](https://dotnet.microsoft.com/en-us/apps/aspnet/web-apps/blazor) with [Radzen Blazor Components](https://blazor.radzen.com/).
  - Razor components (`.razor`) for UI and presentation logic with reactive state management.
  - Dependency injection for services and navigation.
  - Advanced form handling, real-time validations, and interactive modals for entity CRUD.
  - Responsive design with modern UI/UX patterns.
  - Feature-based component organization for better maintainability.

- **Backend:**  
  - ASP.NET Core 9.0 with Entity Framework Core 9.0.
  - Feature-based service architecture organized by domain (e.g., `DepartmentService`, `PublisherService`, `PdfExportService`).
  - Minimal APIs and traditional controllers for CRUD operations and business logic.
  - Advanced authentication and authorization with ASP.NET Identity and role-based access control.
  - Automated email notifications with Resend integration.
  - PDF generation capabilities with QuestPDF for professional document export.
  - Comprehensive logging and error handling throughout the application.

- **Persistence:**  
  - PostgreSQL 17 with Npgsql 9.0.4 driver for optimal performance.
  - Entity Framework Core migrations for database schema management.
  - Entities support soft delete (`IsDeleted` property) for data protection.
  - Data Protection keys stored in database for enhanced security.
  - Automated data seeding for both development and production environments.
  - Comprehensive indexing and query optimization.

- **Infrastructure:**  
  - Docker and Docker Compose for containerized deployment and development.
  - Multi-stage Dockerfile for optimized production builds.
  - Environment-specific configuration via `appsettings.json` and environment variables.
  - PostgreSQL 17 container with persistent volume storage.
  - Forwarded headers configuration for proxy and load balancer support.
  - Health checks and monitoring capabilities.

---

### 2.2. Main Technologies

- **Language:** C# (.NET 9.0)
- **Web Framework:** ASP.NET Core 9.0, Blazor Server
- **ORM:** Entity Framework Core 9.0
- **Database:** PostgreSQL 17
- **UI:** Radzen Blazor Components 7.1.6
- **Authentication:** ASP.NET Identity
- **PDF Generation:** QuestPDF 2025.7.0
- **Email Service:** Resend 0.1.5
- **Containers:** Docker & Docker Compose

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

- **Congregation Management:** CRUD for congregations with detailed configuration including meeting days and schedules.
- **Department Management:** CRUD for departments, assignment of managers and responsibilities.
- **Publisher Management:** CRUD for publishers, assignment to departments and responsibilities, privilege management.
- **Responsibility Management:** CRUD for responsibilities, assignment to departments and publishers.
- **Meeting Scheduling:** Create and manage meetings, assign responsibilities to publishers with automated scheduling.
- **PDF Export System:** Generate professional monthly schedule reports using QuestPDF with automated formatting.
- **Email Notification System:** Automated email notifications for account confirmation, password reset, and system updates using Resend.
- **Dashboard & Analytics:** Interactive dashboard with real-time metrics, statistics, and system health monitoring.
- **Role-Based Access Control:** Granular permission system with Admin, Manager, and User roles.
- **Soft Delete:** Logical deletion of entities to prevent data loss and maintain audit trails.
- **Authentication and Authorization:** Complete user management with ASP.NET Identity.
- **Personal Data Export:** GDPR-compliant data export functionality for privacy compliance.
- **API Endpoints:** RESTful APIs and minimal endpoints for external integration and automation.

---

## 5. User Flows

- **Administrator:**  
  - Accesses the comprehensive admin dashboard with real-time metrics and system status.
  - Manages congregations, departments, publishers, and responsibilities with advanced filtering and search.
  - Assigns tasks and roles with granular permission control.
  - Schedules meetings and assigns responsibilities with automated conflict detection.
  - Generates and downloads professional PDF reports for monthly schedules.
  - Manages user accounts, roles, and system-wide configurations.
  - Monitors system health and receives automated notifications.

- **Manager:**  
  - Accesses department-specific management tools and analytics.
  - Manages publishers and responsibilities within assigned departments.
  - Schedules meetings and assigns responsibilities for their domains.
  - Generates departmental reports and exports data.
  - Receives email notifications for important updates and assignments.

- **Publisher:**  
  - Views personal dashboard with assigned responsibilities and upcoming meetings.
  - Accesses contact information and department details.
  - Receives email notifications for new assignments and schedule changes.
  - Can export personal data for privacy compliance.
  - Updates personal profile information and preferences.

---

## 6. Non-Functional Requirements

- **Security:**  
  - Enterprise-grade authentication with ASP.NET Identity.
  - Role-based authorization with granular permissions (Admin, Manager, User).
  - Data Protection keys stored securely in database.
  - Personal data protection with GDPR compliance features.
  - Soft delete implementation to prevent accidental data loss.
  - Secure email delivery with Resend integration.

- **Scalability:**  
  - Containerized deployment with Docker and Docker Compose.
  - Modular, feature-based service architecture for horizontal scaling.
  - Efficient database queries with Entity Framework Core optimization.
  - Minimal API endpoints for high-performance operations.

- **Maintainability:**  
  - Clear separation between business logic, data access, and presentation layers.
  - Feature-based organization for better code discoverability.
  - Comprehensive logging and error handling throughout the application.
  - Database migrations for controlled schema evolution.
  - Automated testing capabilities and code quality standards.

- **Performance:**  
  - Server-side Blazor for optimal rendering performance.
  - Efficient PDF generation with QuestPDF.
  - Database indexing and query optimization.
  - Caching strategies for frequently accessed data.

- **Usability:**  
  - Responsive design with modern UI/UX patterns using Radzen components.
  - Real-time updates and interactive dashboard widgets.
  - Intuitive navigation and user-friendly interfaces.
  - Comprehensive error messages and user guidance.

- **Internationalization:**  
  - Spanish (es-ES) culture support with proper date and number formatting.
  - Localized text throughout the application interface.
  - Cultural-specific formatting using `CultureInfo`.
  - Foundation for future multi-language support expansion.

---

## 7. Considerations for AI Agents

- **API Interaction:**  
  - Well-defined service interfaces for seamless integration with AI agents.
  - RESTful endpoints and minimal APIs for external system communication.
  - Comprehensive error handling and response formatting for reliable automation.
  - Services are decoupled and follow dependency injection patterns for easy testing and integration.

- **Workflow Automation:**  
  - Automated responsibility assignment algorithms with conflict detection.
  - PDF report generation with customizable templates and scheduling.
  - Email notification system for automated communication workflows.
  - Dashboard metrics and analytics for intelligent decision-making support.
  - Meeting schedule optimization and resource allocation automation.

- **Data Analytics & Intelligence:**  
  - Real-time dashboard metrics for performance monitoring and insights.
  - Publisher assignment patterns and optimization opportunities.
  - Department workload analysis and balancing recommendations.
  - Historical data trends for predictive scheduling and planning.

- **Privacy and Compliance:**  
  - GDPR-compliant personal data export functionality with automated processing.
  - Comprehensive audit trails through soft delete implementation.
  - Secure data handling with encryption and access control.
  - Automated compliance reporting and data retention policies.

- **Integration Capabilities:**  
  - Feature-based service architecture enables modular AI agent integration.
  - Event-driven patterns for real-time updates and notifications.
  - Extensible plugin architecture for custom AI-powered features.
  - Support for batch processing and bulk operations for large-scale automation.

---

## 8. Key Files and Components

### Frontend Components
- **Pages:** `/Components/Pages/` - Main application pages organized by feature
  - `Department/` - Department management interfaces
  - `Publisher/` - Publisher management with advanced filtering and search
  - `Responsibility/` - Responsibility assignment and management
  - `MeetingSchedule/` - Meeting scheduling with drag-and-drop interface
  - `Administration/` - Admin-only system management interfaces
  - `Home.razor` - Interactive dashboard with real-time metrics
  - `Index.razor` - Landing page with system overview

### Backend Services
- **Core Services:** Feature-based organization in `/Features/`
  - `Departments/DepartmentService.cs` - Department business logic
  - `Publishers/PublisherService.cs` - Publisher management with role validation
  - `Responsibilities/ResponsibilityService.cs` - Responsibility assignment logic
  - `MeetingSchedules/MeetingScheduleService.cs` - Meeting scheduling and conflict resolution
  - `ResponsibilityAssignments/ResponsibilityAssignmentService.cs` - Assignment management
  - `PdfExport/PdfExportService.cs` - Professional PDF generation with QuestPDF
  - `Roles/RoleService.cs` - Role and permission management

### Specialized Features
- **PDF Export System:** `/Features/PdfExport/`
  - `PdfExportService.cs` - Core PDF generation logic
  - `Models/MonthlySchedulePdfModel.cs` - PDF data models
  - `Endpoints/MeetingSchedulesEndpoints.cs` - API endpoints for PDF download

- **Email System:** `/Services/`
  - `ResendEmailSender.cs` - Email delivery with Resend integration
  - `ResendOptions.cs` - Email configuration and templates

### Data Layer
- **Database Context:** `Data/ApplicationDbContext.cs` - EF Core context with advanced configurations
- **Entities:** `/Entities/` - Domain models with soft delete support
- **Migrations:** `/Data/Migrations/` - Database schema evolution
- **Data Seeding:** 
  - `DataSeeder.cs` - Production data initialization
  - `DataSeederDevelopment.cs` - Development sample data

### Configuration & Infrastructure
- **Application:** `Program.cs` - Application bootstrap with service registration
- **Docker:** `Dockerfile` - Multi-stage containerization
- **Deployment:** `compose.yaml` - Multi-container orchestration
- **Shared:** `/Shared/` - Common enums, constants, and utilities

---

## 9. Technical Architecture Patterns

### Service Layer Architecture
- **Feature-Based Organization:** Services grouped by business domain rather than technical layers
- **Dependency Injection:** Comprehensive DI container configuration for loose coupling
- **Interface Segregation:** Clear separation between service contracts and implementations
- **Repository Pattern:** Entity Framework Core as the data access abstraction layer

### API Design Patterns
- **Minimal APIs:** Lightweight endpoints for high-performance operations (PDF export)
- **RESTful Controllers:** Traditional MVC pattern for complex business operations
- **Command Query Separation:** Clear distinction between read and write operations
- **Response Standardization:** Consistent error handling and response formatting

### Security Architecture
- **Identity Framework:** ASP.NET Core Identity with custom user management
- **Role-Based Access Control:** Hierarchical permission system (Admin > Manager > User)
- **Data Protection:** Encrypted keys stored in database for session and form protection
- **Authentication Flow:** Email confirmation and password reset with secure token generation

### Data Architecture
- **Entity Framework Core:** Code-first approach with fluent API configuration
- **Soft Delete Pattern:** Logical deletion with query filters for data protection
- **Migration Strategy:** Automated database evolution with rollback capabilities
- **Seeding Strategy:** Environment-specific data initialization for development and production

---

## 10. Extensibility Summary

- The system is ready to add new entities, relationships, and workflows.
- The service- and component-based architecture facilitates integration with AI agents for automation, analysis, and user