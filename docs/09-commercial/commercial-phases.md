# Fases Comerciales

Última sincronización: **2026-08-22 — DOC-SYNC-1**.

Este documento describe fases comerciales y de aceptación con cliente. No sustituye `docs/ROADMAP.md` ni la evidencia técnica.

## Fase 0 — Planeación Y Documentación

Estado actual: **completada**.

### Objetivo

Definir contexto de negocio, alcance inicial, reglas principales, arquitectura base y documentación de trabajo.

### Entregables

- Documentación inicial del proyecto.
- Alcance del MVP.
- Reglas de negocio base.
- Decisiones arquitectónicas iniciales.
- Roadmap y próximos pasos.

### Criterio De Aceptación

Cumplido.

## Fase 1 — Sistema Administrativo MVP

Estado actual: **implementada y validada en DEV/UAT**.

### Entregables Vigentes

- Login privado.
- Usuarios, roles y permisos.
- Cookie auth HttpOnly y CSRF/XSRF.
- Clientes, doctores y clínicas.
- Órdenes de trabajo dental.
- Estados e historial.
- Pagos, abonos, cancelación y saldos calculados.
- Dashboard operativo/financiero básico.
- QA funcional documentado.
- Administración de catálogo, precios e imágenes.

### Pendientes Antes De Producción

- Hardening de contraseña temporal / cambio obligatorio en primer acceso o política equivalente aprobada.
- QA final de release candidate dentro de `PROD-READY-1`.

### Criterio De Aceptación

El flujo administrativo principal ya puede demostrarse y operar en DEV. La aceptación productiva se completa durante Fase 4.

## Fase 2 — Sitio Web Corporativo

Estado actual: **implementada, desplegada y aprobada en DEV; publicación productiva pendiente**.

### Entregables Implementados

- Home corporativa.
- Servicios.
- Catálogo público administrable.
- Datos de contacto confirmados.
- Diseño mobile-first.
- SEO por ruta.
- Accesibilidad y reduced motion.
- Lighthouse de cierre PUB-UX-4.

### Pendientes De Contenido

No bloquean DEV, pero solo deben publicarse cuando sean confirmados:

- Dirección.
- Horarios.
- WhatsApp institucional.
- Redes sociales.
- Mapa.
- Condiciones comerciales todavía no aprobadas formalmente.

### Pendiente Comercial Principal

Publicar `laboratoriodentaltlahuac.com` mediante `PROD-READY-1` y `PROD-RELEASE-1`.

### Criterio De Aceptación

- DEV: cumplido visual/funcionalmente.
- Producción: pendiente hasta que el dominio productivo cargue correctamente y el release sea aceptado.

## Fase 3 — Repartidores, Entregas Y Etiquetas

Estado actual: **MVP operativo implementado en DEV; ampliaciones avanzadas pendientes**.

### Implementado

- Etiquetas de trabajo y entrega desde navegador.
- Tamaños objetivo 76 x 51 mm y 102 x 51 mm.
- Asignación de entrega/repartidor.
- Estados logísticos.
- Reintento de entrega no realizada.
- Listado/detail mobile-first para repartidor.
- Cierre entregado/no entregado.
- Nombre de quien recibe cuando aplica.
- Teléfono/WhatsApp/mapa solo cuando existe dato.

### Pendiente De QA Operativo

- Prueba física con impresora térmica real dentro de `OPS-QA-1`.

### Ampliaciones Fuera Del MVP Cerrado

- QR o código.
- Escaneo desde celular.
- Firma digital o fotografía de recibido.
- Historial completo de intentos/evidencias.
- PWA dedicada, si posteriormente se justifica.

### Criterio De Aceptación

El MVP actual permite asignar, consultar y cerrar entregas. La visión comercial ampliada queda pendiente de una fase futura si el laboratorio la prioriza.

## Fase 4 — QA, Capacitación Y Despliegue

Estado actual: **en preparación; DEV validado, producción pendiente**.

### Trabajo Ya Cubierto

- QA funcional amplio en DEV.
- QA visual público.
- QA catálogo administrable e imágenes.
- QA repartidor.
- Lighthouse público.
- Deploy DEV con health checks y rollback.

### Trabajo Pendiente

1. `OPS-QA-1` — impresora térmica y usuario limitado real.
2. `PROD-READY-1` — seguridad, infraestructura, backups, DNS/HTTPS y release candidate.
3. `PROD-RELEASE-1` — primera publicación productiva.
4. Capacitación básica de usuarios clave, cuando se defina la fecha de salida.
5. `POST-PROD-1` — observación inicial y cierre de aceptación.

### Criterio De Aceptación

La fase comercial 4 se cierra cuando:

- producción está operativa;
- usuarios clave pueden ejecutar los flujos principales;
- backups/restore están definidos y probados;
- smoke productivo es satisfactorio;
- se registra aceptación de la primera ronda.

## Fuente De Prioridad

El orden vigente de trabajo está en `docs/05-delivery/current-work-plan.md`.
