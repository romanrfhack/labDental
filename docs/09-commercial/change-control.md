# Control De Cambios

## Objetivo

Definir un proceso simple para identificar, evaluar y aprobar solicitudes que cambien el alcance, costo o calendario de la primera ronda del proyecto Laboratorio Dental Tláhuac.

Este documento busca evitar ambigüedades: lo incluido se entrega dentro del alcance contratado; lo opcional o fuera de alcance se evalúa y cotiza por separado antes de implementarse.

## Qué Se Considera Cambio De Alcance

Se considera cambio de alcance cualquier solicitud que:

- Agregue funcionalidades, módulos o pantallas no incluidos en la primera ronda.
- Modifique de forma relevante reglas de negocio ya aprobadas.
- Requiera integraciones externas.
- Requiera migración completa o histórica de datos.
- Cambie entregables ya aprobados por fase.
- Requiera reportes avanzados o exportaciones complejas.
- Requiera hardware, servicios locales o infraestructura no validada.
- Requiera app móvil nativa en lugar de web responsive/PWA.
- Genere trabajo adicional fuera de la corrección de errores del alcance incluido.

Un cambio de alcance puede requerir cotización adicional, ajuste de fechas, nuevas dependencias o una fase posterior.

## Qué Se Considera Ajuste Menor

Se considera ajuste menor una corrección o refinamiento que:

- Mantiene el alcance original.
- No agrega módulos ni integraciones nuevas.
- No cambia reglas de negocio principales.
- No requiere rediseñar flujos aprobados.
- No impacta de forma relevante el calendario.
- No requiere infraestructura, hardware o servicios adicionales.

Ejemplos de ajustes menores:

- Corrección de textos.
- Ajuste visual simple dentro de una pantalla incluida.
- Corrección de errores reproducibles en funcionalidades incluidas.
- Cambio menor de etiqueta o nombre de campo.
- Ajuste básico de validación dentro de una regla ya aprobada.

## Proceso Para Solicitar Cambios

1. El cliente registra la solicitud por escrito o por el canal acordado.
2. La solicitud debe describir el objetivo, el problema que resuelve y el resultado esperado.
3. El proveedor revisa si la solicitud es ajuste menor, cambio de alcance u opción para fase posterior.
4. Si es ajuste menor, se agenda dentro de la fase correspondiente cuando sea viable.
5. Si es cambio de alcance, se prepara una evaluación de impacto.
6. El cliente revisa impacto, costo y fechas antes de autorizar.
7. El proveedor implementa el cambio únicamente después de aprobación.

## Evaluación De Impacto

Cada cambio de alcance deberá evaluarse considerando:

- Descripción del cambio.
- Módulos afectados.
- Reglas de negocio afectadas.
- Impacto en base de datos o arquitectura.
- Impacto en QA.
- Impacto en capacitación y documentación.
- Dependencias del cliente.
- Riesgos técnicos.
- Costo adicional.
- Ajuste de calendario.

## Aprobación

Un cambio de alcance se considera aprobado cuando:

- El cambio está documentado.
- El impacto está explicado.
- El precio adicional, si aplica, está confirmado.
- El ajuste de fechas, si aplica, está confirmado.
- El cliente autoriza por escrito o por el medio acordado.
- El proveedor confirma que puede integrarlo al plan de trabajo.

Las solicitudes no aprobadas permanecen como backlog o fase posterior.

## Cotización Adicional

La cotización adicional puede aplicar cuando la solicitud:

- Agrega trabajo no incluido en el Statement of Work.
- Requiere investigación técnica adicional.
- Requiere integración con terceros.
- Requiere migrar, limpiar o transformar datos.
- Requiere nuevos reportes, pantallas o flujos.
- Requiere soporte posterior, monitoreo o mantenimiento.
- Requiere validación de hardware o infraestructura especial.

La cotización deberá indicar alcance, entregables, supuestos, exclusiones, precio y condiciones de aceptación.

## Reprogramación De Fechas

Las fechas podrán reprogramarse cuando:

- El cliente apruebe un cambio de alcance.
- Los materiales, accesos o validaciones del cliente se retrasen.
- El cambio impacte módulos ya planificados.
- La validación de hardware o DNS requiera tiempo adicional.
- Se agreguen pruebas, capacitación o documentación no contempladas.

No se deben asumir fechas nuevas hasta confirmar alcance, dependencias y aprobación comercial.

## Cambios Fuera De Alcance Frecuentes

Los siguientes ejemplos no están incluidos en la primera ronda y deberán evaluarse por separado:

| Solicitud | Tratamiento |
| --- | --- |
| CFDI/facturación | Fuera de alcance. Requiere análisis fiscal, técnico y de proveedor autorizado si aplica. |
| Inventario | Fuera de alcance. Puede plantearse como fase posterior. |
| Migración completa del Excel | Fuera de alcance. La carga parcial acotada puede cotizarse por separado. |
| App móvil nativa | Fuera de alcance. La primera ronda considera web responsive/PWA. |
| Integraciones externas | Fuera de alcance salvo que se documenten y coticen por separado. |
| Servicio local de impresión no validado | Fuera de alcance hasta validar impresora, sistema operativo, red, drivers y formato de etiqueta. |
| Reportes avanzados | Fuera de alcance. Solo se incluye dashboard básico. |

## Registro De Cambio

| Campo | Valor |
| --- | --- |
| ID de cambio | `[ID]` |
| Solicitante | `[NOMBRE]` |
| Fecha de solicitud | `[FECHA]` |
| Descripción | `[DESCRIPCIÓN]` |
| Clasificación | `[AJUSTE MENOR / CAMBIO DE ALCANCE / FASE POSTERIOR]` |
| Impacto estimado | `[IMPACTO]` |
| Precio adicional | `[PRECIO SI APLICA]` |
| Ajuste de fechas | `[AJUSTE SI APLICA]` |
| Estado | `[PENDIENTE / APROBADO / RECHAZADO / DIFERIDO]` |
| Aprobación cliente | `[NOMBRE / FECHA / MEDIO]` |

