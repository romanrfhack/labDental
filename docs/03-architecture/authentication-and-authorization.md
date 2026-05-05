# Autenticación Y Autorización

## Autenticación

- El sitio público no requiere autenticación.
- `/login` permite iniciar sesión.
- `/app/*` requiere usuario autenticado.
- El mecanismo final será JWT o cookies seguras; la decisión queda pendiente.

## Autorización

La autorización será basada en permisos, no solo en nombre de rol.

Rol inicial:

- Admin: tiene todos los permisos.

Ejemplo conceptual:

```csharp
RequirePermission("orders.edit")
```

## Reglas Iniciales

- El backend debe validar permisos en acciones protegidas.
- El frontend puede ocultar acciones no permitidas, pero no sustituye autorización de backend.
- Los permisos deben poder asignarse a roles futuros sin romper rutas o endpoints.

## Criterios De Validación

- Un usuario no autenticado no accede a `/app`.
- Admin puede ejecutar todas las acciones del MVP.
- Una acción protegida puede expresarse como permiso granular.
