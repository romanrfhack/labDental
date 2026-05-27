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
- Login real, `/api/auth/me` autenticado, logout y redirección posterior de `/app/dashboard` quedan pendientes hasta contar con base local accesible y Admin local configurado.

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

- Login real queda pendiente por falta de contenedor/base local dedicada y credenciales Admin locales.
- `/api/auth/me` autenticado queda pendiente.
- Logout queda pendiente.
- Redirección visual de `/app/dashboard` sin sesión queda pendiente.
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
- `/api/auth/me` autenticado, logout y `/api/auth/me` después de logout quedan pendientes hasta ejecutar la prueba con credenciales Admin disponibles en el proceso o desde navegador.
- No se ejecutó `dotnet user-secrets list`, no se imprimieron secretos y no se modificaron `appsettings` con contraseñas.

## Validación Manual Fase 2.1c - 2026-05-23

Resultado reportado desde navegador con Admin local creado por seed:

- `/login` carga correctamente.
- Login con Admin local: validado.
- Redirección posterior al login a `/app/dashboard`: validada.
- Dashboard: no validado; cargó una vez, pero al regresar a la página queda en `Cargando dashboard...`.
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
- `GET /api/auth/me` autenticado: pendiente porque `LT_ADMIN_EMAIL` y `LT_ADMIN_PASSWORD` no están disponibles en el proceso de Codex.
- `GET /api/dashboard/summary` autenticado: pendiente por la misma razón.
- Logout autenticado por curl: pendiente por la misma razón.

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
