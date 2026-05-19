# Repository Guidelines

## Language Rule
Write all contributor-facing explanations, reviews, and guidance in Spanish. Keep code, class names, method names, file names, DTO names, and other technical identifiers exactly as they exist in English. When reporting architecture issues or review findings, describe the problem and the recommended refactor in Spanish without translating code identifiers.

## Clean Architecture Rule
This solution must preserve inward-only dependencies: `Web/API -> Application -> Domain`. `Infrastructure` may depend on `Application` and `Domain` to implement contracts, but `Domain` and `Application` must never depend on `Infrastructure`, `API`, `Web`, EF Core, HTTP, or UI libraries.

## Layer Responsibilities
`Mantenimiento.Domain` contains entities, enums, value objects, and business invariants. Put core rules here when they are independent of transport, persistence, or framework concerns.

`Mantenimiento.Application` contains use cases, DTOs, validators, interfaces, and orchestration logic. It coordinates domain behavior, defines repository/service contracts, and contains application-specific policies.

`Mantenimiento.Infrastructure` contains EF Core, Identity, repository implementations, persistence mappings, and external services such as email. It is an implementation detail and must not own business rules.

`Mantenimiento.API` exposes HTTP endpoints, authentication setup, DI registration, and request/response mapping. Controllers should stay thin and delegate to application services.

`Mantenimiento.Web` contains Blazor UI, view state, and API clients. Keep presentation logic here; do not re-implement domain or application rules in Razor components.

## Architectural Constraints
Do not place SQL, EF queries, `DbContext`, `HttpClient`, JWT handling, or configuration access in `Domain` or `Application`.

Do not put business decisions in controllers, Razor pages, or repository classes. If a rule affects workflow, validation, permissions, or entity state, it belongs in `Domain` or `Application`.

Repositories persist and retrieve data only. They must not contain orchestration, input validation, or UI-specific shaping.

DTOs belong in `Application`; entities must not leak directly through API contracts unless explicitly justified.

## Build and Verification
Use:

```powershell
dotnet build .\SistemaMantenimiento.sln
dotnet run --project .\Mantenimiento.API\Mantenimiento.API.csproj
dotnet run --project .\Mantenimiento.Web\Mantenimiento.Web.csproj
```

Before merging, verify any new code respects the dependency direction and does not introduce cross-layer shortcuts.

## Contributor Rules
Name services, handlers, and DTOs by use case, for example `CrearOrdenRequest` or `OrdenTrabajoService`. Keep one public type per file and follow existing C# conventions: 4-space indentation, file-scoped namespaces, `PascalCase` for types/members, `camelCase` for locals.

If you are unsure where logic belongs, move it inward to the most stable layer that can own it without framework knowledge.
