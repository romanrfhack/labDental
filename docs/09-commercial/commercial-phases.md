# Fases Comerciales

## Fase 0 — Planeación Y Documentación

Estado actual: completada.

### Objetivo

Definir el contexto de negocio, alcance inicial, reglas principales, arquitectura base y documentación de trabajo.

### Entregables

- Documentación inicial del proyecto.
- Alcance del MVP.
- Reglas de negocio base.
- Decisiones arquitectónicas iniciales.
- Roadmap y próximos pasos.

### Criterio De Aceptación

La fase se considera aceptada cuando existe documentación suficiente para iniciar implementación y priorizar el MVP administrativo.

### Dependencias

- Confirmación de objetivos del laboratorio.
- Validación inicial de operación actual basada en Excel.

## Fase 1 — Sistema Administrativo MVP

Estado actual: implementada y validada en QA local.

### Objetivo

Contar con un sistema administrativo privado para operar clientes, doctores, clínicas, órdenes, pagos, saldos y dashboard básico.

### Entregables

- Login privado.
- Usuarios, roles y permisos.
- Cookie auth HttpOnly y protección CSRF/XSRF.
- Clientes, doctores y clínicas.
- Doctores internos para clínicas.
- Órdenes de trabajo dental.
- Estados e historial de estados.
- Pagos, abonos, cancelación de pagos y saldos calculados.
- Dashboard operativo y financiero básico.
- QA funcional documentado.
- Guion de demo y guía de datos de prueba.

### Criterio De Aceptación

La fase se acepta cuando el flujo administrativo principal puede demostrarse con datos de prueba y los criterios de QA local quedan documentados.

### Dependencias

- Base de datos configurada.
- Usuario Admin configurado.
- Validación funcional en ambiente local.

## Fase 2 — Sitio Web Corporativo

Estado actual: pendiente.

### Objetivo

Publicar una presencia digital corporativa para Laboratorio Dental Tláhuac en `laboratoriodentaltlahuac.com`.

### Entregables

- Página principal corporativa.
- Sección de servicios.
- Datos de contacto.
- Ubicación.
- Información institucional.
- Integración con dominio y DNS, sujeto a accesos del cliente.

### Criterio De Aceptación

La fase se acepta cuando el sitio carga correctamente en el dominio acordado, comunica la información aprobada por el cliente y no expone el sistema privado.

### Dependencias

- Logo, textos, datos de contacto, ubicación y servicios proporcionados por el cliente.
- Acceso o coordinación para dominio/DNS.
- Aprobación de contenido.

## Fase 3 — Repartidores, Entregas Y Etiquetas

Estado actual: pendiente.

### Objetivo

Habilitar un flujo trazable de entregas mediante repartidores, etiquetas por orden, escaneo desde celular y evidencia de recibido.

### Entregables

- Etiquetas por orden con QR o código.
- Impresión básica desde navegador.
- Módulo web responsive/PWA para repartidores.
- Asignación de entregas.
- Estados de entrega.
- Escaneo desde celular.
- Captura de firma o fotografía de recibido.
- Nombre de quien recibe.
- Historial por orden y por repartidor.
- Panel administrativo de entregas.

### Criterio De Aceptación

La fase se acepta cuando una entrega puede asignarse, consultarse desde celular, escanearse, cerrarse con evidencia y quedar registrada en historial.

### Dependencias

- Definición de formato de etiqueta.
- Validación de flujo real con repartidores.
- Validación de hardware si se solicita servicio local de impresión.

## Fase 4 — QA, Capacitación Y Despliegue

Estado actual: pendiente/cierre.

### Objetivo

Validar el alcance contratado, capacitar usuarios clave y poner la primera ronda en producción.

### Entregables

- QA funcional de la primera ronda.
- Ajustes menores derivados de QA dentro del alcance.
- Despliegue en ambiente productivo.
- Capacitación básica.
- Documentación de operación.
- Cierre de aceptación.

### Criterio De Aceptación

La fase se acepta cuando el cliente valida los flujos principales, el sistema queda desplegado, los usuarios clave reciben capacitación y se documenta el cierre de la primera ronda.

### Dependencias

- Accesos de producción.
- Dominio/DNS disponibles.
- Datos finales del cliente.
- Aprobación de pruebas con usuarios reales.
