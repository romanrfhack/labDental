# Estado Del Proyecto

Última sincronización documental: **2026-10-06 — integración DEV SEC-PERM-1**.

Este documento describe el estado vigente. El detalle histórico permanece en `docs/IMPLEMENTATION_LOG.md` y `docs/00-governance/changelog.md`.

## Candidata Consolidada — 2026-10-06

- Candidata integrada a DEV mediante PR #10: `codex/sec-perm-1-consolidated-20261006`.
- Base funcional elegida: `codex/sec-perm-1` SHA `31d4541129828028a244083a46c702899a72889e`; merge DEV `16b22fa4ad84749bed5b0c252e91671637b8b352`.
- La rama alternativa `sec-perm-1-admin-permissions` queda como antecedente, no candidata a merge; no mezclar sus endpoints ni su migración.
- HEAD `949808f`: checks 37554298114 y 37554293279 exitosos; 164 tests. Deploy VPS 37555301511 correcto; migración aplicada y health local/público 200. El primer workflow falló únicamente en el helper Python post-QA; corrección `13662f5` desplegada y workflow 37555582224 exitoso, con comprobación BD y smoke anónimo PASS. QA autenticado BLOCKED.
- Se amplían pruebas de retorno a herencia, rechazo de overrides inválidos y protección `401/403/XSRF`.
- QA usuario limitado del 2026-09-07: cierre documental histórico conservado; no se presenta como repetido en esta revisión.
- Revisión HTTP 2026-10-06: DEV `/health` y `/api/catalog/public` 200, `/api/auth/me` sin sesión 401. Dominio principal: mismos endpoints 502; causa pendiente de diagnóstico.
- Revisión y pendientes: `docs/08-qa/sec-perm-1-consolidation.md`.

## Resumen Ejecutivo

Laboratorio Dental Tláhuac tiene un MVP administrativo privado avanzado y un sitio público institucional implementado en la misma solución Angular/.NET. El ambiente DEV está publicado en `https://dev.laboratoriodentaltlahuac.com` desde la rama `dev` y se utiliza como baseline UAT.

El sitio público está aprobado visual y técnicamente en DEV. La administración privada de catálogo, precios e imágenes también está cerrada en DEV hasta Fase 3.5.4 con QA end-to-end aprobado.

El frente activo es seguridad/readiness:

- `OPS-QA-1`: la validación manual de usuario limitado real quedó completada el 2026-09-07; sólo faltan las pruebas físicas de etiquetas `76 x 51 mm` y `102 x 51 mm`.
- `SEC-PERM-1`: implementación autorizada y construida en `codex/sec-perm-1-consolidated-20261006`; incluye edición de permisos por rol, overrides individuales `Allow/Deny`, actualización de permisos de sesiones existentes, protección de Admin y corrección del render duplicado de Clientes. Integrado y desplegado a `dev`; QA autenticado/visual pendiente de acceso.
- Producción continúa **sin desplegar**.

## Estado Por Frente

### Sitio Público

Estado: **cerrado funcional y visualmente en DEV; pendiente promoción a producción**.

Rutas públicas vigentes:

- `/`
- `/servicios`
- `/catalogo`
- `/contacto`
- `/login`

Características vigentes:

- diseño mobile-first aprobado;
- catálogo administrable consumiendo `GET /api/catalog/public` con fallback local;
- imágenes de productos administrables y persistentes fuera de releases;
- SEO por ruta, `robots.txt`, skip link, foco visible y reduced motion.

### Sistema Privado

Estado: **MVP operativo avanzado y validado en DEV/UAT; hardening de permisos desplegado; aceptación autenticada pendiente**.

Implementado en `dev`:

- autenticación por cookie HttpOnly y protección CSRF/XSRF;
- usuarios, roles y permisos basados en roles;
- clientes, doctores y clínicas;
- órdenes de trabajo;
- pagos, abonos, cancelación y saldos calculados;
- dashboard operativo/financiero básico;
- etiquetas internas y de entrega desde navegador;
- entregas/repartidor mobile-first;
- administración privada de catálogo, precios e imágenes.

`SEC-PERM-1` integrado en DEV:

- permisos por rol editables desde UI;
- herencia dinámica de permisos al crear/asignar roles;
- overrides individuales `Allow` / `Deny` con estado normal `Heredado`;
- cálculo efectivo `roles + Allow - Deny`;
- rol Admin protegido;
- refresco de principal/cookie contra la BD para que cambios de permisos surtan efecto sin nuevo login;
- `Security.UserPermissionOverrides` mediante migración EF generada por tooling;
- baseline seed deja de sobrescribir permisos administrados de `Repartidor`;
- fix del render duplicado desktop/responsive de Clientes.

### QA Operativo

Usuario limitado real en DEV: **completado**.

Evidencia confirmada el 2026-09-07:

- login correcto;
- `/api/auth/me`: `customers.view=true`, `reports.view=false`;
- `/app/clientes`: accesible;
- `/app/dashboard`: `/app/access-denied`;
- `/api/customers`: `200` autenticado;
- `/api/dashboard/summary`: `403` autenticado y `401` sin credenciales;
- logout invalida sesión;
- configuración temporal de Limited QA retirada del entorno después de la prueba.

Pendiente obligatorio de `OPS-QA-1`:

- impresión física real `76 x 51 mm`;
- impresión física real `102 x 51 mm`;
- validar escala, márgenes, orientación, corte, offset, contraste y nitidez.

### SEC-PERM-1

Estado: **integrado y desplegado en DEV por PR #10; pendiente QA autenticado/UAT**.

Validación automática de rama:

- build backend: correcto;
- tests backend: correctos;
- build Angular: correcto;
- migración `AddUserPermissionOverrides` generada por `dotnet ef` con `.Designer.cs` y snapshot;
- `dotnet ef migrations has-pending-model-changes`: correcto;
- pruebas agregadas para grant/revoke por rol, Allow/Deny individual, sesión existente, Admin protegido y preservación de `Repartidor` frente al baseline seed.

No se ha promovido a `main` ni se ha desplegado a producción.

## Ambientes Y Ramas

### DEV

- rama: `dev`;
- URL: `https://dev.laboratoriodentaltlahuac.com`;
- estado: publicado y baseline UAT;
- deploy: GitHub Actions + VPS con health checks y rollback.

### Rama De Trabajo SEC-PERM-1

- rama: `codex/sec-perm-1-consolidated-20261006`;
- base original: `dev` SHA `25e1ec41109fc5cbca470a64afcfa3ed782bc44d`;
- estado: integrada por PR #10; conservada como antecedente verificable.

### Producción

- rama prevista: `main`;
- dominio: `https://laboratoriodentaltlahuac.com`;
- estado: **no desplegado**;
- no promover `dev -> main` antes de cerrar `PROD-READY-1`.

## Riesgos / Pendientes Antes De Producción

Prioridad alta:

1. Completar QA autenticado de `SEC-PERM-1` en DEV con Admin y usuario no Admin.
2. Completar pruebas físicas de etiquetas de `OPS-QA-1`.
3. Hardening de cuentas: cambio obligatorio de contraseña temporal en primer acceso o política equivalente aprobada.
4. Revisar cookies `Secure`, sesión, permisos y respuestas `401/403` en release candidate.
5. SQL Server productivo, migraciones revisadas y backup previo.
6. Backup/restore probado de BD y `shared/catalog-images`.
7. Variables/secrets del environment `production`.
8. DNS, HTTPS, health checks y rollback.
9. Smoke de Admin, Repartidor, permisos, catálogo y sitio público.
10. Promoción `dev -> main` únicamente después del checklist `PROD-READY-1` y autorización explícita.

## Próximo Plan De Trabajo

Fuente: `docs/05-delivery/current-work-plan.md`.

Orden vigente:

1. `DOC-SYNC-1` — **cerrado**.
2. `OPS-QA-1` — usuario limitado **cerrado**; hardware de impresión **pendiente**.
3. `SEC-PERM-1` — **integrado en DEV; pendiente QA autenticado/visual**.
4. `PROD-READY-1` — seguridad de cuentas, infraestructura, backups, DNS/HTTPS y release candidate.
5. `PROD-RELEASE-1` — PR `dev -> main`, despliegue y smoke productivo.
6. `POST-PROD-1` — monitoreo y estabilización.

## Backlog Funcional Mayor

Posterior al primer release productivo salvo decisión explícita:

- migración/importación del Excel histórico;
- inventario y proveedores;
- reportes administrativos ampliados;
- automatizaciones y WhatsApp;
- entregas avanzadas: QR, escaneo, evidencia fotográfica/firma e historial de intentos.

## Regla De Fuente De Verdad

Para estado vigente usar, en este orden:

1. `docs/PROJECT_STATUS.md`;
2. `docs/ROADMAP.md`;
3. fuente funcional/técnica específica del frente;
4. `docs/IMPLEMENTATION_LOG.md` y changelog para historia y evidencia.

## 2026-10-06 — Preflight e integración DEV SEC-PERM-1

Se agrega preflight DEV por SSH con secrets del environment existente: consulta `__EFMigrationsHistory`, verifica consistencia de tabla/registro y rechaza migración alternativa o estado inesperado. Antes de integrar, crea backup COPY_ONLY con CHECKSUM e intenta RESTORE VERIFYONLY con identidad privilegiada identificable; no restaura ni altera datos de negocio. El preflight 37554079112 pasó con las seis migraciones esperadas y sin tabla de overrides. Backup creado; RESTORE VERIFYONLY bloqueado por identidad SQL privilegiada no disponible. Credenciales Admin ausentes: QA autenticado pendiente de acceso seguro.

Tras deploy DEV se consulta migración aplicada y se ejecuta QA HTTP sin sesión; si las credenciales Admin configuradas están disponibles y son válidas, se valida Clientes, protección Admin, guardado idempotente de Repartidor y overrides de usuario QA aislado por API. Se prueba Allow/Deny/herencia en sesión abierta, logout y limpieza (overrides vacíos + usuario desactivado). No cambia permisos efectivos de usuarios operativos. Falta QA visual de navegador; ausencia/rechazo de credenciales se registra como BLOCKED y no se presenta como PASS. Evidencia de cada ejecución en Actions.


## 2026-10-06 — Resultado final DEV SEC-PERM-1

Workflow [37555582224](https://github.com/romanrfhack/labDental/actions/runs/37555582224) exitoso sobre `13662f557c013cf9e3e1c461219907e45bb54731`. 164 tests correctos, builds backend/frontend correctos, SQL idempotente y deploy VPS correctos. Health local/público 200. Consulta real post-deploy: siete migraciones esperadas, incluida `20260908031302_AddUserPermissionOverrides`; tabla presente y columnas UserId/PermissionId/Effect. Smoke anónimo health/catalog 200, auth/me y customers 401. Comprobación adicional roles/dashboard sin sesión 401; navegador Clientes redirige a login.

QA autenticado y visual: **BLOCKED**, no PASS; faltan Admin/usuario no Admin, grant/revoke por rol, Allow/Deny/Heredado con sesión abierta, logout y Clientes desktop/móvil en DEV. Sin credenciales Admin configuradas; solicitud segura de login cancelada. No se alteraron permisos operativos ni se crearon usuarios de prueba. Backup previo COPY_ONLY/CHECKSUM creado; VERIFYONLY y restauración completa pendientes. No se promueve producción. Evidencia documental final no cambia código ni requiere nuevo despliegue.
