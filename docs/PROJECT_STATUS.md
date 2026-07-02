# Estado Del Proyecto

## Resumen

Laboratorio Dental Tláhuac tiene un MVP administrativo privado avanzado y una primera versión del sitio público institucional mobile-first implementada. Ambos frentes viven en el mismo repositorio y en la misma app Angular, pero se documentan por separado para evitar confundir fases.

Fase actual del frente público/sistema: Fase 1.6 del sitio público cerrada como validada visualmente por el responsable del proyecto, Fase 2.5 del sistema privado cerrada como pase visual humano privado completado, Fase 2.6 implementada para usuario QA limitado Development-only, Fase 3.0 cerrada como despliegue DEV validado/baseline UAT inicial y Fase 3.1 documentada como análisis operativo para órdenes, etiquetas, reparto, usuarios/roles y catálogo.

## Estado Por Frente

- Sitio público: Fase 1.6 cerrada; `/`, `/servicios`, `/catalogo`, `/contacto` y `/login` fueron aprobados visualmente por el responsable del proyecto. Fase 2.5 confirmó que no hubo regresión visible del sitio público.
- Catálogo: legible y aprobado visualmente; mantiene precios de referencia 2026, frames uniformes de imágenes, placeholders intencionales y condiciones comerciales con texto prudente.
- Deploy DEV: `https://dev.laboratoriodentaltlahuac.com` está publicado desde rama `dev` y queda validado como baseline UAT inicial en Fase 3.0.
- Login/auth: `/login` sigue público; login con Admin local validado manualmente y login QA validado manualmente en DEV; `AuthService`, guards, cookies, XSRF y `returnUrl` no fueron modificados en Fase 2.3, Fase 2.4, Fase 2.5, Fase 2.6 ni Fase 3.0.
- Sistema privado: Fase 2.6 implementada, Fase 3.0 validada en DEV y Fase 3.1 documentada; `/app` y `/app/dashboard` siguen privados, `/dashboard` no es ruta privada real, `/app/ordenes` ya existe como módulo real de órdenes y el siguiente incremento recomendado es extender órdenes con etiquetas, no duplicar paneles.
- Pendientes del cliente: dirección, horarios, WhatsApp real, aprobación final de precios 2026, aprobación de `Anticipo 50%`, aprobación de `Trabajos urgentes +40%` e imágenes faltantes de `Servicios prostodónticos`.

## Sistema Privado / MVP Administrativo

Estado: avanzado, con QA funcional y demo documentadas; Fase 2.3 queda cerrada por corrección de hallazgos QA del sistema privado.

- Ruta privada base: `/app`.
- Dashboard real: `/app/dashboard`.
- Login público de entrada: `/login`.
- Backend .NET 10 y frontend Angular 21 implementados.
- Auth por cookie HttpOnly, CSRF/XSRF y permisos por claims.
- Módulos implementados: clientes, doctores, clínicas, doctores internos, órdenes de trabajo, estados, pagos, saldos calculados y dashboard básico.
- QA funcional documentada en `docs/08-qa/`.
- Demo administrativa documentada en `docs/08-qa/demo-script.md`.
- Validación Fase 2.0 por código: `/login` sigue público; `/app` está protegido por `authGuard`; `/app/dashboard` está protegido por `permissionGuard` y requiere `reports.view`; `/dashboard` no existe como ruta privada real.
- Validación Fase 2.0 de login visual/lógica: los cambios visuales de Fase 1.5 en `login-page.component.ts` no alteraron `AuthService.login()`, manejo de errores, sanitización de `returnUrl`, navegación posterior al login ni solicitud de CSRF desde `AuthService`.
- Validación Fase 2.0 de `returnUrl`: se aceptan rutas internas seguras bajo `/app`, como `/app/dashboard`; valores externos o inválidos como `https://example.com`, `//example.com` y `javascript:alert(1)` usan fallback seguro `/app/dashboard`.
- Validación Fase 2.1: la connection string de desarrollo apunta a `Server=localhost;Database=LaboratorioTlahuac_Dev`, por lo que es local, pero SQL Server no estuvo accesible en este entorno; `dotnet ef database update` falló por conexión y no aplicó migraciones.
- Admin local Fase 2.1 quedó pendiente: no existen variables `LT_ADMIN_EMAIL`, `LT_ADMIN_PASSWORD`, `LT_ADMIN_FULL_NAME`, `SecuritySeed__RunOnStartup=true` en el proceso y no existe archivo de user-secrets para `laboratorio-tlahuac-api-dev`; no se inventaron credenciales ni se guardaron secretos.
- Validación real de login Fase 2.1 quedó pendiente por falta de base local accesible y credenciales Admin locales. Se validó API local con `/health`, Angular local en `http://localhost:4200/login`, `GET /api/auth/csrf` con `204` y `GET /api/auth/me` sin sesión con `401`.
- Permisos Fase 2.1 confirmados por código: el seed Admin asigna todos los permisos de `Permissions.All`, incluyendo `reports.view`, y `/app/dashboard` requiere `reports.view`.
- Validación Fase 2.1c: Docker está disponible y se confirmó que no se usará `codex-cobranza-sql` ni otros contenedores de otros proyectos.
- Contenedor dedicado esperado: `ldt-labdental-sql`; base local esperada: `LaboratorioTlahuac_Dev`; volumen esperado: `ldt-labdental-sql-data`.
- `ldt-labdental-sql` no existe todavía en este entorno y no se creó porque `LDT_SQL_SA_PASSWORD` no está definida en el proceso. Por seguridad no se inventó password ni se imprimieron secretos.
- Puertos locales preferidos revisados: `14336`, `14337` y `14338` están libres en el preflight local; el puerto objetivo sigue siendo `14336` cuando la variable esté definida.
- Fase 2.1c no configuró `ConnectionStrings:DefaultConnection` en user-secrets, no aplicó migraciones, no ejecutó seed Admin y no validó login real porque el bloqueo ocurre antes de crear SQL Server.
- Variables Admin Fase 2.1c: `LT_ADMIN_EMAIL` y `LT_ADMIN_PASSWORD` no están definidas; `LT_ADMIN_FULL_NAME` existe en el proceso pero no se usó porque seed/login quedaron bloqueados.
- Actualización Fase 2.1c 2026-05-23: `ldt-labdental-sql` ya existe, está activo y expone `1433/tcp` en el puerto local `14336`; no se usó `codex-cobranza-sql`.
- `dotnet ef migrations list` confirmó las migraciones `20260508044157_InitialSecurityModel`, `20260509004819_AddCustomersAndInternalDoctors`, `20260509022531_AddWorkOrders` y `20260509053231_AddPayments`.
- `dotnet ef database update` confirmó que `LaboratorioTlahuac_Dev` ya estaba al día; no hubo migraciones nuevas por aplicar.
- La API local levantó en `http://localhost:5277`, ejecutó la ruta de seed al inicio con configuración Admin disponible en user-secrets y luego se apagó `SecuritySeed:RunOnStartup` en user-secrets.
- Validación HTTP 2026-05-23: `/health` respondió `200`, `GET /api/auth/csrf` respondió `204` y `GET /api/auth/me` sin sesión respondió `401`.
- Login real, `/api/auth/me` autenticado, logout y `/api/auth/me` después de logout siguen pendientes porque `LT_ADMIN_EMAIL` y `LT_ADMIN_PASSWORD` no están disponibles en el proceso de Codex; no se extrajeron ni imprimieron secretos.
- Validación manual 2026-05-23: `/login` carga correctamente, el login con Admin local creado por seed funciona, la navegación redirige a `/app/dashboard` y la ruta privada post-logout redirige a `/login?returnUrl=%2Fapp%2Fdashboard`.
- Hallazgo manual 2026-05-23: el dashboard cargó una vez, pero al regresar a la página queda en `Cargando dashboard...`; este pendiente queda cerrado posteriormente con la validación manual de Fase 2.1d.
- `/api/auth/me` autenticado no queda confirmado porque el resultado manual no fue marcado como `sí`; logout independiente tampoco fue marcado, aunque la redirección posterior a logout sí fue reportada como correcta.
- Confirmación de rutas 2026-05-23: `/login` sigue siendo público; `/app` y `/app/dashboard` siguen siendo privados; `/dashboard` no es ruta privada real.
- Diagnóstico Fase 2.1d: `/app/dashboard` solo consulta `GET /api/dashboard/summary`; los errores HTTP sí apagaban `isLoading` con `finalize`, pero una petición que queda pendiente no tenía timeout y podía dejar visible `Cargando dashboard...` indefinidamente.
- Corrección Fase 2.1d: `dashboard-page.component.ts` agrega timeout de 15 segundos a la consulta de resumen y muestra error controlado si la API no responde a tiempo.
- Fase 2.1d no modificó `AuthService`, guards, rutas, cookies, XSRF, backend, endpoints, permisos, migraciones, deploy ni dependencias.
- Validación Fase 2.1d sin sesión: `/health` respondió `200`, `GET /api/auth/csrf` respondió `204`, `GET /api/auth/me` respondió `401` y `GET /api/dashboard/summary` respondió `401`.
- Validación manual Fase 2.1d 2026-05-27: `/login` carga correctamente, login con Admin local validado, redirección posterior a `/app/dashboard` validada y `/app/dashboard` ya no queda indefinidamente en `Cargando dashboard...`.
- Flujo autenticado validado manualmente; `GET /api/auth/me` autenticado no fue inspeccionado de forma independiente.
- `GET /api/dashboard/summary` autenticado queda validado indirectamente por la carga correcta del dashboard; el endpoint no fue inspeccionado de forma independiente.
- Redirección posterior a logout o sesión cerrada validada: `/app/dashboard` redirige a `/login?returnUrl=%2Fapp%2Fdashboard`; logout como acción independiente no queda documentado como inspeccionado por separado.
- Cierre Fase 2.1d: validado manualmente por el responsable del proyecto sin modificar `AuthService`, guards, rutas, cookies, XSRF, backend, endpoints, base de datos, migraciones, deploy ni dependencias.
- Validación Fase 2.2 2026-05-27: SQL dedicado `ldt-labdental-sql` activo en `14336`, base local `LaboratorioTlahuac_Dev`, API en `http://localhost:5277` y Angular en `http://localhost:4200`; no se usó `codex-cobranza-sql`.
- Fase 2.2 validó por HTTP autenticado con Admin: `GET /api/auth/csrf` `204`, `POST /api/auth/login` `200`, `GET /api/auth/me` `200` con 19 permisos, `GET /api/dashboard/summary` `200`, listados de clientes/órdenes/pagos `200`, `POST /api/auth/logout` `200` y `/api/auth/me` posterior `401`.
- Fase 2.2 creó datos locales de prueba claramente marcados: cliente `Cliente QA LDT 20260527-210940 Editado`, orden `OT-20260528-201A16` y pago `Pago QA LDT 20260527-210940`; no se limpiaron.
- Fase 2.2 registró dos hallazgos principales: definir zona horaria de negocio para métricas de dashboard como "Para hoy" y agregar estado activo visual en navegación privada si se prioriza UX.
- Limitación Fase 2.2: no hay navegador/headless local instalado sin agregar dependencias, por lo que redirecciones visuales, consola y Network se documentan por código/API y quedan pendientes para pase manual en navegador real si se requiere.
- Corrección Fase 2.3: el dashboard define `Dashboard:BusinessTimeZone` con default `America/Mexico_City` y calcula el "hoy" operativo con `clock.UtcNow` convertido a esa zona horaria.
- Métricas Fase 2.3 ajustadas: `dueToday`, `overdue` y `upcomingDue` usan la fecha operativa del laboratorio; `DeliveryDate` conserva su significado como fecha de entrega capturada.
- Compatibilidad Fase 2.3: el ID canónico documentado es IANA `America/Mexico_City`; el resolver acepta el equivalente Windows `Central Standard Time (Mexico)` para entornos Windows si aplica.
- Prueba Fase 2.3 agregada: `OperationalSummaryUsesBusinessTimeZoneDateWhenUtcDateDiffers` cubre el caso donde UTC y Mexico City caen en fechas distintas y valida `dueToday`, `overdue` y `upcomingDue`.
- Navegación privada Fase 2.3: `PrivateLayoutComponent` usa `routerLinkActive`, `ariaCurrentWhenActive` y `routerLinkActiveOptions` exacto para `/app/dashboard`; se agregaron estilos de activo, hover y focus visible sin cambiar permisos, rutas ni logout.
- Fase 2.3 no modificó sitio público, `AuthService`, guards, cookies, XSRF, endpoints públicos, rutas privadas, migraciones, deploy ni dependencias.
- Validación Fase 2.4 2026-05-27: SQL dedicado `ldt-labdental-sql` activo en `14336`, base `LaboratorioTlahuac_Dev`, API en `http://localhost:5277` y Angular en `http://localhost:4200`; `codex-cobranza-sql` no apareció activo ni se usó.
- Fase 2.4 validó por HTTP/API con Admin: CSRF `204`, login `200`, `/api/auth/me` `200` con 19 permisos, dashboard/listados `200`, logout `200` y endpoints privados posteriores `401`.
- Fase 2.4 creó datos locales QA: cliente `a5c48811-e171-450b-963e-f929a0d71084` con prefijo `Cliente QA LDT F2.4` y orden `OT-20260528-82F6A6` (`53a35d65-a3ff-4f7d-ab7c-b0b2d658df44`) con `DeliveryDate=2026-05-27`; no se limpiaron.
- Dashboard Fase 2.4: con la orden QA en fecha operativa local `2026-05-27`, `dueToday` subió de 1 a 2 y `upcomingDue` de 1 a 2; `overdue` se mantuvo en 0 y `generatedAtUtc` se confirmó UTC con offset `+00:00`.
- Navegación privada Fase 2.4: no hay navegador/headless disponible sin instalar dependencias, por lo que el estado activo se validó por código/build, shell Angular `200` y búsquedas; queda pendiente pase visual humano.
- Usuario limitado Fase 2.4: no se creó porque no existe mecanismo seguro local fuera de fixtures de pruebas y no se autorizó SQL directo; por código, `permissionGuard` conserva `/app/access-denied` para sesión sin permiso.
- Fase 2.4 no modificó código, `AuthService`, guards, cookies, XSRF, endpoints, rutas privadas, backend, migraciones, deploy ni dependencias.
- Fase 2.5 2026-05-28: pase visual/manual privado confirmado en navegador real por el responsable del proyecto.
- Resultados visuales Fase 2.5: `/login`, login Admin, `/app/dashboard`, `/app/clientes`, `/app/ordenes`, `/app/pagos`, navegación activa en rutas principales, placeholders de inventario/proveedores/usuarios/roles, logout y redirección de `/app/dashboard` sin sesión a `/login?returnUrl=%2Fapp%2Fdashboard` quedaron OK.
- Fase 2.5 confirma que el sitio público no tuvo regresión visible y no reportó bloqueantes visuales.
- Fase 2.5 documenta que `/dashboard` raíz sigue sin ser ruta privada real por revisión técnica de `app.routes.ts`.
- Fase 2.5 define como mecanismo recomendado para usuario QA limitado un seed solo Development, desactivado por default, controlado por user-secrets o variables de entorno, sin imprimir password, sin SQL manual, sin modificar Admin y sin activarse en producción.
- Plan creado: `docs/08-qa/limited-user-qa-plan.md`.
- Fase 2.5 no modificó código, backend, `AuthService`, guards, cookies, XSRF, endpoints, rutas privadas, base de datos, migraciones, deploy ni dependencias.
- Fase 2.6 implementa el seed QA limitado solo `Development` bajo `SecuritySeed:LimitedQaUser`.
- El seed QA limitado queda desactivado por default y requiere `SecuritySeed:LimitedQaUser:RunOnStartup=true` mas email, password y nombre desde user-secrets o variables de entorno.
- Variables soportadas para datos sensibles QA: `LT_QA_LIMITED_EMAIL`, `LT_QA_LIMITED_PASSWORD` y `LT_QA_LIMITED_FULL_NAME`.
- Permisos QA limitados: `SecuritySeed:LimitedQaUser:Permissions`, sincronizados contra claves existentes en `Permissions.All`; para validar access-denied se recomienda `customers.view` sin `reports.view`.
- El rol local `Limited QA` se crea o sincroniza solo en Development; si el email configurado pertenece a un Admin, el seed QA se omite para no alterar Admin.
- Fase 2.6 no crea migraciones, no agrega endpoints, no usa SQL manual, no guarda secretos en archivos versionados, no imprime contrasenas y no modifica rutas privadas, `AuthService`, guards, cookies, XSRF ni deploy.
- Pruebas Fase 2.6 agregadas: casos directos del seeder para no crear fuera de Development, no crear si esta desactivado, no crear si falta configuracion requerida, crear con configuracion completa, no otorgar `reports.view` cuando no se configura, otorgar `customers.view` explicito y no alterar Admin.
- Prueba API Fase 2.6 agregada: login con usuario QA limitado por seed, `/api/auth/me` con permisos limitados, `/api/customers` `200` con `customers.view`, `/api/dashboard/summary` `403` sin `reports.view` y sin sesion `401`.
- Validacion local real Fase 2.6: pendiente porque `LT_QA_LIMITED_EMAIL`, `LT_QA_LIMITED_PASSWORD` y `LT_QA_LIMITED_FULL_NAME` no estan disponibles en el proceso de Codex; no se inventaron credenciales.
- Validacion navegador Fase 2.6 de `/app/access-denied`: pendiente porque no hay navegador/headless local disponible sin instalar dependencias; quedan pasos manuales exactos en `docs/08-qa/limited-user-qa-plan.md`.
- Validación DEV Fase 3.0 2026-07-02: `https://dev.laboratoriodentaltlahuac.com` queda registrado como baseline UAT inicial desde rama `dev`; rutas públicas, `/login`, login QA, `/app/dashboard` autenticado y redirección sin sesión a `/login` fueron confirmados manualmente por el responsable del proyecto.
- Validación `curl` DEV Fase 3.0: `/`, `/servicios`, `/catalogo`, `/contacto`, `/login` y `/app/dashboard` respondieron `200` sin credenciales; para `/app/dashboard`, ese `200` solo confirma el shell SPA y no reemplaza la validación navegador de guards.
- Usuario QA limitado/access-denied en DEV: sigue pendiente de cierre formal si aún no se prueba con cuenta limitada real sin `reports.view`; el login QA validado en DEV sí tiene acceso al dashboard y no prueba el caso `403`/`/app/access-denied`.
- Fase 3.1 2026-07-02: análisis operativo documentado para órdenes, etiquetas, reparto, usuarios/roles y catálogo. Se confirma que `/app/ordenes` ya existe y debe extenderse; no se creó panel duplicado, no se modificó código, no se tocaron migraciones, base de datos, auth, guards, endpoints, cookies, XSRF, deploy ni dependencias.
- Documentos Fase 3.1 creados: `docs/01-product/operations-orders-delivery.md`, `docs/01-product/label-printing.md` y `docs/01-product/driver-mobile-workflow.md`.
- Fase 3.1 confirma que la orden actual tiene folio, cliente, paciente, trabajo, color, fechas, estado, total, observaciones, historial y pagos; faltan datos de reparto como repartidor asignado, salida, receptor, fecha/hora real de entrega y evidencia.
- Fase 3.1 recomienda Fase 3.2 como siguiente incremento: impresión de etiqueta interna y etiqueta de entrega desde órdenes existentes, con rutas privadas bajo `/app` y CSS de impresión en milímetros.

La Fase 1 / Etapa 7 documentada en `docs/05-delivery/phase-1-mvp.md` corresponde a este sistema privado.

## Sitio Público Institucional

Estado: Fase 1 implementada como primera versión pública revisable; Fase 1.6 incorporó pulido visual premium con animaciones sutiles, sin cambiar rutas públicas/privadas, y queda cerrada como validada visualmente por el responsable del proyecto.

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
- No se contó con navegador/headless local para capturas automatizadas; la revisión visual manual ya fue realizada por el responsable del proyecto.
- Login/guards: sin cambios en Fase 1.3.
- No se modificaron backend, `AuthService`, cookies, XSRF, endpoints, base de datos, deploy, dependencias ni rutas privadas en Fase 1.3.
- Documento funcional canónico: `docs/01-product/public-website.md`.
- Checklist responsive canónico: `docs/08-qa/RESPONSIVE_CHECKLIST.md`.
- Fase 1.6 aplicada: header/footer más visuales, hero público con composición institucional, reveal escalonado, parallax ligero del logo, microinteracciones en CTAs/cards y catálogo público más premium.
- Enfoque Fase 1.6: CSS + `IntersectionObserver`; no se instaló GSAP ni otra dependencia.
- `prefers-reduced-motion` desactiva reveal, parallax y transformaciones relevantes; el contenido no depende del movimiento para entenderse.
- `/catalogo` mantiene precios legibles, frames uniformes de imágenes, nota `Precios de referencia 2026 sujetos a confirmación.` y condiciones prudentes.
- `/contacto` diferencia datos confirmados contra pendientes sin inventar dirección, horarios ni WhatsApp.
- `/login` solo recibió pulido visual en SCSS; no se modificó lógica de login, `AuthService`, guards, cookies, XSRF ni `returnUrl`.
- Validación visual manual Fase 1.6 2026-05-27: `/`, `/servicios`, `/catalogo`, `/contacto` y `/login` fueron revisados y aprobados.
- Breakpoints revisados y aprobados manualmente: 360px, 375px, 390px, 414px, 768px, 1024px y desktop.
- Resultado visual Fase 1.6: diseño más atractivo, animaciones sutiles y profesionales, sin scroll horizontal, header móvil estable, logo proporcionado, botones cómodos en celular, catálogo legible, imágenes uniformes, precios correctos, placeholders intencionales y `/contacto` separando datos confirmados contra pendientes.
- Reduced motion queda validado por implementación/código; no se reportaron hallazgos manuales bloqueantes.
- Cierre visual Fase 1.6: cerrada y aprobada visualmente por el responsable del proyecto; lista para revisión/retroalimentación del cliente, manteniendo pendientes de contenido real.

La Fase 0/Fase 1 del sitio público corresponde a este frente nuevo. No contradice el avance del MVP administrativo.

## Dominio Y Deploy

Estado: DEV publicado y validado como baseline UAT inicial; producción pendiente de definición final.

- Dominio principal: `laboratoriodentaltlahuac.com`.
- URL DEV: `https://dev.laboratoriodentaltlahuac.com`.
- Rama DEV desplegada: `dev`.
- Plataforma DEV: VPS.
- Plataforma/DNS/HTTPS productivos: pendientes.
- Fuente canónica de deploy: `docs/05-delivery/DEPLOYMENT.md`.
- Validación DEV canónica: `docs/05-delivery/dev-deployment-validation.md`.

## QA

- QA funcional del MVP administrativo: ejecutada y documentada; Fase 2.2 agrega el reporte `docs/08-qa/private-admin-qa.md`, Fase 2.3 cierra los hallazgos de zona horaria del dashboard y navegación activa, Fase 2.4 agrega pase manual/técnico privado con Admin, Fase 2.5 cierra el pase visual humano privado, Fase 2.6 implementa/prueba por API el usuario QA limitado Development-only y Fase 3.0 registra DEV como baseline UAT inicial.
- QA responsive del sitio público: revisión por código/build ejecutada; Fase 1.6 cerrada como validada visualmente por el responsable del proyecto.
- No existe runner frontend no interactivo; frontend se valida hoy con `npm run build` y revisión manual cuando aplique.
- Validación Fase 1: `npm run build` ejecutado correctamente en `src/LaboratorioTlahuac.Web`.
- Validación Fase 1.1: `npm run build`, `git diff --check`, rutas por `curl` y búsquedas de `/login`, `/app/dashboard` y `/dashboard` ejecutadas.
- Validación 2026-05-13: `npm run build` correcto después del ajuste de guards.
- Revisión de seguridad/routing 2026-05-13: `returnUrl` externo o inválido se normaliza a `/app/dashboard`; usuario autenticado sin permiso conserva flujo a `/app/access-denied`.
- Validación Fase 1.2: `npm run build`, `git diff --check` y búsquedas de rutas ejecutadas.
- Validación Fase 1.3: `npm run build`, `git diff --check`, búsqueda de rutas y verificación de assets del catálogo ejecutadas.
- Validación Fase 1.3.1: `npm run build`, `git diff --check`, búsquedas solicitadas y verificación por nombre de archivos `*:Zone.Identifier` ejecutadas correctamente.
- Validación Fase 1.5: `npm run build`, `git diff --check` y búsquedas solicitadas de logo, contacto, WhatsApp y rutas ejecutadas correctamente.
- Validación Fase 2.0: `npm run build`, `dotnet build`, `dotnet test` y `git diff --check` ejecutados correctamente.
- Validación Fase 2.1: `npm run build`, `dotnet build` y `dotnet test` ejecutados correctamente; el primer `dotnet test` se repitió porque chocó con un `dotnet build` paralelo y produjo bloqueo temporal de archivo en `obj`.
- Validación Fase 2.1 parcial: API levantada en `http://localhost:5277` y `/health` respondió `Healthy`; Angular levantó en `http://localhost:4200/` y `/login` respondió con shell Angular.
- Validación Fase 2.1 bloqueada: no se pudo validar login real, `/api/auth/me` autenticado, logout ni redirección visual de `/app/dashboard` tras logout porque no hay SQL Server local accesible ni Admin local configurado.
- Validación Fase 2.1c preflight Docker: `docker version`, `docker ps`, revisión del contenedor `ldt-labdental-sql`, revisión de puertos `14336`/`14337`/`14338` y verificación de variables locales ejecutadas sin imprimir secretos.
- Validación Fase 2.1c bloqueada: no se creó SQL Server dedicado porque `LDT_SQL_SA_PASSWORD` no está definida; no se configuró user-secrets de conexión, no se ejecutó `dotnet ef database update`, no se levantó API para seed y no se validó login real.
- Validación Fase 2.1c documental: `npm run build`, `dotnet build`, `dotnet test`, `git diff --check` y búsquedas solicitadas ejecutadas correctamente; no se detectaron valores reales de contraseña en archivos versionados, solo nombres de variables o placeholders.
- Validación Fase 2.1c 2026-05-23: `docker ps --filter "name=ldt-labdental-sql"`, `docker port ldt-labdental-sql`, `dotnet ef migrations list`, `dotnet ef database update`, API local, `/health`, `/api/auth/csrf`, `/api/auth/me` sin sesión, apagado de seed, `npm run build`, `dotnet build` y `dotnet test` ejecutados correctamente.
- Validación Fase 2.1c 2026-05-23 pendiente en ejecución por Codex: login real autenticado, `/api/auth/me` autenticado, logout y `/api/auth/me` posterior a logout por ausencia de `LT_ADMIN_EMAIL` y `LT_ADMIN_PASSWORD` en el proceso; login real y dashboard autenticado se cerraron posteriormente por validación manual de Fase 2.1d.
- Validación manual Fase 2.1c 2026-05-23: login real con Admin local confirmado por navegador, redirección a `/app/dashboard` confirmada y redirección post-logout a `/login?returnUrl=%2Fapp%2Fdashboard` confirmada.
- Validación manual Fase 2.1c 2026-05-23 con hallazgo: `/app/dashboard` no queda validado funcionalmente porque al regresar a la página permanece en `Cargando dashboard...`; este hallazgo se cerró posteriormente en Fase 2.1d.
- Validación manual Fase 2.1c 2026-05-23 pendiente como evidencia independiente: confirmar explícitamente `GET /api/auth/me` autenticado y logout como acción independiente.
- Validación técnica post-documentación 2026-05-23: `npm run build`, `dotnet build`, `dotnet test` y `git diff --check` ejecutados correctamente.
- Validación Fase 2.1d 2026-05-23: diagnóstico por código de rutas, guards, `AuthService`, login, dashboard, servicio dashboard y endpoint backend; permiso confirmado como `reports.view` y Admin seed confirmado con `Permissions.All`.
- Validación Fase 2.1d 2026-05-23: `npm run build`, `dotnet test` y `dotnet build` pasaron; un primer `dotnet build` falló por ejecutarse en paralelo con `dotnet test` y bloquear temporalmente `MvcTestingAppManifest.json`, luego pasó en serial.
- Validación HTTP Fase 2.0 con Angular dev server en `http://127.0.0.1:4201/`: `/`, `/servicios`, `/catalogo`, `/contacto`, `/login`, `/app`, `/app/dashboard`, `/dashboard` y casos de `/login?returnUrl=...` respondieron con shell Angular `200`; la protección privada se confirmó por router/guards porque `curl` no ejecuta Angular.
- No existe script `lint` en `src/LaboratorioTlahuac.Web/package.json`.
- Validación Fase 1.6 2026-05-27: `npm run build` correcto desde `src/LaboratorioTlahuac.Web`, sin warnings de presupuesto tras mover estilos públicos pesados a CSS global acotado.
- Validación Fase 1.6 2026-05-27: `dotnet build` correcto con 0 warnings y 0 errores.
- Validación Fase 1.6 2026-05-27: `dotnet test` correcto; Domain 1/1, Application 1/1 y API 90/90.
- Validación Fase 1.6 2026-05-27: `git diff --check` correcto y búsquedas solicitadas de rutas, `prefers-reduced-motion` y `gsap` ejecutadas.
- Validación visual Fase 1.6 2026-05-27: cierre manual confirmado por el responsable del proyecto para rutas públicas, `/login`, breakpoints obligatorios, ausencia de scroll horizontal y comportamiento mobile-first.
- Cierre documental Fase 1.6 2026-05-27: `npm run build`, `dotnet build`, `dotnet test` y `git diff --check` se ejecutaron nuevamente correctamente.
- Cierre documental Fase 1.6 y Fase 2.1d 2026-05-27: `npm run build`, `dotnet build`, `dotnet test`, `git diff --check` y búsquedas obligatorias ejecutadas correctamente.
- Confirmación Fase 2.1d 2026-05-27: dashboard autenticado cerrado manualmente; `GET /api/auth/me` autenticado queda sin inspección independiente y `GET /api/dashboard/summary` autenticado queda validado indirectamente por carga del dashboard.
- Confirmación de rutas 2026-05-27: `/login` sigue público; `/app` y `/app/dashboard` siguen privados por routing/guards; `/dashboard` no es ruta privada real.
- Confirmación de secretos 2026-05-27: la búsqueda en documentos tocados solo encontró nombres de variables, placeholders, textos redactados o menciones de `user-secrets`; no se detectaron valores reales de contraseña, tokens, API keys ni llaves privadas.
- Validación Fase 2.2 2026-05-27: `npm run build`, `dotnet build`, `dotnet test`, `git diff --check` y búsquedas obligatorias ejecutadas correctamente; búsquedas de patrones sensibles se ejecutaron con salida limitada a archivos para no imprimir valores.
- Validación Fase 2.3 2026-05-27: `npm run build`, `dotnet build`, `dotnet test` y `git diff --check` correctos; `dotnet build` cerró con 0 warnings y 0 errores, y `dotnet test` cerró con Domain 1/1, Application 1/1 y API 91/91 tras agregar prueba de zona horaria.
- Validación Fase 2.3 2026-05-27: `docker ps --filter "name=ldt-labdental-sql"` y `docker port ldt-labdental-sql` confirmaron SQL dedicado en `14336`; `docker port` requirió permiso fuera del sandbox.
- Validación Fase 2.3 2026-05-27: búsquedas obligatorias de rutas, `routerLinkActive`, `America/Mexico_City`, variables sensibles, `ConnectionStrings` y `codex-cobranza-sql` ejecutadas; las búsquedas de patrones sensibles se limitaron a archivos para no imprimir valores.
- Nota de validación Fase 2.3: la primera ejecución de `dotnet test` falló por tener dos constructores públicos en el fixture de pruebas; se corrigió dejando un solo constructor público y se repitió correctamente.
- Zona horaria formal de negocio definida en Fase 2.3: `America/Mexico_City`; el hallazgo de `dueToday=0` por cálculo UTC queda corregido por código y prueba automatizada.
- Validación Fase 2.4 2026-05-27: preflight Docker, API/Angular locales, rutas públicas, rutas privadas por shell Angular, login/logout Admin por API, dashboard con fecha operativa, revisión de navegación activa por código y revisión de mecanismo de usuario limitado ejecutados sin imprimir secretos.
- Validación de cierre Fase 2.4 2026-05-27: `npm run build`, `dotnet build`, `dotnet test` y `git diff --check` ejecutados correctamente; `dotnet test` cerró con Domain 1/1, Application 1/1 y API 91/91.
- Búsquedas obligatorias Fase 2.4: rutas, `routerLinkActive`, `America/Mexico_City`, compatibilidad Windows de zona horaria, variables sensibles, `ConnectionStrings` y `codex-cobranza-sql` ejecutadas; las búsquedas de patrones sensibles se limitaron a archivos para no imprimir valores.
- Limitación Fase 2.4: no existe navegador/headless local sin instalar dependencias; la navegación activa queda pendiente de pase visual humano aunque el código y build validan `routerLinkActive`.
- Limitación Fase 2.4: no existe mecanismo seguro de producto para crear usuario limitado local; `/app/access-denied` queda pendiente de validación manual con usuario real.
- Validación manual Fase 2.5 2026-05-28: resultados visuales humanos concretos recibidos y documentados; la fase queda completada para pase visual humano privado.
- Usuario QA limitado Fase 2.5: se recomienda backlog técnico inmediato de seed solo Development documentado en `docs/08-qa/limited-user-qa-plan.md`; no se implementó.
- Validación de cierre Fase 2.5 2026-05-28: `npm run build`, `dotnet build`, `dotnet test`, `git diff --check` y búsquedas obligatorias ejecutadas correctamente; las búsquedas de patrones sensibles se limitaron a archivos para no imprimir valores.
- Validacion Fase 2.6 2026-05-28: `npm run build` correcto desde `src/LaboratorioTlahuac.Web`.
- Validacion Fase 2.6 2026-05-28: `dotnet build` correcto con 0 warnings y 0 errores.
- Validacion Fase 2.6 2026-05-28: `dotnet test` correcto; Domain 1/1, Application 1/1 y API 101/101.
- Validacion automatizada Fase 2.6: el usuario QA limitado creado por configuracion de tests puede iniciar sesion, `/api/auth/me` devuelve `customers.view` sin `reports.view`, `/api/customers` responde `200`, `/api/dashboard/summary` responde `403` con sesion limitada y `401` sin sesion.
- Validacion local real Fase 2.6: no ejecutada porque no hay variables QA limitadas seguras en el proceso y no se ejecuta `dotnet user-secrets list`.
- Validacion navegador Fase 2.6: no ejecutada porque no hay navegador/headless local disponible sin instalar dependencias.
- Validacion de cierre Fase 2.6 2026-05-28: `git diff --check` correcto y busquedas obligatorias ejecutadas; busquedas de patrones sensibles se limitaron a archivos para no imprimir valores.
- Validación Fase 3.0 2026-07-02: `curl` sin credenciales contra `https://dev.laboratoriodentaltlahuac.com`, `/servicios`, `/catalogo`, `/contacto`, `/login` y `/app/dashboard` respondió `200`; se documenta que en SPA Angular la ruta privada puede devolver shell `200` por `curl`, mientras la validación real de guards fue manual por navegador.
- Validación técnica Fase 3.0 2026-07-02: `npm run build` correcto desde `src/LaboratorioTlahuac.Web`; `dotnet build` correcto con 0 errores y 2 warnings `NU1903` por vulnerabilidad conocida de `SQLitePCLRaw.lib.e_sqlite3` 2.1.11 en tests; `dotnet test` correcto con Domain 1/1, Application 1/1 y API 101/101.
- Validación Fase 3.1 2026-07-02: `npm run build` correcto desde `src/LaboratorioTlahuac.Web`; `dotnet build` correcto con 0 errores y 2 warnings `NU1903` conocidos por `SQLitePCLRaw.lib.e_sqlite3` 2.1.11 en tests; `dotnet test` correcto con Domain 1/1, Application 1/1 y API 101/101; `git diff --check` correcto; búsquedas obligatorias de órdenes, `WorkOrder`, `Delivery`, repartidor, catálogo, `/dashboard`, `/app/dashboard` y `/login` ejecutadas.

## Comercial

- Paquete comercial de primera ronda documentado en `docs/09-commercial/`.
- Las fases comerciales no son el roadmap técnico interno.
- Próxima conversación comercial: demo con cliente, alcance, precio, prioridades y materiales del sitio.

## Próxima Tarea Recomendada

Fase 3.2 - MVP impresión de etiquetas desde órdenes existentes.

Alcance sugerido: agregar acciones desde `/app/ordenes/:id` para imprimir etiqueta interna y etiqueta de entrega, crear rutas privadas de impresión bajo `/app/ordenes/:id/etiqueta-trabajo` y `/app/ordenes/:id/etiqueta-entrega`, usar CSS de impresión browser/CSS con tamaños en mm y evitar dependencias, migraciones, impresora directa, PDF obligatorio o QR/barcode en el MVP.

Mantener pendientes de cliente para el sitio público: confirmar vigencia de precios 2026, condiciones comerciales del cartel, WhatsApp como canal real, dirección, horarios y reemplazo de imágenes faltantes por archivos `.webp` específicos.

Pendiente paralelo: cerrar validación de usuario QA limitado y `/app/access-denied` en DEV si aún no queda formalmente validada con cuenta limitada real sin `reports.view`.

Backlog futuro separado: Fase 3.3 entrega/repartidor mobile-first, Fase 3.4 administración de usuarios/roles y Fase 3.5 administración de catálogo, precios e imágenes.
