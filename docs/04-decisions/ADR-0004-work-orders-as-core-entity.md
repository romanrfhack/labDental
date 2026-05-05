# ADR-0004: Orden De Trabajo Como Entidad Central

## Estado

Aceptada.

## Contexto

El Excel actual organiza información por doctor, doctora o clínica, pero las filas representan principalmente trabajos dentales con fechas, pacientes, costos, abonos, saldos y observaciones.

## Decisión

La orden de trabajo dental será la entidad central del sistema.

## Motivo

El Excel evidencia que el flujo principal es seguimiento operativo de trabajos, pagos y entregas.

## Consecuencias

- Clientes, doctores, pagos e inventario se relacionan alrededor de órdenes.
- El detalle de orden debe concentrar estado operativo, fechas, pagos y saldo.
- Reportes futuros deben partir de órdenes como unidad operativa principal.
