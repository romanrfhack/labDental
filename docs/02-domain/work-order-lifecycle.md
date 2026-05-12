# Ciclo De Vida De Orden De Trabajo

## Estados Implementados

| Valor interno | Etiqueta UI |
| --- | --- |
| `Received` | Recibida |
| `InProcess` | En proceso |
| `FirstTrial` | En primera prueba |
| `SecondTrial` | En segunda prueba |
| `ReadyForDelivery` | Lista para entrega |
| `Delivered` | Entregada |
| `Cancelled` | Cancelada |

## Transiciones Válidas Iniciales

- Recibida -> En proceso
- En proceso -> En primera prueba
- En primera prueba -> En proceso
- En proceso -> En segunda prueba
- En segunda prueba -> En proceso
- En proceso -> Lista para entrega
- En primera prueba -> Lista para entrega
- En segunda prueba -> Lista para entrega
- Lista para entrega -> Entregada
- Cualquier estado no terminal -> Cancelada

## Reglas

- El estado inicial al crear una orden es `Received`.
- Cancelada es estado terminal operativo.
- Una orden cancelada no se edita en datos generales durante el MVP.
- Una orden cancelada no cambia a otro estado durante el MVP.
- Cancelar requiere nota.
- Entregada no implica saldo liquidado.
- `Delivered` y `Cancelled` no cuentan como órdenes vencidas en el dashboard.
- `Cancelled` se excluye de operación activa en el dashboard.
- Todo cambio real de estado debe registrar usuario, fecha, estado anterior y estado nuevo.
- Cambiar al mismo estado devuelve `200 OK` sin duplicar historial.
- Las transiciones pueden ampliarse cuando se valide el flujo real con el cliente.

## Criterios De Validación

- El historial permite reconstruir cambios de estado.
- Los estados no se usan para representar pagos o saldos.
- Las excepciones requieren permiso documentado.
