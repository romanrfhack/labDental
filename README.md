# Laboratorio Dental Tlahuac

Plataforma web para Laboratorio Dental Tlahuac. El sistema reemplazara progresivamente la operacion basada en Excel con un sitio publico institucional y una app administrativa privada bajo `/app`.

## Stack

- Backend: .NET 10, ASP.NET Core Web API.
- Frontend: Angular 21 con routing y SCSS.
- Persistencia objetivo: SQL Server con Entity Framework Core.
- Arquitectura: limpia/modular por `Api`, `Application`, `Domain` e `Infrastructure`.
- Autenticacion MVP: cookie segura HttpOnly.
- Autorizacion: permisos granulares por rol.

## Estructura

```text
docs/
src/
  LaboratorioTlahuac.Api/
  LaboratorioTlahuac.Application/
  LaboratorioTlahuac.Domain/
  LaboratorioTlahuac.Infrastructure/
  LaboratorioTlahuac.Web/
tests/
  LaboratorioTlahuac.Api.Tests/
  LaboratorioTlahuac.Application.Tests/
  LaboratorioTlahuac.Domain.Tests/
```

## Backend

Restaurar, compilar y probar:

```bash
dotnet restore
dotnet build
dotnet test
```

Ejecutar API en Development:

```bash
dotnet run --project src/LaboratorioTlahuac.Api/LaboratorioTlahuac.Api.csproj
```

Health check:

```bash
curl http://localhost:5277/health
```

### Base De Datos Y Migraciones

La persistencia usa EF Core con SQL Server. La migracion inicial de seguridad es `InitialSecurityModel`.

Crear nuevas migraciones:

```bash
dotnet ef migrations add NombreMigracion \
  --project src/LaboratorioTlahuac.Infrastructure/LaboratorioTlahuac.Infrastructure.csproj \
  --startup-project src/LaboratorioTlahuac.Api/LaboratorioTlahuac.Api.csproj \
  --output-dir Persistence/Migrations
```

Aplicar migraciones a una base local configurada:

```bash
dotnet ef database update \
  --project src/LaboratorioTlahuac.Infrastructure/LaboratorioTlahuac.Infrastructure.csproj \
  --startup-project src/LaboratorioTlahuac.Api/LaboratorioTlahuac.Api.csproj
```

No hay auto-migracion en startup. No ejecutar `database update` contra produccion sin plan de despliegue y respaldo.

### Seed Admin

El seed de seguridad es idempotente y se ejecuta solo si `SecuritySeed:RunOnStartup` esta en `true`. Crea:

- Rol `Admin`.
- Todos los permisos definidos en `Permissions`.
- Asignacion de todos los permisos al rol `Admin`.
- Usuario Admin inicial, solo si hay configuracion valida.

Aplicar primero las migraciones sobre la base local. El seed no crea esquema ni ejecuta migraciones.

Variables requeridas para crear el usuario Admin:

```bash
LT_ADMIN_EMAIL=admin@example.com
LT_ADMIN_PASSWORD=<password-local-seguro>
LT_ADMIN_FULL_NAME="Administrador"
SecuritySeed__RunOnStartup=true
```

Para desarrollo tambien se pueden usar user-secrets:

```bash
dotnet user-secrets set LT_ADMIN_EMAIL admin@example.com --project src/LaboratorioTlahuac.Api/LaboratorioTlahuac.Api.csproj
dotnet user-secrets set LT_ADMIN_PASSWORD "<password-local-seguro>" --project src/LaboratorioTlahuac.Api/LaboratorioTlahuac.Api.csproj
dotnet user-secrets set LT_ADMIN_FULL_NAME "Administrador" --project src/LaboratorioTlahuac.Api/LaboratorioTlahuac.Api.csproj
dotnet user-secrets set SecuritySeed:RunOnStartup true --project src/LaboratorioTlahuac.Api/LaboratorioTlahuac.Api.csproj
```

No hay contrasena por defecto en el repositorio.

### Endpoints Auth

- `GET /api/auth/csrf`
- `POST /api/auth/login`
- `POST /api/auth/logout`
- `GET /api/auth/me`
- `GET /api/security/permissions-check` solo en Development, protegido por `users.manage`.
- `POST /api/security/csrf-check` solo en Development, protegido por `users.manage`, para validar CSRF.

La cookie de autenticacion es HttpOnly, `SameSite=Lax`, con `Secure` obligatorio en Production. Los endpoints `/api` devuelven `401` o `403`; no redirigen con `302` a HTML.

### Flujo XSRF

El sistema usa cookie auth HttpOnly para la sesión y antiforgery para requests mutables.

- Cookie de sesión: HttpOnly, no legible por JavaScript.
- Cookie `XSRF-TOKEN`: legible por JavaScript por diseño; no contiene la sesión.
- Header requerido en requests mutables bajo `/api`: `X-XSRF-TOKEN`.
- `GET`, `HEAD`, `OPTIONS` y `TRACE` no requieren token.
- `GET /health`, `GET /api/auth/me` y `GET /api/auth/csrf` no se bloquean por CSRF.
- `POST /api/auth/login`, `POST /api/auth/logout` y futuros `POST/PUT/PATCH/DELETE` bajo `/api` requieren token válido.

Probar flujo auth + CSRF con curl:

```bash
curl -i -c /tmp/ldt-cookies.txt http://localhost:5277/api/auth/csrf
XSRF_TOKEN=$(grep XSRF-TOKEN /tmp/ldt-cookies.txt | awk '{print $7}')
curl -i -b /tmp/ldt-cookies.txt -c /tmp/ldt-cookies.txt \
  -H "Content-Type: application/json" \
  -H "X-XSRF-TOKEN: $XSRF_TOKEN" \
  -d '{"email":"admin@example.com","password":"<password-local-seguro>"}' \
  http://localhost:5277/api/auth/login
curl -i -b /tmp/ldt-cookies.txt http://localhost:5277/api/auth/me
curl -i -b /tmp/ldt-cookies.txt -c /tmp/ldt-cookies.txt http://localhost:5277/api/auth/csrf
XSRF_TOKEN=$(grep XSRF-TOKEN /tmp/ldt-cookies.txt | awk '{print $7}')
curl -i -X POST -b /tmp/ldt-cookies.txt \
  -H "X-XSRF-TOKEN: $XSRF_TOKEN" \
  http://localhost:5277/api/auth/logout
```

## Frontend

```bash
cd src/LaboratorioTlahuac.Web
npm install
npm run start
npm run build
```

La URL de API para `ng serve` esta en `src/environments/environment.development.ts`. En produccion el default es mismo origen (`apiBaseUrl: ''`). Frontend tests no estan configurados como script ejecutable; se valida `npm run build`.

Angular configura `withXsrfConfiguration` con `XSRF-TOKEN` y `X-XSRF-TOKEN`. `AuthService` pide `/api/auth/csrf` antes de login/logout y pone el header explícitamente para que funcione también en desarrollo cross-origin.

## npm audit

Se revisó `npm audit`. El reporte inicial encontró vulnerabilidades transitivas en `fast-uri`, `ip-address` y `express-rate-limit`. Se aplicó `npm audit fix` sin `--force`, actualizando dependencias transitivas compatibles, y el resultado final quedó en 0 vulnerabilidades.

## Estado Actual

Fase 1 - MVP operativo. Etapa 2.1 - Hardening de seguridad implementada.

Incluye solucion .NET, proyectos base, app Angular, rutas publicas/privadas, cookie auth HttpOnly, XSRF para requests mutables, modelo inicial de usuarios/roles/permisos, seed Admin, endpoints auth, guards reales, migracion inicial de seguridad, health check y documentacion actualizada.

No incluye CRUD de clientes, ordenes de trabajo, pagos, inventario, proveedores ni dashboard operativo real.

## Proximos Pasos

1. Revisar hardening de seguridad.
2. Implementar CRUD de clientes/doctores/clinicas.
3. Implementar ordenes de trabajo.
4. Implementar pagos y saldos.
5. Implementar dashboard operativo basico.
