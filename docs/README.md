# Índice De Documentación

Última sincronización: **2026-08-22 — DOC-SYNC-1**.

Este directorio contiene la documentación viva de Laboratorio Dental Tláhuac. El estado vigente se concentra en pocas fuentes canónicas; los documentos históricos conservan contexto y evidencia pero no deben prevalecer sobre el estado actual.

## Fuentes Canónicas Vigentes

| Tema | Fuente |
| --- | --- |
| Estado global actual | `docs/PROJECT_STATUS.md` |
| Roadmap global actual | `docs/ROADMAP.md` |
| Plan de trabajo vigente | `docs/05-delivery/current-work-plan.md` |
| Bitácora histórica de tareas | `docs/IMPLEMENTATION_LOG.md` |
| Changelog de entregas | `docs/00-governance/changelog.md` |
| Sitio público | `docs/01-product/public-website.md` |
| Sistema privado | `docs/01-product/internal-system.md` |
| Deploy y dominio | `docs/05-delivery/DEPLOYMENT.md` |
| Ambientes | `docs/06-operations/environments.md` |
| Backup y restore | `docs/06-operations/backup-and-restore.md` |
| Auth y permisos | `docs/03-architecture/AUTH_FLOW.md` |
| Arquitectura global | `docs/03-architecture/ARCHITECTURE.md` |

## Producto / Operación

| Tema | Fuente |
| --- | --- |
| Órdenes y entrega | `docs/01-product/operations-orders-delivery.md` |
| Impresión de etiquetas | `docs/01-product/label-printing.md` |
| Flujo repartidor mobile-first | `docs/01-product/driver-mobile-workflow.md` |
| Diseño MVP entregas | `docs/01-product/delivery-mvp-design.md` |
| Administración de catálogo | `docs/01-product/admin-catalog-management.md` |
| Diseño catálogo administrable | `docs/01-product/catalog-admin-design.md` |
| Diseño upload/storage imágenes | `docs/01-product/catalog-image-upload-design.md` |
| Marca | `docs/02-domain/brand-guidelines.md` |

## QA

| Tema | Fuente |
| --- | --- |
| QA responsive general | `docs/08-qa/RESPONSIVE_CHECKLIST.md` |
| QA MVP administrativo | `docs/08-qa/mvp-qa-checklist.md` |
| QA privado Admin | `docs/08-qa/private-admin-qa.md` |
| QA etiquetas | `docs/08-qa/label-printing-qa.md` |
| QA usuarios/roles | `docs/08-qa/users-roles-qa.md` |
| QA API entregas | `docs/08-qa/delivery-api-qa.md` |
| QA Admin entregas | `docs/08-qa/delivery-admin-ui-qa.md` |
| QA Repartidor | `docs/08-qa/driver-mobile-qa.md` |
| QA API catálogo | `docs/08-qa/catalog-api-qa.md` |
| QA Admin catálogo | `docs/08-qa/catalog-admin-ui-qa.md` |
| QA catálogo público/fallback | `docs/08-qa/public-catalog-api-qa.md` |
| QA API upload imágenes | `docs/08-qa/catalog-image-upload-api-qa.md` |
| QA UI upload imágenes | `docs/08-qa/catalog-image-upload-ui-qa.md` |
| QA usuario limitado | `docs/08-qa/limited-user-qa-plan.md` |
| QA Lighthouse PUB-UX-4 | `docs/08-qa/pub-ux-4-lighthouse.md` |

## Comercial

- `docs/09-commercial/commercial-phases.md` contiene el estado comercial sincronizado.
- El resto de `docs/09-commercial/` conserva propuesta, SOW, alcance y materiales de aceptación.
- La documentación comercial no sustituye el estado técnico.

## Estado Resumido

### Sistema privado

- MVP administrativo: operativo en DEV/UAT.
- Usuarios/roles: implementados.
- Entregas/repartidor: MVP implementado.
- Catálogo administrable e imágenes: Fase 3.5.4 cerrada con QA end-to-end.
- Pendientes manuales inmediatos: impresora térmica real y usuario limitado real.

### Sitio público

- PUB-UX-2: cerrado y aprobado.
- PUB-UX-3: cerrado, desplegado y aprobado.
- PUB-UX-4: cerrado e integrado a `dev`.
- Lighthouse: 100 en accesibilidad, Best Practices y SEO; Performance 91–96.
- Producción: pendiente.

## Plan Vigente

```text
DOC-SYNC-1
    ↓
OPS-QA-1
    ↓
PROD-READY-1
    ↓
PROD-RELEASE-1
    ↓
POST-PROD-1
    ↓
Nueva fase funcional
```

Detalle: `docs/05-delivery/current-work-plan.md`.

## Carpetas

- `00-governance/`: control, decisiones de proyecto, roadmap histórico y changelog.
- `01-product/`: definición funcional y flujos.
- `02-domain/`: reglas de negocio y dominio.
- `03-architecture/`: arquitectura, auth, backend/frontend y datos.
- `04-decisions/`: ADRs.
- `05-delivery/`: fases, deploy y plan de trabajo vigente.
- `06-operations/`: ambientes, backup y operación.
- `07-imports/`: migración futura del Excel.
- `08-qa/`: evidencia y checklists.
- `09-commercial/`: propuesta, fases comerciales y aceptación.

## Regla De Prioridad Documental

Cuando dos documentos contradigan el estado actual, usar este orden:

1. `PROJECT_STATUS.md`.
2. `ROADMAP.md`.
3. `current-work-plan.md`.
4. Fuente específica del frente.
5. Bitácoras/changelog como historia.

## Regla De Actualización

Cada cierre relevante debe actualizar como mínimo:

- `docs/PROJECT_STATUS.md`.
- `docs/ROADMAP.md` si cambia prioridad/fase.
- La fuente específica afectada.
- Changelog o bitácora cuando corresponda.

No reescribir ADRs históricos para reflejar decisiones posteriores; agregar nueva decisión si aplica.
