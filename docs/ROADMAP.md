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
- Fase 3.2: MVP impresión de etiquetas desde órdenes existentes. Recomendada como siguiente fase implementable; debe extender `/app/ordenes` sin crear panel duplicado, usando rutas privadas bajo `/app` y CSS de impresión.
- Fase 3.3: entrega/repartidor mobile-first. Pendiente; requiere rol/permisos de reparto, modelo de entrega/asignación, endpoints privados y UI móvil.
- Fase 3.4: administración de usuarios/roles. Pendiente; conviene validar primero seed/usuarios QA y luego CRUD administrativo seguro.
- Fase 3.5: administración de catálogo, precios e imágenes bajo `/app`. Pendiente; requiere definir permisos administrativos, modelo de datos, endpoints, almacenamiento de imágenes y reglas de publicación antes de implementar. Fuente: `docs/01-product/admin-catalog-management.md`.

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
- Backlog relacionado: administración privada futura de catálogo, precios e imágenes. No modifica el catálogo público actual; `/catalogo` sigue usando data frontend hasta diseñar esa fase. Fuente: `docs/01-product/admin-catalog-management.md`.
- Fase sitio/sistema 2.0: validación real del flujo de `/login`, sesión, `returnUrl` y acceso a `/app/dashboard`, sin rediseñar pantallas ni implementar módulos nuevos. Ejecutada por código/build/tests/curl; login real con Admin local y dashboard autenticado fueron cerrados posteriormente en Fase 2.1d.
- Fase sitio 3: QA mobile-first completo y preparación de contenido final. Pendiente.
- Fase sitio 4: publicación productiva en `laboratoriodentaltlahuac.com` cuando deploy/DNS estén definidos. Pendiente; DEV ya está publicado en `https://dev.laboratoriodentaltlahuac.com` como baseline UAT inicial.
- Fase sitio 5: optimización, Lighthouse o revisión equivalente, y mejoras por feedback. Pendiente.

## Roadmap Comercial

Fuente comercial: `docs/09-commercial/commercial-phases.md`.

- Fase comercial 0: planeación y documentación.
- Fase comercial 1: sistema administrativo MVP.
- Fase comercial 2: sitio web corporativo.
- Fase comercial 3: repartidores, entregas y etiquetas.
- Fase comercial 4: QA, capacitación y despliegue.

Las fases comerciales describen propuesta, alcance y aceptación con cliente. No deben usarse como sustituto de la documentación técnica.

## Siguiente Prioridad Técnica

Fase 3.2 - MVP impresión de etiquetas desde órdenes existentes.

Alcance sugerido: extender `/app/ordenes/:id` con acciones para imprimir etiqueta interna y etiqueta de entrega, agregar rutas privadas de impresión bajo `/app/ordenes/:id/etiqueta-trabajo` y `/app/ordenes/:id/etiqueta-entrega`, y usar CSS de impresión en milímetros sin migraciones, sin dependencias nuevas, sin impresora directa y sin PDF obligatorio.

Pendiente paralelo: cerrar validación de usuario QA limitado y `/app/access-denied` en DEV si aún no queda formalmente validada con cuenta limitada real sin `reports.view`.

## Regla De Actualización

Cada cierre de fase debe actualizar este roadmap, `docs/PROJECT_STATUS.md`, `docs/IMPLEMENTATION_LOG.md` y los documentos canónicos afectados.
