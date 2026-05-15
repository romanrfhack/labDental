# Checklist Responsive Mobile-First

Fuente canónica de QA responsive/mobile-first para el sitio público institucional.

## Estado Fase 1.1

- Primera versión pública implementada en `/`, `/servicios` y `/contacto`.
- `npm run build` ejecutado correctamente desde `src/LaboratorioTlahuac.Web` en Fase 1 y Fase 1.1.
- `git diff --check` ejecutado correctamente.
- Hallazgo manual 2026-05-13: `/app/dashboard` sin sesión no redirigía a `/login` y podía quedar en blanco.
- Corrección aplicada: los guards frontend redirigen a `/login?returnUrl=...` también cuando falla la verificación de sesión.
- Servidor local levantado en `http://127.0.0.1:4200/` para revisión.
- No existe script `lint` en `src/LaboratorioTlahuac.Web/package.json`.
- Revisión técnica por código/SCSS ejecutada para los breakpoints obligatorios.
- Revisión visual por viewport pendiente antes de presentación al cliente.
- No se usaron imágenes externas ni fotos de personas reales en esta fase.
- No se encontró Chromium, Chrome, Firefox, Playwright, Puppeteer ni `wkhtmltoimage` disponibles sin instalar dependencias.

## Estado Fase 1.2

- Copy público ajustado para revisión con datos no confirmados.
- CTA principal de WhatsApp retirado hasta tener número real confirmado.
- `/contacto` muestra WhatsApp, dirección y horarios como no confirmados.
- `/servicios` muestra catálogo en preparación, sin servicios definitivos.
- Pendiente validar visualmente los cambios en los viewports obligatorios.

## Estado Fase 1.3

- Catálogo público agregado en `/catalogo`.
- Secciones y productos se renderizan desde datos estructurados.
- Imágenes locales `.webp` se cargan desde `/assets/catalog/products/`.
- Las imágenes usan frame uniforme con `aspect-ratio: 4 / 3`, `object-fit: contain`, fondo claro y centrado.
- Productos sin imagen específica usan imagen de sección o placeholder visual.
- Pendiente revisión visual real en los viewports obligatorios.

## Estado Fase 1.3.1

- Cierre técnico del catálogo ejecutado por revisión de código/configuración.
- Catálogo validado por código: 12 secciones, 40 productos, 19 imágenes específicas, 16 imágenes representativas de sección y 5 placeholders.
- Placeholders restantes: Reparación de dentadura por fractura, Gancho volado, Descanso metálico c/u, Rebase y Aumentar dientes c/u.
- Precios en data como números y formateados en UI con `Intl.NumberFormat('es-MX')`.
- Nota comercial visible: `Precios de referencia 2026 sujetos a confirmación.`.
- Assets `:Zone.Identifier` retirados del working tree en `src/LaboratorioTlahuac.Web/src/assets/catalog/products/`.
- `angular.json` mantiene el glob `src/assets/**/*.webp`, por lo que no copia `:Zone.Identifier` desde esa carpeta.
- Revisión visual real sigue pendiente porque no hay navegador/headless disponible en el entorno actual.

## Verificado Por Código / Build

- [x] Build frontend correcto con `npm run build`.
- [x] `git diff --check` sin errores.
- [x] `/`, `/catalogo`, `/servicios`, `/contacto`, `/login`, `/app` y `/app/dashboard` responden con shell Angular en dev server local.
- [x] `/login` sigue como entrada pública al sistema.
- [x] `/app` y `/app/dashboard` siguen configuradas como zona privada por routing/guards.
- [x] No se introdujo `/dashboard` como ruta privada real.
- [x] Header mobile usa layout apilado bajo 420px.
- [x] Marca, navegación, botones y footer permiten wrapping de textos largos.
- [x] CTAs públicos mantienen mínimo táctil de 48px.
- [x] Footer usa columnas flexibles en tablet/desktop para evitar overflow por contenido largo.
- [x] No hay imágenes externas ni recursos visuales pesados en la primera vista.
- [x] Guards frontend devuelven redirección a `/login?returnUrl=...` cuando `ensureSession()` falla.
- [x] Fase 1.2 no agrega enlaces externos de WhatsApp sin número confirmado.
- [x] Fase 1.2 no publica dirección ni horarios no confirmados.
- [x] Fase 1.3 renderiza catálogo desde data tipada, no desde una plantilla hardcodeada.
- [x] Fase 1.3 mantiene `/login` como entrada pública y `/app` como zona privada.
- [x] Fase 1.3 usa frames uniformes para imágenes y placeholder cuando faltan assets.
- [x] Fase 1.3.1 confirma por código que no hay `min-width` rígido que fuerce overflow en `/catalogo`.
- [x] Fase 1.3.1 confirma grids de catálogo con `minmax(0, 1fr)`.
- [x] Fase 1.3.1 confirma tarjetas del catálogo con `min-width: 0`.
- [x] Fase 1.3.1 confirma imágenes del catálogo con frame uniforme y `object-fit: contain`.
- [x] Fase 1.3.1 confirma enlaces/botones principales con área táctil de 40px a 48px según contexto.
- [x] Fase 1.3.1 build final, `git diff --check` y búsquedas solicitadas ejecutadas.

## Hallazgos Manuales Recibidos

| Ruta | Hallazgo | Estado |
| --- | --- | --- |
| `/app/dashboard` | Sin sesión, al escribir la URL directa no redirigió a `/login` y quedó sin contenido visible. | Corregido por código; confirmación visual manual pendiente. |

## Verificado Visualmente

- [ ] Confirmar en navegador real que `/app/dashboard` sin sesión redirige a `/login?returnUrl=/app/dashboard`.
- [ ] Resto de revisión visual por breakpoint pendiente. El entorno actual no tiene navegador/headless disponible sin instalar dependencias.
- [ ] Revisar visualmente `/catalogo` antes de aprobación del cliente.

## Pendiente De Revisión Manual

- Abrir `http://127.0.0.1:4200/` en la computadora local.
- Revisar `/`, `/catalogo`, `/servicios`, `/contacto` y `/login` en navegador real.
- Para celular en la misma red local, levantar temporalmente Angular con `npm start -- --host 0.0.0.0 --port 4200`. No es despliegue productivo.

## Viewports Obligatorios

| Viewport | Estado Fase 1.1 | Hallazgo |
| --- | --- | --- |
| 360px | Revisado por código/SCSS; visual pendiente | Header usa layout apilado y nav en 3 columnas. |
| 375px | Revisado por código/SCSS; visual pendiente | CTAs usan ancho completo y mínimo táctil. |
| 390px | Revisado por código/SCSS; visual pendiente | Cards y listas apilan en una columna. |
| 414px | Revisado por código/SCSS; visual pendiente | Header conserva layout móvil hasta 420px. |
| 768px | Revisado por código/SCSS; visual pendiente | Footer ajustado para columnas flexibles. |
| 1024px | Revisado por código/SCSS; visual pendiente | Grids de 3 columnas con `minmax(0, 1fr)`. |
| Desktop amplio | Revisado por código/SCSS; visual pendiente | Padding horizontal queda limitado por cálculo responsive. |

Breakpoints pendientes de revisar visualmente antes de presentar al cliente: 360px, 375px, 390px, 414px, 768px, 1024px y desktop.

## Navegación

- [x] La navegación principal está diseñada con alto táctil mínimo de 44px.
- [x] El header y menú son responsive por CSS.
- [x] El acceso a `/login` es claro en header y CTAs.
- [x] Los enlaces se separan en grid móvil bajo 420px.
- [x] El estado foco/hover tiene feedback visual.
- [ ] Confirmar visualmente separación y foco en navegador real.

## Controles Táctiles

- [x] Botones y enlaces principales tienen área táctil suficiente por CSS.
- [x] No se agregaron formularios públicos en esta fase.
- [x] No se agregaron inputs/selects públicos en esta fase.
- [x] No se agregaron mensajes de error públicos en esta fase.
- [ ] Confirmar visualmente los CTAs en navegador real.

## Layout Y Texto

- [x] El texto usa tamaños legibles por CSS.
- [x] Revisión de CSS: no se detectaron anchos fijos riesgosos en el sitio público.
- [x] Los bloques se apilan en móvil y pasan a grids en 768px.
- [x] Se agregó wrapping en marca, links, botones y footer.
- [x] El contenido importante aparece antes de detalles secundarios.
- [ ] Confirmar visualmente que no existe scroll horizontal en cada viewport.

## Imágenes Y Rendimiento

- [x] No se cargan imágenes en Fase 1.1.
- [x] No se cargan recursos visuales pesados innecesarios en la primera vista móvil.
- [ ] Lighthouse o revisión equivalente queda pendiente para una fase posterior.

## Validación Antes De Presentar Al Cliente

- [ ] Revisar en navegador con emulación móvil.
- [ ] Revisar al menos un dispositivo físico si está disponible.
- [x] Confirmar por dev server que el sitio público responde sin sesión.
- [ ] Confirmar visualmente que `/app` redirige a `/login` sin sesión.
- [ ] Confirmar visualmente que `/app/dashboard` redirige a `/login?returnUrl=/app/dashboard` sin sesión.
- [ ] Confirmar visualmente que el login sigue funcionando después de cambios visuales.

## Alcance

Este checklist no reemplaza la QA funcional del MVP administrativo. La QA funcional privada sigue documentada en:

- `docs/08-qa/mvp-qa-checklist.md`
- `docs/08-qa/mvp-acceptance-checklist.md`
