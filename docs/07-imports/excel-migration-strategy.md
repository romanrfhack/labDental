# Estrategia De Migración Del Excel

## Principios

- No migrar sin validación.
- Importar en modo revisión.
- Mantener observaciones originales.
- No confiar ciegamente en saldos manuales.
- Recalcular saldos desde pagos detectados cuando sea posible.

## Proceso Propuesto

1. Respaldar archivo original.
2. Analizar hojas y columnas.
3. Mapear hojas a clientes.
4. Mapear filas a órdenes.
5. Detectar pagos numéricos y notas financieras.
6. Importar registros a revisión.
7. Resolver inconsistencias manualmente.
8. Confirmar datos antes de marcarlos como operativos.

## Manejo De Inconsistencias

- Registrar motivo de inconsistencia.
- Conservar texto original.
- Permitir revisión manual antes de afectar saldos.
- Evitar correcciones automáticas no verificables.

## Criterios De Validación

- La migración no modifica registros operativos nuevos sin aprobación.
- Los saldos recalculados son comparables contra el Excel, pero no sustituyen revisión.
- Cada registro importado conserva trazabilidad al origen.
