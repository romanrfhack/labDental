# Bitácora De Implementación

## Decisión De Registro

- `docs/IMPLEMENTATION_LOG.md` es la bitácora operativa de tareas ejecutadas por Codex.
- `docs/00-governance/changelog.md` se mantiene como changelog histórico de entregas relevantes.
- Cuando una tarea documental cambie fuentes canónicas, debe registrarse aquí y, si afecta entregables del proyecto, también en el changelog.

## 2026-05-28 - Fase 2.5 Cierre Visual Humano Privado Completado Y Usuario Limitado

### Cambio Realizado

Se cerró documentalmente Fase 2.5 como pase visual humano privado completado y se mantuvo el mecanismo seguro recomendado para usuario QA limitado como backlog técnico inmediato.

No se modificó código frontend/backend, `AuthService`, `auth.guard.ts`, `permission.guard.ts`, cookies, XSRF, endpoints, rutas privadas, base de datos, migraciones, deploy ni dependencias. No se hicieron commits.

### Resultado Visual Humano

El responsable del proyecto confirmó el pase visual/manual privado en navegador real.

Estado registrado en `docs/08-qa/private-admin-qa.md`:

- `/login`: OK.
- Login Admin: OK.
- `/app/dashboard`: OK.
- Navegación activa en `/app/dashboard`: OK.
- `/app/clientes`: OK.
- Navegación activa en `/app/clientes`: OK.
- `/app/ordenes`: OK.
- Navegación activa en `/app/ordenes`: OK.
- `/app/pagos`: OK.
- Navegación activa en `/app/pagos`: OK.
- `/app/inventario`: OK como placeholder.
- `/app/proveedores`: OK como placeholder.
- `/app/admin/usuarios`: OK como placeholder.
- `/app/admin/roles`: OK como placeholder.
- Logout: OK.
- `/app/dashboard` sin sesión redirige a `/login?returnUrl=%2Fapp%2Fdashboard`: OK.
- `/dashboard` raíz no es ruta privada real: OK.
- Sitio público sin regresión visible: OK.
- Observaciones visuales: sin bloqueantes visuales reportados.

### Usuario QA Limitado

Se evaluaron tres opciones:

- Seed QA limitado solo Development.
- Esperar módulo de usuarios/roles.
- Script local de QA.

Recomendación documentada: seed QA limitado solo Development, desactivado por default, controlado por user-secrets o variables de entorno, sin imprimir password, sin SQL manual, sin alterar Admin y sin activarse fuera de `Development`.

Plan creado: `docs/08-qa/limited-user-qa-plan.md`.

### Hallazgos

- Bloqueante: ninguno.
- Alto: ninguno.
- Medio: ninguno.
- Bajo: no se puede cerrar evidencia de `/app/access-denied` con usuario limitado real porque no existe mecanismo seguro local implementado.
- Observación: pase visual humano privado completado sin bloqueantes visuales reportados.

### Archivos Modificados

- `README.md`
- `docs/README.md`
- `docs/PROJECT_STATUS.md`
- `docs/ROADMAP.md`
- `docs/IMPLEMENTATION_LOG.md`
- `docs/01-product/internal-system.md`
- `docs/03-architecture/AUTH_FLOW.md`
- `docs/08-qa/RESPONSIVE_CHECKLIST.md`
- `docs/08-qa/private-admin-qa.md`

### Archivos Creados

- `docs/08-qa/limited-user-qa-plan.md`

### Validaciones De Cierre

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
- `rg "codex-cobranza-sql" docs README.md AGENTS.md`: revisado; solo aparecen menciones documentales o históricas de no uso.

### Siguiente Fase Recomendada

Implementar, si se autoriza tocar backend mínimo, el mecanismo QA limitado solo Development documentado en `docs/08-qa/limited-user-qa-plan.md` para validar `/app/access-denied`.

## 2026-05-27 - Fase 2.4 Pase Visual/Manual Privado Y Permisos

### Cambio Realizado

Se ejecutó la Fase 2.4 como pase manual/técnico del sistema privado para validar los cambios de Fase 2.3 y no se implementaron funcionalidades nuevas.

No se modificaron código frontend/backend, `AuthService`, `auth.guard.ts`, `permission.guard.ts`, cookies, XSRF, endpoints, rutas privadas, migraciones, deploy ni dependencias. No se hicieron commits.

### Entorno

- SQL dedicado: `ldt-labdental-sql`.
- Puerto SQL local: `14336 -> 1433/tcp`.
- Base local: `LaboratorioTlahuac_Dev`.
- API local: `http://localhost:5277`.
- Angular dev server: `http://localhost:4200`.
- `codex-cobranza-sql` no apareció activo y no se usó.
- Credenciales Admin tomadas de variables de entorno locales sin imprimir valores.
- No hay navegador/headless local disponible sin instalar dependencias.

### Validación Ejecutada

- Preflight Docker confirmó `ldt-labdental-sql` activo y puerto `14336`.
- `/health` respondió `200`.
- Rutas públicas `/`, `/servicios`, `/catalogo`, `/contacto` y `/login` respondieron con shell Angular `200`.
- Rutas privadas objetivo de navegación respondieron con shell Angular `200`; la ejecución real de guards/estado activo queda limitada por falta de navegador/headless.
- Login Admin por API: CSRF `204`, login `200`, `/api/auth/me` `200` con 19 permisos.
- Dashboard/listados con Admin: `/api/dashboard/summary`, `/api/customers`, `/api/work-orders` y `/api/payments` respondieron `200`.
- Logout Admin: `POST /api/auth/logout` `200`, `/api/auth/me` posterior `401` y `/api/dashboard/summary` posterior `401`.
- Dashboard zona horaria: una orden QA con `DeliveryDate=2026-05-27` incrementó `dueToday` de 1 a 2 y `upcomingDue` de 1 a 2 con fecha operativa `America/Mexico_City`.
- `generatedAtUtc` se confirmó en UTC con offset `+00:00`; `DeliveryDate` conserva su significado de fecha capturada.
- Navegación activa se validó por código: `RouterLinkActive`, `ariaCurrentWhenActive`, match exacto para `/app/dashboard` y estilos `.is-active`/`focus-visible`.
- Usuario limitado no se creó porque no existe mecanismo seguro local fuera de fixtures de pruebas y no se autorizó SQL directo.

### Datos QA Creados

Quedaron en la base local:

- Cliente `a5c48811-e171-450b-963e-f929a0d71084`, con nombre prefijado `Cliente QA LDT F2.4`.
- Orden `OT-20260528-82F6A6`, id `53a35d65-a3ff-4f7d-ab7c-b0b2d658df44`, `DeliveryDate=2026-05-27`.

No se limpiaron datos QA.

### Hallazgos

- Bloqueante: ninguno.
- Alto: ninguno.
- Medio: ninguno.
- Bajo: no se pudo probar `/app/access-denied` con usuario limitado real por falta de mecanismo seguro de creación local.
- Observación: el pase visual real de navegación activa queda pendiente por falta de navegador/headless disponible sin instalar dependencias.

### Archivos Modificados

- `docs/PROJECT_STATUS.md`
- `docs/ROADMAP.md`
- `docs/IMPLEMENTATION_LOG.md`
- `README.md`
- `docs/README.md`
- `docs/01-product/internal-system.md`
- `docs/03-architecture/AUTH_FLOW.md`
- `docs/08-qa/RESPONSIVE_CHECKLIST.md`
- `docs/08-qa/private-admin-qa.md`

### Validaciones De Cierre

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
- `rg "codex-cobranza-sql" docs README.md AGENTS.md`: revisado; solo aparecen menciones documentales o históricas de no uso.

### Siguiente Fase Recomendada

Fase 2.5 - cierre visual humano del sistema privado y definición de mecanismo seguro para usuario QA limitado.

## 2026-05-27 - Fase 2.3 Corrección De Hallazgos QA Del Sistema Privado

### Cambio Realizado

Se corrigieron los dos hallazgos registrados en Fase 2.2 para el sistema privado:

- Hallazgo medio: métricas operativas del dashboard calculaban "hoy" con fecha UTC pura.
- Hallazgo bajo: la navegación privada no marcaba visualmente la ruta activa.

No se modificaron sitio público, `AuthService`, `auth.guard.ts`, `permission.guard.ts`, cookies, XSRF, endpoints públicos, rutas privadas, migraciones, deploy ni dependencias. No se hicieron commits.

### Zona Horaria De Negocio

- Se agregó configuración `Dashboard:BusinessTimeZone` con default `America/Mexico_City`.
- `DashboardService` conserva `generatedAtUtc` en UTC, pero calcula el "hoy" operativo convirtiendo `clock.UtcNow` a la zona horaria de negocio.
- `dueToday`, `overdue` y `upcomingDue` usan la fecha operativa del laboratorio.
- `DeliveryDate` no cambió de significado ni de tipo.
- El ID canónico documentado es IANA `America/Mexico_City`; para compatibilidad Windows se acepta `Central Standard Time (Mexico)`.

### Navegación Privada

- `PrivateLayoutComponent` incorpora `RouterLinkActive`.
- `/app/dashboard` usa `routerLinkActiveOptions` con `exact: true`.
- Los enlaces privados conservan visibilidad condicional por permisos.
- Se agregaron estilos de activo, hover y `focus-visible` con contraste suficiente.
- No se cambiaron rutas, permisos ni logout.

### Pruebas

- Se agregó `OperationalSummaryUsesBusinessTimeZoneDateWhenUtcDateDiffers` en `DashboardIntegrationTests`.
- El caso fija `clock.UtcNow` en `2026-05-10T04:30:00Z`, cuando Mexico City sigue en fecha local `2026-05-09`.
- La prueba valida que una orden con entrega igual al día local cuenta como `dueToday` y que `overdue` y `upcomingDue` conservan comportamiento esperado.
- Frontend no tiene runner no interactivo ni patrón `.spec.ts`; la navegación privada se validó por código y `npm run build`.

### Archivos Modificados

- `README.md`
- `docs/PROJECT_STATUS.md`
- `docs/ROADMAP.md`
- `docs/IMPLEMENTATION_LOG.md`
- `docs/01-product/internal-system.md`
- `docs/03-architecture/ARCHITECTURE.md`
- `docs/08-qa/RESPONSIVE_CHECKLIST.md`
- `docs/08-qa/private-admin-qa.md`
- `src/LaboratorioTlahuac.Api/appsettings.json`
- `src/LaboratorioTlahuac.Infrastructure/Dashboard/DashboardOptions.cs`
- `src/LaboratorioTlahuac.Infrastructure/Dashboard/DashboardService.cs`
- `src/LaboratorioTlahuac.Infrastructure/Dashboard/DashboardTimeZoneResolver.cs`
- `src/LaboratorioTlahuac.Infrastructure/DependencyInjection.cs`
- `src/LaboratorioTlahuac.Web/src/app/admin/layout/private-layout.component.scss`
- `src/LaboratorioTlahuac.Web/src/app/admin/layout/private-layout.component.ts`
- `tests/LaboratorioTlahuac.Api.Tests/AuthIntegrationTests.cs`
- `tests/LaboratorioTlahuac.Api.Tests/DashboardIntegrationTests.cs`

### Validaciones Ejecutadas

- `npm run build` desde `src/LaboratorioTlahuac.Web`: correcto.
- `dotnet build`: correcto, 0 warnings y 0 errores.
- `dotnet test`: correcto tras corregir el fixture de pruebas; Domain 1/1, Application 1/1 y API 91/91.
- La primera ejecución de `dotnet test` falló porque `TestApplicationFactory` tenía dos constructores públicos; se ajustó a un solo constructor público y se repitió correctamente.
- `git diff --check`: correcto.
- `docker ps --filter "name=ldt-labdental-sql"`: confirmó `ldt-labdental-sql` activo en `14336`.
- `docker port ldt-labdental-sql`: confirmó `1433/tcp -> 0.0.0.0:14336` y `[::]:14336`; se requirió permiso fuera del sandbox.
- `rg "/dashboard" .`: revisado; no se detectó `/dashboard` como ruta privada real nueva.
- `rg "/app/dashboard" .`: revisado; confirma que el dashboard privado real se mantiene bajo `/app/dashboard`.
- `rg "/login" src/LaboratorioTlahuac.Web/src/app docs README.md AGENTS.md`: revisado; confirma `/login` como entrada pública y endpoints de auth existentes.
- `rg "routerLinkActive" src/LaboratorioTlahuac.Web/src/app/admin/layout`: revisado; confirma estado activo en navegación privada.
- `rg "America/Mexico_City" src docs tests README.md`: revisado; confirma configuración/código/documentación de zona horaria.
- `rg --files-with-matches "LT_ADMIN_PASSWORD" .`: ejecutado con salida limitada a archivos para no imprimir valores.
- `rg --files-with-matches "LDT_SQL_SA_PASSWORD" .`: ejecutado con salida limitada a archivos para no imprimir valores.
- `rg --files-with-matches "ConnectionStrings" src docs README.md`: ejecutado con salida limitada a archivos para no imprimir valores.
- `rg "codex-cobranza-sql" docs README.md AGENTS.md`: revisado; solo aparecen menciones documentales o históricas de no uso.

### Siguiente Fase Recomendada

Fase 2.4 - pase visual/manual privado y validación de permisos con usuario limitado si se requiere.

## 2026-05-27 - Fase 2.2 QA Manual/Técnico Del Sistema Privado Con Admin

### Cambio Realizado

Se ejecutó QA manual/técnico del sistema privado existente bajo `/app` con Admin local. No se implementaron funcionalidades nuevas, no se rediseñaron pantallas, no se modificó código frontend/backend, no se tocaron `AuthService`, guards, cookies, XSRF, endpoints, rutas privadas, migraciones, deploy ni dependencias, y no se hicieron commits.

Se creó el reporte `docs/08-qa/private-admin-qa.md` y se actualizaron las fuentes canónicas afectadas.

### Ambiente

- SQL dedicado: `ldt-labdental-sql`.
- Puerto SQL local: `14336 -> 1433/tcp`.
- Base local: `LaboratorioTlahuac_Dev`.
- API local: `http://localhost:5277`.
- Angular local: `http://localhost:4200`.
- `codex-cobranza-sql` no apareció activo y no se usó.
- `LT_ADMIN_EMAIL` y `LT_ADMIN_PASSWORD` se usaron desde variables de entorno sin imprimir valores.

### Resultado QA

- Rutas públicas `/`, `/servicios`, `/catalogo`, `/contacto` y `/login` respondieron con shell Angular `200`.
- Rutas privadas reales detectadas bajo `/app`: dashboard, clientes, órdenes, pagos, inventario, proveedores, usuarios, roles y access-denied.
- `/dashboard` raíz no existe como ruta privada real; el wildcard del router sigue enviando a la home pública.
- Sin sesión, endpoints privados respondieron `401`.
- Con Admin: login `200`, `/api/auth/me` `200` con 19 permisos, dashboard `200`, clientes `200`, órdenes `200`, pagos `200`, logout `200` y `/api/auth/me` posterior a logout `401`.
- `returnUrl` externo sigue bloqueado por código en `login-page.component.ts`; solo se aceptan rutas internas seguras bajo `/app`.
- Usuario sin permiso no se probó con cuenta limitada porque no existe usuario QA limitado disponible; por código, `permissionGuard` redirige a `/app/access-denied`.

### Datos De Prueba Creados

Quedaron en la base local:

- Cliente `Cliente QA LDT 20260527-210940 Editado`, id `fd5fe049-33e9-4732-80fa-790d140468f4`.
- Orden `OT-20260528-201A16`, id `967c2750-cbb4-4aec-908c-14a04fd120fb`.
- Pago `Pago QA LDT 20260527-210940`, id `561b6d36-6dff-4fe3-b08e-6705dc0947dd`.

No se limpiaron datos de prueba.

### Hallazgos

- Medio: la métrica "Para hoy" del dashboard requiere definir zona horaria de negocio; durante QA local en `CST -0600`, una orden con entrega en la fecha local de QA no incrementó `dueToday`.
- Bajo: la navegación privada no marca visualmente la ruta activa porque `PrivateLayoutComponent` no usa `routerLinkActive` ni clase equivalente.
- Observación: no se probó usuario autenticado sin permiso por falta de usuario limitado local.
- Observación: inventario, proveedores, usuarios y roles siguen como páginas placeholder documentadas.
- Observación: no hay navegador/headless local sin instalar dependencias, por lo que consola/Network y redirecciones visuales quedaron cubiertas por código/API.

### Archivos Modificados

- `docs/README.md`
- `docs/PROJECT_STATUS.md`
- `docs/ROADMAP.md`
- `docs/IMPLEMENTATION_LOG.md`
- `docs/01-product/internal-system.md`
- `docs/03-architecture/AUTH_FLOW.md`
- `docs/08-qa/RESPONSIVE_CHECKLIST.md`
- `docs/08-qa/private-admin-qa.md`

### Validaciones Ejecutadas

- `npm run build` desde `src/LaboratorioTlahuac.Web`: correcto.
- `dotnet build`: correcto, 0 warnings y 0 errores.
- `dotnet test`: correcto; Domain 1/1, Application 1/1 y API 90/90.
- `git diff --check`: correcto.
- `rg "/dashboard" .`: revisado; no se detectó `/dashboard` como ruta privada real nueva.
- `rg "/app/dashboard" .`: revisado; confirma que la ruta privada real se mantiene bajo `/app/dashboard`.
- `rg "/login" src/LaboratorioTlahuac.Web/src/app docs README.md AGENTS.md`: revisado; confirma `/login` como entrada pública y endpoints de auth existentes.
- `rg --files-with-matches "LT_ADMIN_PASSWORD" .`: ejecutado con salida limitada a archivos para no imprimir valores.
- `rg --files-with-matches "LDT_SQL_SA_PASSWORD" .`: ejecutado con salida limitada a archivos para no imprimir valores.
- `rg --files-with-matches "ConnectionStrings" src docs README.md`: ejecutado con salida limitada a archivos para no imprimir valores.
- `rg "codex-cobranza-sql" docs README.md AGENTS.md`: revisado; solo aparecen menciones documentales o históricas de no uso.

### Siguiente Fase Recomendada

Fase 2.3 - Corrección de hallazgos QA del sistema privado.

## 2026-05-27 - Cierre Documental De Fase 1.6 Y Fase 2.1d

### Cambio Realizado

Se cerraron documentalmente dos etapas con base en la validación manual confirmada por el responsable del proyecto:

- Fase 1.6 - Pulido visual premium del sitio público.
- Fase 2.1d - Diagnóstico/corrección de loading del dashboard autenticado.

No hubo cambios de código, estilos, frontend funcional, backend, `AuthService`, guards, cookies, XSRF, endpoints, base de datos, migraciones, deploy ni dependencias. No se instalaron paquetes y no se hicieron commits.

### Resultado Manual Registrado

- `/`, `/servicios`, `/catalogo`, `/contacto` y `/login` fueron revisados visualmente y aprobados.
- Breakpoints aprobados: 360px, 375px, 390px, 414px, 768px, 1024px y desktop.
- El sitio público queda mobile-first, sin scroll horizontal y sin problemas visuales bloqueantes reportados.
- El catálogo queda legible, con imágenes uniformes, precios correctos y placeholders intencionales.
- El enfoque CSS + `IntersectionObserver` queda aceptado; no se usó GSAP ni dependencia nueva.
- Reduced motion queda validado por implementación/código; no se reportaron hallazgos manuales bloqueantes.
- Login con Admin local, redirección a `/app/dashboard` y dashboard autenticado quedan validados manualmente.
- `/app/dashboard` ya no queda indefinidamente en `Cargando dashboard...`.
- Flujo autenticado validado manualmente; `GET /api/auth/me` autenticado no fue inspeccionado de forma independiente.
- `GET /api/dashboard/summary` autenticado queda validado indirectamente por la carga correcta del dashboard.
- Redirección posterior a logout o sesión cerrada validada: `/app/dashboard` redirige a `/login?returnUrl=%2Fapp%2Fdashboard`; logout como acción independiente queda para QA amplio si se requiere evidencia separada.

### Archivos Modificados

- `docs/PROJECT_STATUS.md`
- `docs/ROADMAP.md`
- `docs/IMPLEMENTATION_LOG.md`
- `docs/01-product/public-website.md`
- `docs/01-product/internal-system.md`
- `docs/02-domain/brand-guidelines.md`
- `docs/03-architecture/AUTH_FLOW.md`
- `docs/08-qa/RESPONSIVE_CHECKLIST.md`

### Validaciones Ejecutadas

- `git status --short` antes de editar: sin salida; working tree limpio.
- `git diff --stat` antes de editar: sin salida.
- `npm run build` desde `src/LaboratorioTlahuac.Web`: correcto.
- `dotnet build`: correcto, 0 warnings y 0 errores.
- `dotnet test`: correcto; Domain 1/1, Application 1/1 y API 90/90.
- `git diff --check`: correcto.
- `rg "/dashboard" .`: revisado; no muestra `/dashboard` como ruta privada real nueva.
- `rg "/app/dashboard" .`: revisado; confirma que la ruta privada real se mantiene bajo `/app/dashboard`.
- `rg "/login" src/LaboratorioTlahuac.Web/src/app docs README.md AGENTS.md`: revisado; confirma `/login` como entrada pública y endpoints de auth existentes.
- `rg "LT_ADMIN_PASSWORD" .`: ejecutado con salida limitada al patrón para no imprimir valores; solo se encontraron menciones del nombre de variable.
- `rg "LDT_SQL_SA_PASSWORD" .`: ejecutado con salida limitada al patrón para no imprimir valores; solo se encontraron menciones del nombre de variable.
- `rg "ConnectionStrings" src docs README.md`: ejecutado con salida limitada al patrón para no imprimir valores; solo se encontraron menciones de la clave de configuración.
- `rg "codex-cobranza-sql" docs README.md AGENTS.md`: revisado; solo aparecen menciones documentales de que no se usó.

### Confirmaciones

- `/login` sigue público.
- `/app` y `/app/dashboard` siguen privados.
- `/dashboard` no es ruta privada real.
- Fase 1.6 queda cerrada como validada visualmente.
- Fase 2.1d queda cerrada como validada manualmente.
- Siguiente fase recomendada: Fase 2.2 - QA manual del sistema privado con Admin.
- No se ejecutó `dotnet user-secrets list`.
- No se imprimieron secretos.
- No se usó `codex-cobranza-sql`.

## 2026-05-27 - Cierre Documental Parcial De Validación Visual Fase 1.6

### Cambio Realizado

Se actualizó documentación para registrar el reporte manual de revisión visual de Fase 1.6 del sitio público sin modificar código, instalar dependencias ni tocar backend/auth/guards/endpoints/base/deploy.

El cierre queda como parcialmente validado visualmente porque el reporte recibido conserva marcadores sin selección final ni observaciones concretas por ruta o breakpoint.

Nota posterior: este cierre parcial queda superado por el cierre documental completo registrado arriba el mismo 2026-05-27, basado en la confirmación manual final del responsable del proyecto.

### Resultado Manual Recibido

- Rutas reportadas como revisadas: `/`, `/servicios`, `/catalogo`, `/contacto` y `/login`.
- Viewports reportados como revisados: 360px, 375px, 390px, 414px, 768px, 1024px y desktop.
- Puntos adicionales reportados: reduced motion y scroll horizontal.
- Limitación documental: los puntos llegaron como `[correcto / observaciones]`, `[correcto / no probado / observaciones]` y `[no hay / observaciones]`, sin selección explícita ni observaciones.

### Archivos Modificados

- `docs/PROJECT_STATUS.md`
- `docs/IMPLEMENTATION_LOG.md`
- `docs/01-product/public-website.md`
- `docs/02-domain/brand-guidelines.md`
- `docs/08-qa/RESPONSIVE_CHECKLIST.md`

### Alcance No Tocado

- No se modificó código frontend ni backend.
- No se modificaron `AuthService`, guards, rutas, cookies, XSRF, endpoints, base de datos, migraciones, deploy ni dependencias.
- El working tree conserva cambios previos de código de Fase 1.6; este cierre documental modificó únicamente los cinco documentos listados.
- `/login` sigue documentado como público.
- `/app` y `/app/dashboard` siguen documentados como privados.
- `/dashboard` sigue documentado como no ruta privada real.

### Validaciones Ejecutadas

- `npm run build` desde `src/LaboratorioTlahuac.Web`: correcto.
- `dotnet build`: correcto, 0 warnings y 0 errores.
- `dotnet test`: correcto; Domain 1/1, Application 1/1 y API 90/90.
- `git diff --check`: correcto.
- Búsqueda de rutas: `/login` sigue fuera de `/app`; `/app` conserva `authGuard`; `/app/dashboard` conserva `permissionGuard` y `reports.view`; `/dashboard` no aparece como ruta privada real raíz.
- Búsqueda de secretos en los documentos tocados: solo aparecen nombres de variables, placeholders, textos redactados o menciones de `user-secrets`; no se detectaron valores reales de contraseña, tokens, API keys ni llaves privadas.

## 2026-05-27 - Fase 1.6 Pulido Visual Premium Del Sitio Público

### Cambio Realizado

Se implementó pulido visual premium del sitio público mobile-first con animaciones sutiles, composición más moderna, microinteracciones y mejoras de catálogo/contacto.

Enfoque elegido: CSS + `IntersectionObserver`. No se instaló GSAP ni otra dependencia porque los requerimientos de reveal, microinteracción y parallax ligero se cubren con APIs nativas, menor impacto de bundle y limpieza directa al destruir componentes Angular.

### Archivos Leídos

- `AGENTS.md`
- `README.md`
- `docs/PROJECT_STATUS.md`
- `docs/ROADMAP.md`
- `docs/01-product/public-website.md`
- `docs/02-domain/brand-guidelines.md`
- `docs/08-qa/RESPONSIVE_CHECKLIST.md`
- Componentes públicos de layout, home, servicios, catálogo y contacto.
- SCSS visual de `/login`.
- `src/LaboratorioTlahuac.Web/src/app/app.routes.ts`
- `src/LaboratorioTlahuac.Web/package.json`

### Archivos Modificados

- `src/LaboratorioTlahuac.Web/src/app/public/animations/public-scroll-animations.directive.ts`
- `src/LaboratorioTlahuac.Web/src/app/public/layout/public-layout.component.ts`
- `src/LaboratorioTlahuac.Web/src/app/public/layout/public-layout.component.scss`
- `src/LaboratorioTlahuac.Web/src/app/public/pages/home/home-page.component.ts`
- `src/LaboratorioTlahuac.Web/src/app/public/pages/home/home-page.component.scss`
- `src/LaboratorioTlahuac.Web/src/app/public/pages/services/services-page.component.ts`
- `src/LaboratorioTlahuac.Web/src/app/public/pages/catalog/catalog-page.component.ts`
- `src/LaboratorioTlahuac.Web/src/app/public/pages/catalog/catalog-page.component.scss`
- `src/LaboratorioTlahuac.Web/src/app/public/pages/contact/contact-page.component.ts`
- `src/LaboratorioTlahuac.Web/src/app/auth/pages/login/login-page.component.scss`
- `src/LaboratorioTlahuac.Web/src/styles.scss`
- `docs/PROJECT_STATUS.md`
- `docs/ROADMAP.md`
- `docs/IMPLEMENTATION_LOG.md`
- `docs/01-product/public-website.md`
- `docs/02-domain/brand-guidelines.md`
- `docs/08-qa/RESPONSIVE_CHECKLIST.md`

### Mejoras Visuales

- Header público con mejor presencia de logo, navegación con estado activo más claro y microinteracciones.
- Footer más visual, ordenado y con todos los teléfonos/correo confirmados.
- Home con hero institucional más cinematográfico, logo con profundidad, reveal de copy/CTAs, beneficios, proceso y contacto.
- Servicios con composición editorial, tarjetas numeradas y CTA claro al catálogo.
- Catálogo con encabezado premium, resumen visual, contacto/condiciones más claras, cards con frame uniforme, precios legibles y microinteracción de imagen.
- Contacto con cards que separan datos confirmados de pendientes sin inventar dirección, horarios ni WhatsApp.
- Login recibió solo pulido visual de SCSS; la lógica quedó intacta.

### Animación Y Accesibilidad

- La directiva pública observa elementos `data-animate` y `data-parallax`.
- `prefers-reduced-motion: reduce` desactiva reveal, parallax y transformaciones relevantes.
- Si `IntersectionObserver` no existe o JS falla antes de activar la directiva, el contenido permanece visible.
- Las animaciones usan `opacity` y `transform`; no animan propiedades de layout costosas.
- En catálogo, el reveal de productos se limita por lote inicial por sección.

### Alcance No Tocado

- No se modificó backend.
- No se modificaron `AuthService`, `auth.guard.ts`, `permission.guard.ts`, cookies, XSRF, endpoints, base de datos, migraciones, deploy ni contratos API.
- No se cambiaron rutas privadas.
- `/dashboard` no se convirtió en ruta privada real.
- No se instalaron dependencias.

### Validaciones Ejecutadas

- `npm run build` desde `src/LaboratorioTlahuac.Web`: correcto, sin warnings de presupuesto tras mover estilos públicos pesados a CSS global acotado.
- `dotnet build`: correcto, 0 warnings y 0 errores.
- `dotnet test`: correcto; Domain 1/1, Application 1/1 y API 90/90.
- `git diff --check`: correcto.
- `rg "/dashboard" .`: revisado; no muestra `/dashboard` como ruta privada real nueva, las menciones corresponden a documentación, API o `/app/dashboard`.
- `rg "/app/dashboard" .`: revisado; confirma que el dashboard privado real sigue bajo `/app`.
- `rg "/login" src/LaboratorioTlahuac.Web/src/app docs README.md AGENTS.md`: revisado; confirma `/login` como entrada pública y endpoints de auth existentes.
- `rg "prefers-reduced-motion" src/LaboratorioTlahuac.Web/src docs`: revisado; confirma soporte CSS/JS/documentación.
- `rg "gsap" src/LaboratorioTlahuac.Web src/LaboratorioTlahuac.Web/package.json docs`: sin resultados.
- Verificación de navegador/headless: no se encontró `chromium`, `google-chrome`, `firefox` ni Playwright local en `node_modules`; revisión visual real queda pendiente.

## 2026-05-23 - Fase 2.1d Diagnóstico Y Corrección Mínima De Dashboard

### Cambio Realizado

Se diagnosticó el estado `Cargando dashboard...` en `/app/dashboard` y se aplicó una corrección mínima en frontend para evitar carga indefinida cuando la consulta del resumen no termina.

No se modificaron `AuthService`, guards, rutas privadas, cookies, XSRF, backend, endpoints, permisos, seed, migraciones, deploy, dependencias ni `appsettings`.

### Archivos Leídos

- `AGENTS.md`
- `README.md`
- `docs/PROJECT_STATUS.md`
- `docs/03-architecture/AUTH_FLOW.md`
- `docs/03-architecture/ARCHITECTURE.md`
- `docs/01-product/internal-system.md`
- `docs/IMPLEMENTATION_LOG.md`
- `src/LaboratorioTlahuac.Web/src/app/app.routes.ts`
- `src/LaboratorioTlahuac.Web/src/app/core/guards/auth.guard.ts`
- `src/LaboratorioTlahuac.Web/src/app/core/guards/permission.guard.ts`
- `src/LaboratorioTlahuac.Web/src/app/core/auth/auth.service.ts`
- `src/LaboratorioTlahuac.Web/src/app/auth/pages/login/login-page.component.ts`
- `src/LaboratorioTlahuac.Web/src/app/features/dashboard/dashboard-page.component.ts`
- `src/LaboratorioTlahuac.Web/src/app/features/dashboard/dashboard.service.ts`
- `src/LaboratorioTlahuac.Api/Endpoints/DashboardEndpoints.cs`
- `src/LaboratorioTlahuac.Infrastructure/Dashboard/DashboardService.cs`
- `src/LaboratorioTlahuac.Domain/Security/Permissions.cs`

### Archivos Modificados

- `src/LaboratorioTlahuac.Web/src/app/features/dashboard/dashboard-page.component.ts`
- `docs/PROJECT_STATUS.md`
- `docs/IMPLEMENTATION_LOG.md`
- `docs/03-architecture/AUTH_FLOW.md`
- `docs/01-product/internal-system.md`
- `docs/08-qa/RESPONSIVE_CHECKLIST.md`

### Diagnóstico

- `/app/dashboard` sigue protegido por `permissionGuard` con `reports.view`.
- `GET /api/dashboard/summary` sigue protegido en backend con `Permissions.ReportsView`.
- El Admin seed recibe `reports.view` porque `SecuritySeeder` asigna todos los permisos de `Permissions.All`.
- El dashboard solo consulta `GET /api/dashboard/summary`.
- El componente ya apagaba `isLoading` con `finalize` para respuestas correctas o errores HTTP.
- La causa probable del estado persistente es una llamada pendiente a `GET /api/dashboard/summary`: sin timeout, el observable no completa ni falla y `isLoading` permanece activo.

### Corrección

- Se agregó timeout de 15 segundos a `DashboardPageComponent.load()`.
- Si `GET /api/dashboard/summary` tarda demasiado, el dashboard apaga `isLoading` y muestra un error controlado.
- No se cambió la estructura visual del dashboard ni se agregaron modulos.

### Endpoints Revisados

- `GET /health`: `200`.
- `GET /api/auth/csrf`: `204`.
- `GET /api/auth/me` sin sesión: `401`.
- `GET /api/dashboard/summary` sin sesión: `401`.
- `GET /api/auth/me` autenticado: pendiente porque `LT_ADMIN_EMAIL` y `LT_ADMIN_PASSWORD` no están disponibles en el proceso de Codex.
- `GET /api/dashboard/summary` autenticado: pendiente por la misma razón.
- Logout autenticado: pendiente por la misma razón.

### Ambiente

- Contenedor SQL usado/documentado: `ldt-labdental-sql`.
- Puerto SQL documentado: `14336 -> 1433/tcp`.
- No se usó `codex-cobranza-sql`.
- API y frontend estaban activos en `http://localhost:5277` y `http://localhost:4200`.
- No hay navegador/headless disponible sin instalar dependencias.

### Validaciones

- `npm run build` desde `src/LaboratorioTlahuac.Web`: correcto.
- `dotnet test`: correcto; Domain 1/1, Application 1/1 y API 90/90.
- `dotnet build`: primer intento falló por bloqueo temporal de `MvcTestingAppManifest.json` al ejecutarse en paralelo con `dotnet test`; repetido en serial, correcto con 0 warnings y 0 errores.
- `git diff --check`: correcto.
- Búsquedas obligatorias de rutas: `/dashboard`, `/app/dashboard` y `/login` revisadas; `/dashboard` no aparece como ruta privada real nueva.
- Búsquedas obligatorias de secretos: `LT_ADMIN_PASSWORD`, `LDT_SQL_SA_PASSWORD` y `ConnectionStrings` revisadas; solo aparecen nombres de variables, placeholders o cadenas locales/redactadas, no valores reales de contraseña.

### Seguridad

- No se ejecutó `dotnet user-secrets list`.
- No se imprimieron secretos.
- No se modificaron `appsettings*.json` con contraseñas.
- No se instalaron dependencias.

## 2026-05-23 - Fase 2.1c Cierre Parcial Por Validación Manual De Login

### Cambio Realizado

Se actualizó la documentación con la validación manual del login real usando el Admin local creado por seed. No se modificó código, backend, frontend, auth, guards, cookies, XSRF, endpoints, migraciones, deploy ni dependencias.

### Resultado Manual Reportado

- `/login` carga correctamente.
- Login con Admin local: validado.
- Redirección a `/app/dashboard`: validada.
- Dashboard: no validado; cargó una vez, pero al regresar a la página queda en `Cargando dashboard...`.
- `GET /api/auth/me` autenticado: no confirmado porque el resultado manual no fue marcado como `sí`.
- Logout: no confirmado como acción independiente porque el resultado manual no fue marcado.
- Después de logout, `/app/dashboard` redirige a `/login?returnUrl=%2Fapp%2Fdashboard`.

### Confirmaciones De Rutas

- `/login` sigue documentado como público.
- `/app` y `/app/dashboard` siguen documentadas como rutas privadas.
- `/dashboard` sigue documentado como no ruta privada real.

### Seguridad

- No se ejecutó `dotnet user-secrets list`.
- No se imprimieron secretos.
- No se usó `codex-cobranza-sql`.
- SQL correcto documentado: `ldt-labdental-sql` en puerto `14336`.
- No se modificaron `appsettings*.json` con contraseñas.

### Validaciones Técnicas

- `npm run build` desde `src/LaboratorioTlahuac.Web`: correcto.
- `dotnet build`: correcto, 0 warnings y 0 errores.
- `dotnet test`: correcto; Domain 1/1, Application 1/1 y API 90/90.
- `git diff --check`: correcto.

## 2026-05-23 - Fase 2.1c Validación SQL Dedicado, Seed Y Auth Anónima

### Cambio Realizado

Se validó el entorno local dedicado de Laboratorio Dental Tláhuac contra `ldt-labdental-sql` sin usar `codex-cobranza-sql`, sin listar user-secrets, sin imprimir secretos y sin modificar backend, frontend, auth, guards, endpoints, migraciones, deploy, dependencias ni `appsettings` con contraseñas.

Solo se actualizaron documentos de estado para registrar los resultados.

### Contenedor Y Base

- Contenedor usado: `ldt-labdental-sql`.
- Puerto usado: `14336`, mapeado a `1433/tcp`.
- Base validada por EF: `LaboratorioTlahuac_Dev`.
- `docker ps --filter "name=ldt-labdental-sql"` confirmó el contenedor activo.
- `docker port ldt-labdental-sql` confirmó el mapeo `1433/tcp -> 0.0.0.0:14336` y `1433/tcp -> [::]:14336`.

### Migraciones

- Proyecto EF: `src/LaboratorioTlahuac.Infrastructure/LaboratorioTlahuac.Infrastructure.csproj`.
- Startup project: `src/LaboratorioTlahuac.Api/LaboratorioTlahuac.Api.csproj`.
- `dotnet ef migrations list` listó:
  - `20260508044157_InitialSecurityModel`
  - `20260509004819_AddCustomersAndInternalDoctors`
  - `20260509022531_AddWorkOrders`
  - `20260509053231_AddPayments`
- `dotnet ef database update` terminó correctamente y reportó que no había migraciones pendientes.

### Seed Admin

- La API se levantó con `dotnet run --project src/LaboratorioTlahuac.Api/LaboratorioTlahuac.Api.csproj`.
- La ruta de seed se ejecutó al inicio porque `SecuritySeed:RunOnStartup` estaba activo en la configuración efectiva.
- La configuración Admin estuvo disponible para la API desde user-secrets; los logs solo mostraron consultas parametrizadas, no valores.
- Al terminar, se apagó el seed con `dotnet user-secrets set SecuritySeed:RunOnStartup false --project src/LaboratorioTlahuac.Api/LaboratorioTlahuac.Api.csproj`.

### API/Auth

- `GET /health`: `200`.
- `GET /api/auth/csrf`: `204`.
- `GET /api/auth/me` sin sesión: `401`.
- Login real: pendiente porque `LT_ADMIN_EMAIL` y `LT_ADMIN_PASSWORD` no están disponibles en el proceso de Codex.
- `/api/auth/me` autenticado: pendiente por la misma razón.
- Logout: pendiente por la misma razón.
- `/api/auth/me` después de logout: pendiente por la misma razón.

### Validaciones Ejecutadas

- `npm run build` desde `src/LaboratorioTlahuac.Web`: correcto.
- `dotnet build`: correcto, 0 warnings y 0 errores.
- `dotnet test`: correcto; Domain 1/1, Application 1/1 y API 90/90.
- `git diff --check`: correcto.

### Seguridad

- No se ejecutó `dotnet user-secrets list`.
- No se imprimieron secretos.
- No se extrajeron credenciales Admin desde user-secrets para simular login.
- No se modificaron `appsettings*.json` con contraseñas.
- La API local se apagó después de la validación.

## 2026-05-18 - Fase 2.1c Preflight SQL Server Docker Dedicado

### Cambio Realizado

Se ejecutó el preflight para crear o usar una instancia SQL Server Docker dedicada del proyecto Laboratorio Dental Tláhuac sin usar contenedores de otros proyectos y sin imprimir secretos.

La ejecución se detuvo antes de crear el contenedor porque `LDT_SQL_SA_PASSWORD` no está definida en el proceso. No se inventó password, no se guardaron secretos y no se modificaron backend, frontend, auth, guards, cookies, XSRF, endpoints, rutas, deploy, dependencias, appsettings ni migraciones.

### Archivos Leídos

- `AGENTS.md`
- `README.md`
- `docs/PROJECT_STATUS.md`
- `docs/03-architecture/AUTH_FLOW.md`
- `docs/03-architecture/ARCHITECTURE.md`
- `docs/01-product/internal-system.md`
- `docs/IMPLEMENTATION_LOG.md`
- `docs/08-qa/RESPONSIVE_CHECKLIST.md`

### Archivos Modificados

- `docs/PROJECT_STATUS.md`
- `docs/IMPLEMENTATION_LOG.md`
- `docs/03-architecture/AUTH_FLOW.md`
- `docs/01-product/internal-system.md`
- `docs/08-qa/RESPONSIVE_CHECKLIST.md`

### Preflight Del Repo

- `pwd`: `/home/romanrfhack/code/labDental`.
- `git rev-parse --show-toplevel`: `/home/romanrfhack/code/labDental`.
- `git status --short`: sin cambios iniciales.
- `git diff --stat`: sin cambios iniciales.

### Preflight Docker

- Docker está disponible.
- Contenedores activos detectados: `codex-cobranza-sql`, `mysql-ipn` y `n8n`.
- `codex-cobranza-sql` pertenece a otro proyecto y no se usó.
- No se usaron `facturacion-mysqlit`, `mercadosfmcpa-sql`, `bigsmile-sql`, `opticsoft-h1007-sql-0424` ni otros contenedores de otros proyectos.
- `ldt-labdental-sql` no existe en este entorno.
- Puertos revisados: `14336`, `14337` y `14338` no aparecen en escucha; el puerto preferido sigue siendo `14336`.
- No se ejecutó `docker inspect` completo para evitar exponer variables de entorno.
- No se borraron contenedores ni volúmenes.

### Bloqueo Seguro

- `LDT_SQL_SA_PASSWORD` no está definida.
- Por regla de seguridad, no se creó `ldt-labdental-sql`.
- No se creó el volumen `ldt-labdental-sql-data`.
- No se configuró `ConnectionStrings:DefaultConnection` en user-secrets.
- No se ejecutó `dotnet user-secrets list`.
- No se escribió ningún secreto en documentación ni en `appsettings`.

Comandos para que el humano prepare la variable en su terminal local antes de reintentar:

```bash
read -s -p "Password local para sa de SQL Server LDT: " LDT_SQL_SA_PASSWORD
echo
export LDT_SQL_SA_PASSWORD
```

### Admin Local

- `LT_ADMIN_EMAIL` no está definida.
- `LT_ADMIN_PASSWORD` no está definida.
- `LT_ADMIN_FULL_NAME` existe en el proceso, pero no se usó porque seed/login quedaron bloqueados antes de crear SQL Server.
- No se inventaron credenciales Admin.

Comandos para que el humano prepare Admin local antes de validar login real:

```bash
read -p "Admin email local: " LT_ADMIN_EMAIL
export LT_ADMIN_EMAIL
read -s -p "Admin password local: " LT_ADMIN_PASSWORD
echo
export LT_ADMIN_PASSWORD
export LT_ADMIN_FULL_NAME="Administrador Local"
```

### Migraciones Y Login Real

- `dotnet ef migrations list` no se ejecutó en esta fase porque no hay contenedor/base local dedicada disponible.
- `dotnet ef database update` no se ejecutó.
- Seed Admin no se ejecutó.
- Login real no se validó.
- `/api/auth/me` autenticado no se validó.
- Logout no se validó.
- `/app/dashboard` sin sesión no se validó en navegador en esta fase.

### Validaciones Ejecutadas

- `npm run build` desde `src/LaboratorioTlahuac.Web`: correcto.
- `dotnet build`: correcto, 0 warnings y 0 errores.
- `dotnet test`: correcto; Domain 1/1, Application 1/1 y API 90/90.
- `git diff --check`: correcto.
- `rg "/dashboard" .`: no muestra `/dashboard` como ruta privada real; las menciones corresponden a documentación, API de dashboard o `/app/dashboard`.
- `rg "/app/dashboard" .`: confirma que la ruta privada real se mantiene bajo `/app`.
- `rg "/login" src/LaboratorioTlahuac.Web/src/app docs README.md AGENTS.md`: confirma `/login` como entrada pública y endpoint de auth.
- `rg "LT_ADMIN_PASSWORD" .`: solo muestra nombres de variable, placeholders o código de seed; no muestra valores reales.
- `rg "LDT_SQL_SA_PASSWORD" .`: solo muestra nombres de variable y comandos de preparación; no muestra valores reales.
- `rg "ConnectionStrings" src docs README.md`: no muestra una connection string local con password real.
- Revisión adicional de patrones `Password=`, `MSSQL_SA_PASSWORD` y `User Id=sa`: solo placeholders o connection strings redactadas.

### Estado Esperado Al Reintentar

- Contenedor: `ldt-labdental-sql`.
- Imagen: `mcr.microsoft.com/mssql/server:2022-latest`.
- Puerto local preferido: `14336`.
- Volumen: `ldt-labdental-sql-data`.
- Base local: `LaboratorioTlahuac_Dev`.
- Connection string efectiva esperada en user-secrets, redactada: `Server=localhost,14336;Database=LaboratorioTlahuac_Dev;User Id=sa;Password=<redacted>;TrustServerCertificate=True;Encrypt=True`.

## 2026-05-15 - Fase 2.1 Preflight Local Admin Y Login Real

### Cambio Realizado

Se ejecutó el preflight de configuración local segura para validar login real contra API/base local sin modificar backend, frontend, `AuthService`, guards, cookies, XSRF, endpoints, migraciones, deploy, dependencias ni rutas privadas.

### Archivos Leídos

- `AGENTS.md`
- `README.md`
- `docs/03-architecture/AUTH_FLOW.md`
- `docs/03-architecture/ARCHITECTURE.md`
- `docs/PROJECT_STATUS.md`
- `docs/IMPLEMENTATION_LOG.md`
- `src/LaboratorioTlahuac.Api/appsettings.json`
- `src/LaboratorioTlahuac.Api/appsettings.Development.json`
- `src/LaboratorioTlahuac.Api/LaboratorioTlahuac.Api.csproj`
- `src/LaboratorioTlahuac.Infrastructure/Security/Seed/SecuritySeeder.cs`
- `src/LaboratorioTlahuac.Domain/Security/Permissions.cs`
- `src/LaboratorioTlahuac.Web/src/app/app.routes.ts`
- `docs/01-product/internal-system.md`
- `docs/08-qa/RESPONSIVE_CHECKLIST.md`

### Archivos Modificados

- `docs/PROJECT_STATUS.md`
- `docs/IMPLEMENTATION_LOG.md`
- `docs/03-architecture/AUTH_FLOW.md`
- `docs/01-product/internal-system.md`
- `docs/08-qa/RESPONSIVE_CHECKLIST.md`

### Preflight De Ambiente

- `git status --short`: sin cambios iniciales.
- `git diff --stat`: sin cambios iniciales.
- Connection string de desarrollo detectada: `Server=localhost;Database=LaboratorioTlahuac_Dev;Trusted_Connection=True;TrustServerCertificate=True`.
- La connection string apunta claramente a ambiente local por `localhost`; no se detectó conexión remota/productiva.
- `dotnet ef --version`: disponible, versión `10.0.7`.
- `dotnet ef migrations list` compiló correctamente y listó migraciones existentes, pero no pudo determinar estado aplicado porque SQL Server no estuvo accesible.
- Migraciones existentes: `InitialSecurityModel`, `AddCustomersAndInternalDoctors`, `AddWorkOrders`, `AddPayments`.

### Base Local Y Migraciones

- `dotnet ef database update` falló por no poder conectar a SQL Server en `localhost`.
- No se aplicaron migraciones.
- No se creó ni modificó base de datos.

Plantilla actualizada para preparar la base local con el contenedor dedicado de Fase 2.1c, sin guardar secretos en archivos versionados:

```bash
docker run --name ldt-labdental-sql -e "ACCEPT_EULA=Y" -e "MSSQL_PID=Developer" -e "MSSQL_SA_PASSWORD=$LDT_SQL_SA_PASSWORD" -p 14336:1433 -v ldt-labdental-sql-data:/var/opt/mssql -d mcr.microsoft.com/mssql/server:2022-latest
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "<connection-string-local-redacted>" --project src/LaboratorioTlahuac.Api/LaboratorioTlahuac.Api.csproj
dotnet ef database update --project src/LaboratorioTlahuac.Infrastructure/LaboratorioTlahuac.Infrastructure.csproj --startup-project src/LaboratorioTlahuac.Api/LaboratorioTlahuac.Api.csproj
```

Si ya existe `ldt-labdental-sql`, se debe iniciar ese contenedor dedicado y repetir `dotnet ef database update`. No usar contenedores de otros proyectos.

### Admin Local

- Variables de entorno revisadas sin imprimir valores: `LT_ADMIN_EMAIL`, `LT_ADMIN_PASSWORD` y `LT_ADMIN_FULL_NAME` no están definidas; `SecuritySeed__RunOnStartup` no está en `true`.
- No existe archivo de user-secrets para `laboratorio-tlahuac-api-dev` en este entorno.
- No se ejecutó seed Admin porque faltan credenciales locales seguras y la base local no está accesible.
- No se inventaron credenciales, no se imprimieron passwords y no se documentó ningún secreto real.

Comandos exactos para configurar Admin local con user-secrets:

```bash
dotnet user-secrets set LT_ADMIN_EMAIL "<email-local>" --project src/LaboratorioTlahuac.Api/LaboratorioTlahuac.Api.csproj
dotnet user-secrets set LT_ADMIN_PASSWORD "<password-local-seguro>" --project src/LaboratorioTlahuac.Api/LaboratorioTlahuac.Api.csproj
dotnet user-secrets set LT_ADMIN_FULL_NAME "Administrador" --project src/LaboratorioTlahuac.Api/LaboratorioTlahuac.Api.csproj
dotnet user-secrets set SecuritySeed:RunOnStartup true --project src/LaboratorioTlahuac.Api/LaboratorioTlahuac.Api.csproj
```

Después de crear el Admin, se recomienda apagar el seed local:

```bash
dotnet user-secrets set SecuritySeed:RunOnStartup false --project src/LaboratorioTlahuac.Api/LaboratorioTlahuac.Api.csproj
```

### Validación Ejecutada

- API levantada con `dotnet run --project src/LaboratorioTlahuac.Api/LaboratorioTlahuac.Api.csproj`.
- `curl -s http://localhost:5277/health`: respondió `{"status":"Healthy","application":"LaboratorioTlahuac.Api"}`.
- Angular levantado desde `src/LaboratorioTlahuac.Web` con `npm start` en `http://localhost:4200/`.
- `curl -s http://localhost:4200/login`: respondió shell Angular.
- `GET /api/auth/csrf` con cookie jar temporal: `204`.
- `GET /api/auth/me` sin sesión: `401`.
- `npm run build`: correcto.
- `dotnet build`: correcto, 0 warnings y 0 errores.
- `dotnet test`: correcto después de repetirlo en serial; Domain 1/1, Application 1/1 y API 90/90.

### Validación Bloqueada

- Login real desde `/login`: pendiente por falta de SQL Server local accesible y Admin local configurado.
- Redirección post-login a `/app/dashboard`: pendiente.
- `/api/auth/me` autenticado: pendiente.
- Logout autenticado: pendiente.
- Redirección de `/app/dashboard` sin sesión tras logout a `/login?returnUrl=%2Fapp%2Fdashboard`: pendiente de navegador con sesión real.
- Validación con usuario sin `reports.view`: pendiente porque no hay base local con usuarios de prueba.

### Permisos Confirmados Por Código

- `SecuritySeeder` asigna al rol Admin todos los permisos de `Permissions.All`.
- `Permissions.All` incluye `reports.view`.
- `/app/dashboard` tiene `permissionGuard` con `data: { permission: 'reports.view' }`.

### Pendientes Para El Humano

1. Levantar SQL Server local o contenedor local y configurar la connection string por user-secrets si se usa usuario/contraseña.
2. Aplicar migraciones con `dotnet ef database update`.
3. Configurar Admin local con user-secrets o variables de entorno.
4. Arrancar API con `SecuritySeed:RunOnStartup=true` una vez para crear/actualizar Admin.
5. Apagar `SecuritySeed:RunOnStartup` en user-secrets cuando ya no se necesite.
6. Validar login real en navegador y con `curl` sin imprimir contraseña.

## 2026-05-15 - Fase 2.0 Validación Login, Sesión Y Redirección

### Cambio Realizado

Se validó el flujo técnico de entrada desde el sitio público hacia la app privada sin rediseñar pantallas, sin implementar módulos nuevos y sin tocar backend, guards, `AuthService`, cookies, XSRF, endpoints, base de datos, migraciones, deploy, dependencias ni rutas privadas.

### Archivos Leídos

- `AGENTS.md`
- `README.md`
- `docs/PROJECT_STATUS.md`
- `docs/03-architecture/AUTH_FLOW.md`
- `docs/03-architecture/ARCHITECTURE.md`
- `docs/01-product/public-website.md`
- `docs/01-product/internal-system.md`
- `docs/08-qa/RESPONSIVE_CHECKLIST.md`
- `docs/ROADMAP.md`
- `src/LaboratorioTlahuac.Web/src/app/app.routes.ts`
- `src/LaboratorioTlahuac.Web/src/app/auth/pages/login/login-page.component.ts`
- `src/LaboratorioTlahuac.Web/src/app/core/auth/auth.service.ts`
- `src/LaboratorioTlahuac.Web/src/app/core/guards/auth.guard.ts`
- `src/LaboratorioTlahuac.Web/src/app/core/guards/permission.guard.ts`
- `src/LaboratorioTlahuac.Web/src/environments/environment.ts`
- `src/LaboratorioTlahuac.Web/src/environments/environment.development.ts`
- `src/LaboratorioTlahuac.Api/appsettings.json`
- `src/LaboratorioTlahuac.Api/appsettings.Development.json`
- `src/LaboratorioTlahuac.Api/Program.cs`
- `src/LaboratorioTlahuac.Api/Endpoints/AuthEndpoints.cs`

### Archivos Modificados

- `docs/PROJECT_STATUS.md`
- `docs/ROADMAP.md`
- `docs/IMPLEMENTATION_LOG.md`
- `docs/03-architecture/AUTH_FLOW.md`
- `docs/01-product/internal-system.md`
- `docs/01-product/public-website.md`
- `docs/08-qa/RESPONSIVE_CHECKLIST.md`

### Resultados De Validación

- `/login` sigue público en `app.routes.ts`.
- `/app` sigue bajo `PrivateLayoutComponent` con `authGuard`.
- `/app/dashboard` sigue bajo `/app`, con `permissionGuard` y permiso `reports.view`.
- `/dashboard` no existe como ruta privada real; el wildcard del router redirige a la home pública.
- `AuthService.login()` sigue solicitando CSRF, ejecutando `POST /api/auth/login` con `withCredentials`, renovando CSRF y guardando usuario en memoria.
- `login-page.component.ts` conserva manejo de error `423` e inválidos, usa `AuthService.login()` y navega con `router.navigateByUrl(this.getReturnUrl())`.
- `returnUrl` acepta `/app`, `/app/...`, `/app?...` y `/app#...`.
- `returnUrl` rechaza valores externos o inválidos como `https://example.com`, `//example.com`, `javascript:alert(1)`, valores con espacios, backslash o rutas fuera de `/app`; el fallback es `/app/dashboard`.
- Usuario sin sesión en `/app/*` se redirige por guards a `/login?returnUrl=...`.
- Usuario autenticado sin permiso se redirige por `permissionGuard` a `/app/access-denied`; no se trata como usuario sin sesión.

### Validaciones Ejecutadas

- `git status --short`: sin cambios iniciales.
- `git diff --stat`: sin cambios iniciales.
- `npm run build` desde `src/LaboratorioTlahuac.Web`: correcto.
- `dotnet build` desde la raíz: correcto, 0 warnings y 0 errores.
- `dotnet test` desde la raíz: correcto; Domain 1/1, Application 1/1 y API 90/90.
- `git diff --check`: correcto.
- Angular dev server en `http://127.0.0.1:4201/` porque el puerto 4200 ya estaba ocupado.
- `curl` contra `http://127.0.0.1:4201/`, `/servicios`, `/catalogo`, `/contacto`, `/login`, `/app`, `/app/dashboard`, `/dashboard`, `/login?returnUrl=%2Fapp%2Fdashboard`, `/login?returnUrl=https://example.com`, `/login?returnUrl=//example.com` y `/login?returnUrl=javascript:alert(1)`: todos respondieron con shell Angular `200`.

### Pendiente De Login Real

No se validó login real con credenciales porque el entorno local no tiene Admin configurado en `appsettings*.json`: `SecuritySeed:RunOnStartup` está en `false` y `SecuritySeed:Admin` está vacío. No se inventaron credenciales, no se modificó seed y no se tocó base de datos.

Pasos exactos para validación humana:

1. Configurar API/base local y usuario Admin por los mecanismos seguros del proyecto.
2. Levantar API con `dotnet run --project src/LaboratorioTlahuac.Api/LaboratorioTlahuac.Api.csproj`.
3. Levantar Angular desde `src/LaboratorioTlahuac.Web` con `npm start`.
4. Abrir `/login`, iniciar sesión con el Admin local configurado y confirmar redirección a `/app/dashboard`.
5. Confirmar que `GET /api/auth/me` responde el usuario autenticado.
6. Ejecutar logout desde la UI si está disponible.
7. Confirmar que después de logout `/app/dashboard` vuelve a redirigir a `/login?returnUrl=%2Fapp%2Fdashboard`.

## 2026-05-15 - Fase 1.5 Identidad Visual Y Contacto

### Cambio Realizado

Se incorporó identidad visual real del laboratorio en el sitio público: logo LDT, colores institucionales y datos de contacto tomados del cartel/catálogo.

### Archivos Creados

- `docs/02-domain/brand-guidelines.md`

### Asset Incorporado

- `src/LaboratorioTlahuac.Web/src/assets/brand/logo-ldt.webp`
- Ruta pública esperada: `/assets/brand/logo-ldt.webp`

### Archivos Modificados

- `src/LaboratorioTlahuac.Web/src/app/public/layout/public-layout.component.ts`
- `src/LaboratorioTlahuac.Web/src/app/public/layout/public-layout.component.scss`
- `src/LaboratorioTlahuac.Web/src/app/public/pages/home/home-page.component.ts`
- `src/LaboratorioTlahuac.Web/src/app/public/pages/home/home-page.component.scss`
- `src/LaboratorioTlahuac.Web/src/app/public/pages/services/services-page.component.ts`
- `src/LaboratorioTlahuac.Web/src/app/public/pages/catalog/catalog-page.component.ts`
- `src/LaboratorioTlahuac.Web/src/app/public/pages/catalog/catalog-page.component.scss`
- `src/LaboratorioTlahuac.Web/src/app/public/pages/contact/contact-page.component.ts`
- `src/LaboratorioTlahuac.Web/src/app/auth/pages/login/login-page.component.ts`
- `src/LaboratorioTlahuac.Web/src/app/auth/pages/login/login-page.component.scss`
- `docs/PROJECT_STATUS.md`
- `docs/ROADMAP.md`
- `docs/IMPLEMENTATION_LOG.md`
- `docs/01-product/public-website.md`
- `docs/08-qa/RESPONSIVE_CHECKLIST.md`
- `docs/03-architecture/ARCHITECTURE.md`
- `docs/README.md`

### Identidad Y Contacto

- Tokens aplicados: `--ldt-navy`, `--ldt-navy-soft`, `--ldt-blue`, `--ldt-blue-dark`, `--ldt-sky`, `--ldt-sky-light`, `--ldt-gray` y `--ldt-white`.
- Eslogan incorporado: `Precisión • Estética • Confianza`.
- Línea descriptiva incorporada: `Prótesis, restauraciones y soluciones dentales`.
- Teléfonos incorporados como `tel:`: 55 3331 9445, 55 2161 2311 y 55 9802 9816.
- Correo incorporado como `mailto:`: `contacto@laboratoriodentaltlahuac.com`.
- Condiciones visibles en cartel documentadas con prudencia: `Anticipo 50%` y `Trabajos urgentes +40%` requieren confirmación final del cliente.

### Alcance

- No se modificó backend.
- No se modificó `AuthService`.
- No se modificaron `auth.guard.ts` ni `permission.guard.ts`.
- No se modificaron cookies, XSRF, endpoints, base de datos, migraciones, deploy ni dependencias.
- No se modificaron rutas privadas.
- `/login` sigue como entrada pública.
- `/app` y `/app/dashboard` siguen como zona privada.
- `/dashboard` no se creó como ruta privada real.
- No se inventó dirección, horario, WhatsApp, redes sociales ni mapa.

### Validaciones Ejecutadas

- `npm run build` desde `src/LaboratorioTlahuac.Web`: correcto.
- `git diff --check`: correcto.
- `rg "logo-ldt" src/LaboratorioTlahuac.Web/src docs`
- `rg "55 3331 9445" .`
- `rg "55 2161 2311" .`
- `rg "55 9802 9816" .`
- `rg "contacto@laboratoriodentaltlahuac.com" .`
- `rg "WhatsApp" src/LaboratorioTlahuac.Web/src/app/public docs/01-product/public-website.md`
- `rg "/dashboard" .`
- `rg "/app/dashboard" .`
- `rg "/login" src/LaboratorioTlahuac.Web/src/app docs README.md AGENTS.md`

### Pendientes

- Revisión visual real en 360px, 375px, 390px, 414px, 768px, 1024px y desktop.
- Confirmar con el cliente si algún teléfono debe publicarse como WhatsApp.
- Confirmar dirección, horarios y mapa antes de publicarlos.
- Aprobar precios 2026 y condiciones comerciales antes de publicación formal.

## 2026-05-15 - Backlog Futuro Administración De Catálogo

### Cambio Realizado

Se documentó como backlog futuro la funcionalidad `Administración de catálogo, precios e imágenes`.

### Archivos Creados

- `docs/01-product/admin-catalog-management.md`

### Archivos Modificados

- `docs/ROADMAP.md`
- `docs/PROJECT_STATUS.md`
- `docs/IMPLEMENTATION_LOG.md`
- `docs/01-product/public-website.md`
- `docs/01-product/internal-system.md`
- `docs/README.md`

### Alcance Documentado

- No pertenece a la fase actual.
- No se implementa todavía.
- Será una futura mejora de la app privada bajo `/app`.
- Requerirá permisos administrativos, con permiso sugerido `catalog.manage` o equivalente.
- Requerirá definir modelo de datos, endpoints, almacenamiento de imágenes, reglas de publicación y aprobación de precios públicos.
- El catálogo público actual seguirá funcionando desde `catalog-data.ts` hasta que se diseñe y apruebe esta fase.

### Alcance No Ejecutado

- No se implementaron pantallas.
- No se crearon rutas.
- No se tocó backend.
- No se tocó frontend funcional.
- No se tocó auth.
- No se tocaron guards.
- No se tocó base de datos.
- No se crearon migraciones.
- No se crearon endpoints.
- No se instalaron dependencias.
- No se cambió deploy.
- No se modificó el catálogo público actual.

## 2026-05-14 - Ignore De Zone.Identifier

### Cambio Realizado

Se agregó `*:Zone.Identifier` a `.gitignore` para evitar que vuelvan a entrar al control de versiones archivos alternos generados al copiar assets desde Windows.

### Archivos Modificados

- `.gitignore`
- `docs/IMPLEMENTATION_LOG.md`

### Alcance

- No se modificó código.
- No se modificó documentación fuera de esta bitácora.

## 2026-05-14 - Fase 1.3.1 Cierre De Catálogo Público

### Cambio Realizado

Se cerró la revisión técnica del catálogo público en `/catalogo`, se retiraron del working tree los assets `:Zone.Identifier` detectados en la carpeta de productos y se preparó la documentación para revisión visual/comercial del cliente.

### Archivos Modificados

- `src/LaboratorioTlahuac.Web/src/app/public/pages/catalog/catalog-page.component.ts`
- `docs/PROJECT_STATUS.md`
- `docs/ROADMAP.md`
- `docs/IMPLEMENTATION_LOG.md`
- `docs/01-product/public-website.md`
- `docs/08-qa/RESPONSIVE_CHECKLIST.md`

### Archivos Eliminados

- 22 archivos `*:Zone.Identifier` dentro de `src/LaboratorioTlahuac.Web/src/assets/catalog/products/`.
- No se borraron imágenes `.webp`.
- No se borró `protesis-removible-unidad-acrilica.jpg`.

### Validación Del Catálogo

- `/catalogo` está configurado como ruta pública bajo `PublicLayoutComponent`.
- `/servicios` enlaza a `/catalogo`.
- `/login` sigue como ruta pública de entrada al sistema.
- `/app` y `/app/dashboard` siguen bajo layout privado con guards.
- `/dashboard` no existe como ruta privada real; las menciones restantes son documentación, API de dashboard o `/app/dashboard`.
- `catalog-data.ts` contiene 12 secciones y 40 productos.
- Los precios permanecen como números y se formatean con `Intl.NumberFormat('es-MX')`.
- Hay 19 productos con imagen específica, 16 con imagen representativa de sección y 5 placeholders.
- Placeholders restantes: Reparación de dentadura por fractura, Gancho volado, Descanso metálico c/u, Rebase y Aumentar dientes c/u.
- Todas las imágenes referenciadas por el catálogo existen en `src/LaboratorioTlahuac.Web/src/assets/catalog/products/`.

### Copy Comercial

- Se agregó la nota visible `Precios de referencia 2026 sujetos a confirmación.`.
- Los precios provienen del cartel proporcionado y requieren aprobación final del cliente antes de publicación formal.
- No se agregaron condiciones comerciales nuevas.

### Configuración Y Assets

- `angular.json` no se modificó.
- La configuración actual copia `src/assets/**/*.webp` hacia `assets`, suficiente para el catálogo actual.
- Ese glob no copia archivos `:Zone.Identifier` desde `src/assets`.
- `find . -name '*:Zone.Identifier' -type f -print`: sin resultados.
- `rg "Zone.Identifier" .`: solo devuelve menciones documentales, no archivos físicos.

### Validaciones Ejecutadas

- `npm run build` desde `src/LaboratorioTlahuac.Web`: correcto.
- `git diff --check`: correcto.
- `rg "Zone.Identifier" .`
- `rg "/catalogo" src/LaboratorioTlahuac.Web/src/app docs README.md AGENTS.md`
- `rg "/dashboard" .`
- `rg "/app/dashboard" .`
- `rg "/login" src/LaboratorioTlahuac.Web/src/app docs README.md AGENTS.md`
- `git status --short`: muestra las bajas esperadas de `*:Zone.Identifier` y cambios documentales/frontend de esta fase.

### Pendientes

- Revisión visual real de `/catalogo` y rutas públicas en 360px, 375px, 390px, 414px, 768px, 1024px y desktop.
- Aprobación final del cliente sobre precios 2026, vigencia, condiciones comerciales y publicación.
- Reemplazar placeholders y fallbacks por imágenes `.webp` específicas cuando el cliente entregue o apruebe assets.

## 2026-05-13 - Fase 1.3 Catálogo Público

### Cambio Realizado

Se implementó un catálogo público mobile-first con secciones, productos, precios e imágenes locales. La ruta elegida fue `/catalogo` para mantener una página dedicada al volumen del catálogo y conservar `/servicios` como vista introductoria.

### Archivos Creados

- `src/LaboratorioTlahuac.Web/src/app/public/data/catalog-data.ts`
- `src/LaboratorioTlahuac.Web/src/app/public/pages/catalog/catalog-page.component.ts`
- `src/LaboratorioTlahuac.Web/src/app/public/pages/catalog/catalog-page.component.scss`

### Archivos Modificados

- `src/LaboratorioTlahuac.Web/angular.json`
- `src/LaboratorioTlahuac.Web/src/app/app.routes.ts`
- `src/LaboratorioTlahuac.Web/src/app/public/layout/public-layout.component.ts`
- `src/LaboratorioTlahuac.Web/src/app/public/layout/public-layout.component.scss`
- `src/LaboratorioTlahuac.Web/src/app/public/pages/home/home-page.component.ts`
- `src/LaboratorioTlahuac.Web/src/app/public/pages/services/services-page.component.ts`
- `README.md`
- `docs/PROJECT_STATUS.md`
- `docs/ROADMAP.md`
- `docs/IMPLEMENTATION_LOG.md`
- `docs/01-product/public-website.md`
- `docs/03-architecture/ARCHITECTURE.md`
- `docs/08-qa/RESPONSIVE_CHECKLIST.md`

### Estructura Del Catálogo

- Data tipada: `src/LaboratorioTlahuac.Web/src/app/public/data/catalog-data.ts`.
- Interfaces: `CatalogSection` y `CatalogProduct`.
- Imágenes fuente: `src/LaboratorioTlahuac.Web/src/assets/catalog/products/`.
- Ruta pública de imágenes en Angular: `/assets/catalog/products/...`.
- Se agregó `src/assets/**/*.webp` como asset del frontend en `angular.json`.
- No se copian archivos `Zone.Identifier` ni imágenes `.jpg`; el catálogo usa `.webp`.

### Manejo De Imágenes

- Imagen específica del producto si existe.
- Imagen representativa de sección si falta la específica.
- Placeholder visual con iniciales si no hay imagen de producto ni de sección.
- Todas las imágenes usan frame con `aspect-ratio: 4 / 3`, `object-fit: contain`, fondo claro y centrado.

### Imágenes Faltantes O Con Fallback

- Usan imagen de sección: carillas/incrustaciones sin imagen propia, productos sin foto exacta dentro de Zirconia, E-MAX, SIGNUM, Metal-porcelana, Metálicos, Totally Natural, iFlex, Prótesis removible y Prótesis inmediata.
- Usan placeholder: productos de `Servicios prostodónticos`, porque no hay imagen de sección ni producto.
- `protesis-removible-unidad-acrilica.jpg` existe localmente, pero no se usa porque esta fase definió `.webp`.

### Alcance

- No se modificaron backend, `AuthService`, guards, cookies, XSRF, endpoints, base de datos, deploy, dependencias ni rutas privadas.
- `/login` sigue como entrada pública.
- `/app` y `/app/dashboard` siguen siendo zona privada.
- `/dashboard` no se creó como ruta real.

### Validaciones Ejecutadas

- `npm run build` desde `src/LaboratorioTlahuac.Web`: correcto.
- `git diff --check`: correcto.
- `curl -i -s http://127.0.0.1:4200/catalogo`: responde `200 OK` con shell Angular.
- `rg "/dashboard" .`: no muestra `/dashboard` como ruta privada real; las menciones corresponden a documentación, API de dashboard o `/app/dashboard`.
- `rg "/app/dashboard" .`: confirma que la ruta privada real se mantiene bajo `/app`.
- `rg "/login" src/LaboratorioTlahuac.Web/src/app docs README.md AGENTS.md`: confirma `/login` como entrada pública.
- Verificación de assets generados: se copian imágenes `.webp` del catálogo a `dist/laboratorio-tlahuac-web/browser/assets/catalog/products/`.
- No se ejecutó lint porque `src/LaboratorioTlahuac.Web/package.json` no define script `lint`.
- No se ejecutó `dotnet build` ni `dotnet test` porque no se modificó backend ni configuración compartida.

### Pendientes Generados

- Confirmar vigencia de precios con el cliente antes de publicación formal.
- Completar imágenes `.webp` específicas para productos que hoy usan imagen de sección o placeholder.
- Revisar visualmente `/catalogo` en 360px, 375px, 390px, 414px, 768px, 1024px y desktop.

## 2026-05-13 - Fase 1.2 Contenido Público Seguro

### Cambio Realizado

Se ejecutó Fase 1.2 de forma parcial porque no se recibieron datos reales confirmados del cliente. Se pulió el copy público para revisión, se retiró el CTA que mencionaba WhatsApp como acción principal y se dejó claro en el sitio que los datos de contacto y el catálogo final no están confirmados.

### Archivos Modificados

- `src/LaboratorioTlahuac.Web/src/app/public/layout/public-layout.component.ts`
- `src/LaboratorioTlahuac.Web/src/app/public/pages/home/home-page.component.ts`
- `src/LaboratorioTlahuac.Web/src/app/public/pages/services/services-page.component.ts`
- `src/LaboratorioTlahuac.Web/src/app/public/pages/contact/contact-page.component.ts`
- `docs/PROJECT_STATUS.md`
- `docs/ROADMAP.md`
- `docs/IMPLEMENTATION_LOG.md`
- `docs/01-product/public-website.md`
- `docs/08-qa/RESPONSIVE_CHECKLIST.md`

### Contenido Incorporado

- No se incorporó contenido real nuevo porque WhatsApp, dirección, horarios, logo, servicios exactos, texto principal aprobado y materiales visuales siguen pendientes.
- Se incorporó copy seguro para revisión, sin presentar datos no confirmados como definitivos.

### Placeholders Retirados O Reducidos

- Se retiró `WhatsApp pendiente por confirmar` como CTA principal.
- El CTA principal ahora lleva a `/contacto` con texto neutral.
- El footer ya no lista datos pendientes como si fueran contenido de contacto; indica que se publicarán solo con datos confirmados.

### Alcance

- No se modificaron backend, `AuthService`, guards, cookies, XSRF, endpoints, base de datos, deploy, dependencias ni rutas privadas.
- `/login` sigue visible como entrada pública.
- `/app` y `/app/dashboard` siguen siendo zona privada.
- `/dashboard` no se creó como ruta real.

### Pendientes Generados

- Recibir WhatsApp real, dirección, horarios, logo, servicios exactos, texto principal aprobado y materiales visuales.
- Revisar visualmente el sitio en 360px, 375px, 390px, 414px, 768px, 1024px y desktop.

### Validaciones Ejecutadas

- `npm run build` desde `src/LaboratorioTlahuac.Web`: correcto.
- `git diff --check`: correcto.
- `rg "/dashboard" .`
- `rg "/app/dashboard" .`
- `rg "/login" src/LaboratorioTlahuac.Web/src/app docs README.md AGENTS.md`
- `rg "pendiente" docs/01-product/public-website.md src/LaboratorioTlahuac.Web/src/app/public`
- `curl -i -s http://127.0.0.1:4200/`
- `curl -i -s http://127.0.0.1:4200/servicios`
- `curl -i -s http://127.0.0.1:4200/contacto`
- `curl -i -s http://127.0.0.1:4200/login`
- `curl -i -s http://127.0.0.1:4200/app`
- `curl -i -s http://127.0.0.1:4200/app/dashboard`

Las pruebas con `curl` confirman que el dev server sirve el shell Angular en esas rutas. La validación visual y redirecciones de router deben confirmarse en navegador real.

## 2026-05-13 - Revisión Seguridad/Routing De Guards Y ReturnUrl

### Cambio Realizado

Se revisó el flujo de guards y login después de corregir la pantalla en blanco de `/app/dashboard` sin sesión. Se endureció la sanitización de `returnUrl` para aceptar solo rutas internas bajo `/app` y usar fallback seguro `/app/dashboard` para valores externos o inválidos.

### Archivos Modificados

- `src/LaboratorioTlahuac.Web/src/app/auth/pages/login/login-page.component.ts`
- `docs/PROJECT_STATUS.md`
- `docs/IMPLEMENTATION_LOG.md`
- `docs/03-architecture/AUTH_FLOW.md`

### Comportamiento Confirmado Por Código

- `authGuard` redirige usuario sin sesión a `/login?returnUrl=...`.
- `authGuard` también redirige a login si falla la verificación inicial de sesión.
- `permissionGuard` conserva `/app/access-denied` para usuario autenticado sin permiso.
- `permissionGuard` no trata falta de permiso como falta de sesión.
- `returnUrl` preserva rutas internas como `/app`, `/app/dashboard`, `/app/clientes`, `/app/ordenes` y `/app/pagos`.
- `returnUrl` rechaza `https://example.com`, `http://example.com`, `//example.com`, `javascript:alert(1)`, valores con espacios y valores con backslash.
- `/dashboard` no se creó como ruta privada real.

### Validaciones Ejecutadas

- `git status --short`
- `git diff --stat`
- `rg -n "returnUrl|getSafePrivateReturnUrl|navigateByUrl|createUrlTree" src/LaboratorioTlahuac.Web/src/app`
- `npm run build` desde `src/LaboratorioTlahuac.Web`: correcto.
- `git diff --check`: correcto.
- `rg "/dashboard" .`
- `rg "/app/dashboard" .`
- `rg "returnUrl" src/LaboratorioTlahuac.Web/src/app`
- `rg "/login" src/LaboratorioTlahuac.Web/src/app docs README.md AGENTS.md`
- `curl -i -s http://127.0.0.1:4200/app/dashboard`
- `curl -i -s http://127.0.0.1:4200/app`
- `curl -i -s 'http://127.0.0.1:4200/login?returnUrl=%2Fapp%2Fdashboard'`
- `curl -i -s 'http://127.0.0.1:4200/login?returnUrl=https://example.com'`
- `curl -i -s 'http://127.0.0.1:4200/login?returnUrl=//example.com'`

Las pruebas con `curl` confirman que el dev server sirve el shell Angular para esas URLs. La redirección real de cliente requiere navegador porque ocurre dentro del router Angular.

### Pendientes

- Confirmar en navegador real el cambio de URL de `/app/dashboard` sin sesión a `/login?returnUrl=%2Fapp%2Fdashboard`.
- Confirmar en navegador real los casos inválidos: `returnUrl=https://example.com` y `returnUrl=//example.com`.
- No se ejecutaron pruebas con sesión autenticada porque no se levantó API ni usuario de prueba en esta tarea.

## 2026-05-13 - Fase 1.1 Hallazgo Manual De Redirección Privada

### Cambio Realizado

Se atendió el hallazgo manual: al escribir directamente `http://127.0.0.1:4200/app/dashboard` sin sesión, la app podía quedar en blanco si la verificación de sesión fallaba con un error distinto a `401`. Ahora los guards frontend tratan ese error como sesión no autenticada y devuelven un `UrlTree` hacia `/login` con `returnUrl`.

### Archivos Modificados

- `src/LaboratorioTlahuac.Web/src/app/core/guards/auth.guard.ts`
- `src/LaboratorioTlahuac.Web/src/app/core/guards/permission.guard.ts`
- `docs/PROJECT_STATUS.md`
- `docs/IMPLEMENTATION_LOG.md`
- `docs/01-product/public-website.md`
- `docs/03-architecture/AUTH_FLOW.md`
- `docs/08-qa/RESPONSIVE_CHECKLIST.md`

### Alcance

- No se modificó `AuthService`.
- No se modificaron cookies, XSRF, endpoints, backend, base de datos, deploy ni dependencias.
- No se creó `/dashboard` como ruta privada real.
- `/app` y `/app/dashboard` siguen siendo zona privada.

### Validaciones Ejecutadas

- `npm run build` desde `src/LaboratorioTlahuac.Web`: correcto.
- Dev server local recompiló después del cambio.

### Pendientes Generados

- Confirmar manualmente en navegador que `/app/dashboard` sin sesión redirige a `/login?returnUrl=/app/dashboard`.
- Completar revisión visual real en los breakpoints definidos.
- Confirmar contenido real del cliente antes de reemplazar placeholders.

## 2026-05-12 - Fase 1.1 QA Responsive Del Sitio Público

### Cambio Realizado

Se ejecutó una revisión responsive técnica del sitio público y se hicieron ajustes menores de SCSS/layout para reducir riesgo de overflow antes de revisión con cliente.

### Archivos Modificados

- `src/LaboratorioTlahuac.Web/src/app/public/layout/public-layout.component.scss`
- `src/LaboratorioTlahuac.Web/src/app/public/pages/home/home-page.component.scss`
- `src/LaboratorioTlahuac.Web/src/app/public/pages/services/services-page.component.ts`
- `src/LaboratorioTlahuac.Web/src/app/public/pages/contact/contact-page.component.ts`
- `README.md`
- `docs/README.md`
- `docs/PROJECT_STATUS.md`
- `docs/ROADMAP.md`
- `docs/IMPLEMENTATION_LOG.md`
- `docs/01-product/public-website.md`
- `docs/08-qa/RESPONSIVE_CHECKLIST.md`

### Ajustes Responsive

- Footer público ajustado para usar columnas flexibles en tablet/desktop y evitar overflow por columnas `auto`.
- Header, links, botones y footer aceptan wrapping de textos largos.
- Cards/listas públicas mantienen `min-width: 0` para evitar desbordes dentro de grids.
- Páginas `/servicios` y `/contacto` usan padding responsive y ancho máximo de lectura en textos introductorios.
- Botones públicos conservan mínimo táctil de 48px y texto centrado.

### Rutas Verificadas

- `/`: responde en dev server local.
- `/servicios`: responde en dev server local.
- `/contacto`: responde en dev server local.
- `/login`: responde como entrada al sistema, sin cambios de auth.
- `/app` y `/app/dashboard`: responden con shell Angular; la privacidad se confirma por configuración de rutas/guards, sin modificar guards.

### Validaciones Ejecutadas

- `git status --short`
- `git diff --stat`
- `npm run build` desde `src/LaboratorioTlahuac.Web`: correcto.
- `git diff --check`: correcto.
- `curl -i -s http://127.0.0.1:4200/`
- `curl -i -s http://127.0.0.1:4200/servicios`
- `curl -i -s http://127.0.0.1:4200/contacto`
- `curl -i -s http://127.0.0.1:4200/login`
- `curl -i -s http://127.0.0.1:4200/app`
- `curl -i -s http://127.0.0.1:4200/app/dashboard`
- `rg "/dashboard" .`
- `rg "/app/dashboard" .`
- `rg "/login" src/LaboratorioTlahuac.Web/src/app docs README.md AGENTS.md`

### Limitación Del Entorno

No se encontró Chromium, Chrome, Firefox, Playwright, Puppeteer ni `wkhtmltoimage` disponibles sin instalar dependencias. Por esa razón no se generaron capturas ni se marcó como completada la revisión visual por breakpoint.

### Pendientes Generados

- Revisar visualmente 360px, 375px, 390px, 414px, 768px, 1024px y desktop en navegador real o dispositivo.
- Si se revisa desde celular en la misma red, levantar temporalmente Angular con `npm start -- --host 0.0.0.0 --port 4200`; `127.0.0.1` solo sirve en la computadora local.
- Confirmar WhatsApp real, dirección, horarios, logo, servicios exactos, textos finales y materiales visuales aprobados.

## 2026-05-12 - Fase 1 Sitio Público Mobile-First

### Cambio Realizado

Se implementó la primera versión pública del sitio institucional mobile-first dentro de la app Angular existente, sin crear una segunda app y sin modificar backend, autenticación, endpoints, base de datos, deploy ni rutas privadas.

### Archivos Creados

- `src/LaboratorioTlahuac.Web/src/app/public/pages/home/home-page.component.scss`

### Archivos Modificados

- `src/LaboratorioTlahuac.Web/src/index.html`
- `src/LaboratorioTlahuac.Web/src/app/app.routes.ts`
- `src/LaboratorioTlahuac.Web/src/app/public/layout/public-layout.component.ts`
- `src/LaboratorioTlahuac.Web/src/app/public/layout/public-layout.component.scss`
- `src/LaboratorioTlahuac.Web/src/app/public/pages/home/home-page.component.ts`
- `src/LaboratorioTlahuac.Web/src/app/public/pages/services/services-page.component.ts`
- `src/LaboratorioTlahuac.Web/src/app/public/pages/contact/contact-page.component.ts`
- `README.md`
- `docs/PROJECT_STATUS.md`
- `docs/ROADMAP.md`
- `docs/IMPLEMENTATION_LOG.md`
- `docs/01-product/public-website.md`
- `docs/03-architecture/ARCHITECTURE.md`
- `docs/08-qa/RESPONSIVE_CHECKLIST.md`

### Rutas Confirmadas

- `/`: sitio público institucional.
- `/servicios`: página pública de capacidades provisionales.
- `/contacto`: página pública de contacto provisional.
- `/login`: entrada pública al sistema, sin cambios de auth.
- `/app` y `/app/dashboard`: zona privada, sin cambios.

### Validaciones Ejecutadas

- `npm run build` desde `src/LaboratorioTlahuac.Web`: correcto.
- `npm start -- --host 127.0.0.1 --port 4200`: servidor Angular levantado en `http://127.0.0.1:4200/`.
- `curl -s http://127.0.0.1:4200/`: confirma `lang="es"`, title y meta description del sitio público.
- Búsqueda de referencias de `/login`, `/app` y `/app/dashboard`: rutas privadas reales se mantienen bajo `/app`.
- No se ejecutó lint porque `src/LaboratorioTlahuac.Web/package.json` no define script `lint`.
- No se ejecutó `dotnet build` ni `dotnet test` porque no se modificó backend ni configuración compartida.

### Pendientes Generados

- Confirmar WhatsApp, dirección, horarios, logo, servicios exactos y textos finales con el cliente.
- Revisar visualmente los viewports obligatorios del checklist responsive.
- Validar `/app` y `/login` en navegador después de levantar entorno local con API si se hará demo integral.
- Preparar Fase 1.1 con ajustes visuales por feedback y contenido real.

## 2026-05-12 - Fase 0.2 Consolidación Documental

### Cambio Realizado

Se consolidó la documentación para separar sistema privado, sitio público, control global, deploy, QA y documentación comercial antes de iniciar pantallas del sitio público.

### Documentos Creados

- `docs/README.md`
- `docs/01-product/public-website.md`
- `docs/01-product/internal-system.md`
- `docs/03-architecture/ARCHITECTURE.md`
- `docs/03-architecture/AUTH_FLOW.md`
- `docs/05-delivery/DEPLOYMENT.md`
- `docs/08-qa/RESPONSIVE_CHECKLIST.md`

### Documentos Movidos Con `git mv`

- `docs/03-architecture/architecture-overview.md` -> `docs/03-architecture/ARCHITECTURE.md`
- `docs/03-architecture/authentication-and-authorization.md` -> `docs/03-architecture/AUTH_FLOW.md`
- `docs/06-operations/deployment.md` -> `docs/05-delivery/DEPLOYMENT.md`

### Puentes Creados O Reemplazados

- `docs/ARCHITECTURE.md` -> `docs/03-architecture/ARCHITECTURE.md`
- `docs/AUTH_FLOW.md` -> `docs/03-architecture/AUTH_FLOW.md`
- `docs/DEPLOYMENT.md` -> `docs/05-delivery/DEPLOYMENT.md`
- `docs/RESPONSIVE_CHECKLIST.md` -> `docs/08-qa/RESPONSIVE_CHECKLIST.md`
- `docs/03-architecture/architecture-overview.md` -> `docs/03-architecture/ARCHITECTURE.md`
- `docs/03-architecture/authentication-and-authorization.md` -> `docs/03-architecture/AUTH_FLOW.md`
- `docs/06-operations/deployment.md` -> `docs/05-delivery/DEPLOYMENT.md`

### Documentos Modificados

- `AGENTS.md`
- `README.md`
- `docs/PROJECT_STATUS.md`
- `docs/ROADMAP.md`
- `docs/IMPLEMENTATION_LOG.md`
- `docs/00-governance/changelog.md`
- `docs/00-governance/project-status.md`
- `docs/09-commercial/commercial-phases.md`

### Motivo

Evitar duplicados y contradicciones entre la documentación del MVP administrativo avanzado y el frente nuevo del sitio público institucional.

### Pendientes Generados

- Iniciar Fase 1 del sitio público mobile-first.
- Confirmar contenido del cliente para home, servicios y contacto.
- Revisar `src/LaboratorioTlahuac.Web/README.md`, que aún es el README generado por Angular CLI.
- Cuando se eliminen puentes en una fase posterior, actualizar cualquier referencia restante.

## 2026-05-12 - Auditoría Documental

### Cambio Realizado

Se realizó una auditoría documental del repositorio para revisar alineación entre `AGENTS.md`, `README.md`, documentación existente en `docs/`, documentos nuevos de Fase 0, y el inventario vacío de `.agents/` y `.codex/`.

### Archivos Modificados

- `docs/DOCUMENTATION_AUDIT.md`
- `docs/PROJECT_STATUS.md`
- `docs/IMPLEMENTATION_LOG.md`

### Motivo

Detectar duplicados, solapamientos, contradicciones y fuentes canónicas antes de avanzar con el sitio público mobile-first.

### Pendientes Generados

- Definir si se aprueba la estructura documental propuesta.
- Consolidar `README.md`, documentos raíz de `docs/` y carpetas numeradas sin tocar código.
- Separar explícitamente documentación del sistema privado, sitio público, control global y documentación comercial.

## 2026-05-12 - Fase 0 Sitio Público

### Cambio Realizado

Inicialización de Fase 0 para el sitio público de Laboratorio Dental Tláhuac. Se inspeccionó la estructura del repositorio, se detectó el stack existente y se creó documentación raíz para guiar el desarrollo mobile-first del sitio público dentro del repo actual.

### Archivos Modificados

- `AGENTS.md`
- `docs/PROJECT_STATUS.md`
- `docs/ROADMAP.md`
- `docs/IMPLEMENTATION_LOG.md`
- `docs/ARCHITECTURE.md`
- `docs/RESPONSIVE_CHECKLIST.md`
- `docs/DEPLOYMENT.md`
- `docs/AUTH_FLOW.md`

### Motivo

Dejar reglas permanentes para Codex, documentar el estado real del proyecto y definir el plan técnico inicial antes de implementar pantallas complejas o cambios de lógica.

### Pendientes Generados

- Ejecutar Fase 1 con rediseño mobile-first del sitio público existente.
- Confirmar contenido real del cliente: servicios, ubicación, horarios, teléfono, WhatsApp y mensajes comerciales.
- Definir plataforma de despliegue, DNS, HTTPS y configuración productiva.
- Validar visualmente el sitio en anchos móviles antes de presentarlo al cliente.
