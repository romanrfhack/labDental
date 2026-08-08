# Bitácora De Implementación

## Decisión De Registro

- `docs/IMPLEMENTATION_LOG.md` es la bitácora operativa de tareas ejecutadas por Codex.
- `docs/00-governance/changelog.md` se mantiene como changelog histórico de entregas relevantes.
- Cuando una tarea documental cambie fuentes canónicas, debe registrarse aquí y, si afecta entregables del proyecto, también en el changelog.

## 2026-08-08 - Fase 3.5.4.2 UI Upload De Imágenes De Productos

### Frontend Implementado

- `AdminCatalogService` agrega `uploadProductImage(productId, file)` y `clearProductImage(productId)` usando XSRF y `withCredentials: true`.
- Upload crea `FormData` con una sola parte exacta `file`; no fija `Content-Type`, por lo que el navegador genera el multipart boundary.
- El editor de producto agrega el bloque `Imagen del producto` con origen amigable, selector de asset existente, preview definitivo/local, file input, subir/reemplazar y quitar.
- Productos nuevos no intentan upload sin id y muestran `Guarda el producto antes de subir una imagen personalizada.`.
- La UI valida archivo no vacío, máximo `2_097_152` bytes, extensión WebP/JPG/JPEG/PNG, MIME permitido y coherencia básica antes del POST; backend conserva autoridad y validación de firma.
- Preview local usa `URL.createObjectURL()` y revoca la URL al cambiar, cancelar, completar o destruir el componente.
- POST/DELETE actualizan `products()` y `productForm.imagePath` sin requerir guardar otra vez el formulario.
- DELETE pide confirmación y comunica desasociación; no afirma ni intenta borrado físico.
- `catalog.manage` controla upload/quitar; `catalog.view` queda readonly y `Repartidor` conserva falta de acceso.
- La imagen de sección continúa usando selección de asset existente; no se inventó endpoint de secciones.
- `isAllowedCatalogImagePath()` acepta solo `CATALOG_IMAGE_OPTIONS` o `/api/catalog/images/{32 hex}.{webp|jpg|jpeg|png}`; otras rutas `/api`, URLs y esquemas arbitrarios permanecen rechazados.

### Contexto DEV Registrado

- Fase 3.5.4.1 desplegada en commit `1b0384c414b54f541394dbe0e2f1e4a4d9329e93`, release `dev-42-1b0384c`, GitHub Actions `success`.
- `/health`, `/api/catalog/public` y `/catalogo`: `200`; GET de imagen inexistente: `404`.
- Storage persistente `/var/www/laboratorio-tlahuac-dev/shared/catalog-images`, `www-data:www-data`, permisos `0750` y escritura validada para `www-data`.
- `CatalogImages__StoragePath` configurado una sola vez fuera del repositorio en `/etc/laboratorio-tlahuac-dev/api.env`.

### Documentación

- Se creó `docs/08-qa/catalog-image-upload-ui-qa.md` con los 25 casos manuales solicitados.
- Se actualizaron fuentes de producto, sitio público, arquitectura, QA, estado, roadmap, índices y README.

### Exclusiones Confirmadas

- No se modificó backend, `/catalogo` público, upload de secciones, migraciones, dependencias, `AuthService`, guards, cookies, política XSRF ni deploy.
- No se hizo commit.
- No se imprimieron secretos, no se ejecutó `dotnet user-secrets list` y no se usó `codex-cobranza-sql`.

### Validaciones Ejecutadas

- `npm run build`: correcto; initial total `318.75 kB`, sin warning de budget.
- `dotnet build`: correcto con 0 errores; permanecen los warnings `NU1903` conocidos en tests.
- `dotnet test`: correcto; 158/158 pruebas.
- El frontend no tiene script `npm test`; no se agregó framework.
- `git diff --check`: correcto.
- Búsquedas obligatorias de métodos, endpoints, `FormData`, límite de 2 MB, rutas de imagen, permisos, assets y rutas públicas/privadas: ejecutadas.
- Búsquedas de `LT_ADMIN_PASSWORD`, `LT_QA_LIMITED_PASSWORD`, `LDT_SQL_SA_PASSWORD` y `ConnectionStrings`: ejecutadas con `rg -l`, mostrando solo nombres de archivo.

### Siguiente Paso Recomendado

Desplegar Fase 3.5.4.2 a DEV y ejecutar la primera prueba real end-to-end de upload, reemplazo, desasociación y persistencia después de otro deploy.

## 2026-08-08 - Fase 3.5.4.1 Backend Upload De Imágenes De Catálogo

### Backend Implementado

- Se agregaron `POST /api/admin/catalog/products/{id}/image`, `DELETE /api/admin/catalog/products/{id}/image` y `GET /api/catalog/images/{fileName}`.
- POST/DELETE requieren `catalog.manage` y conservan la política XSRF global; GET es público.
- `CatalogImagesOptions.StoragePath` se configura mediante `CatalogImages__StoragePath`; appsettings deja el valor vacío para evitar rutas productivas falsas.
- `ICatalogImageStorage`/`CatalogImageStorage` encapsulan validación y filesystem; Application recibe un `Stream` y no depende de `IFormFile`.
- Se validan archivo único `file`, no vacío, máximo 2,097,152 bytes, extensiones `.webp`/`.jpg`/`.jpeg`/`.png`, MIME permitido/coherente y firmas mínimas PNG/JPEG/RIFF-WEBP.
- El nombre físico ignora el original y usa GUID lowercase de 32 hex + extensión normalizada.
- La escritura usa temporal exclusivo dentro del storage, copia por streaming, rename final sin overwrite y comprobación canónica de raíz.
- El producto debe existir antes de escribir; se guarda `/api/catalog/images/{fileName}` y se toca `UpdatedAtUtc`.
- Si falla la base después de crear el archivo, se intenta retirar solo el archivo nuevo. Reemplazo y DELETE no borran la imagen anterior.
- Storage no configurado/no disponible responde `503` genérico; exceso de tamaño responde `413`; validaciones responden `400`.
- GET devuelve `404` seguro para nombre inválido/inexistente, MIME correcto, cache inmutable y `X-Content-Type-Options: nosniff`.
- `CatalogImagePathValidator` conserva `assets/catalog/products/...` y acepta exactamente rutas nuevas con nombre generado; rechaza otras rutas `/api`, URLs externas y path traversal.

### Pruebas

- `CatalogIntegrationTests` cubre autorización `401`/`403`, producto `404`, archivo faltante/vacío/múltiple, extensión, MIME, coherencia, firma, tamaño, storage no disponible y formatos válidos WebP/JPG/JPEG/PNG.
- Se cubren GET público, nombre inválido/inexistente/query, propagación a `/api/catalog/public`, DELETE sin borrado físico y exclusión de `catalog.manage` para `Repartidor`.
- Cada `TestApplicationFactory` usa una carpeta temporal GUID aislada, sobreescribe la configuración y elimina solo esa carpeta al terminar.
- Suite focalizada de catálogo: 27/27 correcta.

### Documentación

- Se creó `docs/08-qa/catalog-image-upload-api-qa.md`.
- Se actualizaron fuentes de producto, arquitectura/auth, deploy, QA, estado, roadmap, índices y README.

### Exclusiones Confirmadas

- No se modificó UI admin ni `/catalogo` público.
- No se crearon migraciones ni dependencias.
- No se modificaron `AuthService`, guards, cookies, política XSRF, scripts, workflow, Nginx, systemd ni VPS.
- No se borran archivos anteriores ni se implementa limpieza de huérfanos.
- No se hizo commit.

### Validaciones Ejecutadas

- `dotnet build`: correcto con 0 errores y 2 warnings `NU1903` conocidos.
- `dotnet test --no-build`: correcto; Domain 1/1, Application 1/1 y API 156/156.
- `npm run build` desde `src/LaboratorioTlahuac.Web`: correcto; initial total `317.77 kB`, sin warning de budget.
- Suite focalizada `CatalogIntegrationTests`: 27/27 correcta.
- `git diff --check`: correcto.
- Búsquedas obligatorias de endpoints, configuración, storage, permisos, `IFormFile`, multipart, path traversal, assets, rutas y patrones sensibles ejecutadas; patrones sensibles limitados a nombres de archivo.

### Siguiente Fase Recomendada

Preparar almacenamiento DEV y ejecutar Fase 3.5.4.2 — UI upload/reemplazo desde `/app/admin/catalogo`.

## 2026-08-08 - Fase 3.5.4.0 Diseño Operativo De Almacenamiento De Imágenes

### Análisis De Deploy

- Se revisaron `.github/workflows/deploy.yml` y `.github/scripts/deploy-lab.sh` sin modificarlos.
- El deploy instala backend y frontend en `releases/{releaseId}`, mueve `backend/current` y `frontend/current`, conserva cinco releases y crea un directorio `shared` genérico fuera de la poda.
- Los uploads no deben vivir dentro de releases ni de `frontend/current`, porque los symlinks cambian y los releases antiguos se eliminan.
- La configuración exacta de Nginx no está versionada. Como DEV ya expone `/api/catalog/public`, se documenta el supuesto verificable de que el proxy cubre `/api`; el endpoint propuesto no requerirá cambio Nginx si ese supuesto se confirma.
- No se inspeccionó ni modificó el VPS. La ruta DEV esperada se marcó pendiente de confirmación contra el `LDT_APP_ROOT` real.

### Diseño Definido

- Almacenamiento persistente: `${LDT_APP_ROOT}/shared/catalog-images`; ruta DEV esperada `/var/www/laboratorio-tlahuac-dev/shared/catalog-images` sin hardcodearla en aplicación.
- Lectura pública: `GET /api/catalog/images/{fileName}`.
- Upload/reemplazo: `POST /api/admin/catalog/products/{id}/image` con `multipart/form-data`.
- Desasociación: `DELETE /api/admin/catalog/products/{id}/image`; establece `ImagePath` en `null` y no borra el archivo físico.
- El producto guarda `/api/catalog/images/{fileName}` en `ImagePath`; los assets heredados `assets/catalog/products/...` siguen funcionando.
- Formatos permitidos: `.webp`, `.jpg`, `.jpeg` y `.png`; máximo 2 MB; extensión, MIME y firma coherentes.
- Nombre único generado por servidor, comprobación canónica de raíz, bloqueo de path traversal y rechazo de URLs externas.
- Se prefiere WebP, pero no se convierte ni agrega una dependencia en esta fase.
- Upload y desasociación requieren `catalog.manage` y XSRF; lectura de imagen es pública; `catalog.view` no permite mutar y `Repartidor` queda sin acceso.
- El archivo anterior no se borra automáticamente. La limpieza de huérfanos se difiere hasta contar con backup, inventario de referencias y retención.
- Backup recomendado: base de datos y `shared/catalog-images` del mismo punto temporal, fuera de releases y preferentemente fuera del VPS, con restauración de muestra antes de producción.

### Documentación Creada

- `docs/01-product/catalog-image-upload-design.md`

### Documentación Actualizada

- `docs/01-product/catalog-admin-design.md`
- `docs/01-product/admin-catalog-management.md`
- `docs/05-delivery/DEPLOYMENT.md`
- `docs/ROADMAP.md`
- `docs/IMPLEMENTATION_LOG.md`

### Exclusiones Confirmadas

- No se implementó código funcional.
- No se modificaron frontend ni backend funcional.
- No se crearon migraciones ni dependencias.
- No se modificaron scripts, workflow, Nginx, systemd ni VPS.
- No se tocaron autenticación, guards, cookies, sesión ni XSRF.
- No se imprimieron secretos ni se ejecutó `dotnet user-secrets list`.
- No se usó `codex-cobranza-sql`.
- No se hizo commit.

### Validaciones Ejecutadas

- `git diff --check`: correcto.
- `rg "catalog-images" docs README.md .github src`: referencias nuevas presentes en los documentos de diseño, deploy y roadmap.
- `rg "catalog.manage" docs README.md src tests`: permiso existente y reglas propuestas consistentes.
- `rg "upload" docs README.md src`: alcance previo, exclusiones y diseño 3.5.4.0 revisados.
- `rg "codex-cobranza-sql" docs README.md AGENTS.md`: solo menciones documentales/históricas de no uso; no se invocó el contenedor.

### Siguiente Fase Recomendada

Fase 3.5.4.1 — backend upload/reemplazo de imágenes de catálogo.

## 2026-08-08 - Cierre QA DEV Fase 3.5.3 Y Deploy Resiliente

### Cierre QA DEV

- Commit desplegado: `8be9e14ec8cda5e8486770a77733a4413e456e96`.
- GitHub Actions: `success`.
- `GET /health`: `200`.
- `GET /catalogo` sin sesión: `200`.
- `GET /api/catalog/public` sin sesión: `200`.
- El responsable del proyecto confirmó que activar y desactivar productos desde `/app/admin/catalogo` se refleja correctamente en `/catalogo`.
- El responsable del proyecto confirmó que cambiar nombre y precio desde `/app/admin/catalogo` se refleja correctamente en `/catalogo`, conservando precios visibles en MXN.
- Resultado: camino principal admin → API pública → catálogo público validado sin bug claro.
- La prueba forzada del fallback con API bloqueada/offline no se ejecutó en DEV y queda como cobertura manual opcional; no bloquea este cierre.

### Cierre Del Ajuste De Deploy

- El intento anterior del commit `11ea0a296253d2e0a2660963430d49482dc4aaee` falló durante el health check posterior al restart.
- La causa probable se mantiene como timing demasiado agresivo del check anterior, sin evidencia clara de crash del release.
- El deploy exitoso de `8be9e14ec8cda5e8486770a77733a4413e456e96` validó el health check con reintentos y cerró el pendiente técnico.

### Documentación Actualizada

- `README.md`
- `docs/README.md`
- `docs/PROJECT_STATUS.md`
- `docs/ROADMAP.md`
- `docs/IMPLEMENTATION_LOG.md`
- `docs/01-product/public-website.md`
- `docs/01-product/catalog-admin-design.md`
- `docs/01-product/admin-catalog-management.md`
- `docs/05-delivery/DEPLOYMENT.md`
- `docs/08-qa/public-catalog-api-qa.md`
- `docs/08-qa/catalog-api-qa.md`
- `docs/08-qa/catalog-admin-ui-qa.md`

### Exclusiones Confirmadas

- No se modificó código funcional.
- No se modificaron backend, frontend, migraciones ni catálogo.
- No se tocaron `AuthService`, guards, cookies, XSRF ni permisos.
- No se modificó deploy.
- No se instalaron dependencias.
- No se usó `codex-cobranza-sql`.
- No se imprimieron secretos.
- No se hizo commit.

### Validaciones Ejecutadas

- `npm run build`: correcto; initial total `317.77 kB`, sin warning de budget.
- `dotnet build`: correcto con 0 errores y 2 warnings `NU1903` conocidos por `SQLitePCLRaw.lib.e_sqlite3` 2.1.11 en tests.
- `dotnet test --no-build --verbosity normal`: correcto; Domain 1/1, Application 1/1 y API 140/140.
- Verificación HTTP independiente de `/health`, `/catalogo` y `/api/catalog/public`: `200`.
- `git diff --check`: correcto.

### Siguiente Fase Recomendada

Fase 3.5.4 - carga/reemplazo de imágenes desde admin, o una fase corta de pulido QA del catálogo público si aparecen hallazgos visuales.

## 2026-08-08 - Health Check Robusto En Deploy DEV

### Cambio Realizado

- El workflow pasa al script remoto los endpoints de health local y público; para `dev` son `http://127.0.0.1:5012/health` y `https://dev.laboratoriodentaltlahuac.com/health`.
- `deploy-lab.sh` reemplaza la espera fija de 5 segundos y el único `curl` público por `wait_for_health`, con hasta 30 intentos, pausa de 3 segundos, `curl -fsS`, timeout de conexión de 2 segundos, timeout total de 5 segundos y requisito explícito de HTTP `200`.
- El release nuevo valida primero health local y después health público.
- Antes de hacer rollback se imprimen `systemctl status`, las últimas 120 entradas de `journalctl` y los destinos de los symlinks actuales, sin mostrar variables de entorno ni cadenas de conexión.
- El rollback restaura backend y frontend, reinicia la API y valida también health local y público con reintentos. Cualquier falla de restauración, restart o health se reporta explícitamente.

### Diagnóstico Documentado

El deploy de `dev-38-11ea0a2` recibió `502` durante una ventana de health check de aproximadamente 8 segundos y el rollback dejó estable `dev-37-3dc0347`. Como no hubo evidencia clara de crash y arranques previos necesitaron aproximadamente 15–20 segundos para llegar a `Now listening`, el incidente se trata como probable timing demasiado agresivo del health check anterior.

### Exclusiones Confirmadas

- No se modificó código funcional de backend o frontend.
- No se modificaron migraciones ni catálogo.
- No se tocaron autenticación, guards, cookies, XSRF ni permisos.
- No se instalaron dependencias.
- No se hizo commit.

### Validaciones Ejecutadas

- `bash -n .github/scripts/deploy-lab.sh`: correcto.
- Ruta de fallo de `wait_for_health`: correcta con dos intentos controlados contra un puerto local cerrado.
- `git diff --check`: correcto.
- Búsquedas solicitadas de health check, rollback, restart y endpoints local/público: correctas.
- Búsquedas de patrones sensibles ejecutadas con `rg -l`, limitando la salida a nombres de archivo.

### Siguiente Paso Recomendado

Estado posterior: cerrado con el deploy exitoso del commit `8be9e14ec8cda5e8486770a77733a4413e456e96`, documentado en la entrada de cierre QA DEV de esta misma fecha.

## 2026-07-10 - Fase 3.5.3 Catálogo Público Consume API

### Cambio Realizado

Se conectó `/catalogo` público a `GET /api/catalog/public` manteniendo `catalog-data.ts` como fallback local para evitar que la página quede vacía si la API falla, tarda demasiado o devuelve una respuesta inválida.

### Frontend

- Nuevo `PublicCatalogService` en `src/LaboratorioTlahuac.Web/src/app/public/services/public-catalog.service.ts`.
- `getPublicCatalog()` consulta `GET /api/catalog/public` sin `withCredentials` explícito y mapea la respuesta pública al modelo que ya usa la vista.
- El mapper valida que existan secciones, que haya al menos un producto total y que `key`, `name`, `priceAmount`, `products`, `imagePath` y `altText` tengan forma esperada.
- `catalog-page.component.ts` inicia con `catalogSections` desde `catalog-data.ts`, muestra loading breve, reemplaza por API válida y vuelve al fallback ante HTTP error, timeout, respuesta nula, secciones vacías, catálogo sin productos o error de mapeo.
- Se conserva el layout público actual, carrusel, galería, precios MXN, placeholders, `imagePath` y `altText` cuando existe.
- El mensaje de fallback es no técnico: `Mostrando catálogo de referencia disponible.`.

### Documentación Actualizada

- `README.md`
- `docs/README.md`
- `docs/PROJECT_STATUS.md`
- `docs/ROADMAP.md`
- `docs/IMPLEMENTATION_LOG.md`
- `docs/01-product/public-website.md`
- `docs/01-product/catalog-admin-design.md`
- `docs/01-product/admin-catalog-management.md`
- `docs/03-architecture/ARCHITECTURE.md`
- `docs/08-qa/catalog-api-qa.md`
- `docs/08-qa/catalog-admin-ui-qa.md`

### Documentación Creada

- `docs/08-qa/public-catalog-api-qa.md`

### Exclusiones Confirmadas

- No se modificó UI admin.
- No se modificó backend.
- No se crearon migraciones.
- No se implementó upload de imágenes.
- No se eliminó `catalog-data.ts`.
- No se movieron assets.
- No se tocó `AuthService`.
- No se modificaron guards.
- No se tocaron cookies ni XSRF.
- No se tocó deploy.
- No se instalaron dependencias.
- No se hizo commit.

### Validaciones Ejecutadas

- `npm run build` desde `src/LaboratorioTlahuac.Web`: correcto; initial total `317.77 kB`, sin warning de budget.
- `dotnet build`: correcto con 0 errores y 2 warnings `NU1903` conocidos por `SQLitePCLRaw.lib.e_sqlite3` 2.1.11 en tests.
- `dotnet test`: correcto; Domain 1/1, Application 1/1 y API 140/140.
- `git diff --check`: correcto.
- Búsquedas obligatorias de catálogo, rutas públicas/privadas, permisos, upload, variables sensibles, `ConnectionStrings` y `codex-cobranza-sql`: ejecutadas. Las búsquedas de patrones sensibles se limitaron a nombres de archivo para no imprimir valores.

### Siguiente Fase Recomendada

Fase 3.5.4 - carga/reemplazo de imágenes desde admin, o pulido QA de catálogo público si hay hallazgos en DEV.

## 2026-07-05 - QA DEV Fase 3.5.2 UI Admin Catálogo

### Cierre QA DEV

Se documentó el cierre QA DEV de Fase 3.5.2 para la UI privada de administración de catálogo/precios bajo `/app/admin/catalogo`.

Validación manual reportada por el responsable del proyecto:

- Commit desplegado: `e89d1f0b872d253838dc77f5df5fafb61522f9db`.
- GitHub Actions: `success`.
- `GET /health`: `200`.
- `GET /api/admin/catalog/sections` sin sesión: `401`.
- `GET /api/admin/catalog/products` sin sesión: `401`.
- Login Admin: OK.
- Navegación privada muestra `Catálogo` para Admin: OK.
- `/app/admin/catalogo` carga: OK.
- Listado de secciones/productos y filtro por sección: OK.
- Crear, editar y activar/desactivar secciones: OK.
- Crear, editar, activar/desactivar productos y actualizar precio: OK.
- Precio negativo bloqueado: OK.
- Selección de `imagePath` existente, preview y limpiar imagen: OK.
- `/catalogo` público sigue funcionando: OK.
- `Repartidor` no ve `Catálogo` y no accede a `/app/admin/catalogo`: OK.
- Observaciones: sin hallazgos ni bug claro reportado.

### Cambio Realizado

Solo documentación para registrar cierre QA DEV. No se modificó código de aplicación.

### Documentación Actualizada

- `README.md`
- `docs/README.md`
- `docs/PROJECT_STATUS.md`
- `docs/ROADMAP.md`
- `docs/IMPLEMENTATION_LOG.md`
- `docs/01-product/admin-catalog-management.md`
- `docs/01-product/catalog-admin-design.md`
- `docs/01-product/public-website.md`
- `docs/08-qa/catalog-admin-ui-qa.md`

### Exclusiones Confirmadas

- No se modificó backend.
- No se modificaron migraciones.
- No se tocó `AuthService`.
- No se modificaron guards.
- No se tocaron cookies ni XSRF.
- No se tocó deploy.
- No se instalaron dependencias.
- No se imprimieron secretos.
- No se usó `codex-cobranza-sql`.

### Validaciones Ejecutadas

- `npm run build` desde `src/LaboratorioTlahuac.Web`: correcto; initial total `317.27 kB`, sin warning de budget.
- `dotnet build`: correcto con 0 errores y 2 warnings `NU1903` conocidos por `SQLitePCLRaw.lib.e_sqlite3` 2.1.11 en tests.
- `dotnet test`: correcto; Domain 1/1, Application 1/1 y API 140/140.
- `git diff --check`: correcto.

### Siguiente Fase Recomendada

Fase 3.5.3 - `/catalogo` público consume `GET /api/catalog/public` con manejo de error/fallback de transición.

## 2026-07-05 - Fase 3.5.2 UI Admin Catálogo

### Cambio Realizado

Se implementó la UI privada de administración de catálogo/precios bajo `/app/admin/catalogo`, consumiendo endpoints admin existentes y manteniendo `/catalogo` público con `catalog-data.ts`.

### Frontend

- Ruta privada nueva: `/app/admin/catalogo`.
- `permissionGuard` usa `catalog.view` porque el guard actual soporta un permiso por ruta.
- Navegación privada `Catálogo` visible con `catalog.view` o `catalog.manage`.
- Modelos frontend `CatalogSection`, `CatalogProduct` y DTOs de create/update/status/price.
- Servicio `AdminCatalogService` para `/api/admin/catalog/sections` y `/api/admin/catalog/products`.
- Página standalone `AdminCatalogPageComponent` con resumen, estados loading/error/empty/success y layout mobile-first.
- Listado, creación, edición y activación/desactivación de secciones.
- Listado, filtros por sección/estado, creación, edición y activación/desactivación de productos.
- Actualización rápida de precio vía `PATCH /api/admin/catalog/products/{id}/price`.
- Validación UI de nombre requerido, sección requerida y precio no negativo.
- Modo solo lectura para `catalog.view` sin `catalog.manage`.
- Allowlist `catalog-image-options.ts` con assets `.webp` existentes bajo `assets/catalog/products`.
- Preview de imagen cuando existe `imagePath` y opción para limpiar imagen.
- Rutas heredadas `provisionales-yacket-*` y `protesis-removible-unidad-metalica..webp` quedan marcadas como observación visual sin renombrar assets.

### Documentación Actualizada

- `README.md`
- `docs/README.md`
- `docs/PROJECT_STATUS.md`
- `docs/ROADMAP.md`
- `docs/IMPLEMENTATION_LOG.md`
- `docs/01-product/admin-catalog-management.md`
- `docs/01-product/catalog-admin-design.md`
- `docs/01-product/public-website.md`
- `docs/03-architecture/ARCHITECTURE.md`
- `docs/03-architecture/AUTH_FLOW.md`
- `docs/08-qa/catalog-api-qa.md`

### Documentación Creada

- `docs/08-qa/catalog-admin-ui-qa.md`

### Exclusiones Confirmadas

- No se modificó `/catalogo` público.
- No se implementó upload de imágenes.
- No se crearon migraciones.
- No se cambió backend.
- No se tocó `AuthService`.
- No se modificaron guards.
- No se tocaron cookies ni XSRF.
- No se tocó deploy.
- No se instalaron dependencias.
- No se usó `codex-cobranza-sql`.
- No se imprimieron secretos.
- No se ejecutó `dotnet user-secrets list`.
- No se hizo commit.

### Validaciones Ejecutadas

- `npm run build` desde `src/LaboratorioTlahuac.Web`: correcto; initial total `317.27 kB`, sin warning de budget.
- `dotnet build`: correcto con 0 errores y 2 warnings `NU1903` conocidos por `SQLitePCLRaw.lib.e_sqlite3` 2.1.11 en tests.
- Primer `dotnet test` en paralelo con `dotnet build`: falló por bloqueo temporal de archivo en `obj`, patrón conocido del repo.
- `dotnet test` en serial: correcto; Domain 1/1, Application 1/1 y API 140/140.
- `git diff --check`: correcto.
- Búsquedas obligatorias: ejecutadas; los patrones sensibles se revisaron con salida limitada a nombres de archivo para no imprimir secretos.

### Siguiente Fase Recomendada

Fase 3.5.3 - `/catalogo` público consume `GET /api/catalog/public` con manejo de error/fallback de transición.

## 2026-07-05 - Fase 3.5.1 Backend Catálogo Administrable

### Cierre QA DEV

- Working tree local previo reportado por responsable del proyecto: limpio.
- Commit desplegado: `ebcf6e54b77ec6c5afaafdf8c21afc77213bf9d8`.
- GitHub Actions: `success`.
- `GET /health`: `200`.
- `GET /api/catalog/public` sin sesión: `200`.
- `/api/catalog/public` devuelve secciones, productos, precios MXN e `imagePath`.
- `/catalogo` público en DEV: `200`.
- `GET /api/admin/catalog/sections` sin sesión: `401`.
- `GET /api/admin/catalog/products` sin sesión: `401`.
- Observación no bloqueante: validar visualmente rutas de imágenes con nombres `yacket` y doble punto durante Fase 3.5.2.
- Resultado: Fase 3.5.1 cerrada en DEV sin bug claro.
- Validación de cierre documental: `dotnet build` correcto con 0 errores y 2 warnings `NU1903` conocidos; `dotnet test` correcto con Domain 1/1, Application 1/1 y API 140/140; `npm run build` correcto con initial total `314.59 kB` sin warning de budget; `git diff --check` correcto.

### Cambio Realizado

Se implementó backend/base de datos para catálogo administrable, sin cambiar `/catalogo` público ni crear UI admin.

### Backend

- Entidades de dominio `CatalogSection` y `CatalogProduct`.
- Contratos Application para catálogo público/admin.
- Servicio `CatalogService` en Infrastructure.
- Seeder `CatalogSeeder` con `CatalogSeed:RunOnStartup=true`.
- Endpoint público `GET /api/catalog/public`.
- Endpoints admin bajo `/api/admin/catalog/sections` y `/api/admin/catalog/products`.
- Validación de precio no negativo.
- Validación de `ImagePath` como ruta relativa segura bajo `assets/catalog/products/`.

### Migración

- Migración creada: `20260705054221_AddCatalogManagement`.
- Crea `CatalogSections` y `CatalogProducts`.
- Agrega FK `CatalogProducts.CatalogSectionId`.
- Agrega índices únicos por `Key` e índices por `SortOrder` e `IsActive`.
- No toca tablas ajenas.
- No elimina datos.
- No se aplicó migración en VPS.

### Permisos

- Permisos agregados: `catalog.view` y `catalog.manage`.
- Admin recibe ambos permisos por `Permissions.All` y baseline.
- `Repartidor` no recibe permisos de catálogo.
- `GET` admin requiere `catalog.view` o `catalog.manage`.
- Mutaciones admin requieren `catalog.manage`.

### Seed Inicial

- Fuente: `src/LaboratorioTlahuac.Web/src/app/public/data/catalog-data.ts`.
- Seed inicial: 12 secciones y 40 productos.
- Idempotente por `Key`.
- No sobreescribe precios, nombres, orden ni estado de registros existentes.
- Rellena solo `ImagePath`/`AltText` ausentes en registros existentes.
- No depende del filesystem.
- No copia archivos de imagen.

### Pruebas

- Se agregó `tests/LaboratorioTlahuac.Api.Tests/CatalogIntegrationTests.cs`.
- Cobertura pública: 200 sin autenticación, activos únicamente, orden y no exposición de campos administrativos.
- Cobertura admin: 401 sin sesión, 403 sin permiso, listar/crear/actualizar/activar/desactivar secciones y productos, actualizar precio, rechazo de precio negativo, rechazo de `ImagePath` inseguro y rechazo para `Repartidor`.
- Se actualizaron pruebas de permisos y seeder.

### Documentación Actualizada

- `README.md`
- `docs/README.md`
- `docs/PROJECT_STATUS.md`
- `docs/ROADMAP.md`
- `docs/IMPLEMENTATION_LOG.md`
- `docs/01-product/catalog-admin-design.md`
- `docs/01-product/admin-catalog-management.md`
- `docs/01-product/public-website.md`
- `docs/03-architecture/ARCHITECTURE.md`
- `docs/03-architecture/AUTH_FLOW.md`
- `docs/08-qa/catalog-api-qa.md`

### Exclusiones Confirmadas

- No se modificó `/catalogo` para consumir API.
- No se creó UI admin.
- No se implementó upload de imágenes.
- No se borró `catalog-data.ts`.
- No se movieron assets.
- No se cambiaron rutas públicas.
- No se tocó `AuthService`, guards, cookies ni XSRF.
- No se tocó deploy.
- No se instalaron dependencias.
- No se usó `codex-cobranza-sql`.
- No se imprimieron secretos.
- No se ejecutó `dotnet user-secrets list`.
- No se hizo commit.

### Validaciones Ejecutadas

- `dotnet build`: correcto con warnings conocidos `NU1903` por `SQLitePCLRaw.lib.e_sqlite3` 2.1.11 en tests.
- `dotnet test`: correcto; Domain 1/1, Application 1/1 y API 140/140.
- `npm run build` desde `src/LaboratorioTlahuac.Web`: correcto; initial total `314.59 kB`, sin warning de budget.
- `git diff --check`: correcto.
- Búsquedas obligatorias ejecutadas para `catalog.view`, `catalog.manage`, `/api/catalog/public`, `/api/admin/catalog`, `CatalogSection`, `CatalogProduct`, `catalog-data`, `assets/catalog`, `/catalogo`, `/dashboard`, `/app/dashboard`, `/login`, variables sensibles, `ConnectionStrings` y `codex-cobranza-sql`.
- Las búsquedas de patrones sensibles se limitaron a archivos para no imprimir valores.

### Siguiente Fase Recomendada

Fase 3.5.2 - UI admin catálogo/precios con selección de imagen existente.

## 2026-07-05 - Fase 3.5.0 Diseño Técnico Catálogo Administrable

### Cambio Realizado

Se documentó el diseño técnico para administrar catálogo, precios e imágenes desde `/app`. La fase fue solo análisis y documentación.

### Documentación Creada

- `docs/01-product/catalog-admin-design.md`

### Documentación Actualizada

- `docs/01-product/admin-catalog-management.md`
- `docs/01-product/public-website.md`
- `docs/PROJECT_STATUS.md`
- `docs/ROADMAP.md`
- `docs/README.md`
- `docs/03-architecture/ARCHITECTURE.md`
- `docs/IMPLEMENTATION_LOG.md`

### Inventario Confirmado

- `/catalogo` público sigue usando `src/LaboratorioTlahuac.Web/src/app/public/data/catalog-data.ts`.
- Catálogo actual: 12 secciones y 40 productos.
- Productos con imagen específica: 19.
- Productos con imagen representativa de sección: 16.
- Productos con placeholder visual: 5.
- Todas las imágenes referenciadas por `catalog-data.ts` existen en `src/LaboratorioTlahuac.Web/src/assets/catalog/products/`.
- Assets no referenciados detectados: `metal-porcelana-corona-sing-ivoclar-1.webp` y `protesis-removible-unidad-acrilica.jpg`.
- No existe backend de catálogo, endpoints de catálogo, administración de precios/imágenes ni carga de imágenes desde `/app`.

### Diseño Propuesto

- Modelo MVP: `CatalogSection` y `CatalogProduct`.
- Campos clave: `Key`/`Slug` estable, `Price decimal(18,2)`, `IsActive`, `SortOrder`, `Description`, `ImagePath`, `CreatedAtUtc`, `UpdatedAtUtc` y `UpdatedByUserId`.
- Permisos propuestos: `catalog.view`, `catalog.manage` y `catalog.publish` opcional.
- Admin debe recibir permisos de catálogo por baseline de `Permissions.All`.
- `Repartidor` no debe recibir permisos `catalog.*`.
- Endpoint público propuesto: `GET /api/catalog/public`, sin auth y solo con secciones/productos activos.
- Endpoints admin propuestos bajo `/api/admin/catalog`.
- Ruta privada futura recomendada: `/app/admin/catalogo`.

### Estrategia De Imágenes

Recomendación MVP: mantener assets estáticos existentes y permitir seleccionar `ImagePath` desde una allowlist. La carga/reemplazo de imágenes desde admin queda diferida a Fase 3.5.4, con política de almacenamiento, validación de tipo/peso/dimensiones y backup.

### Fases Propuestas

- Fase 3.5.1: backend catálogo administrable + migración + seed inicial desde `catalog-data.ts`.
- Fase 3.5.2: UI admin de catálogo/precios con selección de imagen existente.
- Fase 3.5.3: `/catalogo` público consume API con manejo de error/fallback de transición.
- Fase 3.5.4: carga/reemplazo de imágenes desde admin.

### Exclusiones Confirmadas

- No se modificó código funcional.
- No se crearon migraciones.
- No se tocó backend funcional.
- No se tocó frontend funcional.
- No se instalaron dependencias.
- No se tocó deploy.
- No se usó `codex-cobranza-sql`.
- No se imprimieron secretos.
- No se ejecutó `dotnet user-secrets list`.
- No se hizo commit.

### Validaciones Ejecutadas

- `npm run build` desde `src/LaboratorioTlahuac.Web`: correcto; initial total `314.59 kB`, sin warning de budget.
- `dotnet build`: correcto; 0 errores y 2 warnings `NU1903` conocidos por `SQLitePCLRaw.lib.e_sqlite3` 2.1.11 en tests.
- `dotnet test`: correcto; Domain 1/1, Application 1/1 y API 129/129.
- `git diff --check`: correcto.
- Búsquedas obligatorias ejecutadas: `catalog-data`, `/catalogo`, `catalog.manage`, `Catalog`, `assets/catalog`, `/dashboard`, `/app/dashboard` y `/login`.

### Siguiente Fase Recomendada

Fase 3.5.1 - backend catálogo administrable + migración + seed inicial.

## 2026-07-05 - QA DEV Fase 3.4.4 Pulido UX Operativo De Entregas

### Cambio Realizado

Se documentó el cierre de QA DEV de Fase 3.4.4 para el pulido UX operativo mobile-first de entregas. No se modificó código.

### Deploy Validado

- GitHub Actions: `success`.
- `/health`: `200`.
- `/api/deliveries` sin sesión: `401`.

### Resultados Repartidor

- Login `Repartidor`: OK.
- `/app/entregas` carga: OK.
- Filtros de estado: OK.
- Contadores: OK.
- Cards mobile-first: OK.
- Detalle de entrega: OK.
- Acciones contextuales: OK.
- Reintentar entrega: OK.
- Marcar entregada: OK.
- Marcar no entregada: OK.
- `tel:` aparece solo si hay teléfono: OK.
- WhatsApp aparece solo si existe dato: OK.
- `Abrir mapa` aparece solo si hay dirección: OK.
- Logout: OK.

### Observaciones

- Sin observaciones reportadas.
- Sin bug claro que requiriera modificar código.
- No se tocaron backend, migraciones, `AuthService`, guards, cookies, XSRF, deploy ni dependencias.
- No se imprimieron secretos.
- No se usó `codex-cobranza-sql`.

### Documentación Actualizada

- `docs/08-qa/driver-mobile-qa.md`
- `docs/PROJECT_STATUS.md`
- `docs/ROADMAP.md`
- `docs/IMPLEMENTATION_LOG.md`
- `docs/01-product/driver-mobile-workflow.md`
- `docs/01-product/operations-orders-delivery.md`

### Validaciones Ejecutadas

- `npm run build` desde `src/LaboratorioTlahuac.Web`: correcto; initial total `314.59 kB`, sin warning de budget.
- `dotnet build`: correcto; 0 errores y 2 warnings `NU1903` conocidos por `SQLitePCLRaw.lib.e_sqlite3` 2.1.11 en tests.
- `dotnet test`: correcto; Domain 1/1, Application 1/1 y API 129/129.
- `git diff --check`: correcto.

### Siguiente Fase Recomendada

Fase 3.5 - administración de catálogo/precios/imágenes.

## 2026-07-05 - Fase 3.4.4 Pulido UX Operativo De Entregas

### Cambio Realizado

Se implementó pulido mobile-first para el flujo operativo de repartidor en `/app/entregas` y `/app/entregas/:id`, sin cambiar backend, modelo, migraciones, rutas privadas, permisos, cookies ni XSRF.

### Frontend Repartidor

- `/app/entregas` agrega filtros simples por estado: `Todas`, `En ruta`, `Asignadas`, `No entregadas` y `Entregadas`.
- El listado muestra resumen con contadores de `Asignadas`, `En ruta`, `No entregadas` y `Entregadas`.
- Las cards mobile-first destacan folio, cliente, fecha de entrega, estado, paciente/referencia, trabajo, dirección y contacto solo cuando existen.
- El botón principal de cada card queda más claro para abrir o registrar la entrega según el estado.
- `/app/entregas/:id` reorganiza la jerarquía visual con bloque principal de cliente/folio/fecha/estado, datos de ruta/contacto, datos de entrega y seguimiento.
- El detalle muestra `Llamar` con `tel:` solo si existe teléfono.
- El detalle muestra `WhatsApp` clicable solo si existe dato de WhatsApp.
- El detalle muestra `Abrir mapa` con Google Maps solo si existe dirección.
- La dirección se mantiene como texto visible y no se inventa mapa cuando no hay dirección.
- Las acciones quedan agrupadas como acción operativa contextual: `Reintentar entrega`, `Marcar entregada` o `Marcar no entregada`, según el estado permitido.

### Exclusiones Confirmadas

- No se tocó backend.
- No se crearon migraciones.
- No se instalaron dependencias.
- No se tocó `AuthService`.
- No se tocaron `auth.guard.ts` ni `permission.guard.ts`.
- No se tocaron cookies ni XSRF.
- No se relajaron permisos.
- No se cambiaron rutas privadas.
- No se convirtió `/dashboard` en ruta privada real.
- No se usó `codex-cobranza-sql`.
- No se imprimieron secretos.
- No se ejecutó `dotnet user-secrets list`.
- No se hizo commit.

### Validaciones Ejecutadas

- `npm run build` desde `src/LaboratorioTlahuac.Web`: correcto; initial total `314.59 kB`, sin warning de budget.
- `dotnet build`: correcto; 0 errores y 2 warnings `NU1903` conocidos por `SQLitePCLRaw.lib.e_sqlite3` 2.1.11 en tests.
- `dotnet test`: correcto; Domain 1/1, Application 1/1 y API 129/129.
- `git diff --check`: correcto.
- Búsquedas obligatorias ejecutadas para rutas, permisos, `Reintentar entrega`, `tel:`, `maps`, `/dashboard`, `/app/dashboard`, `/login`, variables sensibles, `ConnectionStrings` y `codex-cobranza-sql`.

### Archivos Modificados

- `src/LaboratorioTlahuac.Web/src/app/features/deliveries/pages/delivery-list-page.component.ts`
- `src/LaboratorioTlahuac.Web/src/app/features/deliveries/pages/delivery-detail-page.component.ts`
- `docs/08-qa/driver-mobile-qa.md`
- `docs/PROJECT_STATUS.md`
- `docs/ROADMAP.md`
- `docs/IMPLEMENTATION_LOG.md`
- `docs/01-product/driver-mobile-workflow.md`
- `docs/01-product/operations-orders-delivery.md`

### Siguiente Fase Recomendada

Fase 3.5 - administración de catálogo/precios/imágenes.

## 2026-07-05 - QA DEV Fase 3.4.3.1 Redirect Y Reintento

### Cambio Realizado

Se documentó el cierre de QA DEV de Fase 3.4.3.1 para redirect post-login por permisos y reintento de entrega fallida. No se modificó código.

### Deploy Validado

- Commit desplegado: `59542efd4f57df7ba04a2444c5496040810d1702`.
- GitHub Actions: `success`.
- `/health`: `200`.
- `/api/deliveries` sin sesión: `401`.

### Resultados Repartidor

- Login `Repartidor` sin `returnUrl` redirige a `/app/entregas`: OK.
- `/app/entregas` carga: OK.
- `/app/dashboard` redirige a `/app/access-denied`: OK.
- `/app/access-denied` muestra `Ir a mi inicio`: OK.
- `Ir a mi inicio` lleva a `/app/entregas`: OK.
- Entrega `FailedDelivery` muestra `Reintentar entrega`: OK.
- `Reintentar entrega` cambia a `En ruta`: OK.
- Después de reintentar permite marcar `Entregada`: OK.
- Validación `recipientName` vacío: OK.
- Logout: OK.

### Resultados Admin

- Login Admin lleva a `/app/dashboard`: OK.
- `/app/ordenes` carga: OK.
- Grid muestra `Estado` de orden y `Entrega`: OK.
- Detalle de orden muestra sección `Entrega`: OK.
- Admin puede reintentar entrega fallida: OK.
- Reintentar no cambia `WorkOrder.Status`: OK.
- Grid se actualiza después de cambios de entrega: OK.

### Observaciones

- Sin observaciones reportadas.
- Sin bug claro que requiriera modificar código.
- No se tocaron backend, migraciones, `AuthService`, guards, cookies, XSRF, deploy ni dependencias.
- No se imprimieron secretos.
- No se usó `codex-cobranza-sql`.

### Documentación Actualizada

- `docs/08-qa/driver-mobile-qa.md`
- `docs/08-qa/delivery-admin-ui-qa.md`
- `docs/08-qa/delivery-api-qa.md`
- `docs/PROJECT_STATUS.md`
- `docs/ROADMAP.md`
- `docs/IMPLEMENTATION_LOG.md`
- `docs/01-product/driver-mobile-workflow.md`
- `docs/01-product/operations-orders-delivery.md`
- `docs/03-architecture/ARCHITECTURE.md`
- `docs/03-architecture/AUTH_FLOW.md`

### Validaciones Ejecutadas

- `npm run build` desde `src/LaboratorioTlahuac.Web`: correcto; initial total `314.59 kB`, sin warning de budget.
- `dotnet build`: correcto; 0 errores y 2 warnings `NU1903` conocidos por `SQLitePCLRaw.lib.e_sqlite3` 2.1.11 en tests.
- `dotnet test`: correcto; Domain 1/1, Application 1/1 y API 129/129.
- `git diff --check`: correcto.

### Siguiente Fase Recomendada

Fase 3.4.4 - pulido UX operativo de entregas, o Fase 3.5 - administración de catálogo/precios/imágenes.

## 2026-07-05 - Fase 3.4.3.1 Redirect Por Permisos Y Reintento De Entrega

### Cambio Realizado

Se corrigió el destino post-login cuando no existe `returnUrl` interno válido explícito y se agregó reintento para entregas en `FailedDelivery`.

### Frontend Auth

- `AuthService` agrega helper de ruta inicial por permisos.
- Prioridad implementada: `reports.view`, `deliveries.view`, `orders.view`, `customers.view`, `payments.view`, `inventory.view`, `suppliers.view`, `users.manage`, `roles.manage`.
- `/login` conserva `returnUrl` interno válido.
- Si no hay `returnUrl` válido, el usuario entra a su ruta inicial por permisos.
- Un usuario `Repartidor` con `deliveries.view` y sin `reports.view` entra a `/app/entregas`.
- `returnUrl` externo, protocol-relative, con esquema, backslash, espacios o fuera de `/app` sigue bloqueado.
- `/app/access-denied` usa la misma ruta inicial por permisos y muestra `Ir a mi inicio`.
- El enlace de marca del layout privado apunta también a la ruta inicial por permisos.

### Backend Delivery

- `WorkOrderDelivery` agrega transición `Retry`.
- Se agrega contrato `DeliveryRetryRequest`.
- Se agrega `IDeliveryService.RetryAsync`.
- Se agrega `PATCH /api/deliveries/{id}/retry`.
- Reintento permitido: `FailedDelivery -> OutForDelivery`.
- Reintento rechazado si la entrega no está en `FailedDelivery`.
- Reintento rechazado si la orden está `Cancelled`.
- Reintento mantiene `AssignedToUserId`.
- Reintento actualiza `OutForDeliveryAtUtc` con hora de servidor.
- Reintento no cambia `WorkOrder.Status`.
- Reintento limpia el estado fallido activo; si vuelve a fallar, `failedReason`/`failedAtUtc` representan el último intento fallido.

### Permisos

- Admin/operación puede reintentar con `deliveries.update`.
- Repartidor asignado puede reintentar su propia entrega fallida con `deliveries.complete`.
- Repartidor no asignado no puede reintentar entrega ajena.
- Usuario sin sesión recibe `401`.
- Usuario sin permisos recibe `403`.

### Frontend Entregas

- `DeliveryService` frontend agrega `retry`.
- La sección `Entrega` de `/app/ordenes/:id` muestra botón `Reintentar entrega` cuando la entrega está `FailedDelivery` y el usuario tiene `deliveries.update`.
- El detalle mobile `/app/entregas/:id` muestra botón `Reintentar entrega` cuando la entrega propia está `FailedDelivery` y el usuario tiene `deliveries.complete`.
- Ambas UIs muestran el texto `La entrega volverá a marcarse como En ruta.`.
- Ambas UIs deshabilitan botones durante la petición y muestran `No se pudo reintentar la entrega.` ante error de retry.
- Después de retry, la entrega se refresca y queda como `OutForDelivery`.

### Pruebas

- `DeliveryIntegrationTests` cubre:
  - Admin puede reintentar entrega fallida.
  - Repartidor asignado puede reintentar su entrega fallida.
  - Repartidor no asignado no puede reintentar entrega ajena.
  - Reintentar entrega no fallida devuelve `400`.
  - Reintentar entrega de orden cancelada devuelve `409`.
  - Reintento deja `status = OutForDelivery`.
  - Entrega reintentada puede cerrarse como `Delivered` con `recipientName`.
  - Reintento no cambia `WorkOrder.Status`.
  - Sin sesión devuelve `401`.
  - Sin permiso devuelve `403`.

### Documentación

- Se documenta que `Repartidor` ahora redirige a `/app/entregas` después del login cuando no hay `returnUrl` explícito.
- Se documenta que `/app/access-denied` usa ruta inicial por permisos.
- Se documenta que `FailedDelivery` puede reintentarse.
- Se documenta que no hay historial completo de intentos todavía.
- Se documenta que `WorkOrder.Status` no se mezcla con `DeliveryStatus`.
- Se documentan criterios de validación DEV para redirect/reintento.

### Exclusiones Confirmadas

- No se tocaron cookies.
- No se tocó XSRF.
- No se relajaron guards.
- No se creó migración.
- No se instalaron dependencias.
- No se tocó deploy.
- No se usó `codex-cobranza-sql`.
- No se imprimieron secretos.
- No se ejecutó `dotnet user-secrets list`.
- No se hizo commit.

### Validaciones Ejecutadas

- `dotnet build`: correcto; 0 errores y 2 warnings `NU1903` conocidos por `SQLitePCLRaw.lib.e_sqlite3` 2.1.11 en tests.
- `dotnet test`: correcto; Domain 1/1, Application 1/1 y API 129/129.
- `npm run build` desde `src/LaboratorioTlahuac.Web`: correcto; initial total `314.59 kB`.
- `git diff --check`: correcto.
- Búsquedas obligatorias ejecutadas para `retry`, `Reintentar entrega`, `FailedDelivery`, `deliveries.complete`, `deliveries.update`, `/app/entregas`, `/app/access-denied`, `/dashboard`, `/app/dashboard`, `/login`, variables sensibles, `ConnectionStrings` y `codex-cobranza-sql`.
- Las búsquedas de patrones sensibles se ejecutaron con salida limitada a archivos para no imprimir valores.

### Archivos Modificados

- `src/LaboratorioTlahuac.Api/Endpoints/DeliveryEndpoints.cs`
- `src/LaboratorioTlahuac.Application/Deliveries/DeliveryContracts.cs`
- `src/LaboratorioTlahuac.Application/Deliveries/IDeliveryService.cs`
- `src/LaboratorioTlahuac.Domain/Deliveries/Entities/WorkOrderDelivery.cs`
- `src/LaboratorioTlahuac.Infrastructure/Deliveries/DeliveryService.cs`
- `tests/LaboratorioTlahuac.Api.Tests/DeliveryIntegrationTests.cs`
- `src/LaboratorioTlahuac.Web/src/app/core/auth/auth.service.ts`
- `src/LaboratorioTlahuac.Web/src/app/auth/pages/login/login-page.component.ts`
- `src/LaboratorioTlahuac.Web/src/app/admin/pages/access-denied/access-denied-page.component.ts`
- `src/LaboratorioTlahuac.Web/src/app/admin/layout/private-layout.component.ts`
- `src/LaboratorioTlahuac.Web/src/app/features/deliveries/delivery.models.ts`
- `src/LaboratorioTlahuac.Web/src/app/features/deliveries/delivery.service.ts`
- `src/LaboratorioTlahuac.Web/src/app/features/deliveries/components/delivery-admin-section.component.ts`
- `src/LaboratorioTlahuac.Web/src/app/features/deliveries/pages/delivery-detail-page.component.ts`
- `README.md`
- `docs/README.md`
- `docs/PROJECT_STATUS.md`
- `docs/ROADMAP.md`
- `docs/IMPLEMENTATION_LOG.md`
- `docs/01-product/driver-mobile-workflow.md`
- `docs/01-product/delivery-mvp-design.md`
- `docs/01-product/operations-orders-delivery.md`
- `docs/03-architecture/ARCHITECTURE.md`
- `docs/03-architecture/AUTH_FLOW.md`
- `docs/08-qa/driver-mobile-qa.md`
- `docs/08-qa/delivery-admin-ui-qa.md`
- `docs/08-qa/delivery-api-qa.md`

### Siguiente Fase Recomendada

QA DEV de redirect/reintento con Admin y usuario `Repartidor`; cerrado posteriormente en la entrada `2026-07-05 - QA DEV Fase 3.4.3.1 Redirect Y Reintento`. Después, Fase 3.4.4 pulido UX operativo de entregas o Fase 3.5 administración de catálogo/precios/imágenes.

## 2026-07-04 - Corrección ARIA Del Carrusel Del Catálogo Público

### Cambio Realizado

Se corrigió la semántica accesible del carrusel visual de secciones en `/catalogo` para dejar de representar el selector como tabs cuando solo existe en el DOM el contenido de la sección activa.

### Frontend

- El selector de secciones usa ahora `nav aria-label="Secciones del catálogo"` con botones nativos.
- La sección activa se marca con `aria-current="true"` y conserva el resaltado visual existente.
- Se eliminaron `role="tablist"`, `role="tab"`, `role="tabpanel"`, `aria-selected` y `aria-controls` del carrusel/galería.
- Se retiró el manejador de teclado específico de tabs; los botones conservan navegación nativa por teclado y las flechas anterior/siguiente mantienen sus `aria-label`.
- Se conservaron click de selección, autoplay, pausa por hover/focus/interacción, `prefers-reduced-motion`, galería inferior, productos, precios y assets.

### Exclusiones Confirmadas

- No se modificó backend.
- No se modificó base de datos.
- No se crearon migraciones.
- No se agregaron dependencias.
- No se modificaron `package.json` ni `package-lock.json`.
- No se cambiaron productos, precios ni datos del catálogo.
- No se hizo commit ni deploy.

### Validaciones Ejecutadas

- `rg -n "role=\"tab|tablist|tabpanel|aria-controls|aria-selected" src/LaboratorioTlahuac.Web/src/app/public/pages/catalog/catalog-page.component.ts`: sin coincidencias.
- `git diff --check`: correcto.
- `npm run build` desde `src/LaboratorioTlahuac.Web`: correcto; initial total `313.95 kB`.
- `dotnet build`: correcto; 0 errores y 2 warnings `NU1903` conocidos por `SQLitePCLRaw.lib.e_sqlite3` 2.1.11 en tests.
- `dotnet test`: correcto; Domain 1/1, Application 1/1 y API 124/124.
- Diffs de `package.json` y `package-lock.json`: sin cambios.

### Archivos Modificados

- `src/LaboratorioTlahuac.Web/src/app/public/pages/catalog/catalog-page.component.ts`
- `docs/PROJECT_STATUS.md`
- `docs/IMPLEMENTATION_LOG.md`

## 2026-07-04 - Rediseño Visual Del Catálogo Público

### Cambio Realizado

Se rediseñó `/catalogo` para reemplazar el selector horizontal tipo chips por un explorador visual con carrusel custom de secciones y galería de imágenes por sección.

### Frontend

- Se implementó el estado del catálogo con Angular signals para secciones, sección activa, imagen activa, productos visibles, pausa de autoplay e imágenes faltantes.
- El carrusel de secciones muestra tarjetas compactas con miniatura, nombre destacado y conteo de productos.
- El carrusel oculta scrollbar visible, conserva scroll interno touch, agrega flechas discretas con `aria-label` y soporta navegación por botones/teclado.
- El autoplay avanza cada 4 segundos, reinicia la galería al índice 0 al cambiar de sección y se pausa con hover, focus, interacción manual y `prefers-reduced-motion`.
- La galería inferior muestra una imagen central estable con miniaturas anterior/siguiente cuando existen.
- Las imágenes se resuelven desde `assets/catalog/products`; si una sección no tiene imagen propia usa una imagen de producto y, si tampoco existe, muestra fallback visual.
- Los productos y precios se siguen leyendo de `src/LaboratorioTlahuac.Web/src/app/public/data/catalog-data.ts` sin cambiar nombres técnicos ni precios.
- Los estilos del explorador se agregaron a `src/LaboratorioTlahuac.Web/src/styles.scss` bajo `.catalog-page` para evitar warning de presupuesto del SCSS del componente.

### Exclusiones Confirmadas

- No se modificó backend.
- No se modificó base de datos.
- No se crearon migraciones.
- No se cambiaron contratos API.
- No se agregaron dependencias npm.
- No se modificaron `package.json` ni `package-lock.json`.
- No se tocaron `/login`, `/app`, auth, guards, cookies, XSRF ni deploy.
- No se hizo commit.

### Validaciones Ejecutadas

- `npm run build` desde `src/LaboratorioTlahuac.Web`: correcto; initial total `313.95 kB`, sin warning de budget.
- `dotnet build`: correcto; 0 errores y 2 warnings `NU1903` conocidos por `SQLitePCLRaw.lib.e_sqlite3` 2.1.11 en tests.
- `dotnet test`: correcto; Domain 1/1, Application 1/1 y API 124/124.
- `curl http://127.0.0.1:4200/catalogo`: `200` con dev server local.
- Capturas Playwright móvil y desktop de `/catalogo`: generadas correctamente.
- Pase manual interactivo completo: pendiente.

### Archivos Modificados

- `src/LaboratorioTlahuac.Web/src/app/public/pages/catalog/catalog-page.component.ts`
- `src/LaboratorioTlahuac.Web/src/styles.scss`
- `docs/PROJECT_STATUS.md`
- `docs/IMPLEMENTATION_LOG.md`

### Siguiente Mejora Recomendada

Agregar swipe avanzado con cálculo de inercia y una prueba visual automatizada de `/catalogo` cuando haya navegador/headless disponible.

## 2026-07-04 - Fase 3.4.3 UI Repartidor Mobile-First

### Cambio Realizado

Se implementó la UI mobile-first del repartidor bajo `/app/entregas`.

### Frontend

- Se creó `/app/entregas` como ruta privada protegida con `deliveries.view`.
- Se creó `/app/entregas/:id` como ruta privada protegida con `deliveries.view`.
- Se agregó navegación privada `Entregas` visible solo con `deliveries.view`.
- El listado consume `DeliveryService.list({ assignedToMe: true })`.
- El listado muestra cards mobile-first con folio, cliente, paciente/referencia, trabajo, estado de entrega, fecha de entrega, dirección/contacto si existen y acción `Ver detalle`.
- El detalle consume `GET /api/deliveries/{id}` mediante `DeliveryService.getById`.
- El detalle valida que `assignedToUserId` coincida con el usuario autenticado antes de mostrar datos de la entrega.
- El detalle muestra cliente, dirección, contacto, folio, paciente, referencia, trabajo, fecha de entrega, estado de orden, seguimiento, recibido y motivo de no entrega.
- Con `deliveries.complete`, permite marcar entregada con `recipientName` requerido cuando la entrega está `OutForDelivery`.
- Con `deliveries.complete`, permite marcar no entregada con `failedReason` requerido cuando la entrega está `Assigned` u `OutForDelivery`.
- Si falta `deliveries.complete`, la pantalla queda en lectura sin formularios de cierre.
- Después de cada cierre, refresca el detalle.
- Loading, estado vacío y errores `400`, `403`, `404` y `409` se muestran de forma controlada.

### Exclusiones Confirmadas

- No se permite asignar ni cambiar repartidor desde la UI del repartidor.
- No se muestran pagos, saldos ni información financiera.
- No se crearon endpoints nuevos.
- No se modificó backend.
- No se crearon migraciones.
- No se tocó `AuthService`, guards, cookies, XSRF ni deploy.
- No se instalaron dependencias.
- No se usó `codex-cobranza-sql`.
- No se imprimieron secretos.
- No se hizo commit.

### Documentación

- Se creó `docs/08-qa/driver-mobile-qa.md`.
- Se actualizaron estado, roadmap, bitácora, documentación funcional, arquitectura/auth, README e índice de docs.
- `docs/08-qa/delivery-admin-ui-qa.md` queda como QA de la UI admin; el QA de repartidor queda separado.

### Validaciones Ejecutadas

- `npm run build` desde `src/LaboratorioTlahuac.Web`: correcto; initial total `305.66 kB`, sin warning de budget.
- `dotnet build`: correcto; 0 errores y 2 warnings `NU1903` conocidos por `SQLitePCLRaw.lib.e_sqlite3` 2.1.11 en tests.
- `dotnet test`: correcto; Domain 1/1, Application 1/1 y API 124/124.
- `git diff --check`: correcto.
- Búsquedas obligatorias ejecutadas: `/app/entregas`, `assignedToMe`, `deliveries.view`, `deliveries.complete`, `/dashboard`, `/app/dashboard` y `/login`.
- Las búsquedas de rutas confirman que `/app/dashboard` sigue siendo la ruta privada real del dashboard, `/dashboard` no se convirtió en ruta privada real y `/login` sigue como entrada pública.

### Archivos Modificados

- `src/LaboratorioTlahuac.Web/src/app/app.routes.ts`
- `src/LaboratorioTlahuac.Web/src/app/admin/layout/private-layout.component.ts`
- `src/LaboratorioTlahuac.Web/src/app/features/deliveries/pages/delivery-list-page.component.ts`
- `src/LaboratorioTlahuac.Web/src/app/features/deliveries/pages/delivery-detail-page.component.ts`
- `README.md`
- `docs/README.md`
- `docs/PROJECT_STATUS.md`
- `docs/ROADMAP.md`
- `docs/IMPLEMENTATION_LOG.md`
- `docs/01-product/driver-mobile-workflow.md`
- `docs/01-product/delivery-mvp-design.md`
- `docs/01-product/operations-orders-delivery.md`
- `docs/01-product/internal-system.md`
- `docs/03-architecture/AUTH_FLOW.md`
- `docs/03-architecture/ARCHITECTURE.md`
- `docs/08-qa/delivery-admin-ui-qa.md`
- `docs/08-qa/driver-mobile-qa.md`

### Siguiente Fase Recomendada

Fase 3.4.3.1 - redirect/reintento, cerrada posteriormente con QA DEV; después Fase 3.4.4 pulido UX operativo de entregas o Fase 3.5 administración de catálogo/precios/imágenes.

## 2026-07-04 - Fase 3.4.2.1 Estado De Entrega En Listado De Órdenes

### Cambio Realizado

Se agregó estado logístico de entrega al listado existente `/app/ordenes`, sin cambiar la semántica de `WorkOrder.Status`.

### Backend

- `GET /api/work-orders` conserva el endpoint existente y agrega un resumen opcional `delivery` por item de listado.
- Campos agregados en el resumen:
  - `deliveryId`
  - `deliveryStatus`
  - `deliveryStatusLabel`
  - `assignedToUserName`
  - `deliveredAtUtc`
  - `failedAtUtc`
- La consulta pagina órdenes y proyecta la entrega asociada desde `WorkOrderDeliveries` sin crear endpoint nuevo.
- Orden sin entrega regresa `delivery: null`.
- `FailedDelivery` se etiqueta como `No entregada`.
- `WorkOrder.Status` no se cambia al marcar entrega fallida; `No entregada` es `DeliveryStatus`, no `WorkOrderStatus`.

### Frontend

- `WorkOrderListItem` agrega `delivery` opcional.
- `/app/ordenes` conserva `Estado` como estado operativo de orden.
- La fecha planeada se renombra visualmente a `Fecha entrega`.
- Se agrega badge/columna `Entrega` en tabla y cards móviles.
- Orden sin entrega muestra `Sin entrega`.
- Entrega fallida muestra badge claro `No entregada`.
- Se mantienen acciones `Ver` y `Editar`.

### Pruebas

- Se agregaron pruebas API para:
  - listado con entrega asociada;
  - `deliveryStatus = FailedDelivery` en listado;
  - preservación de `WorkOrder.Status` cuando falla la entrega;
  - orden sin entrega con `delivery: null`;
  - filtro de listado por `status=Received` con resumen de entrega.

### Exclusiones Confirmadas

- No se modificaron estados existentes de `WorkOrderStatus`.
- No se crearon migraciones.
- No se crearon endpoints nuevos.
- No se creó otro panel de órdenes.
- No se tocaron `/login`, `/app/dashboard`, `/dashboard`, `AuthService`, guards, cookies, XSRF ni deploy.
- No se instalaron dependencias.
- No se ejecutó `dotnet user-secrets list`.
- No se usó `codex-cobranza-sql`.
- No se hizo commit.

### Validaciones Ejecutadas

- `dotnet build`: correcto con 0 errores y 2 warnings `NU1903` conocidos por `SQLitePCLRaw.lib.e_sqlite3` 2.1.11 en tests.
- `dotnet test`: correcto; Domain 1/1, Application 1/1 y API 124/124.
- `npm run build` desde `src/LaboratorioTlahuac.Web`: correcto; initial total `305.34 kB`, sin warning de budget.
- `git diff --check`: correcto.
- Búsquedas obligatorias ejecutadas: `deliveryStatus`, `deliveryStatusLabel`, `No entregada`, `WorkOrderStatus`, `FailedDelivery`, `/app/ordenes`, `/dashboard`, `/app/dashboard`, `/login`, variables sensibles, `ConnectionStrings` y `codex-cobranza-sql`.
- Las búsquedas de `LT_ADMIN_PASSWORD`, `LT_QA_LIMITED_PASSWORD`, `LDT_SQL_SA_PASSWORD` y `ConnectionStrings` se ejecutaron con salida limitada a archivos para no imprimir valores.

### Archivos Modificados

- `src/LaboratorioTlahuac.Application/WorkOrders/WorkOrderContracts.cs`
- `src/LaboratorioTlahuac.Infrastructure/WorkOrders/WorkOrderService.cs`
- `tests/LaboratorioTlahuac.Api.Tests/WorkOrderIntegrationTests.cs`
- `src/LaboratorioTlahuac.Web/src/app/features/orders/work-order.models.ts`
- `src/LaboratorioTlahuac.Web/src/app/features/orders/components/work-order-delivery-status-badge.component.ts`
- `src/LaboratorioTlahuac.Web/src/app/features/orders/pages/work-order-list-page.component.ts`
- `src/LaboratorioTlahuac.Web/src/styles/_badges.scss`
- `src/LaboratorioTlahuac.Web/src/styles/_app-features.scss`
- `docs/PROJECT_STATUS.md`
- `docs/ROADMAP.md`
- `docs/IMPLEMENTATION_LOG.md`
- `docs/01-product/operations-orders-delivery.md`
- `docs/01-product/delivery-mvp-design.md`
- `docs/08-qa/delivery-admin-ui-qa.md`

### Siguiente Fase Recomendada

Fase 3.4.3 - UI repartidor mobile-first bajo `/app/entregas`.

## 2026-07-04 - Cierre Operativo DEV Fase 3.4.2

### Cambio Realizado

Se documentó el cierre operativo del despliegue DEV de Fase 3.4.2 y el ajuste manual de `backend/current`/restart. No se modificó código.

### Incidente De Deploy

- GitHub Actions para commit `97d46e9` falló durante health check con `502`.
- El rollback dejó activo `dev-23-eea8f39`.
- El release nuevo `dev-24-97d46e9` quedó copiado en el VPS.
- El primer intento manual de validación fue inválido porque se intentó sourcear `/etc/laboratorio-tlahuac-dev/api.env` en Bash y la connection string contiene espacios/semicolons.
- La carga correcta de `api.env` con parser seguro permitió validar que `dev-24-97d46e9` arrancaba correctamente.
- El release se validó manualmente en puerto alterno `5013`.
- Se cambió manualmente `backend/current` a `dev-24-97d46e9`.
- Se reinició `laboratorio-tlahuac-dev-api.service` y quedó `active`.

### Validación Final DEV

- `http://127.0.0.1:5012/health`: `200`.
- `http://127.0.0.1:5012/api/deliveries` sin sesión: `401`.
- `https://dev.laboratoriodentaltlahuac.com/health`: `200`.
- `https://dev.laboratoriodentaltlahuac.com/api/deliveries` sin sesión: `401`.

### Pendiente Técnico

- Ajustar el workflow de deploy DEV para esperar más o validar `/health` con reintentos más tolerantes después del restart, evitando falsos negativos `502` cuando el servicio arranca correctamente.

### Siguiente Fase Recomendada

- Ejecutar QA manual DEV de Fase 3.4.2.
- Después iniciar Fase 3.4.3 - UI repartidor mobile-first bajo `/app/entregas`.

### Validaciones Ejecutadas

- `npm run build`: correcto.
- `dotnet build`: correcto.
- `dotnet test`: correcto.
- `git diff --check`: correcto.

### Archivos Modificados

- `docs/05-delivery/dev-deployment-validation.md`
- `docs/08-qa/delivery-admin-ui-qa.md`
- `docs/08-qa/delivery-api-qa.md`
- `docs/PROJECT_STATUS.md`
- `docs/ROADMAP.md`
- `docs/IMPLEMENTATION_LOG.md`

### Confirmaciones

- Solo documentación modificada.
- No se modificó código.
- No se imprimieron secretos.
- No se usó `codex-cobranza-sql`.

## 2026-07-04 - Fase 3.4.2 UI Admin De Entregas Desde Órdenes

### Cambio Realizado

Se implementó la UI administrativa de entregas dentro del detalle de orden existente `/app/ordenes/:id`.

### Frontend

- Se agregó `DeliveryService` frontend para consumir:
  - `GET /api/work-orders/{workOrderId}/delivery`
  - `POST /api/work-orders/{workOrderId}/delivery`
  - `GET /api/deliveries`
  - `GET /api/deliveries/{id}`
  - `PATCH /api/deliveries/{id}/assign`
  - `PATCH /api/deliveries/{id}/out-for-delivery`
  - `PATCH /api/deliveries/{id}/complete`
  - `PATCH /api/deliveries/{id}/failed`
- Se agregaron modelos frontend de entrega.
- Se agregó sección `Entrega` en `/app/ordenes/:id` mediante componente standalone.
- La sección muestra estado vacío, estado logístico, repartidor, timestamps, `Recibió` y motivo de falla.
- La sección permite crear entrega, asignar repartidor, marcar salida, marcar entregada y marcar no entregada.
- Los botones se muestran solo según permisos:
  - `deliveries.assign`
  - `deliveries.update`
  - `deliveries.complete`
- La visualización de entrega requiere `deliveries.view`.
- Cada acción refresca la entrega y notifica al detalle de orden para refrescar estado/historial cuando el backend sincroniza la orden a `Delivered`.
- Los errores `400`, `403`, `404` y `409` se muestran como mensajes controlados.

### Repartidores

- Se reutilizan endpoints admin existentes para listar usuarios/roles.
- Se filtran candidatos por rol `Repartidor` cuando la información está disponible.
- Si no se puede filtrar de forma segura por rol, la UI muestra advertencia visual y selector controlado de usuarios activos.
- No se exponen passwords, `passwordHash` ni contraseñas temporales.
- El backend sigue validando que el usuario asignado esté activo y tenga `deliveries.view`.

### Exclusiones Confirmadas

- No se implementó la UI mobile-first del repartidor.
- No se creó `/app/entregas`.
- No se creó `/dashboard`.
- No se cambió `/login`.
- No se tocaron `AuthService`, `auth.guard.ts`, `permission.guard.ts`, cookies ni XSRF.
- No se modificó backend.
- No se crearon migraciones.
- No se crearon endpoints nuevos.
- No se tocó deploy.
- No se instalaron dependencias.
- No se ejecutó `dotnet user-secrets list`.
- No se usó `codex-cobranza-sql`.
- No se hizo commit.

### Documentación

- Se creó `docs/08-qa/delivery-admin-ui-qa.md`.
- Se actualizaron estado, roadmap, bitácora, fuentes funcionales de delivery y documentación de QA.

### Validaciones Ejecutadas

- `npm run build` desde `src/LaboratorioTlahuac.Web`: correcto; initial total `304.19 kB`, sin warning de budget.
- `dotnet build`: correcto; 0 errores y 2 warnings `NU1903` conocidos por `SQLitePCLRaw.lib.e_sqlite3` 2.1.11 en tests.
- `dotnet test`: correcto; Domain 1/1, Application 1/1 y API 121/121.
- `git diff --check`: correcto.
- Búsquedas obligatorias de `DeliveryService`, permisos `deliveries.*`, `/api/deliveries`, `/api/work-orders`, `/app/ordenes`, `/dashboard`, `/app/dashboard`, `/login`, variables sensibles, `ConnectionStrings` y `codex-cobranza-sql`: ejecutadas. Las búsquedas de patrones sensibles se limitaron a archivos para no imprimir valores.

### Pendientes

- Ejecutar validación manual Admin en DEV:
  - crear entrega;
  - asignar repartidor;
  - marcar salida;
  - marcar entregada con `Recibió`;
  - marcar no entregada con motivo;
  - validar errores de campos obligatorios y `403`.
- Siguiente fase: Fase 3.4.3 - UI repartidor mobile-first bajo `/app/entregas`.

## 2026-07-04 - Cierre DEV Fase 3.4.1 Delivery Y Lazy Loading

### Cambio Realizado

Se documentó el cierre de despliegue DEV para Fase 3.4.1 backend delivery MVP y la optimización frontend lazy loading. No se modificó código.

### Evidencia De Deploy

- Commit desplegado: `e4c28205c6b866ab0d71edb13c49164100340b0d`.
- GitHub Actions run: `28712956106`.
- Resultado deploy DEV: `success`.
- DEV: `https://dev.laboratoriodentaltlahuac.com`.
- `GET /health`: `200`.
- `GET /api/deliveries` sin sesión: `401`.

### Resultado

- Delivery API queda registrada como desplegada y protegida en DEV.
- El cambio de `/api/deliveries` sin sesión de `404` anterior a `401` confirma que `DeliveryEndpoints` están publicados.
- La migración `WorkOrderDeliveries` ya está aplicada o la base DEV está al día.
- Lazy loading frontend queda registrado como desplegado.
- Warning de initial bundle queda resuelto: de `535.62 kB` a `304.19 kB`, sin modificar budgets.

### Pendientes

- Validación manual Admin en DEV del flujo delivery.
- Siguiente fase en ese cierre: Fase 3.4.2 - UI admin de entregas desde órdenes. Implementada posteriormente el 2026-07-04.

### Validaciones Ejecutadas

- `npm run build` desde `src/LaboratorioTlahuac.Web`: correcto; initial total `304.19 kB`, sin warning de budget.
- `dotnet build`: correcto; 0 errores y 2 warnings `NU1903` conocidos por `SQLitePCLRaw.lib.e_sqlite3` 2.1.11 en tests.
- `dotnet test`: correcto; Domain 1/1, Application 1/1 y API 121/121.
- `git diff --check`: correcto.

### Archivos Modificados

- `docs/05-delivery/dev-deployment-validation.md`
- `docs/08-qa/delivery-api-qa.md`
- `docs/PROJECT_STATUS.md`
- `docs/ROADMAP.md`
- `docs/IMPLEMENTATION_LOG.md`
- `docs/03-architecture/frontend-architecture.md`
- `docs/03-architecture/ARCHITECTURE.md`

### Confirmaciones

- Solo documentación modificada.
- No se imprimieron secretos.
- No se ejecutó `dotnet user-secrets list`.
- No se usó `codex-cobranza-sql`.
- `/login` sigue público.
- `/app` y `/app/dashboard` siguen privados.
- `/dashboard` no es ruta privada real.

## 2026-07-04 - Optimización Frontend Lazy Loading De Rutas

### Cambio Realizado

Se redujo el bundle inicial Angular convirtiendo `src/LaboratorioTlahuac.Web/src/app/app.routes.ts` de imports eager con `component:` a rutas lazy con `loadComponent`.

### Motivo

Después de Fase 3.4.1, `npm run build` pasaba pero emitía warning de presupuesto inicial:

- Budget configurado: `maximumWarning: 500kB`, `maximumError: 1MB`.
- Build base: initial total `535.62 kB`.
- Exceso: `35.62 kB` sobre el warning de `500.00 kB`.

No se subió el budget porque el problema venía de cargar eagerly páginas públicas, privadas, layouts y features desde `app.routes.ts`.

### Estrategia Aplicada

- `app.routes.ts` conserva eager solo `Routes`, `authGuard` y `permissionGuard`.
- `PublicLayoutComponent` y `PrivateLayoutComponent` se cargan con `loadComponent`.
- Todas las páginas públicas se cargan con `loadComponent`: `/`, `/catalogo`, `/servicios`, `/contacto` y `/login`.
- Todas las páginas privadas existentes bajo `/app` se cargan con `loadComponent`: dashboard, órdenes, etiquetas, clientes, pagos, inventario, proveedores, usuarios, roles y access denied.
- Los paths, redirects, titles, guards y `data.permission` quedaron sin cambios.
- Los componentes ya eran compatibles con `loadComponent` porque usan componentes standalone con `imports` en sus decorators; no se requirió convertir componentes ni agregar módulos.

### Resultado

- Build posterior: initial total `304.19 kB`.
- Reducción aproximada: `231.43 kB`.
- El warning de budget inicial desapareció.
- No se modificó `angular.json`; `maximumWarning` sigue en `500kB` y `maximumError` sigue en `1MB`.
- `npm run build -- --stats-json` fue soportado por el builder y generó `dist/laboratorio-tlahuac-web/stats.json` durante esa corrida; el build final normal limpia el output y no conserva el archivo.

### Confirmaciones

- `/login` sigue público.
- `/app` y `/app/dashboard` siguen privados por `authGuard` y `permissionGuard`.
- `/dashboard` no se creó ni se convirtió en ruta privada real.
- Las rutas de etiquetas siguen bajo `/app/ordenes/:id/*` y protegidas con `orders.view`.
- `/app/admin/usuarios` y `/app/admin/roles` siguen protegidas con `users.manage` y `roles.manage`.
- No se tocaron backend, `AuthService`, guards, cookies, XSRF, endpoints, migraciones ni deploy.
- No se instalaron dependencias.
- No se imprimieron secretos.
- No se hizo commit.

### Validaciones Ejecutadas

- `git status --short`: limpio antes de iniciar.
- `git diff --stat`: sin cambios antes de iniciar.
- `npm run build` base: correcto con warning inicial `535.62 kB`.
- `npm run build -- --stats-json`: correcto; builder soporta stats.
- `npm run build` posterior: correcto sin warning de budget inicial, initial total `304.19 kB`.
- `dotnet build`: correcto; 0 errores y 2 warnings `NU1903` conocidos por `SQLitePCLRaw.lib.e_sqlite3` 2.1.11 en tests.
- `dotnet test`: correcto; Domain 1/1, Application 1/1 y API 121/121; mismos warnings `NU1903` conocidos durante restore.
- `git diff --check`: correcto.
- `rg "component:" src/LaboratorioTlahuac.Web/src/app/app.routes.ts`: sin coincidencias.
- `rg "loadComponent" src/LaboratorioTlahuac.Web/src/app/app.routes.ts`: confirma rutas lazy.
- `rg "/dashboard" .`, `rg "/app/dashboard" .` y `rg "/login" src/LaboratorioTlahuac.Web/src/app docs README.md AGENTS.md`: ejecutados; las menciones confirman `/app/dashboard` como ruta privada real y `/login` como entrada pública, sin crear `/dashboard` raíz como ruta privada.
- Búsquedas de `LT_ADMIN_PASSWORD`, `LT_QA_LIMITED_PASSWORD`, `LDT_SQL_SA_PASSWORD` y `ConnectionStrings`: ejecutadas con salida limitada a archivos para no imprimir valores.
- `rg "codex-cobranza-sql" docs README.md AGENTS.md`: solo menciones documentales/históricas de no uso.

## 2026-07-04 - Fase 3.4.1.1 QA Técnico Delivery

### Cambio Realizado

Se ejecutó QA técnico real de migración delivery, permisos y endpoints antes de commit/push. No se hizo commit.

### Revisión Técnica

- Migración `20260704053734_AddWorkOrderDeliveries`: solo crea `WorkOrderDeliveries`, FKs e índices; no altera tablas existentes ni agrega seed sensible.
- FK requerida a `WorkOrders`; FK opcional a `Security.Users` para `AssignedToUserId`.
- Campos opcionales (`AssignedToUserId`, `RecipientName`, `DeliveryNotes`, `FailedReason`, timestamps de asignación/salida/entrega/fallo) quedan nullable.
- Índice único por `WorkOrderId`; índices por `AssignedToUserId`, `Status` y `CreatedAtUtc`.
- `WorkOrderDelivery` valida transiciones, requiere `recipientName` para completar, requiere `failedReason` para no entregada y usa timestamps UTC recibidos del servicio.
- `WorkOrder.DeliveryDate` no se modifica; al completar entrega se sincroniza `WorkOrder.Status` a `Delivered`.
- Endpoints usan permisos esperados y validaciones 400 controladas; no exponen password, `passwordHash` ni email del usuario asignado en respuestas de delivery.
- `tests/LaboratorioTlahuac.Domain.Tests/UnitTest1.cs` ya contenía `PermissionsTests`; se renombró a `PermissionsTests.cs` para limpiar el placeholder.

### Corrección Acotada

Durante QA real, el Admin local existente no tenía `deliveries.assign` porque `SecuritySeed:RunOnStartup=false` y solo corría baseline. Se corrigió `SecuritySeeder` para que `SecuritySeed:EnsureBaselineOnStartup=true` sincronice permisos faltantes de `Permissions.All` al rol `Admin` existente sin leer ni escribir contraseñas ni crear usuarios.

Prueba agregada: `BaselineSeedAddsMissingPermissionsToExistingAdminRole`.

### SQL Local

- `docker ps --filter "name=ldt-labdental-sql"`: contenedor activo.
- `docker port ldt-labdental-sql`: `14336 -> 1433/tcp`.
- `docker ps --filter "name=codex-cobranza-sql" --format "{{.Names}}"`: sin salida; no se usó.
- `dotnet ef migrations list`: `20260704053734_AddWorkOrderDeliveries` estaba pendiente.
- `dotnet ef database update`: correcto contra `LaboratorioTlahuac_Dev` local.
- `dotnet ef migrations list` posterior: `20260704053734_AddWorkOrderDeliveries` quedó aplicado.

### API Local Real

Se levantó API local en `http://localhost:5277` y se ejecutó script temporal de QA en `/tmp`, sin imprimir cookies, passwords ni payloads completos.

Resultado:

- `GET /health`: `200`.
- Sin sesión: delivery endpoints respondieron `401` con XSRF válido en mutables.
- Admin: permisos delivery presentes tras corrección de baseline.
- `Repartidor`: solo `deliveries.complete` y `deliveries.view`.
- Flujo Admin: crear orden/entrega, consultar por orden, listar, asignar, registrar salida y completar con `recipientName`: correcto.
- Validación `complete` sin `recipientName`: `400`.
- Validación `failed` sin `failedReason`: `400`.
- Flujo Repartidor: ve entregas asignadas, recibe `403` al asignar y al registrar salida, completa entrega asignada.
- Logout: correcto.
- Se crearon datos QA locales con prefijos de prueba; no se limpiaron.

### Validaciones Ejecutadas

- `dotnet build`: correcto; 0 errores y 2 warnings `NU1903` conocidos por `SQLitePCLRaw.lib.e_sqlite3` 2.1.11 en tests.
- `dotnet test`: correcto; Domain 1/1, Application 1/1 y API 121/121.
- `npm run build` desde `src/LaboratorioTlahuac.Web`: correcto; warning de budget inicial excedido por 26.71 kB.
- `git diff --check`: correcto.
- Búsquedas obligatorias de delivery, permisos, rutas, `Repartidor`, variables sensibles, `ConnectionStrings` y `codex-cobranza-sql`: ejecutadas. Las búsquedas sensibles usaron salida limitada a archivos.

### Confirmaciones

- No se tocaron frontend UI, `AuthService`, guards, cookies, XSRF ni deploy.
- No se instalaron dependencias.
- No se crearon migraciones adicionales.
- No se ejecutó `dotnet user-secrets list`.
- No se imprimieron secretos.
- No se usó `codex-cobranza-sql`.
- No se hizo commit.
- Pendiente en ese momento: aplicar migración en VPS DEV antes de usar endpoints delivery publicados. Cerrado o no aplicable tras el despliegue DEV 2026-07-04 porque `/api/deliveries` ya responde `401` sin sesión.

## 2026-07-04 - Fase 3.4.1 Backend Delivery MVP Y Permisos

### Cambio Realizado

Se implementó Fase 3.4.1 como backend delivery MVP + permisos, sin UI.

### Backend

- Se agregó `DeliveryStatus`.
- Se agregó entidad `WorkOrderDelivery`.
- Se configuró EF para `WorkOrderDeliveries`.
- Se agregó `DbSet<WorkOrderDelivery>`.
- Se agregó migración `20260704053734_AddWorkOrderDeliveries`.
- Se agregaron contratos y servicio `IDeliveryService`.
- Se agregaron endpoints API:
  - `GET /api/deliveries`
  - `GET /api/deliveries/{id}`
  - `GET /api/work-orders/{workOrderId}/delivery`
  - `POST /api/work-orders/{workOrderId}/delivery`
  - `PATCH /api/deliveries/{id}/assign`
  - `PATCH /api/deliveries/{id}/out-for-delivery`
  - `PATCH /api/deliveries/{id}/complete`
  - `PATCH /api/deliveries/{id}/failed`

### Permisos Y Seed

- Se agregaron permisos:
  - `deliveries.view`
  - `deliveries.assign`
  - `deliveries.update`
  - `deliveries.complete`
- Admin conserva todos los permisos porque el seed Admin usa `Permissions.All`.
- `Repartidor` se sincroniza con permisos mínimos:
  - `deliveries.view`
  - `deliveries.complete`
- `Repartidor` no recibe `deliveries.assign`, `deliveries.update`, `orders.view`, `customers.view`, `payments.view`, `users.manage` ni `roles.manage`.

### Reglas Implementadas

- Una orden puede tener una entrega en este MVP.
- `POST /api/work-orders/{workOrderId}/delivery` crea entrega `PendingAssignment`.
- Asignar repartidor pasa a `Assigned`.
- Marcar salida requiere `Assigned` y pasa a `OutForDelivery`.
- Completar requiere `OutForDelivery` y `recipientName`; pasa a `Delivered`.
- No entregada requiere `Assigned` u `OutForDelivery` y `failedReason`; pasa a `FailedDelivery`.
- Transiciones inválidas devuelven `400`.
- Orden cancelada no puede crear, salir, completar ni fallar entrega.
- `DeliveryDate` de `WorkOrder` no se modifica.
- Al completar entrega correctamente, `WorkOrder.Status` se sincroniza a `Delivered` para conservar tableros actuales.
- Usuarios sin permisos administrativos solo ven/mutan entregas asignadas a su usuario.

### Pruebas Agregadas

- `DeliveryIntegrationTests`.
- Actualización de `AdminSecurityIntegrationTests`.
- Actualización de `SecuritySeederTests`.
- Actualización de `PermissionsTests`.

Cobertura principal:

- `401` sin sesión.
- `403` sin permisos.
- Flujo Admin crear/listar/detalle/asignar/salida/completar/fallida.
- Validaciones de `recipientName` y `failedReason`.
- Transición inválida.
- Repartidor ve asignadas y completa.
- Repartidor no puede asignar.
- Permisos y seed esperados.

### Archivos Creados

- `src/LaboratorioTlahuac.Domain/Deliveries/DeliveryStatus.cs`
- `src/LaboratorioTlahuac.Domain/Deliveries/Entities/WorkOrderDelivery.cs`
- `src/LaboratorioTlahuac.Application/Deliveries/DeliveryContracts.cs`
- `src/LaboratorioTlahuac.Application/Deliveries/DeliveryServiceResult.cs`
- `src/LaboratorioTlahuac.Application/Deliveries/IDeliveryService.cs`
- `src/LaboratorioTlahuac.Infrastructure/Deliveries/DeliveryService.cs`
- `src/LaboratorioTlahuac.Infrastructure/Persistence/Configurations/WorkOrderDeliveryConfiguration.cs`
- `src/LaboratorioTlahuac.Api/Endpoints/DeliveryEndpoints.cs`
- `src/LaboratorioTlahuac.Infrastructure/Persistence/Migrations/20260704053734_AddWorkOrderDeliveries.cs`
- `src/LaboratorioTlahuac.Infrastructure/Persistence/Migrations/20260704053734_AddWorkOrderDeliveries.Designer.cs`
- `tests/LaboratorioTlahuac.Api.Tests/DeliveryIntegrationTests.cs`
- `docs/08-qa/delivery-api-qa.md`

### Archivos Modificados

- `README.md`
- `docs/README.md`
- `docs/PROJECT_STATUS.md`
- `docs/ROADMAP.md`
- `docs/IMPLEMENTATION_LOG.md`
- `docs/01-product/delivery-mvp-design.md`
- `docs/01-product/driver-mobile-workflow.md`
- `docs/01-product/internal-system.md`
- `docs/01-product/operations-orders-delivery.md`
- `docs/03-architecture/ARCHITECTURE.md`
- `docs/03-architecture/AUTH_FLOW.md`
- `src/LaboratorioTlahuac.Api/Program.cs`
- `src/LaboratorioTlahuac.Domain/Security/Permissions.cs`
- `src/LaboratorioTlahuac.Infrastructure/DependencyInjection.cs`
- `src/LaboratorioTlahuac.Infrastructure/Persistence/LaboratorioTlahuacDbContext.cs`
- `src/LaboratorioTlahuac.Infrastructure/Persistence/Migrations/LaboratorioTlahuacDbContextModelSnapshot.cs`
- `src/LaboratorioTlahuac.Infrastructure/Security/Seed/SecuritySeeder.cs`
- `tests/LaboratorioTlahuac.Api.Tests/AdminSecurityIntegrationTests.cs`
- `tests/LaboratorioTlahuac.Api.Tests/SecuritySeederTests.cs`
- `tests/LaboratorioTlahuac.Domain.Tests/UnitTest1.cs`

### Validaciones Ejecutadas

- `git status --short` antes de modificar: sin salida.
- `git diff --stat` antes de modificar: sin salida.
- `dotnet build`: correcto; 0 errores y 2 warnings `NU1903` conocidos por `SQLitePCLRaw.lib.e_sqlite3` 2.1.11 en tests.
- `dotnet test`: correcto; Domain 1/1, Application 1/1 y API 120/120.
- `npm run build` desde `src/LaboratorioTlahuac.Web`: correcto; warning de budget inicial excedido por 26.71 kB.
- `git diff --check`: correcto.
- Búsquedas finales solicitadas de delivery, permisos, rutas, `Repartidor`, variables sensibles, `ConnectionStrings` y `codex-cobranza-sql`: ejecutadas; las búsquedas sensibles se limitaron a archivos para no imprimir valores.

### Confirmaciones

- No se implementó UI.
- No se tocaron `AuthService`, guards, cookies ni XSRF.
- No se ejecutó `dotnet user-secrets list`.
- No se usó `codex-cobranza-sql`.
- No se cambiaron rutas privadas reales.
- `/dashboard` no se convirtió en ruta privada real.
- No se cambió `DeliveryDate`.
- No se instalaron dependencias.
- No se hizo deploy real.
- No se ejecutó `dotnet user-secrets list`.
- No se usó `codex-cobranza-sql`.

### Siguiente Fase Recomendada

Fase 3.4.2 - UI admin de entregas desde órdenes.

## 2026-07-03 - Fase 3.4.0 Análisis Técnico Entregas/Repartidor Mobile-First

### Cambio Realizado

Se ejecutó Fase 3.4.0 como análisis técnico/documental previo del flujo de entregas/repartidor mobile-first.

No se implementó código. No se crearon migraciones. No se modificó backend/frontend funcional. No se tocaron `AuthService`, guards, cookies, XSRF, rutas privadas reales, deploy ni dependencias.

### Contexto Revisado

- `AGENTS.md`.
- `README.md`.
- `docs/PROJECT_STATUS.md`.
- `docs/ROADMAP.md`.
- `docs/IMPLEMENTATION_LOG.md`.
- `docs/01-product/driver-mobile-workflow.md`.
- `docs/01-product/operations-orders-delivery.md`.
- `docs/01-product/label-printing.md`.
- `docs/01-product/internal-system.md`.
- `docs/08-qa/users-roles-qa.md`.
- Modelo actual de `WorkOrder`, estados, endpoints, rutas `/app/ordenes`, permisos, rol `Repartidor` y administración de usuarios/roles.

### Hallazgos Técnicos

- `/app/ordenes` ya existe como módulo real y no debe duplicarse.
- El detalle de orden ya concentra estado, historial, pagos y acciones de etiqueta.
- Etiquetas ya existen bajo `/app/ordenes/:id/etiqueta-trabajo` y `/app/ordenes/:id/etiqueta-entrega`.
- `WorkOrder` contiene `DeliveryDate`, pero esa fecha es planeada/capturada; no representa salida, entrega real, receptor ni evidencia.
- `WorkOrderStatus` ya contiene `ReadyForDelivery` y `Delivered`, pero esos estados pertenecen al ciclo operativo de la orden, no a logística fina.
- El detalle actual de orden no expone dirección, teléfono, WhatsApp ni email del cliente; esos datos existen en `Customer`.
- No existe entidad `Delivery`, `WorkOrderDelivery` ni `DeliveryStatus`.
- No existen permisos `deliveries.*` en `Permissions.All`.
- El rol `Repartidor` existe como rol de sistema sin permisos activos y sin acceso amplio a órdenes completas.
- `/app/admin/usuarios` permite crear/editar/activar/desactivar usuarios, asignar roles existentes y setear contraseña temporal.
- `/app/admin/roles` es readonly y muestra roles/permisos.

### Diseño Documentado

Se creó `docs/01-product/delivery-mvp-design.md` con:

- Comparación entre extender `WorkOrder` y crear entidad separada `Delivery` / `WorkOrderDelivery`.
- Recomendación de entidad separada `WorkOrderDelivery` para trazabilidad real.
- Regla MVP de una entrega activa por orden.
- Estados recomendados: `PendingAssignment`, `Assigned`, `OutForDelivery`, `Delivered`, `FailedDelivery` y `Cancelled`.
- Permisos propuestos: `deliveries.view`, `deliveries.assign`, `deliveries.update` y `deliveries.complete`.
- Permisos recomendados para rol `Repartidor`: `deliveries.view`, `deliveries.update` y `deliveries.complete`, sin `orders.view`, `customers.view` ni `payments.view`.
- Rutas recomendadas: `/app/entregas` y `/app/entregas/:id`.
- Endpoints MVP recomendados:
  - `GET /api/deliveries/mine`
  - `GET /api/deliveries/{id}`
  - `GET /api/work-orders/{workOrderId}/delivery`
  - `POST /api/work-orders/{workOrderId}/delivery`
  - `PATCH /api/deliveries/{id}/assignment`
  - `PATCH /api/deliveries/{id}/out-for-delivery`
  - `PATCH /api/deliveries/{id}/delivered`
  - `PATCH /api/deliveries/{id}/failed`
- Flujo MVP para administración desde orden y para repartidor desde celular.
- Fases recomendadas 3.4.1 a 3.4.4 y fase posterior de firma/foto/ubicación/QR.

### Archivos Creados

- `docs/01-product/delivery-mvp-design.md`

### Archivos Modificados

- `README.md`
- `docs/README.md`
- `docs/PROJECT_STATUS.md`
- `docs/ROADMAP.md`
- `docs/IMPLEMENTATION_LOG.md`
- `docs/01-product/driver-mobile-workflow.md`
- `docs/01-product/internal-system.md`
- `docs/01-product/operations-orders-delivery.md`
- `docs/03-architecture/ARCHITECTURE.md`

### Validaciones Ejecutadas

- `git status --short` antes de documentar: sin salida.
- `git diff --stat` antes de documentar: sin salida.
- `npm run build` desde `src/LaboratorioTlahuac.Web`: correcto; warning de presupuesto inicial excedido por 31.04 kB (`531.04 kB` contra budget `500.00 kB`).
- `dotnet build`: correcto; 0 errores y 2 warnings `NU1903` conocidos por `SQLitePCLRaw.lib.e_sqlite3` 2.1.11 en tests.
- `dotnet test`: correcto; Domain 1/1, Application 1/1 y API 110/110.
- `git diff --check`: correcto.
- Búsquedas/revisión local ejecutadas para `WorkOrder`, `WorkOrderStatus`, endpoints de órdenes, rutas `/app/ordenes`, permisos, `Repartidor`, administración de usuarios/roles, `Delivery`, `/app/entregas`, `/api/deliveries`, `/app/dashboard` y `/login`.

### Confirmaciones

- No se modificó código.
- No se crearon migraciones.
- No se agregaron endpoints.
- No se agregaron permisos reales a `Permissions.All`.
- No se modificó base de datos.
- No se modificaron `AuthService`, `auth.guard.ts`, `permission.guard.ts`, cookies ni XSRF.
- No se tocó deploy.
- No se instalaron dependencias.
- No se ejecutó `dotnet user-secrets list`.
- No se usó `codex-cobranza-sql`.
- No se imprimieron secretos.

### Siguiente Fase Recomendada

Fase 3.4.1 - backend delivery MVP + permisos.

Alcance recomendado: agregar permisos `deliveries.*`, crear entidad `WorkOrderDelivery`, crear `DeliveryStatus`, generar migración, implementar endpoints mínimos de entregas y cubrir autorización con pruebas antes de construir UI admin/repartidor.

## 2026-07-03 - Fase 3.3.1 QA Seguridad Usuarios/Roles Y Preparacion DEV

### Cambio Realizado

Se ejecutó QA de seguridad, validación técnica y preparación de despliegue DEV para la administración MVP de usuarios y roles.

No se implementaron funcionalidades nuevas, no se rediseñaron pantallas, no se hicieron migraciones, no se tocó deploy real y no se hizo commit.

### Seguridad Revisada

- `appsettings.json` conserva `SecuritySeed:EnsureBaselineOnStartup=false` y `SecuritySeed:RunOnStartup=false`.
- `appsettings.Development.json` mantiene `SecuritySeed:EnsureBaselineOnStartup=true` solo para Development.
- Revisión redaccionada de `ConnectionStrings`: cadenas locales sin `Password=` ni `User Id=`.
- `SecuritySeed:Admin:Password` permanece vacío en `appsettings.json`.
- No se detectaron passwords reales ni secretos guardados en appsettings.
- Todos los endpoints admin tienen `RequireAuthorization` con `users.manage` o `roles.manage`.
- Sin sesión, los nueve endpoints admin devolvieron `401`; para `POST`/`PUT`/`PATCH` se envió XSRF válido para aislar la validación de autenticación.
- Usuario sin permisos admin recibe `403` por pruebas API existentes para `/api/admin/users` y `/api/admin/roles`.
- La contraseña temporal no se devuelve en listados ni detalles, no se registra en logs y queda como riesgo DEV/UAT hasta implementar force-change password.
- El backend evita desactivar la propia cuenta y evita dejar el sistema sin un usuario activo con `users.manage`.
- `Repartidor` queda como rol base sin permisos activos; permisos reales de entregas se difieren a Fase 3.4.

### Validacion Local

- `docker ps --filter name=ldt-labdental-sql`: `ldt-labdental-sql` activo en `14336`.
- `docker ps --filter name=codex-cobranza-sql`: sin contenedor activo; no se usó.
- API local levantada con `dotnet run --project src/LaboratorioTlahuac.Api/LaboratorioTlahuac.Api.csproj`.
- Angular local levantado con `npm start` en `http://localhost:4200/`.
- `GET /health`: `200`.
- `GET /api/auth/csrf`: `204` y token XSRF presente.
- `/login`, `/app/admin/usuarios` y `/app/admin/roles`: shell Angular `200` por `curl`.

### Validaciones Ejecutadas

- `npm run build` desde `src/LaboratorioTlahuac.Web`: correcto; warning de presupuesto inicial excedido por 31.04 kB (`531.04 kB` contra budget `500.00 kB`).
- `dotnet build`: correcto; 0 errores y 2 warnings `NU1903` conocidos por `SQLitePCLRaw.lib.e_sqlite3` 2.1.11 en tests.
- `dotnet test`: correcto; Domain 1/1, Application 1/1 y API 110/110.
- `git diff --check`: correcto.
- Búsquedas obligatorias ejecutadas para endpoints, permisos, rutas, `Repartidor`, `/dashboard`, `/app/dashboard`, `/login`, variables sensibles, `ConnectionStrings` y `codex-cobranza-sql`.
- Las búsquedas de variables sensibles y `ConnectionStrings` se limitaron a archivos para no imprimir valores.

### Documentación Actualizada

- `docs/08-qa/users-roles-qa.md`
- `docs/PROJECT_STATUS.md`
- `docs/ROADMAP.md`
- `docs/01-product/internal-system.md`
- `docs/03-architecture/AUTH_FLOW.md`
- `docs/03-architecture/ARCHITECTURE.md`
- `docs/IMPLEMENTATION_LOG.md`

### Pendientes

- Validar visualmente en DEV `/app/admin/usuarios` y `/app/admin/roles` con Admin real.
- Validar en DEV `/app/access-denied` con usuario real sin `users.manage`/`roles.manage`.
- Implementar force-change password antes de producción.
- Atender el warning de budget inicial en una fase de optimización frontend; no bloquea DEV porque está por debajo del `maximumError` de `1MB`.
- Si el pase DEV no encuentra bloqueantes, hacer commit/push a `dev` y desplegar DEV antes de iniciar Fase 3.4.

## 2026-07-03 - Fase 3.3 Administracion De Usuarios Y Roles MVP

### Cambio Realizado

Se implementó la administración MVP de usuarios y roles para DEV/UAT.

`/app/admin/usuarios` deja de ser placeholder y permite:

- listar usuarios;
- crear usuarios con contraseña temporal capturada por Admin;
- editar email y nombre;
- activar/desactivar usuarios;
- asignar roles existentes;
- actualizar contraseña temporal sin mostrarla después ni enviarla por correo;
- ver estados de carga, error y vacío con layout responsive.

`/app/admin/roles` deja de ser placeholder y permite ver roles, conteos y permisos por rol en modo solo lectura.

### Backend

Endpoints agregados bajo `/api/admin`:

- `GET /api/admin/users`
- `GET /api/admin/users/{id}`
- `POST /api/admin/users`
- `PUT /api/admin/users/{id}`
- `PATCH /api/admin/users/{id}/status`
- `PATCH /api/admin/users/{id}/roles`
- `POST /api/admin/users/{id}/temporary-password`
- `GET /api/admin/roles`
- `GET /api/admin/roles/{id}`

Archivos creados:

- `src/LaboratorioTlahuac.Application/Admin/AdminSecurityContracts.cs`
- `src/LaboratorioTlahuac.Application/Admin/AdminSecurityServiceResult.cs`
- `src/LaboratorioTlahuac.Application/Admin/IAdminSecurityService.cs`
- `src/LaboratorioTlahuac.Infrastructure/Admin/AdminSecurityService.cs`
- `src/LaboratorioTlahuac.Api/Endpoints/AdminSecurityEndpoints.cs`
- `tests/LaboratorioTlahuac.Api.Tests/AdminSecurityIntegrationTests.cs`

Archivos modificados:

- `src/LaboratorioTlahuac.Domain/Security/Entities/User.cs`: agrega actualización controlada de email/nombre.
- `src/LaboratorioTlahuac.Infrastructure/DependencyInjection.cs`: registra `IAdminSecurityService`.
- `src/LaboratorioTlahuac.Api/Program.cs`: mapea endpoints admin y considera `SecuritySeed:EnsureBaselineOnStartup`.
- `src/LaboratorioTlahuac.Infrastructure/Security/Seed/SecuritySeedOptions.cs`: agrega bandera de baseline.
- `src/LaboratorioTlahuac.Infrastructure/Security/Seed/SecuritySeeder.cs`: asegura permisos existentes y rol `Repartidor`.
- `src/LaboratorioTlahuac.Api/appsettings.json`: documenta baseline apagado por default general.
- `src/LaboratorioTlahuac.Api/appsettings.Development.json`: activa baseline Development.

### Frontend

Archivos creados:

- `src/LaboratorioTlahuac.Web/src/app/admin/admin-security.models.ts`
- `src/LaboratorioTlahuac.Web/src/app/admin/admin-security.service.ts`

Archivos modificados:

- `src/LaboratorioTlahuac.Web/src/app/admin/pages/users/users-page.component.ts`
- `src/LaboratorioTlahuac.Web/src/app/admin/pages/roles/roles-page.component.ts`
- `src/LaboratorioTlahuac.Web/src/styles.scss`

Las rutas existentes en `app.routes.ts` se conservaron:

- `/app/admin/usuarios` requiere `users.manage`.
- `/app/admin/roles` requiere `roles.manage`.

No se modificaron `auth.guard.ts`, `permission.guard.ts`, `AuthService`, cookies, XSRF, `returnUrl` ni `/dashboard`.

### Seguridad Y Decisiones

- El modelo actual soporta usuarios, roles y permisos; no se crearon migraciones.
- No hay delete de usuarios ni roles.
- Roles/permisos quedan readonly en Fase 3.3.
- Las respuestas admin no incluyen `passwordHash`.
- La contraseña temporal se recibe por request, se hashea con `PasswordHasher<User>` y no se devuelve en respuestas.
- El backend evita desactivar al propio usuario.
- El backend evita dejar el sistema sin un usuario activo con `users.manage`.
- `Repartidor` se prepara como rol de sistema sin permisos activos y sin acceso amplio a órdenes completas.
- Permisos futuros sugeridos para entregas: `deliveries.view` y `deliveries.update`; no se agregaron a `Permissions.All` porque no existe módulo de entregas todavía.

### Pruebas Agregadas

`AdminSecurityIntegrationTests` cubre:

- Admin puede listar usuarios.
- Admin puede crear usuario.
- Admin puede asignar rol.
- Admin puede activar/desactivar usuario.
- Usuario sin permiso recibe `403`.
- Usuario sin sesión recibe `401`.
- No se filtra `passwordHash`.
- La contraseña temporal no se devuelve en respuesta.
- Roles list/detail funcionan.
- Admin existente conserva `users.manage`, `roles.manage` y rol `Admin`.
- `Repartidor` existe y no tiene permisos.

### Validaciones Ejecutadas

- `git status --short`: limpio al inicio.
- `git diff --stat`: limpio al inicio.
- `npm run build` desde `src/LaboratorioTlahuac.Web`: correcto; warning de presupuesto inicial excedido por 31.04 kB.
- `dotnet build`: correcto con 0 errores y 2 warnings `NU1903` conocidos por `SQLitePCLRaw.lib.e_sqlite3` 2.1.11 en tests.
- `dotnet test`: correcto; Domain 1/1, Application 1/1 y API 110/110.
- `git diff --check`: correcto.
- `rg "/app/admin/usuarios" src docs README.md`: ejecutado.
- `rg "/app/admin/roles" src docs README.md`: ejecutado.
- `rg "users.manage" src docs tests README.md`: ejecutado.
- `rg "roles.manage" src docs tests README.md`: ejecutado.
- `rg "Repartidor" src docs tests README.md`: ejecutado.
- `rg "/dashboard" .`: ejecutado; no se creó `/dashboard` como ruta privada real.
- `rg "/app/dashboard" .`: ejecutado; `/app/dashboard` sigue siendo la ruta privada real.
- `rg "/login" src/LaboratorioTlahuac.Web/src/app docs README.md AGENTS.md`: ejecutado; `/login` sigue público con `returnUrl`.
- `rg "LT_ADMIN_PASSWORD" .`: ejecutado; solo nombres de variables/placeholders o referencias documentales, sin valores reales impresos en documentación.
- `rg "LT_QA_LIMITED_PASSWORD" .`: ejecutado; solo nombres de variables/placeholders o referencias documentales, sin valores reales impresos en documentación.
- `rg "LDT_SQL_SA_PASSWORD" .`: ejecutado; solo nombres de variables/placeholders o referencias documentales, sin valores reales impresos en documentación.
- `rg "ConnectionStrings" src docs README.md`: ejecutado; solo claves/placeholders o cadenas locales sin passwords reales documentados.
- `rg "codex-cobranza-sql" docs README.md AGENTS.md`: ejecutado; solo menciones históricas/documentales de no uso.

### Pendientes

- Ejecutar validación visual real en DEV de `/app/admin/usuarios` y `/app/admin/roles`.
- Confirmar `/app/access-denied` en DEV con usuario real sin `users.manage`/`roles.manage`.
- Implementar force-change password antes de operación productiva amplia.
- Diseñar Fase 3.4 de entregas/repartidor mobile-first con permisos, modelo, endpoints y UI.
- Mantener pendiente la prueba física de etiquetas por driver de impresora.

## 2026-07-03 - Auditoría Admin Angular Zoneless Extendida

### Cambio Realizado

Se auditó el frontend Admin completo buscando pantallas que todavía actualizaran estado renderizado desde `subscribe()` o `finalize()` mediante propiedades mutables normales en Angular sin `zone.js` y con `HttpClient` + `withFetch()`.

Se corrigieron los componentes con evidencia clara y alcance acotado:

- `/app/ordenes/:id/editar`.
- `/app/ordenes/:id/etiqueta-trabajo`.
- `/app/ordenes/:id/etiqueta-entrega`.
- `/app/pagos`.
- Sección de pagos dentro de `/app/ordenes/:id`.
- Sección de doctores internos dentro del detalle de clientes.

### Causa Técnica

El patrón pendiente era el mismo ya corregido en Clientes, Dashboard y Órdenes: estados como `isLoading`, `order`, `items`, `payments`, `summary`, `errorMessage`, `isSubmitting`, `isCreating`, `cancellingPaymentId` o filtros se mutaban dentro de callbacks async.

En Angular zoneless esas mutaciones no siempre invalidan la vista, aunque la respuesta HTTP sea `200`, por lo que la pantalla puede quedarse en `Cargando...` hasta que un clic, blur, focus u otro evento DOM provoca un nuevo ciclo de render.

### Inventario Global Ejecutado

Se ejecutaron búsquedas sobre `src/LaboratorioTlahuac.Web/src/app` para:

- Textos y estados de carga: `Cargando`, `loading`, `isLoading`.
- Patrones async: `subscribe()`, `finalize()`, `catchError()`, `HttpErrorResponse`, `firstValueFrom`, `lastValueFrom`.
- Uso de Angular signals: `signal()`, `computed()`, `toSignal`, `effect()`.
- Pantallas Admin relevantes: Etiquetas, Pagos, Ordenes, Clientes, Dashboard, Inventario, Proveedores, Usuarios y Roles.
- APIs de change detection o hacks: `ChangeDetectorRef`, `detectChanges`, `NgZone`, `ApplicationRef`, `window.location`, `setTimeout`, `dispatchEvent`, `reload`.

### Matriz De Auditoría Resumida

| Pantalla/ruta | Archivo | Problema encontrado | Acción | Estado |
| --- | --- | --- | --- | --- |
| `/app/ordenes/:id/editar` | `work-order-edit-page.component.ts` | `order`, `isLoading`, `isSubmitting` y mensajes eran mutables actualizados desde HTTP. | Migrado a signals y navegación post-update con `NavigationExtras.info`. | Corregido |
| `/app/ordenes/:id` | `work-order-detail-page.component.ts` | La página principal ya usaba signals; la sección hija de pagos no. | No tocar página; corregir componente hijo de pagos. | Ya estaba correcto / hijo corregido |
| Etiqueta interna | `work-order-job-label-page.component.ts` | `order`, `isLoading`, `errorMessage` y `fallbackRoute` eran mutables. | Migrado a signals. | Corregido |
| Etiqueta entrega | `work-order-delivery-label-page.component.ts` | `order`, `isLoading`, `errorMessage` y `fallbackRoute` eran mutables. | Migrado a signals. | Corregido |
| `/app/pagos` | `payment-list-page.component.ts` | Items, métodos, filtros, paginación, loading y error eran mutables. | Migrado a signals y bindings `[ngModel]`/`(ngModelChange)`. | Corregido |
| Pagos en detalle de orden | `work-order-payments-section.component.ts` | Resumen, pagos, métodos, flags de crear/cancelar/cargar y mensajes eran mutables. | Migrado a signals. | Corregido |
| Doctores internos | `internal-doctors-section.component.ts` | Lista, filtro, formulario visible, doctor en edición, saving/loading y error eran mutables. | Migrado a signals conservando Reactive Forms. | Corregido |
| `/app/ordenes` | `work-order-list-page.component.ts` | Ya corregido en tarea previa con signals. | Revisado sin cambios. | Ya estaba correcto |
| `/app/ordenes/nueva` | `work-order-create-page.component.ts` y `work-order-form.component.ts` | Ya corregido con signals para submit, clientes y doctores. | Revisado sin cambios. | Ya estaba correcto |
| `/app/dashboard` | `dashboard-page.component.ts` | Ya corregido con signals. | Revisado sin cambios. | Ya estaba correcto |
| `/app/clientes` y detalle/crear/editar | `features/customers/pages/*` | Listado, detalle, crear y editar ya tenían signals en los flujos reportados. | Revisado sin cambios, salvo sección de doctores internos. | Ya estaba correcto / hijo corregido |
| Inventario | `inventory-page.component.ts` | Placeholder sin HTTP ni loading. | No tocar. | No aplica |
| Proveedores | `suppliers-page.component.ts` | Placeholder sin HTTP ni loading. | No tocar. | No aplica |
| Usuarios | `users-page.component.ts` | Placeholder sin HTTP ni loading. | No tocar. | No aplica |
| Roles | `roles-page.component.ts` | Placeholder sin HTTP ni loading. | No tocar. | No aplica |

### Archivos Modificados

- `src/LaboratorioTlahuac.Web/src/app/features/orders/pages/work-order-edit-page.component.ts`: migra carga/submit/error/orden a signals, conserva `WorkOrderUpsertRequest` y usa `NavigationExtras.info` para el flash transitorio post-guardado.
- `src/LaboratorioTlahuac.Web/src/app/features/orders/pages/work-order-job-label-page.component.ts`: migra etiqueta interna a signals sin cambiar formato ni `window.print()`.
- `src/LaboratorioTlahuac.Web/src/app/features/orders/pages/work-order-delivery-label-page.component.ts`: migra etiqueta de entrega a signals sin cambiar formato ni `window.print()`.
- `src/LaboratorioTlahuac.Web/src/app/features/payments/pages/payment-list-page.component.ts`: migra listado de pagos, filtros, métodos y paginación a signals.
- `src/LaboratorioTlahuac.Web/src/app/features/payments/components/work-order-payments-section.component.ts`: migra resumen, pagos, métodos, crear/cancelar y mensajes a signals.
- `src/LaboratorioTlahuac.Web/src/app/features/customers/components/internal-doctors-section.component.ts`: migra lista, filtro, modo formulario, doctor en edición, loading/saving y errores a signals conservando `FormGroup`.
- `docs/PROJECT_STATUS.md` y `docs/IMPLEMENTATION_LOG.md`: documentan auditoría, pantallas corregidas, pantallas revisadas y validación.

### Decisiones Técnicas

- No se modificaron servicios porque los contratos frontend/backend ya eran suficientes.
- No se modificó backend, base de datos, migraciones, endpoints ni DTOs.
- No se modificaron auth, sesiones, cookies, CSRF/XSRF, guards ni interceptor `401`.
- No se agregó `zone.js`, no se quitó `withFetch()` y no se usaron hacks de repintado como `setTimeout`, reload, `ApplicationRef.tick()`, clicks simulados, `detectChanges()` indiscriminado ni `document.dispatchEvent`.
- Los `403` siguen mostrando mensajes controlados locales; los `401` quedan a cargo del interceptor global existente.
- No se hizo rediseño responsive global.

### Validaciones Ejecutadas

- `git status --short`: limpio al inicio.
- `git log --oneline --decorate -8`: HEAD inicial en `d3534bc (HEAD -> dev, origin/dev) Ordenes a Angulas signals`.
- `npm run build` intermedio desde `src/LaboratorioTlahuac.Web`: correcto; warning de presupuesto inicial excedido por 7.01 kB.
- Validación de cierre: `git diff --check` correcto; `npm run build` correcto con warning de presupuesto inicial excedido por 7.01 kB; `dotnet build` correcto con 0 errores y 2 warnings `NU1903` conocidos por `SQLitePCLRaw.lib.e_sqlite3` 2.1.11 en tests; `dotnet test` correcto con Domain 1/1, Application 1/1 y API 101/101; `git status --short` y `git diff --stat` ejecutados.
- No existe script frontend `test` en `src/LaboratorioTlahuac.Web/package.json`; scripts detectados: `ng`, `start`, `build` y `watch`.

### Pendientes

- Validación manual en navegador real de `/app/ordenes/:id/editar`, etiqueta interna, etiqueta entrega, `/app/pagos`, crear/cancelar pagos si aplica y sección de doctores internos.
- Confirmar manualmente que un `401` redirige a `/login` y que un `403` no redirige a login en las pantallas corregidas.
- Revisar en otra fase cualquier pantalla futura de Inventario, Proveedores, Usuarios o Roles cuando deje de ser placeholder y agregue HTTP real.

## 2026-07-03 - Corrección Admin Órdenes Listado En Angular Zoneless Y Responsive

### Cambio Realizado

Se corrigió Admin > Órdenes de trabajo para que `/app/ordenes` pinte el listado al entrar al módulo sin depender de clic, blur o focus.

También se ajustó el responsive de la pantalla: el header permite wrap, los filtros de órdenes usan grid adaptable, la tabla queda contenida en scroll horizontal local para anchos intermedios y en móvil se muestran cards por orden con acciones Ver/Editar accesibles.

### Causa Técnica

El render congelado tenía la misma causa que Clientes, Nueva orden, Dashboard y Detalle de orden: `WorkOrderListPageComponent` mantenía `items`, `customers`, `statuses`, `isLoading`, `errorMessage`, `totalCount`, `page`, `pageSize` y filtros como propiedades mutables actualizadas dentro de `subscribe()` y `finalize()`.

En Angular zoneless con `HttpClient` y `withFetch()`, esas mutaciones pueden no invalidar inmediatamente la vista aunque los endpoints respondan `200`; por eso la pantalla podía quedar en `Cargando ordenes...` hasta que un evento DOM como blur/focus provocaba repintado.

El desbordamiento responsive venía de dos puntos: `orders-toolbar` tenía una grilla de siete columnas fijas/minmax que no envolvía bien en anchos intermedios, y la tabla de ocho columnas se renderizaba directamente en el flujo de página sin contenedor de overflow local ni alternativa móvil.

### Flujo Auditado

- Ruta real: `/app/ordenes`.
- Componente: `WorkOrderListPageComponent`.
- Servicio de órdenes: `WorkOrderService.list()` y `WorkOrderService.getStatuses()`.
- Servicio de clientes para filtro: `CustomerService.list({ isActive: true, pageSize: 100 })`.
- Endpoints al entrar: `GET /api/work-orders`, `GET /api/work-orders/statuses` y `GET /api/customers?isActive=true&pageSize=100`.
- Filtros conservados: búsqueda, cliente, estado, entrega desde, entrega hasta, incluir canceladas, botón Filtrar y paginación.
- Acciones conservadas: Ver, Editar cuando hay permiso y la orden no está cancelada, y navegación a `/app/ordenes/nueva`.
- Backend auditado: `WorkOrderEndpoints`, `CustomerEndpoints`, `WorkOrderService.ListAsync()` y contratos `WorkOrderListQuery`/`WorkOrderPagedResponse`.

### Respuestas De Auditoría

- `Cargando ordenes...` lo renderizaba `WorkOrderListPageComponent`.
- Los tres GET iniciales se ejecutaban desde `subscribe()`; el listado además apagaba loading en `finalize()`.
- El componente sí usaba propiedades mutables normales para items, clientes, estados, loading, error, total, página, total de páginas derivado y filtros.
- El loading se apagaba en success/error mediante `finalize()`, pero al ser mutable podía no repintar en zoneless.
- Ya existía mensaje visible para errores del listado; se mantuvo y se agregaron mensajes controlados para errores de carga de filtros.
- `CustomerListPageComponent` ya tenía el patrón corregido con signals; `WorkOrderListPageComponent` era el pendiente.
- No se encontró evidencia de lentitud o error backend como causa raíz.
- El botón `Nueva orden` estaba dentro de `page-header` sin wrap global suficiente para anchos reducidos.
- La tabla necesitaba scroll local en tablet y una alternativa móvil; se implementaron ambas.
- El patrón de Clientes se replicó para signals, y el responsive se ajustó con clases existentes más `table-scroll`, `orders-table-scroll`, `orders-mobile-list`, `order-card` y `pagination-actions`.

### Archivos Modificados

- `src/LaboratorioTlahuac.Web/src/app/features/orders/pages/work-order-list-page.component.ts`: migra estado renderizado, filtros y paginación a Angular signals; actualiza template a lecturas `signal()`; mantiene endpoints, DTOs, filtros, paginación y acciones; limpia listado/total en error y usa mensajes controlados.
- `src/LaboratorioTlahuac.Web/src/styles.scss`: ajusta `page-header`, `orders-toolbar`, contención de tablas, cards móviles de órdenes y paginación para evitar overflow global y mantener la tabla usable.
- `docs/PROJECT_STATUS.md` y `docs/IMPLEMENTATION_LOG.md`: documentan causa raíz, alcance, responsive, archivos modificados, validación y confirmaciones.

### Decisiones Técnicas

- No se modificó `WorkOrderService` porque los endpoints y DTOs ya coincidían con backend.
- No se tocó backend, base de datos ni migraciones porque la auditoría no mostró una causa backend.
- No se modificaron auth, sesiones, cookies, CSRF/XSRF, guards ni interceptor `401`; un `401` sigue siendo responsabilidad del interceptor global.
- No se agregó `zone.js` ni se quitó `withFetch()`.
- No se usaron hacks de repintado como `setTimeout`, reload, `ApplicationRef.tick()`, clicks simulados ni `detectChanges()`.

### Validaciones Ejecutadas

- `git status --short`: limpio al inicio.
- `git log --oneline --decorate -5`: HEAD en `be647a2 (HEAD -> dev, origin/dev) fix: update customer edit state with signals`.
- `git diff --check`: correcto.
- `npm run build` desde `src/LaboratorioTlahuac.Web`: correcto; reporta warning de presupuesto inicial excedido por 5.93 kB.
- `dotnet build`: correcto; 0 errores y 2 warnings `NU1903` conocidos por `SQLitePCLRaw.lib.e_sqlite3` 2.1.11 en el proyecto de tests.
- `dotnet test`: correcto; Domain 1/1, Application 1/1 y API 101/101.
- No existe script frontend `test` en `src/LaboratorioTlahuac.Web/package.json`; solo existen `ng`, `start`, `build` y `watch`.

### Confirmaciones

- No se hizo commit.
- No se desplegó.
- No se modificó backend.
- No se modificó base de datos.
- No se crearon migraciones.
- No se cambiaron contratos API ni endpoints.
- No se modificaron auth, sesiones, cookies, CSRF/XSRF ni interceptor `401`.
- No se agregaron secretos ni PII en documentación.

### Pendientes

- Validar manualmente en navegador real: `/app/ordenes` pinta sin clic/blur/focus, recarga correctamente, Network muestra `work-orders`, `customers` y `statuses` en `200`, filtros y paginación funcionan, Ver/Editar navegan correctamente y responsive desktop/tablet/móvil/DevTools no genera overflow global.
- Validar regresión visual rápida en `/app/clientes`, `/app/dashboard`, `/app/ordenes/nueva`, `/app/ordenes/{id}` y `/login` ante `401`.
- Siguen existiendo patrones mutables async fuera del alcance en pantallas de etiquetas de orden, edición de orden, pagos y algunos componentes secundarios; no se tocaron en esta tarea.

## 2026-07-03 - Corrección Admin Clientes Editar En Angular Zoneless

### Cambio Realizado

Se corrigió Admin > Clientes > Editar para que `/app/clientes/{id}/editar` cargue los datos del cliente en el formulario sin quedarse mostrando `Cargando cliente...`.

El componente de edición ahora usa Angular signals para el estado que controla renderizado y submit: `customer`, `isLoading`, `isSubmitting` y `errorMessage`.

### Causa Técnica

La auditoría confirmó el mismo patrón zoneless corregido previamente en Clientes listado/detalle/crear, Nueva orden, Dashboard y Detalle de orden: `CustomerEditPageComponent` mantenía `customer`, `isLoading`, `isSubmitting`, `loadErrorMessage` y `errorMessage` como propiedades mutables actualizadas dentro de `subscribe()` y `finalize()`.

En Angular sin `zone.js`, con `HttpClient` y `withFetch()`, esas mutaciones pueden no invalidar inmediatamente la vista aunque el `GET /api/customers/{id}` haya respondido, dejando visible `Cargando cliente...`.

### Flujo Auditado

- Ruta real: `/app/clientes/:id/editar`.
- Componente: `CustomerEditPageComponent`.
- Formulario hijo: `CustomerFormComponent`.
- Lectura de parámetro: `this.route.snapshot.paramMap.get('id')`.
- Servicio frontend: `CustomerService.getById(id)` y `CustomerService.update(id, request)`.
- Endpoint de carga: `GET /api/customers/{id}`.
- Endpoint de guardado: `PUT /api/customers/{id}`.
- Endpoint backend: `CustomerEndpoints.MapCustomerEndpoints()`.
- Servicio backend: `CustomerService.GetByIdAsync()` y `UpdateAsync()`.
- DTO de submit conservado: `CustomerUpsertRequest`.

### Respuestas De Auditoría

- `Cargando cliente...` en edición lo renderizaba `CustomerEditPageComponent`.
- La ruta real está declarada como `/app/clientes/:id/editar` bajo `/app`.
- El parámetro `id` se lee desde `ActivatedRoute.snapshot.paramMap`.
- Por código, el `GET /api/customers/{id}` se ejecuta mediante `CustomerService.getById(id)`.
- Backend expone `GET /api/customers/{id:guid}` y responde `CustomerDetailResponse` con `MapCustomerDetail(customer)` cuando existe.
- No se encontró mismatch de contrato frontend/backend como causa.
- El componente de edición sí usaba propiedades mutables actualizadas en `subscribe()`/`finalize()`.
- El loading se apagaba en success/error mediante `finalize()`, pero al ser propiedad mutable podía no repintar en modo zoneless.
- Existía error visible de carga (`loadErrorMessage`) para 404/genérico; quedó consolidado en `errorMessage` signal con mensajes controlados para 404, 403 y error genérico.
- `CustomerFormComponent` recibe correctamente el `customer` cargado y ejecuta `ngOnChanges()` para poblar el formulario.
- El submit conserva `CustomerUpsertRequest` y el `PUT /api/customers/{id}` existente.
- La diferencia principal frente a `CustomerDetailPageComponent` era que detalle ya usaba signals y edición no.

### Archivos Modificados

- `src/LaboratorioTlahuac.Web/src/app/features/customers/pages/customer-edit-page.component.ts`: migra estado renderizado a signals, actualiza template a lecturas `signal()`, mantiene errores controlados, conserva el servicio existente y navega al detalle con `NavigationExtras.info` transitorio tras guardar.
- `docs/PROJECT_STATUS.md` y `docs/IMPLEMENTATION_LOG.md`: documentan causa raíz, alcance, archivos modificados, validación y confirmaciones.

### Decisiones Técnicas

- No se modificó `CustomerFormComponent` porque sus inputs actuales funcionan para creación y edición; el padre ahora entrega valores desde signals.
- No se modificó `CustomerService` porque `GET /api/customers/{id}` y `PUT /api/customers/{id}` ya coinciden con backend y DTOs.
- La alerta de éxito post-update usa `NavigationExtras.info`, no `history.state`, para que no sobreviva a refresh.
- Se respetó el interceptor `401` existente: los componentes mantienen mensajes controlados como fallback y el redirect centralizado a `/login` sigue siendo responsabilidad del interceptor.
- No se usaron hacks de repintado como `setTimeout`, reload, `ApplicationRef.tick()`, clicks simulados ni `detectChanges()`.

### Validaciones Ejecutadas

- `git diff --check`: correcto.
- `npm run build` desde `src/LaboratorioTlahuac.Web`: correcto; reporta warning de presupuesto inicial excedido por 2.79 kB.
- `dotnet build`: correcto; 0 errores y 2 warnings `NU1903` conocidos por `SQLitePCLRaw.lib.e_sqlite3` 2.1.11 en el proyecto de tests.
- `dotnet test`: correcto; Domain 1/1, Application 1/1 y API 101/101.
- `git status --short`: ejecutado.
- `git diff --stat`: ejecutado.
- No existe script frontend `test` en `src/LaboratorioTlahuac.Web/package.json`; solo existen `ng`, `start`, `build` y `watch`.

### Confirmaciones

- No se modificó backend.
- No se modificó base de datos.
- No se crearon migraciones.
- No se cambiaron contratos API ni endpoints.
- No se modificaron auth, sesiones, cookies, CSRF/XSRF, guards ni interceptor `401`.
- No se agregó `zone.js` ni se quitó `withFetch()`.
- No se desplegó.
- No se hizo commit.

### Pendientes

- Validar manualmente en navegador real: `/app/clientes`, clic en `Editar`, carga de `/app/clientes/{id}/editar` sin clic manual, edición de un campo no crítico, guardado, navegación al detalle, regreso a listado, refresh directo de la URL de edición, consola sin errores y Network con `GET /api/customers/{id}` y `PUT /api/customers/{id}` correctos.
- Validar regresión manual rápida en `/app/clientes/nuevo`, `/app/clientes/{id}`, `/app/dashboard` y `/app/ordenes/nueva`.
- Continúan existiendo patrones de estado mutable async en otros módulos administrativos fuera del alcance, por ejemplo doctores internos, órdenes listado/edición, etiquetas y pagos.

## 2026-07-03 - Redirección A Login Por Sesión Expirada

### Cambio Realizado

Se implementó una redirección frontend centralizada a `/login` cuando cualquier request HTTP de Angular recibe `401 Unauthorized`, cubriendo el caso de sesión expirada mientras el usuario ya está dentro del layout privado.

El Dashboard conserva su mensaje local `Inicia sesion para consultar el dashboard.` como fallback, pero la UX principal ante sesión expirada pasa a ser navegación Angular a `/login` con `replaceUrl: true`.

### Auditoría

- No existía un interceptor HTTP registrado antes de esta tarea.
- `provideHttpClient` se configura en `src/LaboratorioTlahuac.Web/src/app/app.config.ts`.
- La app usa `withFetch()` y también `withXsrfConfiguration({ cookieName: 'XSRF-TOKEN', headerName: 'X-XSRF-TOKEN' })`.
- `AuthService.me()` y `AuthService.logout()` manejan `401` localmente para limpiar/normalizar sesión, pero no navegaban cuando una sesión expiraba dentro de una pantalla privada ya montada.
- `authGuard` y `permissionGuard` redirigen a `/login?returnUrl=...` cuando `ensureSession()` indica que no hay sesión o falla durante navegación a `/app/*`.
- `permissionGuard` redirige a `/app/access-denied` cuando hay sesión pero falta permiso.
- Cuando la sesión expiraba después de que el usuario ya estaba dentro del layout admin, el guard no volvía a ejecutarse necesariamente; el componente que hacía la llamada recibía el error y mostraba su mensaje local.
- Dashboard hacía `GET /api/dashboard/summary` y convertía `401` en `Inicia sesion para consultar el dashboard.` dentro de `DashboardPageComponent.toErrorMessage()`.
- Otros módulos administrativos tienen manejo local de `HttpErrorResponse` y mensajes para `403` o errores genéricos: clientes, órdenes, etiquetas y pagos. Esos mensajes quedan como fallback/local UX; el redirect de `401` queda centralizado.
- El punto mínimo y seguro para centralizar el redirect fue un `HttpInterceptorFn` registrado en `provideHttpClient`, preservando `withFetch()` y XSRF.

### Archivos Modificados

- `src/LaboratorioTlahuac.Web/src/app/core/interceptors/auth-expired.interceptor.ts`: nuevo interceptor funcional; detecta `HttpErrorResponse` con `status === 401`, evita navegar si ya está en `/login`, evita redirects simultáneos con una bandera local y ejecuta `router.navigate(['/login'], { replaceUrl: true })`.
- `src/LaboratorioTlahuac.Web/src/app/app.config.ts`: registra `withInterceptors([authExpiredInterceptor])` sin remover `withFetch()` ni `withXsrfConfiguration()`.
- `docs/PROJECT_STATUS.md` y `docs/IMPLEMENTATION_LOG.md`: documentan alcance, decisión técnica, validación y restricciones respetadas.

### Decisiones Técnicas

- Se usó interceptor funcional porque la app ya configura `provideHttpClient` en `app.config.ts` y Angular 21 soporta `HttpInterceptorFn`.
- No se implementó `returnUrl` en el interceptor porque el requerimiento explícito fue redirigir solo a `/login`; el patrón existente de `returnUrl` se mantiene en guards para navegación inicial a rutas privadas.
- No se trata `403` como sesión expirada; los componentes y guards mantienen los mensajes o redirecciones de no autorizado.
- No se cambió Dashboard porque su `401` local queda como fallback si la navegación no alcanza a completarse.
- No se usó `window.location.href`, `setTimeout`, reloads, eventos simulados ni cambios de backend.

### Validaciones Ejecutadas

- `git diff --check`: correcto.
- `npm run build` desde `src/LaboratorioTlahuac.Web`: correcto; reporta warning de presupuesto inicial excedido por 2.39 kB.
- `dotnet build`: correcto; se mantienen 2 warnings `NU1903` conocidos por `SQLitePCLRaw.lib.e_sqlite3` 2.1.11 en el proyecto de tests.
- `dotnet test`: correcto; Domain 1/1, Application 1/1 y API 101/101.
- `git status --short`: ejecutado.
- `git diff --stat`: ejecutado.
- No existe script frontend `test` en `src/LaboratorioTlahuac.Web/package.json`; solo existen `ng`, `start`, `build` y `watch`.

### Confirmaciones

- No se modificó backend.
- No se modificó base de datos.
- No se crearon migraciones.
- No se cambiaron contratos API ni endpoints.
- No se modificaron cookies, sesiones, CSRF/XSRF ni configuración de seguridad backend.
- No se modificaron `AuthService`, guards ni `returnUrl` existente.
- No se quitó `withFetch()`.
- No se desplegó.
- No se hizo commit.

### Pendientes

- Validar manualmente en navegador real: iniciar sesión, abrir `/app/dashboard`, expirar o eliminar cookie de sesión desde DevTools, provocar un request protegido con `401`, confirmar URL final `/login`, confirmar que no hay loop y que `/login` carga correctamente.
- Validar regresión en navegador real: login, logout, Dashboard, Clientes y Órdenes.
- Validar con usuario autenticado sin permiso que un `403` sigue mostrando no autorizado o `/app/access-denied` y no redirige a login.
- En una fase posterior se pueden revisar mensajes locales redundantes de `401` en componentes si se quiere limpiar fallback visual, pero no es necesario para la UX principal.

## 2026-07-03 - Corrección Admin Órdenes Detalle Y Formato MXN

### Cambio Realizado

Se corrigieron dos problemas en Admin > Órdenes:

- Después de crear una orden desde `/app/ordenes/nueva`, la navegación a `/app/ordenes/{id}` podía quedar mostrando `Cargando orden...` aunque el detalle ya hubiera sido recibido por el frontend.
- El campo `Costo total` del formulario de nueva/edición de orden dejó de mostrarse como número plano y ahora usa formato moneda MXN en la UI sin cambiar el valor numérico enviado al backend.

### Causa Técnica

Para `Cargando orden...`, la auditoría encontró el mismo patrón zoneless ya corregido en Clientes, Nueva orden y Dashboard: `WorkOrderDetailPageComponent` pintaba `order`, `isLoading`, `errorMessage`, `statuses`, `isChangingStatus` y `statusErrorMessage` desde propiedades mutables actualizadas dentro de `subscribe()`/`finalize()`. En Angular 21 sin `zone.js`, con `HttpClient` usando `withFetch()`, esas mutaciones pueden no invalidar inmediatamente la vista.

Para `Costo total`, el formulario usaba `<input type="number" formControlName="totalAmount">`, por lo que la UI no tenía formato de moneda. El contrato ya era correcto: TypeScript `totalAmount?: number | null` y backend `decimal? TotalAmount`.

### Flujo Auditado

- Ruta de nueva orden: `/app/ordenes/nueva`.
- Componente de creación: `WorkOrderCreatePageComponent`.
- Formulario: `WorkOrderFormComponent`.
- Submit: `WorkOrderUpsertRequest` se arma desde `this.form.getRawValue()`; `totalAmount` sale de `value.totalAmount`.
- Servicio frontend: `WorkOrderService.create()` hace `POST /api/work-orders`; `WorkOrderService.getById()` hace `GET /api/work-orders/{id}`.
- Navegación post-create: después del POST exitoso se navega a `/app/ordenes/{id}`.
- Ruta de detalle: `/app/ordenes/:id`.
- Componente de detalle: `WorkOrderDetailPageComponent`, que renderiza `Cargando orden...`.
- Endpoint backend: `WorkOrderEndpoints.MapWorkOrderEndpoints()` expone `POST /api/work-orders` y `GET /api/work-orders/{id}`.
- Servicio backend: `WorkOrderService.CreateAsync()` y `GetByIdAsync()` devuelven `WorkOrderDetailResponse` usando `MapDetail()`.

### Respuestas De Auditoría

- El texto `Cargando orden...` lo renderiza `WorkOrderDetailPageComponent`.
- Después del POST de creación, la app navega a `/app/ordenes/{id}`.
- Por código, el GET de detalle se ejecuta en `ngOnInit()` mediante `WorkOrderService.getById(id)`.
- Backend responde el mismo DTO de detalle para create y get-by-id; no se encontró mismatch de contrato como causa.
- El detalle usaba propiedades mutables actualizadas dentro de `subscribe()`/`finalize()`.
- `isLoading` se apagaba en `finalize()` para success y error, pero al ser propiedad mutable podía no repintar en zoneless.
- Sí existía `errorMessage` visible para 404, 403 y error genérico.
- No se encontró mismatch entre `WorkOrderDetailResponse.TotalAmount decimal?` y `WorkOrderDetail.totalAmount number | null`.
- El formulario usa Reactive Forms con `FormGroup`/`FormControl`; no usa `ngModel`.
- `WorkOrderUpsertRequest` se construye en `WorkOrderFormComponent.submit()` desde `getRawValue()`.
- El campo `Costo total` corresponde a `totalAmount` en frontend y `TotalAmount` en backend.
- Actualmente se envía como `number | null`, no como string.

### Archivos Modificados

- `src/LaboratorioTlahuac.Web/src/app/features/orders/pages/work-order-detail-page.component.ts`: `order`, `statuses`, `isLoading`, `errorMessage`, `successMessage`, `isChangingStatus` y `statusErrorMessage` pasan a Angular signals; el template lee con `signal()`. Carga y cambio de estado mantienen `finalize()` para apagar loading/submitting y errores controlados.
- `src/LaboratorioTlahuac.Web/src/app/features/orders/pages/work-order-create-page.component.ts`: la navegación post-create usa `NavigationExtras.info` con `Orden creada correctamente.` para mensaje flash transitorio, sin usar `history.state`.
- `src/LaboratorioTlahuac.Web/src/app/features/orders/components/work-order-form.component.ts`: el campo `Costo total` usa display local con formato MXN al blur y valor editable simple al focus; el `FormControl` real conserva `number | null`.
- `docs/PROJECT_STATUS.md` y `docs/IMPLEMENTATION_LOG.md`: se documenta la causa raíz, alcance, archivos modificados, validación y confirmaciones.

### Decisiones Técnicas

- No se modificó `WorkOrderService` porque los endpoints, rutas y DTOs ya coincidían.
- No se cambió backend porque `POST /api/work-orders` ya devuelve `WorkOrderDetailResponse` y `GET /api/work-orders/{id}` usa el mismo mapeo.
- No se agregó una librería de currency mask; se implementaron helpers locales con `Intl.NumberFormat('es-MX', { style: 'currency', currency: 'MXN' })`.
- No se guarda el string formateado en el `FormControl`; el submit conserva `totalAmount` como número o `null`.
- No se usaron hacks de repintado como `setTimeout`, `window.location.reload()`, `ApplicationRef.tick()`, clicks simulados, eventos manuales ni `detectChanges()` indiscriminado.

### Validaciones Ejecutadas

- `git diff --check`: correcto.
- `npm run build` desde `src/LaboratorioTlahuac.Web`: correcto; reporta warning de presupuesto inicial excedido por 1.99 kB.
- `dotnet build`: correcto; se mantienen 2 warnings `NU1903` conocidos por `SQLitePCLRaw.lib.e_sqlite3` 2.1.11 en el proyecto de tests.
- `dotnet test`: correcto; Domain 1/1, Application 1/1 y API 101/101.
- `git status --short`: ejecutado.
- `git diff --stat`: ejecutado.
- No existe script frontend `test` en `src/LaboratorioTlahuac.Web/package.json`; solo existen `ng`, `start`, `build` y `watch`.

### Confirmaciones

- No se modificó backend.
- No se modificó base de datos.
- No se crearon migraciones.
- No se cambiaron contratos API ni DTOs backend.
- No se modificaron `AuthService`, guards, cookies, sesiones, CSRF/XSRF ni permisos.
- No se agregó `zone.js` ni se quitó `withFetch()`.
- No se desplegó.
- No se hizo commit.

### Pendientes

- Validar manualmente en navegador real: crear orden desde `/app/ordenes/nueva`, confirmar POST exitoso, navegación a `/app/ordenes/{id}`, detalle pintado sin clic manual, refresh correcto, mensaje flash sin reaparecer tras refresh, consola sin errores y Network sin retries inesperados.
- Validar regresión rápida en navegador real: `/app/clientes`, `/app/dashboard` y `/app/ordenes/nueva`.
- Continúan existiendo patrones de estado mutable async en otros módulos administrativos como órdenes listado/edición, etiquetas y pagos; no se corrigieron en esta tarea para mantener el alcance.

## 2026-07-02 - Corrección Admin Dashboard En Angular Zoneless

### Cambio Realizado

Se corrigió el bug de Admin > Dashboard donde `/app/dashboard` podía quedar mostrando `Cargando dashboard...` aunque el componente ya tuviera `finalize()` para apagar el estado de carga.

La causa técnica encontrada fue estado async renderizado como propiedades mutables (`summary`, `isLoading`, `errorMessage`) actualizado dentro de `subscribe()`/`finalize()` en Angular 21 sin `zone.js`, con `HttpClient` usando `withFetch()`. En ese modo, la respuesta HTTP o el error pueden actualizar el estado TypeScript sin invalidar inmediatamente la vista.

### Flujo Auditado

- Ruta frontend: `/app/dashboard`.
- Componente: `DashboardPageComponent`.
- Servicio frontend: `DashboardService.getSummary()`.
- Endpoint consumido: `GET /api/dashboard/summary`.
- Endpoint backend: `DashboardEndpoints.MapDashboardEndpoints()`.
- Servicio backend: `IDashboardService.GetSummaryAsync()` implementado por `DashboardService` en infraestructura.
- Permiso requerido en frontend/backend: `reports.view`.

### Archivos Modificados

- `src/LaboratorioTlahuac.Web/src/app/features/dashboard/dashboard-page.component.ts`: `summary`, `isLoading` y `errorMessage` pasan a Angular signals; el template lee con `summary()`, `isLoading()` y `errorMessage()`. La carga conserva `timeout(15000)`, muestra error controlado y apaga loading en `finalize()` para success y error.
- `docs/PROJECT_STATUS.md` y `docs/IMPLEMENTATION_LOG.md`: se documenta corrección, causa real, alcance, validación y pendientes.

### Decisiones Técnicas

- Se mantuvo `GET /api/dashboard/summary` y el contrato `DashboardSummary` sin cambios porque el modelo TypeScript coincide con los records C#.
- No se modificó backend en esta tarea: la auditoría encontró oportunidades de performance por listas materializadas antes de ordenar/tomar resultados, pero no evidencia de contrato incompatible, endpoint no autorizado o error backend como causa directa del loader infinito.
- No se agregó `zone.js`, no se quitó `withFetch()` y no se usaron hacks de repintado como `setTimeout`, `window.location.reload()`, `ApplicationRef.tick()`, clicks simulados, eventos manuales ni `detectChanges()` indiscriminado.

### Validaciones Ejecutadas

- `git diff --check`: correcto.
- `npm run build` desde `src/LaboratorioTlahuac.Web`: correcto.
- `dotnet build`: correcto; se mantienen warnings `NU1903` conocidos por `SQLitePCLRaw.lib.e_sqlite3` 2.1.11 en proyectos de prueba.
- `dotnet test`: correcto.
- `git status --short`: deja modificados solo el componente de Dashboard y los dos documentos obligatorios.
- `git diff --stat`: ejecutado.
- No existe script frontend `test` en `src/LaboratorioTlahuac.Web/package.json`; solo existen `ng`, `start`, `build` y `watch`.

### Confirmaciones

- No se modificó backend.
- No se modificó base de datos.
- No se crearon migraciones.
- No se cambiaron contratos API ni endpoints.
- No se modificaron `AuthService`, guards, cookies, sesiones, CSRF/XSRF ni permisos.
- No se hizo commit.

### Pendientes

- Validar manualmente en navegador real: abrir `/app/dashboard`, confirmar que sale de `Cargando dashboard...`, pinta métricas/resumen sin clic, recargar y revisar consola/Network.
- Revisar en una fase posterior las oportunidades de performance en `DashboardService`: ordenar/tomar en base de datos para `latestWorkOrders`, `dueSoonWorkOrders` y `latestPayments`, y evaluar agregados financieros sin materializar todas las órdenes/pagos cuando crezca el volumen.
- Continúan existiendo patrones de estado mutable async en otros módulos administrativos como pagos y órdenes listado/detalle/edición; no se corrigieron en esta tarea para mantener el alcance.

## 2026-07-02 - Corrección Admin Órdenes Nueva Orden En Angular Zoneless

### Cambio Realizado

Se corrigió el bug de Admin > Órdenes > Nueva orden donde el select `Cliente` no mostraba los clientes activos cargados inicialmente en `/app/ordenes/nueva` hasta que el usuario hacía clic en la pantalla y volvía a abrir el desplegable.

La causa técnica documentada fue el mismo patrón observado en Clientes: estado async renderizado como propiedades mutables actualizado dentro de `subscribe()`/`finalize()` en Angular 21 sin `zone.js`, con `HttpClient` usando `withFetch()`. En ese modo, la respuesta HTTP puede actualizar el estado TypeScript sin invalidar inmediatamente la vista hasta un evento DOM posterior.

### Archivos Modificados

- `src/LaboratorioTlahuac.Web/src/app/features/orders/components/work-order-form.component.ts`: `customers`, `internalDoctors`, `selectedCustomer`, `isLoadingCustomers`, `isLoadingDoctors` y `localErrorMessage` pasan a Angular signals; el template lee con `customers()`, `selectedCustomer()`, `internalDoctors()` y flags `signal()`. Se conserva la carga de clientes activos con `CustomerService.list({ isActive: true, pageSize: 100 })`, la selección, validaciones, doctores internos y el contrato de submit.
- `src/LaboratorioTlahuac.Web/src/app/features/orders/pages/work-order-create-page.component.ts`: `isSubmitting` y `errorMessage` pasan a signals para que el estado renderizado del POST de creación sea reactivo en modo zoneless; se conserva `POST /api/work-orders` y la navegación al detalle.
- `docs/PROJECT_STATUS.md` y `docs/IMPLEMENTATION_LOG.md`: se documenta corrección, causa técnica, alcance y validación.

### Decisiones Técnicas

- Se usaron Angular signals porque integran el estado async con el sistema reactivo de Angular zoneless y hacen que las lecturas de template invaliden la vista al recibir clientes activos.
- No se movió la carga al padre ni se modificó `CustomerService` o `WorkOrderService`; los endpoints y DTOs ya eran correctos.
- No se agregó `zone.js`, no se quitó `withFetch()` y no se usaron hacks de repintado como `setTimeout`, `window.location.reload()`, `ApplicationRef.tick()`, clicks simulados, eventos manuales ni `detectChanges()` indiscriminado.

### Validaciones Ejecutadas

- `npm run build` desde `src/LaboratorioTlahuac.Web`: correcto.
- `dotnet build`: correcto, 0 errores; reportó 2 warnings `NU1903` conocidos por vulnerabilidad de `SQLitePCLRaw.lib.e_sqlite3` 2.1.11 en el proyecto de tests.
- `dotnet test`: primer intento en paralelo con `dotnet build` falló por bloqueo transitorio de archivos generados; repetido secuencialmente fue correcto con Domain 1/1, Application 1/1 y API 101/101.
- `git diff --check`: correcto.
- `git status --short`: solo quedan modificados los dos archivos frontend de Órdenes y los dos documentos obligatorios.
- `git diff --stat`: 4 archivos modificados, 129 inserciones y 64 eliminaciones.
- No existe script `test` en `src/LaboratorioTlahuac.Web/package.json`, por lo que no hay runner frontend no interactivo adicional que ejecutar.

### Confirmaciones

- No se modificó backend.
- No se modificó base de datos.
- No se crearon migraciones.
- No se cambiaron contratos API ni endpoints.
- No se modificaron `AuthService`, guards, cookies, sesiones, CSRF/XSRF ni permisos.
- No se agregó `zone.js` ni se quitó `withFetch()`.
- No se descartaron cambios previos del working tree.
- No se hizo commit.

### Pendientes

- Ejecutar la prueba manual en navegador real: abrir `/app/ordenes/nueva`, abrir inmediatamente el select `Cliente`, confirmar `Selecciona un cliente`, `Cliente activo tipo Clínica` y `Cliente activo tipo Doctor`, recargar y repetir, seleccionar cliente, validar campos mínimos y revisar consola/Network.
- Se detectan patrones similares de estado mutable async en otros módulos administrativos como órdenes listado/detalle/edición, dashboard y pagos; no se corrigieron en esta tarea para mantener el alcance solicitado.

## 2026-07-02 - Corrección Admin Clientes En Angular Zoneless

### Cambio Realizado

Se corrigió el bug de Admin > Clientes donde, después de crear un cliente, la UI podía quedar en `Cargando cliente...` y solo repintar después de un clic del usuario.

La causa técnica documentada fue estado mutable no reactivo actualizado dentro de `subscribe()`/`finalize()` en una app Angular 21 operando sin `zone.js`, con `HttpClient` usando `withFetch()`. En ese modo, mutar propiedades normales desde callbacks async puede no disparar el repintado hasta un evento DOM posterior.

### Archivos Modificados

- `src/LaboratorioTlahuac.Web/src/app/features/customers/pages/customer-create-page.component.ts`: `isSubmitting` y `errorMessage` pasan a signals; el submit conserva `POST /api/customers` y navega al detalle con `NavigationExtras.info` transitorio para `successMessage`.
- `src/LaboratorioTlahuac.Web/src/app/features/customers/pages/customer-detail-page.component.ts`: `customer`, `isLoading`, `errorMessage` y `successMessage` pasan a signals; el template lee con sintaxis `signal()`, la carga directa por URL conserva `GET /api/customers/{id}` y el mensaje de éxito solo se lee desde `Router.currentNavigation()?.extras.info`.
- `src/LaboratorioTlahuac.Web/src/app/features/customers/pages/customer-list-page.component.ts`: listado, carga, error, paginación y totales renderizados pasan a signals/computed; búsqueda, filtros y paginación conservan el contrato existente.
- `src/LaboratorioTlahuac.Web/src/app/features/customers/components/customer-form.component.ts`: el input `errorMessage` acepta `string | null` para recibir signals sin romper type-checking.
- `src/LaboratorioTlahuac.Web/src/styles.scss`: se agrega `.alert-success` para una confirmación visible y sobria.
- `docs/PROJECT_STATUS.md` y `docs/IMPLEMENTATION_LOG.md`: se documenta corrección, causa, alcance y validación.

### Decisiones Técnicas

- Se usaron Angular signals porque integran el estado async con el sistema reactivo de Angular zoneless y hacen que las lecturas de template (`customer()`, `isLoading()`, `errorMessage()`) invaliden la vista correctamente.
- El mensaje `Cliente creado correctamente.` usa `NavigationExtras.info` en vez de `state` para que sea información transitoria de la navegación inmediata y no persista en `history.state` tras recargar `/app/clientes/{id}`.
- No se agregó `zone.js`, no se quitó `withFetch()` y no se usaron hacks de repintado como `setTimeout`, `window.location.reload()`, `ApplicationRef.tick()`, clicks simulados, eventos manuales ni `detectChanges()` indiscriminado.
- No se modificó `CustomerService` porque los contratos HTTP y endpoints ya eran correctos; el problema estaba en el estado del frontend.

### Validaciones Ejecutadas

- `git diff --check`: correcto.
- `npm run build` desde `src/LaboratorioTlahuac.Web`: correcto.
- `dotnet build`: correcto, 0 errores; reportó 2 warnings `NU1903` conocidos por vulnerabilidad de `SQLitePCLRaw.lib.e_sqlite3` 2.1.11 en el proyecto de tests.
- `dotnet test`: correcto; Domain 1/1, Application 1/1 y API 101/101.
- No existe script `test` en `src/LaboratorioTlahuac.Web/package.json`, por lo que no hay runner frontend no interactivo adicional que ejecutar.

### Confirmaciones

- No se modificó backend.
- No se modificó base de datos.
- No se crearon migraciones.
- No se cambiaron contratos API ni endpoints.
- No se modificaron `AuthService`, guards, cookies, sesiones, CSRF/XSRF ni permisos.
- No se hizo commit.

### Pendientes

- Ejecutar la prueba manual en navegador real: abrir `/app/clientes`, confirmar listado sin clic, crear cliente de prueba, confirmar `POST /api/customers`, ver navegación a `/app/clientes/{id}`, confirmar `Cliente creado correctamente.`, recargar el detalle por URL directa, volver al listado, revisar consola y Network.
- Se detectan patrones similares de estado mutable async fuera de Clientes en otros módulos administrativos; no se corrigieron en esta tarea para mantener el alcance solicitado.

## 2026-07-02 - Fase 3.2.1 QA Técnico De Etiquetas Y Preparación DEV

### Cambio Realizado

Se ejecutó QA técnico/documental de Fase 3.2.1 para el MVP de impresión de etiquetas desde órdenes existentes y se preparó la lista de validación para despliegue a VPS DEV desde rama `dev`.

No se implementó funcionalidad nueva. No se agregaron QR/barcode, PDF, integración directa con driver/SDK de impresora, endpoints, migraciones ni dependencias.

### Validación Técnica

Confirmado por código y búsquedas:

- `/app/ordenes/:id/etiqueta-trabajo` vive bajo `/app`, hereda `authGuard` y requiere `orders.view`.
- `/app/ordenes/:id/etiqueta-entrega` vive bajo `/app`, hereda `authGuard` y requiere `orders.view`.
- `/login` sigue público.
- `/app` y `/app/dashboard` siguen privados.
- `/dashboard` no es ruta privada real.
- Las etiquetas usan `WorkOrderService.getById()` y el modelo existente `WorkOrderDetail`.
- No se agregaron endpoints; el frontend sigue usando `/api/work-orders/{id}`.
- No se agregaron migraciones; las migraciones existentes siguen siendo las históricas hasta `20260509053231_AddPayments`.
- `package.json` no incluye dependencias nuevas para etiquetas.
- Los botones `Imprimir` usan `window.print()`.
- `@media print` oculta navegación, topbar, encabezado de pantalla y acciones.
- Existe `@page` para 76 x 51 mm y 102 x 51 mm.
- Las etiquetas usan texto negro, fondo blanco y bordes; no dependen de color para ser legibles.
- Textos largos se compactan o restringen con `compact()`, `overflow`, `text-overflow` y `-webkit-line-clamp`.
- Dirección/contacto faltantes usan textos seguros `Dirección pendiente` y `Contacto pendiente`; no se inventan datos.

### Validación Visual

No se ejecutó validación visual automatizada local porque no hay navegador/headless disponible sin instalar dependencias:

- `chromium`: no disponible en `PATH`.
- `google-chrome`: no disponible en `PATH`.
- `chromium-browser`: no disponible en `PATH`.
- El frontend no declara Playwright ni Puppeteer.

Queda pendiente abrir las rutas en DEV con navegador real, probar `Imprimir` y confirmar etiqueta física con impresora térmica.

### Checklist Físico Preparado

Se actualizó `docs/08-qa/label-printing-qa.md` con:

- Rutas validadas.
- Tamaños 76 x 51 mm y 102 x 51 mm.
- Datos incluidos por etiqueta.
- Limitaciones conocidas.
- Checklist manual con navegador.
- Checklist de prueba física con impresora térmica.
- Ajustes esperados de navegador/driver: escala 100%, sin encabezado/pie, tamaño de papel personalizado, orientación, calibración de offset, densidad/velocidad y rollo.
- Resultado de build/test.
- Pendiente explícito de prueba física en DEV.

### Archivos Modificados

- `docs/08-qa/label-printing-qa.md`
- `docs/01-product/label-printing.md`
- `docs/PROJECT_STATUS.md`
- `docs/ROADMAP.md`
- `docs/IMPLEMENTATION_LOG.md`

### Validaciones Ejecutadas

- `git status --short` antes de editar: sin salida.
- `git diff --stat` antes de editar: sin salida.
- `npm run build` desde `src/LaboratorioTlahuac.Web`: correcto.
- `dotnet build`: correcto, 0 errores; reportó 2 warnings `NU1903` conocidos por vulnerabilidad de `SQLitePCLRaw.lib.e_sqlite3` 2.1.11 en el proyecto de tests.
- `dotnet test`: correcto; Domain 1/1, Application 1/1 y API 101/101.
- `rg "/app/ordenes" src docs README.md`: ejecutado.
- `rg "etiqueta" src docs README.md`: ejecutado.
- `rg "@page" src/LaboratorioTlahuac.Web/src`: ejecutado.
- `rg "window.print" src/LaboratorioTlahuac.Web/src`: ejecutado.
- `rg "/dashboard" .`: ejecutado; no se convirtió `/dashboard` en ruta privada real.
- `rg "/app/dashboard" .`: ejecutado; confirma que el dashboard privado real sigue bajo `/app/dashboard`.
- `rg "/login" src/LaboratorioTlahuac.Web/src/app docs README.md AGENTS.md`: ejecutado; confirma `/login` como entrada pública.
- `rg --files-with-matches "LT_ADMIN_PASSWORD" .`: ejecutado con salida limitada a archivos para no imprimir valores.
- `rg --files-with-matches "LT_QA_LIMITED_PASSWORD" .`: ejecutado con salida limitada a archivos para no imprimir valores.
- `rg --files-with-matches "LDT_SQL_SA_PASSWORD" .`: ejecutado con salida limitada a archivos para no imprimir valores.
- `rg --files-with-matches "ConnectionStrings" src docs README.md`: ejecutado con salida limitada a archivos para no imprimir valores.
- `rg "codex-cobranza-sql" docs README.md AGENTS.md`: ejecutado; solo hay menciones históricas/documentales de no uso.
- `git diff --check`: correcto después de actualizar documentación.

### Confirmaciones

- No se instaló ninguna dependencia.
- No se crearon migraciones.
- No se agregaron endpoints.
- No se modificó backend.
- No se modificó `AuthService`.
- No se modificaron guards.
- No se tocaron cookies ni XSRF.
- No se cambiaron rutas privadas.
- No se convirtió `/dashboard` en ruta privada real.
- No se imprimieron secretos.
- No se ejecutó `dotnet user-secrets list`.
- No se usó `codex-cobranza-sql`.
- No se hicieron commits.

### Resultado

No hay hallazgos técnicos bloqueantes para recomendar commit/push a `dev` y despliegue a VPS DEV. La aceptación final de la fase debe cerrarse con prueba física de etiqueta interna 76 x 51 mm y etiqueta entrega 102 x 51 mm en impresora térmica real.

## 2026-07-02 - Fase 3.2 MVP Impresión De Etiquetas Desde Órdenes

### Cambio Realizado

Se implementó el MVP de impresión de etiquetas desde órdenes existentes bajo `/app/ordenes`, sin crear panel duplicado.

Desde el detalle `/app/ordenes/:id` se agregaron acciones:

- `Etiqueta interna`
- `Etiqueta entrega`

Rutas privadas nuevas:

- `/app/ordenes/:id/etiqueta-trabajo`
- `/app/ordenes/:id/etiqueta-entrega`

Ambas rutas viven bajo `/app`, heredan autenticación de la zona privada y usan `permissionGuard` con `orders.view`.

### Implementación Técnica

- Se crearon páginas standalone dentro del feature de órdenes:
  - `WorkOrderJobLabelPageComponent`
  - `WorkOrderDeliveryLabelPageComponent`
- Ambas reutilizan `WorkOrderService.getById()` y `GET /api/work-orders/{id}`.
- Cada pantalla maneja carga, error, no encontrado y sin permiso.
- Cada pantalla incluye botón `Imprimir` con `window.print()` y botón `Volver a la orden`.
- El CSS usa `@media print`, `@page`, tamaños en milímetros, alto contraste y oculta navegación/topbar/botones al imprimir.
- La etiqueta interna usa tamaño objetivo 76 x 51 mm.
- La etiqueta de entrega usa tamaño objetivo 102 x 51 mm.

### Datos Incluidos

Etiqueta interna:

- LDT.
- Texto `Etiqueta interna`.
- Folio / número de orden.
- Cliente.
- Doctor interno si existe.
- Paciente.
- Fecha de recepción.
- Fecha de entrega.
- Estado.
- Color si existe.
- Trabajo solicitado.
- Observaciones breves si existen.

Etiqueta entrega:

- LDT.
- Texto `Entrega`.
- Folio / número de orden.
- Cliente.
- Paciente o referencia.
- Trabajo solicitado.
- Fecha de entrega.
- Estado.
- `Dirección pendiente`.
- `Contacto pendiente`.
- `Recibe: __________________`.
- `Firma: __________________`.

### Limitaciones Documentadas

- El detalle actual de orden no incluye dirección/contacto completos del cliente.
- No se consulta `GET /api/customers/{id}` desde la etiqueta para no exigir `customers.view` en rutas cuyo permiso es `orders.view`.
- No se implementó QR/barcode.
- No se implementó PDF.
- No se implementó impresión directa por driver/SDK.
- No se implementó etiqueta chica 51 x 25 mm.
- No se implementó repartidor asignado, evidencia de entrega, firma digital ni foto.
- La prueba física con impresora térmica real queda pendiente en DEV.

### Archivos Creados

- `src/LaboratorioTlahuac.Web/src/app/features/orders/pages/work-order-job-label-page.component.ts`
- `src/LaboratorioTlahuac.Web/src/app/features/orders/pages/work-order-job-label-page.component.scss`
- `src/LaboratorioTlahuac.Web/src/app/features/orders/pages/work-order-delivery-label-page.component.ts`
- `src/LaboratorioTlahuac.Web/src/app/features/orders/pages/work-order-delivery-label-page.component.scss`
- `docs/08-qa/label-printing-qa.md`

### Archivos Modificados

- `README.md`
- `docs/README.md`
- `docs/PROJECT_STATUS.md`
- `docs/ROADMAP.md`
- `docs/IMPLEMENTATION_LOG.md`
- `docs/01-product/admin-catalog-management.md`
- `docs/01-product/internal-system.md`
- `docs/01-product/label-printing.md`
- `docs/01-product/operations-orders-delivery.md`
- `docs/03-architecture/ARCHITECTURE.md`
- `docs/03-architecture/AUTH_FLOW.md`
- `docs/03-architecture/frontend-architecture.md`
- `src/LaboratorioTlahuac.Web/src/app/app.routes.ts`
- `src/LaboratorioTlahuac.Web/src/app/features/orders/pages/work-order-detail-page.component.ts`

### Validaciones Ejecutadas

- `git status --short` antes de editar: sin salida.
- `git diff --stat` antes de editar: sin salida.
- `npm run build` desde `src/LaboratorioTlahuac.Web`: correcto.
- `dotnet build`: correcto, 0 errores; reportó 2 warnings `NU1903` conocidos por vulnerabilidad de `SQLitePCLRaw.lib.e_sqlite3` 2.1.11 en el proyecto de tests.
- `dotnet test`: correcto; Domain 1/1, Application 1/1 y API 101/101.
- `git diff --check`: correcto.
- `rg "/app/ordenes" src docs README.md`: ejecutado.
- `rg "etiqueta" src docs README.md`: ejecutado.
- `rg "@page" src/LaboratorioTlahuac.Web/src`: ejecutado.
- `rg "window.print" src/LaboratorioTlahuac.Web/src`: ejecutado.
- `rg "/dashboard" .`: ejecutado; no se creó `/dashboard` como ruta privada real.
- `rg "/app/dashboard" .`: ejecutado; confirma que el dashboard privado real sigue bajo `/app/dashboard`.
- `rg "/login" src/LaboratorioTlahuac.Web/src/app docs README.md AGENTS.md`: ejecutado; confirma `/login` como ruta pública/entrada.
- `rg -l "LT_ADMIN_PASSWORD" .`: ejecutado con salida limitada a archivos para no imprimir valores.
- `rg -l "LT_QA_LIMITED_PASSWORD" .`: ejecutado con salida limitada a archivos para no imprimir valores.
- `rg -l "LDT_SQL_SA_PASSWORD" .`: ejecutado con salida limitada a archivos para no imprimir valores.
- `rg "ConnectionStrings" src docs README.md`: ejecutado; solo se revisaron claves/menciones documentales o de configuración.
- `rg "codex-cobranza-sql" docs README.md AGENTS.md`: ejecutado; solo hay menciones históricas/documentales de no uso.

### Confirmaciones

- No se instaló ninguna dependencia.
- No se crearon migraciones.
- No se agregaron endpoints.
- No se modificó backend.
- No se modificó `AuthService`.
- No se modificaron `auth.guard.ts` ni `permission.guard.ts`.
- No se tocaron cookies ni XSRF.
- No se tocó deploy.
- No se imprimieron secretos.
- No se ejecutó `dotnet user-secrets list`.
- No se usó `codex-cobranza-sql`.
- No se hicieron commits.
- `/login` sigue público.
- `/app` y `/app/dashboard` siguen privados.
- `/dashboard` no es ruta privada real.

### Siguiente Fase Recomendada

Validar Fase 3.2 en DEV con impresora térmica real, confirmar escala/márgenes/orientación en navegador y avanzar con usuarios/roles como habilitador previo de reparto. Esta recomendación quedó actualizada posteriormente por Fase 3.3.

## 2026-07-02 - Fase 3.1 Análisis Operativo De Órdenes, Etiquetas Y Reparto

### Cambio Realizado

Se ejecutó Fase 3.1 como análisis/documentación para el siguiente frente operativo de Laboratorio Dental Tláhuac:

- Seguimiento de órdenes/pedidos de clientes.
- Impresión de etiquetas para trabajos recibidos.
- Impresión de etiquetas para entrega/repartidor.
- Reducción de errores de entrega.
- Trazabilidad futura de quién entregó, cuándo, a qué cliente y quién recibió.
- Priorización futura de usuarios/roles y catálogo.

No se implementó código. No se modificó frontend funcional, backend, `AuthService`, guards, cookies, XSRF, endpoints, base de datos, migraciones, dependencias ni deploy. No se hicieron commits.

### Hallazgos Documentados

- `/app/ordenes` ya existe como módulo real de órdenes y no debe duplicarse con otro panel.
- Las rutas reales actuales son `/app/ordenes`, `/app/ordenes/nueva`, `/app/ordenes/:id` y `/app/ordenes/:id/editar`.
- El modelo actual de orden cubre folio, cliente, doctor interno, paciente, trabajo, color, fechas, estado, total, observaciones, historial y pagos.
- `DeliveryDate` es fecha planeada/capturada de entrega; no es fecha/hora real de entrega ni evidencia.
- Faltan datos de reparto: repartidor asignado, salida a ruta, receptor, fecha/hora real de entrega, intento fallido, motivo, observaciones de entrega y evidencia.
- Los datos completos de cliente existen en clientes, pero el detalle actual de orden no incluye dirección/contacto completos.
- La administración de usuarios/roles sigue como placeholder; existe modelo y permisos, pero no CRUD funcional.
- El catálogo público sigue en `catalog-data.ts`; administración privada de catálogo permanece como backlog.

### Diseño Operativo

Se documentó el flujo futuro:

1. Recepción de trabajo y creación de orden.
2. Impresión de etiqueta interna y pegado al trabajo físico.
3. Seguimiento interno por estado, fechas, observaciones y pagos/saldos.
4. Salida a repartidor con etiqueta de entrega.
5. Entrega mobile-first con captura de `Recibió` y fecha/hora de servidor.

### Etiquetas

Tamaños documentados:

- 51 x 25 mm: etiqueta chica para folio/código.
- 76 x 51 mm / 3 x 2: etiqueta interna de trabajo.
- 102 x 51 mm / 4 x 2: etiqueta de entrega/repartidor.

Estrategia inicial:

- Impresión desde navegador con CSS de impresión y tamaños en mm.
- Sin impresora directa.
- Sin PDF obligatorio.
- Sin QR/barcode en MVP si requiere dependencia.

### Repartidor

Se propuso MVP mobile-first:

- Rol futuro `Repartidor`.
- Permisos sugeridos `deliveries.view` y `deliveries.update`.
- Ruta privada recomendada `/app/entregas`.
- Listado de entregas asignadas.
- Detalle con cliente, dirección, contacto, indicaciones y trabajos.
- Acción `Marcar como entregado`.
- Captura de `Recibió`.
- Fecha/hora registrada por servidor.

### Usuarios/Roles Y Catálogo

- Usuarios/roles: no implementar todavía si no es prioritario; conviene validar primero seed/usuarios QA y después CRUD admin seguro.
- Catálogo: mantener como backlog; requiere modelo de datos, endpoints, almacenamiento de imágenes, permisos y reglas de publicación.
- El catálogo no debe bloquear el flujo operativo de órdenes/entregas.

### Archivos Modificados

- `docs/README.md`
- `docs/PROJECT_STATUS.md`
- `docs/ROADMAP.md`
- `docs/IMPLEMENTATION_LOG.md`
- `docs/01-product/internal-system.md`
- `docs/01-product/admin-catalog-management.md`

### Archivos Creados

- `docs/01-product/operations-orders-delivery.md`
- `docs/01-product/label-printing.md`
- `docs/01-product/driver-mobile-workflow.md`

### Validaciones Ejecutadas

- `git status --short` antes de editar: sin salida.
- `git diff --stat` antes de editar: sin salida.
- `npm run build` desde `src/LaboratorioTlahuac.Web`: correcto.
- `dotnet build`: correcto, 0 errores; reportó 2 warnings `NU1903` por vulnerabilidad conocida de `SQLitePCLRaw.lib.e_sqlite3` 2.1.11 en el proyecto de tests.
- `dotnet test`: correcto; Domain 1/1, Application 1/1 y API 101/101.
- `git diff --check`: correcto.
- `rg "/app/ordenes" src docs README.md`: revisado; confirma módulo real de órdenes y nuevas menciones documentales de etiquetas.
- `rg "WorkOrder" src tests docs`: revisado; salida extensa por entidades, tests, servicios y documentación.
- `rg "Delivery" src tests docs`: revisado; confirma que lo existente funcional se limita a `DeliveryDate`/`ReadyForDelivery` y que las nuevas referencias de reparto son documentales.
- `rg "repartidor" docs src`: revisado; referencias existentes comerciales y nuevas fuentes producto; no hay código funcional de repartidor.
- `rg "catalog" src docs README.md`: revisado; confirma catálogo público actual y backlog privado.
- `rg "/dashboard" .`: revisado; no se creó `/dashboard` como ruta privada real.
- `rg "/app/dashboard" .`: revisado; confirma que el dashboard privado real sigue bajo `/app/dashboard`.
- `rg "/login" src/LaboratorioTlahuac.Web/src/app docs README.md AGENTS.md`: revisado; confirma `/login` como entrada pública y endpoints/rutas de auth existentes.

### Confirmaciones

- Solo documentación modificada.
- No se modificó código.
- No se crearon migraciones.
- No se tocó base de datos.
- No se tocaron backend/frontend funcionales.
- No se tocó `AuthService`.
- No se tocaron guards.
- No se tocaron cookies ni XSRF.
- No se tocaron endpoints.
- No se instalaron dependencias.
- No se tocó deploy.
- No se imprimieron secretos.
- No se ejecutó `dotnet user-secrets list`.
- No se usó `codex-cobranza-sql`.
- No se hicieron commits.

### Siguiente Fase Recomendada

Fase 3.2 - MVP impresión de etiquetas desde órdenes existentes.

Alcance recomendado: extender `/app/ordenes/:id` con acciones para imprimir etiqueta interna y etiqueta de entrega; crear rutas privadas bajo `/app/ordenes/:id/etiqueta-trabajo` y `/app/ordenes/:id/etiqueta-entrega`; usar CSS de impresión en mm; no agregar dependencias, migraciones, impresora directa, PDF obligatorio ni QR/barcode en el MVP.

## 2026-07-02 - Fase 3.0 Cierre Formal DEV Y Baseline UAT

### Cambio Realizado

Se cerró documentalmente Fase 3.0 como validación formal del despliegue DEV y baseline UAT inicial.

DEV queda registrado como publicado en:

- `https://dev.laboratoriodentaltlahuac.com`

Rama desplegada:

- `dev`

No se implementó funcionalidad nueva. No se modificó código frontend/backend, `AuthService`, guards, cookies, XSRF, endpoints, rutas privadas, base de datos, migraciones, dependencias ni despliegue real. No se hicieron commits.

### Resultado Manual Confirmado

El responsable del proyecto confirmó:

- `/` público: OK.
- `/servicios`: OK.
- `/catalogo`: OK.
- `/contacto`: OK.
- `/login`: OK.
- Login QA: OK.
- `/app/dashboard` autenticado: OK.
- Usuario sin autenticar solo navega rutas públicas: OK.
- Usuario sin autenticar al intentar `/app/dashboard` redirige a `/login`: OK.
- `/dashboard` raíz no es ruta privada real: OK.
- VPS DEV desplegado desde rama `dev`: OK.

### Validación DEV Por `curl`

`curl` sin credenciales respondió `200` para:

- `https://dev.laboratoriodentaltlahuac.com/`
- `https://dev.laboratoriodentaltlahuac.com/servicios`
- `https://dev.laboratoriodentaltlahuac.com/catalogo`
- `https://dev.laboratoriodentaltlahuac.com/contacto`
- `https://dev.laboratoriodentaltlahuac.com/login`
- `https://dev.laboratoriodentaltlahuac.com/app/dashboard`

Se documenta la diferencia clave: en SPA Angular, `curl` puede recibir shell `200` para `/app/dashboard`; la evidencia real de guards/redirección privada proviene de la validación manual por navegador.

### Estado De Base DEV Y VPS

No se inspeccionó directamente la base DEV, no se ejecutaron migraciones, no se corrieron seeds y no se tocaron datos.

Estado inferido por validación manual:

- Login QA funciona.
- `/app/dashboard` autenticado carga.
- DEV queda operativo para baseline UAT inicial.

Los nombres de servicios del VPS y detalles internos de reverse proxy no estaban documentados en el repositorio y no se inspeccionaron en esta fase.

### Pendientes Conservados

- Dirección real del laboratorio.
- Horarios.
- WhatsApp real.
- Aprobación final de precios 2026.
- Aprobación de `Anticipo 50%`.
- Aprobación de `Trabajos urgentes +40%`.
- Imágenes faltantes para `Servicios prostodónticos`.
- Validación de usuario QA limitado y `/app/access-denied` en DEV si aún no queda cerrada formalmente con cuenta limitada sin `reports.view`.
- Definición del siguiente incremento funcional.

### Archivos Modificados

- `README.md`
- `docs/README.md`
- `docs/PROJECT_STATUS.md`
- `docs/ROADMAP.md`
- `docs/IMPLEMENTATION_LOG.md`
- `docs/03-architecture/AUTH_FLOW.md`
- `docs/05-delivery/DEPLOYMENT.md`
- `docs/08-qa/private-admin-qa.md`
- `docs/08-qa/limited-user-qa-plan.md`

### Archivos Creados

- `docs/05-delivery/dev-deployment-validation.md`

### Validaciones Ejecutadas

- `git status --short` antes de editar: sin salida.
- `git branch --show-current`: `dev`.
- `git rev-parse --abbrev-ref --symbolic-full-name @{u}`: `origin/dev`.
- `git diff --stat` antes de editar: sin salida.
- `curl` sin credenciales contra rutas DEV solicitadas: `200` en todas.
- `npm run build` desde `src/LaboratorioTlahuac.Web`: correcto.
- `dotnet build`: correcto, 0 errores; reportó 2 warnings `NU1903` por vulnerabilidad conocida de `SQLitePCLRaw.lib.e_sqlite3` 2.1.11 en el proyecto de tests.
- `dotnet test`: correcto; Domain 1/1, Application 1/1 y API 101/101.
- `git diff --check`: correcto.
- `rg "/dashboard" .`: revisado.
- `rg "/app/dashboard" .`: revisado.
- `rg "/login" src/LaboratorioTlahuac.Web/src/app docs README.md AGENTS.md`: revisado.
- `rg "dev.laboratoriodentaltlahuac.com" docs README.md`: revisado.
- `rg "LT_ADMIN_PASSWORD" .`: revisado; solo nombres de variables/placeholders documentales.
- `rg "LT_QA_LIMITED_PASSWORD" .`: revisado; solo nombres de variables/placeholders documentales.
- `rg "LDT_SQL_SA_PASSWORD" .`: revisado; solo nombres de variables/placeholders documentales.
- `rg "ConnectionStrings" src docs README.md`: revisado; no se documentaron secretos reales.
- `rg "codex-cobranza-sql" docs README.md AGENTS.md`: revisado; solo menciones históricas/documentales de no uso.

### Confirmaciones

- Solo documentación modificada.
- No se modificó código.
- No se instalaron dependencias.
- No se crearon migraciones.
- No se imprimieron secretos.
- No se usó `codex-cobranza-sql`.
- `/login` sigue público.
- `/app` y `/app/dashboard` siguen privados.
- `/dashboard` no es ruta privada real.
- DEV queda registrado como baseline UAT inicial validado.

### Siguiente Fase Recomendada

Fase 3.1 - UAT DEV con usuario QA limitado real para cerrar `/app/access-denied` si aún no está formalmente validado; después definir el siguiente incremento funcional.

## 2026-05-28 - Fase 2.6 Usuario QA Limitado Development-only

### Cambio Realizado

Se implemento el mecanismo seguro Development-only para crear o sincronizar un usuario QA limitado local y validar permisos/access-denied sin alterar Admin.

El mecanismo vive en el seed de seguridad existente:

- `SecuritySeed:LimitedQaUser:RunOnStartup`.
- `SecuritySeed:LimitedQaUser:Email`.
- `SecuritySeed:LimitedQaUser:Password`.
- `SecuritySeed:LimitedQaUser:FullName`.
- `SecuritySeed:LimitedQaUser:Permissions`.
- Variables sensibles soportadas: `LT_QA_LIMITED_EMAIL`, `LT_QA_LIMITED_PASSWORD` y `LT_QA_LIMITED_FULL_NAME`.

Condiciones aplicadas:

- Solo corre en `Development`.
- Desactivado por default.
- Requiere habilitacion explicita.
- No guarda secretos en archivos versionados.
- No imprime contrasenas.
- No usa SQL manual.
- No crea migraciones.
- No expone endpoints nuevos.
- No altera Admin; si el email configurado pertenece a un Admin, el seed QA se omite.
- No modifica rutas privadas, `AuthService`, `auth.guard.ts`, `permission.guard.ts`, cookies, XSRF ni deploy.

### Implementacion Tecnica

- `Program.cs` ejecuta el seeder si `SecuritySeed:RunOnStartup=true` o si el entorno es `Development` y `SecuritySeed:LimitedQaUser:RunOnStartup=true`.
- `SecuritySeeder` separa el seed Admin del seed QA limitado para que activar solo QA no sincronice Admin.
- El rol local `Limited QA` se crea o sincroniza con los permisos configurados.
- Los permisos QA se parsean desde `SecuritySeed:LimitedQaUser:Permissions` y solo se aplican si existen en `Permissions.All`.
- El usuario QA se crea o actualiza de forma idempotente, se activa, se desbloquea y queda asignado solo al rol `Limited QA`.
- `User` agrega metodos de dominio para renombrar y limpiar lockout sin cambiar esquema.
- `DependencyInjection` registra si el runtime esta en Development para que Infrastructure no dependa de hosting web.

### Pruebas

Se agregaron:

- `SecuritySeederTests`.
- `LimitedQaUserSeedIntegrationTests`.

Cobertura nueva:

- No crea usuario limitado fuera de Development.
- No crea usuario limitado si `RunOnStartup` no esta activo.
- No crea usuario limitado si falta configuracion requerida.
- Crea usuario limitado en Development con configuracion completa.
- Usuario limitado no tiene `reports.view` cuando no se configura.
- Usuario limitado puede tener `customers.view`.
- Admin existente no se altera.
- Login API con usuario QA limitado.
- `/api/auth/me` devuelve permisos limitados y no expone `passwordHash`.
- `/api/customers` responde `200` con `customers.view`.
- `/api/dashboard/summary` responde `403` con sesion limitada sin `reports.view`.
- `/api/dashboard/summary` responde `401` sin sesion.

### Validacion Local

- SQL dedicado confirmado: `ldt-labdental-sql`.
- Puerto confirmado: `14336 -> 1433/tcp`.
- No se uso `codex-cobranza-sql`.
- `LT_QA_LIMITED_EMAIL`, `LT_QA_LIMITED_PASSWORD` y `LT_QA_LIMITED_FULL_NAME` no estan disponibles en el proceso de Codex.
- No se inventaron credenciales.
- No se ejecuto `dotnet user-secrets list`.
- No se creo usuario QA limitado en la base local real durante esta ejecucion.
- No hay navegador/headless local disponible sin instalar dependencias, por lo que `/app/access-denied` queda pendiente de pase manual en navegador real.

### Archivos Modificados

- `README.md`
- `docs/README.md`
- `docs/PROJECT_STATUS.md`
- `docs/ROADMAP.md`
- `docs/IMPLEMENTATION_LOG.md`
- `docs/01-product/internal-system.md`
- `docs/03-architecture/AUTH_FLOW.md`
- `docs/03-architecture/ARCHITECTURE.md`
- `docs/08-qa/RESPONSIVE_CHECKLIST.md`
- `docs/08-qa/limited-user-qa-plan.md`
- `docs/08-qa/private-admin-qa.md`
- `src/LaboratorioTlahuac.Api/Program.cs`
- `src/LaboratorioTlahuac.Domain/Security/Entities/User.cs`
- `src/LaboratorioTlahuac.Infrastructure/DependencyInjection.cs`
- `src/LaboratorioTlahuac.Infrastructure/Security/Seed/SecuritySeedOptions.cs`
- `src/LaboratorioTlahuac.Infrastructure/Security/Seed/SecuritySeeder.cs`
- `tests/LaboratorioTlahuac.Api.Tests/AuthIntegrationTests.cs`

### Archivos Creados

- `tests/LaboratorioTlahuac.Api.Tests/SecuritySeederTests.cs`
- `tests/LaboratorioTlahuac.Api.Tests/LimitedQaUserSeedIntegrationTests.cs`

### Validaciones Ejecutadas

- `npm run build` desde `src/LaboratorioTlahuac.Web`: correcto.
- `dotnet build`: correcto, 0 warnings y 0 errores.
- `dotnet test`: correcto; Domain 1/1, Application 1/1 y API 101/101.
- `git diff --check`: correcto.
- `docker ps --filter name=ldt-labdental-sql`: confirmo SQL dedicado activo.
- `docker port ldt-labdental-sql`: confirmo `14336 -> 1433/tcp`.
- `docker ps --filter name=codex-cobranza-sql`: sin contenedor activo.
- `rg "/dashboard" .`: revisado; no se detecto `/dashboard` como ruta privada real nueva.
- `rg "/app/dashboard" .`: revisado; confirma que el dashboard privado real se mantiene bajo `/app/dashboard`.
- `rg "/login" src/LaboratorioTlahuac.Web/src/app docs README.md AGENTS.md`: revisado; confirma `/login` como entrada publica y endpoints/rutas de auth existentes.
- `rg "access-denied" src docs tests README.md`: revisado; confirma ruta/pagina y documentacion de pendiente manual.
- `rg "LimitedQaUser" src docs tests README.md`: revisado; confirma configuracion y pruebas nuevas.
- `rg "LT_QA_LIMITED" src docs tests README.md`: revisado; solo aparecen nombres de variables, placeholders y codigo de lectura de configuracion.
- `rg --files-with-matches "LT_ADMIN_PASSWORD" .`: ejecutado con salida limitada a archivos para no imprimir valores.
- `rg --files-with-matches "LT_QA_LIMITED_PASSWORD" .`: ejecutado con salida limitada a archivos para no imprimir valores.
- `rg --files-with-matches "LDT_SQL_SA_PASSWORD" .`: ejecutado con salida limitada a archivos para no imprimir valores.
- `rg "ConnectionStrings" src docs README.md`: revisado; no se detectaron secretos reales.
- `rg "codex-cobranza-sql" docs README.md AGENTS.md`: revisado; las menciones corresponden a documentacion de no uso o historico.

### Siguiente Fase Recomendada

Ejecutar pase manual con usuario QA limitado real: configurar user-secrets, levantar API en Development contra `ldt-labdental-sql`, apagar el seed, iniciar sesion en `/login`, abrir `/app/dashboard` y confirmar redireccion a `/app/access-denied`; luego confirmar `/app/clientes` y logout.

## 2026-05-28 - Fase 2.5 Cierre Visual Humano Privado Completado Y Usuario Limitado

### Cambio Realizado

Se cerró documentalmente Fase 2.5 como pase visual humano privado completado y se mantuvo el mecanismo seguro recomendado para usuario QA limitado como backlog técnico inmediato.

No se modificó código frontend/backend, `AuthService`, `auth.guard.ts`, `permission.guard.ts`, cookies, XSRF, endpoints, rutas privadas, base de datos, migraciones, deploy ni dependencias. No se hicieron commits.

### Resultado Visual Humano

El responsable del proyecto confirmó el pase visual/manual privado en navegador real.

Estado registrado en `docs/08-qa/private-admin-qa.md`:

- `/login`: OK.
- Login Admin: OK.
- `/app/dashboard`: OK.
- Navegación activa en `/app/dashboard`: OK.
- `/app/clientes`: OK.
- Navegación activa en `/app/clientes`: OK.
- `/app/ordenes`: OK.
- Navegación activa en `/app/ordenes`: OK.
- `/app/pagos`: OK.
- Navegación activa en `/app/pagos`: OK.
- `/app/inventario`: OK como placeholder.
- `/app/proveedores`: OK como placeholder.
- `/app/admin/usuarios`: OK como placeholder.
- `/app/admin/roles`: OK como placeholder.
- Logout: OK.
- `/app/dashboard` sin sesión redirige a `/login?returnUrl=%2Fapp%2Fdashboard`: OK.
- `/dashboard` raíz no es ruta privada real: OK.
- Sitio público sin regresión visible: OK.
- Observaciones visuales: sin bloqueantes visuales reportados.

### Usuario QA Limitado

Se evaluaron tres opciones:

- Seed QA limitado solo Development.
- Esperar módulo de usuarios/roles.
- Script local de QA.

Recomendación documentada: seed QA limitado solo Development, desactivado por default, controlado por user-secrets o variables de entorno, sin imprimir password, sin SQL manual, sin alterar Admin y sin activarse fuera de `Development`.

Plan creado: `docs/08-qa/limited-user-qa-plan.md`.

### Hallazgos

- Bloqueante: ninguno.
- Alto: ninguno.
- Medio: ninguno.
- Bajo: no se puede cerrar evidencia de `/app/access-denied` con usuario limitado real porque no existe mecanismo seguro local implementado.
- Observación: pase visual humano privado completado sin bloqueantes visuales reportados.

### Archivos Modificados

- `README.md`
- `docs/README.md`
- `docs/PROJECT_STATUS.md`
- `docs/ROADMAP.md`
- `docs/IMPLEMENTATION_LOG.md`
- `docs/01-product/internal-system.md`
- `docs/03-architecture/AUTH_FLOW.md`
- `docs/08-qa/RESPONSIVE_CHECKLIST.md`
- `docs/08-qa/private-admin-qa.md`

### Archivos Creados

- `docs/08-qa/limited-user-qa-plan.md`

### Validaciones De Cierre

- `npm run build` desde `src/LaboratorioTlahuac.Web`: correcto.
- `dotnet build`: correcto, 0 warnings y 0 errores.
- `dotnet test`: correcto; Domain 1/1, Application 1/1 y API 91/91.
- `git diff --check`: correcto.
- `rg "/dashboard" .`: revisado; no se detectó `/dashboard` como ruta privada real nueva.
- `rg "/app/dashboard" .`: revisado; confirma que el dashboard privado real se mantiene bajo `/app/dashboard`.
- `rg "/login" src/LaboratorioTlahuac.Web/src/app docs README.md AGENTS.md`: revisado; confirma `/login` como entrada pública y endpoints/rutas de auth existentes.
- `rg "routerLinkActive" src/LaboratorioTlahuac.Web/src/app/admin/layout`: revisado; confirma navegación activa por `RouterLinkActive`.
- `rg "America/Mexico_City" src docs tests README.md`: revisado; confirma configuración/código/documentación de zona horaria.
- `rg -F "Central Standard Time (Mexico)" src docs tests README.md`: revisado con búsqueda literal por paréntesis; confirma compatibilidad Windows.
- `rg --files-with-matches "LT_ADMIN_PASSWORD" .`: ejecutado con salida limitada a archivos para no imprimir valores.
- `rg --files-with-matches "LDT_SQL_SA_PASSWORD" .`: ejecutado con salida limitada a archivos para no imprimir valores.
- `rg --files-with-matches "ConnectionStrings" src docs README.md`: ejecutado con salida limitada a archivos para no imprimir valores.
- `rg "codex-cobranza-sql" docs README.md AGENTS.md`: revisado; solo aparecen menciones documentales o históricas de no uso.

### Siguiente Fase Recomendada

Implementar, si se autoriza tocar backend mínimo, el mecanismo QA limitado solo Development documentado en `docs/08-qa/limited-user-qa-plan.md` para validar `/app/access-denied`.

## 2026-05-27 - Fase 2.4 Pase Visual/Manual Privado Y Permisos

### Cambio Realizado

Se ejecutó la Fase 2.4 como pase manual/técnico del sistema privado para validar los cambios de Fase 2.3 y no se implementaron funcionalidades nuevas.

No se modificaron código frontend/backend, `AuthService`, `auth.guard.ts`, `permission.guard.ts`, cookies, XSRF, endpoints, rutas privadas, migraciones, deploy ni dependencias. No se hicieron commits.

### Entorno

- SQL dedicado: `ldt-labdental-sql`.
- Puerto SQL local: `14336 -> 1433/tcp`.
- Base local: `LaboratorioTlahuac_Dev`.
- API local: `http://localhost:5277`.
- Angular dev server: `http://localhost:4200`.
- `codex-cobranza-sql` no apareció activo y no se usó.
- Credenciales Admin tomadas de variables de entorno locales sin imprimir valores.
- No hay navegador/headless local disponible sin instalar dependencias.

### Validación Ejecutada

- Preflight Docker confirmó `ldt-labdental-sql` activo y puerto `14336`.
- `/health` respondió `200`.
- Rutas públicas `/`, `/servicios`, `/catalogo`, `/contacto` y `/login` respondieron con shell Angular `200`.
- Rutas privadas objetivo de navegación respondieron con shell Angular `200`; la ejecución real de guards/estado activo queda limitada por falta de navegador/headless.
- Login Admin por API: CSRF `204`, login `200`, `/api/auth/me` `200` con 19 permisos.
- Dashboard/listados con Admin: `/api/dashboard/summary`, `/api/customers`, `/api/work-orders` y `/api/payments` respondieron `200`.
- Logout Admin: `POST /api/auth/logout` `200`, `/api/auth/me` posterior `401` y `/api/dashboard/summary` posterior `401`.
- Dashboard zona horaria: una orden QA con `DeliveryDate=2026-05-27` incrementó `dueToday` de 1 a 2 y `upcomingDue` de 1 a 2 con fecha operativa `America/Mexico_City`.
- `generatedAtUtc` se confirmó en UTC con offset `+00:00`; `DeliveryDate` conserva su significado de fecha capturada.
- Navegación activa se validó por código: `RouterLinkActive`, `ariaCurrentWhenActive`, match exacto para `/app/dashboard` y estilos `.is-active`/`focus-visible`.
- Usuario limitado no se creó porque no existe mecanismo seguro local fuera de fixtures de pruebas y no se autorizó SQL directo.

### Datos QA Creados

Quedaron en la base local:

- Cliente `a5c48811-e171-450b-963e-f929a0d71084`, con nombre prefijado `Cliente QA LDT F2.4`.
- Orden `OT-20260528-82F6A6`, id `53a35d65-a3ff-4f7d-ab7c-b0b2d658df44`, `DeliveryDate=2026-05-27`.

No se limpiaron datos QA.

### Hallazgos

- Bloqueante: ninguno.
- Alto: ninguno.
- Medio: ninguno.
- Bajo: no se pudo probar `/app/access-denied` con usuario limitado real por falta de mecanismo seguro de creación local.
- Observación: el pase visual real de navegación activa queda pendiente por falta de navegador/headless disponible sin instalar dependencias.

### Archivos Modificados

- `docs/PROJECT_STATUS.md`
- `docs/ROADMAP.md`
- `docs/IMPLEMENTATION_LOG.md`
- `README.md`
- `docs/README.md`
- `docs/01-product/internal-system.md`
- `docs/03-architecture/AUTH_FLOW.md`
- `docs/08-qa/RESPONSIVE_CHECKLIST.md`
- `docs/08-qa/private-admin-qa.md`

### Validaciones De Cierre

- `npm run build` desde `src/LaboratorioTlahuac.Web`: correcto.
- `dotnet build`: correcto, 0 warnings y 0 errores.
- `dotnet test`: correcto; Domain 1/1, Application 1/1 y API 91/91.
- `git diff --check`: correcto.
- `rg "/dashboard" .`: revisado; no se detectó `/dashboard` como ruta privada real nueva.
- `rg "/app/dashboard" .`: revisado; confirma que el dashboard privado real se mantiene bajo `/app/dashboard`.
- `rg "/login" src/LaboratorioTlahuac.Web/src/app docs README.md AGENTS.md`: revisado; confirma `/login` como entrada pública y endpoints/rutas de auth existentes.
- `rg "routerLinkActive" src/LaboratorioTlahuac.Web/src/app/admin/layout`: revisado; confirma navegación activa por `RouterLinkActive`.
- `rg "America/Mexico_City" src docs tests README.md`: revisado; confirma configuración/código/documentación de zona horaria.
- `rg -F "Central Standard Time (Mexico)" src docs tests README.md`: revisado con búsqueda literal por paréntesis; confirma compatibilidad Windows.
- `rg --files-with-matches "LT_ADMIN_PASSWORD" .`: ejecutado con salida limitada a archivos para no imprimir valores.
- `rg --files-with-matches "LDT_SQL_SA_PASSWORD" .`: ejecutado con salida limitada a archivos para no imprimir valores.
- `rg --files-with-matches "ConnectionStrings" src docs README.md`: ejecutado con salida limitada a archivos para no imprimir valores.
- `rg "codex-cobranza-sql" docs README.md AGENTS.md`: revisado; solo aparecen menciones documentales o históricas de no uso.

### Siguiente Fase Recomendada

Fase 2.5 - cierre visual humano del sistema privado y definición de mecanismo seguro para usuario QA limitado.

## 2026-05-27 - Fase 2.3 Corrección De Hallazgos QA Del Sistema Privado

### Cambio Realizado

Se corrigieron los dos hallazgos registrados en Fase 2.2 para el sistema privado:

- Hallazgo medio: métricas operativas del dashboard calculaban "hoy" con fecha UTC pura.
- Hallazgo bajo: la navegación privada no marcaba visualmente la ruta activa.

No se modificaron sitio público, `AuthService`, `auth.guard.ts`, `permission.guard.ts`, cookies, XSRF, endpoints públicos, rutas privadas, migraciones, deploy ni dependencias. No se hicieron commits.

### Zona Horaria De Negocio

- Se agregó configuración `Dashboard:BusinessTimeZone` con default `America/Mexico_City`.
- `DashboardService` conserva `generatedAtUtc` en UTC, pero calcula el "hoy" operativo convirtiendo `clock.UtcNow` a la zona horaria de negocio.
- `dueToday`, `overdue` y `upcomingDue` usan la fecha operativa del laboratorio.
- `DeliveryDate` no cambió de significado ni de tipo.
- El ID canónico documentado es IANA `America/Mexico_City`; para compatibilidad Windows se acepta `Central Standard Time (Mexico)`.

### Navegación Privada

- `PrivateLayoutComponent` incorpora `RouterLinkActive`.
- `/app/dashboard` usa `routerLinkActiveOptions` con `exact: true`.
- Los enlaces privados conservan visibilidad condicional por permisos.
- Se agregaron estilos de activo, hover y `focus-visible` con contraste suficiente.
- No se cambiaron rutas, permisos ni logout.

### Pruebas

- Se agregó `OperationalSummaryUsesBusinessTimeZoneDateWhenUtcDateDiffers` en `DashboardIntegrationTests`.
- El caso fija `clock.UtcNow` en `2026-05-10T04:30:00Z`, cuando Mexico City sigue en fecha local `2026-05-09`.
- La prueba valida que una orden con entrega igual al día local cuenta como `dueToday` y que `overdue` y `upcomingDue` conservan comportamiento esperado.
- Frontend no tiene runner no interactivo ni patrón `.spec.ts`; la navegación privada se validó por código y `npm run build`.

### Archivos Modificados

- `README.md`
- `docs/PROJECT_STATUS.md`
- `docs/ROADMAP.md`
- `docs/IMPLEMENTATION_LOG.md`
- `docs/01-product/internal-system.md`
- `docs/03-architecture/ARCHITECTURE.md`
- `docs/08-qa/RESPONSIVE_CHECKLIST.md`
- `docs/08-qa/private-admin-qa.md`
- `src/LaboratorioTlahuac.Api/appsettings.json`
- `src/LaboratorioTlahuac.Infrastructure/Dashboard/DashboardOptions.cs`
- `src/LaboratorioTlahuac.Infrastructure/Dashboard/DashboardService.cs`
- `src/LaboratorioTlahuac.Infrastructure/Dashboard/DashboardTimeZoneResolver.cs`
- `src/LaboratorioTlahuac.Infrastructure/DependencyInjection.cs`
- `src/LaboratorioTlahuac.Web/src/app/admin/layout/private-layout.component.scss`
- `src/LaboratorioTlahuac.Web/src/app/admin/layout/private-layout.component.ts`
- `tests/LaboratorioTlahuac.Api.Tests/AuthIntegrationTests.cs`
- `tests/LaboratorioTlahuac.Api.Tests/DashboardIntegrationTests.cs`

### Validaciones Ejecutadas

- `npm run build` desde `src/LaboratorioTlahuac.Web`: correcto.
- `dotnet build`: correcto, 0 warnings y 0 errores.
- `dotnet test`: correcto tras corregir el fixture de pruebas; Domain 1/1, Application 1/1 y API 91/91.
- La primera ejecución de `dotnet test` falló porque `TestApplicationFactory` tenía dos constructores públicos; se ajustó a un solo constructor público y se repitió correctamente.
- `git diff --check`: correcto.
- `docker ps --filter "name=ldt-labdental-sql"`: confirmó `ldt-labdental-sql` activo en `14336`.
- `docker port ldt-labdental-sql`: confirmó `1433/tcp -> 0.0.0.0:14336` y `[::]:14336`; se requirió permiso fuera del sandbox.
- `rg "/dashboard" .`: revisado; no se detectó `/dashboard` como ruta privada real nueva.
- `rg "/app/dashboard" .`: revisado; confirma que el dashboard privado real se mantiene bajo `/app/dashboard`.
- `rg "/login" src/LaboratorioTlahuac.Web/src/app docs README.md AGENTS.md`: revisado; confirma `/login` como entrada pública y endpoints de auth existentes.
- `rg "routerLinkActive" src/LaboratorioTlahuac.Web/src/app/admin/layout`: revisado; confirma estado activo en navegación privada.
- `rg "America/Mexico_City" src docs tests README.md`: revisado; confirma configuración/código/documentación de zona horaria.
- `rg --files-with-matches "LT_ADMIN_PASSWORD" .`: ejecutado con salida limitada a archivos para no imprimir valores.
- `rg --files-with-matches "LDT_SQL_SA_PASSWORD" .`: ejecutado con salida limitada a archivos para no imprimir valores.
- `rg --files-with-matches "ConnectionStrings" src docs README.md`: ejecutado con salida limitada a archivos para no imprimir valores.
- `rg "codex-cobranza-sql" docs README.md AGENTS.md`: revisado; solo aparecen menciones documentales o históricas de no uso.

### Siguiente Fase Recomendada

Fase 2.4 - pase visual/manual privado y validación de permisos con usuario limitado si se requiere.

## 2026-05-27 - Fase 2.2 QA Manual/Técnico Del Sistema Privado Con Admin

### Cambio Realizado

Se ejecutó QA manual/técnico del sistema privado existente bajo `/app` con Admin local. No se implementaron funcionalidades nuevas, no se rediseñaron pantallas, no se modificó código frontend/backend, no se tocaron `AuthService`, guards, cookies, XSRF, endpoints, rutas privadas, migraciones, deploy ni dependencias, y no se hicieron commits.

Se creó el reporte `docs/08-qa/private-admin-qa.md` y se actualizaron las fuentes canónicas afectadas.

### Ambiente

- SQL dedicado: `ldt-labdental-sql`.
- Puerto SQL local: `14336 -> 1433/tcp`.
- Base local: `LaboratorioTlahuac_Dev`.
- API local: `http://localhost:5277`.
- Angular local: `http://localhost:4200`.
- `codex-cobranza-sql` no apareció activo y no se usó.
- `LT_ADMIN_EMAIL` y `LT_ADMIN_PASSWORD` se usaron desde variables de entorno sin imprimir valores.

### Resultado QA

- Rutas públicas `/`, `/servicios`, `/catalogo`, `/contacto` y `/login` respondieron con shell Angular `200`.
- Rutas privadas reales detectadas bajo `/app`: dashboard, clientes, órdenes, pagos, inventario, proveedores, usuarios, roles y access-denied.
- `/dashboard` raíz no existe como ruta privada real; el wildcard del router sigue enviando a la home pública.
- Sin sesión, endpoints privados respondieron `401`.
- Con Admin: login `200`, `/api/auth/me` `200` con 19 permisos, dashboard `200`, clientes `200`, órdenes `200`, pagos `200`, logout `200` y `/api/auth/me` posterior a logout `401`.
- `returnUrl` externo sigue bloqueado por código en `login-page.component.ts`; solo se aceptan rutas internas seguras bajo `/app`.
- Usuario sin permiso no se probó con cuenta limitada porque no existe usuario QA limitado disponible; por código, `permissionGuard` redirige a `/app/access-denied`.

### Datos De Prueba Creados

Quedaron en la base local:

- Cliente `Cliente QA LDT 20260527-210940 Editado`, id `fd5fe049-33e9-4732-80fa-790d140468f4`.
- Orden `OT-20260528-201A16`, id `967c2750-cbb4-4aec-908c-14a04fd120fb`.
- Pago `Pago QA LDT 20260527-210940`, id `561b6d36-6dff-4fe3-b08e-6705dc0947dd`.

No se limpiaron datos de prueba.

### Hallazgos

- Medio: la métrica "Para hoy" del dashboard requiere definir zona horaria de negocio; durante QA local en `CST -0600`, una orden con entrega en la fecha local de QA no incrementó `dueToday`.
- Bajo: la navegación privada no marca visualmente la ruta activa porque `PrivateLayoutComponent` no usa `routerLinkActive` ni clase equivalente.
- Observación: no se probó usuario autenticado sin permiso por falta de usuario limitado local.
- Observación: inventario, proveedores, usuarios y roles siguen como páginas placeholder documentadas.
- Observación: no hay navegador/headless local sin instalar dependencias, por lo que consola/Network y redirecciones visuales quedaron cubiertas por código/API.

### Archivos Modificados

- `docs/README.md`
- `docs/PROJECT_STATUS.md`
- `docs/ROADMAP.md`
- `docs/IMPLEMENTATION_LOG.md`
- `docs/01-product/internal-system.md`
- `docs/03-architecture/AUTH_FLOW.md`
- `docs/08-qa/RESPONSIVE_CHECKLIST.md`
- `docs/08-qa/private-admin-qa.md`

### Validaciones Ejecutadas

- `npm run build` desde `src/LaboratorioTlahuac.Web`: correcto.
- `dotnet build`: correcto, 0 warnings y 0 errores.
- `dotnet test`: correcto; Domain 1/1, Application 1/1 y API 90/90.
- `git diff --check`: correcto.
- `rg "/dashboard" .`: revisado; no se detectó `/dashboard` como ruta privada real nueva.
- `rg "/app/dashboard" .`: revisado; confirma que la ruta privada real se mantiene bajo `/app/dashboard`.
- `rg "/login" src/LaboratorioTlahuac.Web/src/app docs README.md AGENTS.md`: revisado; confirma `/login` como entrada pública y endpoints de auth existentes.
- `rg --files-with-matches "LT_ADMIN_PASSWORD" .`: ejecutado con salida limitada a archivos para no imprimir valores.
- `rg --files-with-matches "LDT_SQL_SA_PASSWORD" .`: ejecutado con salida limitada a archivos para no imprimir valores.
- `rg --files-with-matches "ConnectionStrings" src docs README.md`: ejecutado con salida limitada a archivos para no imprimir valores.
- `rg "codex-cobranza-sql" docs README.md AGENTS.md`: revisado; solo aparecen menciones documentales o históricas de no uso.

### Siguiente Fase Recomendada

Fase 2.3 - Corrección de hallazgos QA del sistema privado.

## 2026-05-27 - Cierre Documental De Fase 1.6 Y Fase 2.1d

### Cambio Realizado

Se cerraron documentalmente dos etapas con base en la validación manual confirmada por el responsable del proyecto:

- Fase 1.6 - Pulido visual premium del sitio público.
- Fase 2.1d - Diagnóstico/corrección de loading del dashboard autenticado.

No hubo cambios de código, estilos, frontend funcional, backend, `AuthService`, guards, cookies, XSRF, endpoints, base de datos, migraciones, deploy ni dependencias. No se instalaron paquetes y no se hicieron commits.

### Resultado Manual Registrado

- `/`, `/servicios`, `/catalogo`, `/contacto` y `/login` fueron revisados visualmente y aprobados.
- Breakpoints aprobados: 360px, 375px, 390px, 414px, 768px, 1024px y desktop.
- El sitio público queda mobile-first, sin scroll horizontal y sin problemas visuales bloqueantes reportados.
- El catálogo queda legible, con imágenes uniformes, precios correctos y placeholders intencionales.
- El enfoque CSS + `IntersectionObserver` queda aceptado; no se usó GSAP ni dependencia nueva.
- Reduced motion queda validado por implementación/código; no se reportaron hallazgos manuales bloqueantes.
- Login con Admin local, redirección a `/app/dashboard` y dashboard autenticado quedan validados manualmente.
- `/app/dashboard` ya no queda indefinidamente en `Cargando dashboard...`.
- Flujo autenticado validado manualmente; `GET /api/auth/me` autenticado no fue inspeccionado de forma independiente.
- `GET /api/dashboard/summary` autenticado queda validado indirectamente por la carga correcta del dashboard.
- Redirección posterior a logout o sesión cerrada validada: `/app/dashboard` redirige a `/login?returnUrl=%2Fapp%2Fdashboard`; logout como acción independiente queda para QA amplio si se requiere evidencia separada.

### Archivos Modificados

- `docs/PROJECT_STATUS.md`
- `docs/ROADMAP.md`
- `docs/IMPLEMENTATION_LOG.md`
- `docs/01-product/public-website.md`
- `docs/01-product/internal-system.md`
- `docs/02-domain/brand-guidelines.md`
- `docs/03-architecture/AUTH_FLOW.md`
- `docs/08-qa/RESPONSIVE_CHECKLIST.md`

### Validaciones Ejecutadas

- `git status --short` antes de editar: sin salida; working tree limpio.
- `git diff --stat` antes de editar: sin salida.
- `npm run build` desde `src/LaboratorioTlahuac.Web`: correcto.
- `dotnet build`: correcto, 0 warnings y 0 errores.
- `dotnet test`: correcto; Domain 1/1, Application 1/1 y API 90/90.
- `git diff --check`: correcto.
- `rg "/dashboard" .`: revisado; no muestra `/dashboard` como ruta privada real nueva.
- `rg "/app/dashboard" .`: revisado; confirma que la ruta privada real se mantiene bajo `/app/dashboard`.
- `rg "/login" src/LaboratorioTlahuac.Web/src/app docs README.md AGENTS.md`: revisado; confirma `/login` como entrada pública y endpoints de auth existentes.
- `rg "LT_ADMIN_PASSWORD" .`: ejecutado con salida limitada al patrón para no imprimir valores; solo se encontraron menciones del nombre de variable.
- `rg "LDT_SQL_SA_PASSWORD" .`: ejecutado con salida limitada al patrón para no imprimir valores; solo se encontraron menciones del nombre de variable.
- `rg "ConnectionStrings" src docs README.md`: ejecutado con salida limitada al patrón para no imprimir valores; solo se encontraron menciones de la clave de configuración.
- `rg "codex-cobranza-sql" docs README.md AGENTS.md`: revisado; solo aparecen menciones documentales de que no se usó.

### Confirmaciones

- `/login` sigue público.
- `/app` y `/app/dashboard` siguen privados.
- `/dashboard` no es ruta privada real.
- Fase 1.6 queda cerrada como validada visualmente.
- Fase 2.1d queda cerrada como validada manualmente.
- Siguiente fase recomendada: Fase 2.2 - QA manual del sistema privado con Admin.
- No se ejecutó `dotnet user-secrets list`.
- No se imprimieron secretos.
- No se usó `codex-cobranza-sql`.

## 2026-05-27 - Cierre Documental Parcial De Validación Visual Fase 1.6

### Cambio Realizado

Se actualizó documentación para registrar el reporte manual de revisión visual de Fase 1.6 del sitio público sin modificar código, instalar dependencias ni tocar backend/auth/guards/endpoints/base/deploy.

El cierre queda como parcialmente validado visualmente porque el reporte recibido conserva marcadores sin selección final ni observaciones concretas por ruta o breakpoint.

Nota posterior: este cierre parcial queda superado por el cierre documental completo registrado arriba el mismo 2026-05-27, basado en la confirmación manual final del responsable del proyecto.

### Resultado Manual Recibido

- Rutas reportadas como revisadas: `/`, `/servicios`, `/catalogo`, `/contacto` y `/login`.
- Viewports reportados como revisados: 360px, 375px, 390px, 414px, 768px, 1024px y desktop.
- Puntos adicionales reportados: reduced motion y scroll horizontal.
- Limitación documental: los puntos llegaron como `[correcto / observaciones]`, `[correcto / no probado / observaciones]` y `[no hay / observaciones]`, sin selección explícita ni observaciones.

### Archivos Modificados

- `docs/PROJECT_STATUS.md`
- `docs/IMPLEMENTATION_LOG.md`
- `docs/01-product/public-website.md`
- `docs/02-domain/brand-guidelines.md`
- `docs/08-qa/RESPONSIVE_CHECKLIST.md`

### Alcance No Tocado

- No se modificó código frontend ni backend.
- No se modificaron `AuthService`, guards, rutas, cookies, XSRF, endpoints, base de datos, migraciones, deploy ni dependencias.
- El working tree conserva cambios previos de código de Fase 1.6; este cierre documental modificó únicamente los cinco documentos listados.
- `/login` sigue documentado como público.
- `/app` y `/app/dashboard` siguen documentados como privados.
- `/dashboard` sigue documentado como no ruta privada real.

### Validaciones Ejecutadas

- `npm run build` desde `src/LaboratorioTlahuac.Web`: correcto.
- `dotnet build`: correcto, 0 warnings y 0 errores.
- `dotnet test`: correcto; Domain 1/1, Application 1/1 y API 90/90.
- `git diff --check`: correcto.
- Búsqueda de rutas: `/login` sigue fuera de `/app`; `/app` conserva `authGuard`; `/app/dashboard` conserva `permissionGuard` y `reports.view`; `/dashboard` no aparece como ruta privada real raíz.
- Búsqueda de secretos en los documentos tocados: solo aparecen nombres de variables, placeholders, textos redactados o menciones de `user-secrets`; no se detectaron valores reales de contraseña, tokens, API keys ni llaves privadas.

## 2026-05-27 - Fase 1.6 Pulido Visual Premium Del Sitio Público

### Cambio Realizado

Se implementó pulido visual premium del sitio público mobile-first con animaciones sutiles, composición más moderna, microinteracciones y mejoras de catálogo/contacto.

Enfoque elegido: CSS + `IntersectionObserver`. No se instaló GSAP ni otra dependencia porque los requerimientos de reveal, microinteracción y parallax ligero se cubren con APIs nativas, menor impacto de bundle y limpieza directa al destruir componentes Angular.

### Archivos Leídos

- `AGENTS.md`
- `README.md`
- `docs/PROJECT_STATUS.md`
- `docs/ROADMAP.md`
- `docs/01-product/public-website.md`
- `docs/02-domain/brand-guidelines.md`
- `docs/08-qa/RESPONSIVE_CHECKLIST.md`
- Componentes públicos de layout, home, servicios, catálogo y contacto.
- SCSS visual de `/login`.
- `src/LaboratorioTlahuac.Web/src/app/app.routes.ts`
- `src/LaboratorioTlahuac.Web/package.json`

### Archivos Modificados

- `src/LaboratorioTlahuac.Web/src/app/public/animations/public-scroll-animations.directive.ts`
- `src/LaboratorioTlahuac.Web/src/app/public/layout/public-layout.component.ts`
- `src/LaboratorioTlahuac.Web/src/app/public/layout/public-layout.component.scss`
- `src/LaboratorioTlahuac.Web/src/app/public/pages/home/home-page.component.ts`
- `src/LaboratorioTlahuac.Web/src/app/public/pages/home/home-page.component.scss`
- `src/LaboratorioTlahuac.Web/src/app/public/pages/services/services-page.component.ts`
- `src/LaboratorioTlahuac.Web/src/app/public/pages/catalog/catalog-page.component.ts`
- `src/LaboratorioTlahuac.Web/src/app/public/pages/catalog/catalog-page.component.scss`
- `src/LaboratorioTlahuac.Web/src/app/public/pages/contact/contact-page.component.ts`
- `src/LaboratorioTlahuac.Web/src/app/auth/pages/login/login-page.component.scss`
- `src/LaboratorioTlahuac.Web/src/styles.scss`
- `docs/PROJECT_STATUS.md`
- `docs/ROADMAP.md`
- `docs/IMPLEMENTATION_LOG.md`
- `docs/01-product/public-website.md`
- `docs/02-domain/brand-guidelines.md`
- `docs/08-qa/RESPONSIVE_CHECKLIST.md`

### Mejoras Visuales

- Header público con mejor presencia de logo, navegación con estado activo más claro y microinteracciones.
- Footer más visual, ordenado y con todos los teléfonos/correo confirmados.
- Home con hero institucional más cinematográfico, logo con profundidad, reveal de copy/CTAs, beneficios, proceso y contacto.
- Servicios con composición editorial, tarjetas numeradas y CTA claro al catálogo.
- Catálogo con encabezado premium, resumen visual, contacto/condiciones más claras, cards con frame uniforme, precios legibles y microinteracción de imagen.
- Contacto con cards que separan datos confirmados de pendientes sin inventar dirección, horarios ni WhatsApp.
- Login recibió solo pulido visual de SCSS; la lógica quedó intacta.

### Animación Y Accesibilidad

- La directiva pública observa elementos `data-animate` y `data-parallax`.
- `prefers-reduced-motion: reduce` desactiva reveal, parallax y transformaciones relevantes.
- Si `IntersectionObserver` no existe o JS falla antes de activar la directiva, el contenido permanece visible.
- Las animaciones usan `opacity` y `transform`; no animan propiedades de layout costosas.
- En catálogo, el reveal de productos se limita por lote inicial por sección.

### Alcance No Tocado

- No se modificó backend.
- No se modificaron `AuthService`, `auth.guard.ts`, `permission.guard.ts`, cookies, XSRF, endpoints, base de datos, migraciones, deploy ni contratos API.
- No se cambiaron rutas privadas.
- `/dashboard` no se convirtió en ruta privada real.
- No se instalaron dependencias.

### Validaciones Ejecutadas

- `npm run build` desde `src/LaboratorioTlahuac.Web`: correcto, sin warnings de presupuesto tras mover estilos públicos pesados a CSS global acotado.
- `dotnet build`: correcto, 0 warnings y 0 errores.
- `dotnet test`: correcto; Domain 1/1, Application 1/1 y API 90/90.
- `git diff --check`: correcto.
- `rg "/dashboard" .`: revisado; no muestra `/dashboard` como ruta privada real nueva, las menciones corresponden a documentación, API o `/app/dashboard`.
- `rg "/app/dashboard" .`: revisado; confirma que el dashboard privado real sigue bajo `/app`.
- `rg "/login" src/LaboratorioTlahuac.Web/src/app docs README.md AGENTS.md`: revisado; confirma `/login` como entrada pública y endpoints de auth existentes.
- `rg "prefers-reduced-motion" src/LaboratorioTlahuac.Web/src docs`: revisado; confirma soporte CSS/JS/documentación.
- `rg "gsap" src/LaboratorioTlahuac.Web src/LaboratorioTlahuac.Web/package.json docs`: sin resultados.
- Verificación de navegador/headless: no se encontró `chromium`, `google-chrome`, `firefox` ni Playwright local en `node_modules`; revisión visual real queda pendiente.

## 2026-05-23 - Fase 2.1d Diagnóstico Y Corrección Mínima De Dashboard

### Cambio Realizado

Se diagnosticó el estado `Cargando dashboard...` en `/app/dashboard` y se aplicó una corrección mínima en frontend para evitar carga indefinida cuando la consulta del resumen no termina.

No se modificaron `AuthService`, guards, rutas privadas, cookies, XSRF, backend, endpoints, permisos, seed, migraciones, deploy, dependencias ni `appsettings`.

### Archivos Leídos

- `AGENTS.md`
- `README.md`
- `docs/PROJECT_STATUS.md`
- `docs/03-architecture/AUTH_FLOW.md`
- `docs/03-architecture/ARCHITECTURE.md`
- `docs/01-product/internal-system.md`
- `docs/IMPLEMENTATION_LOG.md`
- `src/LaboratorioTlahuac.Web/src/app/app.routes.ts`
- `src/LaboratorioTlahuac.Web/src/app/core/guards/auth.guard.ts`
- `src/LaboratorioTlahuac.Web/src/app/core/guards/permission.guard.ts`
- `src/LaboratorioTlahuac.Web/src/app/core/auth/auth.service.ts`
- `src/LaboratorioTlahuac.Web/src/app/auth/pages/login/login-page.component.ts`
- `src/LaboratorioTlahuac.Web/src/app/features/dashboard/dashboard-page.component.ts`
- `src/LaboratorioTlahuac.Web/src/app/features/dashboard/dashboard.service.ts`
- `src/LaboratorioTlahuac.Api/Endpoints/DashboardEndpoints.cs`
- `src/LaboratorioTlahuac.Infrastructure/Dashboard/DashboardService.cs`
- `src/LaboratorioTlahuac.Domain/Security/Permissions.cs`

### Archivos Modificados

- `src/LaboratorioTlahuac.Web/src/app/features/dashboard/dashboard-page.component.ts`
- `docs/PROJECT_STATUS.md`
- `docs/IMPLEMENTATION_LOG.md`
- `docs/03-architecture/AUTH_FLOW.md`
- `docs/01-product/internal-system.md`
- `docs/08-qa/RESPONSIVE_CHECKLIST.md`

### Diagnóstico

- `/app/dashboard` sigue protegido por `permissionGuard` con `reports.view`.
- `GET /api/dashboard/summary` sigue protegido en backend con `Permissions.ReportsView`.
- El Admin seed recibe `reports.view` porque `SecuritySeeder` asigna todos los permisos de `Permissions.All`.
- El dashboard solo consulta `GET /api/dashboard/summary`.
- El componente ya apagaba `isLoading` con `finalize` para respuestas correctas o errores HTTP.
- La causa probable del estado persistente es una llamada pendiente a `GET /api/dashboard/summary`: sin timeout, el observable no completa ni falla y `isLoading` permanece activo.

### Corrección

- Se agregó timeout de 15 segundos a `DashboardPageComponent.load()`.
- Si `GET /api/dashboard/summary` tarda demasiado, el dashboard apaga `isLoading` y muestra un error controlado.
- No se cambió la estructura visual del dashboard ni se agregaron modulos.

### Endpoints Revisados

- `GET /health`: `200`.
- `GET /api/auth/csrf`: `204`.
- `GET /api/auth/me` sin sesión: `401`.
- `GET /api/dashboard/summary` sin sesión: `401`.
- `GET /api/auth/me` autenticado: pendiente porque `LT_ADMIN_EMAIL` y `LT_ADMIN_PASSWORD` no están disponibles en el proceso de Codex.
- `GET /api/dashboard/summary` autenticado: pendiente por la misma razón.
- Logout autenticado: pendiente por la misma razón.

### Ambiente

- Contenedor SQL usado/documentado: `ldt-labdental-sql`.
- Puerto SQL documentado: `14336 -> 1433/tcp`.
- No se usó `codex-cobranza-sql`.
- API y frontend estaban activos en `http://localhost:5277` y `http://localhost:4200`.
- No hay navegador/headless disponible sin instalar dependencias.

### Validaciones

- `npm run build` desde `src/LaboratorioTlahuac.Web`: correcto.
- `dotnet test`: correcto; Domain 1/1, Application 1/1 y API 90/90.
- `dotnet build`: primer intento falló por bloqueo temporal de `MvcTestingAppManifest.json` al ejecutarse en paralelo con `dotnet test`; repetido en serial, correcto con 0 warnings y 0 errores.
- `git diff --check`: correcto.
- Búsquedas obligatorias de rutas: `/dashboard`, `/app/dashboard` y `/login` revisadas; `/dashboard` no aparece como ruta privada real nueva.
- Búsquedas obligatorias de secretos: `LT_ADMIN_PASSWORD`, `LDT_SQL_SA_PASSWORD` y `ConnectionStrings` revisadas; solo aparecen nombres de variables, placeholders o cadenas locales/redactadas, no valores reales de contraseña.

### Seguridad

- No se ejecutó `dotnet user-secrets list`.
- No se imprimieron secretos.
- No se modificaron `appsettings*.json` con contraseñas.
- No se instalaron dependencias.

## 2026-05-23 - Fase 2.1c Cierre Parcial Por Validación Manual De Login

### Cambio Realizado

Se actualizó la documentación con la validación manual del login real usando el Admin local creado por seed. No se modificó código, backend, frontend, auth, guards, cookies, XSRF, endpoints, migraciones, deploy ni dependencias.

### Resultado Manual Reportado

- `/login` carga correctamente.
- Login con Admin local: validado.
- Redirección a `/app/dashboard`: validada.
- Dashboard: no validado; cargó una vez, pero al regresar a la página queda en `Cargando dashboard...`.
- `GET /api/auth/me` autenticado: no confirmado porque el resultado manual no fue marcado como `sí`.
- Logout: no confirmado como acción independiente porque el resultado manual no fue marcado.
- Después de logout, `/app/dashboard` redirige a `/login?returnUrl=%2Fapp%2Fdashboard`.

### Confirmaciones De Rutas

- `/login` sigue documentado como público.
- `/app` y `/app/dashboard` siguen documentadas como rutas privadas.
- `/dashboard` sigue documentado como no ruta privada real.

### Seguridad

- No se ejecutó `dotnet user-secrets list`.
- No se imprimieron secretos.
- No se usó `codex-cobranza-sql`.
- SQL correcto documentado: `ldt-labdental-sql` en puerto `14336`.
- No se modificaron `appsettings*.json` con contraseñas.

### Validaciones Técnicas

- `npm run build` desde `src/LaboratorioTlahuac.Web`: correcto.
- `dotnet build`: correcto, 0 warnings y 0 errores.
- `dotnet test`: correcto; Domain 1/1, Application 1/1 y API 90/90.
- `git diff --check`: correcto.

## 2026-05-23 - Fase 2.1c Validación SQL Dedicado, Seed Y Auth Anónima

### Cambio Realizado

Se validó el entorno local dedicado de Laboratorio Dental Tláhuac contra `ldt-labdental-sql` sin usar `codex-cobranza-sql`, sin listar user-secrets, sin imprimir secretos y sin modificar backend, frontend, auth, guards, endpoints, migraciones, deploy, dependencias ni `appsettings` con contraseñas.

Solo se actualizaron documentos de estado para registrar los resultados.

### Contenedor Y Base

- Contenedor usado: `ldt-labdental-sql`.
- Puerto usado: `14336`, mapeado a `1433/tcp`.
- Base validada por EF: `LaboratorioTlahuac_Dev`.
- `docker ps --filter "name=ldt-labdental-sql"` confirmó el contenedor activo.
- `docker port ldt-labdental-sql` confirmó el mapeo `1433/tcp -> 0.0.0.0:14336` y `1433/tcp -> [::]:14336`.

### Migraciones

- Proyecto EF: `src/LaboratorioTlahuac.Infrastructure/LaboratorioTlahuac.Infrastructure.csproj`.
- Startup project: `src/LaboratorioTlahuac.Api/LaboratorioTlahuac.Api.csproj`.
- `dotnet ef migrations list` listó:
  - `20260508044157_InitialSecurityModel`
  - `20260509004819_AddCustomersAndInternalDoctors`
  - `20260509022531_AddWorkOrders`
  - `20260509053231_AddPayments`
- `dotnet ef database update` terminó correctamente y reportó que no había migraciones pendientes.

### Seed Admin

- La API se levantó con `dotnet run --project src/LaboratorioTlahuac.Api/LaboratorioTlahuac.Api.csproj`.
- La ruta de seed se ejecutó al inicio porque `SecuritySeed:RunOnStartup` estaba activo en la configuración efectiva.
- La configuración Admin estuvo disponible para la API desde user-secrets; los logs solo mostraron consultas parametrizadas, no valores.
- Al terminar, se apagó el seed con `dotnet user-secrets set SecuritySeed:RunOnStartup false --project src/LaboratorioTlahuac.Api/LaboratorioTlahuac.Api.csproj`.

### API/Auth

- `GET /health`: `200`.
- `GET /api/auth/csrf`: `204`.
- `GET /api/auth/me` sin sesión: `401`.
- Login real: pendiente porque `LT_ADMIN_EMAIL` y `LT_ADMIN_PASSWORD` no están disponibles en el proceso de Codex.
- `/api/auth/me` autenticado: pendiente por la misma razón.
- Logout: pendiente por la misma razón.
- `/api/auth/me` después de logout: pendiente por la misma razón.

### Validaciones Ejecutadas

- `npm run build` desde `src/LaboratorioTlahuac.Web`: correcto.
- `dotnet build`: correcto, 0 warnings y 0 errores.
- `dotnet test`: correcto; Domain 1/1, Application 1/1 y API 90/90.
- `git diff --check`: correcto.

### Seguridad

- No se ejecutó `dotnet user-secrets list`.
- No se imprimieron secretos.
- No se extrajeron credenciales Admin desde user-secrets para simular login.
- No se modificaron `appsettings*.json` con contraseñas.
- La API local se apagó después de la validación.

## 2026-05-18 - Fase 2.1c Preflight SQL Server Docker Dedicado

### Cambio Realizado

Se ejecutó el preflight para crear o usar una instancia SQL Server Docker dedicada del proyecto Laboratorio Dental Tláhuac sin usar contenedores de otros proyectos y sin imprimir secretos.

La ejecución se detuvo antes de crear el contenedor porque `LDT_SQL_SA_PASSWORD` no está definida en el proceso. No se inventó password, no se guardaron secretos y no se modificaron backend, frontend, auth, guards, cookies, XSRF, endpoints, rutas, deploy, dependencias, appsettings ni migraciones.

### Archivos Leídos

- `AGENTS.md`
- `README.md`
- `docs/PROJECT_STATUS.md`
- `docs/03-architecture/AUTH_FLOW.md`
- `docs/03-architecture/ARCHITECTURE.md`
- `docs/01-product/internal-system.md`
- `docs/IMPLEMENTATION_LOG.md`
- `docs/08-qa/RESPONSIVE_CHECKLIST.md`

### Archivos Modificados

- `docs/PROJECT_STATUS.md`
- `docs/IMPLEMENTATION_LOG.md`
- `docs/03-architecture/AUTH_FLOW.md`
- `docs/01-product/internal-system.md`
- `docs/08-qa/RESPONSIVE_CHECKLIST.md`

### Preflight Del Repo

- `pwd`: `/home/romanrfhack/code/labDental`.
- `git rev-parse --show-toplevel`: `/home/romanrfhack/code/labDental`.
- `git status --short`: sin cambios iniciales.
- `git diff --stat`: sin cambios iniciales.

### Preflight Docker

- Docker está disponible.
- Contenedores activos detectados: `codex-cobranza-sql`, `mysql-ipn` y `n8n`.
- `codex-cobranza-sql` pertenece a otro proyecto y no se usó.
- No se usaron `facturacion-mysqlit`, `mercadosfmcpa-sql`, `bigsmile-sql`, `opticsoft-h1007-sql-0424` ni otros contenedores de otros proyectos.
- `ldt-labdental-sql` no existe en este entorno.
- Puertos revisados: `14336`, `14337` y `14338` no aparecen en escucha; el puerto preferido sigue siendo `14336`.
- No se ejecutó `docker inspect` completo para evitar exponer variables de entorno.
- No se borraron contenedores ni volúmenes.

### Bloqueo Seguro

- `LDT_SQL_SA_PASSWORD` no está definida.
- Por regla de seguridad, no se creó `ldt-labdental-sql`.
- No se creó el volumen `ldt-labdental-sql-data`.
- No se configuró `ConnectionStrings:DefaultConnection` en user-secrets.
- No se ejecutó `dotnet user-secrets list`.
- No se escribió ningún secreto en documentación ni en `appsettings`.

Comandos para que el humano prepare la variable en su terminal local antes de reintentar:

```bash
read -s -p "Password local para sa de SQL Server LDT: " LDT_SQL_SA_PASSWORD
echo
export LDT_SQL_SA_PASSWORD
```

### Admin Local

- `LT_ADMIN_EMAIL` no está definida.
- `LT_ADMIN_PASSWORD` no está definida.
- `LT_ADMIN_FULL_NAME` existe en el proceso, pero no se usó porque seed/login quedaron bloqueados antes de crear SQL Server.
- No se inventaron credenciales Admin.

Comandos para que el humano prepare Admin local antes de validar login real:

```bash
read -p "Admin email local: " LT_ADMIN_EMAIL
export LT_ADMIN_EMAIL
read -s -p "Admin password local: " LT_ADMIN_PASSWORD
echo
export LT_ADMIN_PASSWORD
export LT_ADMIN_FULL_NAME="Administrador Local"
```

### Migraciones Y Login Real

- `dotnet ef migrations list` no se ejecutó en esta fase porque no hay contenedor/base local dedicada disponible.
- `dotnet ef database update` no se ejecutó.
- Seed Admin no se ejecutó.
- Login real no se validó.
- `/api/auth/me` autenticado no se validó.
- Logout no se validó.
- `/app/dashboard` sin sesión no se validó en navegador en esta fase.

### Validaciones Ejecutadas

- `npm run build` desde `src/LaboratorioTlahuac.Web`: correcto.
- `dotnet build`: correcto, 0 warnings y 0 errores.
- `dotnet test`: correcto; Domain 1/1, Application 1/1 y API 90/90.
- `git diff --check`: correcto.
- `rg "/dashboard" .`: no muestra `/dashboard` como ruta privada real; las menciones corresponden a documentación, API de dashboard o `/app/dashboard`.
- `rg "/app/dashboard" .`: confirma que la ruta privada real se mantiene bajo `/app`.
- `rg "/login" src/LaboratorioTlahuac.Web/src/app docs README.md AGENTS.md`: confirma `/login` como entrada pública y endpoint de auth.
- `rg "LT_ADMIN_PASSWORD" .`: solo muestra nombres de variable, placeholders o código de seed; no muestra valores reales.
- `rg "LDT_SQL_SA_PASSWORD" .`: solo muestra nombres de variable y comandos de preparación; no muestra valores reales.
- `rg "ConnectionStrings" src docs README.md`: no muestra una connection string local con password real.
- Revisión adicional de patrones `Password=`, `MSSQL_SA_PASSWORD` y `User Id=sa`: solo placeholders o connection strings redactadas.

### Estado Esperado Al Reintentar

- Contenedor: `ldt-labdental-sql`.
- Imagen: `mcr.microsoft.com/mssql/server:2022-latest`.
- Puerto local preferido: `14336`.
- Volumen: `ldt-labdental-sql-data`.
- Base local: `LaboratorioTlahuac_Dev`.
- Connection string efectiva esperada en user-secrets, redactada: `Server=localhost,14336;Database=LaboratorioTlahuac_Dev;User Id=sa;Password=<redacted>;TrustServerCertificate=True;Encrypt=True`.

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

Plantilla actualizada para preparar la base local con el contenedor dedicado de Fase 2.1c, sin guardar secretos en archivos versionados:

```bash
docker run --name ldt-labdental-sql -e "ACCEPT_EULA=Y" -e "MSSQL_PID=Developer" -e "MSSQL_SA_PASSWORD=$LDT_SQL_SA_PASSWORD" -p 14336:1433 -v ldt-labdental-sql-data:/var/opt/mssql -d mcr.microsoft.com/mssql/server:2022-latest
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "<connection-string-local-redacted>" --project src/LaboratorioTlahuac.Api/LaboratorioTlahuac.Api.csproj
dotnet ef database update --project src/LaboratorioTlahuac.Infrastructure/LaboratorioTlahuac.Infrastructure.csproj --startup-project src/LaboratorioTlahuac.Api/LaboratorioTlahuac.Api.csproj
```

Si ya existe `ldt-labdental-sql`, se debe iniciar ese contenedor dedicado y repetir `dotnet ef database update`. No usar contenedores de otros proyectos.

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
