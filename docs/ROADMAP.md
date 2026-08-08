# Roadmap Global

Este roadmap separa tres líneas: sistema privado, sitio público y fases comerciales. Los números de fase pueden repetirse entre líneas; siempre deben leerse con su contexto.

## Sistema Privado / Roadmap Técnico

Fuente detallada: `docs/00-governance/roadmap.md` y `docs/05-delivery/phase-1-mvp.md`.

- Fase técnica 0: documentación y definición inicial. Completada.
- Fase técnica 1: MVP operativo privado. Avanzado, con Etapa 7 de QA funcional y demo documentada.
- Fase técnica 2: migración del Excel. Pendiente.
- Fase técnica 3: inventario y proveedores. Pendiente.
- Fase técnica 4: reportes administrativos. Pendiente.
- Fase técnica 5: automatizaciones y WhatsApp. Pendiente.
- Fase sistema 2.1d: diagnóstico/corrección de loading de `/app/dashboard`. Completada y cerrada manualmente por el responsable del proyecto.
- Fase sistema 2.2: QA manual/técnico del sistema privado con Admin. Completada con reporte en `docs/08-qa/private-admin-qa.md`.
- Fase sistema 2.3: corrección de hallazgos QA del sistema privado. Completada; define zona horaria operativa `America/Mexico_City` para métricas del dashboard y agrega estado activo visual en navegación privada.
- Fase sistema 2.4: pase manual/técnico privado y validación de permisos. Completada por API/código/build con Admin; `/app/access-denied` queda pendiente de usuario limitado real por falta de mecanismo seguro de usuario QA limitado.
- Fase sistema 2.5: cierre visual humano del sistema privado y definición de mecanismo seguro para usuario QA limitado. Completada para pase visual humano privado el 2026-05-28; se recomienda seed QA limitado solo Development como backlog técnico inmediato, sin implementarlo todavía.
- Fase sistema 2.6: mecanismo seguro Development-only para usuario QA limitado. Implementada; seed desactivado por default, activado por configuracion explicita, probado por automatizacion API y pendiente de pase manual en navegador con credenciales locales reales.
- Fase 3.0: cierre formal del despliegue DEV y baseline UAT. Completada el 2026-07-02 como fase documental/de validación; `https://dev.laboratoriodentaltlahuac.com` queda registrado como DEV publicado desde rama `dev`, con sitio público, `/login`, login QA, `/app/dashboard` autenticado y redirección sin sesión a `/login` validados manualmente.
- Fase 3.1: análisis operativo y plan de implementación para órdenes, etiquetas, reparto, usuarios/roles y catálogo. Documentada; no implementa código, migraciones, endpoints, auth, guards, deploy ni base de datos. Fuentes: `docs/01-product/operations-orders-delivery.md`, `docs/01-product/label-printing.md` y `docs/01-product/driver-mobile-workflow.md`.
- Fase 3.2: MVP impresión de etiquetas desde órdenes existentes. Implementada el 2026-07-02; extiende `/app/ordenes/:id` con acciones de etiqueta interna y entrega, agrega rutas privadas bajo `/app` protegidas con `orders.view`, usa CSS/browser print con tamaños 76 x 51 mm y 102 x 51 mm, y no agrega dependencias, migraciones, endpoints ni PDF/QR/barcode.
- Fase 3.2.1: QA técnico/visual de etiquetas y preparación de despliegue DEV. Completada el 2026-07-02 por revisión técnica local, build/test y checklist físico; no tuvo hallazgos bloqueantes. La prueba visual local quedó limitada por falta de navegador/headless sin instalar dependencias y la impresión física queda pendiente en DEV.
- Fase 3.3: administración de usuarios/roles MVP para DEV/UAT. Implementada el 2026-07-03; `/app/admin/usuarios` permite listar, crear, editar datos básicos, activar/desactivar, asignar roles existentes y setear contraseña temporal sin exponerla en respuestas; `/app/admin/roles` queda funcional readonly con permisos por rol; `Repartidor` queda preparado como rol sin permisos amplios. No implementa reparto, entregas, catálogo ni recuperación por correo.
- Fase 3.3.1: QA de seguridad, validación técnica y preparación de despliegue DEV para usuarios/roles. Completada el 2026-07-03 sin bloqueantes; confirma appsettings sin secretos reales, endpoints admin protegidos con `users.manage`/`roles.manage`, `401` sin sesión en los nueve endpoints, pruebas API 110/110, build frontend con warning de budget no bloqueante y pendiente force-change password antes de producción.
- Fase 3.4.0: análisis técnico previo de entrega/repartidor mobile-first. Documentada el 2026-07-03; recomienda entidad separada `WorkOrderDelivery`, estados logísticos propios, ruta `/app/entregas`, endpoints `/api/deliveries/*` y permisos `deliveries.view`, `deliveries.assign`, `deliveries.update` y `deliveries.complete`. No implementa código, migraciones, endpoints, permisos reales, auth, guards, cookies, XSRF, deploy ni dependencias.
- Fase 3.4.1: backend delivery MVP + permisos. Implementada y desplegada a DEV el 2026-07-04; agrega `WorkOrderDelivery`, `DeliveryStatus`, migración `AddWorkOrderDeliveries`, endpoints `/api/deliveries` y `/api/work-orders/{workOrderId}/delivery`, permisos `deliveries.view`, `deliveries.assign`, `deliveries.update` y `deliveries.complete`, y rol `Repartidor` con permisos mínimos `deliveries.view`/`deliveries.complete`. Deploy DEV: commit `e4c28205c6b866ab0d71edb13c49164100340b0d`, GitHub Actions run `28712956106`, resultado `success`; `/health` responde `200` y `/api/deliveries` sin sesión responde `401`.
- Fase 3.4.1.1: QA técnico de migración delivery, permisos y endpoints reales. Completada el 2026-07-04 contra SQL local `ldt-labdental-sql`/`LaboratorioTlahuac_Dev`; aplica migración local, valida API real con Admin/Repartidor QA y corrige sincronización baseline de permisos faltantes para rol Admin existente. El pendiente de base DEV quedó cerrado o no aplica porque `/api/deliveries` ya responde `401` en DEV.
- Fase 3.4.1.2: cierre documental de deploy DEV y lazy loading. Completada el 2026-07-04; confirma Delivery API desplegada/protegida, lazy loading desplegado, warning de initial bundle resuelto de `535.62 kB` a `304.19 kB` y validación manual Admin en DEV como pendiente.
- Fase 3.4.2: UI admin desde órdenes para asignar, registrar salida/estado y ver seguimiento. Implementada el 2026-07-04 en `/app/ordenes/:id`; consume endpoints delivery existentes, reutiliza endpoints admin para candidatos de repartidor y no crea rutas nuevas. Cierre operativo DEV: GitHub Actions de commit `97d46e9` falló por health check `502`, rollback dejó activo `dev-23-eea8f39`, y `dev-24-97d46e9` se validó manualmente en puerto alterno `5013` y se activó ajustando `backend/current`; health final `200` y `/api/deliveries` sin sesión `401`.
- Fase 3.4.2.1: estado de entrega en listado/grid de órdenes. Implementada el 2026-07-04; `GET /api/work-orders` incluye resumen `delivery` opcional sin cambiar `WorkOrder.Status`, `/app/ordenes` muestra `Estado` de orden y badge `Entrega` por separado, las órdenes sin entrega muestran `Sin entrega` y `FailedDelivery` se muestra como `No entregada`. No crea migraciones, endpoints nuevos, rutas nuevas ni cambios de auth/deploy.
- Fase 3.4.3: UI repartidor mobile-first bajo `/app/entregas`. Implementada el 2026-07-04; agrega rutas privadas `/app/entregas` y `/app/entregas/:id` con `deliveries.view`, listado mobile-first con `assignedToMe=true`, detalle de entrega asignada al usuario autenticado, cierre con `recipientName`, no entrega con `failedReason` y lectura sin acciones si falta `deliveries.complete`.
- Fase 3.4.3.1: redirect post-login por permisos y reintento de entrega fallida. Implementada y validada en DEV el 2026-07-05; sin `returnUrl` interno válido el login usa la ruta inicial por permisos (`reports.view` primero y `deliveries.view -> /app/entregas` para `Repartidor`), `/app/access-denied` enlaza a `Ir a mi inicio`, y `PATCH /api/deliveries/{id}/retry` permite pasar `FailedDelivery` a `OutForDelivery` sin cambiar `WorkOrder.Status`. Admin/operación reintenta con `deliveries.update`; repartidor asignado reintenta con `deliveries.complete`. QA DEV: commit `59542efd4f57df7ba04a2444c5496040810d1702`, GitHub Actions `success`, `/health` `200`, `/api/deliveries` sin sesión `401`, resultados Repartidor/Admin OK y sin observaciones reportadas. No crea migraciones, no agrega dependencias y no toca cookies/XSRF.
- Fase 3.4.4: pulido UX operativo de entregas. Implementada y validada en DEV el 2026-07-05; `/app/entregas` agrega filtros `Todas`, `En ruta`, `Asignadas`, `No entregadas` y `Entregadas`, contadores por estado operativo, cards mobile-first más claras y detalle con teléfono `tel:`, WhatsApp y Google Maps solo cuando existen datos. QA DEV: GitHub Actions `success`, `/health` `200`, `/api/deliveries` sin sesión `401`, resultados de Repartidor OK y sin observaciones reportadas. No cambia backend, migraciones, dependencias, permisos, rutas privadas, auth, guards, cookies ni XSRF.
- Fase 3.5.0: diseño técnico del catálogo administrable. Documentada; incluye inventario del catálogo público actual, confirma que `/catalogo` usa `catalog-data.ts`, propone modelo `CatalogSection`/`CatalogProduct`, permisos `catalog.view`/`catalog.manage` con `catalog.publish` opcional, endpoints público/admin, estrategia de imágenes y fases 3.5.1 a 3.5.4. Recomendación MVP: backend + migración + seed inicial y selección de imágenes existentes; upload queda para fase posterior con almacenamiento y backup definidos. No implementa código, migraciones, endpoints, frontend funcional, dependencias ni deploy.
- Ajuste técnico de despliegue DEV: cerrado el 2026-08-08. El workflow espera health local y público con reintentos después del restart; el commit `8be9e14ec8cda5e8486770a77733a4413e456e96` desplegó con GitHub Actions `success` y `/health` respondió `200`.
- Fase 3.5.1: backend catálogo administrable + migración + seed inicial. Implementada y cerrada en DEV el 2026-07-05; agrega `CatalogSection`, `CatalogProduct`, migración `AddCatalogManagement`, seed idempotente desde `catalog-data.ts`, permisos `catalog.view`/`catalog.manage`, `GET /api/catalog/public` sin autenticación y endpoints admin bajo `/api/admin/catalog`. QA DEV: commit `ebcf6e54b77ec6c5afaafdf8c21afc77213bf9d8`, GitHub Actions `success`, `/health` `200`, `/api/catalog/public` sin sesión `200`, `/catalogo` `200` y endpoints admin sin sesión `401`. No cambia `/catalogo`, no crea UI admin y no implementa upload.
- Fase 3.5.2: UI admin catálogo/precios con selección de imagen existente. Implementada y validada en DEV el 2026-07-05 bajo `/app/admin/catalogo`; agrega modelos y servicio frontend de catálogo, navegación privada `Catálogo`, lectura de secciones/productos, filtros por sección/estado, creación/edición/activación de secciones y productos, actualización rápida de precio, selección de `imagePath` desde assets `.webp` existentes, preview y modo readonly para `catalog.view` sin `catalog.manage`. QA DEV: commit `e89d1f0b872d253838dc77f5df5fafb61522f9db`, GitHub Actions `success`, `/health` `200`, endpoints admin sin sesión `401`, flujo Admin OK, precio negativo bloqueado OK, selección/preview/limpieza de imagen OK, `/catalogo` público OK y `Repartidor` sin navegación/acceso OK. No cambia `/catalogo`, no implementa upload, no crea migraciones, no toca backend, `AuthService`, guards, cookies, XSRF, deploy ni dependencias.
- Fase 3.5.3: catálogo público consume API con fallback. Implementada el 2026-07-10 y cerrada en DEV el 2026-08-08 con commit `8be9e14ec8cda5e8486770a77733a4413e456e96`, GitHub Actions `success`, `/health` `200`, `/catalogo` `200` y `/api/catalog/public` sin sesión `200`. Activar/desactivar productos y cambiar nombre/precio desde admin se reflejó correctamente en `/catalogo`. El fallback forzado offline no se probó en DEV y queda como cobertura manual opcional. No cambia UI admin, backend, migraciones, upload, `AuthService`, guards, cookies, XSRF ni dependencias.

## Sitio Público Institucional

Fuente funcional: `docs/01-product/public-website.md`.

- Fase sitio 0: preparación documental y consolidación. Completada con Fase 0.2.
- Fase sitio 1: primera versión mobile-first para `/`, `/servicios` y `/contacto`. Implementada.
- Fase sitio 1.1: revisión responsive técnica ejecutada; revisión visual por viewport cubierta posteriormente por Fase 1.6.
- Fase sitio 1.2: contenido aprobado y reemplazo de placeholders. Parcial; faltan datos reales confirmados.
- Fase sitio 1.3: catálogo público con precios e imágenes locales en `/catalogo`. Implementada.
- Fase sitio 1.3.1: cierre técnico del catálogo, limpieza de assets `:Zone.Identifier`, documentación de placeholders y preparación para revisión visual del cliente. Implementada por código/documentación; revisión visual real cubierta posteriormente por Fase 1.6.
- Fase sitio 1.5: identidad visual LDT, logo, tokens de marca y datos de contacto del cartel/catálogo. Implementada; validación visual cubierta posteriormente por Fase 1.6 y aprobación comercial del cliente pendiente.
- Fase sitio 1.6: pulido visual premium del sitio público con CSS/IntersectionObserver, reveal sutil, parallax ligero, microinteracciones y mejoras de composición en home, servicios, catálogo, contacto y login visual. Completada y validada visualmente por el responsable del proyecto.
- Backlog relacionado: administración privada de catálogo, precios e imágenes. Fase 3.5.2 dejó UI admin bajo `/app/admin/catalogo` y quedó validada en DEV; Fase 3.5.3 quedó cerrada en DEV con `/catalogo` consumiendo `GET /api/catalog/public` y conservando `catalog-data.ts` como fallback. Fuente: `docs/01-product/admin-catalog-management.md`.
- Fase sitio/sistema 2.0: validación real del flujo de `/login`, sesión, `returnUrl` y acceso a `/app/dashboard`, sin rediseñar pantallas ni implementar módulos nuevos. Ejecutada por código/build/tests/curl; login real con Admin local y dashboard autenticado fueron cerrados posteriormente en Fase 2.1d.
- Fase sitio 3: QA mobile-first completo y preparación de contenido final. Pendiente.
- Fase sitio 4: publicación productiva en `laboratoriodentaltlahuac.com` cuando deploy/DNS estén definidos. Pendiente; DEV ya está publicado en `https://dev.laboratoriodentaltlahuac.com` como baseline UAT inicial.
- Fase sitio 5: optimización, Lighthouse o revisión equivalente, y mejoras por feedback. Pendiente; la optimización técnica de lazy loading de rutas ya quedó desplegada en DEV y eliminó el warning de initial bundle sin cambiar budgets.

## Roadmap Comercial

Fuente comercial: `docs/09-commercial/commercial-phases.md`.

- Fase comercial 0: planeación y documentación.
- Fase comercial 1: sistema administrativo MVP.
- Fase comercial 2: sitio web corporativo.
- Fase comercial 3: repartidores, entregas y etiquetas.
- Fase comercial 4: QA, capacitación y despliegue.

Las fases comerciales describen propuesta, alcance y aceptación con cliente. No deben usarse como sustituto de la documentación técnica.

## Siguiente Prioridad Técnica

Fase 3.5.4, carga/reemplazo de imágenes desde admin con política de almacenamiento, validación y backup, o una fase corta de pulido QA del catálogo público si aparecen hallazgos visuales.

Fase 3.5.3 queda cerrada en DEV: `/catalogo` consume `GET /api/catalog/public`, refleja cambios administrados y conserva fallback local con `catalog-data.ts`.

La carga/reemplazo de imágenes desde admin queda para Fase 3.5.4. Pendiente paralelo: cerrar validación de usuario QA limitado y `/app/access-denied` si aún no queda formalmente validada con cuenta limitada real sin `reports.view`.

## Regla De Actualización

Cada cierre de fase debe actualizar este roadmap, `docs/PROJECT_STATUS.md`, `docs/IMPLEMENTATION_LOG.md` y los documentos canónicos afectados.
