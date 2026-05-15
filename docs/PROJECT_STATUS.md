# Estado Del Proyecto

## Resumen

Laboratorio Dental Tláhuac tiene un MVP administrativo privado avanzado y una primera versión del sitio público institucional mobile-first implementada. Ambos frentes viven en el mismo repositorio y en la misma app Angular, pero se documentan por separado para evitar confundir fases.

Fase actual del frente público: Fase 1.3.1 del sitio público institucional, cierre técnico del catálogo público y preparación para revisión visual del cliente.

## Sistema Privado / MVP Administrativo

Estado: avanzado, con QA funcional y demo documentadas.

- Ruta privada base: `/app`.
- Dashboard real: `/app/dashboard`.
- Login público de entrada: `/login`.
- Backend .NET 10 y frontend Angular 21 implementados.
- Auth por cookie HttpOnly, CSRF/XSRF y permisos por claims.
- Módulos implementados: clientes, doctores, clínicas, doctores internos, órdenes de trabajo, estados, pagos, saldos calculados y dashboard básico.
- QA funcional documentada en `docs/08-qa/`.
- Demo administrativa documentada en `docs/08-qa/demo-script.md`.

La Fase 1 / Etapa 7 documentada en `docs/05-delivery/phase-1-mvp.md` corresponde a este sistema privado.

## Sitio Público Institucional

Estado: Fase 1 implementada como primera versión pública revisable; Fase 1.3.1 cerró la revisión técnica del catálogo público.

- Rutas públicas existentes: `/`, `/catalogo`, `/servicios`, `/contacto`.
- Entrada al sistema: `/login`.
- Ubicación técnica: `src/LaboratorioTlahuac.Web/src/app/public`.
- `/` muestra landing mobile-first con hero, capacidades, proceso, beneficios, contacto y entrada al sistema.
- `/catalogo` muestra productos, secciones, precios e imágenes locales cuando existen.
- `/servicios` y `/contacto` funcionan como páginas públicas de apoyo.
- No se muestra enlace de WhatsApp porque el número real sigue pendiente.
- Fase 1.2 pulió copy público y retiró CTAs que podían interpretarse como contacto confirmado.
- WhatsApp, dirección, horarios, logo, texto principal aprobado y materiales visuales siguen pendientes.
- El catálogo inicial con precios ya fue incorporado desde datos estructurados.
- Imágenes del catálogo: `src/LaboratorioTlahuac.Web/src/assets/catalog/products/`.
- Data del catálogo: `src/LaboratorioTlahuac.Web/src/app/public/data/catalog-data.ts`.
- Catálogo validado por código: 12 secciones, 40 productos, 19 productos con imagen específica, 16 productos con imagen representativa de sección y 5 productos con placeholder visual.
- Los 5 placeholders restantes pertenecen a `Servicios prostodónticos`: Reparación de dentadura por fractura, Gancho volado, Descanso metálico c/u, Rebase y Aumentar dientes c/u.
- Los precios se mantienen como números en la data y se formatean en MXN con `Intl.NumberFormat('es-MX')`.
- La UI muestra la nota comercial prudente `Precios de referencia 2026 sujetos a confirmación.` porque la información proviene del cartel proporcionado y requiere aprobación final del cliente antes de publicar.
- `angular.json` conserva el copiado de assets `.webp` desde `src/assets` hacia `assets`; no copia archivos `:Zone.Identifier` desde esa carpeta.
- Los archivos `:Zone.Identifier` del catálogo fueron retirados del working tree. Las imágenes `.webp` y el `.jpg` existente no se borraron.
- Fase 1.1 ajustó wrapping, espaciado, footer responsive y contenedores públicos para reducir riesgo de overflow.
- Hallazgo manual corregido: al abrir `/app/dashboard` sin sesión, el frontend debe redirigir a `/login?returnUrl=/app/dashboard` en vez de quedar en blanco si falla la verificación de sesión.
- Revisión de seguridad/routing completada: `returnUrl` se restringe a rutas internas seguras bajo `/app` y usa fallback `/app/dashboard` si recibe valores externos o inválidos.
- No se contó con navegador/headless local para capturas; la revisión visual real en dispositivo o navegador queda pendiente.
- Login/guards: sin cambios en Fase 1.3.
- No se modificaron backend, `AuthService`, cookies, XSRF, endpoints, base de datos, deploy, dependencias ni rutas privadas en Fase 1.3.
- Documento funcional canónico: `docs/01-product/public-website.md`.
- Checklist responsive canónico: `docs/08-qa/RESPONSIVE_CHECKLIST.md`.

La Fase 0/Fase 1 del sitio público corresponde a este frente nuevo. No contradice el avance del MVP administrativo.

## Dominio Y Deploy

Estado: pendiente de definición productiva.

- Dominio principal: `laboratoriodentaltlahuac.com`.
- Plataforma de deploy: pendiente.
- DNS: pendiente.
- HTTPS productivo: pendiente.
- Fuente canónica de deploy: `docs/05-delivery/DEPLOYMENT.md`.

## QA

- QA funcional del MVP administrativo: ejecutada y documentada.
- QA responsive del sitio público: revisión por código/build ejecutada; revisión visual por viewport pendiente.
- No existe runner frontend no interactivo; frontend se valida hoy con `npm run build` y revisión manual cuando aplique.
- Validación Fase 1: `npm run build` ejecutado correctamente en `src/LaboratorioTlahuac.Web`.
- Validación Fase 1.1: `npm run build`, `git diff --check`, rutas por `curl` y búsquedas de `/login`, `/app/dashboard` y `/dashboard` ejecutadas.
- Validación 2026-05-13: `npm run build` correcto después del ajuste de guards.
- Revisión de seguridad/routing 2026-05-13: `returnUrl` externo o inválido se normaliza a `/app/dashboard`; usuario autenticado sin permiso conserva flujo a `/app/access-denied`.
- Validación Fase 1.2: `npm run build`, `git diff --check` y búsquedas de rutas ejecutadas.
- Validación Fase 1.3: `npm run build`, `git diff --check`, búsqueda de rutas y verificación de assets del catálogo ejecutadas.
- Validación Fase 1.3.1: `npm run build`, `git diff --check`, búsquedas solicitadas y verificación por nombre de archivos `*:Zone.Identifier` ejecutadas correctamente.
- No existe script `lint` en `src/LaboratorioTlahuac.Web/package.json`.
- Zona horaria formal de negocio sigue pendiente para métricas de "hoy", vencidas y próximos 7 días.

## Comercial

- Paquete comercial de primera ronda documentado en `docs/09-commercial/`.
- Las fases comerciales no son el roadmap técnico interno.
- Próxima conversación comercial: demo con cliente, alcance, precio, prioridades y materiales del sitio.

## Próxima Tarea Recomendada

Revisión visual y comercial del cliente del catálogo público en `/catalogo`, confirmación de vigencia de precios 2026 y reemplazo de imágenes faltantes por archivos `.webp` específicos. Después, recibir WhatsApp, dirección, horarios, logo, texto principal aprobado y materiales visuales.
