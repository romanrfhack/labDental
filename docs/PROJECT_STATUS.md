# Estado Del Proyecto

Última sincronización documental: **2026-08-22 — DOC-SYNC-1**.

Este documento describe el estado vigente. El detalle histórico permanece en `docs/IMPLEMENTATION_LOG.md` y `docs/00-governance/changelog.md`.

## Resumen Ejecutivo

Laboratorio Dental Tláhuac tiene un MVP administrativo privado avanzado y un sitio público institucional implementado en la misma solución Angular/.NET. El ambiente DEV está publicado en `https://dev.laboratoriodentaltlahuac.com` desde la rama `dev` y se utiliza como baseline UAT.

El sitio público ya completó su ciclo reciente de rediseño y optimización:

- `PUB-UX-2`: catálogo público rediseñado como workspace responsive y **aprobado visualmente**.
- `PUB-UX-3`: home, servicios, contacto y header rediseñados, desplegados en DEV y **aprobados visualmente**.
- `PUB-UX-4`: accesibilidad, estabilidad visual, SEO y Lighthouse; integrado a `dev` mediante PR #8, merge `bfa07d0285ca66fab359c151b43ed9458a6b7727`.
- Lighthouse de cierre del árbol integrado: Accesibilidad `100`, Best Practices `100`, SEO `100` en `/`, `/servicios`, `/catalogo` y `/contacto`; Performance entre `91` y `96`.

La administración privada de catálogo, precios e imágenes también está cerrada en DEV hasta Fase 3.5.4 con QA end-to-end aprobado.

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

- Diseño mobile-first aprobado.
- Header público compacto y navegación móvil accesible.
- Home editorial con acceso directo al catálogo y contacto.
- Servicios como directorio hacia categorías reales del catálogo.
- Catálogo administrable consumiendo `GET /api/catalog/public` con fallback local.
- Imágenes de productos administrables y persistentes fuera de releases.
- Selección de categoría compartible mediante hash.
- SEO por ruta y `robots.txt` válido.
- Skip link, foco visible, reduced motion y mejoras de navegación por teclado.

Datos públicos confirmados:

- Teléfonos: `55 3331 9445`, `55 2161 2311`, `55 9802 9816`.
- Correo: `contacto@laboratoriodentaltlahuac.com`.

Datos que siguen sin publicarse por falta de confirmación final:

- Dirección.
- Horarios.
- WhatsApp como canal institucional.
- Redes sociales.
- Mapa/ubicación pública.
- Condiciones comerciales no aprobadas formalmente.

### Sistema Privado

Estado: **MVP operativo avanzado y validado en DEV/UAT**.

Implementado:

- Autenticación por cookie HttpOnly y protección CSRF/XSRF.
- Usuarios, roles y permisos.
- Clientes, doctores y clínicas.
- Órdenes de trabajo.
- Pagos, abonos, cancelación y saldos calculados.
- Dashboard operativo/financiero básico.
- Etiquetas internas y de entrega desde navegador.
- Entregas/repartidor mobile-first con estados, asignación, reintento y cierre.
- Administración privada de catálogo, precios e imágenes.

QA pendiente no bloqueante en DEV:

- Prueba física de etiquetas con impresora térmica real: `76 x 51 mm` y `102 x 51 mm`.
- Validación manual en navegador de un usuario limitado real sin `reports.view` y `/app/access-denied`.
- Prueba forzada del fallback público de catálogo con API bloqueada/offline: opcional.

### Catálogo Administrable

Estado: **Fase 3.5.4 cerrada en DEV con QA end-to-end aprobado**.

- Secciones/productos/precios administrables.
- Activar/desactivar y ordenar.
- Upload/reemplazo/desasociación de imagen de producto.
- Compatibilidad con assets heredados.
- Storage persistente `${LDT_APP_ROOT}/shared/catalog-images`.
- GET público `/api/catalog/images/{fileName}`.
- DELETE desasocia; no elimina físicamente el archivo.

Backlog de imágenes, no defectos del MVP:

- Inventario de huérfanos.
- Política de retención y limpieza segura.
- Backup automatizado.
- Conversión/recompresión WebP.
- Upload de imagen de sección.
- Galería múltiple/CDN/cloud storage.

## Ambientes Y Ramas

### DEV

- Rama: `dev`.
- URL: `https://dev.laboratoriodentaltlahuac.com`.
- Estado: publicado y validado como baseline UAT.
- Deploy: GitHub Actions + VPS con releases inmutables, health checks y rollback.

### Producción

- Rama prevista: `main`.
- Dominio: `https://laboratoriodentaltlahuac.com`.
- Estado: **no desplegado**.
- `dev` contiene una cantidad significativa de trabajo posterior a `main`; no debe promoverse sin fase explícita de readiness.
- El workflow productivo está protegido por `LDT_ENABLE_PROD_DEPLOY == true` y requiere configuración del environment `production`.

## Riesgos / Pendientes Antes De Producción

Prioridad alta:

1. Hardening de cuentas: definir e implementar cambio obligatorio de contraseña temporal en primer acceso antes de usuarios productivos.
2. Base SQL Server productiva y revisión controlada de migraciones.
3. Backup y restore probado de base de datos.
4. Backup y restore conjunto de `shared/catalog-images` con la BD.
5. Variables/secrets del environment `production`.
6. DNS, HTTPS y redirecciones canónicas.
7. Health checks local/público y rollback de producción.
8. Smoke de Admin, Repartidor, catálogo administrable y sitio público.
9. Promoción controlada `dev -> main` únicamente después del checklist productivo.

## Próximo Plan De Trabajo

Fuente: `docs/05-delivery/current-work-plan.md`.

Orden acordado:

1. `DOC-SYNC-1` — reconciliación documental. **Esta actualización lo cierra al integrarse a `dev`.**
2. `OPS-QA-1` — impresora térmica + usuario limitado; fallback catálogo opcional.
3. `PROD-READY-1` — seguridad, infraestructura, backups, DNS/HTTPS y release candidate.
4. `PROD-RELEASE-1` — PR `dev -> main`, despliegue y smoke productivo.
5. `POST-PROD-1` — monitoreo, backups operativos y observación inicial.
6. Después: seleccionar nueva fase funcional entre migración Excel, inventario/proveedores, reportes, automatizaciones/WhatsApp o ampliación de entregas.

## Backlog Funcional Mayor

Pendiente de priorización posterior al release productivo:

- Migración/importación del Excel histórico.
- Inventario y proveedores.
- Reportes administrativos ampliados.
- Automatizaciones y WhatsApp.
- Entregas avanzadas: QR/código, escaneo, evidencia fotográfica/firma e historial de intentos.

## Regla De Fuente De Verdad

Para estado vigente usar, en este orden:

1. `docs/PROJECT_STATUS.md`.
2. `docs/ROADMAP.md`.
3. Fuente funcional/técnica específica del frente.
4. `docs/IMPLEMENTATION_LOG.md` y changelog para historia y evidencia.

Los documentos históricos no deben interpretarse como estado vigente cuando contradigan esta sincronización.