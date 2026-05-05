# Contexto De Negocio

Laboratorio Dental Tláhuac es un laboratorio dental que administra trabajos solicitados por doctores, doctoras, clínicas y otros clientes relacionados.

Actualmente la operación se controla principalmente con un archivo Excel. Cada hoja representa en su mayoría a un doctor, doctora o clínica. El archivo concentra pacientes, trabajos dentales, fechas, pruebas, entregas, costos, abonos, saldos y observaciones.

El flujo principal del negocio no es solo mantener un catálogo de clientes. La operación gira alrededor del seguimiento de trabajos dentales desde la recepción hasta la entrega, junto con su estado financiero.

## Problemas A Resolver

- Reducir captura manual duplicada.
- Evitar errores en saldos calculados manualmente.
- Disminuir dependencia de un archivo Excel único.
- Separar seguimiento operativo de seguimiento financiero.
- Facilitar búsqueda, trazabilidad y continuidad de operación.

## Criterios De Validación

- El sistema se diseña alrededor de órdenes de trabajo.
- La información histórica del Excel se trata como fuente útil, pero no perfecta.
- Los nuevos registros deben poder operarse sin Excel en el MVP.

## Próximos Pasos

- Revisar el Excel real y confirmar variaciones entre hojas.
- Validar campos indispensables con usuarios operativos.
