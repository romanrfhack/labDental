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

## Sitio Público Institucional

Fuente funcional: `docs/01-product/public-website.md`.

- Fase sitio 0: preparación documental y consolidación. Completada con Fase 0.2.
- Fase sitio 1: primera versión mobile-first para `/`, `/servicios` y `/contacto`. Implementada.
- Fase sitio 1.1: revisión responsive técnica ejecutada; revisión visual por viewport pendiente.
- Fase sitio 1.2: contenido aprobado y reemplazo de placeholders. Parcial; faltan datos reales confirmados.
- Fase sitio 1.3: catálogo público con precios e imágenes locales en `/catalogo`. Implementada.
- Fase sitio 1.3.1: cierre técnico del catálogo, limpieza de assets `:Zone.Identifier`, documentación de placeholders y preparación para revisión visual del cliente. Implementada por código/documentación; revisión visual real pendiente.
- Fase sitio 2: revisión visual de `/login` como entrada al sistema, sin cambiar auth. Pendiente.
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

Revisar `/catalogo` con el cliente, confirmar vigencia de precios 2026, aprobar condiciones comerciales antes de publicar y completar imágenes `.webp` faltantes. En paralelo, completar revisión visual manual en 360px, 375px, 390px, 414px, 768px, 1024px y desktop.

## Regla De Actualización

Cada cierre de fase debe actualizar este roadmap, `docs/PROJECT_STATUS.md`, `docs/IMPLEMENTATION_LOG.md` y los documentos canónicos afectados.
