# Reglas De Pagos

## Conceptos

- `TotalAmount`: importe total acordado en la orden de trabajo.
- `Payment`: movimiento financiero registrado contra una orden.
- `PaidAmount`: suma de pagos no cancelados.
- `Balance`: saldo calculado.
- `PaymentStatus`: estado financiero calculado.

## Métodos De Pago

Valores internos y etiquetas:

- `Cash`: Efectivo.
- `BankTransfer`: Transferencia.
- `Card`: Tarjeta.
- `Other`: Otro.

## Fórmulas

- `PaidAmount = suma(Payment.Amount donde IsCancelled = false)`.
- `Balance = TotalAmount - PaidAmount`.
- Si `TotalAmount` es `null`, `Balance = null`.

## PaymentStatus

- `TotalNotSet`: `TotalAmount` no está definido.
- `Unpaid`: `TotalAmount` está definido, es mayor a 0 y `PaidAmount = 0`.
- `Partial`: `PaidAmount > 0` y menor que `TotalAmount`.
- `Paid`: `PaidAmount = TotalAmount`, o `TotalAmount = 0` y `PaidAmount = 0`.
- `Overpaid`: `PaidAmount > TotalAmount`.

Etiqueta de `Overpaid`: "Saldo a favor / revisar".

## Registro

- La orden debe existir.
- `WorkOrder.TotalAmount` debe estar definido.
- La orden no debe estar `Cancelled`.
- `PaymentDate` es obligatorio.
- `Amount` debe ser mayor a 0.
- `Method` debe ser un método válido.
- `Reference` es opcional y máximo 100 caracteres.
- `Notes` es opcional y máximo 1000 caracteres.
- El sobrepago se permite y se refleja como `Overpaid`.

## Cancelación

- No hay delete físico de pagos.
- No se editan pagos libremente en el MVP.
- Un pago se corrige cancelando con motivo y registrando otro pago si aplica.
- `CancellationReason` es obligatorio y máximo 1000 caracteres.
- No se puede cancelar dos veces el mismo pago.
- Un pago cancelado no cuenta para `PaidAmount` ni `Balance`.
- La cancelación conserva `CancelledAtUtc` y `CancelledByUserId`.

## Permisos

- `payments.view`: consultar pagos, métodos y resúmenes financieros.
- `payments.create`: registrar pagos/abonos.
- `payments.cancel`: cancelar pagos con motivo.
- `orders.view` no debe exponer saldos detallados sin `payments.view`.

## Restricciones De Alcance

- No hay cortes de caja avanzados.
- No hay facturación ni CFDI.
- No hay reportes avanzados de cobranza.
- No hay migración del Excel en esta etapa.

## Criterios De Validación

- Un pago cancelado no afecta el saldo.
- Un sobrepago no se oculta; queda marcado para revisión.
- El saldo se calcula y no se captura manualmente.
- Los endpoints mutables requieren XSRF y permisos.
