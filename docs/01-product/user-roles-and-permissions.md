# Roles Y Permisos

## Rol Inicial

El rol inicial confirmado es Admin. Admin tiene todos los permisos disponibles en el sistema mediante seed inicial.

Aunque al inicio solo exista Admin, el sistema debe diseñarse con permisos granulares para permitir roles futuros sin romper contratos ni reescribir reglas de autorización.

## Permisos Confirmados

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
- `inventory.view`
- `inventory.create`
- `inventory.adjust`
- `suppliers.view`
- `suppliers.create`
- `users.manage`
- `roles.manage`
- `reports.view`

## Reglas Iniciales

- Los permisos deben validar acciones, no solo pantallas.
- Las rutas privadas deben requerir autenticación.
- Las acciones sensibles deben requerir permiso explícito.
- El nombre del rol no debe ser la única fuente de autorización.
- El seed inicial asigna todos los permisos al rol Admin sin duplicarlos.
- Roles futuros podrán recibir subconjuntos de permisos.

## Uso En Clientes

- `customers.view`: permite listar y consultar clientes y doctores internos.
- `customers.create`: permite crear clientes y doctores internos para clínicas.
- `customers.edit`: permite editar, activar y desactivar clientes y doctores internos.
- La autorización se valida por permiso, no por `Role == Admin`.

## Uso En Órdenes

- `orders.view`: permite listar, filtrar y consultar detalle de órdenes.
- `orders.create`: permite crear órdenes nuevas.
- `orders.edit`: permite editar datos generales de una orden no cancelada.
- `orders.changeStatus`: permite cambiar estado operativo de una orden y crear historial.
- `orders.delete`: reservado para futuro; no habilita delete físico en el MVP.
- La autorización se valida por permiso, no por `Role == Admin`.

## Uso En Pagos

- `payments.view`: permite consultar pagos, listados globales y resúmenes financieros calculados.
- `payments.create`: permite registrar pagos y abonos sobre órdenes con `TotalAmount` definido y no canceladas.
- `payments.cancel`: permite cancelar pagos con motivo; no habilita delete físico ni edición libre.
- `orders.view` permite consultar órdenes, pero no expone saldos detallados; la UI consume endpoints de pagos protegidos por `payments.view`.
- La autorización se valida por permiso, no por `Role == Admin`.

## Uso En Dashboard

- `reports.view`: permite acceder a `/app/dashboard` y `GET /api/dashboard/summary`.
- `orders.view`: permite recibir y visualizar la sección operativa del dashboard.
- `payments.view`: permite recibir y visualizar la sección financiera del dashboard.
- `customers.view`: permite recibir y visualizar la sección básica de clientes del dashboard.
- `reports.view` no implica acceso automático a operación, cobranza ni clientes.
- El backend devuelve como `null` las secciones para las que el usuario no tiene permiso.
- La autorización se valida por permiso, no por `Role == Admin`.

## Criterios De Validación

- Admin puede ejecutar todas las acciones del MVP.
- Los permisos existen como contrato conceptual desde el diseño inicial.
- Una futura creación de roles no debe exigir cambios breaking en endpoints o rutas.
