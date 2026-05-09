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

## Criterios De Validación

- Admin puede ejecutar todas las acciones del MVP.
- Los permisos existen como contrato conceptual desde el diseño inicial.
- Una futura creación de roles no debe exigir cambios breaking en endpoints o rutas.
