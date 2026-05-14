# Statement Of Work

## Proyecto

- Cliente: Laboratorio Dental Tláhuac.
- Proyecto: Primera ronda de implementación de software a medida.
- Documento: Statement of Work base para propuesta/contrato.
- Fecha: `[FECHA]`.
- Versión: `[VERSIÓN]`.

## Objetivo

Implementar una primera ronda productiva que permita al Laboratorio Dental Tláhuac operar su MVP administrativo, publicar un sitio web corporativo y habilitar un flujo inicial de repartidores, etiquetas y evidencia de entrega.

El proyecto busca reducir la dependencia operativa de Excel, centralizar información clave y dejar una base lista para evolucionar por fases.

## Alcance Incluido

### Sistema Administrativo Privado

- Login privado.
- Usuarios, roles y permisos.
- Seguridad con cookie HttpOnly y protección CSRF/XSRF.
- Clientes, doctores, clínicas y doctores internos.
- Órdenes de trabajo dental.
- Estados de órdenes e historial.
- Pagos, abonos y saldos calculados.
- Cancelación de pagos con motivo.
- Dashboard operativo y financiero básico.

### Sitio Web Corporativo

- Sitio público corporativo para Laboratorio Dental Tláhuac.
- Contenido institucional aprobado por el cliente.
- Sección de servicios.
- Datos de contacto.
- Ubicación.
- Separación entre sitio público y sistema administrativo privado.
- Configuración con dominio/DNS acordado, sujeto a accesos o coordinación del cliente.

### Repartidores, Entregas Y Etiquetas

- Etiquetas por orden con QR o código.
- Impresión básica de etiquetas desde navegador.
- Módulo web responsive/PWA para repartidores.
- Consulta de entregas asignadas desde celular.
- Asignación de entregas.
- Escaneo QR/código desde celular, sujeto a compatibilidad de dispositivo y navegador.
- Captura de nombre de quien recibe.
- Evidencia de entrega mediante firma o fotografía.
- Historial de entregas por orden y por repartidor.
- Panel administrativo básico de entregas.

### QA, Despliegue Y Documentación

- QA funcional de la primera ronda.
- Ajustes menores derivados de QA dentro del alcance contratado.
- Despliegue en ambiente productivo acordado.
- Capacitación básica para usuarios clave.
- Documentación operativa básica.
- Cierre de aceptación.

## Alcance Opcional

Los siguientes puntos no forman parte automática del alcance incluido, pero pueden cotizarse por separado:

- Migración parcial o carga inicial acotada desde Excel.
- Reportes adicionales.
- Exportaciones específicas.
- Servicio local de impresión, sujeto a validación de impresora, sistema operativo, red, drivers y formato de etiqueta.
- Automatizaciones de comunicación.
- Geolocalización o mapas.
- Soporte posterior a la puesta en producción.
- Mantenimiento mensual.
- Bolsa de horas.
- Evolutivos posteriores.

## Alcance No Incluido

- Migración completa del histórico del Excel.
- Inventario avanzado.
- Proveedores.
- Compras.
- Cuentas por pagar.
- CFDI/facturación.
- Reportes avanzados.
- Exportaciones complejas.
- WhatsApp automatizado.
- App móvil nativa.
- Geolocalización avanzada.
- Optimización de rutas.
- Integración avanzada con mapas.
- Integraciones externas no especificadas.
- Hardware, impresoras, etiquetas físicas, lectores o consumibles.
- Servicio local de impresión sin validación previa de hardware.
- Soporte continuo o indefinido.

## Entregables

| Entregable | Descripción | Estado esperado |
| --- | --- | --- |
| Sistema administrativo MVP | Funcionalidad privada para clientes, órdenes, pagos, saldos, permisos y dashboard básico. | Disponible para validación y uso productivo. |
| Sitio web corporativo | Sitio público con información aprobada del laboratorio. | Publicado en dominio acordado. |
| Módulo de repartidores | Flujo web responsive/PWA para consulta, avance y cierre de entregas. | Disponible para usuarios autorizados. |
| Etiquetas | Identificación por orden con QR o código e impresión básica desde navegador. | Formato validado con el cliente. |
| Evidencia de entrega | Captura de firma o fotografía y nombre de quien recibe. | Asociada al historial de entrega. |
| QA funcional | Validación de flujos principales de la primera ronda. | Sin bloqueos críticos conocidos. |
| Capacitación básica | Sesión para usuarios clave. | Ejecutada y documentada. |
| Documentación básica | Documentación operativa y de cierre. | Entregada al cliente. |

## Fases

### Fase 0 - Planeación Y Documentación

Estado actual: completada.

Incluye documentación base, alcance, roadmap, paquete comercial y definición de prioridades.

### Fase 1 - Sistema Administrativo MVP

Estado actual: implementada y validada en QA local.

Incluye login, seguridad, usuarios, roles, permisos, clientes, doctores, clínicas, órdenes, pagos, saldos, dashboard y QA local.

### Fase 2 - Sitio Web Corporativo

Estado actual: pendiente.

Incluye preparación y publicación del sitio público con contenidos, contacto, ubicación y servicios aprobados por el cliente.

### Fase 3 - Repartidores, Entregas Y Etiquetas

Estado actual: pendiente.

Incluye etiquetas, impresión básica desde navegador, módulo web responsive/PWA para repartidores, asignación, escaneo, evidencia e historial.

### Fase 4 - QA, Capacitación Y Despliegue

Estado actual: pendiente/cierre.

Incluye validación funcional, capacitación básica, despliegue productivo y cierre de aceptación.

## Criterios De Aceptación

### Aceptación General

- El alcance contratado está implementado o documentado como pendiente de fase posterior.
- Los flujos principales fueron validados con usuarios clave del cliente.
- No existen bloqueos críticos conocidos para producción.
- El sistema administrativo privado no queda expuesto como sitio público.
- El sitio web corporativo carga correctamente en el dominio acordado.
- Las etiquetas pueden generarse y se pueden imprimir desde navegador.
- El flujo de entrega permite asignar, consultar, escanear, cerrar con evidencia y consultar historial.
- La capacitación básica fue realizada.
- Los pendientes o mejoras quedan registrados como backlog posterior o cambio de alcance.

### Aceptación Por Fase

- Fase 0: documentación suficiente para confirmar alcance y prioridades.
- Fase 1: demo funcional del sistema administrativo con datos de prueba y QA local documentada.
- Fase 2: sitio web publicado con contenido aprobado por el cliente.
- Fase 3: flujo de entrega validado con etiqueta, repartidor, escaneo, evidencia e historial.
- Fase 4: despliegue completado, usuarios capacitados y cierre documentado.

## Supuestos

- El sistema será web.
- Los repartidores usarán web responsive/PWA desde celular.
- La app móvil nativa no forma parte de esta primera ronda.
- La impresión incluida es impresión básica desde navegador.
- Cualquier servicio local de impresión dependerá de validar hardware, sistema operativo, red, drivers y formato de etiqueta.
- El cliente entregará logo, textos, datos de contacto, ubicación y servicios.
- El cliente proporcionará acceso a dominio/DNS o coordinará los cambios necesarios.
- El cliente proporcionará usuarios clave para validar los flujos.
- La migración completa del Excel se considera alcance posterior.
- La capacitación incluida es básica y enfocada en los flujos de la primera ronda.
- El soporte continuo posterior al cierre es opcional y requiere acuerdo separado.

## Dependencias

- Aprobación del alcance contratado.
- Confirmación de precio y forma de pago.
- Materiales del sitio web entregados por el cliente.
- Datos de contacto, ubicación y servicios validados.
- Accesos o coordinación para dominio/DNS.
- Definición de usuarios clave.
- Confirmación de hardware de impresión y modelo de impresora.
- Equipo o ambiente para pruebas de impresión, si aplica.
- Accesos y credenciales necesarios para producción.
- Validación de datos antes de puesta en producción.
- Aprobación por fase.

## Responsabilidades Del Proveedor

- Implementar los entregables incluidos en este Statement of Work.
- Mantener el alcance separado entre incluido, opcional y fuera de alcance.
- Informar dependencias, bloqueos y riesgos relevantes.
- Ejecutar QA funcional sobre los flujos incluidos.
- Corregir errores atribuibles al alcance incluido durante la validación de la fase correspondiente.
- Preparar el despliegue en el ambiente acordado.
- Ejecutar capacitación básica para usuarios clave.
- Documentar pendientes, decisiones y cambios de alcance.
- Solicitar aprobación antes de implementar cambios fuera de alcance.

## Responsabilidades Del Cliente

- Revisar y aprobar el alcance contratado.
- Entregar logo, textos, servicios, datos de contacto y ubicación.
- Validar dominio/DNS o coordinar cambios con el proveedor correspondiente.
- Designar usuarios clave para validación.
- Validar flujos de negocio con datos y casos reales.
- Probar la demo y reportar observaciones oportunamente.
- Confirmar hardware de impresión, modelo de impresora y equipo para pruebas.
- Validar los datos antes de producción.
- Entregar accesos, credenciales o coordinación técnica necesaria.
- Aprobar entregables por fase.
- Separar solicitudes nuevas como cambios de alcance.

## Condiciones Para Cambios De Alcance

Se considera cambio de alcance cualquier solicitud que:

- Agregue un módulo no incluido.
- Cambie de forma relevante una regla de negocio aprobada.
- Requiera integración con terceros.
- Requiera migrar información histórica no contemplada.
- Requiera reportes avanzados, CFDI, inventario o app móvil nativa.
- Cambie formatos, flujos o entregables después de su aprobación.
- Requiera soporte, mantenimiento o desarrollo posterior al cierre.

Los cambios de alcance deberán documentarse, estimarse y aprobarse antes de implementarse. Podrán generar costo adicional y ajuste de calendario.

## Condiciones Para Cierre De Fase

Una fase puede cerrarse cuando:

- Los entregables de la fase están disponibles para revisión.
- El cliente cuenta con información suficiente para validar.
- Los criterios de aceptación de la fase se cumplen.
- No existen bloqueos críticos atribuibles al alcance incluido.
- Las observaciones menores quedan registradas.
- Los cambios fuera de alcance quedan separados para evaluación posterior.
- El cliente aprueba el cierre por escrito o por el medio acordado.

## Puesta En Producción

La puesta en producción requiere:

- Alcance contratado confirmado.
- Ambiente productivo definido.
- Dominio/DNS disponibles o coordinados.
- Variables de entorno y credenciales productivas configuradas.
- Base de datos productiva preparada.
- Usuario Admin productivo definido por el cliente.
- QA funcional sin bloqueos críticos.
- Plan básico de respaldo o respaldo inicial definido.
- Validación de hardware si se requiere impresión local más allá del navegador.

La puesta en producción no incluye migración completa del Excel ni soporte continuo posterior, salvo acuerdo adicional.

## Capacitación

La capacitación básica incluye una sesión para usuarios clave sobre:

- Acceso al sistema.
- Clientes, doctores y clínicas.
- Órdenes de trabajo.
- Pagos, abonos y saldos.
- Dashboard.
- Etiquetas y entregas.
- Captura de evidencia.
- Buenas prácticas operativas.

Capacitaciones adicionales, materiales especializados o sesiones recurrentes pueden cotizarse por separado.

## Soporte Posterior Opcional

Después del cierre de la primera ronda, el cliente puede contratar soporte posterior bajo una modalidad separada:

- Bolsa de horas.
- Mantenimiento mensual.
- Soporte por evento.
- Evolutivos por fase.

El soporte posterior no se considera incluido por defecto y deberá acordarse en precio, vigencia, canales, tiempos de respuesta y alcance.

## Precio Y Condiciones Comerciales

- Precio total de la primera ronda: `[PRECIO ACORDADO]`.
- Moneda: `[MONEDA]`.
- Forma de pago: `[FORMA DE PAGO]`.
- Vigencia de propuesta: `[VIGENCIA]`.
- Condiciones especiales: `[CONDICIONES ESPECIALES]`.

No se incluyen importes en este documento hasta que sean confirmados comercialmente.

## Fechas Estimadas

- Fecha de autorización: `[FECHA DE AUTORIZACIÓN]`.
- Inicio estimado: `[INICIO ESTIMADO]`.
- Entrega estimada por fase: `[ENTREGA ESTIMADA POR FASE]`.
- Puesta en producción estimada: `[PUESTA EN PRODUCCIÓN ESTIMADA]`.

Las fechas deberán confirmarse al cerrar alcance, precio, dependencias y disponibilidad de materiales del cliente.

## Aprobaciones Y Firmas

### Cliente

- Nombre: `[NOMBRE]`
- Cargo: `[CARGO]`
- Firma: ______________________________
- Fecha: `[FECHA]`

### Proveedor

- Nombre: `[NOMBRE]`
- Cargo: `[CARGO]`
- Firma: ______________________________
- Fecha: `[FECHA]`

