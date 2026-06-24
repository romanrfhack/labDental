# Flujo De Autenticación Y Autorización

Fuente canónica de login, sesión, cookies, CSRF/XSRF, permisos, rutas privadas y redirecciones.

## Rutas Reales

- Sitio público: `/`, `/catalogo`, `/servicios`, `/contacto`.
- Login: `/login`.
- App privada: `/app`.
- Dashboard privado real: `/app/dashboard`.

`/dashboard` no es ruta privada real en el router actual.

## Backend Auth

Endpoints actuales:

- `GET /api/auth/csrf`
- `POST /api/auth/login`
- `POST /api/auth/logout`
- `GET /api/auth/me`

Endpoints técnicos solo en Development:

- `GET /api/security/permissions-check`
- `POST /api/security/csrf-check`

Seed tecnico solo en Development:

- `SecuritySeed:LimitedQaUser:RunOnStartup`
- `SecuritySeed:LimitedQaUser:Email`
- `SecuritySeed:LimitedQaUser:Password`
- `SecuritySeed:LimitedQaUser:FullName`
- `SecuritySeed:LimitedQaUser:Permissions`
- Variables sensibles equivalentes: `LT_QA_LIMITED_EMAIL`, `LT_QA_LIMITED_PASSWORD` y `LT_QA_LIMITED_FULL_NAME`

Este seed no expone endpoint HTTP, no corre fuera de `Development`, esta desactivado por default y debe apagarse despues de sincronizar el usuario local.

## Cookie De Sesión

- Provider: ASP.NET Core Cookie Authentication.
- Cookie productiva configurada: `__Host-Ldt.Auth`.
- Cookie de desarrollo configurada: `Ldt.Dev.Auth`.
- `HttpOnly`: sí.
- `SameSite`: `Lax`.
- `Secure`: obligatorio en producción.
- Expiración: 8 horas con sliding expiration.
- El frontend no guarda tokens en `localStorage` ni `sessionStorage`.

## CSRF/XSRF

La autenticación usa cookies, por lo que los requests mutables bajo `/api` requieren protección CSRF/XSRF.

- Cookie legible por JavaScript: `XSRF-TOKEN`.
- Header requerido: `X-XSRF-TOKEN`.
- Cookie interna antiforgery: HttpOnly.
- Métodos mutables protegidos: `POST`, `PUT`, `PATCH`, `DELETE`.
- Métodos seguros excluidos: `GET`, `HEAD`, `OPTIONS`, `TRACE`.

Exclusiones documentadas:

- `GET /health`
- `GET /api/auth/csrf`
- `GET /api/auth/me`

`POST /api/auth/login` y `POST /api/auth/logout` sí requieren token XSRF.

## Flujo De Login

1. Angular solicita `GET /api/auth/csrf`.
2. La API emite `XSRF-TOKEN` y cookie antiforgery interna.
3. Angular envía `POST /api/auth/login` con email, password, `withCredentials` y `X-XSRF-TOKEN`.
4. La API valida usuario, contraseña, usuario activo y bloqueo.
5. Si el login es correcto, la API emite cookie de sesión HttpOnly.
6. Angular renueva `GET /api/auth/csrf` porque cambió la identidad.
7. Angular guarda el usuario en memoria del `AuthService`.
8. `GET /api/auth/me` rehidrata sesión después de refrescar página.

## Permisos

La autorización es por permisos, no por nombre de rol.

- Admin recibe todos los permisos mediante seed inicial.
- Usuario QA limitado local puede recibir una allowlist explicita de permisos mediante `SecuritySeed:LimitedQaUser:Permissions`; para probar `/app/access-denied` no debe incluir `reports.view`.
- Los permisos se emiten como claims `permission`.
- El backend valida permisos con policies y `RequirePermission`.
- El frontend usa permisos para navegación y visibilidad, pero no sustituye la autorización backend.

Permisos por ruta privada:

- `/app/dashboard`: `reports.view`
- `/app/ordenes`: `orders.view`
- `/app/ordenes/nueva`: `orders.create`
- `/app/ordenes/:id`: `orders.view`
- `/app/ordenes/:id/editar`: `orders.edit`
- `/app/clientes`: `customers.view`
- `/app/clientes/nuevo`: `customers.create`
- `/app/clientes/:id`: `customers.view`
- `/app/clientes/:id/editar`: `customers.edit`
- `/app/pagos`: `payments.view`
- `/app/inventario`: `inventory.view`
- `/app/proveedores`: `suppliers.view`
- `/app/admin/usuarios`: `users.manage`
- `/app/admin/roles`: `roles.manage`

## Redirecciones

- Usuario sin sesión en `/app/*`: redirección frontend a `/login?returnUrl=...`.
- Si la verificación frontend de sesión falla por error de red/API durante la navegación a `/app/*`, el guard debe tratarlo como sesión no autenticada y redirigir a `/login?returnUrl=...` para evitar una pantalla en blanco.
- Usuario autenticado sin permiso: redirección frontend a `/app/access-denied`.
- API sin sesión: responde `401`.
- API sin permiso: responde `403`.
- Endpoints `/api` no redirigen con `302` a `/login`.

La Fase 2.6 valida por API automatizada esta diferencia: sin sesion `/api/dashboard/summary` responde `401`; con usuario QA limitado autenticado sin `reports.view` responde `403`.

## ReturnUrl Seguro

`returnUrl` se usa solo para regresar a una ruta privada interna después de login correcto.

Valores aceptados:

- `/app`
- `/app/...`
- `/app?...`
- `/app#...`

Valores rechazados o normalizados al fallback seguro `/app/dashboard`:

- Rutas externas como `https://example.com` o `http://example.com`.
- URLs protocol-relative como `//example.com`.
- Esquemas ejecutables como `javascript:alert(1)`.
- Valores con espacios al inicio/final.
- Valores con backslash.
- Rutas que solo parecen privadas pero no pertenecen a `/app`, por ejemplo `/application`.

La sanitización vive en el componente de login antes de ejecutar `router.navigateByUrl(...)`.

## Diferencia Entre Sin Sesión Y Sin Permiso

- Usuario no autenticado en `/app/*`: redirección a `/login?returnUrl=...`.
- Error al verificar sesión en `/app/*`: redirección a `/login?returnUrl=...` como fallback seguro.
- Usuario autenticado sin permiso requerido por `permissionGuard`: redirección a `/app/access-denied`.
- Un usuario autenticado sin permiso no debe tratarse como usuario sin sesión.

## Pendientes De Seguridad

- Definir checklist de seguridad previo a producción.
- Confirmar HTTPS, reverse proxy y política final de CORS.
- Evaluar Content Security Policy cuando el sitio público incorpore más assets o integraciones.
- Mantener revisión de dependencias frontend y backend.
- Validar protección contra abuso para cualquier formulario público futuro.

## Validación Fase 2.0 - 2026-05-15

Resultado de `/login`:

- `/login` sigue configurado como ruta pública en `app.routes.ts`.
- La revisión de `login-page.component.ts` confirma que los cambios visuales de Fase 1.5 no alteraron la llamada a `AuthService.login()`, el manejo de errores, la navegación posterior al login ni la sanitización de `returnUrl`.
- `AuthService.login()` sigue solicitando CSRF, enviando `POST /api/auth/login` con `withCredentials`, renovando CSRF después del login correcto y guardando el usuario autenticado en memoria.

Resultado de `returnUrl`:

- `/login?returnUrl=%2Fapp%2Fdashboard` conserva el destino interno seguro `/app/dashboard`.
- `https://example.com`, `//example.com` y `javascript:alert(1)` no se usan como destino de navegación posterior al login.
- Valores externos, valores con esquema, protocol-relative, espacios, backslash o rutas fuera de `/app` usan fallback seguro `/app/dashboard`.

Resultado de `/app/dashboard` sin sesión:

- Por código, `/app` está protegido con `authGuard` y `/app/dashboard` está dentro de esa zona privada.
- Si `ensureSession()` devuelve `false` o falla con error durante la navegación a `/app/*`, el guard redirige a `/login?returnUrl=...`.
- `/app/dashboard` además requiere el permiso `reports.view` mediante `permissionGuard`.

Resultado de validación de rutas:

- Confirmado por código: `/`, `/servicios`, `/catalogo`, `/contacto` y `/login` son rutas públicas.
- Confirmado por código: `/app` y `/app/dashboard` son rutas privadas.
- Confirmado por código: `/dashboard` no es ruta privada real; el wildcard del router redirige a la home pública.
- Confirmado por `curl` con Angular dev server en `http://127.0.0.1:4201/`: todas las rutas solicitadas respondieron con shell Angular `200`. `curl` no ejecuta Angular, por lo que la redirección real del guard queda como validación por código hasta probar en navegador.

Resultado de login real:

- Pendiente en entorno local porque no hay API/base/credenciales Admin configuradas.
- `appsettings.Development.json` tiene `SecuritySeed:RunOnStartup` en `false`.
- `appsettings.json` conserva `SecuritySeed:Admin` vacío.
- No se inventaron credenciales, no se modificó seed y no se tocó base de datos en esta fase.

Diferencia entre usuario sin sesión y usuario sin permiso:

- Sin sesión: `authGuard` o `permissionGuard` redirige a `/login?returnUrl=...`.
- Error al verificar sesión: se trata como no autenticado y redirige a `/login?returnUrl=...`.
- Con sesión pero sin permiso: `permissionGuard` redirige a `/app/access-denied`.
- La API mantiene la diferencia HTTP: `401` para sin sesión y `403` para sin permiso.

## Validación Fase 2.1 - 2026-05-15

Resultado de ambiente local:

- `appsettings.Development.json` apunta a `Server=localhost;Database=LaboratorioTlahuac_Dev`, por lo que la conexión declarada es local.
- `dotnet ef` está disponible en versión `10.0.7`.
- `dotnet ef migrations list` listó las migraciones existentes, pero no pudo consultar estado aplicado porque SQL Server no estuvo accesible en `localhost`.
- `dotnet ef database update` falló por conexión a SQL Server; no se aplicaron migraciones.

Resultado de Admin local:

- No hay variables `LT_ADMIN_EMAIL`, `LT_ADMIN_PASSWORD` ni `LT_ADMIN_FULL_NAME` en el proceso.
- `SecuritySeed__RunOnStartup` no está en `true`.
- No existe archivo de user-secrets para `laboratorio-tlahuac-api-dev` en este entorno.
- No se creó Admin local, no se inventaron credenciales y no se guardaron secretos en archivos versionados.

Resultado de API/auth:

- API local levantó en `http://localhost:5277`.
- `GET /health` respondió saludable.
- `GET /api/auth/csrf` respondió `204` y emitió cookies de CSRF.
- `GET /api/auth/me` sin sesión respondió `401`.
- Login real, `/api/auth/me` autenticado, logout y redirección posterior de `/app/dashboard` quedaron pendientes en esa fase hasta contar con base local accesible y Admin local configurado; login real y dashboard autenticado se cerraron posteriormente por validación manual de Fase 2.1d.

Resultado de permisos:

- Por código, `SecuritySeeder` asigna al rol Admin todos los permisos de `Permissions.All`.
- `Permissions.All` incluye `reports.view`.
- `/app/dashboard` sigue protegido por `permissionGuard` y requiere `reports.view`.

Comandos seguros pendientes para Admin local:

```bash
dotnet user-secrets set LT_ADMIN_EMAIL "<email-local>" --project src/LaboratorioTlahuac.Api/LaboratorioTlahuac.Api.csproj
dotnet user-secrets set LT_ADMIN_PASSWORD "<password-local-seguro>" --project src/LaboratorioTlahuac.Api/LaboratorioTlahuac.Api.csproj
dotnet user-secrets set LT_ADMIN_FULL_NAME "Administrador" --project src/LaboratorioTlahuac.Api/LaboratorioTlahuac.Api.csproj
dotnet user-secrets set SecuritySeed:RunOnStartup true --project src/LaboratorioTlahuac.Api/LaboratorioTlahuac.Api.csproj
```

Después de crear el Admin, se recomienda apagar el seed local si ya no se necesita:

```bash
dotnet user-secrets set SecuritySeed:RunOnStartup false --project src/LaboratorioTlahuac.Api/LaboratorioTlahuac.Api.csproj
```

## Validación Fase 2.1c - 2026-05-18

Resultado de ambiente local Docker:

- Docker está disponible.
- El contenedor dedicado esperado es `ldt-labdental-sql`.
- `ldt-labdental-sql` no existe todavía en este entorno.
- No se usó `codex-cobranza-sql` ni ningún contenedor de otro proyecto.
- Los puertos `14336`, `14337` y `14338` no aparecen en escucha; el puerto preferido para reintentar es `14336`.
- La ejecución se detuvo porque `LDT_SQL_SA_PASSWORD` no está definida.

Resultado de conexión/base:

- No se creó el contenedor SQL Server.
- No se creó el volumen `ldt-labdental-sql-data`.
- No se configuró `ConnectionStrings:DefaultConnection` en user-secrets.
- No se aplicaron migraciones.
- Base local esperada al reintentar: `LaboratorioTlahuac_Dev`.
- Connection string esperada en user-secrets, redactada: `Server=localhost,14336;Database=LaboratorioTlahuac_Dev;User Id=sa;Password=<redacted>;TrustServerCertificate=True;Encrypt=True`.

Resultado de Admin local:

- `LT_ADMIN_EMAIL` y `LT_ADMIN_PASSWORD` no están definidas.
- `LT_ADMIN_FULL_NAME` existe en el proceso, pero no se usó.
- No se ejecutó seed Admin, no se inventaron credenciales y no se imprimieron secretos.

Resultado de login real:

- Login real quedó pendiente en esa fase por falta de contenedor/base local dedicada y credenciales Admin locales.
- `/api/auth/me` autenticado quedó pendiente como evidencia independiente.
- Logout quedó pendiente como evidencia independiente.
- Redirección visual de `/app/dashboard` sin sesión quedó pendiente en esa fase y se validó posteriormente como redirección posterior a logout o sesión cerrada.
- `/login` sigue documentado como ruta pública; `/app` y `/app/dashboard` siguen documentadas como rutas privadas; `/dashboard` sigue sin ser ruta privada real.

## Validación Fase 2.1c - 2026-05-23

Resultado de ambiente local Docker:

- Contenedor usado: `ldt-labdental-sql`.
- Puerto usado: `14336`, mapeado desde `1433/tcp`.
- Base validada por EF: `LaboratorioTlahuac_Dev`.
- No se usó `codex-cobranza-sql` ni ningún contenedor de otro proyecto.

Resultado de migraciones:

- `dotnet ef migrations list` con `LaboratorioTlahuac.Infrastructure` como proyecto y `LaboratorioTlahuac.Api` como startup project listó:
  - `20260508044157_InitialSecurityModel`
  - `20260509004819_AddCustomersAndInternalDoctors`
  - `20260509022531_AddWorkOrders`
  - `20260509053231_AddPayments`
- `dotnet ef database update` confirmó que no había migraciones pendientes: la base ya estaba al día.

Resultado de seed Admin:

- La API local levantó en `http://localhost:5277`.
- `SecuritySeed:RunOnStartup` estaba activo al iniciar la API y ejecutó la ruta de seed.
- El seed tuvo configuración Admin disponible desde user-secrets, confirmado por las consultas parametrizadas de arranque sin imprimir valores.
- Al finalizar la validación se ejecutó `dotnet user-secrets set SecuritySeed:RunOnStartup false --project src/LaboratorioTlahuac.Api/LaboratorioTlahuac.Api.csproj`.

Resultado de API/auth:

- `GET /health` respondió `200`.
- `GET /api/auth/csrf` respondió `204`.
- `GET /api/auth/me` sin sesión respondió `401`.
- Login real no se ejecutó porque `LT_ADMIN_EMAIL` y `LT_ADMIN_PASSWORD` no están disponibles en el proceso de Codex.
- `/api/auth/me` autenticado, logout y `/api/auth/me` después de logout quedaron pendientes como evidencia independiente hasta ejecutar la prueba con credenciales Admin disponibles en el proceso o desde navegador.
- No se ejecutó `dotnet user-secrets list`, no se imprimieron secretos y no se modificaron `appsettings` con contraseñas.

## Validación Fase 2.2 - 2026-05-27

Resultado de auth con Admin local:

- `LT_ADMIN_EMAIL` y `LT_ADMIN_PASSWORD` estuvieron disponibles como variables de entorno locales y se usaron sin imprimir valores.
- `GET /api/auth/csrf` antes de login respondió `204`.
- `POST /api/auth/login` respondió `200`.
- `GET /api/auth/csrf` después de login respondió `204`.
- `GET /api/auth/me` autenticado respondió `200` y reportó 19 permisos.
- Permisos confirmados en Admin: `reports.view`, `orders.view`, `customers.view` y `payments.view`.
- `POST /api/auth/logout` con XSRF renovado respondió `200`.
- `GET /api/auth/me` posterior al logout respondió `401`.

Resultado sin sesión:

- `GET /api/auth/me` respondió `401`.
- `GET /api/dashboard/summary` respondió `401`.
- Endpoints privados revisados (`/api/customers`, `/api/work-orders`, `/api/payments` y `/api/dashboard/summary`) respondieron `401`.

Resultado de rutas y `returnUrl`:

- `/login` sigue siendo ruta pública.
- `/app` y `/app/dashboard` siguen siendo rutas privadas.
- `/dashboard` no es ruta privada real.
- Por código, `authGuard` y `permissionGuard` construyen `/login?returnUrl=...` para usuario sin sesión o error al verificar sesión.
- Por código, `permissionGuard` conserva la diferencia entre usuario sin sesión y usuario autenticado sin permiso; si hay sesión pero falta permiso, redirige a `/app/access-denied`.
- Por código, `getSafePrivateReturnUrl()` conserva solo rutas internas seguras bajo `/app` y normaliza destinos externos o inválidos a `/app/dashboard`.

Limitaciones:

- No se probó usuario autenticado sin permiso por falta de usuario QA limitado local.
- No se inspeccionó Network de navegador porque no hay navegador/headless local sin instalar dependencias.

## Validación Manual Fase 2.1c - 2026-05-23

Resultado reportado desde navegador con Admin local creado por seed:

- `/login` carga correctamente.
- Login con Admin local: validado.
- Redirección posterior al login a `/app/dashboard`: validada.
- Dashboard: no validado en Fase 2.1c; cargó una vez, pero al regresar a la página queda en `Cargando dashboard...`. Este pendiente se cierra posteriormente en Fase 2.1d.
- `GET /api/auth/me` autenticado: no confirmado porque el resultado manual no fue marcado como `sí`.
- Logout: no confirmado como acción independiente porque el resultado manual no fue marcado.
- Después de logout, `/app/dashboard` redirige a `/login?returnUrl=%2Fapp%2Fdashboard`.

Confirmación de rutas y seguridad:

- `/login` sigue siendo ruta pública.
- `/app` y `/app/dashboard` siguen siendo rutas privadas.
- `/dashboard` no es ruta privada real.
- No se imprimieron secretos, no se ejecutó `dotnet user-secrets list`, no se usó `codex-cobranza-sql` y no se modificaron `appsettings` con contraseñas.

## Diagnóstico Fase 2.1d - 2026-05-23

Resultado de revisión de rutas, auth y permisos:

- `/login` sigue siendo ruta pública.
- `/app` sigue protegido por `authGuard`.
- `/app/dashboard` sigue protegido por `permissionGuard` y requiere `reports.view`.
- `/dashboard` no es ruta privada real.
- `AuthService` y guards no se modificaron.
- El Admin seed incluye `reports.view` porque `Permissions.All` contiene ese permiso y el seed asigna todos los permisos al rol Admin.

Resultado de endpoints revisados:

- `GET /health`: `200`.
- `GET /api/auth/csrf`: `204`.
- `GET /api/auth/me` sin sesión: `401`.
- `GET /api/dashboard/summary` sin sesión: `401`.
- `GET /api/auth/me` autenticado: pendiente como evidencia independiente porque `LT_ADMIN_EMAIL` y `LT_ADMIN_PASSWORD` no están disponibles en el proceso de Codex.
- `GET /api/dashboard/summary` autenticado por curl: pendiente por la misma razón; queda validado indirectamente después por carga correcta del dashboard.
- Logout autenticado por curl: pendiente como evidencia independiente por la misma razón.

Causa probable del estado `Cargando dashboard...`:

- El componente del dashboard solo hace una llamada HTTP: `GET /api/dashboard/summary`.
- Los errores HTTP ya apagaban `isLoading` mediante `finalize`.
- Si la llamada quedaba pendiente sin completar ni fallar, no existia timeout y el texto `Cargando dashboard...` podia permanecer indefinidamente.

Corrección aplicada:

- `dashboard-page.component.ts` agrega timeout de 15 segundos a la consulta de resumen.
- Si la consulta tarda demasiado, el dashboard apaga el estado de carga y muestra un error controlado.
- No se modificaron cookies, CSRF/XSRF, endpoints, permisos, migraciones, seed, deploy ni `appsettings`.

Limitación de validación:

- No hay navegador/headless disponible sin instalar dependencias.
- Validación manual sugerida con DevTools: revisar `GET /api/auth/me`, `GET /api/dashboard/summary`, status code, respuesta y errores de consola después de login y al regresar a `/app/dashboard`.

## Cierre Manual Fase 2.1d - 2026-05-27

Resultado confirmado por el responsable del proyecto:

- `/login` sigue siendo ruta pública.
- `/app` y `/app/dashboard` siguen siendo rutas privadas.
- `/app/dashboard` autenticado fue validado manualmente con Admin local.
- `/app/dashboard` ya no queda indefinidamente en `Cargando dashboard...`.
- `/app/dashboard` sin sesión o con sesión cerrada redirige a `/login?returnUrl=%2Fapp%2Fdashboard`.
- `/dashboard` no es ruta privada real.

Matices de evidencia:

- Flujo autenticado validado manualmente; `GET /api/auth/me` autenticado no fue inspeccionado de forma independiente.
- `GET /api/dashboard/summary` autenticado queda validado indirectamente por la carga correcta del dashboard; el endpoint no fue inspeccionado de forma independiente.
- La redirección posterior a logout o sesión cerrada queda validada; logout como acción independiente no queda documentado como inspeccionado por separado.
- No se modificaron `AuthService`, guards, cookies, CSRF/XSRF, endpoints, base de datos, migraciones ni deploy en este cierre documental.

## Validación Fase 2.4 - 2026-05-27

Resultado de auth con Admin local:

- Credenciales Admin disponibles como variables de entorno locales y usadas sin imprimir valores.
- `GET /api/auth/csrf` antes de login respondió `204`.
- `POST /api/auth/login` respondió `200`.
- `GET /api/auth/csrf` después de login respondió `204`.
- `GET /api/auth/me` autenticado respondió `200` y reportó 19 permisos.
- `GET /api/dashboard/summary` autenticado respondió `200`.
- `GET /api/customers`, `GET /api/work-orders` y `GET /api/payments` autenticados respondieron `200`.
- `POST /api/auth/logout` respondió `200`.
- `GET /api/auth/me` después de logout respondió `401`.
- `GET /api/dashboard/summary` después de logout respondió `401`.

Resultado de rutas y redirecciones:

- `/login` sigue siendo ruta pública.
- `/app` sigue protegido por `authGuard`.
- `/app/dashboard` sigue protegido por `permissionGuard` y requiere `reports.view`.
- `/dashboard` no es ruta privada real; no se cambió el router para convertirla en privada.
- Por código, usuario sin sesión en `/app/*` sigue redirigiendo a `/login?returnUrl=...`.
- Por código, `getSafePrivateReturnUrl()` conserva solo rutas internas seguras bajo `/app` y normaliza destinos externos o inválidos a `/app/dashboard`.
- La redirección visual real en navegador no se pudo ejecutar porque no hay navegador/headless local disponible sin instalar dependencias.

Resultado de permisos:

- No se creó usuario limitado local.
- El seed disponible asegura Admin; no provee usuario limitado configurable.
- Los endpoints Development de seguridad son solo diagnósticos y no crean usuarios.
- Las páginas de usuarios/roles siguen como placeholders sin CRUD seguro.
- Los usuarios limitados existen solo en fixtures de pruebas automatizadas con SQLite en memoria.
- No se autorizó creación directa por SQL y no se alteraron permisos del Admin.
- Por código, `permissionGuard` conserva la diferencia entre falta de sesión y falta de permiso: sin sesión va a `/login?returnUrl=...`; sesión autenticada sin permiso va a `/app/access-denied`.
- Por pruebas API, una sesión sin permiso recibe `403`, incluyendo `/api/dashboard/summary` sin `reports.view`.

No se modificaron `AuthService`, `auth.guard.ts`, `permission.guard.ts`, cookies, CSRF/XSRF, endpoints, rutas privadas, migraciones, deploy ni dependencias.

## Fase 2.5 - Cierre Visual Y Usuario QA Limitado

Estado: pase visual humano privado completado; usuario QA limitado no implementado.

Resultado manual visual:

- `/login` carga correctamente en navegador real.
- Login Admin funciona correctamente.
- `/app/dashboard`, `/app/clientes`, `/app/ordenes` y `/app/pagos` cargan correctamente.
- La navegación activa se muestra correctamente en `/app/dashboard`, `/app/clientes`, `/app/ordenes` y `/app/pagos`.
- `/app/inventario`, `/app/proveedores`, `/app/admin/usuarios` y `/app/admin/roles` se muestran correctamente como placeholders.
- Logout funciona correctamente.
- `/app/dashboard` sin sesión redirige a `/login?returnUrl=%2Fapp%2Fdashboard`.
- `/dashboard` raíz no es ruta privada real.
- No hubo regresión visible del sitio público ni bloqueantes visuales reportados.

Resultado de revisión:

- El flujo actual mantiene la diferencia entre falta de sesión y falta de permiso.
- Usuario sin sesión en `/app/*` debe redirigir a `/login?returnUrl=...`.
- Usuario autenticado sin permiso debe redirigir a `/app/access-denied`.
- API sin sesión responde `401`.
- API sin permiso responde `403`.
- Admin conserva todos los permisos porque el seed asigna `Permissions.All` al rol Admin.
- Los permisos se emiten como claims `permission`.
- Las cuentas limitadas actuales existen solo en fixtures de pruebas automatizadas con SQLite en memoria.
- No existe todavía mecanismo seguro de producto para crear usuario QA limitado en la base local real.

Mecanismo recomendado para una fase posterior:

- Extender el seed con una rama QA limitada solo para `Environment=Development`.
- Mantenerla desactivada por default.
- Requerir `SecuritySeed:QaLimited:Enabled=true`.
- Tomar email/password/nombre desde user-secrets o variables de entorno, por ejemplo `LDT_QA_LIMITED_EMAIL`, `LDT_QA_LIMITED_PASSWORD` y `LDT_QA_LIMITED_FULL_NAME`.
- No imprimir password ni valores sensibles.
- No usar SQL manual.
- No modificar el Admin existente.
- Para validar `/app/access-denied` contra `/app/dashboard`, el usuario limitado no debe tener `reports.view`.

Fuente detallada: `docs/08-qa/limited-user-qa-plan.md`.
