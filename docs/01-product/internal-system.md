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
- Usuario QA limitado local Development-only para validación de permisos.
- Clientes, doctores, clínicas y doctores internos.
- Órdenes de trabajo dental.
- Estados e historial de órdenes.
- Pagos, abonos, cancelación de pagos y saldos calculados.
- Dashboard operativo y financiero básico.
- Páginas iniciales de inventario, proveedores, usuarios y roles.

## Análisis Operativo Fase 3.1

Estado: documentado, sin implementación de código.

La Fase 3.1 define el siguiente frente operativo sobre órdenes existentes: etiquetas internas, etiquetas de entrega, flujo de salida a repartidor, captura de recibido y priorización futura de usuarios/roles y catálogo.

Fuentes funcionales:

- `docs/01-product/operations-orders-delivery.md`
- `docs/01-product/label-printing.md`
- `docs/01-product/driver-mobile-workflow.md`

Decisión principal: no crear un panel duplicado de órdenes. El flujo debe extender `/app/ordenes`, especialmente `/app/ordenes/:id`, porque ahí ya existen datos de orden, estado, historial y pagos.

Siguiente fase recomendada: Fase 3.2 - MVP impresión de etiquetas desde órdenes existentes.

## Backlog Futuro

### Órdenes, Etiquetas Y Reparto

Estado: análisis documentado en Fase 3.1.

Prioridad sugerida:

1. Fase 3.2: imprimir etiqueta interna y etiqueta de entrega desde `/app/ordenes/:id`.
2. Fase 3.3: entrega/repartidor mobile-first bajo `/app/entregas` o ruta equivalente.
3. Fase 3.4: administración de usuarios/roles.
4. Fase 3.5: administración de catálogo.

Fase 3.2 puede implementarse sin migraciones si reutiliza datos existentes de orden/cliente y CSS de impresión. Fase 3.3 sí requerirá diseño de base para asignación, salida, entrega, receptor y trazabilidad.

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
- Zona horaria operativa: `America/Mexico_City`, configurable con `Dashboard:BusinessTimeZone`.
- El "hoy" operativo se calcula convirtiendo `clock.UtcNow` a la zona horaria del laboratorio; no se calcula con fecha UTC pura.
- Las métricas `dueToday`, `overdue` y `upcomingDue` usan la fecha operativa local del laboratorio.
- `generatedAtUtc` sigue siendo UTC y `DeliveryDate` conserva su significado como fecha de entrega capturada.
- Secciones internas condicionadas:
  - operación con `orders.view`;
  - cobranza con `payments.view`;
  - clientes con `customers.view`.

## Validación De Acceso Fase 2.0

Estado: validado por código, build, tests y shell Angular; login real quedó pendiente en esa fase y fue cerrado posteriormente por validación manual con Admin local.

- `/login` sigue siendo la entrada pública al sistema privado.
- `/app` sigue protegido por `authGuard`.
- `/app/dashboard` sigue siendo el dashboard privado real y requiere `reports.view`.
- Usuario sin sesión en `/app/dashboard` debe ser redirigido a `/login?returnUrl=%2Fapp%2Fdashboard`.
- Usuario autenticado sin `reports.view` debe ir a `/app/access-denied`, no a `/login`.
- `/dashboard` no es ruta privada real.
- `returnUrl` posterior al login solo acepta rutas internas seguras bajo `/app`; destinos externos o inválidos usan fallback `/app/dashboard`.
- La validación manual posterior confirmó inicio de sesión con Admin local y acceso a `/app/dashboard`; `GET /api/auth/me` autenticado y logout independiente quedan como evidencia opcional para Fase 2.2.

## Validación De Acceso Fase 2.1

Estado: preflight local ejecutado; login real quedó pendiente en esa fase por falta de SQL Server local accesible y Admin local configurado.

- La API local levantó en `http://localhost:5277` y `/health` respondió saludable.
- Angular levantó en `http://localhost:4200/` y `/login` respondió con shell Angular.
- La base declarada para desarrollo es local: `Server=localhost;Database=LaboratorioTlahuac_Dev`.
- SQL Server no estuvo accesible en `localhost`; las migraciones no se aplicaron.
- No existen credenciales Admin locales en variables de entorno ni user-secrets en este entorno.
- `GET /api/auth/csrf` respondió `204`; `GET /api/auth/me` sin sesión respondió `401`.
- Login real, `/api/auth/me` autenticado, logout y redirección tras logout quedaron pendientes en esa fase; login real y dashboard autenticado se cerraron posteriormente por validación manual.
- Admin recibirá `reports.view` cuando el seed pueda ejecutarse, porque el seed asigna todos los permisos a Admin y `/app/dashboard` requiere `reports.view`.

## Validación De Acceso Fase 2.6

Estado: mecanismo seguro Development-only implementado para crear o sincronizar un usuario QA limitado local.

- Configuracion principal: `SecuritySeed:LimitedQaUser`.
- Activacion: `SecuritySeed:LimitedQaUser:RunOnStartup=true`.
- Datos sensibles: `LT_QA_LIMITED_EMAIL`, `LT_QA_LIMITED_PASSWORD` y `LT_QA_LIMITED_FULL_NAME`, o equivalentes bajo `SecuritySeed:LimitedQaUser`.
- Permisos: `SecuritySeed:LimitedQaUser:Permissions`, sincronizado contra `Permissions.All`.
- Permiso recomendado: `customers.view`.
- Permiso excluido para validar `/app/access-denied` en dashboard: `reports.view`.
- El usuario QA limitado puede iniciar sesion y conserva permisos limitados.
- `/api/auth/me` devuelve permisos limitados sin `passwordHash`.
- `/api/dashboard/summary` responde `403` con sesion limitada sin `reports.view`.
- Sin sesion, `/api/dashboard/summary` responde `401`.
- La validacion manual esperada en navegador es entrar a `/app/dashboard` con usuario limitado y confirmar redireccion a `/app/access-denied`.
- No se cambiaron rutas privadas, `AuthService`, guards, cookies, XSRF, endpoints, migraciones ni deploy.

## Validación De Acceso Fase 2.1c

Estado: SQL Server Docker dedicado activo; migraciones, seed de arranque, endpoints anónimos y login real manual validados. La fase quedó parcialmente cerrada por el hallazgo de carga persistente en `/app/dashboard`; ese pendiente se cierra posteriormente en Fase 2.1d.

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
- Dashboard: no queda validado en Fase 2.1c; cargó una vez, pero al regresar a la página queda en `Cargando dashboard...`. Este pendiente se cierra posteriormente en Fase 2.1d.
- `/api/auth/me` autenticado: no confirmado porque el resultado manual no fue marcado como `sí`.
- Logout: no confirmado como acción independiente porque el resultado manual no fue marcado; la redirección posterior sí fue reportada como correcta.
- Después de logout, `/app/dashboard` redirige a `/login?returnUrl=%2Fapp%2Fdashboard`.
- `/login` sigue siendo público; `/app` y `/app/dashboard` siguen siendo privados; `/dashboard` no es ruta privada real.

## Validación De Acceso Fase 2.1d

Estado: corrección mínima aplicada y cerrada manualmente; el dashboard privado ya no queda indefinidamente en `Cargando dashboard...`.

- Endpoint usado por `/app/dashboard`: `GET /api/dashboard/summary`.
- Permiso de ruta y endpoint: `reports.view`.
- El Admin creado por seed recibe `reports.view` porque el seed asigna todos los permisos de `Permissions.All`.
- Falta de sesión devuelve `401`; falta de permiso debe llevar a `/app/access-denied` por frontend o `403` por API, no a carga infinita.
- Causa probable identificada: si `GET /api/dashboard/summary` queda pendiente, el componente no tenia timeout y `isLoading` permanecia activo.
- Corrección aplicada: timeout de 15 segundos en la consulta del dashboard y mensaje de error controlado cuando la API tarda demasiado.
- No se modificaron rutas privadas, `AuthService`, guards, backend, permisos, migraciones, deploy ni dependencias.
- Validación manual 2026-05-27: `/login` carga correctamente, login con Admin local validado, redirección a `/app/dashboard` validada y dashboard autenticado validado por el responsable del proyecto.
- Flujo autenticado validado manualmente; `GET /api/auth/me` autenticado no fue inspeccionado de forma independiente.
- `GET /api/dashboard/summary` autenticado queda validado indirectamente por la carga correcta del dashboard; el endpoint no fue inspeccionado de forma independiente.
- Redirección posterior a logout o sesión cerrada validada: `/app/dashboard` redirige a `/login?returnUrl=%2Fapp%2Fdashboard`; logout como acción independiente no queda documentado como inspeccionado por separado.
- `/login` sigue siendo público; `/app` y `/app/dashboard` siguen siendo privados; `/dashboard` no es ruta privada real.
- Siguiente etapa: Fase 2.2 - QA manual más amplio del sistema privado con Admin.

## Validación De Acceso Fase 2.2

Estado: QA manual/técnico del sistema privado con Admin ejecutado y documentado en `docs/08-qa/private-admin-qa.md`.

- Entorno usado: API local en `http://localhost:5277`, Angular en `http://localhost:4200`, SQL dedicado `ldt-labdental-sql`, puerto `14336`, base `LaboratorioTlahuac_Dev`.
- No se usó `codex-cobranza-sql`.
- `/login` sigue como entrada pública.
- `/app` y `/app/dashboard` siguen bajo zona privada.
- `/dashboard` no es ruta privada real.
- Sin sesión, endpoints privados revisados responden `401`.
- Con Admin, `POST /api/auth/login` respondió `200`, `GET /api/auth/me` respondió `200` con 19 permisos y `GET /api/dashboard/summary` respondió `200`.
- Logout validado: `POST /api/auth/logout` respondió `200` y `/api/auth/me` posterior respondió `401`.
- Se validaron clientes, órdenes y pagos con datos QA locales: alta/edición de cliente, alta/edición/cambio de estado de orden y registro de pago.
- Inventario, proveedores, usuarios y roles siguen como páginas placeholder pendientes o futuras, sin flujo funcional completo.
- Hallazgos registrados en Fase 2.2: definir zona horaria de negocio para métricas del dashboard y agregar estado activo visual en navegación privada; ambos quedaron corregidos posteriormente en Fase 2.3.
- Usuario autenticado sin permiso no se probó por falta de usuario limitado local; por código, `permissionGuard` conserva redirección a `/app/access-denied`.

## Corrección De Hallazgos Fase 2.3

Estado: corrección mínima aplicada y validada por build/pruebas.

- Hallazgo medio corregido: el dashboard ya no usa fecha UTC pura para `dueToday`, `overdue` ni `upcomingDue`.
- Zona horaria de negocio definida: `America/Mexico_City`.
- Configuración técnica: `Dashboard:BusinessTimeZone`; default seguro en código y `appsettings.json`.
- Compatibilidad de IDs: el ID canónico es IANA `America/Mexico_City`; el backend acepta `Central Standard Time (Mexico)` para entornos Windows cuando aplique.
- Prueba agregada: `OperationalSummaryUsesBusinessTimeZoneDateWhenUtcDateDiffers`, con UTC y Mexico City en fechas distintas.
- Hallazgo bajo corregido: la navegación privada usa `routerLinkActive`, `ariaCurrentWhenActive` y clase visual activa; `/app/dashboard` usa match exacto.
- Validación ejecutada: `npm run build`, `dotnet build`, `dotnet test`, `git diff --check` y búsquedas obligatorias.
- No se cambiaron rutas, permisos, logout, `AuthService`, guards, cookies, XSRF, endpoints públicos, migraciones, deploy ni dependencias.

## Validación De Acceso Fase 2.4

Estado: pase manual/técnico privado ejecutado con Admin local; navegación activa validada por código/build y dashboard validado por API/datos QA. Pase visual humano y usuario limitado real quedan pendientes por limitaciones seguras del entorno.

- Entorno usado: API local en `http://localhost:5277`, Angular en `http://localhost:4200`, SQL dedicado `ldt-labdental-sql`, puerto `14336`, base `LaboratorioTlahuac_Dev`.
- No se usó `codex-cobranza-sql`.
- `/login` sigue como entrada pública.
- `/app` y `/app/dashboard` siguen bajo zona privada.
- `/dashboard` no es ruta privada real.
- Rutas públicas `/`, `/servicios`, `/catalogo`, `/contacto` y `/login` respondieron con shell Angular `200`.
- Rutas privadas objetivo bajo `/app` respondieron con shell Angular `200`; la ejecución visual de guards/estado activo no se pudo probar por falta de navegador/headless local sin instalar dependencias.
- Con Admin, `POST /api/auth/login` respondió `200`, `GET /api/auth/me` respondió `200` con 19 permisos y `GET /api/dashboard/summary` respondió `200`.
- Logout validado: `POST /api/auth/logout` respondió `200`; `/api/auth/me` y `/api/dashboard/summary` posteriores respondieron `401`.
- Dashboard zona horaria validado con datos QA: una orden con `DeliveryDate=2026-05-27`, igual a la fecha operativa local, incrementó `dueToday` en +1 y `upcomingDue` en +1.
- `generatedAtUtc` sigue siendo UTC, serializado con offset `+00:00`; `DeliveryDate` conserva significado de fecha capturada.
- Datos QA locales creados: cliente con prefijo `Cliente QA LDT F2.4` (`a5c48811-e171-450b-963e-f929a0d71084`) y orden `OT-20260528-82F6A6` (`53a35d65-a3ff-4f7d-ab7c-b0b2d658df44`), no limpiados.
- Usuario autenticado sin permiso no se probó con cuenta limitada porque no existe mecanismo seguro local fuera de fixtures de pruebas y no se autorizó SQL directo; por código, `permissionGuard` conserva redirección a `/app/access-denied`.
- No se modificaron rutas, permisos, logout, `AuthService`, guards, cookies, XSRF, endpoints, migraciones, deploy ni dependencias.

## Validación De Acceso Fase 2.5

Estado: pase visual humano privado completado y definición documental de mecanismo seguro para usuario QA limitado.

- El responsable del proyecto confirmó en navegador real `/login`, login Admin, `/app/dashboard`, `/app/clientes`, `/app/ordenes`, `/app/pagos`, navegación activa en rutas principales, placeholders de inventario/proveedores/usuarios/roles, logout y redirección de `/app/dashboard` sin sesión a `/login?returnUrl=%2Fapp%2Fdashboard`.
- Fase 2.5 queda completada para pase visual humano privado y registrada en `docs/08-qa/private-admin-qa.md`.
- `/login` sigue documentado como entrada pública.
- `/app` y `/app/dashboard` siguen documentadas como zona privada.
- `/dashboard` raíz no es ruta privada real; se conserva como confirmación por código/routing.
- El sitio público no tuvo regresión visible reportada.
- El Admin existente no se alteró y sigue definido por seed con todos los permisos de `Permissions.All`.
- Los permisos actuales se emiten como claims `permission`.
- El usuario autenticado sin permiso sigue debiendo ir a `/app/access-denied`, pero falta validarlo con usuario real limitado.
- Mecanismo recomendado: seed QA limitado solo Development, desactivado por default y controlado por user-secrets o variables de entorno, documentado en `docs/08-qa/limited-user-qa-plan.md`.
- No se implementó el mecanismo en esta fase.
- No se modificaron rutas, permisos, logout, `AuthService`, guards, cookies, XSRF, endpoints, migraciones, deploy ni dependencias.

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
- `docs/08-qa/private-admin-qa.md`
