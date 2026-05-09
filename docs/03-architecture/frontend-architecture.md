# Arquitectura Frontend

## Implementación Actual

App Angular 21 con routing, SCSS, sesión real por cookie HttpOnly y guards funcionales en `src/LaboratorioTlahuac.Web`.

## Estructura Conceptual

- Rutas públicas: sitio institucional y login.
- Rutas privadas: módulos bajo `/app`.
- Layout público: navegación y contenido institucional.
- Layout privado: navegación operativa, sesión y módulos internos.
- Features: clientes, órdenes, pagos, inventario, proveedores, administración.
- Servicios API: encapsulan llamadas HTTP por dominio funcional.

## Estructura Real

```text
src/app/
  core/
    guards/
    http/
  shared/
  public/
  auth/
  admin/
  features/
```

## Rutas Implementadas

- `/`
- `/servicios`
- `/contacto`
- `/login`
- `/app/dashboard`
- `/app/ordenes`
- `/app/clientes`
- `/app/pagos`
- `/app/inventario`
- `/app/proveedores`
- `/app/admin/usuarios`
- `/app/admin/roles`
- `/app/access-denied`

## Estilo

Se eligió SCSS como formato de estilos para Angular. El diseño actual es mínimo y no representa pantallas finales.

## Seguridad En Frontend

- `AuthService` llama a `/api/auth/login`, `/api/auth/logout` y `/api/auth/me`.
- `AuthService` llama a `/api/auth/csrf` antes de requests mutables de auth.
- La sesión se guarda solo en memoria del servicio Angular.
- Las cookies HttpOnly viajan con `withCredentials`.
- `HttpClient` configura XSRF con cookie `XSRF-TOKEN` y header `X-XSRF-TOKEN`.
- `AuthService` además coloca `X-XSRF-TOKEN` explícitamente en login/logout para soportar desarrollo cross-origin con `apiBaseUrl`.
- No se guardan tokens en `localStorage` ni `sessionStorage`.
- `AuthGuard` protege `/app/*` y redirige a `/login` con `returnUrl`.
- `PermissionGuard` valida `data.permission` en cada ruta privada.
- Si falta permiso, navega a `/app/access-denied`.
- El layout privado muestra usuario autenticado y botón logout.
- El frontend puede ocultar navegación no permitida, pero la autorización real se valida en backend.

## Permisos Por Ruta

- `/app/dashboard`: `reports.view`
- `/app/ordenes`: `orders.view`
- `/app/clientes`: `customers.view`
- `/app/pagos`: `payments.view`
- `/app/inventario`: `inventory.view`
- `/app/proveedores`: `suppliers.view`
- `/app/admin/usuarios`: `users.manage`
- `/app/admin/roles`: `roles.manage`

## Configuración

- `src/environments/environment.development.ts`: `http://localhost:5277`.
- `src/environments/environment.ts`: mismo origen, sin URL final de producción hardcodeada.
- `withXsrfConfiguration`: `cookieName = XSRF-TOKEN`, `headerName = X-XSRF-TOKEN`.

## Flujo XSRF En AuthService

- `login`: pide CSRF como anónimo, envía login con `X-XSRF-TOKEN`, renueva CSRF ya autenticado y guarda usuario en memoria.
- `logout`: pide CSRF actual y envía logout con `X-XSRF-TOKEN`.
- `me`: no requiere CSRF porque es `GET`.
- La cookie `XSRF-TOKEN` se lee desde `document.cookie`; no se persiste en almacenamiento local.

## Criterios De Validación

- El sitio público puede navegarse sin sesión.
- Las rutas privadas redirigen a login si no hay sesión.
- Las acciones sensibles dependen de permisos, no solo de rol.

## Próximos Pasos

- Agregar pruebas frontend cuando se incorpore runner no interactivo.
- Implementar pantallas funcionales por módulo cuando inicie cada CRUD.
- Reutilizar el flujo XSRF para servicios mutables de clientes, órdenes y pagos.
