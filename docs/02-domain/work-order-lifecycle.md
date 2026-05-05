# Ciclo De Vida De Orden De Trabajo

## Estados Sugeridos

- Recibida
- En proceso
- En primera prueba
- En segunda prueba
- Lista para entrega
- Entregada
- Cancelada

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

- Cancelada es estado terminal operativo.
- Una orden cancelada no debe editarse libremente sin permiso especial.
- Entregada no implica saldo liquidado.
- El cambio de estado debe registrar usuario, fecha y estado anterior.
- Las transiciones pueden ampliarse cuando se valide el flujo real con el cliente.

## Criterios De Validación

- El historial permite reconstruir cambios de estado.
- Los estados no se usan para representar pagos o saldos.
- Las excepciones requieren permiso documentado.
