# Auditoría Documental

Fecha: 2026-05-12.

Nota posterior: la Fase 0.2 de consolidación documental ya fue ejecutada. Las fuentes canónicas actuales están listadas en `docs/README.md`; este documento queda como evidencia de la auditoría previa y puede contener rutas o recomendaciones históricas resueltas mediante puentes.

## Resumen Ejecutivo

La documentación actual sí contiene la información necesaria para continuar el proyecto, pero quedó dividida en tres líneas que hoy se mezclan:

- Sistema privado existente: MVP administrativo bajo `/app`, implementado y validado en QA local.
- Sitio público nuevo: sitio corporativo mobile-first pendiente de diseño y consolidación.
- Control global del proyecto: estado, roadmap, bitácora, changelog, entrega, operación y alcance comercial.

La aparente contradicción principal no es técnica: `README.md` y `docs/00-governance/project-status.md` dicen que el MVP administrativo está en Fase 1 Etapa 7, mientras `docs/PROJECT_STATUS.md` dice Fase 0. Esto se resuelve separando el estado por frente: sistema privado en Fase 1 Etapa 7 y sitio público en Fase 0 de preparación documental.

No se recomienda borrar, mover ni fusionar archivos todavía. La siguiente tarea debería consolidar fuentes canónicas, agregar índices y dejar los documentos nuevos en carpetas coherentes.

## Estado Actual De La Documentación

- `AGENTS.md` existe y define reglas permanentes para Codex, pero apunta a documentos raíz que podrían moverse en una consolidación futura.
- `README.md` funciona como guía técnica completa, pero contiene demasiado detalle que ya existe en `docs/`.
- `docs/` tiene documentación madura del MVP administrativo en carpetas numeradas.
- Los documentos raíz nuevos de `docs/` funcionan como resumen de arranque del sitio público, pero duplican documentos previos.
- La documentación comercial en `docs/09-commercial/` está alineada para propuesta/contrato, pero usa fases comerciales distintas a las fases técnicas.
- `.agents/` y `.codex/` existen como carpetas, pero no contienen archivos relevantes en esta revisión.

## Inventario De Documentos Revisados

### Raíz Del Repo

| Ruta | Propósito aparente | Temas principales | Alcance | Estado | Recomendación |
| --- | --- | --- | --- | --- | --- |
| `AGENTS.md` | Reglas permanentes para Codex | mobile-first, documentación obligatoria, comandos, auth | General | Vigente, pendiente de consolidar rutas | Mantener en raíz; actualizar rutas solo después de mover documentos |
| `README.md` | Guía técnica y ejecutiva del repo | stack, comandos, migraciones, endpoints, QA, comercial, estado | Ambos + general | Vigente pero demasiado amplio | Convertir en resumen con enlaces a docs canónicos |
| `src/LaboratorioTlahuac.Web/README.md` | README generado por Angular CLI | ng serve, scaffolding, build, test/e2e genéricos | Frontend | Parcial/desactualizado | Revisar después; menciona `ng test`/`ng e2e` aunque no hay scripts npm configurados |

### docs/ Raíz

| Ruta | Propósito aparente | Temas principales | Alcance | Estado | Recomendación |
| --- | --- | --- | --- | --- | --- |
| `docs/PROJECT_STATUS.md` | Estado actual global reciente | sistema existente, sitio público, fase 0 sitio público | General + sitio público | Parcial, necesita reconciliarse | Mantener como estado global canónico separando frentes |
| `docs/ROADMAP.md` | Roadmap reciente del sitio público | fases de sitio, login, dashboard, deploy | Sitio público + conexión sistema | Parcial, nombre/alcance confuso | Convertir en roadmap global o mover detalle a `docs/01-product/public-website.md` |
| `docs/IMPLEMENTATION_LOG.md` | Bitácora nueva de cambios | inicialización Fase 0 sitio público | General | Vigente pero incompleta | Mantener como bitácora global o enlazar con changelog |
| `docs/ARCHITECTURE.md` | Resumen técnico reciente | stack, estructura, rutas, separación público/privado | Ambos | Duplicado de `docs/03-architecture/*` | Mover/fusionar con arquitectura global en `docs/03-architecture/` |
| `docs/AUTH_FLOW.md` | Resumen reciente de auth | login, cookies, CSRF, permisos, redirecciones | Sistema privado | Duplicado parcial | Mover/fusionar con `docs/03-architecture/authentication-and-authorization.md` |
| `docs/DEPLOYMENT.md` | Resumen reciente de deploy | dominio, variables, build, DNS, checklist | General + sitio público | Duplicado parcial | Fusionar con `docs/06-operations/deployment.md` |
| `docs/RESPONSIVE_CHECKLIST.md` | Checklist mobile-first | viewports, touch, formularios, layout, Lighthouse | Sitio público | Vigente | Mover a `docs/08-qa/RESPONSIVE_CHECKLIST.md` |

### docs/00-governance/

| Ruta | Propósito aparente | Temas principales | Alcance | Estado | Recomendación |
| --- | --- | --- | --- | --- | --- |
| `docs/00-governance/project-status.md` | Estado histórico/canónico previo | MVP administrativo Fase 1 Etapa 7 | Sistema privado + general | Vigente para sistema privado | Fusionar información viva hacia `docs/PROJECT_STATUS.md` o mantener como detalle histórico enlazado |
| `docs/00-governance/roadmap.md` | Roadmap técnico original | Fases 0-5: MVP, migración, inventario, reportes, WhatsApp | Sistema privado + evolución técnica | Vigente como roadmap interno | Reubicar o enlazar desde roadmap global; aclarar que no es roadmap del sitio público |
| `docs/00-governance/changelog.md` | Historial de cambios entregados | etapas implementadas, QA, paquete comercial | General | Vigente | Mantener como changelog de entregas; enlazar con `IMPLEMENTATION_LOG` |
| `docs/00-governance/definition-of-done.md` | Criterios de terminado | código, pruebas, docs, reglas, ADRs | General | Vigente | Mantener en governance y enlazar desde README/AGENTS |

### docs/01-product/

| Ruta | Propósito aparente | Temas principales | Alcance | Estado | Recomendación |
| --- | --- | --- | --- | --- | --- |
| `docs/01-product/business-context.md` | Contexto de negocio | operación basada en Excel, problemas, objetivos | General + sistema privado | Vigente | Mantener |
| `docs/01-product/mvp-scope.md` | Alcance funcional MVP | login, clientes, órdenes, pagos, dashboard, sitio básico | Sistema privado + sitio básico | Vigente con aclaración | Mantener; aclarar diferencia entre sitio básico y sitio corporativo nuevo |
| `docs/01-product/screens-and-flows.md` | Rutas y flujos | rutas públicas/privadas, dashboard, pagos, detalle orden | Ambos, dominante privado | Vigente | Mantener como fuente de flujos privados; enlazar con futura definición del sitio público |
| `docs/01-product/user-roles-and-permissions.md` | Modelo funcional de permisos | Admin, permisos, uso por módulo | Sistema privado | Vigente | Mantener como fuente de producto para permisos |

### docs/02-domain/

| Ruta | Propósito aparente | Temas principales | Alcance | Estado | Recomendación |
| --- | --- | --- | --- | --- | --- |
| `docs/02-domain/business-rules.md` | Reglas de negocio consolidadas | clientes, órdenes, pagos, dashboard, seguridad | Sistema privado | Vigente | Mantener |
| `docs/02-domain/entities.md` | Entidades conceptuales | User, Role, Customer, WorkOrder, Payment, inventario futuro | Sistema privado | Vigente | Mantener |
| `docs/02-domain/payment-rules.md` | Reglas financieras | pagos, saldo, sobrepago, cancelación, permisos | Sistema privado | Vigente | Mantener |
| `docs/02-domain/work-order-lifecycle.md` | Estados de orden | transiciones, terminales, historial | Sistema privado | Vigente | Mantener |
| `docs/02-domain/inventory-rules.md` | Reglas futuras de inventario | materiales, movimientos, stock mínimo | Sistema privado futuro | Parcial/futuro | Mantener como propuesta de fase posterior |

### docs/03-architecture/

| Ruta | Propósito aparente | Temas principales | Alcance | Estado | Recomendación |
| --- | --- | --- | --- | --- | --- |
| `docs/03-architecture/architecture-overview.md` | Vista global técnica | stack, estructura, contratos, pendientes | Ambos | Vigente | Convertir en `docs/03-architecture/ARCHITECTURE.md` o fuente global |
| `docs/03-architecture/authentication-and-authorization.md` | Documento técnico de auth | cookie, CSRF, permisos, guards, riesgos | Sistema privado | Vigente | Fuente canónica de auth; fusionar con `docs/AUTH_FLOW.md` |
| `docs/03-architecture/backend-architecture.md` | Arquitectura backend | capas, EF, endpoints, servicios, dashboard | Sistema privado/API | Vigente | Mantener |
| `docs/03-architecture/database-design.md` | Diseño de base | tablas, índices, relaciones, migraciones | Sistema privado | Vigente | Mantener |
| `docs/03-architecture/frontend-architecture.md` | Arquitectura frontend | rutas, layouts, servicios, guards, módulos | Ambos, dominante privado | Vigente | Mantener; agregar sección de sitio público en fase futura |

### docs/04-decisions/

| Ruta | Propósito aparente | Temas principales | Alcance | Estado | Recomendación |
| --- | --- | --- | --- | --- | --- |
| `docs/04-decisions/ADR-0001-docs-first.md` | Decisión documental | docs-first, trazabilidad | General | Vigente | Mantener |
| `docs/04-decisions/ADR-0002-single-public-site-private-app.md` | Decisión público/privado | mismo dominio, `/`, `/login`, `/app` | Ambos | Vigente | Mantener |
| `docs/04-decisions/ADR-0003-permission-based-authorization.md` | Decisión de permisos | permisos granulares, backend autoritativo | Sistema privado | Vigente | Mantener |
| `docs/04-decisions/ADR-0004-work-orders-as-core-entity.md` | Decisión de entidad central | orden de trabajo como núcleo | Sistema privado | Vigente | Mantener |
| `docs/04-decisions/ADR-0005-cookie-based-authentication.md` | Decisión de auth | cookie HttpOnly para MVP | Sistema privado | Vigente | Mantener |
| `docs/04-decisions/ADR-0006-security-model-and-admin-seed.md` | Decisión de seguridad | User/Role/Permission, seed Admin | Sistema privado | Vigente | Mantener |
| `docs/04-decisions/ADR-0007-csrf-xsrf-protection-for-cookie-auth.md` | Decisión CSRF | XSRF-TOKEN, X-XSRF-TOKEN, login protegido | Sistema privado | Vigente | Mantener |
| `docs/04-decisions/ADR-0008-customer-and-internal-doctor-model.md` | Decisión de clientes | Customer, Clinic, InternalDoctor | Sistema privado | Vigente | Mantener |
| `docs/04-decisions/ADR-0009-work-orders-as-operational-core.md` | Decisión de órdenes | WorkOrder, historial, pagos posteriores | Sistema privado | Vigente histórico | Mantener; no reescribir aunque mencione pagos como etapa posterior |
| `docs/04-decisions/ADR-0010-payments-as-financial-movements.md` | Decisión de pagos | Payment como movimiento, saldo calculado | Sistema privado | Vigente | Mantener |
| `docs/04-decisions/ADR-0011-dashboard-permission-aware-sections.md` | Decisión dashboard | secciones condicionadas por permisos | Sistema privado | Vigente | Mantener |

### docs/05-delivery/

| Ruta | Propósito aparente | Temas principales | Alcance | Estado | Recomendación |
| --- | --- | --- | --- | --- | --- |
| `docs/05-delivery/phase-0-kickoff.md` | Entrega inicial | documentación, stack, hosting, alcance | General histórico | Vigente histórico | Mantener |
| `docs/05-delivery/phase-1-mvp.md` | Plan/estado del MVP administrativo | etapas 1-7, backlog, salida | Sistema privado | Vigente | Mantener como fuente de delivery del MVP privado |
| `docs/05-delivery/phase-2-excel-migration.md` | Fase futura de migración | análisis, mapeo, revisión | Sistema privado futuro | Vigente/futuro | Mantener |
| `docs/05-delivery/phase-3-inventory-suppliers.md` | Fase futura inventario | proveedores, materiales, stock | Sistema privado futuro | Vigente/futuro | Mantener |
| `docs/05-delivery/next-steps.md` | Próximos pasos comerciales | demo, SOW, precio, DNS, prioridad | General/comercial | Vigente | Mantener, pero enlazar desde status global |

### docs/06-operations/

| Ruta | Propósito aparente | Temas principales | Alcance | Estado | Recomendación |
| --- | --- | --- | --- | --- | --- |
| `docs/06-operations/deployment.md` | Estrategia inicial de deploy | servidor, dominio, HTTPS, reverse proxy | General | Vigente parcial | Fuente canónica recomendada de deploy tras fusionar `docs/DEPLOYMENT.md` |
| `docs/06-operations/environments.md` | Ambientes | local, development, production, URLs | General | Parcial/desactualizado | Actualizar puertos y datos detectados en una tarea futura |
| `docs/06-operations/backup-and-restore.md` | Backups | backups, restore, retención | General | Parcial/desactualizado | Actualizar: SQL Server ya es proveedor objetivo; la estrategia productiva sigue pendiente |

### docs/07-imports/

| Ruta | Propósito aparente | Temas principales | Alcance | Estado | Recomendación |
| --- | --- | --- | --- | --- | --- |
| `docs/07-imports/excel-analysis.md` | Análisis inicial del Excel | hojas, campos, riesgos | Sistema privado/migración | Vigente | Mantener |
| `docs/07-imports/excel-migration-strategy.md` | Estrategia de migración | modo revisión, inconsistencias, trazabilidad | Sistema privado/migración | Vigente | Mantener |

### docs/08-qa/

| Ruta | Propósito aparente | Temas principales | Alcance | Estado | Recomendación |
| --- | --- | --- | --- | --- | --- |
| `docs/08-qa/mvp-qa-checklist.md` | QA funcional ejecutada | build, tests, migraciones, flujos admin | Sistema privado | Vigente | Mantener |
| `docs/08-qa/mvp-acceptance-checklist.md` | Criterios aceptación MVP | funcional, técnico, demo | Sistema privado | Vigente | Mantener |
| `docs/08-qa/known-issues.md` | Hallazgos conocidos | runner frontend, zona horaria, SQL local | Sistema privado | Vigente | Mantener |
| `docs/08-qa/demo-data-guide.md` | Datos manuales de demo | clientes, órdenes, pagos esperados | Sistema privado | Vigente | Mantener |
| `docs/08-qa/demo-script.md` | Guion funcional de demo | login, dashboard, clientes, órdenes, pagos | Sistema privado | Vigente | Mantener |

### docs/09-commercial/

| Ruta | Propósito aparente | Temas principales | Alcance | Estado | Recomendación |
| --- | --- | --- | --- | --- | --- |
| `docs/09-commercial/executive-summary.md` | Resumen comercial | MVP, sitio, repartidores, valor | Comercial | Vigente | Mantener |
| `docs/09-commercial/first-round-scope.md` | Alcance de primera ronda | incluido, no incluido, opcionales | Comercial | Vigente propuesta | Mantener |
| `docs/09-commercial/commercial-phases.md` | Fases comerciales | F0 documentación, F1 MVP, F2 sitio, F3 repartidores | Comercial | Vigente | Mantener; aclarar que no es roadmap técnico |
| `docs/09-commercial/scope-matrix.md` | Matriz de alcance | módulos incluidos/opcionales/fuera | Comercial | Vigente | Mantener |
| `docs/09-commercial/delivery-and-acceptance-plan.md` | Plan de entrega comercial | fases, aceptación, producción, capacitación | Comercial | Vigente | Mantener |
| `docs/09-commercial/assumptions-and-exclusions.md` | Supuestos/exclusiones | responsabilidades, límites, alcance | Comercial | Vigente | Mantener |
| `docs/09-commercial/pricing-template.md` | Plantilla económica | partidas, pagos, garantía, soporte | Comercial | Vigente | Mantener |
| `docs/09-commercial/proposal-one-pager.md` | Propuesta ejecutiva breve | qué se construye, beneficios, fases | Comercial | Vigente | Mantener |
| `docs/09-commercial/client-demo-outline.md` | Guion ejecutivo de demo | presentación al cliente y visión futura | Comercial | Vigente | Mantener |
| `docs/09-commercial/repartidores-etiquetas-module.md` | Definición módulo futuro | entregas, QR, PWA, evidencia | Comercial + producto futuro | Vigente propuesta | Mantener aquí; si se contrata, crear doc producto/técnico separado |
| `docs/09-commercial/statement-of-work.md` | SOW base | alcance, entregables, fases, responsabilidades | Comercial/contrato | Vigente propuesta | Mantener |
| `docs/09-commercial/change-control.md` | Control de cambios | clasificación, evaluación, aprobación | Comercial/gestión | Vigente | Mantener |
| `docs/09-commercial/client-responsibilities.md` | Responsabilidades cliente | contenidos, DNS, validación, hardware | Comercial/gestión | Vigente | Mantener |
| `docs/09-commercial/demo-meeting-agenda.md` | Agenda de demo | flujo, preguntas, decisiones, minuta | Comercial/gestión | Vigente | Mantener |

## Inventario De .agents/ Y .codex/

| Ruta | Contenido relevante encontrado | Recomendación |
| --- | --- | --- |
| `.agents/` | Carpeta existe, sin archivos hasta profundidad revisada | No documentar como fuente canónica por ahora |
| `.codex/` | Carpeta existe, sin archivos hasta profundidad revisada | No documentar como fuente canónica por ahora |

## Duplicados Y Solapamientos Encontrados

- `README.md` vs `docs/PROJECT_STATUS.md`: ambos describen estado, pero con alcance distinto.
- `README.md` vs `docs/ROADMAP.md`: README habla de MVP administrativo y próximos pasos comerciales; roadmap nuevo habla del sitio público.
- `README.md` vs `docs/ARCHITECTURE.md` y `docs/03-architecture/*`: stack, rutas, auth, endpoints y estructura aparecen en varios lugares.
- `README.md` vs `docs/AUTH_FLOW.md` y `docs/03-architecture/authentication-and-authorization.md`: flujo de cookie, CSRF y endpoints auth se repite.
- `README.md` vs `docs/DEPLOYMENT.md` y `docs/06-operations/deployment.md`: build, dominio y deploy aparecen duplicados.
- `README.md` vs `docs/08-qa/*`: QA manual y evidencia están resumidos en README y detallados en QA.
- `README.md` vs `docs/09-commercial/*`: paquete comercial está resumido en README y detallado en comercial.
- `docs/ARCHITECTURE.md` vs `docs/03-architecture/architecture-overview.md`: ambos son arquitectura global.
- `docs/AUTH_FLOW.md` vs `docs/03-architecture/authentication-and-authorization.md`: ambos son auth; el segundo es más completo.
- `docs/DEPLOYMENT.md` vs `docs/06-operations/deployment.md`: el primero es más detallado en variables y checklist; el segundo está en la carpeta correcta.
- `docs/RESPONSIVE_CHECKLIST.md` vs `docs/08-qa/*`: no es duplicado funcional, pero debería vivir con QA.
- `docs/ROADMAP.md` vs `docs/00-governance/roadmap.md` vs `docs/09-commercial/commercial-phases.md`: tres roadmaps con fases distintas.
- `docs/IMPLEMENTATION_LOG.md` vs `docs/00-governance/changelog.md`: ambos registran cambios; el changelog tiene la historia real más completa.

## Contradicciones Encontradas

### Estado Del Proyecto

No hay contradicción real si se separa por frente:

- Sistema privado: Fase 1 - MVP operativo, Etapa 7 - QA funcional y demo preparada.
- Sitio público: Fase 0 reciente de preparación documental, con Fase 1 pendiente de diseño mobile-first.

Riesgo: si `docs/PROJECT_STATUS.md` se lee como estado global único, parece contradecir `README.md` y `docs/00-governance/project-status.md`.

### Stack Técnico

No se detectó contradicción importante en stack:

- Angular 21.
- SCSS.
- ASP.NET Core Web API.
- .NET 10.
- EF Core.
- SQL Server.

Matiz: `README.md` dice "permisos granulares por rol"; técnicamente el sistema autoriza por claims de permiso y los roles agrupan permisos.

### Rutas

No se detectó contradicción operativa:

- Público: `/`, `/servicios`, `/contacto`.
- Login: `/login`.
- Privado: `/app` y `/app/dashboard`.
- API: `/api/auth`, `/api/customers`, `/api/work-orders`, `/api/payments`, `/api/dashboard/summary`.

Riesgo: `docs/AUTH_FLOW.md` menciona `/dashboard` como ruta privada sugerida. Está documentado como sugerencia, pero puede confundir porque la ruta real es `/app/dashboard`.

### Autenticación

No hay contradicción importante:

- Cookie HttpOnly.
- CSRF/XSRF con `XSRF-TOKEN` y `X-XSRF-TOKEN`.
- Permisos por claims.
- API devuelve `401`/`403` sin redirigir endpoints `/api`.

### Deploy

Hay documentación parcial y una desactualización menor:

- Dominio `laboratoriodentaltlahuac.com` está consistente.
- Plataforma de deploy sigue pendiente.
- `docs/06-operations/environments.md` dice que faltan puertos y base local; ya hay puertos detectados para API y Angular en documentación/código.
- `docs/06-operations/backup-and-restore.md` dice que la base de datos está pendiente de definir; SQL Server ya es proveedor objetivo, aunque la estrategia productiva de backup sigue pendiente.

### QA

No hay contradicción:

- `docs/08-qa/*` cubre QA funcional del MVP administrativo.
- `docs/RESPONSIVE_CHECKLIST.md` cubre QA mobile-first del sitio público.

Recomendación: agrupar el checklist responsive dentro de `docs/08-qa/`.

### Fases

Hay riesgo alto de confusión por fases con el mismo número:

- Roadmap técnico original: Fase 1 = MVP operativo, Fase 2 = migración Excel, Fase 3 = inventario.
- Roadmap comercial: Fase 2 = sitio web, Fase 3 = repartidores/etiquetas.
- Roadmap nuevo del sitio: Fase 1 = sitio público mobile-first, Fase 3 = dashboard inicial privado.

No es contradicción si cada roadmap queda nombrado por alcance. Sí es un riesgo documental si se dejan todos como "Roadmap" sin prefijo.

### README Frontend Generado

`src/LaboratorioTlahuac.Web/README.md` parece generado por Angular CLI y menciona `ng test`, `ng e2e` y Vitest. El `package.json` actual no define scripts npm de test/e2e/lint. Debe revisarse en una futura limpieza documental.

## Riesgos De Documentación

- Un lector puede pensar que el proyecto "regresó" a Fase 0 cuando solo el sitio público está en Fase 0.
- Los documentos raíz nuevos pueden volverse canónicos por accidente aunque los documentos antiguos sean más completos.
- README puede quedar como copia larga de muchos docs y desactualizarse rápido.
- Los roadmaps pueden mezclarse: técnico, comercial y sitio público.
- Los documentos de deploy están divididos entre raíz y operaciones.
- La bitácora nueva puede duplicar el changelog sin una regla clara.
- AGENTS.md apunta a rutas raíz que podrían cambiar después de consolidar.

## Separación Recomendada

### Sitio Público

Debe documentarse como frente propio:

- Definición funcional en `docs/01-product/public-website.md`.
- Checklist responsive en `docs/08-qa/RESPONSIVE_CHECKLIST.md`.
- Rutas públicas y layout en arquitectura frontend.
- Deploy/dominio en operaciones.

### Sistema Privado

Debe conservar su documentación actual:

- Producto: `docs/01-product/mvp-scope.md`, `screens-and-flows.md`, `user-roles-and-permissions.md`.
- Dominio: `docs/02-domain/*`.
- Arquitectura: `docs/03-architecture/*`.
- QA: `docs/08-qa/mvp-*`, demo y hallazgos.

### Documentación Global

Debe quedar como entrada y control:

- `README.md` breve.
- `AGENTS.md` en raíz.
- `docs/README.md` como índice general.
- `docs/PROJECT_STATUS.md` como estado global separado por frente.
- `docs/ROADMAP.md` como roadmap global con enlaces a roadmaps específicos.
- `docs/IMPLEMENTATION_LOG.md` como bitácora operativa o log de tareas.

### Documentación Comercial

Debe quedarse en `docs/09-commercial/`:

- Propuesta.
- SOW.
- Matriz de alcance.
- Responsabilidades.
- Control de cambios.
- Agenda y guion ejecutivo.

No debe mezclarse con documentación técnica como si todo estuviera implementado.

## Propuesta De Estructura Final

```text
AGENTS.md
README.md
docs/
  README.md
  PROJECT_STATUS.md
  ROADMAP.md
  IMPLEMENTATION_LOG.md
  00-governance/
    definition-of-done.md
    changelog.md
  01-product/
    public-website.md
    internal-system.md
    business-context.md
    screens-and-flows.md
    user-roles-and-permissions.md
  02-domain/
    business-rules.md
    entities.md
    payment-rules.md
    work-order-lifecycle.md
    inventory-rules.md
  03-architecture/
    ARCHITECTURE.md
    AUTH_FLOW.md
    backend-architecture.md
    database-design.md
    frontend-architecture.md
  04-decisions/
    ADR-*.md
  05-delivery/
    phase-0-kickoff.md
    phase-1-mvp.md
    phase-2-excel-migration.md
    phase-3-inventory-suppliers.md
    next-steps.md
  06-operations/
    deployment.md
    environments.md
    backup-and-restore.md
  07-imports/
    excel-analysis.md
    excel-migration-strategy.md
  08-qa/
    RESPONSIVE_CHECKLIST.md
    mvp-qa-checklist.md
    mvp-acceptance-checklist.md
    known-issues.md
    demo-data-guide.md
    demo-script.md
  09-commercial/
    *.md
```

Nota sobre `DEPLOYMENT.md`: aunque la estructura sugerida en la tarea proponía `docs/05-delivery/DEPLOYMENT.md`, la carpeta existente `docs/06-operations/` ya agrupa deploy, ambientes y backup. Por coherencia operativa, la fuente canónica recomendada para deploy es `docs/06-operations/deployment.md`, con enlaces desde delivery si hace falta.

## Fuentes Canónicas Recomendadas Por Tema

| Tema | Fuente canónica recomendada | Rol de README |
| --- | --- | --- |
| Stack | `docs/03-architecture/ARCHITECTURE.md` | Resumen breve |
| Comandos | `README.md` y `docs/06-operations/deployment.md` | Comandos mínimos |
| Auth | `docs/03-architecture/AUTH_FLOW.md` | Enlace |
| Rutas públicas | `docs/01-product/public-website.md` y `docs/03-architecture/frontend-architecture.md` | Resumen |
| Rutas privadas | `docs/01-product/screens-and-flows.md` y `docs/03-architecture/frontend-architecture.md` | Resumen |
| API | `docs/03-architecture/backend-architecture.md` | Enlace y endpoints principales |
| QA | `docs/08-qa/` | Enlace |
| Deploy | `docs/06-operations/deployment.md` | Comando build + enlace |
| Dominio | `docs/PROJECT_STATUS.md` y `docs/06-operations/deployment.md` | Mención breve |
| Roadmap | `docs/ROADMAP.md` | Enlace |
| Estado del proyecto | `docs/PROJECT_STATUS.md` | Resumen muy corto |
| Bitácora | `docs/IMPLEMENTATION_LOG.md` o `docs/00-governance/changelog.md` con roles definidos | Enlace |
| Decisiones | `docs/04-decisions/ADR-*.md` | Enlace |
| Comercial | `docs/09-commercial/` | Enlace |

## Cambios Recomendados Para La Siguiente Tarea

1. Crear `docs/README.md` como índice general.
2. Convertir `docs/PROJECT_STATUS.md` en estado global separado por frentes:
   - Sistema privado.
   - Sitio público.
   - Comercial/deploy.
3. Convertir `README.md` en entrada breve con enlaces canónicos.
4. Fusionar `docs/ARCHITECTURE.md` con `docs/03-architecture/architecture-overview.md`.
5. Fusionar `docs/AUTH_FLOW.md` con `docs/03-architecture/authentication-and-authorization.md`.
6. Fusionar `docs/DEPLOYMENT.md` con `docs/06-operations/deployment.md`.
7. Mover `docs/RESPONSIVE_CHECKLIST.md` a `docs/08-qa/RESPONSIVE_CHECKLIST.md`.
8. Crear `docs/01-product/public-website.md`.
9. Crear o consolidar `docs/01-product/internal-system.md`.
10. Definir la relación entre `docs/IMPLEMENTATION_LOG.md` y `docs/00-governance/changelog.md`.
11. Actualizar `AGENTS.md` solo después de decidir rutas canónicas.
12. Revisar `src/LaboratorioTlahuac.Web/README.md` para evitar comandos genéricos incorrectos.

## Archivos Que NO Deben Tocarse Todavía

- Código fuente en `src/`.
- Pruebas en `tests/`.
- Configuración de build y dependencias.
- Configuración de deploy.
- Migraciones y base de datos.
- Rutas frontend/backend.
- Autenticación, permisos, cookies o CSRF/XSRF.
- ADRs aceptadas, salvo que una tarea futura pida agregar notas de contexto sin reescribir decisiones históricas.
- Documentos comerciales, salvo consolidación explícita de índices/enlaces.

## Conclusión

La documentación no está rota, pero necesita una capa de orden. La prioridad no debe ser reescribir todo, sino nombrar fuentes canónicas, convertir README en índice técnico breve y separar con claridad el avance del sistema privado del arranque del sitio público.
