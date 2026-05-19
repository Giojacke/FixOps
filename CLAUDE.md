# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Solution Overview

`SistemaMantenimiento` is a maintenance work-order management system built with Clean Architecture on .NET 9. It exposes a **JWT-secured REST API** (`Mantenimiento.API`) consumed by a **Blazor WebAssembly** frontend (`Mantenimiento.Web`).

## Build & Run Commands

```powershell
# Build entire solution
dotnet build .\SistemaMantenimiento.sln

# Run API (http://localhost:5284, swagger at /swagger)
dotnet run --project .\Mantenimiento.API\Mantenimiento.API.csproj

# Run Blazor frontend (http://localhost:5166)
dotnet run --project .\Mantenimiento.Web\Mantenimiento.Web.csproj

# Apply EF Core migrations
dotnet ef database update --project .\Mantenimiento.Infrastructure\Mantenimiento.Infrastructure.csproj --startup-project .\Mantenimiento.API\Mantenimiento.API.csproj

# Add a new migration
dotnet ef migrations add <MigrationName> --project .\Mantenimiento.Infrastructure\Mantenimiento.Infrastructure.csproj --startup-project .\Mantenimiento.API\Mantenimiento.API.csproj

# Publish
dotnet publish .\Mantenimiento.API\Mantenimiento.API.csproj -c Release -o .\artifacts\api
dotnet publish .\Mantenimiento.Web\Mantenimiento.Web.csproj -c Release -o .\artifacts\web
```

There are no test projects in this solution.

## Architecture

Five projects with strictly inward-only dependencies:

```
Mantenimiento.Domain          ← no dependencies
Mantenimiento.Application     ← depends on Domain
Mantenimiento.Infrastructure  ← depends on Domain + Application
Mantenimiento.API             ← depends on all three
Mantenimiento.Web             ← depends on Application (DTOs only)
```

**Key patterns:**
- **Repository + Unit of Work** — `IRepository<T>` / `IUnitOfWork` in Domain; `EfRepository<T>` / `UnitOfWork` in Infrastructure.
- **Result pattern** — services return result objects instead of throwing exceptions for expected failures.
- **DTO boundary** — Application-layer DTOs (`OrdenTrabajoDto`, `CrearOrdenRequest`, etc.) cross the API/Web boundary; domain entities never leave Infrastructure/Application.
- **FluentValidation** — validators live in Application and are registered via `AddValidatorsFromAssemblyContaining<CrearOrdenRequestValidator>()`.

## Domain Model

Core entities: `OrdenTrabajo`, `Operacion`, `EncuestaSatisfaccion`, `Dependencia`, `Material`, `Usuario`.

Key enums:
- `EstadoOrden`: Pendiente → EnProceso → Finalizada | Cancelada
- `NivelUrgencia`, `RolUsuario` (Administrador, Tecnico, Solicitante)

`Usuario` extends `IdentityUser<Guid>` and carries a `RolUsuario` enum and a `DependenciaId` FK.

## Database

- **EF Core 9** with SQL Server (LocalDB for dev).
- `MantenimientoDbContext` inherits from `IdentityDbContext<Usuario, IdentityRole<Guid>, Guid>`.
- Connection string: `Server=(localdb)\mssqllocaldb;Database=MantenimientoDB;Trusted_Connection=True;MultipleActiveResultSets=true`
- Migrations live in `Mantenimiento.Infrastructure/Migrations/`.

## Authentication

- **JWT Bearer** (7-day expiry). Token payload includes `NameIdentifier`, `Name`, `Email`, `RolEnum` claim, and ASP.NET Core role claims.
- JWT config keys: `Jwt:Key`, `Jwt:Issuer`, `Jwt:Audience` in `appsettings.json`.
- **Blazor side:** `CustomAuthenticationStateProvider` reads the token from `Blazored.LocalStorage` and parses claims directly from the JWT.
- API uses `[Authorize(Roles = "...")]` on controllers; role strings match `RolUsuario` enum names.

## Seed Data (auto-applied on API startup)

`IdentityDataSeeder` creates roles, five dependencies, and five users (two Administradores, three Tecnicos). Default password is in `SeedSettings:DefaultPassword` (`Admin123!` in dev). No Solicitante seed users — they self-register.

## API Conventions

- Base path: `api/[controller]`
- All controllers require authorization except `POST /api/auth/login`.
- CORS policy `"BlazorPolicy"` allows `http://localhost:5166` and the HTTPS equivalents — update for production.

## Language Convention

- Source code (identifiers, comments): **English**
- Contributor documentation and commit messages: **Spanish**

## Configuration Files

- `Mantenimiento.API/appsettings.json` — connection string, JWT, SMTP (`EmailSettings`), seed password.
- `Mantenimiento.Web/wwwroot/appsettings.json` — API base URL (`http://localhost:5284/`), update for production deployments.
