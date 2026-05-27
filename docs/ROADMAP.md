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
- Backlog futuro: administración de catálogo, precios e imágenes bajo `/app`. Pendiente; no pertenece a la fase actual y requiere definir permisos administrativos, modelo de datos, endpoints, almacenamiento de imágenes y reglas de publicación antes de implementar. Fuente: `docs/01-product/admin-catalog-management.md`.

## Sitio Público Institucional

Fuente funcional: `docs/01-product/public-website.md`.

- Fase sitio 0: preparación documental y consolidación. Completada con Fase 0.2.
- Fase sitio 1: primera versión mobile-first para `/`, `/servicios` y `/contacto`. Implementada.
- Fase sitio 1.1: revisión responsive técnica ejecutada; revisión visual por viewport pendiente.
- Fase sitio 1.2: contenido aprobado y reemplazo de placeholders. Parcial; faltan datos reales confirmados.
- Fase sitio 1.3: catálogo público con precios e imágenes locales en `/catalogo`. Implementada.
- Fase sitio 1.3.1: cierre técnico del catálogo, limpieza de assets `:Zone.Identifier`, documentación de placeholders y preparación para revisión visual del cliente. Implementada por código/documentación; revisión visual real pendiente.
- Fase sitio 1.5: identidad visual LDT, logo, tokens de marca y datos de contacto del cartel/catálogo. Implementada; revisión visual real y aprobación comercial del cliente pendientes.
- Fase sitio 1.6: pulido visual premium del sitio público con CSS/IntersectionObserver, reveal sutil, parallax ligero, microinteracciones y mejoras de composición en home, servicios, catálogo, contacto y login visual. Implementada; revisión visual real por viewport pendiente.
- Backlog relacionado: administración privada futura de catálogo, precios e imágenes. No modifica el catálogo público actual; `/catalogo` sigue usando data frontend hasta diseñar esa fase. Fuente: `docs/01-product/admin-catalog-management.md`.
- Fase sitio/sistema 2.0: validación real del flujo de `/login`, sesión, `returnUrl` y acceso a `/app/dashboard`, sin rediseñar pantallas ni implementar módulos nuevos. Ejecutada por código/build/tests/curl; login real con Admin local queda pendiente por falta de API/base/credenciales configuradas.
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

Completar validación manual de login real con API/base local y Admin configurado: `/login` -> login correcto -> `/app/dashboard` -> `GET /api/auth/me` -> logout -> `/app/dashboard` redirige de nuevo a `/login?returnUrl=%2Fapp%2Fdashboard`.

Después, revisar visualmente Fase 1.6 del sitio público en 360px, 375px, 390px, 414px, 768px, 1024px y desktop, confirmar vigencia de precios 2026, aprobar condiciones comerciales antes de publicar, confirmar WhatsApp/dirección/horarios y completar imágenes `.webp` faltantes.

## Regla De Actualización

Cada cierre de fase debe actualizar este roadmap, `docs/PROJECT_STATUS.md`, `docs/IMPLEMENTATION_LOG.md` y los documentos canónicos afectados.
