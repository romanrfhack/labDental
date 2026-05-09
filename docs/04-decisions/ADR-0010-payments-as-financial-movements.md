# ADR-0010: Modelar pagos como movimientos financieros asociados a órdenes de trabajo

## Estado

Aceptada para MVP.

## Contexto

El Excel actual registra costos, abonos y saldos en columnas, a veces mezclando notas de pago en texto libre. El sistema debe calcular saldos de forma confiable y conservar trazabilidad.

## Decisión

Crear `Payment` como movimiento asociado a `WorkOrder`. Los pagos no se editan ni se eliminan físicamente; se cancelan con motivo. `PaidAmount`, `Balance` y `PaymentStatus` se calculan a partir de `WorkOrder.TotalAmount` y pagos no cancelados.

## Consecuencias Positivas

- Evita saldos capturados manualmente.
- Permite múltiples abonos por orden.
- Conserva trazabilidad de cancelaciones.
- Facilita reportes futuros de cobranza.
- Separa estado operativo de estado financiero.

## Consecuencias Negativas

- Requiere capturar `TotalAmount` antes de registrar pagos.
- Correcciones requieren cancelar y volver a registrar.
- Sobrepagos requieren revisión operativa.
- Los saldos históricos dependen de que los pagos estén correctamente capturados.

## Alternativas Consideradas

- Guardar saldo como campo editable.
- Agregar columnas `Abono1`/`Abono2`/`Abono3`.
- Permitir edición libre de pagos.
- Implementar cortes de caja en esta misma etapa.
