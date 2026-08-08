# Administración De Catálogo, Precios E Imágenes

Definición funcional y estado de la administración de catálogo de Laboratorio Dental Tláhuac.

## Estado

Fase 3.5.2 implementó y cerró en DEV la UI privada de administración de catálogo y precios bajo `/app/admin/catalogo`, consumiendo endpoints admin existentes. Fase 3.5.3 conectó `/catalogo` público a `GET /api/catalog/public` con fallback a `catalog-data.ts` y quedó cerrada en DEV el 2026-08-08.

El catálogo público actual en `/catalogo` consulta la API pública cuando la respuesta contiene secciones y productos válidos. Si la API falla, tarda demasiado, devuelve respuesta nula, secciones vacías, catálogo sin productos o datos con forma inválida, la UI conserva el catálogo estructurado del frontend en `src/LaboratorioTlahuac.Web/src/app/public/data/catalog-data.ts`.

QA DEV Fase 3.5.3: commit `8be9e14ec8cda5e8486770a77733a4413e456e96`, GitHub Actions `success`, `/health` `200`, `/catalogo` `200` y `/api/catalog/public` sin sesión `200`. La activación/desactivación de productos y los cambios de nombre/precio realizados desde `/app/admin/catalogo` se reflejaron correctamente en `/catalogo`. No se ejecutó una prueba forzada del fallback con API bloqueada/offline en DEV.

Actualización Fase 3.2: la administración de catálogo permanece como backlog. No debe bloquear el flujo operativo de órdenes, etiquetas y reparto. El MVP de etiquetas desde órdenes existentes ya quedó implementado; antes del catálogo sigue conviniendo validar impresión real y, si el cliente lo prioriza, avanzar flujo mobile-first de entrega/repartidor.

Actualización Fase 3.5.0: el diseño técnico quedó documentado en `docs/01-product/catalog-admin-design.md`. La recomendación MVP es crear backend/base de datos con seed desde `catalog-data.ts`, exponer endpoints público/admin, administrar precios/secciones/productos bajo `/app` y permitir solo selección de imágenes existentes al inicio. La carga de imágenes desde admin queda recomendada para una fase posterior con política de almacenamiento y backup.

Actualización Fase 3.5.1: el backend quedó implementado con `CatalogSection`, `CatalogProduct`, migración `20260705054221_AddCatalogManagement`, seed idempotente desde `catalog-data.ts`, `GET /api/catalog/public` sin autenticación y endpoints admin bajo `/api/admin/catalog/sections` y `/api/admin/catalog/products`. Los permisos reales son `catalog.view` y `catalog.manage`; Admin recibe ambos por baseline, `Repartidor` no recibe permisos de catálogo. No se creó UI admin, no se implementó upload y no se modificó `/catalogo`.

QA DEV Fase 3.5.1, 2026-07-05: commit `ebcf6e54b77ec6c5afaafdf8c21afc77213bf9d8` desplegado con GitHub Actions `success`; `/health` respondió `200`, `/api/catalog/public` sin sesión respondió `200`, `/catalogo` respondió `200` y los endpoints admin `/api/admin/catalog/sections` y `/api/admin/catalog/products` sin sesión respondieron `401`. Observación para Fase 3.5.2: revisar visualmente rutas heredadas con `yacket` y doble punto al seleccionar imágenes existentes.

Actualización Fase 3.5.2: la UI admin quedó implementada como ruta privada `/app/admin/catalogo`, protegida por `catalog.view` en el router actual. La navegación privada muestra `Catálogo` a usuarios con `catalog.view` o `catalog.manage`. Usuarios con `catalog.view` pueden listar secciones/productos en modo solo lectura; usuarios con `catalog.manage` pueden crear, editar, activar/desactivar, actualizar precios y seleccionar `imagePath` desde una allowlist local de assets `.webp` existentes. La UI muestra preview cuando hay imagen y marca rutas heredadas con `yacket` o doble punto como observación visual. No se implementó upload, no se modificó `/catalogo`, no se crearon migraciones y no se tocó backend.

QA DEV Fase 3.5.2, 2026-07-05: commit `e89d1f0b872d253838dc77f5df5fafb61522f9db` desplegado con GitHub Actions `success`; `/health` respondió `200`; los endpoints admin `/api/admin/catalog/sections` y `/api/admin/catalog/products` sin sesión respondieron `401`. Admin quedó validado para login, navegación `Catálogo`, carga de `/app/admin/catalogo`, listado de secciones/productos, filtro por sección, creación/edición/activación de secciones y productos, actualización de precio, bloqueo de precio negativo, selección/preview/limpieza de `imagePath` existente. `/catalogo` público siguió funcionando y `Repartidor` quedó validado sin navegación ni acceso a `/app/admin/catalogo`. Observaciones: sin hallazgos ni bug claro reportado.

Actualización Fase 3.5.3, 2026-07-10: `/catalogo` público queda preparado para reflejar cambios administrados desde `/app/admin/catalogo` mediante `GET /api/catalog/public`, siempre que el endpoint responda con datos válidos. `catalog-data.ts` no se elimina y queda como fallback de transición. No se implementa upload, no se modifica UI admin, no se crean migraciones y no se toca auth, guards, cookies, XSRF ni deploy.

Actualización Fase 3.5.4.0, 2026-08-08: **CERRADA**. El diseño operativo de almacenamiento quedó definido en `docs/01-product/catalog-image-upload-design.md`; los uploads viven en `${LDT_APP_ROOT}/shared/catalog-images`, fuera de releases, se leen mediante `GET /api/catalog/images/{fileName}` y el producto guarda esa ruta pública en `ImagePath`.

Actualización Fase 3.5.4.1, 2026-08-08: **CERRADA EN DEV**. El backend de upload/reemplazo/desasociación está desplegado en commit `1b0384c414b54f541394dbe0e2f1e4a4d9329e93`; el storage persistente está preparado para `www-data` y el flujo POST/GET/DELETE quedó validado end-to-end.

Actualización Fase 3.5.4.2, 2026-08-08: **CERRADA EN DEV**. `/app/admin/catalogo` está desplegado en commit `f9acb0dfa973bd131ab2850c69105c4a90d84470`, con GitHub Actions `success`. Admin puede crear/editar producto; selección, preview, upload, reemplazo, render público y quitar imagen quedaron aprobados.

Fase 3.5.4 completa: **QA end-to-end APROBADO EN DEV**. La persistencia se confirmó después del release backend `dev-44-8c2f92b`. Después de desasociar, API y catálogo público dejaron de referenciar/mostrar la imagen, mientras el archivo físico y su GET directo permanecieron disponibles, conforme a la decisión de no borrar físicamente en DELETE.

## Propósito

Permitir que usuarios autorizados administren secciones, productos, precios e imágenes del catálogo desde la app privada, sin exponer edición en el sitio público.

## Alcance Implementado

### Administración De Secciones

- Crear sección.
- Editar sección.
- Activar o desactivar sección.
- Ordenar secciones.

### Administración De Productos

- Crear producto.
- Editar nombre.
- Editar descripción.
- Editar precio.
- Activar o desactivar producto.
- Ordenar productos.
- Asignar producto a sección.

### Administración De Imágenes

- Subir/reemplazar imagen de producto mediante `POST /api/admin/catalog/products/{id}/image`.
- Desasociar imagen mediante `DELETE /api/admin/catalog/products/{id}/image`, sin borrar el archivo físico en el MVP.
- Definir imagen específica de producto.
- Mantener selección de asset existente para imagen representativa de sección; su upload queda en backlog.
- Validar máximo 2 MB, extensión, MIME y firma para `.webp`, `.jpg`, `.jpeg` y `.png`.
- Preferir WebP sin convertir todavía.

## Seguridad

- Acceso solo para usuarios autorizados.
- Permisos implementados: `catalog.view` para lectura admin y `catalog.manage` para mutaciones.
- Upload, reemplazo y desasociación requieren `catalog.manage` y XSRF; `catalog.view` no basta.
- `GET /api/catalog/images/{fileName}` es público; el rol `Repartidor` no accede a administración ni mutaciones.
- La edición no debe exponerse en el sitio público.
- Cualquier definición de permisos, guards, sesión o autorización debe revisar `docs/03-architecture/AUTH_FLOW.md` y `docs/03-architecture/ARCHITECTURE.md` antes de implementar.

## Arquitectura Definida/Implementada

Antes de implementar se debe definir:

- Migración del catálogo desde `catalog-data.ts` a backend/base de datos. Implementado en Fase 3.5.1 con seed inicial idempotente.
- Modelo de datos para secciones, productos, precios, imágenes, estados y ordenamiento. Implementado en Fase 3.5.1 con `CatalogSection` y `CatalogProduct` con `ImagePath` simple para MVP.
- Endpoints requeridos y reglas de autorización. Implementado en Fase 3.5.1: `GET /api/catalog/public` y endpoints privados bajo `/api/admin/catalog`.
- Almacenamiento de imágenes: definido en Fase 3.5.4.0 como `${LDT_APP_ROOT}/shared/catalog-images`, configurado por ambiente y fuera de releases. La ruta DEV `/var/www/laboratorio-tlahuac-dev/shared/catalog-images` quedó verificada con `www-data:www-data`, modo `0750`, y un upload real de `86688` bytes.
- Entrega pública: definida como `GET /api/catalog/images/{fileName}` para reutilizar el proxy de `/api`, sin requerir cambio Nginx si el prefijo completo ya se enruta al backend.
- Reglas de validación: máximo 2 MB, extensiones `.webp`, `.jpg`, `.jpeg`, `.png`, MIME/firma coherentes, nombre único generado por servidor, bloqueo de path traversal y rechazo de URLs externas.
- Persistencia: `ImagePath` acepta assets actuales y `/api/catalog/images/{fileName}`; reemplazo no borra el archivo anterior en el MVP.
- Backup: archivos y base deben respaldarse/restaurarse como un mismo punto temporal; limpieza de huérfanos queda diferida.
- Historial de cambios de precios, si el cliente lo requiere.
- Flujo de aprobación antes de publicar cambios, si aplica.

## Publicación

- Evaluar si deben existir cambios guardados y cambios publicados.
- Documentar que los precios públicos requieren aprobación del cliente.
- Definir reglas para publicar, retirar o programar cambios visibles en `/catalogo`.

## Fuera De Alcance De Fase 3.5.4

Durante Fase 3.5.4.2 queda fuera de alcance:

- No se modifica `AuthService`.
- No se modifican guards.
- No se implementa upload de secciones; conservan selección de asset existente.
- No se instalan dependencias.
- No se cambia deploy.
- No se elimina `catalog-data.ts`.
- No se mueve ningún asset.

## Prioridad Recomendada

La secuencia sugerida después de Fase 3.5.0 es:

1. Fase 3.5.1: backend catálogo administrable + migración + seed inicial desde `catalog-data.ts`. Implementada.
2. Fase 3.5.2: UI admin de catálogo/precios con selección de imagen existente. Implementada.
3. Fase 3.5.3: `/catalogo` público consume API con manejo de error/fallback de transición. Implementada localmente.
4. Fase 3.5.4.0: diseño operativo de almacenamiento persistente, endpoints, seguridad y backup. Cerrada.
5. Fase 3.5.4.1: backend upload/reemplazo/desasociación de imágenes de catálogo. Cerrada en DEV.
6. Fase 3.5.4.2: UI upload/reemplazo/desasociación desde `/app/admin/catalogo`. Cerrada en DEV; QA end-to-end completo aprobado.

El catálogo requiere modelo de datos, endpoints, almacenamiento de imágenes, permisos y reglas de publicación; por eso no conviene mezclarlo con el MVP operativo de entrega.

Backlog futuro, no bugs: inventario de huérfanos, política de retención, limpieza segura de no referenciados, backup automatizado, posible conversión/recompresión WebP, upload de sección y galería múltiple/CDN/cloud storage.
