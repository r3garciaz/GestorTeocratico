# Gestor Teocratico

[![.NET](https://img.shields.io/badge/.NET-9.0-blue.svg)](https://dotnet.microsoft.com/)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-17-blue.svg)](https://www.postgresql.org/)
[![Blazor](https://img.shields.io/badge/Blazor-Server-purple.svg)](https://blazor.net/)
[![Docker](https://img.shields.io/badge/Docker-Enabled-blue.svg)](https://www.docker.com/)

Una aplicación web moderna para la gestión integral de congregaciones religiosas, desarrollada con ASP.NET Core Blazor Server y PostgreSQL.

## 📋 Descripción

Gestor Teocratico es una aplicación web diseñada para facilitar la administración de congregaciones religiosas, permitiendo la gestión de:

- **Congregaciones** y su información general
- **Departamentos** dentro de la congregación
- **Publicadores** con datos personales y privilegios
- **Responsabilidades** y asignaciones
- **Horarios de reuniones** y programación
- **Usuarios** y roles con autenticación segura

## ✨ Características Principales

### 🏛️ Gestión de Congregaciones
- CRUD completo de congregaciones
- Información de contacto y horarios de reunión
- Gestión de departamentos asociados

### 👥 Administración de Publicadores
- Registro de publicadores con datos personales
- Gestión de privilegios y roles
- Asignación a departamentos y responsabilidades
- Exportación de datos personales (cumplimiento GDPR)

### 📋 Sistema de Responsabilidades
- Creación y gestión de responsabilidades por departamento
- Asignación de responsabilidades a publicadores
- Programación de reuniones con asignaciones automáticas

### 🔐 Seguridad y Autenticación
- Autenticación basada en ASP.NET Identity
- Sistema de roles y permisos
- Eliminación lógica (soft delete) para protección de datos

### 📄 Generación de Reportes
- Exportación a PDF con QuestPDF
- Reportes de asignaciones y programación
- Datos personales para cumplimiento normativo

## 🛠️ Tecnologías Utilizadas

### Backend
- **ASP.NET Core 9.0** - Framework web principal
- **Entity Framework Core** - ORM para acceso a datos
- **PostgreSQL** - Base de datos principal
- **ASP.NET Identity** - Autenticación y autorización
- **QuestPDF** - Generación de documentos PDF

### Frontend
- **Blazor Server** - Framework de UI interactivo
- **Radzen Blazor Components** - Biblioteca de componentes UI
- **CSS/HTML** - Estilos y maquetación

### Infraestructura
- **Docker & Docker Compose** - Contenedorización
- **Npgsql** - Driver de PostgreSQL para .NET

## 🏗️ Arquitectura

El proyecto sigue una arquitectura limpia con separación clara de responsabilidades:

```
GestorTeocratico/
├── Components/              # Componentes Blazor UI
│   ├── Layout/             # Layouts de la aplicación
│   ├── Pages/              # Páginas de la aplicación
│   └── Account/            # Componentes de autenticación
├── Controllers/            # Controladores API
├── Data/                   # Contexto de base de datos y migraciones
├── Entities/               # Modelos de dominio
├── Features/               # Servicios organizados por características
│   ├── Congregations/
│   ├── Departments/
│   ├── Publishers/
│   ├── Responsibilities/
│   └── ...
├── Models/                 # DTOs y ViewModels
├── Services/               # Servicios transversales
└── Shared/                 # Enums y utilidades compartidas
```

## 🚀 Instalación y Configuración

### Prerrequisitos

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download)
- [Docker](https://www.docker.com/) (opcional, para despliegue con contenedores)
- [PostgreSQL](https://www.postgresql.org/) (si no usas Docker)

### Instalación Local

1. **Clonar el repositorio:**
   ```bash
   git clone https://github.com/tu-usuario/GestorTeocratico.git
   cd GestorTeocratico
   ```

2. **Configurar la base de datos:**
   
   Crear un archivo `.env` en la raíz del proyecto:
   ```env
   POSTGRES_DB=gestorteocratico
   POSTGRES_USER=postgres
   POSTGRES_PASSWORD=tu_password
   POSTGRES_PORT=5432
   ```

3. **Restaurar dependencias:**
   ```bash
   dotnet restore
   ```

4. **Aplicar migraciones:**
   ```bash
   cd GestorTeocratico
   dotnet ef database update
   ```

5. **Ejecutar la aplicación:**
   ```bash
   dotnet run
   ```

   La aplicación estará disponible en `https://localhost:7095`

### Instalación con Docker

1. **Clonar el repositorio:**
   ```bash
   git clone https://github.com/tu-usuario/GestorTeocratico.git
   cd GestorTeocratico
   ```

2. **Configurar variables de entorno:**
   
   Crear un archivo `.env`:
   ```env
   POSTGRES_DB=gestorteocratico
   POSTGRES_USER=postgres
   POSTGRES_PASSWORD=tu_password
   POSTGRES_PORT=5432
   ```

3. **Ejecutar con Docker Compose:**
   ```bash
   docker compose up --build
   ```

   La aplicación estará disponible en `http://localhost:8080`

## 🗃️ Modelo de Datos

### Entidades Principales

- **Congregation**: Información general de la congregación
- **Department**: Departamentos dentro de la congregación
- **Publisher**: Publicadores con datos personales y privilegios
- **Responsibility**: Responsabilidades asignables
- **MeetingSchedule**: Programación de reuniones
- **ResponsibilityAssignment**: Asignaciones específicas por reunión

### Relaciones

- Una congregación tiene múltiples departamentos
- Un departamento tiene un publicador responsable y múltiples responsabilidades
- Los publicadores pueden tener múltiples responsabilidades (relación N:N)
- Las reuniones tienen múltiples asignaciones de responsabilidades

## 👤 Usuario por Defecto

En el entorno de desarrollo, se crea automáticamente un usuario administrador:

- **Email**: `admin@gestorteocratico.com`
- **Contraseña**: `Admin123!`

## 🔧 Configuración

### Base de Datos

La aplicación utiliza PostgreSQL como base de datos principal. La cadena de conexión se configura en `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=gestorteocratico;Username=postgres;Password=password"
  }
}
```

### Variables de Entorno

Para producción, configura las siguientes variables de entorno:

- `ConnectionStrings__DefaultConnection`: Cadena de conexión a PostgreSQL
- `ASPNETCORE_ENVIRONMENT`: Entorno de ejecución (Development/Production)

## 🤝 Contribución

1. Fork el proyecto
2. Crea una rama para tu feature (`git checkout -b feature/AmazingFeature`)
3. Commit tus cambios (`git commit -m 'Add some AmazingFeature'`)
4. Push a la rama (`git push origin feature/AmazingFeature`)
5. Abre un Pull Request

### Estándares de Código

- Seguir las convenciones de C# y .NET
- Utilizar Entity Framework Core para acceso a datos
- Implementar pruebas unitarias para nuevas funcionalidades
- Documentar APIs y métodos públicos

## 📜 Licencia

Este proyecto está bajo la Licencia MIT - ver el archivo [LICENSE](LICENSE) para más detalles.

## 📞 Soporte

Si tienes preguntas o necesitas ayuda:

- Abre un [issue](https://github.com/tu-usuario/GestorTeocratico/issues) en GitHub
- Revisa la documentación en el [PRD](GestorTeocratico/Shared/PRD.md)

## 📈 Estado del Proyecto

El proyecto se encuentra en desarrollo activo. Características principales implementadas:

- ✅ Gestión de congregaciones, departamentos y publicadores
- ✅ Sistema de responsabilidades y asignaciones
- ✅ Autenticación y autorización
- ✅ Generación de reportes PDF
- ✅ Eliminación lógica de datos
- ✅ Dockerización completa

### Próximas Características

- 🔄 Dashboard con métricas y estadísticas
- 🔄 Notificaciones automáticas
- 🔄 API REST completa
- 🔄 Aplicación móvil
- 🔄 Internacionalización (i18n)

---

Desarrollado con ❤️ para la gestión eficiente de congregaciones religiosas.
