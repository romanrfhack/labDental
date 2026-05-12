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
- `AddPayments`

El dashboard operativo básico no cambia el esquema; no requiere migración nueva.

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
- `TotalAmount` es opcional; saldos y pagos detallados se consultan en endpoints protegidos por `payments.view`.

### Endpoints Pagos

Modulo implementado para pagos, abonos y saldos calculados. Los datos financieros detallados usan permisos `payments.*`, no `orders.view`.

- `GET /api/work-orders/{workOrderId}/payments` requiere `payments.view`.
- `GET /api/work-orders/{workOrderId}/payments/summary` requiere `payments.view`.
- `POST /api/work-orders/{workOrderId}/payments` requiere `payments.create` y XSRF.
- `PATCH /api/work-orders/{workOrderId}/payments/{paymentId}/cancel` requiere `payments.cancel` y XSRF.
- `GET /api/payments` requiere `payments.view`.
- `GET /api/payments/methods` requiere `payments.view`.
- `GET /api/payments/statuses` requiere `payments.view`.

`GET /api/payments` acepta `search`, `customerId`, `workOrderId`, `method`, `paymentDateFrom`, `paymentDateTo`, `includeCancelled`, `page` y `pageSize`. Si `includeCancelled` no se envia, excluye pagos cancelados.

Convencion de respuesta mutable:

- `POST /api/work-orders/{workOrderId}/payments` devuelve `201 Created` con `{ payment, summary }`.
- `PATCH /cancel` devuelve `200 OK` con `{ payment, summary }`.

Reglas principales:

- No hay delete fisico ni edicion libre de pagos en el MVP.
- `Amount` debe ser mayor a 0.
- `PaymentDate` y `Method` son obligatorios.
- No se registran pagos si `WorkOrder.TotalAmount` es `null`.
- No se registran pagos en ordenes `Cancelled`.
- Los pagos cancelados no cuentan para `PaidAmount` ni `Balance`.
- El sobrepago se permite y deja `PaymentStatus = Overpaid` con etiqueta "Saldo a favor / revisar".
- `Balance` es `null` si `TotalAmount` no esta definido.

### Endpoint Dashboard

Dashboard operativo básico implementado bajo:

- `GET /api/dashboard/summary` requiere `reports.view`.

La respuesta incluye `generatedAtUtc` y secciones condicionadas por permisos:

- `customerSummary` solo con `customers.view`: clientes activos, doctores activos, clínicas activas y clientes inactivos.
- `operationalSummary` solo con `orders.view`: órdenes activas, entregadas, canceladas, vencidas, para hoy, próximos 7 días, conteo por estado, últimas 5 órdenes y próximas 5 entregas.
- `financialSummary` solo con `payments.view`: total por cobrar, órdenes con saldo, conteos por estado financiero, pagos cancelados y últimos 5 pagos vigentes.

Si falta permiso para una sección, esa sección se devuelve como `null`. El endpoint es `GET`, no modifica estado y no requiere XSRF. La convención MVP para "hoy" usa `DateOnly.FromDateTime(IClock.UtcNow.UtcDateTime)`; la zona horaria formal de negocio queda pendiente.

Reglas financieras del dashboard:

- `PaidAmount` usa pagos no cancelados.
- `Balance = TotalAmount - PaidAmount`.
- Solo se consideran órdenes con `TotalAmount != null`.
- `totalReceivable` suma solo balances positivos y excluye órdenes `Cancelled`.
- Los conteos financieros de órdenes con saldo y estados de pago excluyen órdenes `Cancelled`.
- Los sobrepagos no reducen `totalReceivable`.

No hay reportes avanzados, cortes de caja, exportación Excel/PDF, facturación, CFDI, inventario, proveedores ni migración del Excel en esta etapa.

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

### Probar Pagos Localmente

1. Configurar SQL Server local en `ConnectionStrings:DefaultConnection`.
2. Aplicar migraciones con `dotnet ef database update`.
3. Ejecutar API y Angular.
4. Iniciar sesion como Admin.
5. Crear cliente.
6. Crear orden con `TotalAmount`.
7. Entrar al detalle de la orden.
8. Registrar pago parcial.
9. Verificar `PaidAmount`, `Balance` y `PaymentStatus = Partial`.
10. Registrar segundo pago hasta cubrir total.
11. Verificar `PaymentStatus = Paid`.
12. Registrar sobrepago y verificar `Overpaid`.
13. Cancelar un pago con motivo.
14. Verificar que el saldo se recalcula.
15. Crear orden sin `TotalAmount` e intentar pago; debe fallar.
16. Cancelar orden e intentar pago; debe fallar.
17. Entrar a `/app/pagos` y verificar listado.
18. Confirmar que pagos cancelados no aparecen por default.
19. Confirmar que mutables sin XSRF fallan.
20. Confirmar que usuario sin `payments.view` no puede consultar.

### Probar Dashboard Localmente

1. Configurar SQL Server local en `ConnectionStrings:DefaultConnection`.
2. Aplicar migraciones existentes con `dotnet ef database update`.
3. Ejecutar API y Angular.
4. Iniciar sesión como Admin.
5. Crear clientes, órdenes y pagos.
6. Entrar a `/app/dashboard`.
7. Verificar métricas de clientes, operación y cobranza.
8. Verificar últimas órdenes, próximas entregas y últimos pagos.
9. Cancelar un pago y confirmar que no cuenta para saldo.
10. Cancelar una orden y confirmar que no cuenta como activa ni vencida.
11. Probar usuarios sin `payments.view`, `orders.view` o `customers.view` y confirmar que la sección correspondiente no aparece.
12. Confirmar que `/health` sigue público.

### QA Manual Del MVP

La documentación de QA de la Etapa 7 está en `docs/08-qa/`:

- `mvp-qa-checklist.md`: checklist funcional ejecutado.
- `demo-script.md`: guion de demo para cliente.
- `demo-data-guide.md`: datos manuales sugeridos para demo.
- `known-issues.md`: hallazgos priorizados.
- `mvp-acceptance-checklist.md`: criterios de aceptación del MVP administrativo.

Para repetir la QA manual:

1. Configurar SQL Server local de QA, no producción.
2. Aplicar migraciones con `dotnet ef database update`.
3. Configurar Admin seed solo para ambiente local.
4. Ejecutar API y Angular.
5. Seguir `docs/08-qa/mvp-qa-checklist.md`.
6. Usar `docs/08-qa/demo-data-guide.md` para capturar datos de prueba.
7. Presentar con `docs/08-qa/demo-script.md`.

## npm audit

Se revisó `npm audit` en la validación de Etapa 7 y el resultado fue 0 vulnerabilidades.

## Estado Actual

Fase 1 - MVP operativo. Etapa 7 - QA funcional y demo preparada.

Incluye solucion .NET, proyectos base, app Angular, rutas publicas/privadas, cookie auth HttpOnly, XSRF para requests mutables, modelo inicial de usuarios/roles/permisos, seed Admin, endpoints auth, guards reales, migraciones de seguridad, clientes, ordenes, pagos, dashboard operativo basico, CRUD de clientes/doctores/clinicas, ordenes de trabajo con estados e historial, pagos cancelables con motivo, saldos calculados, health check, QA funcional documentada y guion de demo.

No incluye inventario, proveedores, facturacion, CFDI, cortes de caja avanzados, reportes avanzados, exportacion Excel/PDF ni importacion del Excel.

## Proximos Pasos

1. Ejecutar demo con cliente.
2. Capturar feedback.
3. Cerrar alcance comercial.
4. Definir prioridad entre sitio web y repartidores/etiquetas.
5. Planear siguiente fase contratada.
