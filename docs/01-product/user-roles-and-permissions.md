# Roles Y Permisos

Última actualización: **2026-09-07 — SEC-PERM-1**.

## Modelo De Autorización

El sistema utiliza autorización basada en permisos granulares. El backend es la fuente autoritativa y el frontend usa los permisos efectivos para navegación, rutas y visibilidad de acciones.

Un usuario puede tener uno o más roles. **Los permisos de los roles no se copian al usuario**: se heredan dinámicamente.

La regla de permisos efectivos es:

`permisos efectivos = unión de permisos de roles + Allow individuales - Deny individuales`

Los overrides individuales existen únicamente como excepciones. El estado normal y recomendado es **Heredado**.

## Rol Admin

Admin es un rol protegido del sistema:

- tiene todos los permisos conocidos;
- el seed garantiza que reciba permisos nuevos cuando el catálogo de permisos crece;
- sus permisos no pueden reducirse desde la UI;
- los usuarios que pertenezcan a Admin no admiten overrides individuales que degraden ese conjunto;
- Admin no depende del nombre del rol para proteger endpoints: los endpoints siguen autorizándose por permisos.

## Roles Operativos

Los roles no protegidos pueden configurarse desde `/app/admin/roles`.

Al cambiar permisos de un rol:

- todos sus usuarios heredan el nuevo conjunto;
- no se crean copias de permisos en cada usuario;
- el cambio se refleja en sesiones existentes en la siguiente solicitud autenticada;
- la UI debe advertir cuántos usuarios están asociados al rol.

### Repartidor

Permisos iniciales al crear el rol por primera vez:

- `deliveries.view`
- `deliveries.complete`

A partir de SEC-PERM-1, el baseline seed **no sobrescribe** posteriormente una configuración administrativa de Repartidor. El baseline sólo establece ese conjunto al crear el rol. Esto permite administrar el rol desde la UI sin que un reinicio de Development revierta el cambio.

### Limited QA

Rol técnico usado para QA en Development. El seed específico de Limited QA sólo se ejecuta cuando se habilita explícitamente y continúa pudiendo sincronizar el conjunto solicitado para una prueba controlada.

No debe considerarse un rol operativo de producción.

## Overrides Individuales

Persistencia: `Security.UserPermissionOverrides`.

Cada registro identifica:

- `UserId`;
- `PermissionId`;
- `Effect`: `Allow` o `Deny`.

En la UI de usuario cada permiso se presenta con tres opciones:

- **Heredado**: no existe override; se usa el resultado de los roles.
- **Permitir**: agrega el permiso aunque ningún rol lo otorgue.
- **Denegar**: elimina el permiso efectivo aunque algún rol lo otorgue.

La UI debe mostrar además:

- si el permiso es efectivo;
- si se hereda;
- qué rol o roles lo originan;
- la clave técnica del permiso como información secundaria.

## Actualización De Sesión

Los permisos se emiten como claims de la cookie de autenticación, pero no permanecen obsoletos hasta el siguiente login.

En cada solicitud autenticada, la validación de la cookie consulta el estado de seguridad persistido del usuario:

- activo/bloqueado;
- roles vigentes;
- permisos de roles;
- overrides individuales.

Si roles o permisos difieren de los claims actuales, el principal se reemplaza y la cookie se renueva. La autorización de la misma solicitud utiliza ya el principal actualizado.

El `permissionGuard` de Angular refresca `/api/auth/me` antes de permitir una ruta protegida por permiso, para que navegación y menú se alineen con el backend sin requerir logout/login.

## Permisos Confirmados

El catálogo vigente se define en `Domain/Security/Permissions.cs`. Entre los permisos principales están:

- `orders.view`
- `orders.create`
- `orders.edit`
- `orders.delete`
- `orders.changeStatus`
- `payments.view`
- `payments.create`
- `payments.cancel`
- `customers.view`
- `customers.create`
- `customers.edit`
- `deliveries.view`
- `deliveries.assign`
- `deliveries.update`
- `deliveries.complete`
- `inventory.view`
- `inventory.create`
- `inventory.adjust`
- `suppliers.view`
- `suppliers.create`
- `users.manage`
- `roles.manage`
- `reports.view`
- `catalog.view`
- `catalog.manage`

La lista de código es la fuente de verdad si se agregan permisos posteriores.

## Reglas De Seguridad

- Los permisos validan acciones, no sólo pantallas.
- Las rutas privadas requieren autenticación.
- Las acciones sensibles requieren permiso explícito.
- El nombre del rol no es la fuente de autorización de endpoints.
- El backend nunca confía sólo en ocultar controles del frontend.
- Una edición de permisos no debe dejar al sistema sin al menos un usuario activo con `users.manage`.
- Admin no puede degradarse mediante edición de rol u overrides individuales.
- Cambios de permisos deben surtir efecto en sesiones ya abiertas.
- Password hashes y contraseñas temporales nunca se devuelven por API.

## Uso En Clientes

- `customers.view`: listar y consultar clientes y doctores internos.
- `customers.create`: crear clientes y doctores internos para clínicas.
- `customers.edit`: editar, activar y desactivar clientes y doctores internos.

## Uso En Órdenes

- `orders.view`: listar, filtrar y consultar detalle de órdenes.
- `orders.create`: crear órdenes nuevas.
- `orders.edit`: editar datos generales de una orden no cancelada.
- `orders.changeStatus`: cambiar estado operativo de una orden y crear historial.
- `orders.delete`: reservado para futuro; no habilita delete físico en el MVP.

## Uso En Pagos

- `payments.view`: consultar pagos, listados globales y resúmenes financieros calculados.
- `payments.create`: registrar pagos y abonos.
- `payments.cancel`: cancelar pagos con motivo; no habilita delete físico ni edición libre.

## Uso En Dashboard

- `reports.view`: acceder a `/app/dashboard` y `GET /api/dashboard/summary`.
- `orders.view`: recibir sección operativa del dashboard.
- `payments.view`: recibir sección financiera.
- `customers.view`: recibir sección básica de clientes.
- `reports.view` no implica acceso automático a operación, cobranza ni clientes.
- El backend devuelve `null` para secciones internas que el usuario no puede consultar.

## Criterios De Validación SEC-PERM-1

- Crear un usuario con un rol y comprobar herencia sin copiar permisos.
- Agregar/quitar un permiso de rol y comprobar efecto en una sesión ya iniciada.
- Agregar `Allow` individual y comprobar acceso.
- Aplicar `Deny` sobre un permiso heredado y comprobar rechazo.
- Confirmar que `/api/auth/me` refleja el conjunto efectivo actualizado.
- Confirmar que Admin no puede degradarse.
- Confirmar que el baseline seed no revierte permisos administrados de Repartidor.
- Mantener respuestas API `401` sin sesión y `403` sin permiso.
