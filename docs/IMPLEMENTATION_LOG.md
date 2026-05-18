# Bitácora De Implementación

## Decisión De Registro

- `docs/IMPLEMENTATION_LOG.md` es la bitácora operativa de tareas ejecutadas por Codex.
- `docs/00-governance/changelog.md` se mantiene como changelog histórico de entregas relevantes.
- Cuando una tarea documental cambie fuentes canónicas, debe registrarse aquí y, si afecta entregables del proyecto, también en el changelog.

## 2026-05-15 - Fase 2.1 Preflight Local Admin Y Login Real

### Cambio Realizado

Se ejecutó el preflight de configuración local segura para validar login real contra API/base local sin modificar backend, frontend, `AuthService`, guards, cookies, XSRF, endpoints, migraciones, deploy, dependencias ni rutas privadas.

### Archivos Leídos

- `AGENTS.md`
- `README.md`
- `docs/03-architecture/AUTH_FLOW.md`
- `docs/03-architecture/ARCHITECTURE.md`
- `docs/PROJECT_STATUS.md`
- `docs/IMPLEMENTATION_LOG.md`
- `src/LaboratorioTlahuac.Api/appsettings.json`
- `src/LaboratorioTlahuac.Api/appsettings.Development.json`
- `src/LaboratorioTlahuac.Api/LaboratorioTlahuac.Api.csproj`
- `src/LaboratorioTlahuac.Infrastructure/Security/Seed/SecuritySeeder.cs`
- `src/LaboratorioTlahuac.Domain/Security/Permissions.cs`
- `src/LaboratorioTlahuac.Web/src/app/app.routes.ts`
- `docs/01-product/internal-system.md`
- `docs/08-qa/RESPONSIVE_CHECKLIST.md`

### Archivos Modificados

- `docs/PROJECT_STATUS.md`
- `docs/IMPLEMENTATION_LOG.md`
- `docs/03-architecture/AUTH_FLOW.md`
- `docs/01-product/internal-system.md`
- `docs/08-qa/RESPONSIVE_CHECKLIST.md`

### Preflight De Ambiente

- `git status --short`: sin cambios iniciales.
- `git diff --stat`: sin cambios iniciales.
- Connection string de desarrollo detectada: `Server=localhost;Database=LaboratorioTlahuac_Dev;Trusted_Connection=True;TrustServerCertificate=True`.
- La connection string apunta claramente a ambiente local por `localhost`; no se detectó conexión remota/productiva.
- `dotnet ef --version`: disponible, versión `10.0.7`.
- `dotnet ef migrations list` compiló correctamente y listó migraciones existentes, pero no pudo determinar estado aplicado porque SQL Server no estuvo accesible.
- Migraciones existentes: `InitialSecurityModel`, `AddCustomersAndInternalDoctors`, `AddWorkOrders`, `AddPayments`.

### Base Local Y Migraciones

- `dotnet ef database update` falló por no poder conectar a SQL Server en `localhost`.
- No se aplicaron migraciones.
- No se creó ni modificó base de datos.

Comandos exactos para preparar una base local con SQL Server disponible, sin guardar secretos en archivos versionados:

```bash
docker run --name laboratorio-tlahuac-sql-local -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=<password-local-sql-seguro>" -p 1433:1433 -d mcr.microsoft.com/mssql/server:2022-latest
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost,1433;Database=LaboratorioTlahuac_Dev;User Id=sa;Password=<password-local-sql-seguro>;TrustServerCertificate=True" --project src/LaboratorioTlahuac.Api/LaboratorioTlahuac.Api.csproj
dotnet ef database update --project src/LaboratorioTlahuac.Infrastructure/LaboratorioTlahuac.Infrastructure.csproj --startup-project src/LaboratorioTlahuac.Api/LaboratorioTlahuac.Api.csproj
```

Si ya existe SQL Server local compatible con `Trusted_Connection=True`, basta con levantarlo y repetir `dotnet ef database update`.

### Admin Local

- Variables de entorno revisadas sin imprimir valores: `LT_ADMIN_EMAIL`, `LT_ADMIN_PASSWORD` y `LT_ADMIN_FULL_NAME` no están definidas; `SecuritySeed__RunOnStartup` no está en `true`.
- No existe archivo de user-secrets para `laboratorio-tlahuac-api-dev` en este entorno.
- No se ejecutó seed Admin porque faltan credenciales locales seguras y la base local no está accesible.
- No se inventaron credenciales, no se imprimieron passwords y no se documentó ningún secreto real.

Comandos exactos para configurar Admin local con user-secrets:

```bash
dotnet user-secrets set LT_ADMIN_EMAIL "<email-local>" --project src/LaboratorioTlahuac.Api/LaboratorioTlahuac.Api.csproj
dotnet user-secrets set LT_ADMIN_PASSWORD "<password-local-seguro>" --project src/LaboratorioTlahuac.Api/LaboratorioTlahuac.Api.csproj
dotnet user-secrets set LT_ADMIN_FULL_NAME "Administrador" --project src/LaboratorioTlahuac.Api/LaboratorioTlahuac.Api.csproj
dotnet user-secrets set SecuritySeed:RunOnStartup true --project src/LaboratorioTlahuac.Api/LaboratorioTlahuac.Api.csproj
```

Después de crear el Admin, se recomienda apagar el seed local:

```bash
dotnet user-secrets set SecuritySeed:RunOnStartup false --project src/LaboratorioTlahuac.Api/LaboratorioTlahuac.Api.csproj
```

### Validación Ejecutada

- API levantada con `dotnet run --project src/LaboratorioTlahuac.Api/LaboratorioTlahuac.Api.csproj`.
- `curl -s http://localhost:5277/health`: respondió `{"status":"Healthy","application":"LaboratorioTlahuac.Api"}`.
- Angular levantado desde `src/LaboratorioTlahuac.Web` con `npm start` en `http://localhost:4200/`.
- `curl -s http://localhost:4200/login`: respondió shell Angular.
- `GET /api/auth/csrf` con cookie jar temporal: `204`.
- `GET /api/auth/me` sin sesión: `401`.
- `npm run build`: correcto.
- `dotnet build`: correcto, 0 warnings y 0 errores.
- `dotnet test`: correcto después de repetirlo en serial; Domain 1/1, Application 1/1 y API 90/90.

### Validación Bloqueada

- Login real desde `/login`: pendiente por falta de SQL Server local accesible y Admin local configurado.
- Redirección post-login a `/app/dashboard`: pendiente.
- `/api/auth/me` autenticado: pendiente.
- Logout autenticado: pendiente.
- Redirección de `/app/dashboard` sin sesión tras logout a `/login?returnUrl=%2Fapp%2Fdashboard`: pendiente de navegador con sesión real.
- Validación con usuario sin `reports.view`: pendiente porque no hay base local con usuarios de prueba.

### Permisos Confirmados Por Código

- `SecuritySeeder` asigna al rol Admin todos los permisos de `Permissions.All`.
- `Permissions.All` incluye `reports.view`.
- `/app/dashboard` tiene `permissionGuard` con `data: { permission: 'reports.view' }`.

### Pendientes Para El Humano

1. Levantar SQL Server local o contenedor local y configurar la connection string por user-secrets si se usa usuario/contraseña.
2. Aplicar migraciones con `dotnet ef database update`.
3. Configurar Admin local con user-secrets o variables de entorno.
4. Arrancar API con `SecuritySeed:RunOnStartup=true` una vez para crear/actualizar Admin.
5. Apagar `SecuritySeed:RunOnStartup` en user-secrets cuando ya no se necesite.
6. Validar login real en navegador y con `curl` sin imprimir contraseña.

## 2026-05-15 - Fase 2.0 Validación Login, Sesión Y Redirección

### Cambio Realizado

Se validó el flujo técnico de entrada desde el sitio público hacia la app privada sin rediseñar pantallas, sin implementar módulos nuevos y sin tocar backend, guards, `AuthService`, cookies, XSRF, endpoints, base de datos, migraciones, deploy, dependencias ni rutas privadas.

### Archivos Leídos

- `AGENTS.md`
- `README.md`
- `docs/PROJECT_STATUS.md`
- `docs/03-architecture/AUTH_FLOW.md`
- `docs/03-architecture/ARCHITECTURE.md`
- `docs/01-product/public-website.md`
- `docs/01-product/internal-system.md`
- `docs/08-qa/RESPONSIVE_CHECKLIST.md`
- `docs/ROADMAP.md`
- `src/LaboratorioTlahuac.Web/src/app/app.routes.ts`
- `src/LaboratorioTlahuac.Web/src/app/auth/pages/login/login-page.component.ts`
- `src/LaboratorioTlahuac.Web/src/app/core/auth/auth.service.ts`
- `src/LaboratorioTlahuac.Web/src/app/core/guards/auth.guard.ts`
- `src/LaboratorioTlahuac.Web/src/app/core/guards/permission.guard.ts`
- `src/LaboratorioTlahuac.Web/src/environments/environment.ts`
- `src/LaboratorioTlahuac.Web/src/environments/environment.development.ts`
- `src/LaboratorioTlahuac.Api/appsettings.json`
- `src/LaboratorioTlahuac.Api/appsettings.Development.json`
- `src/LaboratorioTlahuac.Api/Program.cs`
- `src/LaboratorioTlahuac.Api/Endpoints/AuthEndpoints.cs`

### Archivos Modificados

- `docs/PROJECT_STATUS.md`
- `docs/ROADMAP.md`
- `docs/IMPLEMENTATION_LOG.md`
- `docs/03-architecture/AUTH_FLOW.md`
- `docs/01-product/internal-system.md`
- `docs/01-product/public-website.md`
- `docs/08-qa/RESPONSIVE_CHECKLIST.md`

### Resultados De Validación

- `/login` sigue público en `app.routes.ts`.
- `/app` sigue bajo `PrivateLayoutComponent` con `authGuard`.
- `/app/dashboard` sigue bajo `/app`, con `permissionGuard` y permiso `reports.view`.
- `/dashboard` no existe como ruta privada real; el wildcard del router redirige a la home pública.
- `AuthService.login()` sigue solicitando CSRF, ejecutando `POST /api/auth/login` con `withCredentials`, renovando CSRF y guardando usuario en memoria.
- `login-page.component.ts` conserva manejo de error `423` e inválidos, usa `AuthService.login()` y navega con `router.navigateByUrl(this.getReturnUrl())`.
- `returnUrl` acepta `/app`, `/app/...`, `/app?...` y `/app#...`.
- `returnUrl` rechaza valores externos o inválidos como `https://example.com`, `//example.com`, `javascript:alert(1)`, valores con espacios, backslash o rutas fuera de `/app`; el fallback es `/app/dashboard`.
- Usuario sin sesión en `/app/*` se redirige por guards a `/login?returnUrl=...`.
- Usuario autenticado sin permiso se redirige por `permissionGuard` a `/app/access-denied`; no se trata como usuario sin sesión.

### Validaciones Ejecutadas

- `git status --short`: sin cambios iniciales.
- `git diff --stat`: sin cambios iniciales.
- `npm run build` desde `src/LaboratorioTlahuac.Web`: correcto.
- `dotnet build` desde la raíz: correcto, 0 warnings y 0 errores.
- `dotnet test` desde la raíz: correcto; Domain 1/1, Application 1/1 y API 90/90.
- `git diff --check`: correcto.
- Angular dev server en `http://127.0.0.1:4201/` porque el puerto 4200 ya estaba ocupado.
- `curl` contra `http://127.0.0.1:4201/`, `/servicios`, `/catalogo`, `/contacto`, `/login`, `/app`, `/app/dashboard`, `/dashboard`, `/login?returnUrl=%2Fapp%2Fdashboard`, `/login?returnUrl=https://example.com`, `/login?returnUrl=//example.com` y `/login?returnUrl=javascript:alert(1)`: todos respondieron con shell Angular `200`.

### Pendiente De Login Real

No se validó login real con credenciales porque el entorno local no tiene Admin configurado en `appsettings*.json`: `SecuritySeed:RunOnStartup` está en `false` y `SecuritySeed:Admin` está vacío. No se inventaron credenciales, no se modificó seed y no se tocó base de datos.

Pasos exactos para validación humana:

1. Configurar API/base local y usuario Admin por los mecanismos seguros del proyecto.
2. Levantar API con `dotnet run --project src/LaboratorioTlahuac.Api/LaboratorioTlahuac.Api.csproj`.
3. Levantar Angular desde `src/LaboratorioTlahuac.Web` con `npm start`.
4. Abrir `/login`, iniciar sesión con el Admin local configurado y confirmar redirección a `/app/dashboard`.
5. Confirmar que `GET /api/auth/me` responde el usuario autenticado.
6. Ejecutar logout desde la UI si está disponible.
7. Confirmar que después de logout `/app/dashboard` vuelve a redirigir a `/login?returnUrl=%2Fapp%2Fdashboard`.

## 2026-05-15 - Fase 1.5 Identidad Visual Y Contacto

### Cambio Realizado

Se incorporó identidad visual real del laboratorio en el sitio público: logo LDT, colores institucionales y datos de contacto tomados del cartel/catálogo.

### Archivos Creados

- `docs/02-domain/brand-guidelines.md`

### Asset Incorporado

- `src/LaboratorioTlahuac.Web/src/assets/brand/logo-ldt.webp`
- Ruta pública esperada: `/assets/brand/logo-ldt.webp`

### Archivos Modificados

- `src/LaboratorioTlahuac.Web/src/app/public/layout/public-layout.component.ts`
- `src/LaboratorioTlahuac.Web/src/app/public/layout/public-layout.component.scss`
- `src/LaboratorioTlahuac.Web/src/app/public/pages/home/home-page.component.ts`
- `src/LaboratorioTlahuac.Web/src/app/public/pages/home/home-page.component.scss`
- `src/LaboratorioTlahuac.Web/src/app/public/pages/services/services-page.component.ts`
- `src/LaboratorioTlahuac.Web/src/app/public/pages/catalog/catalog-page.component.ts`
- `src/LaboratorioTlahuac.Web/src/app/public/pages/catalog/catalog-page.component.scss`
- `src/LaboratorioTlahuac.Web/src/app/public/pages/contact/contact-page.component.ts`
- `src/LaboratorioTlahuac.Web/src/app/auth/pages/login/login-page.component.ts`
- `src/LaboratorioTlahuac.Web/src/app/auth/pages/login/login-page.component.scss`
- `docs/PROJECT_STATUS.md`
- `docs/ROADMAP.md`
- `docs/IMPLEMENTATION_LOG.md`
- `docs/01-product/public-website.md`
- `docs/08-qa/RESPONSIVE_CHECKLIST.md`
- `docs/03-architecture/ARCHITECTURE.md`
- `docs/README.md`

### Identidad Y Contacto

- Tokens aplicados: `--ldt-navy`, `--ldt-navy-soft`, `--ldt-blue`, `--ldt-blue-dark`, `--ldt-sky`, `--ldt-sky-light`, `--ldt-gray` y `--ldt-white`.
- Eslogan incorporado: `Precisión • Estética • Confianza`.
- Línea descriptiva incorporada: `Prótesis, restauraciones y soluciones dentales`.
- Teléfonos incorporados como `tel:`: 55 3331 9445, 55 2161 2311 y 55 9802 9816.
- Correo incorporado como `mailto:`: `contacto@laboratoriodentaltlahuac.com`.
- Condiciones visibles en cartel documentadas con prudencia: `Anticipo 50%` y `Trabajos urgentes +40%` requieren confirmación final del cliente.

### Alcance

- No se modificó backend.
- No se modificó `AuthService`.
- No se modificaron `auth.guard.ts` ni `permission.guard.ts`.
- No se modificaron cookies, XSRF, endpoints, base de datos, migraciones, deploy ni dependencias.
- No se modificaron rutas privadas.
- `/login` sigue como entrada pública.
- `/app` y `/app/dashboard` siguen como zona privada.
- `/dashboard` no se creó como ruta privada real.
- No se inventó dirección, horario, WhatsApp, redes sociales ni mapa.

### Validaciones Ejecutadas

- `npm run build` desde `src/LaboratorioTlahuac.Web`: correcto.
- `git diff --check`: correcto.
- `rg "logo-ldt" src/LaboratorioTlahuac.Web/src docs`
- `rg "55 3331 9445" .`
- `rg "55 2161 2311" .`
- `rg "55 9802 9816" .`
- `rg "contacto@laboratoriodentaltlahuac.com" .`
- `rg "WhatsApp" src/LaboratorioTlahuac.Web/src/app/public docs/01-product/public-website.md`
- `rg "/dashboard" .`
- `rg "/app/dashboard" .`
- `rg "/login" src/LaboratorioTlahuac.Web/src/app docs README.md AGENTS.md`

### Pendientes

- Revisión visual real en 360px, 375px, 390px, 414px, 768px, 1024px y desktop.
- Confirmar con el cliente si algún teléfono debe publicarse como WhatsApp.
- Confirmar dirección, horarios y mapa antes de publicarlos.
- Aprobar precios 2026 y condiciones comerciales antes de publicación formal.

## 2026-05-15 - Backlog Futuro Administración De Catálogo

### Cambio Realizado

Se documentó como backlog futuro la funcionalidad `Administración de catálogo, precios e imágenes`.

### Archivos Creados

- `docs/01-product/admin-catalog-management.md`

### Archivos Modificados

- `docs/ROADMAP.md`
- `docs/PROJECT_STATUS.md`
- `docs/IMPLEMENTATION_LOG.md`
- `docs/01-product/public-website.md`
- `docs/01-product/internal-system.md`
- `docs/README.md`

### Alcance Documentado

- No pertenece a la fase actual.
- No se implementa todavía.
- Será una futura mejora de la app privada bajo `/app`.
- Requerirá permisos administrativos, con permiso sugerido `catalog.manage` o equivalente.
- Requerirá definir modelo de datos, endpoints, almacenamiento de imágenes, reglas de publicación y aprobación de precios públicos.
- El catálogo público actual seguirá funcionando desde `catalog-data.ts` hasta que se diseñe y apruebe esta fase.

### Alcance No Ejecutado

- No se implementaron pantallas.
- No se crearon rutas.
- No se tocó backend.
- No se tocó frontend funcional.
- No se tocó auth.
- No se tocaron guards.
- No se tocó base de datos.
- No se crearon migraciones.
- No se crearon endpoints.
- No se instalaron dependencias.
- No se cambió deploy.
- No se modificó el catálogo público actual.

## 2026-05-14 - Ignore De Zone.Identifier

### Cambio Realizado

Se agregó `*:Zone.Identifier` a `.gitignore` para evitar que vuelvan a entrar al control de versiones archivos alternos generados al copiar assets desde Windows.

### Archivos Modificados

- `.gitignore`
- `docs/IMPLEMENTATION_LOG.md`

### Alcance

- No se modificó código.
- No se modificó documentación fuera de esta bitácora.

## 2026-05-14 - Fase 1.3.1 Cierre De Catálogo Público

### Cambio Realizado

Se cerró la revisión técnica del catálogo público en `/catalogo`, se retiraron del working tree los assets `:Zone.Identifier` detectados en la carpeta de productos y se preparó la documentación para revisión visual/comercial del cliente.

### Archivos Modificados

- `src/LaboratorioTlahuac.Web/src/app/public/pages/catalog/catalog-page.component.ts`
- `docs/PROJECT_STATUS.md`
- `docs/ROADMAP.md`
- `docs/IMPLEMENTATION_LOG.md`
- `docs/01-product/public-website.md`
- `docs/08-qa/RESPONSIVE_CHECKLIST.md`

### Archivos Eliminados

- 22 archivos `*:Zone.Identifier` dentro de `src/LaboratorioTlahuac.Web/src/assets/catalog/products/`.
- No se borraron imágenes `.webp`.
- No se borró `protesis-removible-unidad-acrilica.jpg`.

### Validación Del Catálogo

- `/catalogo` está configurado como ruta pública bajo `PublicLayoutComponent`.
- `/servicios` enlaza a `/catalogo`.
- `/login` sigue como ruta pública de entrada al sistema.
- `/app` y `/app/dashboard` siguen bajo layout privado con guards.
- `/dashboard` no existe como ruta privada real; las menciones restantes son documentación, API de dashboard o `/app/dashboard`.
- `catalog-data.ts` contiene 12 secciones y 40 productos.
- Los precios permanecen como números y se formatean con `Intl.NumberFormat('es-MX')`.
- Hay 19 productos con imagen específica, 16 con imagen representativa de sección y 5 placeholders.
- Placeholders restantes: Reparación de dentadura por fractura, Gancho volado, Descanso metálico c/u, Rebase y Aumentar dientes c/u.
- Todas las imágenes referenciadas por el catálogo existen en `src/LaboratorioTlahuac.Web/src/assets/catalog/products/`.

### Copy Comercial

- Se agregó la nota visible `Precios de referencia 2026 sujetos a confirmación.`.
- Los precios provienen del cartel proporcionado y requieren aprobación final del cliente antes de publicación formal.
- No se agregaron condiciones comerciales nuevas.

### Configuración Y Assets

- `angular.json` no se modificó.
- La configuración actual copia `src/assets/**/*.webp` hacia `assets`, suficiente para el catálogo actual.
- Ese glob no copia archivos `:Zone.Identifier` desde `src/assets`.
- `find . -name '*:Zone.Identifier' -type f -print`: sin resultados.
- `rg "Zone.Identifier" .`: solo devuelve menciones documentales, no archivos físicos.

### Validaciones Ejecutadas

- `npm run build` desde `src/LaboratorioTlahuac.Web`: correcto.
- `git diff --check`: correcto.
- `rg "Zone.Identifier" .`
- `rg "/catalogo" src/LaboratorioTlahuac.Web/src/app docs README.md AGENTS.md`
- `rg "/dashboard" .`
- `rg "/app/dashboard" .`
- `rg "/login" src/LaboratorioTlahuac.Web/src/app docs README.md AGENTS.md`
- `git status --short`: muestra las bajas esperadas de `*:Zone.Identifier` y cambios documentales/frontend de esta fase.

### Pendientes

- Revisión visual real de `/catalogo` y rutas públicas en 360px, 375px, 390px, 414px, 768px, 1024px y desktop.
- Aprobación final del cliente sobre precios 2026, vigencia, condiciones comerciales y publicación.
- Reemplazar placeholders y fallbacks por imágenes `.webp` específicas cuando el cliente entregue o apruebe assets.

## 2026-05-13 - Fase 1.3 Catálogo Público

### Cambio Realizado

Se implementó un catálogo público mobile-first con secciones, productos, precios e imágenes locales. La ruta elegida fue `/catalogo` para mantener una página dedicada al volumen del catálogo y conservar `/servicios` como vista introductoria.

### Archivos Creados

- `src/LaboratorioTlahuac.Web/src/app/public/data/catalog-data.ts`
- `src/LaboratorioTlahuac.Web/src/app/public/pages/catalog/catalog-page.component.ts`
- `src/LaboratorioTlahuac.Web/src/app/public/pages/catalog/catalog-page.component.scss`

### Archivos Modificados

- `src/LaboratorioTlahuac.Web/angular.json`
- `src/LaboratorioTlahuac.Web/src/app/app.routes.ts`
- `src/LaboratorioTlahuac.Web/src/app/public/layout/public-layout.component.ts`
- `src/LaboratorioTlahuac.Web/src/app/public/layout/public-layout.component.scss`
- `src/LaboratorioTlahuac.Web/src/app/public/pages/home/home-page.component.ts`
- `src/LaboratorioTlahuac.Web/src/app/public/pages/services/services-page.component.ts`
- `README.md`
- `docs/PROJECT_STATUS.md`
- `docs/ROADMAP.md`
- `docs/IMPLEMENTATION_LOG.md`
- `docs/01-product/public-website.md`
- `docs/03-architecture/ARCHITECTURE.md`
- `docs/08-qa/RESPONSIVE_CHECKLIST.md`

### Estructura Del Catálogo

- Data tipada: `src/LaboratorioTlahuac.Web/src/app/public/data/catalog-data.ts`.
- Interfaces: `CatalogSection` y `CatalogProduct`.
- Imágenes fuente: `src/LaboratorioTlahuac.Web/src/assets/catalog/products/`.
- Ruta pública de imágenes en Angular: `/assets/catalog/products/...`.
- Se agregó `src/assets/**/*.webp` como asset del frontend en `angular.json`.
- No se copian archivos `Zone.Identifier` ni imágenes `.jpg`; el catálogo usa `.webp`.

### Manejo De Imágenes

- Imagen específica del producto si existe.
- Imagen representativa de sección si falta la específica.
- Placeholder visual con iniciales si no hay imagen de producto ni de sección.
- Todas las imágenes usan frame con `aspect-ratio: 4 / 3`, `object-fit: contain`, fondo claro y centrado.

### Imágenes Faltantes O Con Fallback

- Usan imagen de sección: carillas/incrustaciones sin imagen propia, productos sin foto exacta dentro de Zirconia, E-MAX, SIGNUM, Metal-porcelana, Metálicos, Totally Natural, iFlex, Prótesis removible y Prótesis inmediata.
- Usan placeholder: productos de `Servicios prostodónticos`, porque no hay imagen de sección ni producto.
- `protesis-removible-unidad-acrilica.jpg` existe localmente, pero no se usa porque esta fase definió `.webp`.

### Alcance

- No se modificaron backend, `AuthService`, guards, cookies, XSRF, endpoints, base de datos, deploy, dependencias ni rutas privadas.
- `/login` sigue como entrada pública.
- `/app` y `/app/dashboard` siguen siendo zona privada.
- `/dashboard` no se creó como ruta real.

### Validaciones Ejecutadas

- `npm run build` desde `src/LaboratorioTlahuac.Web`: correcto.
- `git diff --check`: correcto.
- `curl -i -s http://127.0.0.1:4200/catalogo`: responde `200 OK` con shell Angular.
- `rg "/dashboard" .`: no muestra `/dashboard` como ruta privada real; las menciones corresponden a documentación, API de dashboard o `/app/dashboard`.
- `rg "/app/dashboard" .`: confirma que la ruta privada real se mantiene bajo `/app`.
- `rg "/login" src/LaboratorioTlahuac.Web/src/app docs README.md AGENTS.md`: confirma `/login` como entrada pública.
- Verificación de assets generados: se copian imágenes `.webp` del catálogo a `dist/laboratorio-tlahuac-web/browser/assets/catalog/products/`.
- No se ejecutó lint porque `src/LaboratorioTlahuac.Web/package.json` no define script `lint`.
- No se ejecutó `dotnet build` ni `dotnet test` porque no se modificó backend ni configuración compartida.

### Pendientes Generados

- Confirmar vigencia de precios con el cliente antes de publicación formal.
- Completar imágenes `.webp` específicas para productos que hoy usan imagen de sección o placeholder.
- Revisar visualmente `/catalogo` en 360px, 375px, 390px, 414px, 768px, 1024px y desktop.

## 2026-05-13 - Fase 1.2 Contenido Público Seguro

### Cambio Realizado

Se ejecutó Fase 1.2 de forma parcial porque no se recibieron datos reales confirmados del cliente. Se pulió el copy público para revisión, se retiró el CTA que mencionaba WhatsApp como acción principal y se dejó claro en el sitio que los datos de contacto y el catálogo final no están confirmados.

### Archivos Modificados

- `src/LaboratorioTlahuac.Web/src/app/public/layout/public-layout.component.ts`
- `src/LaboratorioTlahuac.Web/src/app/public/pages/home/home-page.component.ts`
- `src/LaboratorioTlahuac.Web/src/app/public/pages/services/services-page.component.ts`
- `src/LaboratorioTlahuac.Web/src/app/public/pages/contact/contact-page.component.ts`
- `docs/PROJECT_STATUS.md`
- `docs/ROADMAP.md`
- `docs/IMPLEMENTATION_LOG.md`
- `docs/01-product/public-website.md`
- `docs/08-qa/RESPONSIVE_CHECKLIST.md`

### Contenido Incorporado

- No se incorporó contenido real nuevo porque WhatsApp, dirección, horarios, logo, servicios exactos, texto principal aprobado y materiales visuales siguen pendientes.
- Se incorporó copy seguro para revisión, sin presentar datos no confirmados como definitivos.

### Placeholders Retirados O Reducidos

- Se retiró `WhatsApp pendiente por confirmar` como CTA principal.
- El CTA principal ahora lleva a `/contacto` con texto neutral.
- El footer ya no lista datos pendientes como si fueran contenido de contacto; indica que se publicarán solo con datos confirmados.

### Alcance

- No se modificaron backend, `AuthService`, guards, cookies, XSRF, endpoints, base de datos, deploy, dependencias ni rutas privadas.
- `/login` sigue visible como entrada pública.
- `/app` y `/app/dashboard` siguen siendo zona privada.
- `/dashboard` no se creó como ruta real.

### Pendientes Generados

- Recibir WhatsApp real, dirección, horarios, logo, servicios exactos, texto principal aprobado y materiales visuales.
- Revisar visualmente el sitio en 360px, 375px, 390px, 414px, 768px, 1024px y desktop.

### Validaciones Ejecutadas

- `npm run build` desde `src/LaboratorioTlahuac.Web`: correcto.
- `git diff --check`: correcto.
- `rg "/dashboard" .`
- `rg "/app/dashboard" .`
- `rg "/login" src/LaboratorioTlahuac.Web/src/app docs README.md AGENTS.md`
- `rg "pendiente" docs/01-product/public-website.md src/LaboratorioTlahuac.Web/src/app/public`
- `curl -i -s http://127.0.0.1:4200/`
- `curl -i -s http://127.0.0.1:4200/servicios`
- `curl -i -s http://127.0.0.1:4200/contacto`
- `curl -i -s http://127.0.0.1:4200/login`
- `curl -i -s http://127.0.0.1:4200/app`
- `curl -i -s http://127.0.0.1:4200/app/dashboard`

Las pruebas con `curl` confirman que el dev server sirve el shell Angular en esas rutas. La validación visual y redirecciones de router deben confirmarse en navegador real.

## 2026-05-13 - Revisión Seguridad/Routing De Guards Y ReturnUrl

### Cambio Realizado

Se revisó el flujo de guards y login después de corregir la pantalla en blanco de `/app/dashboard` sin sesión. Se endureció la sanitización de `returnUrl` para aceptar solo rutas internas bajo `/app` y usar fallback seguro `/app/dashboard` para valores externos o inválidos.

### Archivos Modificados

- `src/LaboratorioTlahuac.Web/src/app/auth/pages/login/login-page.component.ts`
- `docs/PROJECT_STATUS.md`
- `docs/IMPLEMENTATION_LOG.md`
- `docs/03-architecture/AUTH_FLOW.md`

### Comportamiento Confirmado Por Código

- `authGuard` redirige usuario sin sesión a `/login?returnUrl=...`.
- `authGuard` también redirige a login si falla la verificación inicial de sesión.
- `permissionGuard` conserva `/app/access-denied` para usuario autenticado sin permiso.
- `permissionGuard` no trata falta de permiso como falta de sesión.
- `returnUrl` preserva rutas internas como `/app`, `/app/dashboard`, `/app/clientes`, `/app/ordenes` y `/app/pagos`.
- `returnUrl` rechaza `https://example.com`, `http://example.com`, `//example.com`, `javascript:alert(1)`, valores con espacios y valores con backslash.
- `/dashboard` no se creó como ruta privada real.

### Validaciones Ejecutadas

- `git status --short`
- `git diff --stat`
- `rg -n "returnUrl|getSafePrivateReturnUrl|navigateByUrl|createUrlTree" src/LaboratorioTlahuac.Web/src/app`
- `npm run build` desde `src/LaboratorioTlahuac.Web`: correcto.
- `git diff --check`: correcto.
- `rg "/dashboard" .`
- `rg "/app/dashboard" .`
- `rg "returnUrl" src/LaboratorioTlahuac.Web/src/app`
- `rg "/login" src/LaboratorioTlahuac.Web/src/app docs README.md AGENTS.md`
- `curl -i -s http://127.0.0.1:4200/app/dashboard`
- `curl -i -s http://127.0.0.1:4200/app`
- `curl -i -s 'http://127.0.0.1:4200/login?returnUrl=%2Fapp%2Fdashboard'`
- `curl -i -s 'http://127.0.0.1:4200/login?returnUrl=https://example.com'`
- `curl -i -s 'http://127.0.0.1:4200/login?returnUrl=//example.com'`

Las pruebas con `curl` confirman que el dev server sirve el shell Angular para esas URLs. La redirección real de cliente requiere navegador porque ocurre dentro del router Angular.

### Pendientes

- Confirmar en navegador real el cambio de URL de `/app/dashboard` sin sesión a `/login?returnUrl=%2Fapp%2Fdashboard`.
- Confirmar en navegador real los casos inválidos: `returnUrl=https://example.com` y `returnUrl=//example.com`.
- No se ejecutaron pruebas con sesión autenticada porque no se levantó API ni usuario de prueba en esta tarea.

## 2026-05-13 - Fase 1.1 Hallazgo Manual De Redirección Privada

### Cambio Realizado

Se atendió el hallazgo manual: al escribir directamente `http://127.0.0.1:4200/app/dashboard` sin sesión, la app podía quedar en blanco si la verificación de sesión fallaba con un error distinto a `401`. Ahora los guards frontend tratan ese error como sesión no autenticada y devuelven un `UrlTree` hacia `/login` con `returnUrl`.

### Archivos Modificados

- `src/LaboratorioTlahuac.Web/src/app/core/guards/auth.guard.ts`
- `src/LaboratorioTlahuac.Web/src/app/core/guards/permission.guard.ts`
- `docs/PROJECT_STATUS.md`
- `docs/IMPLEMENTATION_LOG.md`
- `docs/01-product/public-website.md`
- `docs/03-architecture/AUTH_FLOW.md`
- `docs/08-qa/RESPONSIVE_CHECKLIST.md`

### Alcance

- No se modificó `AuthService`.
- No se modificaron cookies, XSRF, endpoints, backend, base de datos, deploy ni dependencias.
- No se creó `/dashboard` como ruta privada real.
- `/app` y `/app/dashboard` siguen siendo zona privada.

### Validaciones Ejecutadas

- `npm run build` desde `src/LaboratorioTlahuac.Web`: correcto.
- Dev server local recompiló después del cambio.

### Pendientes Generados

- Confirmar manualmente en navegador que `/app/dashboard` sin sesión redirige a `/login?returnUrl=/app/dashboard`.
- Completar revisión visual real en los breakpoints definidos.
- Confirmar contenido real del cliente antes de reemplazar placeholders.

## 2026-05-12 - Fase 1.1 QA Responsive Del Sitio Público

### Cambio Realizado

Se ejecutó una revisión responsive técnica del sitio público y se hicieron ajustes menores de SCSS/layout para reducir riesgo de overflow antes de revisión con cliente.

### Archivos Modificados

- `src/LaboratorioTlahuac.Web/src/app/public/layout/public-layout.component.scss`
- `src/LaboratorioTlahuac.Web/src/app/public/pages/home/home-page.component.scss`
- `src/LaboratorioTlahuac.Web/src/app/public/pages/services/services-page.component.ts`
- `src/LaboratorioTlahuac.Web/src/app/public/pages/contact/contact-page.component.ts`
- `README.md`
- `docs/README.md`
- `docs/PROJECT_STATUS.md`
- `docs/ROADMAP.md`
- `docs/IMPLEMENTATION_LOG.md`
- `docs/01-product/public-website.md`
- `docs/08-qa/RESPONSIVE_CHECKLIST.md`

### Ajustes Responsive

- Footer público ajustado para usar columnas flexibles en tablet/desktop y evitar overflow por columnas `auto`.
- Header, links, botones y footer aceptan wrapping de textos largos.
- Cards/listas públicas mantienen `min-width: 0` para evitar desbordes dentro de grids.
- Páginas `/servicios` y `/contacto` usan padding responsive y ancho máximo de lectura en textos introductorios.
- Botones públicos conservan mínimo táctil de 48px y texto centrado.

### Rutas Verificadas

- `/`: responde en dev server local.
- `/servicios`: responde en dev server local.
- `/contacto`: responde en dev server local.
- `/login`: responde como entrada al sistema, sin cambios de auth.
- `/app` y `/app/dashboard`: responden con shell Angular; la privacidad se confirma por configuración de rutas/guards, sin modificar guards.

### Validaciones Ejecutadas

- `git status --short`
- `git diff --stat`
- `npm run build` desde `src/LaboratorioTlahuac.Web`: correcto.
- `git diff --check`: correcto.
- `curl -i -s http://127.0.0.1:4200/`
- `curl -i -s http://127.0.0.1:4200/servicios`
- `curl -i -s http://127.0.0.1:4200/contacto`
- `curl -i -s http://127.0.0.1:4200/login`
- `curl -i -s http://127.0.0.1:4200/app`
- `curl -i -s http://127.0.0.1:4200/app/dashboard`
- `rg "/dashboard" .`
- `rg "/app/dashboard" .`
- `rg "/login" src/LaboratorioTlahuac.Web/src/app docs README.md AGENTS.md`

### Limitación Del Entorno

No se encontró Chromium, Chrome, Firefox, Playwright, Puppeteer ni `wkhtmltoimage` disponibles sin instalar dependencias. Por esa razón no se generaron capturas ni se marcó como completada la revisión visual por breakpoint.

### Pendientes Generados

- Revisar visualmente 360px, 375px, 390px, 414px, 768px, 1024px y desktop en navegador real o dispositivo.
- Si se revisa desde celular en la misma red, levantar temporalmente Angular con `npm start -- --host 0.0.0.0 --port 4200`; `127.0.0.1` solo sirve en la computadora local.
- Confirmar WhatsApp real, dirección, horarios, logo, servicios exactos, textos finales y materiales visuales aprobados.

## 2026-05-12 - Fase 1 Sitio Público Mobile-First

### Cambio Realizado

Se implementó la primera versión pública del sitio institucional mobile-first dentro de la app Angular existente, sin crear una segunda app y sin modificar backend, autenticación, endpoints, base de datos, deploy ni rutas privadas.

### Archivos Creados

- `src/LaboratorioTlahuac.Web/src/app/public/pages/home/home-page.component.scss`

### Archivos Modificados

- `src/LaboratorioTlahuac.Web/src/index.html`
- `src/LaboratorioTlahuac.Web/src/app/app.routes.ts`
- `src/LaboratorioTlahuac.Web/src/app/public/layout/public-layout.component.ts`
- `src/LaboratorioTlahuac.Web/src/app/public/layout/public-layout.component.scss`
- `src/LaboratorioTlahuac.Web/src/app/public/pages/home/home-page.component.ts`
- `src/LaboratorioTlahuac.Web/src/app/public/pages/services/services-page.component.ts`
- `src/LaboratorioTlahuac.Web/src/app/public/pages/contact/contact-page.component.ts`
- `README.md`
- `docs/PROJECT_STATUS.md`
- `docs/ROADMAP.md`
- `docs/IMPLEMENTATION_LOG.md`
- `docs/01-product/public-website.md`
- `docs/03-architecture/ARCHITECTURE.md`
- `docs/08-qa/RESPONSIVE_CHECKLIST.md`

### Rutas Confirmadas

- `/`: sitio público institucional.
- `/servicios`: página pública de capacidades provisionales.
- `/contacto`: página pública de contacto provisional.
- `/login`: entrada pública al sistema, sin cambios de auth.
- `/app` y `/app/dashboard`: zona privada, sin cambios.

### Validaciones Ejecutadas

- `npm run build` desde `src/LaboratorioTlahuac.Web`: correcto.
- `npm start -- --host 127.0.0.1 --port 4200`: servidor Angular levantado en `http://127.0.0.1:4200/`.
- `curl -s http://127.0.0.1:4200/`: confirma `lang="es"`, title y meta description del sitio público.
- Búsqueda de referencias de `/login`, `/app` y `/app/dashboard`: rutas privadas reales se mantienen bajo `/app`.
- No se ejecutó lint porque `src/LaboratorioTlahuac.Web/package.json` no define script `lint`.
- No se ejecutó `dotnet build` ni `dotnet test` porque no se modificó backend ni configuración compartida.

### Pendientes Generados

- Confirmar WhatsApp, dirección, horarios, logo, servicios exactos y textos finales con el cliente.
- Revisar visualmente los viewports obligatorios del checklist responsive.
- Validar `/app` y `/login` en navegador después de levantar entorno local con API si se hará demo integral.
- Preparar Fase 1.1 con ajustes visuales por feedback y contenido real.

## 2026-05-12 - Fase 0.2 Consolidación Documental

### Cambio Realizado

Se consolidó la documentación para separar sistema privado, sitio público, control global, deploy, QA y documentación comercial antes de iniciar pantallas del sitio público.

### Documentos Creados

- `docs/README.md`
- `docs/01-product/public-website.md`
- `docs/01-product/internal-system.md`
- `docs/03-architecture/ARCHITECTURE.md`
- `docs/03-architecture/AUTH_FLOW.md`
- `docs/05-delivery/DEPLOYMENT.md`
- `docs/08-qa/RESPONSIVE_CHECKLIST.md`

### Documentos Movidos Con `git mv`

- `docs/03-architecture/architecture-overview.md` -> `docs/03-architecture/ARCHITECTURE.md`
- `docs/03-architecture/authentication-and-authorization.md` -> `docs/03-architecture/AUTH_FLOW.md`
- `docs/06-operations/deployment.md` -> `docs/05-delivery/DEPLOYMENT.md`

### Puentes Creados O Reemplazados

- `docs/ARCHITECTURE.md` -> `docs/03-architecture/ARCHITECTURE.md`
- `docs/AUTH_FLOW.md` -> `docs/03-architecture/AUTH_FLOW.md`
- `docs/DEPLOYMENT.md` -> `docs/05-delivery/DEPLOYMENT.md`
- `docs/RESPONSIVE_CHECKLIST.md` -> `docs/08-qa/RESPONSIVE_CHECKLIST.md`
- `docs/03-architecture/architecture-overview.md` -> `docs/03-architecture/ARCHITECTURE.md`
- `docs/03-architecture/authentication-and-authorization.md` -> `docs/03-architecture/AUTH_FLOW.md`
- `docs/06-operations/deployment.md` -> `docs/05-delivery/DEPLOYMENT.md`

### Documentos Modificados

- `AGENTS.md`
- `README.md`
- `docs/PROJECT_STATUS.md`
- `docs/ROADMAP.md`
- `docs/IMPLEMENTATION_LOG.md`
- `docs/00-governance/changelog.md`
- `docs/00-governance/project-status.md`
- `docs/09-commercial/commercial-phases.md`

### Motivo

Evitar duplicados y contradicciones entre la documentación del MVP administrativo avanzado y el frente nuevo del sitio público institucional.

### Pendientes Generados

- Iniciar Fase 1 del sitio público mobile-first.
- Confirmar contenido del cliente para home, servicios y contacto.
- Revisar `src/LaboratorioTlahuac.Web/README.md`, que aún es el README generado por Angular CLI.
- Cuando se eliminen puentes en una fase posterior, actualizar cualquier referencia restante.

## 2026-05-12 - Auditoría Documental

### Cambio Realizado

Se realizó una auditoría documental del repositorio para revisar alineación entre `AGENTS.md`, `README.md`, documentación existente en `docs/`, documentos nuevos de Fase 0, y el inventario vacío de `.agents/` y `.codex/`.

### Archivos Modificados

- `docs/DOCUMENTATION_AUDIT.md`
- `docs/PROJECT_STATUS.md`
- `docs/IMPLEMENTATION_LOG.md`

### Motivo

Detectar duplicados, solapamientos, contradicciones y fuentes canónicas antes de avanzar con el sitio público mobile-first.

### Pendientes Generados

- Definir si se aprueba la estructura documental propuesta.
- Consolidar `README.md`, documentos raíz de `docs/` y carpetas numeradas sin tocar código.
- Separar explícitamente documentación del sistema privado, sitio público, control global y documentación comercial.

## 2026-05-12 - Fase 0 Sitio Público

### Cambio Realizado

Inicialización de Fase 0 para el sitio público de Laboratorio Dental Tláhuac. Se inspeccionó la estructura del repositorio, se detectó el stack existente y se creó documentación raíz para guiar el desarrollo mobile-first del sitio público dentro del repo actual.

### Archivos Modificados

- `AGENTS.md`
- `docs/PROJECT_STATUS.md`
- `docs/ROADMAP.md`
- `docs/IMPLEMENTATION_LOG.md`
- `docs/ARCHITECTURE.md`
- `docs/RESPONSIVE_CHECKLIST.md`
- `docs/DEPLOYMENT.md`
- `docs/AUTH_FLOW.md`

### Motivo

Dejar reglas permanentes para Codex, documentar el estado real del proyecto y definir el plan técnico inicial antes de implementar pantallas complejas o cambios de lógica.

### Pendientes Generados

- Ejecutar Fase 1 con rediseño mobile-first del sitio público existente.
- Confirmar contenido real del cliente: servicios, ubicación, horarios, teléfono, WhatsApp y mensajes comerciales.
- Definir plataforma de despliegue, DNS, HTTPS y configuración productiva.
- Validar visualmente el sitio en anchos móviles antes de presentarlo al cliente.
