# Estado Del Proyecto

## Resumen

Laboratorio Dental Tláhuac tiene un MVP administrativo privado avanzado y una primera versión del sitio público institucional mobile-first implementada. Ambos frentes viven en el mismo repositorio y en la misma app Angular, pero se documentan por separado para evitar confundir fases.

Fase actual del frente público: Fase 1.5 del sitio público institucional, integración de identidad visual LDT, tokens de marca y datos de contacto del cartel/catálogo.

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

Estado: Fase 1 implementada como primera versión pública revisable; Fase 1.5 incorporó logo, colores institucionales y contacto real del cartel/catálogo.

- Rutas públicas existentes: `/`, `/catalogo`, `/servicios`, `/contacto`.
- Entrada al sistema: `/login`.
- Ubicación técnica: `src/LaboratorioTlahuac.Web/src/app/public`.
- `/` muestra landing mobile-first con hero, capacidades, proceso, beneficios, contacto y entrada al sistema.
- `/catalogo` muestra productos, secciones, precios e imágenes locales cuando existen.
- `/servicios` y `/contacto` funcionan como páginas públicas de apoyo.
- Logo público: `src/LaboratorioTlahuac.Web/src/assets/brand/logo-ldt.webp`, servido como `/assets/brand/logo-ldt.webp`.
- Identidad incorporada: Laboratorio Dental Tláhuac, `Precisión • Estética • Confianza` y `Prótesis, restauraciones y soluciones dentales`.
- Contacto incorporado desde cartel/catálogo: 55 3331 9445, 55 2161 2311, 55 9802 9816 y `contacto@laboratoriodentaltlahuac.com`.
- Los teléfonos se muestran como enlaces `tel:` y el correo como `mailto:`.
- No se muestra enlace de WhatsApp porque sigue pendiente confirmar si esos teléfonos operan como WhatsApp.
- Fase 1.2 pulió copy público y retiró CTAs que podían interpretarse como contacto confirmado.
- Dirección, horarios, WhatsApp como canal real, redes sociales, mapa y materiales visuales adicionales siguen pendientes.
- El catálogo inicial con precios ya fue incorporado desde datos estructurados.
- Imágenes del catálogo: `src/LaboratorioTlahuac.Web/src/assets/catalog/products/`.
- Data del catálogo: `src/LaboratorioTlahuac.Web/src/app/public/data/catalog-data.ts`.
- Backlog futuro documentado: administración de catálogo, precios e imágenes desde la app privada bajo `/app`. No pertenece a la fase actual, no está implementado y no cambia el funcionamiento actual de `/catalogo`.
- Catálogo validado por código: 12 secciones, 40 productos, 19 productos con imagen específica, 16 productos con imagen representativa de sección y 5 productos con placeholder visual.
- Los 5 placeholders restantes pertenecen a `Servicios prostodónticos`: Reparación de dentadura por fractura, Gancho volado, Descanso metálico c/u, Rebase y Aumentar dientes c/u.
- Los precios se mantienen como números en la data y se formatean en MXN con `Intl.NumberFormat('es-MX')`.
- La UI muestra la nota comercial prudente `Precios de referencia 2026 sujetos a confirmación.` porque la información proviene del cartel proporcionado y requiere aprobación final del cliente antes de publicar.
- Condiciones visibles en cartel/catálogo: `Anticipo 50%` y `Trabajos urgentes +40%`; se documentan y se muestran con texto prudente de confirmación pendiente, no como condiciones definitivas.
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
- Validación Fase 1.5: `npm run build`, `git diff --check` y búsquedas solicitadas de logo, contacto, WhatsApp y rutas ejecutadas correctamente.
- No existe script `lint` en `src/LaboratorioTlahuac.Web/package.json`.
- Zona horaria formal de negocio sigue pendiente para métricas de "hoy", vencidas y próximos 7 días.

## Comercial

- Paquete comercial de primera ronda documentado en `docs/09-commercial/`.
- Las fases comerciales no son el roadmap técnico interno.
- Próxima conversación comercial: demo con cliente, alcance, precio, prioridades y materiales del sitio.

## Próxima Tarea Recomendada

Revisión visual y comercial del cliente del sitio público con identidad LDT aplicada, confirmación de vigencia de precios 2026, condiciones comerciales del cartel, WhatsApp como canal real, dirección, horarios y reemplazo de imágenes faltantes por archivos `.webp` específicos.

Backlog futuro separado: evaluar la fase de administración de catálogo, precios e imágenes en la app privada solo después de definir permisos administrativos, modelo de datos, endpoints, almacenamiento de imágenes, reglas de publicación y aprobación del cliente para precios públicos.
