# Reglas De Inventario

El inventario será básico o quedará fuera del MVP según el roadmap. No debe bloquear el flujo inicial de órdenes, clientes y pagos.

## Conceptos

- Material: insumo usado por el laboratorio.
- Entrada: incremento de existencia por compra, devolución o ajuste positivo.
- Salida: decremento de existencia por uso, venta, devolución o ajuste negativo.
- Ajuste: corrección manual justificada.
- Merma: pérdida o desperdicio registrado.
- Stock mínimo: nivel que dispara alerta o revisión.

## Reglas Iniciales

- Todo movimiento debe tener tipo, cantidad, fecha y usuario.
- No se deben crear salidas automáticas por orden en el MVP salvo decisión posterior.
- Los ajustes deben requerir observación.
- El stock mínimo no bloquea operación; genera alerta.

## Criterios De Validación

- Inventario no se mezcla con pagos ni estados de orden.
- Los movimientos permiten auditar cambios de existencia.
- La incorporación de inventario debe ser incremental y no breaking.
