# Autenticación Y Autorización

## Autenticación

- El sitio público no requiere autenticación.
- `/login` permite iniciar sesión.
- `/app/*` requiere usuario autenticado.
- Para el MVP se usa cookie segura HttpOnly.
- El backend expone `GET /api/auth/csrf`, `POST /api/auth/login`, `POST /api/auth/logout` y `GET /api/auth/me`.
- La cookie usa nombre específico del proyecto, `HttpOnly`, `SameSite=Lax`, expiración de 8 horas y sliding expiration.
- `Secure` es obligatorio en Production. En Development se permite la política de transporte de la request para facilitar pruebas locales.
- Los endpoints `/api` devuelven `401` cuando no hay sesión y `403` cuando falta permiso. No redirigen con `302` a `/login`.

Flujo de login:

1. Angular llama a `GET /api/auth/csrf`.
2. El backend emite cookie `XSRF-TOKEN` legible por JavaScript y cookie antiforgery interna.
3. Angular envía email y password a `POST /api/auth/login` con `withCredentials` y header `X-XSRF-TOKEN`.
4. El backend normaliza email, valida usuario activo/no bloqueado y verifica hash con `PasswordHasher<User>`.
5. Si las credenciales son correctas, emite cookie de sesión HttpOnly y claims de usuario, roles y permisos.
6. Angular renueva `GET /api/auth/csrf` porque el token inicial fue emitido para usuario anónimo.
7. Angular guarda el usuario en memoria del servicio, no tokens.
8. `GET /api/auth/me` rehidrata la sesión después de refrescar la página.

## Autorización

La autorización será basada en permisos, no solo en nombre de rol.

Rol inicial:

- Admin: tiene todos los permisos.

Permisos base definidos en `LaboratorioTlahuac.Domain.Security.Permissions`.

Claims emitidos al iniciar sesión:

- `NameIdentifier`
- `Email`
- `Name`
- `Role`
- `permission`, uno por permiso

Ejemplo:

```csharp
RequirePermission("orders.edit")
```

`RequirePermissionAttribute` y las policies validan claims `permission`. No se autoriza por `Role == Admin`.

## Reglas Iniciales

- El backend debe validar permisos en acciones protegidas.
- El frontend puede ocultar acciones no permitidas, pero no sustituye autorización de backend.
- Los permisos deben poder asignarse a roles futuros sin romper rutas o endpoints.
- El checker valida claims de permiso emitidos en la sesión.
- Cambios de permisos pueden requerir re-login para refrescar claims.
- Usuarios inactivos o bloqueados no pueden iniciar sesión; la API devuelve `423`.

## Angular

- `AuthService` implementa `login`, `logout`, `me`, `isAuthenticated` y `hasPermission`.
- `AuthGuard` protege `/app`.
- `PermissionGuard` valida el permiso declarado en la ruta.
- Si falta sesión, se redirige a `/login`.
- Si falta permiso, se redirige a `/app/access-denied`.
- No se usa `localStorage` ni `sessionStorage` para tokens.

## CSRF/XSRF

La autenticación por cookie requiere protección CSRF porque el navegador envía cookies automáticamente.

- El backend usa `IAntiforgery`.
- Header requerido: `X-XSRF-TOKEN`.
- Cookie legible por JavaScript: `XSRF-TOKEN`.
- La cookie de sesión sigue siendo HttpOnly.
- La cookie `XSRF-TOKEN` no es HttpOnly por diseño y no contiene la sesión.
- `SameSite=Lax` se usa para el MVP de mismo dominio.
- `Secure` es obligatorio en Production.

Endpoints protegidos por antiforgery:

- `POST /api/auth/login`
- `POST /api/auth/logout`
- Futuros `POST/PUT/PATCH/DELETE` bajo `/api`
- `POST /api/security/csrf-check` solo en Development

Exclusiones:

- `GET /health`
- `GET /api/auth/csrf`
- `GET /api/auth/me`
- Métodos seguros: `GET`, `HEAD`, `OPTIONS`, `TRACE`

Decisión sobre login:

`POST /api/auth/login` sí requiere CSRF. El token se obtiene antes del login con `GET /api/auth/csrf`. Esto reduce login CSRF y mantiene un flujo compatible con Angular.

Riesgos residuales:

- XSS podría leer `XSRF-TOKEN`; se debe seguir endureciendo salida HTML, dependencias y CSP cuando se agreguen pantallas más ricas.
- Cambios de identidad requieren renovar token XSRF; por eso Angular lo renueva después del login.
- El endpoint técnico `csrf-check` existe solo en Development.

## Criterios De Validación

- Un usuario no autenticado no debe acceder a `/app`.
- Admin recibe todos los permisos mediante seed inicial.
- Una acción protegida puede expresarse como permiso granular.
- Un request mutable bajo `/api` sin `X-XSRF-TOKEN` debe fallar.
