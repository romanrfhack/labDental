# QA Usuarios, Roles Y Permisos — SEC-PERM-1

Última actualización: **2026-09-07**.

## Estado

La administración MVP de usuarios/roles ya estaba operativa en DEV. `SEC-PERM-1` amplía ese modelo con permisos editables por rol, overrides individuales por usuario y refresco de permisos de sesiones existentes.

Estado actual:

- validación automática de `codex/sec-perm-1`: **correcta**;
- migración EF: **generada y sincronizada**;
- validación manual del usuario limitado previo en DEV: **completada**;
- QA visual/operativa de la nueva UI de permisos: **pendiente después del merge/deploy a DEV**.

## Contrato De Permisos

Regla efectiva:

`permisos efectivos = unión de permisos de roles + Allow individuales - Deny individuales`

- `Heredado`: sin override individual.
- `Allow`: agrega permiso aunque el rol no lo otorgue.
- `Deny`: quita permiso aunque uno o más roles lo otorguen.
- Admin conserva todos los permisos y no puede degradarse desde la UI.

## Endpoints

| Endpoint | Permiso administrador | Comportamiento |
| --- | --- | --- |
| `GET /api/admin/users` | `users.manage` | lista usuarios |
| `GET /api/admin/users/{id}` | `users.manage` | detalle, roles y permisos efectivos |
| `POST /api/admin/users` | `users.manage` | crea usuario con roles; permisos se heredan |
| `PUT /api/admin/users/{id}` | `users.manage` | actualiza perfil |
| `PATCH /api/admin/users/{id}/status` | `users.manage` | activa/desactiva |
| `PATCH /api/admin/users/{id}/roles` | `users.manage` | sincroniza roles |
| `PUT /api/admin/users/{id}/permissions` | `users.manage` | sincroniza overrides `Allow/Deny` |
| `POST /api/admin/users/{id}/temporary-password` | `users.manage` | cambia contraseña temporal |
| `GET /api/admin/roles` | `roles.manage` | lista roles y conteos |
| `GET /api/admin/roles/{id}` | `roles.manage` | detalle y catálogo de permisos |
| `PUT /api/admin/roles/{id}/permissions` | `roles.manage` | sincroniza permisos de rol no protegido |

Los endpoints mutables conservan validación XSRF.

## Seguridad

### Admin

- rol Admin protegido;
- el seed garantiza todos los permisos conocidos;
- la UI no permite reducir permisos de Admin;
- la API rechaza intentos de reducción;
- usuarios con Admin no admiten overrides individuales degradantes;
- se conserva la regla de no dejar el sistema sin al menos un usuario activo con `users.manage`.

### Sesiones Existentes

La cookie contiene claims de roles/permisos, pero `OnValidatePrincipal` vuelve a resolver desde BD:

- usuario activo/bloqueado;
- roles;
- permisos de rol;
- overrides.

Si el principal ya no coincide, se reemplaza y la cookie se renueva. El nuevo permiso se usa antes de autorizar la solicitud actual.

El `permissionGuard` de Angular refresca `/api/auth/me` antes de resolver una navegación protegida.

### Repartidor Y Seed

- al crear `Repartidor` por primera vez, baseline: `deliveries.view` + `deliveries.complete`;
- después, el baseline seed no sobrescribe la configuración administrativa del rol;
- el seed explícito de Limited QA continúa siendo técnico y sólo Development.

### Contraseña Temporal

Sin cambio funcional en SEC-PERM-1:

- nunca se devuelve hash ni contraseña en API;
- Admin captura la contraseña temporal explícitamente;
- sigue pendiente para `PROD-READY-1` forzar cambio en primer login o aprobar política equivalente.

## Validación Automática Ejecutada

Workflow temporal `SEC-PERM-1 validation`:

- `dotnet restore`: correcto;
- `dotnet build --configuration Release`: correcto;
- `dotnet test --configuration Release`: correcto;
- `npm ci`: correcto;
- `npm run build`: correcto;
- `dotnet ef migrations add AddUserPermissionOverrides`: correcto;
- migración `.cs`, `.Designer.cs` y `LaboratorioTlahuacDbContextModelSnapshot.cs` generados por EF;
- `dotnet ef migrations has-pending-model-changes`: correcto.

Pruebas específicas agregadas:

1. grant de permiso por rol aplicado a sesión ya abierta;
2. revoke por rol aplicado a la misma sesión;
3. `Allow` individual aplicado a sesión existente;
4. `Deny` individual prevalece sobre permiso heredado;
5. `/api/auth/me` refleja permisos efectivos;
6. rol Admin no puede reducirse;
7. usuario Admin no puede degradarse por override;
8. baseline seed no revierte permisos editados de Repartidor.

## Evidencia Manual De Usuario Limitado Ya Ejecutada

En DEV, 2026-09-07:

- login real correcto;
- `customers.view=true`;
- `reports.view=false`;
- `/app/clientes` carga;
- `/app/dashboard -> /app/access-denied`;
- `/api/customers` autenticado: `200`;
- `/api/dashboard/summary` autenticado: `403`;
- `/api/dashboard/summary` sin credenciales: `401`;
- logout: `/api/auth/me` devuelve `401`;
- seed temporal retirado del ambiente.

## Checklist Manual Después Del Deploy DEV De SEC-PERM-1

### Rol

1. Iniciar sesión como Admin.
2. Abrir `/app/admin/roles`.
3. Confirmar que Admin aparece protegido y no editable.
4. Seleccionar Repartidor.
5. Agregar temporalmente un permiso no sensible de prueba, por ejemplo `customers.view`.
6. Guardar y confirmar aviso de usuarios afectados.
7. Con usuario Repartidor ya autenticado, confirmar que el nuevo permiso surte efecto sin relogin.
8. Retirar el permiso y confirmar que se pierde sin relogin.
9. Restaurar el conjunto operativo acordado del rol.

### Usuario

10. Abrir `/app/admin/usuarios` y crear/usar un usuario QA no Admin.
11. Confirmar que sus permisos iniciales coinciden con los roles y no son copias individuales.
12. Verificar que cada permiso muestra estado efectivo y rol de origen.
13. Aplicar `Allow` a un permiso no heredado y validar acceso.
14. Volver a `Heredado` y validar que desaparece la excepción.
15. Aplicar `Deny` a un permiso heredado y validar rechazo.
16. Volver a `Heredado` y validar recuperación por rol.
17. Confirmar que usuario Admin muestra overrides bloqueados.

### Regresión

18. Confirmar `/api/...` sin sesión devuelve `401`.
19. Confirmar sesión sin permiso devuelve `403`.
20. Confirmar menú/rutas Angular coinciden con `/api/auth/me`.
21. Confirmar Clientes ya no muestra una segunda tabla sin formato en desktop.
22. Revisar consola del navegador sin errores nuevos.

## Criterio De Salida

SEC-PERM-1 puede cerrarse cuando:

- PR a `dev` integrado;
- migración aplicada por flujo DEV;
- health público/local correcto;
- checklist manual esencial de rol y override de usuario pasa;
- Clientes duplicado corregido visualmente;
- sin regresiones `401/403`;
- evidencia de DEV registrada.
