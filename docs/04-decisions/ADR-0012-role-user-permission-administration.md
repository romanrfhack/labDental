# ADR-0012: Administración De Permisos Por Rol Y Overrides Por Usuario

## Estado

Aceptada — 2026-09-07.

## Contexto

El sistema ya autorizaba acciones mediante permisos granulares heredados de roles. La administración UI permitía crear usuarios y asignar roles, pero los permisos de los roles eran de sólo lectura y no existían excepciones por usuario.

Se requiere:

- editar permisos por rol;
- mantener herencia automática al crear/asignar usuarios;
- permitir excepciones individuales sin copiar todo el conjunto del rol;
- evitar que una cookie existente conserve privilegios obsoletos después de un cambio administrativo;
- proteger el rol Admin contra pérdida accidental de privilegios administrativos.

## Decisión

### Herencia

No copiar permisos del rol al usuario.

Los permisos base siguen obteniéndose dinámicamente de `UserRole -> RolePermission -> Permission`.

### Overrides

Agregar `Security.UserPermissionOverrides` con clave `(UserId, PermissionId)` y `Effect`:

- `Allow`;
- `Deny`.

Regla efectiva:

`roles + Allow - Deny`.

La ausencia de registro significa `Heredado`.

### Admin

Admin es un rol protegido:

- recibe todos los permisos conocidos;
- no puede reducirse desde la UI/API de edición de rol;
- un usuario Admin no puede ser degradado mediante overrides individuales.

La autorización de endpoints continúa basada en permisos y no en `Role == Admin`.

### Sesión

Los permisos continúan representados como claims de la cookie, pero en cada solicitud autenticada `OnValidatePrincipal` consulta el grafo de seguridad actual del usuario. Si roles/permisos/estado cambiaron, reemplaza el principal y renueva la cookie antes de autorización.

Esto prioriza revocación inmediata sobre ahorro de una consulta de seguridad por request. Si el volumen futuro lo exige, puede evolucionarse a un `SecurityVersion`/stamp con caché sin cambiar el contrato de permisos efectivos.

### Seed

- Admin continúa recibiendo permisos nuevos por baseline.
- Repartidor recibe su conjunto inicial sólo cuando se crea por primera vez; un baseline posterior no sobrescribe una configuración administrada.
- Limited QA conserva su seed explícito, Development-only, porque es un mecanismo técnico de pruebas.

## Consecuencias

Positivas:

- roles siguen siendo mantenibles y centralizados;
- excepciones individuales son visibles y mínimas;
- revocaciones no esperan logout/login;
- backend sigue siendo fuente autoritativa;
- no se depende de ocultar controles de UI.

Costos:

- cada solicitud autenticada revalida el grafo de seguridad en BD;
- la UI necesita explicar origen/efecto para evitar configuraciones confusas;
- existe una nueva migración de seguridad.

## Reglas De Seguridad

- no dejar el sistema sin al menos un usuario activo con `users.manage`;
- Admin no puede degradarse;
- `Deny` prevalece sobre herencia para usuarios no Admin;
- los cambios mutables siguen protegidos por XSRF;
- `401` significa ausencia/sesión inválida y `403` significa autenticado sin permiso.

## Alternativas Rechazadas

### Copiar permisos del rol al crear cada usuario

Rechazada porque crea snapshots obsoletos: modificar el rol dejaría usuarios desincronizados.

### Sólo permisos por usuario

Rechazada porque elimina el valor operativo de los roles y aumenta la carga administrativa.

### Mantener claims hasta el siguiente login

Rechazada porque una revocación administrativa no sería efectiva de inmediato.
