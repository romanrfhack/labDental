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
- Fase 3.4.1: backend delivery MVP + permisos. Pendiente; debe implementar modelo, migración, permisos y endpoints mínimos.
- Fase 3.4.2: UI admin desde órdenes para asignar, registrar salida/estado y ver seguimiento. Pendiente.
- Fase 3.4.3: UI repartidor mobile-first bajo `/app/entregas`. Pendiente.
- Fase 3.4.4: QA DEV y ajustes del flujo de entregas. Pendiente.
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

Iniciar Fase 3.4.1 - backend delivery MVP + permisos, tomando como fuente `docs/01-product/delivery-mvp-design.md`.

Alcance sugerido inmediato: agregar permisos `deliveries.view`, `deliveries.assign`, `deliveries.update` y `deliveries.complete`; crear `WorkOrderDelivery`; definir `DeliveryStatus`; crear endpoints mínimos de entregas; cubrir autorización con pruebas. La prueba física de etiquetas y validación DEV final de usuarios/roles pueden seguir en paralelo si aún faltan evidencias humanas, pero no bloquean el backend de entregas.

Pendiente paralelo: cerrar validación de usuario QA limitado y `/app/access-denied` en DEV si aún no queda formalmente validada con cuenta limitada real sin `reports.view`.

## Regla De Actualización

Cada cierre de fase debe actualizar este roadmap, `docs/PROJECT_STATUS.md`, `docs/IMPLEMENTATION_LOG.md` y los documentos canónicos afectados.
