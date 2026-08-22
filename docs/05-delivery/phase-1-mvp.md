# Fase 1 — MVP Operativo

Última sincronización: **2026-08-22 — DOC-SYNC-1**.

## Estado

**Implementada y superada como baseline funcional.**

Este documento resume la Fase 1 histórica. El proyecto ya avanzó posteriormente a usuarios/roles, entregas, catálogo administrable, despliegue DEV y rediseño/optimización del sitio público.

## Objetivo Original

Permitir que el laboratorio registre operación nueva en la plataforma sin depender del Excel para el flujo cotidiano.

## Etapas

### Etapa 1 — Arquitectura Base

Estado: **completada**.

- Solución .NET 10.
- Proyectos Api, Application, Domain e Infrastructure.
- App Angular.
- Layout público/privado.
- Health check.
- Permisos base.

### Etapa 2 — Autenticación Y Autorización

Estado: **completada**.

- Usuarios, roles y permisos.
- EF Core SQL Server.
- Login/logout/me.
- Cookie HttpOnly.
- Guards Angular.
- Pruebas de integración.

### Etapa 2.1 — Hardening CSRF/XSRF

Estado: **completada**.

- Protección antiforgery en mutaciones.
- Token XSRF.
- Login/logout protegidos.
- Angular configurado para XSRF.

### Etapa 3 — Clientes / Doctores / Clínicas

Estado: **completada**.

- CRUD de clientes.
- Clínicas y doctores internos.
- Desactivación lógica.
- Pruebas backend.

### Etapa 4 — Órdenes De Trabajo Dental

Estado: **completada**.

- Órdenes.
- Folio único.
- Edición.
- Estados e historial.
- Cancelación con reglas.

### Etapa 5 — Pagos, Abonos Y Saldos

Estado: **completada**.

- Pagos por orden.
- Cancelación de pagos.
- `PaidAmount`, `Balance`, estado financiero.
- Listado y detalle.

### Etapa 6 — Dashboard Operativo Básico

Estado: **completada**.

- `GET /api/dashboard/summary`.
- Métricas de clientes, órdenes, cobranza y saldos.
- Secciones condicionadas por permisos.
- Dashboard Angular real.

### Etapa 7 — QA Funcional Y Demo

Estado: **completada técnicamente; validaciones humanas posteriores cubiertas por fases 2.x/3.x**.

- QA funcional contra SQL Server.
- Login, clientes, órdenes, pagos, saldos y dashboard.
- CSRF/XSRF, `401`, `403`, `/health`.
- Guion y datos de demo.
- QA privado posterior con Admin.
- Pase visual humano privado cerrado en fases posteriores.

## Backlog Original

- [x] Crear solución backend.
- [x] Crear app frontend.
- [x] Implementar login.
- [x] Implementar clientes.
- [x] Implementar órdenes.
- [x] Implementar pagos.
- [x] Implementar dashboard.
- [x] Ejecutar QA funcional MVP.
- [x] Preparar guion de demo.
- [x] Definir permisos base.
- [x] Implementar roles/permisos.
- [x] Implementar hardening CSRF/XSRF.
- [x] Realizar deploy inicial en DEV.

## Evolución Posterior Ya Implementada

Después de esta fase se incorporaron:

- Usuario QA limitado Development-only.
- Deploy DEV/UAT.
- Impresión de etiquetas.
- Usuarios y roles administrables.
- Entregas/repartidor mobile-first.
- Catálogo administrable.
- Upload y persistencia de imágenes.
- Rediseño público PUB-UX-2/3.
- Optimización/accesibilidad/SEO PUB-UX-4.

## Pendientes Vigentes Que Ya No Pertenecen A Fase 1

- Prueba física de impresora térmica.
- Usuario limitado real en navegador.
- Readiness de producción.
- Primera publicación productiva.
- Migración del Excel histórico.
- Inventario/proveedores.
- Reportes ampliados.
- Automatizaciones/WhatsApp.

Fuente vigente de prioridad: `docs/05-delivery/current-work-plan.md`.

## Criterio De Salida

La Fase 1 se considera cerrada porque el sistema puede operar registros nuevos, órdenes, pagos, saldos y dashboard, y ese baseline ya está desplegado en DEV.
