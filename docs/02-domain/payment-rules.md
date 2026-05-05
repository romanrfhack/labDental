# Reglas De Pagos

## Conceptos

- Total de la orden: importe acordado para el trabajo.
- Pago parcial: abono registrado contra una orden.
- Saldo calculado: total de la orden menos pagos vigentes.
- Pago cancelado: pago que queda trazable, pero no suma al total pagado vigente.

## Reglas Iniciales

- No se permiten pagos negativos.
- El saldo no se captura manualmente.
- Los pagos se registran como movimientos.
- La cancelación de pagos requiere trazabilidad de usuario, fecha y motivo.
- Los métodos de pago deben registrarse de forma estructurada cuando sea posible.
- Si los pagos vigentes superan el total, el caso se identifica como saldo a favor / revisar.

## Métodos De Pago Iniciales

- Efectivo.
- Transferencia.
- Tarjeta.
- Otro.

## Fórmula Conceptual

Saldo = total de la orden - suma de pagos vigentes.

## Criterios De Validación

- Un pago cancelado no afecta el saldo.
- Un sobrepago no se oculta; queda marcado para revisión.
- Los pagos del Excel migrado deben validarse antes de afectar saldos definitivos.
