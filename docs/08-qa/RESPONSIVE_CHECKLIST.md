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
- La revisión visual real de identidad queda cubierta por el cierre manual de Fase 1.6.

## Estado Fase 1.5

- Logo LDT incorporado desde `/assets/brand/logo-ldt.webp` en header público, home y login visual.
- Tokens institucionales aplicados en componentes públicos y login visual: navy, blue, sky, gray y white.
- Header público mantiene layout mobile-first; bajo 640px apila marca y navegación.
- Teléfonos incorporados como enlaces `tel:`: 55 3331 9445, 55 2161 2311 y 55 9802 9816.
- Correo incorporado como `mailto:contacto@laboratoriodentaltlahuac.com`.
- Dirección, horarios y WhatsApp siguen pendientes de confirmar; no se agregó mapa.
- `/catalogo` mantiene la nota `Precios de referencia 2026 sujetos a confirmación.`.
- `Anticipo 50%` y `Trabajos urgentes +40%` se muestran solo con texto prudente de confirmación pendiente.
- La revisión visual real de identidad, header, logo y navegación queda cubierta por el cierre manual de Fase 1.6.

## Estado Fase 1.6

- Pulido visual premium del sitio público aplicado en `/`, `/servicios`, `/catalogo`, `/contacto` y SCSS visual de `/login`.
- Enfoque elegido: CSS moderno + `IntersectionObserver`; no se instaló GSAP ni otra dependencia.
- `prefers-reduced-motion` queda implementado en CSS y en la directiva pública de animación.
- El contenido permanece visible si JS falla; la clase de estado animado solo se activa cuando la directiva corre en navegador.
- Las animaciones usan `opacity` y `transform`; no se animan `width`, `height`, `top` ni `left`.
- En catálogo, el reveal de productos se limita por sección con lote inicial, para evitar animar listas grandes completas de golpe.
- Header/footer, CTAs, cards, catálogo y contacto mantienen enfoque mobile-first por grids apilados, `minmax(0, 1fr)` y wrapping.
- No se encontró navegador/headless local disponible: `chromium`, `google-chrome` y `firefox` no existen en el entorno; Playwright no está instalado en `node_modules`.
- Revisión visual manual confirmada el 2026-05-27 para `/`, `/servicios`, `/catalogo`, `/contacto` y `/login`.
- Breakpoints aprobados manualmente: 360px, 375px, 390px, 414px, 768px, 1024px y desktop.
- No se detectó scroll horizontal; header móvil, logo, botones, catálogo, imágenes, precios, placeholders y contacto quedaron aprobados visualmente.
- Reduced motion queda validado por implementación/código; no se reportaron hallazgos manuales bloqueantes.
- Cierre visual Fase 1.6: validada visualmente por el responsable del proyecto.

## Estado Fase 2.0

- Validación de flujo login/sesión/redirección ejecutada por código, build, tests y `curl`.
- `/login` sigue público.
- `/app` y `/app/dashboard` siguen privados por routing/guards.
- `/dashboard` no se convirtió en ruta privada real.
- `returnUrl` externo o inválido sigue bloqueado por sanitización en login.
- Login real quedó pendiente en esa fase; posteriormente fue validado manualmente con Admin local.

## Estado Fase 2.1

- API local levantada en `http://localhost:5277`; `/health` respondió saludable.
- Angular levantado en `http://localhost:4200/`; `/login` respondió con shell Angular.
- `GET /api/auth/csrf` respondió `204`.
- `GET /api/auth/me` sin sesión respondió `401`.
- Login real en navegador quedó pendiente en esa fase porque SQL Server local no estuvo accesible y no había Admin local configurado; posteriormente fue validado manualmente.
- Redirección visual de `/app/dashboard` sin sesión y después de logout quedó pendiente en esa fase; la redirección posterior a logout o sesión cerrada fue validada posteriormente.
- Validación de usuario sin `reports.view` queda pendiente para QA amplio por falta de usuarios de prueba.

## Estado Fase 2.1c

- Preflight Docker dedicado ejecutado para `ldt-labdental-sql` y validación local actualizada el 2026-05-23.
- `ldt-labdental-sql` está activo y expone SQL Server en el puerto local `14336`.
- No se usó `codex-cobranza-sql` ni ningún contenedor de otro proyecto.
- `dotnet ef migrations list` y `dotnet ef database update` confirmaron que `LaboratorioTlahuac_Dev` está al día con las cuatro migraciones existentes.
- La API local ejecutó el seed Admin al iniciar y después se apagó `SecuritySeed:RunOnStartup` en user-secrets.
- `GET /health`, `GET /api/auth/csrf` y `GET /api/auth/me` sin sesión fueron validados por `curl`.
- Login real en navegador con Admin local fue validado manualmente.
- Redirección posterior al login a `/app/dashboard` fue validada manualmente.
- Dashboard no queda validado en Fase 2.1c: cargó una vez, pero al regresar queda en `Cargando dashboard...`. Este pendiente se cierra posteriormente en Fase 2.1d.
- Redirección post-logout de `/app/dashboard` a `/login?returnUrl=%2Fapp%2Fdashboard` fue validada manualmente.
- `GET /api/auth/me` autenticado y logout como acción independiente quedan sin confirmación explícita.
- No se modificaron frontend, rutas, guards ni lógica de auth.

## Estado Fase 2.1d

- Diagnóstico de `/app/dashboard` ejecutado por código y endpoints anónimos.
- `/app/dashboard` consulta `GET /api/dashboard/summary` y requiere `reports.view`.
- Causa probable: una consulta pendiente a `GET /api/dashboard/summary` no tenia timeout y podia dejar `Cargando dashboard...` indefinidamente.
- Corrección mínima aplicada: timeout de 15 segundos y error controlado en el dashboard.
- No se rediseñó dashboard y no se cambiaron rutas privadas, guards, `AuthService`, backend, endpoints, permisos ni dependencias.
- Validación manual 2026-05-27: login con Admin local, redirección a `/app/dashboard` y dashboard autenticado quedan validados por el responsable del proyecto.
- `/app/dashboard` ya no queda indefinidamente en `Cargando dashboard...`.
- Flujo autenticado validado manualmente; `GET /api/auth/me` autenticado no fue inspeccionado de forma independiente.
- `GET /api/dashboard/summary` autenticado queda validado indirectamente por la carga correcta del dashboard; el endpoint no fue inspeccionado de forma independiente.
- Redirección posterior a logout o sesión cerrada validada: `/app/dashboard` redirige a `/login?returnUrl=%2Fapp%2Fdashboard`; logout como acción independiente queda para QA amplio si se requiere evidencia separada.

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
- [x] Fase 1.5 confirma logo en rutas públicas y login visual sin cambiar lógica de auth.
- [x] Fase 1.5 confirma contacto visible en home, `/catalogo`, `/contacto` y footer.
- [x] Fase 1.5 mantiene `/login` como entrada pública y `/app` como zona privada por routing/guards.
- [x] Fase 1.5 no introduce `/dashboard` como ruta privada real.
- [x] Fase 2.0 confirma por código que los cambios visuales de login no alteraron `AuthService.login()`.
- [x] Fase 2.0 confirma por código que `/login?returnUrl=%2Fapp%2Fdashboard` acepta el destino interno seguro.
- [x] Fase 2.0 confirma por código que `https://example.com`, `//example.com` y `javascript:alert(1)` usan fallback seguro.
- [x] Fase 2.0 ejecuta `npm run build`, `dotnet build`, `dotnet test` y `git diff --check` correctamente.
- [x] Fase 2.1 ejecuta `npm run build`, `dotnet build` y `dotnet test` correctamente.
- [x] Fase 2.1 confirma API local saludable y Angular local sirviendo `/login`.
- [x] Fase 2.1 confirma `GET /api/auth/me` sin sesión con `401`.
- [x] Fase 2.1c confirma que el preflight Docker no usó contenedores de otros proyectos y se detuvo sin secretos al faltar `LDT_SQL_SA_PASSWORD`.
- [x] Fase 2.1c ejecuta `npm run build`, `dotnet build`, `dotnet test`, `git diff --check` y búsquedas de rutas/secretos correctamente.
- [x] Fase 2.1c 2026-05-23 confirma `ldt-labdental-sql` activo en `14336`, migraciones al día, seed de arranque ejecutado, `/health` 200, CSRF 204 y `/api/auth/me` sin sesión 401.
- [x] Fase 2.1c 2026-05-23 valida manualmente `/login`, login con Admin local, redirección a `/app/dashboard` y redirección post-logout a `/login?returnUrl=%2Fapp%2Fdashboard`.
- [x] Fase 2.1d cierra el pendiente de Fase 2.1c: `/app/dashboard` ya no queda indefinidamente en `Cargando dashboard...`.
- [ ] Fase 2.1c 2026-05-23 deja pendiente confirmar explícitamente `GET /api/auth/me` autenticado y logout como acción independiente.
- [x] Fase 2.1d identifica que la única llamada del dashboard es `GET /api/dashboard/summary` y agrega timeout para evitar carga indefinida.
- [x] Fase 2.1d mantiene `/login` público, `/app` y `/app/dashboard` privados, y no introduce `/dashboard` como ruta privada real.
- [x] Fase 2.1d queda validada manualmente por el responsable del proyecto para flujo dashboard autenticado.
- [x] Fase 1.6 implementa `prefers-reduced-motion` para reveal, parallax y microinteracciones relevantes.
- [x] Fase 1.6 mantiene el sitio usable si falla JS de animación.
- [x] Fase 1.6 mantiene `/catalogo` legible por código: grids responsive, frames uniformes, precios visibles y cards con `min-width: 0`.
- [x] Fase 1.6 ejecuta `npm run build`, `dotnet build` y `dotnet test` correctamente.
- [x] Fase 1.6 ejecuta `git diff --check` y búsquedas de rutas, reduced motion y GSAP.
- [x] Cierre documental Fase 1.6 2026-05-27 ejecuta nuevamente `npm run build`, `dotnet build`, `dotnet test` y `git diff --check` correctamente.
- [x] Cierre documental Fase 1.6 2026-05-27 confirma que `/login` sigue público, `/app` y `/app/dashboard` siguen privados y `/dashboard` no es ruta privada real.
- [x] Cierre documental Fase 1.6 2026-05-27 confirma que no se detectaron valores reales de secretos en documentos tocados.
- [x] Cierre documental Fase 1.6 y Fase 2.1d 2026-05-27 ejecuta `npm run build`, `dotnet build`, `dotnet test`, `git diff --check` y búsquedas obligatorias correctamente.
- [x] Fase 1.6 queda cerrada como validada visualmente por el responsable del proyecto.

## Hallazgos Manuales Recibidos

| Ruta | Hallazgo | Estado |
| --- | --- | --- |
| `/app/dashboard` | Sin sesión, al escribir la URL directa no redirigió a `/login` y quedó sin contenido visible. | Corregido; redirección posterior a logout o sesión cerrada validada manualmente. |
| `/app/dashboard` | Después de login con Admin local redirige correctamente, pero al regresar a la página podía quedar en `Cargando dashboard...`. | Cerrado en Fase 2.1d; dashboard autenticado validado manualmente. |
| `/app/dashboard` | Si `GET /api/dashboard/summary` queda pendiente, el componente no tenia timeout. | Corregido con timeout y error controlado; validado manualmente por carga correcta del dashboard. |
| Sitio público Fase 1.6 | Rutas públicas, `/login`, viewports obligatorios, mobile-first, catálogo, contacto y ausencia de scroll horizontal revisados por el responsable. | Validado visualmente. |

## Verificado Visualmente

- [x] Confirmar en navegador real que después de logout `/app/dashboard` redirige a `/login?returnUrl=%2Fapp%2Fdashboard`.
- [x] Confirmar en navegador real login correcto desde `/login` hacia `/app/dashboard` con API/base local y Admin configurado.
- [x] Confirmar que `/app/dashboard` deja de quedarse en `Cargando dashboard...` al regresar a la página.
- [ ] Confirmar explícitamente `GET /api/auth/me` autenticado desde navegador/devtools o `curl` con sesión en Fase 2.2 si se requiere evidencia independiente.
- [x] Registrar revisión visual manual Fase 1.6 recibida para rutas públicas, `/login` y viewports obligatorios.
- [x] Cerrar Fase 1.6 como validación visual completa.
- [x] Revisar visualmente `/catalogo` antes de aprobación del cliente.
- [x] Revisar visualmente logo, header y navegación en 360px.

## Pendiente QA Amplio / Cliente

- Ejecutar Fase 2.2 - QA manual del sistema privado con Admin.
- Inspeccionar de forma independiente `GET /api/auth/me`, `GET /api/dashboard/summary` y logout si se requiere evidencia de red.
- Confirmar dirección, horarios, WhatsApp real, aprobación final de precios 2026, `Anticipo 50%`, `Trabajos urgentes +40%` e imágenes faltantes de `Servicios prostodónticos`.
- Para celular en la misma red local, levantar temporalmente Angular con `npm start -- --host 0.0.0.0 --port 4200`. No es despliegue productivo.

## Viewports Obligatorios

| Viewport | Estado Fase 1.6 | Hallazgo |
| --- | --- | --- |
| 360px | Aprobado manualmente | Sin problemas visuales bloqueantes reportados. |
| 375px | Aprobado manualmente | Sin problemas visuales bloqueantes reportados. |
| 390px | Aprobado manualmente | Sin problemas visuales bloqueantes reportados. |
| 414px | Aprobado manualmente | Sin problemas visuales bloqueantes reportados. |
| 768px | Aprobado manualmente | Sin problemas visuales bloqueantes reportados. |
| 1024px | Aprobado manualmente | Sin problemas visuales bloqueantes reportados. |
| Desktop amplio | Aprobado manualmente | Sin problemas visuales bloqueantes reportados. |

Breakpoints revisados y aprobados manualmente por el responsable del proyecto.

Estado Fase 1.6 por código: se revisaron reglas responsive, `overflow-x: clip`, grids móviles, nav horizontal controlado del catálogo, CTAs táctiles y reduced motion por CSS/JS.

## Navegación

- [x] La navegación principal está diseñada con alto táctil mínimo de 44px.
- [x] El header y menú son responsive por CSS.
- [x] El acceso a `/login` es claro en header y CTAs.
- [x] Los enlaces se separan en grid móvil bajo 420px.
- [x] El estado foco/hover tiene feedback visual.
- [x] Confirmar visualmente separación y foco en navegador real.

## Controles Táctiles

- [x] Botones y enlaces principales tienen área táctil suficiente por CSS.
- [x] No se agregaron formularios públicos en esta fase.
- [x] No se agregaron inputs/selects públicos en esta fase.
- [x] No se agregaron mensajes de error públicos en esta fase.
- [x] Confirmar visualmente los CTAs en navegador real.

## Layout Y Texto

- [x] El texto usa tamaños legibles por CSS.
- [x] Revisión de CSS: no se detectaron anchos fijos riesgosos en el sitio público.
- [x] Los bloques se apilan en móvil y pasan a grids en 768px.
- [x] Se agregó wrapping en marca, links, botones y footer.
- [x] El contenido importante aparece antes de detalles secundarios.
- [x] Confirmar visualmente que no existe scroll horizontal en cada viewport.

## Imágenes Y Rendimiento

- [x] No se cargan imágenes en Fase 1.1.
- [x] No se cargan recursos visuales pesados innecesarios en la primera vista móvil.
- [ ] Lighthouse o revisión equivalente queda pendiente para una fase posterior.

## Validación Antes De Presentar Al Cliente

- [x] Revisar en navegador con emulación móvil.
- [ ] Revisar al menos un dispositivo físico si está disponible.
- [x] Confirmar por dev server que el sitio público responde sin sesión.
- [x] Confirmar visualmente que `/app` sigue privado.
- [x] Confirmar visualmente que `/app/dashboard` redirige a `/login?returnUrl=%2Fapp%2Fdashboard` sin sesión o sesión cerrada.
- [x] Confirmar visualmente que el login sigue funcionando después de cambios visuales.

## Alcance

Este checklist no reemplaza la QA funcional del MVP administrativo. La QA funcional privada sigue documentada en:

- `docs/08-qa/mvp-qa-checklist.md`
- `docs/08-qa/mvp-acceptance-checklist.md`
