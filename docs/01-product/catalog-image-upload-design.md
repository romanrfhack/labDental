# Diseño Operativo De Almacenamiento De Imágenes Del Catálogo

Fase 3.5.4.0, 2026-08-08. Esta fase define el almacenamiento persistente y el contrato operativo previo a implementar carga/reemplazo de imágenes desde `/app/admin/catalogo`.

Actualización Fase 3.5.4.1, 2026-08-08: el backend quedó implementado, desplegado y validado operativamente en DEV en commit `1b0384c414b54f541394dbe0e2f1e4a4d9329e93`, release `dev-42-1b0384c`. Existen POST/DELETE por producto, GET público, almacenamiento tipado, validación hasta 2 MB, nombres GUID y pruebas API aisladas. No se creó migración.

Actualización Fase 3.5.4.2, 2026-08-08: la UI quedó implementada localmente solo para productos en `/app/admin/catalogo`. Agrega multipart/XSRF, validación de archivo, preview local, reemplazo y desasociación; conserva assets heredados, modo readonly y el backend como autoridad. Deploy y QA end-to-end DEV quedan pendientes.

## Resultado De La Fase

Se adopta como estrategia MVP una carpeta persistente del ambiente, fuera de los releases de backend y frontend, y lectura pública a través de la API existente:

```text
almacenamiento: ${LDT_APP_ROOT}/shared/catalog-images
ruta DEV esperada: /var/www/laboratorio-tlahuac-dev/shared/catalog-images
ruta pública: GET /api/catalog/images/{fileName}
imagePath persistido: /api/catalog/images/{fileName}
```

La ruta concreta de DEV ya fue preparada como `/var/www/laboratorio-tlahuac-dev/shared/catalog-images`, con `www-data:www-data`, permisos `0750` y escritura validada para `www-data`. `CatalogImages__StoragePath` está configurado una sola vez en `/etc/laboratorio-tlahuac-dev/api.env`. La aplicación no hardcodea esa ruta ni imprime configuración sensible.

Esta estrategia mantiene funcionando las rutas heredadas `assets/catalog/products/...`. No requiere una tabla ni migración nueva: el `ImagePath` actual admite la ruta pública dentro de su límite de 300 caracteres. Backend y frontend aceptan exactamente la allowlist heredada o `/api/catalog/images/{32 hex}.{webp|jpg|jpeg|png}` y rechazan URLs/rutas arbitrarias.

## Alcance Y Exclusiones

Esta fase es únicamente análisis y documentación:

- No implementa endpoints ni escritura de archivos.
- No modifica frontend ni backend funcional.
- No crea migraciones ni dependencias.
- No modifica scripts, workflow, Nginx, systemd ni el VPS.
- No cambia autenticación, cookies, sesión, guards ni XSRF.
- No imprime secretos ni requiere inspeccionar valores sensibles.
- No convierte imágenes a WebP todavía.

## Evidencia Del Deploy DEV Actual

La evidencia disponible en `.github/workflows/deploy.yml` y `.github/scripts/deploy-lab.sh` confirma este layout lógico bajo `LDT_APP_ROOT`:

```text
${LDT_APP_ROOT}/
├── backend/
│   ├── releases/{releaseId}/
│   └── current -> releases/{releaseId}
├── frontend/
│   ├── releases/{releaseId}/
│   └── current -> releases/{releaseId}
├── migrations/
│   └── releases/{releaseId}.sql
├── logs/
└── shared/
```

Hallazgos confirmados por los scripts:

- El workflow compila backend y frontend, genera un script idempotente de migración y empaqueta `backend`, `frontend` y `migrations.sql` por release.
- El script remoto instala backend en `backend/releases/{releaseId}` y frontend en `frontend/releases/{releaseId}`.
- Los symlinks `backend/current` y `frontend/current` se cambian de forma independiente hacia el release nuevo y se restauran en rollback si falla health.
- El servicio API se reinicia después de cambiar los symlinks.
- Se conservan los cinco releases más recientes de backend y frontend; los anteriores se eliminan.
- El script ya ejecuta `mkdir -p ${LDT_APP_ROOT}/shared`, pero no crea `shared/catalog-images`, no asigna permisos específicos a esa subcarpeta y no la respalda.
- Los releases instalados reciben ownership `www-data:www-data`. El usuario efectivo real del servicio debe confirmarse antes de aplicar permisos a la carpeta compartida.

Límites de la evidencia:

- El valor real de `LDT_APP_ROOT` está configurado fuera del repositorio y no se imprimió.
- La existencia física y permisos actuales de `shared` en el VPS no se verificaron en esta fase; solo se confirma que el script intenta crearlo en cada deploy.
- La configuración exacta de Nginx no está versionada en este repositorio.

## Proxy Y Entrega Pública Esperados

DEV ya expone correctamente `GET /api/catalog/public` y el health público a través del mismo dominio. De ello se infiere que el reverse proxy enruta tráfico de API hacia ASP.NET Core. La configuración exacta de Nginx no está disponible para confirmarlo línea por línea.

La recomendación es servir los uploads mediante `GET /api/catalog/images/{fileName}` desde el backend. Mientras el proxy existente cubra `/api`, esta ruta no necesita un bloque nuevo de Nginx ni publicar directamente la carpeta del filesystem. Antes de habilitar 3.5.4.1 en DEV se debe validar este supuesto con una petición pública al endpoint nuevo; si el proxy usa una allowlist de rutas en vez de un prefijo `/api`, el ajuste de Nginx se tratará como cambio separado y documentado.

También se debe confirmar que el límite de body del proxy admite al menos 2 MB más el overhead de `multipart/form-data`. Esto no implica una ruta/alias nuevo; si el límite actual es menor, su ajuste será una preparación operativa explícita y documentada antes de probar upload.

No se recomienda servir `${LDT_APP_ROOT}/shared/catalog-images` mediante alias directo de Nginx en el MVP, porque duplicaría reglas de validación/headers y agregaría una dependencia de deploy innecesaria.

## Riesgo Que Se Resuelve

- Un archivo escrito en `backend/releases/{releaseId}` queda ligado a un release que será reemplazado y eventualmente eliminado por la poda automática.
- Un archivo escrito en `frontend/releases/{releaseId}` o a través de `frontend/current` tiene el mismo problema; `current` es un symlink móvil, no almacenamiento persistente.
- Un upload colocado en el artefacto Angular no existe en el siguiente release a menos que vuelva a empaquetarse.
- Guardar archivos sin límites permite agotar disco o memoria.
- Confiar solo en nombre/extensión permite publicar contenido que no es una imagen.
- Usar el nombre enviado por el cliente puede permitir colisiones, caracteres inseguros o path traversal.
- Borrar automáticamente la imagen anterior durante un reemplazo dificulta rollback y puede romper otros registros si una ruta se reutiliza.
- La base de datos y los archivos pueden quedar desincronizados si falla una de las dos operaciones.
- Sin backup, restaurar solo la base recuperaría `imagePath` que apuntan a archivos ausentes.

## Estrategia MVP Aprobada Para Implementación

### Ubicación Y Configuración

- Carpeta efectiva: `${LDT_APP_ROOT}/shared/catalog-images`.
- Ruta DEV esperada, pendiente de verificación: `/var/www/laboratorio-tlahuac-dev/shared/catalog-images`.
- La ruta se configura por ambiente; no se deriva de `backend/current` ni del content root del release.
- El proceso debe fallar de forma explícita y segura al subir si la configuración falta, la carpeta no existe o no tiene permisos de escritura. No debe hacer fallback silencioso a un release ni a `/tmp`.
- La lectura pública solo resuelve archivos dentro de esa raíz configurada.

### Formatos Y Tamaño

- Extensiones permitidas, sin distinguir mayúsculas: `.webp`, `.jpg`, `.jpeg`, `.png`.
- Tamaño máximo por archivo: 2 MB (`2,097,152` bytes).
- El límite debe comprobarse antes y durante la copia; no se debe confiar únicamente en `Content-Length`.
- MIME declarados permitidos: `image/webp`, `image/jpeg` e `image/png`, coherentes con la extensión normalizada.
- Además del `Content-Type`, el backend debe verificar la firma mínima del archivo: RIFF/WEBP para WebP, firma JPEG y firma PNG. Esta validación reduce archivos falsamente etiquetados; validación completa de decodificación/dimensiones queda fuera del MVP si no existe una dependencia segura ya disponible.
- Se prefiere que el usuario cargue WebP, pero el backend no convierte ni recomprime en 3.5.4.1.
- SVG, GIF, BMP, TIFF, PDF, archivos sin extensión y URLs externas se rechazan.

### Nombre Seguro Y Path Traversal

- El nombre original se usa, como máximo, para mensajes/auditoría segura; nunca como nombre físico ni parte de `imagePath`.
- El backend genera un nombre único no predecible, por ejemplo un GUID criptográficamente aleatorio en minúsculas más la extensión normalizada: `{guid:N}.webp`.
- `fileName` en el GET público debe ser un único segmento y coincidir con el patrón de nombre generado más una extensión permitida.
- Se rechazan separadores `/` y `\`, segmentos `.`/`..`, URL encoding equivalente, nombres absolutos, drive letters, query strings, fragments y caracteres fuera del patrón.
- Como segunda defensa, la ruta canónica resultante debe comenzar dentro de la raíz canónica configurada antes de abrir el archivo.
- No se aceptan `imagePath` con `http://`, `https://`, `//`, `data:`, `file:` u otros esquemas.

### Escritura Y Consistencia

Flujo recomendado para el upload/reemplazo:

1. Autorizar `catalog.manage` y validar XSRF.
2. Confirmar que el producto existe.
3. Validar presencia de una sola parte `file`, tamaño, extensión, MIME y firma.
4. Escribir por streaming a un archivo temporal con creación exclusiva dentro de la misma carpeta persistente.
5. Mover/renombrar atómicamente al nombre final una vez validada la copia.
6. Actualizar el `CatalogProduct.ImagePath` a `/api/catalog/images/{fileName}` y tocar `UpdatedAtUtc` mediante el flujo de dominio.
7. Si falla la actualización de base después de crear el archivo nuevo, intentar retirar únicamente ese archivo nuevo como compensación y registrar el error sin exponer rutas sensibles.
8. Responder con el `imagePath` efectivo y/o el DTO admin actualizado.

El reemplazo se registra en el producto mediante su `ImagePath` y `UpdatedAtUtc` existentes. El modelo actual no guarda `UpdatedByUserId`; Fase 3.5.4.1 no debe afirmar auditoría por usuario sin un cambio de modelo explícito. Puede registrar en logs operativos el id de producto y usuario autenticado, evitando nombre original, rutas internas y datos sensibles.

La imagen anterior no se borra automáticamente en el MVP. Queda como posible huérfana para permitir rollback y evitar romper referencias compartidas. La limpieza física se difiere hasta contar con inventario de referencias, backup probado y política de retención.

## Endpoints Propuestos

### Subir O Reemplazar Imagen De Producto

```text
POST /api/admin/catalog/products/{id}/image
Content-Type: multipart/form-data
parte requerida: file
autorización: catalog.manage
XSRF: requerido
```

Comportamiento:

- Crea un archivo con nombre seguro en almacenamiento persistente.
- Reemplaza el valor de `CatalogProduct.ImagePath`; no sobreescribe físicamente el archivo anterior.
- Devuelve `404` si el producto no existe; `400`/problema de validación si el archivo falta o no cumple; `413` si excede 2 MB; `401` sin sesión; `403` sin permiso; y error controlado si almacenamiento/configuración no están disponibles.

### Desasociar Imagen De Producto

```text
DELETE /api/admin/catalog/products/{id}/image
autorización: catalog.manage
XSRF: requerido
```

En el MVP, `DELETE` significa establecer `CatalogProduct.ImagePath` en `null` y actualizar `UpdatedAtUtc`; no elimina el archivo físico. Se prefiere este endpoint explícito sobre reutilizar el `PUT` completo. `PATCH .../image` con `imagePath: null` seguiría siendo una alternativa compatible, pero no se recomiendan ambos contratos a la vez.

### Leer Imagen Pública

```text
GET /api/catalog/images/{fileName}
autorización: pública
XSRF: no aplica
```

Comportamiento:

- Solo admite nombres generados por el servidor y extensiones permitidas.
- Devuelve `404` para nombre inválido o archivo inexistente sin revelar la ruta interna.
- Emite el `Content-Type` validado según formato/extensión y `X-Content-Type-Options: nosniff`.
- Puede usar cache público con una vigencia larga porque cada reemplazo obtiene un nombre único; el contenido de un nombre publicado debe ser inmutable.
- No lista directorios ni permite descargar archivos fuera de la raíz configurada.

Las imágenes estáticas actuales continúan con rutas `assets/catalog/products/...`; las imágenes nuevas usan `/api/catalog/images/...`. El endpoint público del catálogo puede devolver cualquiera de los dos formatos en `imagePath`.

## Permisos Y Acceso

- `POST /api/admin/catalog/products/{id}/image`: requiere `catalog.manage`.
- `DELETE /api/admin/catalog/products/{id}/image`: requiere `catalog.manage`.
- `GET /api/catalog/images/{fileName}`: público, sin autenticación.
- Los métodos mutables están bajo `/api` y conservan la política XSRF global.
- `catalog.view` por sí solo no permite upload, reemplazo ni desasociación.
- Admin obtiene `catalog.manage` por el baseline actual.
- `Repartidor` no recibe `catalog.view` ni `catalog.manage`, no ve la administración de catálogo y no puede invocar los endpoints mutables.
- El frontend solo controla visibilidad/UX; el backend sigue siendo la autoridad de autorización.

## Impacto Operativo En Deploy

Fase 3.5.4.1 requerirá preparar el ambiente antes de activar la UI:

1. Confirmar el valor real de `LDT_APP_ROOT` sin imprimir secretos.
2. Crear `${LDT_APP_ROOT}/shared/catalog-images` si no existe.
3. Confirmar el usuario y grupo efectivos del servicio API.
4. Asignar la carpeta a ese usuario/grupo con permisos mínimos: escritura para el servicio, lectura para el servicio y sin escritura pública. No se recomienda `777`.
5. Configurar `CatalogImages__StoragePath` en el archivo/gestor de ambiente fuera del repositorio.
6. Verificar espacio libre y capacidad de escribir/leer con una prueba controlada.
7. Confirmar que el límite de request body del proxy admite 2 MB más overhead multipart.
8. Desplegar backend y validar POST/GET/DELETE, health y persistencia después de un segundo deploy/rollback.

El script actual ya preserva `shared` porque solo poda `backend/releases`, `frontend/releases` y migraciones antiguas. Sin embargo, la creación de `catalog-images`, ownership y configuración todavía no están implementadas. Cualquier cambio posterior al script o al VPS debe documentarse en `docs/05-delivery/DEPLOYMENT.md` y validarse primero en DEV.

No se requiere cambio Nginx si el proxy actual enruta todo `/api`. Esta condición debe verificarse; no se asume como evidencia directa porque la configuración no está en el repositorio.

## Backup Y Restauración

Backup mínimo recomendado para DEV antes de habilitar uso real:

- Incluir `${LDT_APP_ROOT}/shared/catalog-images` en el respaldo operativo junto con la base de datos del mismo ambiente.
- Tomar backup manual antes de cambios de permisos, movimiento de carpeta, limpieza de huérfanos o restauraciones.
- Conservar archivos y base de datos de un mismo punto temporal para no restaurar `imagePath` sin su archivo.
- Guardar el respaldo fuera de la carpeta de releases y, preferentemente, fuera del mismo VPS.
- Restringir acceso al respaldo; aunque las imágenes serán públicas por endpoint, el backup no debe exponer otros datos ni configuraciones.
- Validar restauración en un ambiente no productivo: restaurar archivos, conservar nombres, verificar permisos y comprobar que una muestra de `imagePath` devuelve `200` y el tipo correcto.

No se define todavía una retención automatizada. Como baseline operativo se recomienda copia diaria si el cliente empieza a cargar imágenes con frecuencia, más un backup previo a deploy/cambio operativo. La frecuencia, destino, cifrado y retención final deben acordarse antes de producción.

## Decisiones Diferidas

- Conversión, redimensionado o recomprensión automática a WebP.
- Validación de dimensiones/píxeles mediante decoder dedicado.
- Upload de imagen representativa de sección.
- Galerías o múltiples imágenes por producto.
- Tabla de assets, deduplicación por hash y auditoría persistente por usuario.
- Eliminación física y job de limpieza de archivos huérfanos.
- CDN u object storage.
- Cuotas globales y alertas automáticas de disco.

## Criterios De Aceptación Para Fase 3.5.4.1

- La API usa almacenamiento configurado fuera de releases y nunca hace fallback a una carpeta efímera.
- Upload y desasociación requieren `catalog.manage` y XSRF.
- Repartidor recibe `403` autenticado y sin permiso; sin sesión se recibe `401`.
- El GET público no requiere auth y no permite path traversal.
- Solo se aceptan `.webp`, `.jpg`, `.jpeg` y `.png`, hasta 2 MB, con extensión, MIME y firma coherentes.
- El nombre físico es único y generado por servidor.
- `CatalogProduct.ImagePath` guarda `/api/catalog/images/{fileName}` y las rutas heredadas continúan funcionando.
- Reemplazar o desasociar no borra el archivo anterior.
- Un deploy posterior no pierde la imagen cargada.
- Existe backup manual documentado y una restauración de muestra antes de producción.

## Implementación Fase 3.5.4.1

- Opciones tipadas: `CatalogImagesOptions`, sección `CatalogImages`, propiedad `StoragePath` y variable de ambiente `CatalogImages__StoragePath`; el valor efectivo debe ser una ruta absoluta existente para no resolver contra content root/releases.
- Almacenamiento: `ICatalogImageStorage`/`CatalogImageStorage`; Application recibe `Stream` y metadatos seguros, sin depender de `IFormFile`.
- Upload: `POST /api/admin/catalog/products/{id}/image`, `multipart/form-data`, una sola parte `file`, `catalog.manage` y XSRF global.
- Desasociación: `DELETE /api/admin/catalog/products/{id}/image`; responde con el producto actualizado, toca `UpdatedAtUtc` y no borra el archivo.
- Lectura: `GET /api/catalog/images/{fileName}` público, solo nombre lowercase de 32 hex + extensión permitida, `404` seguro para nombre/archivo inválido, `X-Content-Type-Options: nosniff` y cache inmutable.
- Límite: `2,097,152` bytes. Exceso responde `413`; validación de archivo responde `400`; almacenamiento sin configuración/carpeta/acceso responde `503` sin revelar la ruta.
- Escritura: producto existente antes de almacenar; temporal exclusivo en la misma raíz; rename final sin overwrite; si falla `SaveChangesAsync`, se intenta borrar solo el archivo nuevo.
- Compatibilidad: `CatalogImagePathValidator` acepta `assets/catalog/products/...` y exactamente `/api/catalog/images/{fileName}`; continúa rechazando URLs externas, otras rutas `/api` y path traversal.
- Pruebas: carpeta temporal GUID aislada por factory y limpieza al terminar; cobertura de autorización, formatos, tamaño, firma, GET, DELETE, catálogo público y rol `Repartidor`.

## Implementación Fase 3.5.4.2

- `AdminCatalogService.uploadProductImage()` crea `FormData`, agrega una parte exacta `file`, obtiene headers XSRF y usa POST con credenciales sin fijar `Content-Type`.
- `clearProductImage()` usa DELETE con XSRF y credenciales.
- Solo productos existentes muestran file input; productos nuevos deben guardarse antes. Secciones conservan selección de asset existente.
- Validación UI: archivo no vacío, máximo `2_097_152` bytes, extensiones WebP/JPG/JPEG/PNG, MIME permitido y coherencia básica extensión/MIME. Backend conserva validación de firma.
- Preview local usa `URL.createObjectURL()` y revoca URLs al cambiar, cancelar, completar o destruir el componente.
- Upload y DELETE actualizan el producto local y el formulario sin un guardado adicional.
- `catalog.manage` ve controles mutables; `catalog.view` queda readonly.
- DELETE significa desasociar. Los archivos anteriores y huérfanos no se borran; cleanup queda para una fase futura.

## Pendiente Operativo DEV

El storage y backend DEV ya están preparados y activos. Falta desplegar 3.5.4.2, ejecutar el checklist de `docs/08-qa/catalog-image-upload-ui-qa.md` y confirmar que una imagen cargada sobrevive a otro deploy.

## Siguiente Fase Recomendada

Deploy DEV y primera prueba real end-to-end de upload/persistencia para Fase 3.5.4.2.
