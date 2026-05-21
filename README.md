# FixOps — Sistema Inteligente de Gestión de Mantenimiento

**Repositorio:** https://github.com/Giojacke/FixOps

Sistema web para digitalizar y centralizar la gestión de órdenes de trabajo de mantenimiento. Permite a organizaciones con equipos técnicos registrar solicitudes, asignar técnicos, hacer seguimiento de operaciones, calificar el servicio y analizar métricas de desempeño en tiempo real.

---

## Tecnologías Utilizadas

| Capa | Tecnología |
|---|---|
| Backend API | .NET 9 · ASP.NET Core Web API |
| ORM / Base de datos | Entity Framework Core 9 · SQL Server (LocalDB dev) |
| Autenticación | ASP.NET Core Identity · JWT Bearer |
| Frontend | Blazor WebAssembly (.NET 9) |
| Validaciones | FluentValidation |
| Documentación API | Swagger / OpenAPI |
| Gráficos | Chart.js 4.4.3 |
| Correo | SMTP (MailKit) con cola de reintentos |
| Importación Excel | ClosedXML |

---

## Épicas y MVP Implementadas

### Épica 1 — Administración y Seguridad del Sistema
- **HU-17** Crear usuarios (Técnico, Programador, Solicitante, Administrador) con validación de email único
- **HU-18** Asociar jefe a dependencia con email para notificaciones automáticas
- **HU-22** Autorización por rol: cada usuario ve y puede hacer solo lo que le corresponde

### Épica 2 — Gestión Operativa de Mantenimiento
- **HU-6**  Crear órdenes de trabajo con folio único, dependencia, urgencia y técnico asignado
- **HU-7**  Visualizar órdenes filtradas por estado (Pendiente → En Proceso → Finalizada | Cancelada)
- **HU-9**  Agregar múltiples operaciones a una orden; bloqueado en órdenes finalizadas
- **HU-10** Actualizar estado de operación con restricción al técnico asignado

### Épica 3 — Encuestas y Automatización
- **HU-12** Envío automático de encuesta por correo al jefe de la dependencia al finalizar una orden
- **HU-13** Responder encuesta con calificación 1–5 en Atención, Servicio y Tiempo + comentario opcional
- **HU-15** Dashboard de métricas de satisfacción por técnico y por dependencia con filtro de fechas
- **HU-20** Gestión completa de materiales (CRUD + stock + bloqueo de eliminación si está en uso)

**Resultado: 11/11 Historias de Usuario implementadas y validadas**

---

## Patrones de Diseño GoF Implementados

### 1. Repository (Patrón de Acceso a Datos)
Abstrae el acceso a la base de datos detrás de interfaces. Las capas superiores nunca conocen EF Core.
```
Domain/Interfaces/IRepository.cs           ← interfaz genérica
Infrastructure/Persistence/EfRepository.cs ← implementación con EF Core
Infrastructure/Persistence/OrdenTrabajoRepository.cs ← repositorio especializado
```

### 2. Unit of Work
Agrupa múltiples operaciones de repositorio en una sola transacción atómica.
```
Domain/Interfaces/IUnitOfWork.cs
Infrastructure/Persistence/UnitOfWork.cs
```
```csharp
await unitOfWork.Operaciones.AddAsync(operacion);
await unitOfWork.SaveChangesAsync(); // una sola transacción
```

### 3. Strategy (FluentValidation)
Cada validador encapsula una estrategia de validación intercambiable, registrada en el contenedor de DI.
```
Application/Validators/CrearOrdenRequestValidator.cs
Application/Validators/EncuestaValidator.cs
Application/Validators/MasterValidators.cs
```
```csharp
// El controlador no sabe qué reglas aplica, solo invoca la estrategia
var result = await validator.ValidateAsync(request);
```

### 4. Facade (Servicios de Aplicación)
Los servicios de la capa Application actúan como fachada: simplifican operaciones complejas que involucran múltiples repositorios, validaciones y efectos secundarios (email, auditoría).
```
Application/Services/OrdenTrabajoService.cs  ← fachada de órdenes
Application/Services/EncuestaService.cs      ← fachada de encuestas
Application/Services/MasterServices.cs       ← fachada de maestros
```

### 5. Observer (Cola de Correos con Worker)
El `EmailQueueWorker` observa la base de datos en busca de correos pendientes y los procesa de forma asíncrona, sin que el flujo principal tenga que esperar.
```
Infrastructure/BackgroundServices/EmailQueueWorker.cs  ← observer/worker
Infrastructure/Services/QueuedEmailService.cs          ← encola eventos
API/Controllers/CorreoQueueController.cs               ← administración de la cola
```

### 6. Factory Method (Result<T>)
El patrón Result encapsula éxito o fallo usando métodos de fábrica estáticos, evitando excepciones para flujos esperados.
```
Application/Common/Result.cs
```
```csharp
return Result<Guid>.Success(encuesta.Id);   // factory de éxito
return Result<Guid>.Failure("Mensaje.");     // factory de fallo
```

### 7. Template Method (EfRepository<T>)
El repositorio genérico define el esqueleto de las operaciones CRUD. Los repositorios especializados sobrescriben solo lo necesario (ej: eager loading).
```
Infrastructure/Persistence/Repositories/EfRepository.cs         ← template
Infrastructure/Persistence/Repositories/OrdenTrabajoRepository.cs ← override con Include()
```

### 8. Chain of Responsibility (Pipeline de Middleware)
ASP.NET Core procesa cada request a través de una cadena configurable: autenticación → autorización → validación → controlador → respuesta.
```
API/Program.cs  ← configuración de la cadena
```
```csharp
app.UseAuthentication();   // eslabón 1
app.UseAuthorization();    // eslabón 2
app.MapControllers();      // eslabón final
```

---

## Pasos para Ejecutar

### Requisitos previos
- .NET SDK 9.x
- SQL Server o SQL Server LocalDB

### 1. Clonar el repositorio
```powershell
git clone https://github.com/Giojacke/FixOps.git
cd FixOps
```

### 2. Aplicar migraciones y levantar la API
```powershell
dotnet ef database update --project .\Mantenimiento.Infrastructure\Mantenimiento.Infrastructure.csproj --startup-project .\Mantenimiento.API\Mantenimiento.API.csproj

dotnet run --project .\Mantenimiento.API\Mantenimiento.API.csproj
```
- API disponible en: `http://localhost:5284`
- Swagger/OpenAPI en: `http://localhost:5284/swagger`

### 3. Levantar el frontend
```powershell
dotnet run --project .\Mantenimiento.Web\Mantenimiento.Web.csproj
```
- Aplicación en: `http://localhost:5166`
- Landing page en: `http://localhost:5166/landing.html`

### 4. Usuarios de prueba (seed automático)

| Usuario | Contraseña | Rol |
|---|---|---|
| admin1@mantenimiento.com | Admin123! | Administrador |
| tecnico1@mantenimiento.com | Admin123! | Técnico |
| tecnico2@mantenimiento.com | Admin123! | Técnico |
| tecnico3@mantenimiento.com | Admin123! | Técnico |

---

## Arquitectura del Proyecto

```
SistemaMantenimiento/
├── Mantenimiento.Domain/          ← Entidades, enums, interfaces (sin dependencias)
├── Mantenimiento.Application/     ← Casos de uso, DTOs, validaciones, servicios
├── Mantenimiento.Infrastructure/  ← EF Core, Identity, repositorios, email, workers
├── Mantenimiento.API/             ← Controllers REST, JWT, Swagger
└── Mantenimiento.Web/             ← Blazor WASM, páginas, servicios HTTP client
```

Las dependencias fluyen **solo hacia adentro** (Clean Architecture):
`Web / API → Application → Domain ← Infrastructure`

---

## Evidencia de Funcionamiento

### Dashboard de Analítica
![Dashboard métricas](docs/screenshots/dashboard-metricas.png)

### Gestión de Órdenes de Trabajo
![Órdenes de trabajo](docs/screenshots/ordenes-trabajo.png)

### Módulo de Maestros
![Maestros](docs/screenshots/maestros.png)

### Swagger API
![Swagger](docs/screenshots/swagger-api.png)

### Landing Page
![Landing](docs/screenshots/landing-page.png)

> Las capturas se encuentran en la carpeta `/docs/screenshots/` del repositorio.
