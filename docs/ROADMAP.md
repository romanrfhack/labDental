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
- Backlog futuro: administración de catálogo, precios e imágenes bajo `/app`. Pendiente; no pertenece a la fase actual y requiere definir permisos administrativos, modelo de datos, endpoints, almacenamiento de imágenes y reglas de publicación antes de implementar. Fuente: `docs/01-product/admin-catalog-management.md`.

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
- Fase sitio 4: publicación en `laboratoriodentaltlahuac.com` cuando deploy/DNS estén definidos. Pendiente.
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

Implementar usuario QA limitado seguro en Development.

Alcance sugerido: si se autoriza tocar backend mínimo, implementar el seed QA limitado solo Development descrito en `docs/08-qa/limited-user-qa-plan.md` para validar `/app/access-denied` con una sesión autenticada sin permisos suficientes. La administración de catálogo, precios e imágenes permanece como backlog futuro, no como fase actual.

## Regla De Actualización

Cada cierre de fase debe actualizar este roadmap, `docs/PROJECT_STATUS.md`, `docs/IMPLEMENTATION_LOG.md` y los documentos canónicos afectados.
