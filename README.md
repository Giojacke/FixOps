# Sistema de Mantenimiento

Backend y frontend para gestion de ordenes de trabajo de mantenimiento.
Solucion basada en .NET 9 con separacion por capas (Domain/Application/Infrastructure/API/Web).

## 1. Arquitectura

### Proyectos

- `Mantenimiento.Domain`: entidades, enums e interfaces de repositorio/UoW.
- `Mantenimiento.Application`: casos de uso, DTOs, validaciones y servicios de aplicacion.
- `Mantenimiento.Infrastructure`: EF Core, Identity, repositorios concretos, UnitOfWork y servicios externos.
- `Mantenimiento.API`: capa de exposicion HTTP (ASP.NET Core), autenticacion/autorizacion JWT.
- `Mantenimiento.Web`: cliente Blazor WebAssembly.

### Patrones aplicados

- Clean Architecture (dependencias hacia adentro).
- Repository + Unit of Work.
- DTO mapping entre capas.
- Validacion de entrada con FluentValidation.

## 2. Stack Tecnologico

- .NET SDK 9.x
- ASP.NET Core Web API
- Entity Framework Core (SQL Server)
- ASP.NET Core Identity
- JWT Bearer Authentication
- Blazor WebAssembly

## 3. Requisitos

- .NET SDK 9.x
- SQL Server (LocalDB para desarrollo local)
- PowerShell (scripts/comandos de ejemplo)

## 4. Configuracion por Entorno

### Archivos

- `Mantenimiento.API/appsettings.json`: base comun, sin secretos reales.
- `Mantenimiento.API/appsettings.Development.json`: desarrollo local.

### Variables criticas

- `ConnectionStrings:DefaultConnection`
- `Jwt:Key`
- `Jwt:Issuer`
- `Jwt:Audience`
- `EmailSettings:*`
- `SeedSettings:DefaultPassword` (solo desarrollo/demo)

### Recomendacion de produccion

- No almacenar secretos en repositorio.
- Usar Secret Manager (local), variables de entorno, o proveedor externo de secretos (Key Vault / Secrets Manager).

## 5. Seguridad

### Estado actual

- API protegida con JWT Bearer.
- Endpoints de ordenes con autorizacion por rol.
- Identity para usuarios/roles.

### Requerido para produccion

- Rotacion de `Jwt:Key` y vigencia corta de tokens.
- Refresh tokens con revocacion.
- Politica de contrasenas robusta y MFA para perfiles administrativos.
- CORS restringido a dominios reales.
- Rate limiting y proteccion de brute-force en login.
- Validacion centralizada de errores (ProblemDetails + trazabilidad).
- Auditoria de acciones sensibles (quien, cuando, que cambio).

## 6. Base de Datos y Migraciones

### Desarrollo

La API inicia seed de roles/usuarios/dependencias automaticamente.

### Produccion (recomendado)

- Ejecutar migraciones en pipeline o job controlado:

```powershell
dotnet ef database update --project .\Mantenimiento.Infrastructure\Mantenimiento.Infrastructure.csproj --startup-project .\Mantenimiento.API\Mantenimiento.API.csproj
```

- Desactivar seed de usuarios de prueba.
- Usar cuentas bootstrap temporales con expiracion.

## 7. Ejecucion Local

### API

```powershell
dotnet run --project .\Mantenimiento.API\Mantenimiento.API.csproj
```

URLs:

- `http://localhost:5284`
- `https://localhost:7259`

Swagger:

- `http://localhost:5284/swagger`
- `https://localhost:7259/swagger`

### Web

```powershell
dotnet run --project .\Mantenimiento.Web\Mantenimiento.Web.csproj
```

URLs:

- `http://localhost:5166`
- `https://localhost:7106`

## 8. Usuarios Seed (solo desarrollo/demo)

- `admin1@mantenimiento.com`
- `admin2@mantenimiento.com`
- `tecnico1@mantenimiento.com`
- `tecnico2@mantenimiento.com`
- `tecnico3@mantenimiento.com`

Password local por defecto:

- `Admin123!`

Ubicacion:

- `Mantenimiento.API/appsettings.Development.json` -> `SeedSettings:DefaultPassword`

## 9. Build, Calidad y Pruebas

### Build

```powershell
dotnet build .\SistemaMantenimiento.sln
```

Nota: si la API esta corriendo, puede bloquear DLL en `bin/Debug`.

### Recomendado para produccion

- Agregar proyectos de pruebas:
  - unitarias (`Application`, reglas de negocio)
  - integracion (`API + DB`)
  - contrato (OpenAPI)
- Activar analizadores y tratar warnings criticos como error.
- Escaneo de dependencias y vulnerabilidades en CI.

## 10. Observabilidad (faltante para prod)

Implementar:

- Logging estructurado (Serilog u OpenTelemetry Logs).
- Correlation ID por request.
- Metricas (latencia, throughput, errores por endpoint).
- Trazas distribuidas (OpenTelemetry).
- Health checks (`/health`, `/ready`) para orquestadores.

## 11. Despliegue

### Estrategia recomendada

- Contenerizar API y Web.
- Pipeline CI/CD con etapas:
  - restore
  - build
  - test
  - scan
  - publish artifact
  - deploy por entorno (dev/stg/prod)
- Configuracion 12-factor por entorno.

### Checklist minimo pre-produccion

- [ ] secretos fuera del repo
- [ ] HTTPS obligatorio + HSTS
- [ ] CORS restringido
- [ ] migraciones controladas
- [ ] seed de prueba desactivado
- [ ] backups y plan de restore DB
- [ ] politicas de retencion de logs
- [ ] monitoreo y alertas configuradas
- [ ] pruebas de carga basicas
- [ ] plan de rollback

## 12. Estado del Repositorio

Este repositorio nacio como proyecto dummy para arquitectura y pruebas funcionales.
Tiene base tecnica valida para evolucion, pero requiere los puntos de hardening listados antes de usar en produccion real.
