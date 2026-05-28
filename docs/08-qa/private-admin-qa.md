# QA Sistema Privado Con Admin - Fase 2.2

## Resumen Ejecutivo

Fase 2.2 ejecutada como QA manual/técnico del sistema privado existente bajo `/app`, usando Admin local y sin modificar código, rutas, backend, auth, cookies, XSRF, endpoints, base de datos por migraciones, deploy ni dependencias.

Resultado general: el flujo Admin por API local funciona. Login, `/api/auth/me`, dashboard, clientes, ordenes, pagos y logout respondieron correctamente. Se crearon datos de prueba locales marcados como QA para validar altas y mutaciones mínimas. No hay hallazgos bloqueantes.

Limitación de ejecución: no hay navegador/headless local instalado en este entorno, por lo que las redirecciones visuales de guards, consola del navegador y Network del navegador se validaron por código y por respuestas HTTP/API, no por inspección visual automatizada.

Seguimiento Fase 2.3: los dos hallazgos principales de este reporte quedaron corregidos por código y pruebas. El dashboard usa fecha operativa de negocio `America/Mexico_City` para `dueToday`, `overdue` y `upcomingDue`; la navegación privada marca la ruta activa con `routerLinkActive` y estilos accesibles.

## Fase 2.5 - Cierre visual humano privado y usuario limitado

### Fecha

- Documentación: 2026-05-28 07:26 CST, America/Mexico_City.
- Cierre manual recibido: 2026-05-28 09:16 CST, America/Mexico_City.
- Sin commits.
- Sin cambios de código.

### Entorno

- Entorno objetivo documentado: Development local.
- SQL dedicado esperado/validado en fase previa: `ldt-labdental-sql`.
- Puerto SQL local: `14336`.
- Base local: `LaboratorioTlahuac_Dev`.
- No se usó `codex-cobranza-sql`.
- No se ejecutó `dotnet user-secrets list`.
- No se imprimieron secretos.
- No se instalaron dependencias.
- Pase visual/manual: navegador real del responsable del proyecto.

### Resultado Visual Humano Por Ruta

El responsable del proyecto confirmó el pase visual/manual privado en navegador real. Fase 2.5 queda cerrada como completada para el pase visual humano privado.

| Punto | Resultado |
| --- | --- |
| `/login` | OK. |
| Login Admin | OK. |
| `/app/dashboard` | OK. |
| Navegación activa en `/app/dashboard` | OK. |
| `/app/clientes` | OK. |
| Navegación activa en `/app/clientes` | OK. |
| `/app/ordenes` | OK. |
| Navegación activa en `/app/ordenes` | OK. |
| `/app/pagos` | OK. |
| Navegación activa en `/app/pagos` | OK. |
| `/app/inventario` | OK como placeholder. |
| `/app/proveedores` | OK como placeholder. |
| `/app/admin/usuarios` | OK como placeholder. |
| `/app/admin/roles` | OK como placeholder. |
| Logout | OK. |
| `/app/dashboard` sin sesión redirige a `/login?returnUrl=%2Fapp%2Fdashboard` | OK. |
| `/dashboard` raíz no es ruta privada real | OK. |
| Sitio público sin regresión visible | OK. |
| Observaciones visuales | Sin bloqueantes visuales reportados. |

### Navegación Activa

Estado técnico conservado desde Fase 2.3/Fase 2.4:

- `PrivateLayoutComponent` usa `RouterLinkActive`.
- Los enlaces privados usan `routerLinkActive="is-active"` y `ariaCurrentWhenActive="page"`.
- `/app/dashboard` usa `[routerLinkActiveOptions]="{ exact: true }"`.
- Los estilos `.is-active`, `:hover` y `:focus-visible` existen en `private-layout.component.scss`.
- El pase humano confirmó navegación activa en `/app/dashboard`, `/app/clientes`, `/app/ordenes` y `/app/pagos`.

### Logout Y Redirecciones Sin Sesión

Estado técnico/API conservado desde Fase 2.4:

- Login Admin por API, `/api/auth/me` con 19 permisos, dashboard/listados y logout fueron validados.
- Después de logout, `/api/auth/me` y `/api/dashboard/summary` respondieron `401`.
- Por código, usuario sin sesión en `/app/*` redirige a `/login?returnUrl=...`.
- Por código, usuario autenticado sin permiso redirige a `/app/access-denied`.
- El pase humano confirmó logout y redirección de `/app/dashboard` sin sesión a `/login?returnUrl=%2Fapp%2Fdashboard`.

### Estado De Usuario Limitado

No existe todavía un mecanismo seguro de producto para crear usuario QA limitado en la base local real.

Revisión técnica:

- El seed actual asegura permisos y rol Admin; el Admin recibe todos los permisos de `Permissions.All`.
- Los permisos se emiten como claims `permission`.
- `Permissions.All` contiene 19 permisos.
- Los usuarios limitados existentes están en fixtures de pruebas con SQLite en memoria, no en la base local real.
- Las páginas `/app/admin/usuarios` y `/app/admin/roles` siguen como placeholders.
- No se creó usuario por SQL manual.
- No se alteró el Admin.
- No se crearon migraciones.

### Recomendación Sobre Mecanismo Seguro

Recomendación: documentar como backlog técnico inmediato la Opción 1, seed QA limitado solo Development, sin implementarla en Fase 2.5.

Condiciones mínimas recomendadas:

- Desactivado por default.
- Solo ejecutable con `Environment=Development`.
- Habilitado explícitamente por `SecuritySeed:QaLimited:Enabled=true`.
- Email, password y nombre tomados de user-secrets o variables de entorno, no de archivos versionados.
- Password nunca impresa.
- Usuario creado con servicios/entidades existentes y `PasswordHasher<User>`, no con SQL manual.
- Permisos configurables por allowlist de `Permissions.All`; para validar `/app/access-denied` contra `/app/dashboard`, no incluir `reports.view`.

Plan detallado: `docs/08-qa/limited-user-qa-plan.md`.

### Hallazgos

#### Bloqueante

Ninguno.

#### Alto

Ninguno.

#### Medio

Ninguno.

#### Bajo

| Ruta | Hallazgo | Evidencia | Recomendación |
| --- | --- | --- | --- |
| `/app/access-denied` | No se puede cerrar evidencia con usuario limitado real porque no existe mecanismo seguro local implementado. | Seed actual solo cubre Admin; fixtures limitados solo existen en pruebas; usuarios/roles siguen como placeholders. | Implementar posteriormente el seed QA limitado solo Development si se autoriza tocar backend mínimo. |

#### Observación

| Ruta | Observación | Evidencia | Recomendación |
| --- | --- | --- | --- |
| `/app/*` | Sin bloqueantes visuales reportados en el pase humano privado. | Confirmación manual del responsable del proyecto. | Mantener monitoreo visual en la siguiente revisión amplia o staging. |

### Estado De Fase 2.5

Completada para el pase visual humano privado:

- Registrados los resultados manuales concretos por ruta.
- Navegación activa privada validada visualmente en rutas principales.
- Logout y redirección sin sesión validados visualmente.
- `/dashboard` raíz confirmado como no ruta privada real.
- Sitio público confirmado sin regresión visible.
- Mecanismo recomendado para usuario QA limitado documentado y pendiente de implementación.
- Creado plan técnico documental para usuario limitado.
- No se implementaron funcionalidades nuevas.

### Validaciones Técnicas De Cierre Fase 2.5

- `npm run build` desde `src/LaboratorioTlahuac.Web`: correcto.
- `dotnet build`: correcto, 0 warnings y 0 errores.
- `dotnet test`: correcto; Domain 1/1, Application 1/1 y API 91/91.
- `git diff --check`: correcto.
- `rg "/dashboard" .`: revisado; no se detectó `/dashboard` como ruta privada real nueva.
- `rg "/app/dashboard" .`: revisado; confirma que el dashboard privado real se mantiene bajo `/app/dashboard`.
- `rg "/login" src/LaboratorioTlahuac.Web/src/app docs README.md AGENTS.md`: revisado; confirma `/login` como entrada pública y endpoints/rutas de auth existentes.
- `rg "routerLinkActive" src/LaboratorioTlahuac.Web/src/app/admin/layout`: revisado; confirma navegación activa por `RouterLinkActive`.
- `rg "America/Mexico_City" src docs tests README.md`: revisado; confirma configuración/código/documentación de zona horaria.
- `rg -F "Central Standard Time (Mexico)" src docs tests README.md`: revisado con búsqueda literal por paréntesis; confirma compatibilidad Windows.
- `rg --files-with-matches "LT_ADMIN_PASSWORD" .`: ejecutado con salida limitada a archivos para no imprimir valores.
- `rg --files-with-matches "LDT_SQL_SA_PASSWORD" .`: ejecutado con salida limitada a archivos para no imprimir valores.
- `rg --files-with-matches "ConnectionStrings" src docs README.md`: ejecutado con salida limitada a archivos para no imprimir valores.
- `rg "codex-cobranza-sql" docs README.md AGENTS.md`: revisado; las menciones corresponden a documentación de no uso o histórico.

### Siguiente Fase Recomendada

Implementar, si se autoriza tocar backend mínimo, el mecanismo Development-only para usuario QA limitado descrito en `docs/08-qa/limited-user-qa-plan.md` y validar `/app/access-denied` con una sesión real sin permisos suficientes.

## Fase 2.4 - Pase visual/manual privado y permisos

### Fecha

- Ejecución local: 2026-05-27 22:15 CST, America/Mexico_City.
- Sin commits.

### Entorno

- SQL dedicado: `ldt-labdental-sql`.
- Puerto SQL local: `14336 -> 1433/tcp`.
- Base local: `LaboratorioTlahuac_Dev`.
- API local: `http://localhost:5277`.
- Angular dev server: `http://localhost:4200`.
- Contenedor excluido: `codex-cobranza-sql`; no apareció activo y no se usó.
- Credenciales Admin: tomadas de variables de entorno locales, sin imprimir valores.
- Navegador/headless: no disponible sin instalar dependencias; `chromium`, `google-chrome`, `firefox` y `node_modules/.bin/playwright` no existen en este entorno.

### Validación De Navegación Activa

Rutas objetivo:

| Ruta | Resultado |
| --- | --- |
| `/app/dashboard` | Shell Angular `200`; validación visual pendiente por falta de navegador/headless. |
| `/app/clientes` | Shell Angular `200`; validación visual pendiente por falta de navegador/headless. |
| `/app/ordenes` | Shell Angular `200`; validación visual pendiente por falta de navegador/headless. |
| `/app/pagos` | Shell Angular `200`; validación visual pendiente por falta de navegador/headless. |
| `/app/inventario` | Shell Angular `200`; validación visual pendiente por falta de navegador/headless. |
| `/app/proveedores` | Shell Angular `200`; validación visual pendiente por falta de navegador/headless. |
| `/app/admin/usuarios` | Shell Angular `200`; validación visual pendiente por falta de navegador/headless. |
| `/app/admin/roles` | Shell Angular `200`; validación visual pendiente por falta de navegador/headless. |

Evidencia por código:

- `PrivateLayoutComponent` importa y usa `RouterLinkActive`.
- Todos los enlaces privados revisados tienen `routerLinkActive="is-active"` y `ariaCurrentWhenActive="page"`.
- `/app/dashboard` usa `[routerLinkActiveOptions]="{ exact: true }"`.
- Los estilos `.is-active`, `:hover` y `:focus-visible` existen en `private-layout.component.scss`.
- No se detectó cambio de rutas privadas ni conversión de `/dashboard` en ruta privada real.

Resultado: navegación activa validada por código/build y shell Angular. Queda pendiente pase visual humano en navegador real para confirmar foco visible, legibilidad y ausencia de regresión visual.

### Validación De Dashboard Y Zona Horaria

Resultado por código/pruebas:

- `Dashboard:BusinessTimeZone` mantiene default `America/Mexico_City`.
- `generatedAtUtc` se conserva en UTC; la API serializó `2026-05-28T04:15:10.3515775+00:00` y se parseó como UTC.
- `dueToday`, `overdue` y `upcomingDue` usan la fecha operativa del laboratorio.
- `DeliveryDate` no cambió de tipo ni significado; sigue siendo fecha de entrega capturada.
- La prueba `OperationalSummaryUsesBusinessTimeZoneDateWhenUtcDateDiffers` cubre el caso donde UTC y America/Mexico_City caen en fechas distintas.

Resultado manual por API local con Admin:

| Métrica | Antes | Después de orden QA | Delta |
| --- | ---: | ---: | ---: |
| `dueToday` | 1 | 2 | +1 |
| `upcomingDue` | 1 | 2 | +1 |
| `overdue` | 0 | 0 | 0 |

- Fecha operativa local usada para la orden QA: `2026-05-27`.
- La orden QA tuvo `DeliveryDate=2026-05-27`.
- Resultado esperado confirmado: una orden con entrega igual a la fecha operativa del laboratorio incrementa `dueToday`.

### Validación De Usuario Limitado / Access-Denied

No se creó usuario limitado local.

Motivo:

- El seed actual crea/asegura Admin; no provee un seed local configurable para usuario limitado.
- Las páginas `/app/admin/usuarios` y `/app/admin/roles` siguen como placeholders, sin CRUD seguro para crear usuario QA.
- Los endpoints de diagnóstico de seguridad en Development (`/api/security/permissions-check` y `/api/security/csrf-check`) no crean usuarios.
- Los usuarios limitados existentes están solo en fixtures de pruebas automatizadas con SQLite en memoria; no son mecanismo seguro para la base local real.
- No se autorizó crear usuario por SQL directo y no se alteraron permisos del Admin.

Evidencia conservada:

- Por código, `permissionGuard` redirige a `/app/access-denied` cuando `ensureSession()` confirma sesión pero falta el permiso requerido.
- Por pruebas API, usuarios autenticados sin permiso reciben `403`, incluyendo `SummaryWithoutReportsPermissionReturnsForbidden` para `/api/dashboard/summary`.

Resultado: validación manual de `/app/access-denied` queda pendiente hasta existir un mecanismo seguro de creación de usuario limitado o autorización explícita para preparar datos QA por SQL.

### Validación De Login/Logout Y Rutas Privadas

Resultado HTTP/API con Admin:

- `GET /api/auth/csrf` antes de login: `204`.
- `POST /api/auth/login`: `200`.
- `GET /api/auth/csrf` después de login: `204`.
- `GET /api/auth/me`: `200`, con 19 permisos.
- `GET /api/dashboard/summary`: `200`.
- `GET /api/customers`: `200`.
- `GET /api/work-orders`: `200`.
- `GET /api/payments`: `200`.
- `POST /api/auth/logout`: `200`.
- `GET /api/auth/me` después de logout: `401`.
- `GET /api/dashboard/summary` después de logout: `401`.

Confirmación por código:

- `/login` sigue público.
- `/app` sigue protegido por `authGuard`.
- `/app/dashboard` sigue protegido por `permissionGuard` y requiere `reports.view`.
- `/dashboard` no es ruta privada real; el wildcard conserva redirección a home pública.
- Usuario sin sesión en `/app/dashboard` debe ir a `/login?returnUrl=%2Fapp%2Fdashboard` por `authGuard`/`permissionGuard`.
- `returnUrl` externo o inválido sigue bloqueado por `getSafePrivateReturnUrl()`.

Limitación: la redirección visual real sin sesión y post-logout no se ejecutó en navegador porque no hay navegador/headless disponible sin instalar dependencias.

### Validación De Rutas Públicas

Todas respondieron con shell Angular `200` desde Angular dev server:

| Ruta | Resultado |
| --- | --- |
| `/` | `200` |
| `/servicios` | `200` |
| `/catalogo` | `200` |
| `/contacto` | `200` |
| `/login` | `200` |

No se modificó el sitio público.

### Datos QA Creados

Quedan en la base local `LaboratorioTlahuac_Dev`.

| Tipo | Identificador | Dato |
| --- | --- | --- |
| Cliente | `a5c48811-e171-450b-963e-f929a0d71084` | Nombre con prefijo `Cliente QA LDT F2.4`. |
| Orden | `53a35d65-a3ff-4f7d-ab7c-b0b2d658df44` | `OT-20260528-82F6A6`, `DeliveryDate=2026-05-27`, paciente con prefijo `Paciente QA LDT F2.4`. |

No se limpiaron datos QA.

### Hallazgos

#### Bloqueante

Ninguno.

#### Alto

Ninguno.

#### Medio

Ninguno.

#### Bajo

| Ruta | Hallazgo | Evidencia | Recomendación |
| --- | --- | --- | --- |
| `/app/access-denied` | No se pudo probar usuario limitado real por falta de mecanismo seguro de creación local. | Seed solo Admin, usuarios/roles placeholders, fixtures limitados solo en pruebas. | Crear mecanismo QA seguro o autorizar preparación SQL local documentada antes de requerir evidencia visual completa. |

#### Observación

| Ruta | Observación | Evidencia | Recomendación |
| --- | --- | --- | --- |
| `/app/*` | Pase visual de navegación activa queda pendiente por falta de navegador/headless disponible sin instalar dependencias. | `command -v` sin Chromium/Chrome/Firefox y sin Playwright local. | Ejecutar revisión humana en navegador real para confirmar activo, foco visible y consola/Network. |
| `/api/dashboard/summary` | `generatedAtUtc` se serializa con offset `+00:00`, no necesariamente con sufijo `Z`; sigue siendo UTC. | Respuesta local parseada a `2026-05-28T04:15:10.351Z`. | Mantener la validación por offset UTC, no por formato literal `Z`. |

### Pendientes Restantes

- Ejecutar pase visual humano en navegador real para navegación activa privada y foco visible.
- Probar `/app/access-denied` con usuario QA limitado cuando exista mecanismo seguro o autorización explícita para crear datos locales de seguridad.
- Revisar consola/Network en navegador real si se requiere evidencia visual adicional antes de staging.

### Validaciones Técnicas De Cierre

- `npm run build` desde `src/LaboratorioTlahuac.Web`: correcto.
- `dotnet build`: correcto, 0 warnings y 0 errores.
- `dotnet test`: correcto; Domain 1/1, Application 1/1 y API 91/91.
- `git diff --check`: correcto.
- `rg "/dashboard" .`: revisado; no se detectó `/dashboard` como ruta privada real nueva.
- `rg "/app/dashboard" .`: revisado; confirma que el dashboard privado real se mantiene bajo `/app/dashboard`.
- `rg "/login" src/LaboratorioTlahuac.Web/src/app docs README.md AGENTS.md`: revisado; confirma `/login` como entrada pública y endpoints/rutas de auth existentes.
- `rg "routerLinkActive" src/LaboratorioTlahuac.Web/src/app/admin/layout`: revisado; confirma `RouterLinkActive` en navegación privada.
- `rg "America/Mexico_City" src docs tests README.md`: revisado; confirma zona horaria operativa.
- `rg -F "Central Standard Time (Mexico)" src docs tests README.md`: revisado con búsqueda literal por paréntesis; confirma compatibilidad Windows documentada/codificada.
- `rg --files-with-matches "LT_ADMIN_PASSWORD" .`: ejecutado con salida limitada a archivos para no imprimir valores.
- `rg --files-with-matches "LDT_SQL_SA_PASSWORD" .`: ejecutado con salida limitada a archivos para no imprimir valores.
- `rg --files-with-matches "ConnectionStrings" src docs README.md`: ejecutado con salida limitada a archivos para no imprimir valores.
- `rg "codex-cobranza-sql" docs README.md AGENTS.md`: revisado; las menciones corresponden a documentación de no uso o histórico.

### Recomendación De Siguiente Fase

Fase 2.5 - cierre visual humano del sistema privado y definición de un mecanismo seguro para usuario QA limitado, antes de preparar staging o deploy.

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
- Seguimiento Fase 2.3: el "hoy" operativo del dashboard se calcula con `Dashboard:BusinessTimeZone`, default `America/Mexico_City`, convirtiendo `clock.UtcNow` a la fecha local de negocio.
- Métricas Fase 2.3 ajustadas: `dueToday`, `overdue` y `upcomingDue`.
- `generatedAtUtc` sigue reportándose en UTC y `DeliveryDate` no cambia de significado.
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
- Seguimiento Fase 2.3: los enlaces privados usan `routerLinkActive`, `ariaCurrentWhenActive` y estilos activos/focus visibles; `/app/dashboard` usa match exacto para evitar marcar rutas equivocadas.

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
| `/app/dashboard` | Metrica "Para hoy" requiere definicion de zona horaria de negocio. | En Mexico local `2026-05-27 21:xx CST`, crear orden QA con entrega `2026-05-27` y consultar dashboard. | Corregido en Fase 2.3: "hoy" usa `Dashboard:BusinessTimeZone` (`America/Mexico_City`) en vez de fecha UTC pura. | Las metricas de "hoy", vencidas y proximos dias deben alinearse con la zona horaria operativa del laboratorio. | Prueba `OperationalSummaryUsesBusinessTimeZoneDateWhenUtcDateDiffers` cubre UTC y Mexico City en fechas distintas. | Cerrado por código/prueba; pendiente solo pase visual/API manual si se requiere evidencia adicional. |

### Bajo

| Ruta | Hallazgo | Pasos | Actual | Esperado | Evidencia | Recomendacion |
| --- | --- | --- | --- | --- | --- | --- |
| `/app/*` | La navegacion privada no marca visualmente ruta activa. | Revisar `PrivateLayoutComponent`. | Corregido en Fase 2.3 con `routerLinkActive`, `ariaCurrentWhenActive`, match exacto para dashboard y estilos activos/focus visibles. | El usuario debe identificar rapidamente la seccion actual. | `private-layout.component.ts` y `private-layout.component.scss`; `npm run build` correcto. | Cerrado por código/build; pendiente validación visual manual en navegador real si se requiere evidencia adicional. |

### Observaciones

| Ruta | Observacion | Evidencia | Recomendacion |
| --- | --- | --- | --- |
| `/app/access-denied` | No se probo usuario autenticado sin permiso por falta de usuario limitado local. | `permissionGuard` redirige a `/app/access-denied` si hay sesion sin permiso. | Crear usuario QA limitado en una fase posterior si se requiere evidencia manual completa de permisos. |
| `/app/inventario`, `/app/proveedores`, `/app/admin/usuarios`, `/app/admin/roles` | Son paginas placeholder documentadas como pendientes o futuras. | Componentes muestran mensajes de modulo futuro/pendiente. | Mantener como backlog; no tratarlas como bug funcional del MVP actual. |
| Navegador | No se inspecciono consola/Network en navegador real o headless. | No hay Chromium/Chrome/Firefox/Playwright local sin instalar dependencias. | Ejecutar pase visual/manual en navegador real si se requiere evidencia de consola. |

## Pendientes

- Validar en navegador real la redireccion visual sin sesion a `/login?returnUrl=%2Fapp%2Fdashboard`.
- Probar usuario autenticado sin permiso contra `/app/access-denied`.
- Validar visualmente en navegador real los estilos activos de navegación privada si se requiere evidencia adicional.
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

## Validaciones Tecnicas De Seguimiento Fase 2.3

- `npm run build` desde `src/LaboratorioTlahuac.Web`: correcto.
- `dotnet build`: correcto, 0 warnings y 0 errores.
- `dotnet test`: correcto; Domain 1/1, Application 1/1 y API 91/91.
- `git diff --check`: correcto.
- `docker ps --filter "name=ldt-labdental-sql"` y `docker port ldt-labdental-sql`: SQL dedicado confirmado en `14336`; `docker port` requirió permiso fuera del sandbox.
- Búsquedas obligatorias de rutas, `routerLinkActive`, `America/Mexico_City`, variables sensibles, `ConnectionStrings` y `codex-cobranza-sql`: ejecutadas. Las búsquedas de patrones sensibles se limitaron a archivos para no imprimir valores.
- Prueba agregada: `OperationalSummaryUsesBusinessTimeZoneDateWhenUtcDateDiffers`.
- Frontend: no existe runner no interactivo ni patrón `.spec.ts`; la validación de navegación activa queda por código/build y pase visual manual si se requiere.

## Recomendacion De Siguiente Fase

Siguiente fase recomendada: Fase 2.5 - cierre visual humano del sistema privado y definición de mecanismo seguro para usuario QA limitado.

Motivo: los hallazgos principales de Fase 2.2 ya quedaron corregidos y Fase 2.4 validó dashboard/auth por API; queda pendiente evidencia visual real de navegación activa y usuario limitado para `/app/access-denied` antes de staging/deploy si se requiere cobertura completa.
