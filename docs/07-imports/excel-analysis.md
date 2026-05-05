# Análisis Del Excel

## Hallazgos Conocidos

- Excel con múltiples hojas por doctor/cliente.
- Campos principales: fecha, paciente, trabajo, folio/nota, color, prueba 1, prueba 2, entrega, costo, abonado, restante, observaciones.
- Datos inconsistentes.
- Pagos mezclados como texto libre.
- No hay inventario ni proveedores estructurados.

## Riesgos

- Hojas con columnas diferentes.
- Saldos calculados manualmente o desactualizados.
- Observaciones que mezclan datos operativos y financieros.
- Pacientes repetidos sin identificador único.

## Criterios De Validación

- No asumir que todas las hojas tienen la misma estructura.
- No usar el saldo manual como fuente única de verdad.
- Preservar observaciones originales para auditoría.

## Próximos Pasos

- Obtener muestra real del Excel.
- Identificar patrones por hoja.
- Definir reglas de importación y revisión.
