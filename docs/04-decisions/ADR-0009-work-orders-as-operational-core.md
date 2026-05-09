# ADR-0009: Modelar órdenes de trabajo como núcleo operativo del sistema

## Estado

Aceptada para MVP.

## Contexto

El Excel actual registra trabajos dentales por doctor o clínica, incluyendo paciente, trabajo, color, pruebas, entrega, costo y observaciones. El sistema debe sustituir ese flujo sin replicar la estructura de hojas por doctor.

## Decisión

Crear `WorkOrder` como entidad central relacionada con `Customer` e `InternalDoctor` opcional. La orden tendrá estado operativo, fechas de trabajo, costo total opcional e historial de cambios de estado. Los pagos y saldos se implementarán en una etapa posterior.

## Consecuencias Positivas

- Representa el flujo principal del laboratorio.
- Permite dejar de usar Excel para nuevos trabajos.
- Deja preparada la etapa de pagos.
- Permite seguimiento por estado y fecha de entrega.
- Conserva trazabilidad de cambios de estado.

## Consecuencias Negativas

- Requiere validar relación `Customer`/`InternalDoctor`.
- El folio generado no será necesariamente secuencial en MVP.
- Pagos/saldos aún no estarán disponibles hasta la siguiente etapa.
- Cambios de estado requieren disciplina operativa.

## Alternativas Consideradas

- Crear una tabla por doctor.
- Usar solo notas libres sin entidad `WorkOrder`.
- Implementar pagos junto con órdenes en la misma etapa.
- Usar folio secuencial desde el inicio.
