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
- `/app/dashboard` requiere `reports.view`.
- Las secciones del dashboard se muestran según permisos adicionales: operación con `orders.view`, cobranza con `payments.view` y clientes con `customers.view`.
- `/app/pagos` requiere `payments.view`.
- La sección de pagos dentro de `/app/ordenes/:id` solo se muestra con `payments.view`.
- Inventario y proveedores pueden quedar visibles como módulos futuros o deshabilitados si están fuera del MVP.

## Dashboard

`/app/dashboard` es el dashboard real básico del MVP. Consume `GET /api/dashboard/summary` y muestra:

- Métricas operativas si el usuario tiene `orders.view`.
- Métricas financieras si el usuario tiene `payments.view`.
- Métricas básicas de clientes si el usuario tiene `customers.view`.
- Conteo de órdenes por estado.
- Últimas órdenes.
- Próximas entregas.
- Últimos pagos.

Si una sección no llega en la respuesta por falta de permiso, la UI muestra un mensaje de acceso limitado para esa sección. No incluye gráficas complejas, exportaciones, reportes avanzados ni cortes de caja.

## Pagos

`/app/pagos` permite consultar pagos registrados con:

- Búsqueda.
- Filtro por método.
- Filtro por rango de fecha de pago.
- Toggle para incluir pagos cancelados.
- Columnas de orden, cliente, paciente, fecha, monto, método, referencia y estado cancelado.

## Detalle De Orden

`/app/ordenes/:id` concentra información operativa de la orden. Si el usuario tiene `payments.view`, también muestra:

- Total de la orden.
- Pagado.
- Saldo.
- Estado financiero calculado.
- Lista de pagos.
- Formulario de registro si tiene `payments.create`, la orden no está cancelada y `TotalAmount` está definido.
- Acción de cancelación si tiene `payments.cancel`.

Si `TotalAmount` es `null`, la UI muestra: "Define el total de la orden antes de registrar pagos." Si la orden está `Cancelled`, no se permite registrar pagos.

## Criterios De Validación Del MVP

- El flujo principal se puede completar sin cambiar de sistema para nuevos registros del MVP.
- La vista de detalle de orden concentra datos operativos, estado e historial.
- El usuario entiende qué órdenes requieren prueba, entrega o cobranza.
- Los pagos cancelados no cuentan para saldo y los sobrepagos se marcan como "Saldo a favor / revisar".
