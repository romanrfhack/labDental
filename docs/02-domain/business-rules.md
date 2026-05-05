# Reglas De Negocio

## Reglas Iniciales

- La orden de trabajo es la entidad central.
- El cliente puede ser doctor, doctora, clínica u otro.
- Una clínica puede tener doctores internos.
- El saldo no se captura manualmente; se calcula.
- El estado operativo de la orden no debe mezclarse con el estado financiero.
- Los pagos se registran como movimientos.
- Los cambios relevantes deben dejar trazabilidad.

## Separación Operativa Y Financiera

El estado operativo describe avance del trabajo dental. El estado financiero se deriva del total, pagos vigentes, cancelaciones y saldo. Una orden puede estar entregada y tener saldo pendiente.

## Trazabilidad

Debe existir historial para cambios relevantes, especialmente cambios de estado, pagos, cancelaciones y ajustes futuros de inventario.

## Criterios De Validación

- Ningún saldo se guarda como valor manual autoritativo.
- Los pagos no modifican directamente el total de la orden.
- Las reglas nuevas se documentan antes o junto con su implementación.
