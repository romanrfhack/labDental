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

La persistencia usa EF Core con SQL Server. Migraciones creadas:

- `InitialSecurityModel`
- `AddCustomersAndInternalDoctors`
- `AddWorkOrders`

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

Listar migraciones:

```bash
dotnet ef migrations list \
  --project src/LaboratorioTlahuac.Infrastructure/LaboratorioTlahuac.Infrastructure.csproj \
  --startup-project src/LaboratorioTlahuac.Api/LaboratorioTlahuac.Api.csproj
```

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

### Endpoints Clientes

Modulo implementado bajo `/api/customers`, protegido por permisos:

- `GET /api/customers` requiere `customers.view`.
- `GET /api/customers/{id}` requiere `customers.view`.
- `POST /api/customers` requiere `customers.create` y XSRF.
- `PUT /api/customers/{id}` requiere `customers.edit` y XSRF.
- `PATCH /api/customers/{id}/status` requiere `customers.edit` y XSRF.
- `GET /api/customers/{customerId}/internal-doctors` requiere `customers.view`.
- `POST /api/customers/{customerId}/internal-doctors` requiere `customers.create` y XSRF.
- `PUT /api/customers/{customerId}/internal-doctors/{doctorId}` requiere `customers.edit` y XSRF.
- `PATCH /api/customers/{customerId}/internal-doctors/{doctorId}/status` requiere `customers.edit` y XSRF.

`GET /api/customers` acepta `search`, `type`, `isActive`, `page` y `pageSize`. Si `isActive` no se envia, devuelve solo clientes activos. `PATCH /status` devuelve `200 OK` con el cliente actualizado. Cambiar una clinica con doctores internos activos a `Doctor` u `Other` devuelve `409 Conflict`. Intentar administrar doctores internos en un cliente que no es `Clinic` devuelve `400 Bad Request`.

### Endpoints Ordenes De Trabajo

Modulo implementado bajo `/api/work-orders`, protegido por permisos:

- `GET /api/work-orders` requiere `orders.view`.
- `GET /api/work-orders/{id}` requiere `orders.view`.
- `GET /api/work-orders/statuses` requiere `orders.view`.
- `POST /api/work-orders` requiere `orders.create` y XSRF.
- `PUT /api/work-orders/{id}` requiere `orders.edit` y XSRF.
- `PATCH /api/work-orders/{id}/status` requiere `orders.changeStatus` y XSRF.

`GET /api/work-orders` acepta `search`, `customerId`, `internalDoctorId`, `status`, rangos de `receivedDate` y `deliveryDate`, `includeCancelled`, `page` y `pageSize`. Si `includeCancelled` no se envia, excluye ordenes `Cancelled`.

El backend genera `OrderNumber` con formato MVP `OT-yyyyMMdd-XXXXXX` y un indice unico en base de datos. El formato puede cambiar antes de produccion si el cliente requiere folio secuencial.

Convenciones implementadas:

- `Status` inicial es `Received`.
- Todo cambio real de estado crea historial.
- Cambiar al mismo estado devuelve `200 OK` sin duplicar historial.
- Cambiar a `Cancelled` requiere nota.
- Una orden `Cancelled` no se edita ni vuelve a otro estado en el MVP.
- No existe delete fisico de ordenes.
- `TotalAmount` es opcional; pagos, abonos y saldos no estan implementados todavia.

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

### Probar Clientes Localmente

1. Configurar una base SQL Server local en `ConnectionStrings:DefaultConnection`.
2. Aplicar migraciones con `dotnet ef database update`.
3. Configurar y ejecutar seed Admin.
4. Ejecutar API con `dotnet run --project src/LaboratorioTlahuac.Api/LaboratorioTlahuac.Api.csproj`.
5. Ejecutar Angular con `cd src/LaboratorioTlahuac.Web && npm run start`.
6. Iniciar sesion como Admin y entrar a `/app/clientes`.
7. Crear un cliente tipo `Doctor`.
8. Crear un cliente tipo `Clinic`.
9. Entrar al detalle de la clinica y agregar un doctor interno.
10. Validar que un cliente tipo `Doctor` no acepta doctores internos.
11. Editar y desactivar clientes; por default el listado muestra solo activos.

### Probar Ordenes Localmente

1. Configurar SQL Server local en `ConnectionStrings:DefaultConnection`.
2. Aplicar migraciones con `dotnet ef database update`.
3. Ejecutar API y Angular.
4. Iniciar sesion como Admin.
5. Crear cliente tipo `Doctor`.
6. Crear cliente tipo `Clinic`.
7. Agregar doctor interno activo a la clinica.
8. Entrar a `/app/ordenes`.
9. Crear orden para cliente tipo `Doctor`.
10. Crear orden para `Clinic` seleccionando doctor interno.
11. Editar datos generales de una orden no cancelada.
12. Cambiar estado de `Received` a `InProcess`.
13. Cambiar estado a `Cancelled` con nota.
14. Confirmar que una orden `Cancelled` no se edita.
15. Confirmar que canceladas no aparecen por default y si aparecen con "Incluir canceladas".
16. Confirmar que mutables sin XSRF devuelven `400`.
17. Confirmar que un usuario sin `orders.view` no consulta ordenes.

## npm audit

Se revisó `npm audit`. En la validación de Etapa 4 apareció una vulnerabilidad moderada transitiva en `hono`. Se aplicó `npm audit fix` sin `--force`, actualizando dependencias transitivas compatibles, y el resultado final quedó en 0 vulnerabilidades.

## Estado Actual

Fase 1 - MVP operativo. Etapa 4 - Ordenes de trabajo dental implementada.

Incluye solucion .NET, proyectos base, app Angular, rutas publicas/privadas, cookie auth HttpOnly, XSRF para requests mutables, modelo inicial de usuarios/roles/permisos, seed Admin, endpoints auth, guards reales, migraciones de seguridad, clientes y ordenes, CRUD de clientes/doctores/clinicas, ordenes de trabajo con estados e historial, health check y documentacion actualizada.

No incluye pagos, abonos, saldos calculados, inventario, proveedores, importacion del Excel ni dashboard operativo real.

## Proximos Pasos

1. Revisar ordenes de trabajo.
2. Implementar pagos, abonos y saldos.
3. Implementar dashboard operativo basico.
4. Preparar migracion del Excel.
5. Revisar UX con cliente.
