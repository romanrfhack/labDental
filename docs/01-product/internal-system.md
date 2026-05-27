# Sistema Privado / MVP Administrativo

Fuente canónica funcional del sistema privado de Laboratorio Dental Tláhuac.

## Propósito

Permitir que el laboratorio opere registros nuevos sin depender del Excel para el flujo principal: clientes, órdenes de trabajo, pagos, saldos y dashboard.

## Ruta Base

- App privada: `/app`.
- Dashboard real: `/app/dashboard`.
- Entrada pública de login: `/login`.

## Módulos Actuales

- Autenticación y sesión privada.
- Clientes, doctores, clínicas y doctores internos.
- Órdenes de trabajo dental.
- Estados e historial de órdenes.
- Pagos, abonos, cancelación de pagos y saldos calculados.
- Dashboard operativo y financiero básico.
- Páginas iniciales de inventario, proveedores, usuarios y roles.

## Backlog Futuro

### Administración De Catálogo, Precios E Imágenes

Estado: pendiente, fuera de la fase actual y no implementado.

Esta mejora futura deberá vivir dentro de la app privada bajo `/app` y requerir permisos administrativos. Permitiría administrar secciones, productos, precios e imágenes del catálogo público sin exponer edición en el sitio público.

Fuente funcional: `docs/01-product/admin-catalog-management.md`.

Al diseñarla se deberá definir modelo de datos, endpoints, almacenamiento de imágenes, reglas de publicación, validación de formatos y permisos como `catalog.manage` o equivalente. El catálogo público actual seguirá usando `catalog-data.ts` hasta que esta fase sea aprobada e implementada.

## Clientes

- El cliente puede ser `Doctor`, `Clinic` u `Other`.
- Las clínicas pueden tener doctores internos.
- Clientes y doctores internos se desactivan; no hay delete físico en el MVP.
- La autorización usa `customers.view`, `customers.create` y `customers.edit`.

## Órdenes

- La orden de trabajo es la entidad central.
- Cada orden pertenece a un cliente.
- Una orden puede tener doctor interno solo si el cliente es clínica.
- Estados principales: recibida, en proceso, pruebas, lista para entrega, entregada y cancelada.
- Una orden cancelada es terminal en el MVP.
- La autorización usa `orders.view`, `orders.create`, `orders.edit` y `orders.changeStatus`.

## Pagos

- Los pagos son movimientos asociados a órdenes.
- Los saldos se calculan desde `TotalAmount` y pagos no cancelados.
- No hay edición libre ni delete físico de pagos en el MVP.
- Los pagos se cancelan con motivo.
- La autorización usa `payments.view`, `payments.create` y `payments.cancel`.

## Dashboard

- Ruta: `/app/dashboard`.
- API: `GET /api/dashboard/summary`.
- Acceso: `reports.view`.
- Secciones internas condicionadas:
  - operación con `orders.view`;
  - cobranza con `payments.view`;
  - clientes con `customers.view`.

## Validación De Acceso Fase 2.0

Estado: validado por código, build, tests y shell Angular; login real queda pendiente por falta de API/base/credenciales Admin locales.

- `/login` sigue siendo la entrada pública al sistema privado.
- `/app` sigue protegido por `authGuard`.
- `/app/dashboard` sigue siendo el dashboard privado real y requiere `reports.view`.
- Usuario sin sesión en `/app/dashboard` debe ser redirigido a `/login?returnUrl=%2Fapp%2Fdashboard`.
- Usuario autenticado sin `reports.view` debe ir a `/app/access-denied`, no a `/login`.
- `/dashboard` no es ruta privada real.
- `returnUrl` posterior al login solo acepta rutas internas seguras bajo `/app`; destinos externos o inválidos usan fallback `/app/dashboard`.
- Para validar login real, el humano debe configurar API/base local y Admin seguro, iniciar sesión desde `/login`, confirmar redirección a `/app/dashboard`, validar `GET /api/auth/me`, ejecutar logout y confirmar que `/app/dashboard` vuelve a pedir login.

## Validación De Acceso Fase 2.1

Estado: preflight local ejecutado; login real sigue pendiente por falta de SQL Server local accesible y Admin local configurado.

- La API local levantó en `http://localhost:5277` y `/health` respondió saludable.
- Angular levantó en `http://localhost:4200/` y `/login` respondió con shell Angular.
- La base declarada para desarrollo es local: `Server=localhost;Database=LaboratorioTlahuac_Dev`.
- SQL Server no estuvo accesible en `localhost`; las migraciones no se aplicaron.
- No existen credenciales Admin locales en variables de entorno ni user-secrets en este entorno.
- `GET /api/auth/csrf` respondió `204`; `GET /api/auth/me` sin sesión respondió `401`.
- Login real, `/api/auth/me` autenticado, logout y redirección tras logout quedan pendientes hasta configurar base y Admin locales.
- Admin recibirá `reports.view` cuando el seed pueda ejecutarse, porque el seed asigna todos los permisos a Admin y `/app/dashboard` requiere `reports.view`.

## Validación De Acceso Fase 2.1c

Estado: SQL Server Docker dedicado activo; migraciones, seed de arranque, endpoints anónimos y login real manual validados. La fase queda parcialmente cerrada porque `/app/dashboard` presenta un hallazgo de carga persistente al regresar a la página y falta confirmar `/api/auth/me` autenticado.

- Contenedor dedicado usado: `ldt-labdental-sql`.
- Volumen esperado: `ldt-labdental-sql-data`.
- Puerto local usado: `14336`.
- Base local usada: `LaboratorioTlahuac_Dev`.
- `docker ps --filter "name=ldt-labdental-sql"` confirmó el contenedor activo.
- `docker port ldt-labdental-sql` confirmó `1433/tcp -> 0.0.0.0:14336` y `1433/tcp -> [::]:14336`.
- Se confirmó que `codex-cobranza-sql` pertenece a otro proyecto y no se usó.
- No se usaron contenedores de otros proyectos.
- `dotnet ef migrations list` listó `20260508044157_InitialSecurityModel`, `20260509004819_AddCustomersAndInternalDoctors`, `20260509022531_AddWorkOrders` y `20260509053231_AddPayments`.
- `dotnet ef database update` confirmó que la base ya estaba al día.
- La API local levantó en `http://localhost:5277`; el seed Admin se ejecutó en arranque con configuración disponible en user-secrets y luego se apagó `SecuritySeed:RunOnStartup`.
- `GET /health` respondió `200`, `GET /api/auth/csrf` respondió `204` y `GET /api/auth/me` sin sesión respondió `401`.
- `LT_ADMIN_EMAIL` y `LT_ADMIN_PASSWORD` no están definidas en el proceso de Codex; no se inventaron ni extrajeron credenciales desde user-secrets.
- Validación manual posterior: `/login` carga correctamente, el login con Admin local creado por seed funciona y la navegación redirige a `/app/dashboard`.
- Dashboard: no queda validado; cargó una vez, pero al regresar a la página queda en `Cargando dashboard...`.
- `/api/auth/me` autenticado: no confirmado porque el resultado manual no fue marcado como `sí`.
- Logout: no confirmado como acción independiente porque el resultado manual no fue marcado; la redirección posterior sí fue reportada como correcta.
- Después de logout, `/app/dashboard` redirige a `/login?returnUrl=%2Fapp%2Fdashboard`.
- `/login` sigue siendo público; `/app` y `/app/dashboard` siguen siendo privados; `/dashboard` no es ruta privada real.

## Validación De Acceso Fase 2.1d

Estado: corrección mínima aplicada al dashboard privado para que una consulta pendiente no deje la pantalla indefinidamente en `Cargando dashboard...`.

- Endpoint usado por `/app/dashboard`: `GET /api/dashboard/summary`.
- Permiso de ruta y endpoint: `reports.view`.
- El Admin creado por seed recibe `reports.view` porque el seed asigna todos los permisos de `Permissions.All`.
- Falta de sesión devuelve `401`; falta de permiso debe llevar a `/app/access-denied` por frontend o `403` por API, no a carga infinita.
- Causa probable identificada: si `GET /api/dashboard/summary` queda pendiente, el componente no tenia timeout y `isLoading` permanecia activo.
- Corrección aplicada: timeout de 15 segundos en la consulta del dashboard y mensaje de error controlado cuando la API tarda demasiado.
- No se modificaron rutas privadas, `AuthService`, guards, backend, permisos, migraciones, deploy ni dependencias.
- `/api/auth/me` autenticado y `GET /api/dashboard/summary` autenticado quedan pendientes de confirmar con credenciales Admin disponibles o navegador manual.
- `/login` sigue siendo público; `/app` y `/app/dashboard` siguen siendo privados; `/dashboard` no es ruta privada real.

## Permisos

El sistema autoriza por permisos, no por nombre de rol. El rol Admin inicial recibe todos los permisos mediante seed.

Fuente técnica de auth: `docs/03-architecture/AUTH_FLOW.md`.

## Exclusiones Actuales

- Inventario automático.
- Proveedores funcionales completos.
- CFDI/facturación.
- Reportes avanzados.
- Exportación Excel/PDF avanzada.
- Migración completa del Excel.
- WhatsApp automatizado.
- App móvil nativa.
- Administración de catálogo, precios e imágenes.

## QA

La QA funcional del MVP administrativo está documentada en:

- `docs/08-qa/mvp-qa-checklist.md`
- `docs/08-qa/mvp-acceptance-checklist.md`
- `docs/08-qa/known-issues.md`
