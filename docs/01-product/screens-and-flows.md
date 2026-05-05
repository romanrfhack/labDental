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
- `/app/clientes`
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
- Inventario y proveedores pueden quedar visibles como módulos futuros o deshabilitados si están fuera del MVP.

## Criterios De Validación

- El flujo principal se puede completar sin cambiar de sistema.
- La vista de detalle de orden concentra estado operativo, pagos y saldo.
- El usuario entiende qué órdenes requieren prueba, entrega o cobranza.
