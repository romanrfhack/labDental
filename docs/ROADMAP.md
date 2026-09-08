# Roadmap Global

Última sincronización: **2026-09-07 — SEC-PERM-1**.

Este roadmap prioriza el trabajo vigente. El roadmap histórico permanece en `docs/00-governance/roadmap.md` y la evidencia de implementación en `docs/IMPLEMENTATION_LOG.md`.

## 1. Estado Actual

### Sistema Privado

Implementado y validado en DEV/UAT:

- login, sesión, CSRF/XSRF y autorización por permisos;
- clientes, doctores y clínicas;
- órdenes, estados e historial;
- pagos, abonos, cancelación y saldos;
- dashboard básico;
- usuarios y roles;
- etiquetas de trabajo y entrega;
- entregas/repartidor mobile-first;
- catálogo administrable con precios e imágenes persistentes.

QA operativo:

- usuario limitado real y `/app/access-denied`: **completado 2026-09-07**;
- impresión térmica física real `76 x 51 mm` y `102 x 51 mm`: **pendiente**.

Seguridad de permisos:

- `SEC-PERM-1` está implementado en `codex/sec-perm-1` y pendiente de integración/QA en DEV.

### Sitio Público

Estado en DEV: **aprobado visual y técnicamente**.

- `PUB-UX-2`: catálogo workspace responsive — cerrado.
- `PUB-UX-3`: home/servicios/contacto/header — cerrado.
- `PUB-UX-4`: accesibilidad, SEO y Lighthouse — cerrado e integrado a `dev`.

## 2. Plan Vigente Priorizado

### DOC-SYNC-1 — Reconciliación Documental

Estado: **cerrado**.

### OPS-QA-1 — QA Operativo Pendiente

Estado: **en curso sólo por hardware de impresión**.

Completado:

- cuenta real sin `reports.view`;
- login correcto;
- `/app/dashboard -> /app/access-denied`;
- módulo permitido operativo;
- backend `200/403/401` validado;
- logout y retiro del seed temporal confirmados.

Pendiente:

- etiqueta interna `76 x 51 mm` en impresora real;
- etiqueta entrega `102 x 51 mm` en impresora real;
- escala, márgenes, orientación, corte, offset, contraste y nitidez.

Cobertura opcional:

- fallback de `/catalogo` con `GET /api/catalog/public` bloqueado.

### SEC-PERM-1 — Administración De Roles Y Permisos

Estado: **implementado en rama; pendiente PR/deploy DEV y QA manual**.

Objetivo:

Completar la administración granular de seguridad antes de producción.

Entregables:

- edición de permisos por rol desde UI;
- Admin protegido y con todos los permisos;
- nuevos usuarios heredan permisos de sus roles sin duplicarlos;
- overrides individuales `Allow` / `Deny`;
- UI triestado `Heredado / Permitir / Denegar` con origen del permiso;
- cambios efectivos en sesiones abiertas sin requerir nuevo login;
- baseline seed no sobrescribe permisos administrados de `Repartidor`;
- migración `Security.UserPermissionOverrides` generada con EF;
- cobertura automática de grant/revoke, overrides, Admin y sesión;
- corrección visual de tabla duplicada de Clientes.

Criterio de cierre:

- branch CI verde;
- PR a `dev` revisado;
- deploy DEV saludable;
- smoke Admin: editar rol y usuario;
- smoke usuario limitado: navegación/API responden según permisos efectivos;
- no hay regresión de `401/403` ni de herencia de roles.

### PROD-READY-1 — Preparación Para Producción

Estado: **pendiente; no promover a `main` antes de cerrarla**.

Dependencias:

- `SEC-PERM-1` cerrado en DEV;
- `OPS-QA-1` sin pendiente físico bloqueante.

Alcance mínimo:

- cambio obligatorio de contraseña temporal en primer acceso o política equivalente;
- revisión final de cookies, sesión y permisos;
- environment `production` completo;
- SQL Server productivo;
- migraciones revisadas e idempotentes;
- backup de BD antes de migraciones;
- restore probado en ambiente no productivo;
- backup/restore conjunto BD + `shared/catalog-images`;
- DNS, `www`, HTTPS y redirecciones canónicas;
- health checks y rollback;
- smoke Admin, Repartidor, permisos, catálogo y sitio público;
- release candidate identificado por SHA.

### PROD-RELEASE-1 — Primera Publicación Productiva

Estado: **pendiente**.

Dependencia: `PROD-READY-1` cerrada.

Flujo:

1. congelar release candidate en `dev`;
2. PR `dev -> main`;
3. revisar diff acumulado;
4. habilitar conscientemente deploy productivo;
5. aplicar migraciones según procedimiento aprobado;
6. health checks y smoke;
7. confirmar DNS/HTTPS;
8. aceptación o rollback.

### POST-PROD-1 — Estabilización Inicial

Estado: **pendiente**.

- monitoreo de errores/disponibilidad;
- backups automáticos;
- restore periódico de prueba;
- crecimiento de imágenes;
- feedback de usuarios reales.

## 3. Roadmap Funcional Posterior

No adelantar salvo decisión explícita:

- migración del Excel;
- inventario y proveedores;
- reportes administrativos;
- automatizaciones y WhatsApp;
- entregas avanzadas con QR/escaneo/evidencia;
- ciclo de vida avanzado de imágenes.

## 4. Regla De Priorización

Secuencia vigente:

`OPS-QA-1 (hardware) + SEC-PERM-1 -> PROD-READY-1 -> PROD-RELEASE-1 -> POST-PROD-1 -> nueva fase funcional`

`SEC-PERM-1` puede avanzar mientras se espera el hardware porque es hardening de seguridad previo a producción.

**No fusionar `dev -> main` antes de cerrar `PROD-READY-1`.**
