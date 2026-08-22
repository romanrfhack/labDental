# Roadmap Global

Última sincronización: **2026-08-22 — DOC-SYNC-1**.

Este roadmap prioriza el trabajo vigente. El roadmap histórico permanece en `docs/00-governance/roadmap.md` y la evidencia de implementación en `docs/IMPLEMENTATION_LOG.md`.

## 1. Estado Actual

### Sistema Privado

Implementado y validado en DEV/UAT:

- Login, sesión, CSRF/XSRF y autorización por permisos.
- Clientes, doctores y clínicas.
- Órdenes, estados e historial.
- Pagos, abonos, cancelación y saldos.
- Dashboard básico.
- Usuarios y roles.
- Etiquetas de trabajo y entrega.
- Entregas/repartidor mobile-first.
- Catálogo administrable con precios e imágenes persistentes.

Pendientes de QA operativo:

- Impresión térmica física real.
- Usuario limitado real y `/app/access-denied`.

### Sitio Público

Estado en DEV: **aprobado visual y técnicamente**.

- `PUB-UX-2`: catálogo workspace responsive — cerrado y aprobado.
- `PUB-UX-3`: home/servicios/contacto/header — cerrado, desplegado y aprobado.
- `PUB-UX-4`: optimización, accesibilidad, SEO y Lighthouse — cerrado e integrado a `dev` mediante PR #8, merge `bfa07d0285ca66fab359c151b43ed9458a6b7727`.

Lighthouse de cierre:

| Ruta | Performance | Accesibilidad | Best Practices | SEO |
| --- | ---: | ---: | ---: | ---: |
| `/` | 91 | 100 | 100 | 100 |
| `/servicios` | 95 | 100 | 100 | 100 |
| `/catalogo` | 93 | 100 | 100 | 100 |
| `/contacto` | 96 | 100 | 100 | 100 |

La fase pública equivalente a QA responsive/optimización queda cubierta por PUB-UX-2/3/4. El siguiente gran hito público es producción.

## 2. Plan Vigente Priorizado

### DOC-SYNC-1 — Reconciliación Documental

Estado: **en cierre con esta actualización**.

Objetivo:

- Eliminar contradicciones entre documentación histórica y estado real de `dev`.
- Registrar aprobaciones visuales de PUB-UX-2 y PUB-UX-3.
- Registrar integración y Lighthouse de PUB-UX-4.
- Actualizar estado comercial, ambientes, backup/restore y siguiente plan de trabajo.

Salida:

- Fuentes canónicas sincronizadas.
- Próxima fase inequívoca: `OPS-QA-1`.

### OPS-QA-1 — QA Operativo Pendiente

Estado: **siguiente fase**.

Objetivo:

Cerrar evidencia operativa que requiere condiciones reales y no desarrollo mayor.

Alcance obligatorio:

1. Impresora térmica real en DEV:
   - etiqueta interna `76 x 51 mm`;
   - etiqueta de entrega `102 x 51 mm`;
   - escala, márgenes, orientación, corte, contraste y alineación.
2. Usuario limitado real en DEV:
   - login válido;
   - permiso `customers.view` o equivalente controlado;
   - ausencia de `reports.view`;
   - `/app/dashboard -> /app/access-denied`;
   - recurso permitido continúa accesible.

Cobertura opcional:

- Forzar falla/bloqueo de `GET /api/catalog/public` para verificar fallback local de `/catalogo`.

No incluye nuevas funcionalidades salvo corrección de un hallazgo real.

### PROD-READY-1 — Preparación Para Producción

Estado: **pendiente; no iniciar promoción a `main` antes de cerrarla**.

Objetivo:

Convertir el estado aprobado de DEV en un release candidate seguro.

Alcance mínimo:

- Cambio obligatorio de contraseña temporal en primer acceso, o política equivalente aprobada antes de crear usuarios productivos.
- Environment `production` de GitHub completo.
- SQL Server productivo.
- Migraciones revisadas e idempotentes.
- Backup de BD antes de migraciones.
- Restore probado en ambiente no productivo.
- `${LDT_APP_ROOT}/shared/catalog-images` productivo con permisos mínimos.
- Backup/restore conjunto BD + imágenes.
- DNS del dominio principal y decisión sobre `www`.
- HTTPS y redirecciones canónicas.
- Health checks local y público.
- Validación de rollback.
- Cookies seguras y respuestas `401/403` correctas.
- Smoke Admin, Repartidor, catálogo administrable y sitio público.
- Release candidate identificado por SHA.

### PROD-RELEASE-1 — Primera Publicación Productiva

Estado: **pendiente**.

Dependencia: `PROD-READY-1` cerrada.

Flujo esperado:

1. Congelar release candidate en `dev`.
2. PR `dev -> main`.
3. Revisión final del diff acumulado.
4. Habilitar conscientemente `LDT_ENABLE_PROD_DEPLOY`.
5. Deploy productivo.
6. Aplicar migraciones mediante flujo documentado.
7. Health checks.
8. Smoke funcional.
9. Confirmar DNS/HTTPS.
10. Registrar aceptación o ejecutar rollback.

### POST-PROD-1 — Estabilización Inicial

Estado: **pendiente**.

Objetivo:

- Monitorear errores y disponibilidad.
- Confirmar backups automáticos.
- Ejecutar restauración periódica de prueba según política acordada.
- Revisar crecimiento de `shared/catalog-images`.
- Capturar feedback de usuarios reales.
- Cerrar aceptación de primera ronda.

## 3. Roadmap Funcional Posterior

Estas fases no deben adelantarse al primer release productivo salvo decisión explícita del responsable.

### Fase Técnica 2 — Migración Del Excel

Pendiente.

- Análisis de histórico.
- Mapeo y deduplicación.
- Importación en modo revisión.
- Manejo explícito de inconsistencias.

### Fase Técnica 3 — Inventario Y Proveedores

Pendiente.

- Proveedores.
- Materiales.
- Entradas/salidas/ajustes/mermas.
- Stock mínimo y alertas.

### Fase Técnica 4 — Reportes Administrativos

Pendiente.

- Reportes por cliente, periodo, órdenes, pagos, saldos y entregas.
- Sin BI predictivo en primera iteración.

### Fase Técnica 5 — Automatizaciones Y WhatsApp

Pendiente.

- Recordatorios y plantillas operativas.
- Consentimiento y trazabilidad.
- No implica CRM completo ni campañas masivas.

### Entregas Avanzadas

Backlog posterior al MVP:

- QR/código.
- Escaneo desde celular.
- Firma o fotografía de recibido.
- Historial completo de intentos/evidencias.

### Ciclo De Vida De Imágenes

Backlog posterior al MVP:

- Inventario de huérfanos.
- Retención y limpieza segura.
- Backup automatizado.
- Conversión/recompresión WebP.
- Imagen administrable de sección.
- Galería múltiple/CDN/cloud storage.

## 4. Mapa Comercial Actual

- Fase comercial 0 — Planeación/documentación: **completada**.
- Fase comercial 1 — Sistema administrativo MVP: **implementada en DEV/UAT**.
- Fase comercial 2 — Sitio web corporativo: **implementado y aprobado en DEV; producción pendiente**.
- Fase comercial 3 — Repartidores/entregas/etiquetas: **MVP operativo implementado; QA físico de etiquetas y ampliaciones avanzadas pendientes**.
- Fase comercial 4 — QA/capacitación/despliegue: **en preparación; producción pendiente**.

Fuente comercial detallada: `docs/09-commercial/commercial-phases.md`.

## 5. Regla De Priorización

Mientras no exista una nueva decisión explícita:

`DOC-SYNC-1 -> OPS-QA-1 -> PROD-READY-1 -> PROD-RELEASE-1 -> POST-PROD-1 -> nueva fase funcional`.

No iniciar trabajo funcional mayor ni fusionar `dev` a `main` de forma incidental fuera de esta secuencia.