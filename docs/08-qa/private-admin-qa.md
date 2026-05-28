# QA Sistema Privado Con Admin - Fase 2.2

## Resumen Ejecutivo

Fase 2.2 ejecutada como QA manual/técnico del sistema privado existente bajo `/app`, usando Admin local y sin modificar código, rutas, backend, auth, cookies, XSRF, endpoints, base de datos por migraciones, deploy ni dependencias.

Resultado general: el flujo Admin por API local funciona. Login, `/api/auth/me`, dashboard, clientes, ordenes, pagos y logout respondieron correctamente. Se crearon datos de prueba locales marcados como QA para validar altas y mutaciones mínimas. No hay hallazgos bloqueantes.

Limitación de ejecución: no hay navegador/headless local instalado en este entorno, por lo que las redirecciones visuales de guards, consola del navegador y Network del navegador se validaron por código y por respuestas HTTP/API, no por inspección visual automatizada.

## Entorno Usado

- Fecha de ejecución local: 2026-05-27, America/Mexico_City, `CST -0600`.
- API local: `http://localhost:5277`.
- Angular dev server: `http://localhost:4200`.
- SQL usado: `ldt-labdental-sql`.
- Puerto SQL local: `14336 -> 1433/tcp`.
- Base local: `LaboratorioTlahuac_Dev`.
- Contenedor excluido: `codex-cobranza-sql`; no apareció activo y no se usó.
- Credenciales Admin: tomadas de variables de entorno locales `LT_ADMIN_EMAIL` y `LT_ADMIN_PASSWORD`, sin imprimir valores.

## Preflight

- `docker ps --filter "name=ldt-labdental-sql"`: contenedor activo.
- `docker port ldt-labdental-sql`: `1433/tcp -> 0.0.0.0:14336` y `[::]:14336`.
- `docker ps --filter "name=codex-cobranza-sql"`: sin contenedor activo.
- `GET /health`: `200`.
- Angular `/`: `200` con shell Angular.

## Rutas Publicas Revisadas

Todas respondieron con shell Angular `200` desde `http://localhost:4200`.

| Ruta | Resultado |
| --- | --- |
| `/` | Carga publica confirmada por HTTP. |
| `/servicios` | Carga publica confirmada por HTTP. |
| `/catalogo` | Carga publica confirmada por HTTP. |
| `/contacto` | Carga publica confirmada por HTTP. |
| `/login` | Carga publica confirmada por HTTP. |

No se hicieron cambios al sitio publico durante esta fase.

## Rutas Privadas Reales

Detectadas en `app.routes.ts` bajo `/app`.

| Ruta | Permiso | Resultado QA |
| --- | --- | --- |
| `/app` | Sesion requerida | Redirige por router a `/app/dashboard` si hay sesion; protegido por `authGuard`. |
| `/app/dashboard` | `reports.view` | Endpoint de resumen respondio `200` con Admin. |
| `/app/ordenes` | `orders.view` | Listado respondio `200`. |
| `/app/ordenes/nueva` | `orders.create` | Alta validada por API con dato QA. |
| `/app/ordenes/:id` | `orders.view` | Detalle implicado por alta/edicion/pago de orden QA. |
| `/app/ordenes/:id/editar` | `orders.edit` | Edicion validada por API. |
| `/app/clientes` | `customers.view` | Listado respondio `200`. |
| `/app/clientes/nuevo` | `customers.create` | Alta validada por API con dato QA. |
| `/app/clientes/:id` | `customers.view` | Detalle implicado por alta/edicion de cliente QA. |
| `/app/clientes/:id/editar` | `customers.edit` | Edicion validada por API. |
| `/app/pagos` | `payments.view` | Listado respondio `200`. |
| `/app/inventario` | `inventory.view` | Pagina placeholder de roadmap futuro. |
| `/app/proveedores` | `suppliers.view` | Pagina placeholder de roadmap futuro. |
| `/app/admin/usuarios` | `users.manage` | Pagina placeholder pendiente de implementacion. |
| `/app/admin/roles` | `roles.manage` | Pagina placeholder pendiente de implementacion. |
| `/app/access-denied` | Sesion requerida | Pagina existente para usuario autenticado sin permiso. |

`/dashboard` raiz no existe como ruta privada real. El router conserva wildcard hacia la home publica.

## Auth Y Sesion

### Sin Sesion

- `GET /api/auth/me`: `401`.
- `GET /api/dashboard/summary`: `401`.
- Endpoints privados revisados sin sesion (`/api/customers`, `/api/work-orders`, `/api/payments`, `/api/dashboard/summary`): `401`.
- Por codigo, `authGuard` y `permissionGuard` redirigen a `/login?returnUrl=...` cuando no hay sesion o falla `ensureSession()`.

### Con Admin

Flujo autenticado ejecutado por HTTP con cookie jar temporal:

- `GET /api/auth/csrf`: `204`.
- `POST /api/auth/login`: `200`.
- `GET /api/auth/csrf` posterior al login: `204`.
- `GET /api/auth/me`: `200`.
- Permisos reportados por `/api/auth/me`: 19.
- Permisos confirmados: `reports.view`, `orders.view`, `customers.view`, `payments.view`.
- `GET /api/dashboard/summary`: `200`.

### Logout

- `POST /api/auth/logout` con XSRF renovado despues del login: `200`.
- `GET /api/auth/me` despues de logout: `401`.

### ReturnUrl

Confirmado por lectura de `login-page.component.ts`:

- Acepta rutas internas seguras bajo `/app`, como `/app/dashboard`.
- Rechaza rutas externas, protocol-relative, esquemas como `javascript:`, backslash y valores con espacios al inicio/final.
- Fallback seguro: `/app/dashboard`.

### Usuario Sin Permiso

No se probo con usuario limitado porque no hay usuario de prueba sin permisos disponible en esta fase. Por codigo, `permissionGuard` diferencia usuario autenticado sin permiso y redirige a `/app/access-denied`, no a `/login`.

## Resultado Por Modulo Privado

### Dashboard

- Ruta: `/app/dashboard`.
- API: `GET /api/dashboard/summary`.
- Resultado Admin: `200`.
- Estado final despues de datos QA:
  - ordenes activas: 1.
  - ultimas ordenes: 1.
  - cuentas con saldo pendiente: 1.
  - ordenes con pago parcial: 1.
  - ultimos pagos: 1.
  - clientes activos: 1.
- El endpoint ya no queda pendiente en la prueba API.
- El componente conserva timeout de 15 segundos y mensaje controlado si la consulta tarda demasiado.

### Clientes

- Ruta principal: `/app/clientes`.
- Listado Admin: `200`.
- Estado inicial: vacio.
- Alta de cliente QA: `201`.
- Edicion de cliente QA: `200`.
- Busqueda de cliente QA: `200`, total `1`.
- UI disponible por codigo: filtros por busqueda/tipo/estado, paginacion, alta, detalle, edicion, activar/desactivar y validaciones visibles.
- Doctores internos: seccion disponible para clientes tipo clinica; no se creo clinica QA en esta fase.

### Ordenes

- Ruta principal: `/app/ordenes`.
- Listado Admin: `200`.
- Estados disponibles: `200`, 7 opciones.
- Estado inicial: vacio.
- Alta de orden QA: `201`.
- Edicion de orden QA: `200`.
- Cambio de estado QA a `InProcess`: `200`.
- Busqueda de orden QA: `200`, total `1`.
- UI disponible por codigo: filtros, alta, detalle, edicion, cambio de estado, historial, pagos embebidos y validaciones de fechas/campos.

### Pagos

- Ruta principal: `/app/pagos`.
- Listado Admin: `200`.
- Metodos disponibles: `200`, 4 opciones.
- Estado inicial: vacio.
- Registro de pago QA: `201`.
- Resumen de pago de la orden QA: `200`, total `1200`, pagado `300`, saldo `900`, estado `Partial`.
- Busqueda de pago QA: `200`, total `1`.
- UI disponible por codigo: listado, filtros, registro de pago desde detalle de orden y cancelacion con motivo.
- Cancelacion de pago no se ejecuto para conservar un pago activo de prueba.

### Inventario

- Ruta: `/app/inventario`.
- Estado: pagina placeholder con mensaje de modulo futuro segun roadmap.
- No hay flujo funcional de inventario en esta fase.

### Proveedores

- Ruta: `/app/proveedores`.
- Estado: pagina placeholder con mensaje de modulo futuro segun roadmap.
- No hay flujo funcional de proveedores en esta fase.

### Usuarios Y Roles

- Rutas: `/app/admin/usuarios` y `/app/admin/roles`.
- Estado: paginas placeholder pendientes de implementacion.
- No hay CRUD funcional de usuarios/roles en esta fase.

### Navegacion Privada

- Menu privado existe en `PrivateLayoutComponent`.
- Los enlaces se muestran segun permisos del usuario autenticado.
- Con Admin, `/api/auth/me` confirma permisos necesarios para dashboard, ordenes, clientes y pagos.
- Logout por API validado correctamente.
- No hay `routerLinkActive` visible en la navegacion privada actual; se registra como hallazgo bajo.

## Datos De Prueba Creados

Quedan en la base local `LaboratorioTlahuac_Dev`.

| Tipo | Identificador | Dato |
| --- | --- | --- |
| Cliente | `fd5fe049-33e9-4732-80fa-790d140468f4` | `Cliente QA LDT 20260527-210940 Editado` |
| Orden | `967c2750-cbb4-4aec-908c-14a04fd120fb` | `OT-20260528-201A16`, paciente `Paciente QA LDT Editado` |
| Pago | `561b6d36-6dff-4fe3-b08e-6705dc0947dd` | Referencia `Pago QA LDT 20260527-210940`, monto `300` |

No se limpiaron datos de prueba.

## Hallazgos

### Medio

| Ruta | Hallazgo | Pasos | Actual | Esperado | Evidencia | Recomendacion |
| --- | --- | --- | --- | --- | --- | --- |
| `/app/dashboard` | Metrica "Para hoy" requiere definicion de zona horaria de negocio. | En Mexico local `2026-05-27 21:xx CST`, crear orden QA con entrega `2026-05-27` y consultar dashboard. | `dueToday=0` aunque la fecha local capturada coincide con el dia local de QA. | Las metricas de "hoy", vencidas y proximos dias deben alinearse con la zona horaria operativa del laboratorio. | `GET /api/dashboard/summary` respondio `200` con `activeOrders=1` y `dueToday=0`. | Fase 2.3: definir zona horaria de negocio y ajustar/calibrar metricas de dashboard si aplica. |

### Bajo

| Ruta | Hallazgo | Pasos | Actual | Esperado | Evidencia | Recomendacion |
| --- | --- | --- | --- | --- | --- | --- |
| `/app/*` | La navegacion privada no marca visualmente ruta activa. | Revisar `PrivateLayoutComponent`. | Los enlaces usan `routerLink`, pero no `routerLinkActive` ni clase activa. | El usuario debe identificar rapidamente la seccion actual. | Lectura de `private-layout.component.ts`. | Fase 2.3: agregar estado activo visual si se prioriza pulido de UX privado. |

### Observaciones

| Ruta | Observacion | Evidencia | Recomendacion |
| --- | --- | --- | --- |
| `/app/access-denied` | No se probo usuario autenticado sin permiso por falta de usuario limitado local. | `permissionGuard` redirige a `/app/access-denied` si hay sesion sin permiso. | Crear usuario QA limitado en una fase posterior si se requiere evidencia manual completa de permisos. |
| `/app/inventario`, `/app/proveedores`, `/app/admin/usuarios`, `/app/admin/roles` | Son paginas placeholder documentadas como pendientes o futuras. | Componentes muestran mensajes de modulo futuro/pendiente. | Mantener como backlog; no tratarlas como bug funcional del MVP actual. |
| Navegador | No se inspecciono consola/Network en navegador real o headless. | No hay Chromium/Chrome/Firefox/Playwright local sin instalar dependencias. | Ejecutar pase visual/manual en navegador real si se requiere evidencia de consola. |

## Pendientes

- Validar en navegador real la redireccion visual sin sesion a `/login?returnUrl=%2Fapp%2Fdashboard`.
- Probar usuario autenticado sin permiso contra `/app/access-denied`.
- Definir zona horaria formal de negocio para metricas del dashboard.
- Decidir si los placeholders de inventario/proveedores/usuarios/roles deben ocultarse, quedarse como backlog visible o implementarse en fases posteriores.

## Validaciones Tecnicas De Cierre

- `npm run build` desde `src/LaboratorioTlahuac.Web`: correcto.
- `dotnet build`: correcto, 0 warnings y 0 errores.
- `dotnet test`: correcto; Domain 1/1, Application 1/1 y API 90/90.
- `git diff --check`: correcto.
- `rg "/dashboard" .`: revisado; no se detectó `/dashboard` como ruta privada real nueva.
- `rg "/app/dashboard" .`: revisado; confirma que el dashboard privado real se mantiene bajo `/app/dashboard`.
- `rg "/login" src/LaboratorioTlahuac.Web/src/app docs README.md AGENTS.md`: revisado; confirma `/login` como entrada pública y endpoints de auth existentes.
- `rg --files-with-matches "LT_ADMIN_PASSWORD" .`: ejecutado con salida limitada a archivos para no imprimir valores.
- `rg --files-with-matches "LDT_SQL_SA_PASSWORD" .`: ejecutado con salida limitada a archivos para no imprimir valores.
- `rg --files-with-matches "ConnectionStrings" src docs README.md`: ejecutado con salida limitada a archivos para no imprimir valores.
- `rg "codex-cobranza-sql" docs README.md AGENTS.md`: revisado; las menciones corresponden a documentación de no uso o histórico.

## Recomendacion De Siguiente Fase

Siguiente fase recomendada: Fase 2.3 - Correccion de hallazgos QA del sistema privado.

Motivo: no hay bloqueantes, pero si hay hallazgos de zona horaria de dashboard y pulido de navegacion privada antes de preparar staging/deploy.
