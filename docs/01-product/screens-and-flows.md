# Pantallas Y Flujos

## Rutas Públicas

- `/`
- `/servicios`
- `/contacto`
- `/login`

## Rutas Privadas

- `/app/dashboard`
- `/app/ordenes`
- `/app/ordenes/nueva`
- `/app/ordenes/:id`
- `/app/ordenes/:id/editar`
- `/app/clientes`
- `/app/clientes/nuevo`
- `/app/clientes/:id`
- `/app/clientes/:id/editar`
- `/app/pagos`
- `/app/inventario`
- `/app/proveedores`
- `/app/admin/usuarios`
- `/app/admin/roles`

## Flujo Principal

Cliente/Doctor -> Orden de trabajo -> Pruebas -> Entrega -> Pagos -> Saldo.

## Reglas De Navegación

- Las rutas públicas no requieren autenticación.
- `/app/*` requiere login.
- Las rutas administrativas requieren permisos específicos.
- Las rutas de órdenes requieren `orders.view`, `orders.create` u `orders.edit` según la acción.
- Las rutas de clientes requieren `customers.view`, `customers.create` o `customers.edit` según la acción.
- Inventario y proveedores pueden quedar visibles como módulos futuros o deshabilitados si están fuera del MVP.

## Criterios De Validación Del MVP

- El flujo principal se podrá completar sin cambiar de sistema cuando pagos quede implementado.
- La vista de detalle de orden concentra datos operativos, estado e historial.
- El usuario entiende qué órdenes requieren prueba, entrega o cobranza.
- Pagos, abonos y saldos se agregarán en una etapa posterior.
