# Flujo De Autenticación Y Autorización

Fuente canónica de login, sesión, cookies, CSRF/XSRF, permisos, rutas privadas y redirecciones.

## Rutas Reales

- Sitio público: `/`, `/servicios`, `/contacto`.
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
