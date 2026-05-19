# DEPLOYMENT.md

Guia tecnica de despliegue para `Sistema de Mantenimiento`.

## 1. Objetivo

Definir un flujo repetible de CI/CD para `Mantenimiento.API` y `Mantenimiento.Web` con promotion por ambientes `dev -> stg -> prod`.

## 2. Estrategia Recomendada

- Build una sola vez por commit/tag.
- Publicar artefactos versionados.
- Desplegar el mismo artefacto entre ambientes (sin recompilar).
- Configuracion por ambiente via variables/secretos.

## 3. Ambientes

- `dev`: validacion funcional continua.
- `stg`: validacion pre-productiva (paridad cercana a prod).
- `prod`: trafico real.

Cada ambiente debe tener:

- Base de datos independiente.
- Secretos independientes.
- URL/API endpoint independiente.

## 4. Variables y Secretos

### API

Variables:

- `ASPNETCORE_ENVIRONMENT`
- `ConnectionStrings__DefaultConnection`
- `Jwt__Issuer`
- `Jwt__Audience`
- `AllowedHosts`

Secretos:

- `Jwt__Key`
- `EmailSettings__Host`
- `EmailSettings__Port`
- `EmailSettings__User`
- `EmailSettings__Password`

### Web

Variables:

- `ApiBaseUrl` (si decides externalizar `BaseAddress` por config)

## 5. Pipeline CI (ejemplo GitHub Actions)

Crear `.github/workflows/ci.yml`:

```yaml
name: CI

on:
  pull_request:
    branches: [ main ]
  push:
    branches: [ main ]

jobs:
  build-test:
    runs-on: ubuntu-latest

    steps:
      - name: Checkout
        uses: actions/checkout@v4

      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '9.0.x'

      - name: Restore
        run: dotnet restore SistemaMantenimiento.sln

      - name: Build
        run: dotnet build SistemaMantenimiento.sln --no-restore -c Release

      - name: Test
        run: dotnet test SistemaMantenimiento.sln --no-build -c Release
```

## 6. Pipeline CD (conceptual)

### Etapas

1. `package`
2. `deploy-dev`
3. `deploy-stg` (aprobacion manual)
4. `deploy-prod` (aprobacion manual)

### Acciones por etapa

- Package:
  - `dotnet publish` API/Web
  - generar artefactos versionados (`buildId`, `git sha`, `tag`)
- Deploy:
  - descargar artefacto
  - inyectar config por ambiente
  - aplicar migraciones
  - health check post-deploy

## 7. Publicacion de Artefactos

Ejemplo API:

```powershell
dotnet publish .\Mantenimiento.API\Mantenimiento.API.csproj -c Release -o .\artifacts\api
```

Ejemplo Web:

```powershell
dotnet publish .\Mantenimiento.Web\Mantenimiento.Web.csproj -c Release -o .\artifacts\web
```

## 8. Migraciones en Despliegue

Recomendado en `stg/prod`:

```powershell
dotnet ef database update --project .\Mantenimiento.Infrastructure\Mantenimiento.Infrastructure.csproj --startup-project .\Mantenimiento.API\Mantenimiento.API.csproj
```

Buenas practicas:

- respaldar DB antes de cambios de esquema.
- ejecutar migraciones en ventana controlada.
- nunca usar seed de usuarios de prueba en prod.

## 9. Rollback

### Rollback de aplicacion

- redeploy del ultimo artefacto estable.
- invalidar sesiones/tokens si aplica.

### Rollback de base de datos

- restaurar backup si migration no es backward-compatible.
- preferir migraciones forward-only con scripts de contingencia.

## 10. Verificacion Post-Deploy

Checklist:

- [ ] endpoint de salud responde OK
- [ ] autenticacion JWT funcional
- [ ] endpoints criticos responden en SLA esperado
- [ ] logs sin errores criticos sostenidos
- [ ] metricas base (error rate, latencia) normales

## 11. Hardening Minimo Antes de Prod

- [ ] quitar contrasena de seed demo (`Admin123!`)
- [ ] mover todos los secretos a vault/secret store
- [ ] activar rate limiting en login
- [ ] aplicar politicas de password y lockout
- [ ] habilitar auditoria y trazabilidad
- [ ] agregar pruebas de integracion automatizadas

## 12. Nota sobre este repositorio

El proyecto esta en fase demo/dummy. Esta guia define el camino para llevarlo a despliegues productivos controlados, pero requiere completar el hardening indicado.
