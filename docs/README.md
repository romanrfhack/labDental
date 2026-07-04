# Índice De Documentación

Este directorio contiene la documentación viva del proyecto Laboratorio Dental Tláhuac. La documentación se separa en sistema privado, sitio público, control global, operación, QA y paquete comercial.

## Fuentes Canónicas

| Tema | Fuente |
| --- | --- |
| Estado global | `docs/PROJECT_STATUS.md` |
| Roadmap global | `docs/ROADMAP.md` |
| Bitácora de tareas | `docs/IMPLEMENTATION_LOG.md` |
| Changelog histórico de entregas | `docs/00-governance/changelog.md` |
| Sitio público | `docs/01-product/public-website.md` |
| Sistema privado | `docs/01-product/internal-system.md` |
| Operación de órdenes y entrega | `docs/01-product/operations-orders-delivery.md` |
| Impresión de etiquetas | `docs/01-product/label-printing.md` |
| Flujo mobile-first repartidor | `docs/01-product/driver-mobile-workflow.md` |
| Diseño MVP entregas/repartidor | `docs/01-product/delivery-mvp-design.md` |
| Backlog administración de catálogo | `docs/01-product/admin-catalog-management.md` |
| Lineamientos de marca | `docs/02-domain/brand-guidelines.md` |
| Arquitectura global | `docs/03-architecture/ARCHITECTURE.md` |
| Auth y permisos | `docs/03-architecture/AUTH_FLOW.md` |
| Deploy y dominio | `docs/05-delivery/DEPLOYMENT.md` |
| Validación DEV baseline UAT | `docs/05-delivery/dev-deployment-validation.md` |
| QA responsive | `docs/08-qa/RESPONSIVE_CHECKLIST.md` |
| QA MVP administrativo | `docs/08-qa/mvp-qa-checklist.md` |
| QA sistema privado con Admin | `docs/08-qa/private-admin-qa.md` |
| QA impresión de etiquetas | `docs/08-qa/label-printing-qa.md` |
| QA usuarios y roles | `docs/08-qa/users-roles-qa.md` |
| QA API entregas | `docs/08-qa/delivery-api-qa.md` |
| Plan QA usuario limitado | `docs/08-qa/limited-user-qa-plan.md` |
| Alcance comercial | `docs/09-commercial/` |

## Carpetas

- `00-governance/`: decisiones de control del proyecto, definition of done, roadmap técnico histórico y changelog de entregas.
- `01-product/`: definición funcional del sitio público, sistema privado, pantallas, flujos, contexto y permisos.
- `02-domain/`: reglas de negocio, entidades, pagos, ciclo de vida de órdenes e inventario futuro.
- `03-architecture/`: arquitectura técnica, auth, backend, frontend y base de datos.
- `04-decisions/`: ADRs aceptadas. No reescribir decisiones históricas; agregar nuevas ADRs cuando aplique.
- `05-delivery/`: fases de entrega, deploy y próximos pasos.
- `06-operations/`: ambientes, backup y operación. `deployment.md` queda como puente hacia `05-delivery/DEPLOYMENT.md`.
- `07-imports/`: análisis y estrategia para migración del Excel.
- `08-qa/`: QA funcional del MVP administrativo, checklist responsive, demo, datos y hallazgos.
- `09-commercial/`: propuesta, SOW, fases comerciales, matriz de alcance, responsabilidades y control de cambios.

## Separación De Frentes

- Sistema privado / MVP administrativo: vive bajo `/app`; su estado actual está avanzado y documentado en QA, con Fase 2.5 cerrada como pase visual humano privado completado, Fase 2.6 implementada para usuario QA limitado Development-only, DEV validado como baseline UAT inicial en Fase 3.0, Fase 3.1 documentada como análisis operativo para órdenes, etiquetas, reparto, usuarios/roles y catálogo, Fase 3.2 implementada para impresión MVP de etiquetas desde órdenes existentes, Fase 3.3 implementada para administración MVP de usuarios/roles, Fase 3.4.0 documentada como diseño técnico previo de entregas/repartidor y Fase 3.4.1 implementada como backend delivery MVP + permisos.
- Sitio público institucional: vive en la app Angular existente bajo `src/LaboratorioTlahuac.Web/src/app/public`; la primera versión mobile-first está implementada y Fase 1.6 quedó validada visualmente por el responsable del proyecto.
- Documentación comercial: describe alcance propuesto y fases comerciales, no necesariamente implementación técnica actual.

## Regla De Actualización

Cada cambio de código debe actualizar `docs/PROJECT_STATUS.md` y `docs/IMPLEMENTATION_LOG.md`. Los cambios de arquitectura, auth, deploy, producto o QA deben actualizar también su fuente canónica correspondiente.
