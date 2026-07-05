# Sitio Público Institucional

Fuente canónica funcional del sitio público de Laboratorio Dental Tláhuac.

## Propósito

Publicar una presencia digital clara, confiable y mobile-first para Laboratorio Dental Tláhuac en `laboratoriodentaltlahuac.com`, sin exponer el sistema administrativo privado.

## Estado Fase 1

Primera versión pública implementada dentro de `src/LaboratorioTlahuac.Web/src/app/public`.

Secciones listas en `/`:

- Hero principal con mensaje institucional.
- Capacidades del laboratorio con contenido provisional.
- Proceso de trabajo de alto nivel.
- Beneficios para doctores, consultorios y clínicas.
- Contacto con CTA neutral hacia `/contacto`.
- Entrada visible al sistema mediante `/login`.

Páginas públicas listas:

- `/`: landing institucional mobile-first.
- `/catalogo`: catálogo público con productos, precios de referencia e imágenes locales.
- `/servicios`: página de capacidades provisionales.
- `/contacto`: página de contacto con teléfonos y correo del cartel/catálogo; dirección, horarios y WhatsApp siguen pendientes.

Decisión de navegación: se mantienen páginas públicas existentes y la home también usa secciones internas para que la primera experiencia móvil sea recorrible sin cambiar de ruta.

## Estado Fase 1.1

QA responsive técnico parcialmente completado.

- Se revisó la estructura de header, CTAs, cards, footer y páginas públicas por código/SCSS.
- Se ajustó el footer para evitar columnas rígidas en tablet/desktop.
- Se reforzó el wrapping de marca, links, botones y textos largos.
- Se mantuvo `/login` como entrada pública al sistema y `/app` como zona privada.
- Hallazgo manual corregido: `/app/dashboard` sin sesión debe redirigir a `/login?returnUrl=/app/dashboard` aunque falle la verificación inicial de sesión.
- No se modificaron `AuthService`, backend, endpoints, cookies, XSRF, base de datos ni deploy.
- Revisión visual por breakpoint queda pendiente porque el entorno local no tiene navegador/headless disponible sin instalar dependencias.

## Estado Fase 1.2

Fase ejecutada parcialmente por falta de contenido real confirmado.

Contenido real incorporado:

- Ninguno. La solicitud dejó WhatsApp, dirección, horarios, logo, servicios exactos, texto principal aprobado y materiales visuales como pendientes.

Cambios aplicados para revisión:

- Se retiró el CTA principal `WhatsApp pendiente por confirmar`.
- Se reemplazó por CTA neutral hacia `/contacto`.
- Se ajustaron textos de home, servicios, contacto y footer para no presentar datos pendientes como definitivos.
- `/servicios` queda como página preparada para el catálogo final, sin publicar servicios exactos.
- `/contacto` queda como página preparada para datos finales, sin teléfono, dirección ni horarios inventados.

Secciones listas para revisión del cliente:

- Hero principal.
- Capacidades/catálogo en preparación.
- Proceso de trabajo de alto nivel.
- Beneficios para doctores, consultorios y clínicas.
- Contacto sin datos no confirmados.
- Entrada a `/login`.

## Estado Fase 1.3

Catálogo público implementado en ruta dedicada `/catalogo`.

Decisión de ruta:

- Se agregó `/catalogo` porque el catálogo contiene muchas secciones, productos, precios e imágenes.
- `/servicios` se mantiene como página introductoria y enlaza al catálogo.
- `/login` sigue como entrada pública al sistema.
- `/app` y `/app/dashboard` siguen como zona privada.

Data del catálogo:

- Archivo: `src/LaboratorioTlahuac.Web/src/app/public/data/catalog-data.ts`.
- Interfaces: `CatalogSection` y `CatalogProduct`.
- La UI renderiza secciones y productos dinámicamente desde esa estructura.
- Los precios se formatean en MXN desde valores numéricos.

Imágenes:

- Carpeta fuente: `src/LaboratorioTlahuac.Web/src/assets/catalog/products/`.
- Ruta pública esperada en Angular: `/assets/catalog/products/`.
- Formato usado en catálogo: `.webp`.
- Angular copia `src/assets/**/*.webp` como assets públicos.
- Las imágenes se muestran en un frame uniforme con `aspect-ratio: 4 / 3`, `object-fit: contain`, fondo claro y centrado.

Manejo de placeholders:

- Si existe imagen específica del producto, se usa esa imagen.
- Si falta imagen específica, se usa imagen representativa de la sección.
- Si no existe imagen de producto ni de sección, se muestra placeholder visual con iniciales.

Imágenes pendientes:

- Faltan imágenes específicas para varios productos que hoy usan imagen de sección.
- Faltan imágenes para `Servicios prostodónticos`; esos productos usan placeholder.
- Existe `protesis-removible-unidad-acrilica.jpg`, pero no se usa en esta fase porque el criterio definido fue `.webp`.

Contenido incorporado:

- Catálogo inicial completo con secciones, productos y precios provistos para Fase 1.3.
- No se agregaron WhatsApp, dirección, horarios ni logo porque siguen sin confirmarse.

## Estado Fase 1.3.1

Cierre técnico del catálogo y preparación para revisión visual del cliente.

Ruta y estructura:

- Ruta pública: `/catalogo`.
- `/servicios` permanece como página introductoria y enlaza a `/catalogo`.
- Archivo de datos: `src/LaboratorioTlahuac.Web/src/app/public/data/catalog-data.ts`.
- Carpeta de imágenes: `src/LaboratorioTlahuac.Web/src/assets/catalog/products/`.
- Ruta pública esperada de imágenes: `/assets/catalog/products/`.
- `angular.json` conserva el copiado de `src/assets/**/*.webp` hacia `assets`; no se requirió cambiar configuración.

Conteo validado por código:

- Secciones: 12.
- Productos: 40.
- Productos con imagen específica: 19.
- Productos con imagen representativa de sección: 16.
- Productos con placeholder visual: 5.

Placeholders restantes:

- Reparación de dentadura por fractura.
- Gancho volado.
- Descanso metálico c/u.
- Rebase.
- Aumentar dientes c/u.

Datos y precios:

- Los precios permanecen como números en `catalog-data.ts`, no como strings formateados.
- La UI formatea precios con `Intl.NumberFormat('es-MX')` y moneda `MXN`.
- El copy visible usa la nota `Precios de referencia 2026 sujetos a confirmación.`.
- El catálogo se cargó desde el cartel proporcionado; precios, vigencia 2026 y cualquier condición comercial requieren aprobación final del cliente antes de publicar formalmente.
- No se agregaron condiciones comerciales nuevas en esta fase.

Backlog futuro relacionado:

- La administración de catálogo, precios e imágenes queda documentada como fase futura separada en `docs/01-product/admin-catalog-management.md`.
- No pertenece a la fase actual del sitio público.
- Fase 3.5.1 ya implementó backend/API/seed de catálogo y Fase 3.5.2 ya implementó UI admin bajo `/app/admin/catalogo`, pero `/catalogo` no consume todavía la API.
- El catálogo público actual sigue funcionando desde `catalog-data.ts` hasta que se diseñe la fase privada.
- La edición futura deberá vivir bajo `/app`, requerir permisos administrativos y no exponerse en el sitio público.
- Fase 3.5.0 documenta el diseño técnico en `docs/01-product/catalog-admin-design.md`. Hasta que se implemente y valide Fase 3.5.3, `/catalogo` debe seguir funcionando con `catalog-data.ts` para no romper la experiencia pública.
- Fase 3.5.1 implementa `GET /api/catalog/public` y endpoints admin de catálogo, pero `/catalogo` no los consume todavía. La transición pública queda para Fase 3.5.3.
- Fase 3.5.2 permite administrar secciones/productos/precios e imágenes existentes desde `/app/admin/catalogo`; no modifica la fuente actual de `/catalogo`.
- QA DEV Fase 3.5.1 confirmó que `/catalogo` sigue respondiendo `200` y que la API pública de catálogo también responde `200` sin sesión. No se cambió la fuente de datos visible del catálogo público.

Assets:

- Se retiraron del working tree los archivos `:Zone.Identifier` detectados dentro de `src/LaboratorioTlahuac.Web/src/assets/catalog/products/`.
- No se borraron imágenes `.webp`.
- No se borró `protesis-removible-unidad-acrilica.jpg`; sigue documentado como asset local no usado porque el catálogo de esta fase referencia `.webp`.

Rutas confirmadas por revisión de código:

- `/catalogo`, `/servicios` y `/contacto` están bajo layout público.
- `/login` sigue como entrada pública al sistema.
- `/app` y `/app/dashboard` siguen bajo zona privada con guards.
- `/dashboard` no existe como ruta privada real.

## Estado Fase 1.5

Identidad visual y contacto real del cartel/catálogo incorporados.

Marca:

- Logo fuente: `src/LaboratorioTlahuac.Web/src/assets/brand/logo-ldt.webp`.
- Ruta pública: `/assets/brand/logo-ldt.webp`.
- El logo se usa proporcionalmente en header público, home y login visual.
- Nombre comercial: Laboratorio Dental Tláhuac.
- Eslogan: `Precisión • Estética • Confianza`.
- Línea descriptiva: `Prótesis, restauraciones y soluciones dentales`.
- Tokens documentados en `docs/02-domain/brand-guidelines.md`.

Datos de contacto incorporados:

- Teléfonos: 55 3331 9445, 55 2161 2311 y 55 9802 9816.
- Correo: `contacto@laboratoriodentaltlahuac.com`.
- Los teléfonos se muestran como teléfonos con enlaces `tel:`.
- El correo se muestra con enlace `mailto:`.

Datos pendientes:

- Dirección.
- Horarios.
- WhatsApp como canal real.
- Redes sociales.
- Mapa o ubicación pública.

Condiciones comerciales:

- Se mantiene `Precios de referencia 2026 sujetos a confirmación.`.
- `Anticipo 50%` y `Trabajos urgentes +40%` aparecen en el cartel/catálogo, pero requieren aprobación final del cliente.
- Si se muestran en UI, deben aparecer con texto prudente de confirmación pendiente, no como condiciones definitivas.

Alcance técnico:

- No se modificaron backend, `AuthService`, guards, cookies, XSRF, endpoints, base de datos, migraciones, deploy, dependencias ni rutas privadas.
- `/login` sigue como entrada pública.
- `/app` y `/app/dashboard` siguen como zona privada.
- `/dashboard` no se creó como ruta privada real.

## Estado Fase 1.6

Pulido visual premium del sitio público implementado y validado visualmente por el responsable del proyecto.

Enfoque técnico:

- Se resolvió sin GSAP y sin dependencias nuevas.
- Se agregó una directiva pública reusable basada en `IntersectionObserver`, `matchMedia('(prefers-reduced-motion: reduce)')` y CSS transitions.
- El parallax es ligero y limitado al logo del hero en home mediante `transform`, sin pinning, sin scrub agresivo y sin smooth scroll global.
- Los estilos pesados de home y catálogo viven en `src/LaboratorioTlahuac.Web/src/styles.scss` con selectores acotados por `.home-page` y `.catalog-page` para no afectar la app privada.
- Si JS de animación falla, el contenido queda visible porque el estado oculto solo se activa cuando la directiva agrega `public-animation-ready`.

Páginas afectadas:

- `/`: hero institucional con fondo más cinematográfico, logo con profundidad, entrada de copy/CTAs, reveal escalonado de beneficios, proceso y contacto.
- `/servicios`: composición más editorial, tarjetas con índice visual, banda de ruta recomendada y CTA claro a `/catalogo`.
- `/catalogo`: encabezado premium, resumen visual, contacto/condiciones más claras, cards de producto con frame uniforme, microinteracción de imagen y reveal por lote por sección.
- `/contacto`: cards separan datos confirmados y pendientes; teléfonos/correo quedan destacados sin inventar dirección, horarios ni WhatsApp.
- `/login`: solo pulido visual de SCSS para reforzar marca; no se modificó lógica, `AuthService`, guards ni `returnUrl`.

Accesibilidad y movimiento:

- `prefers-reduced-motion: reduce` desactiva reveal, parallax y transformaciones de hover relevantes.
- Las animaciones no son necesarias para entender el contenido.
- Focus visible y contraste se mantienen con tokens LDT.
- No se oculta contenido esencial sin JS.

Validación visual manual 2026-05-27:

- `/`, `/servicios`, `/catalogo`, `/contacto` y `/login` fueron revisados visualmente y aprobados.
- Breakpoints revisados y aprobados: 360px, 375px, 390px, 414px, 768px, 1024px y desktop.
- No se detectó scroll horizontal.
- El header no se rompe en móvil, el logo se ve proporcionado y los botones son cómodos en celular.
- El catálogo sigue legible, las imágenes se ven uniformes, los precios se leen correctamente y los placeholders se ven intencionales.
- `/contacto` separa datos confirmados y pendientes correctamente.
- El diseño se considera más atractivo visualmente y las animaciones se sienten sutiles y profesionales.
- El sitio sigue siendo mobile-first y no se detectaron problemas visuales bloqueantes.
- Reduced motion queda validado por implementación/código; no se reportaron hallazgos manuales bloqueantes.

Cierre:

- Fase 1.6 queda cerrada como validada visualmente.
- El enfoque CSS + `IntersectionObserver` queda aceptado para esta etapa.
- No se usó GSAP ni se instalaron dependencias nuevas.
- El sitio queda listo para revisión y retroalimentación del cliente.

Pendientes de cliente:

- Dirección.
- Horarios.
- WhatsApp como canal real.
- Aprobación final de precios 2026.
- Aprobación de `Anticipo 50%`.
- Aprobación de `Trabajos urgentes +40%`.
- Imágenes faltantes de `Servicios prostodónticos`.
- Lighthouse o revisión equivalente queda para una fase posterior.

## Audiencia

- Doctores y doctoras que buscan un laboratorio dental.
- Clínicas dentales que requieren trabajos de laboratorio.
- Clientes potenciales que necesitan ubicación, servicios y contacto.
- Personal interno que usará `/login` como entrada al sistema.

## Páginas Públicas Planeadas

Rutas existentes para Fase 1:

- `/`: página principal.
- `/catalogo`: catálogo público de productos y precios.
- `/servicios`: servicios del laboratorio.
- `/contacto`: datos de contacto, ubicación y forma de comunicación.
- `/login`: entrada pública al sistema privado.

Rutas futuras opcionales:

- `/trabajos`
- `/ubicacion`
- `/privacidad`

## Enfoque Mobile-First

- El cliente revisará primero desde celular.
- La navegación debe funcionar cómodamente con dedo.
- Los botones y enlaces deben tener área táctil suficiente.
- El texto debe ser legible sin zoom.
- No debe existir scroll horizontal.
- Las imágenes deben estar optimizadas.
- Validación obligatoria: `docs/08-qa/RESPONSIVE_CHECKLIST.md`.

## Entrada A Login

- `/login` debe mantenerse visible como acceso al sistema.
- `/login` no debe mezclarse visualmente con rutas privadas.
- El rediseño visual del sitio público no debe cambiar auth, cookies, guards, permisos ni CSRF/XSRF.
- Validación Fase 2.0: `/login` sigue público y los cambios visuales de Fase 1.5 no alteraron `AuthService.login()`, manejo de errores, sanitización de `returnUrl` ni navegación posterior al login.
- El flujo validado manualmente para Fase 2.1d es `/` o ruta pública -> `/login` -> login correcto con Admin local -> `/app/dashboard`; QA manual más amplio del sistema privado queda para Fase 2.2.

## Contenido Pendiente Del Cliente

- Servicios finales a publicar.
- Textos institucionales.
- WhatsApp como canal real.
- Ubicación.
- Horarios.
- Fotografías o materiales visuales adicionales aprobados.
- Aprobación final de precios y condiciones comerciales visibles en el cartel.

Estos datos siguen pendientes después de Fase 1.2; no se deben presentar placeholders como información definitiva.

## Contenido Provisional

No se inventaron dirección, horarios, WhatsApp, redes sociales ni mapa. Mientras el cliente confirma información, el sitio evita enlazar canales no confirmados y muestra avisos seguros:

- WhatsApp no confirmado.
- Dirección no confirmada.
- Horarios no confirmados.

Las capacidades publicadas en Fase 1 son descripciones generales provisionales, no catálogo final de servicios. Deben validarse con el cliente antes de presentarse como oferta cerrada.

## Relación Con Dominio

- Dominio principal: `laboratoriodentaltlahuac.com`.
- El sitio público debe cargar por HTTPS cuando haya producción.
- La app privada seguirá bajo `/app` en el mismo dominio salvo decisión futura.

## Fuera De Alcance De Fase 1 Del Sitio

- Cambiar auth.
- Cambiar rutas privadas.
- Cambiar endpoints.
- Cambiar deploy productivo.
- Crear una app o repo nuevo.
- Implementar módulos privados nuevos.
- Implementar administración de catálogo, precios o imágenes.
- Migrar `/catalogo` de `catalog-data.ts` a `GET /api/catalog/public` antes de Fase 3.5.3.
- Implementar upload de imágenes desde la UI admin de catálogo antes de Fase 3.5.4.

## Secciones Faltantes O Pendientes

- Contenido final aprobado por el cliente.
- Logo o identidad visual final.
- Fotografías o materiales visuales propios del laboratorio.
- Aviso de privacidad si se solicitarán datos personales desde el sitio público.
- Formulario real de contacto, solo cuando exista backend o herramienta confirmada para recibir mensajes.
- QA visual completa en los viewports del checklist responsive, cuando el reporte manual incluya resultados explícitos y observaciones concretas.
