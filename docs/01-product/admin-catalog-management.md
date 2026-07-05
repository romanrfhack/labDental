# Administración De Catálogo, Precios E Imágenes

Backlog futuro para Laboratorio Dental Tláhuac.

## Estado

Fase 3.5.1 ya implementó el backend/base de datos, permisos, seed inicial y endpoints de catálogo. La UI privada de administración todavía no está implementada.

El catálogo público actual en `/catalogo` sigue funcionando con datos estructurados del frontend en `src/LaboratorioTlahuac.Web/src/app/public/data/catalog-data.ts` hasta Fase 3.5.3. No debe cambiarse a consumo de API antes de validar la transición.

Actualización Fase 3.2: la administración de catálogo permanece como backlog. No debe bloquear el flujo operativo de órdenes, etiquetas y reparto. El MVP de etiquetas desde órdenes existentes ya quedó implementado; antes del catálogo sigue conviniendo validar impresión real y, si el cliente lo prioriza, avanzar flujo mobile-first de entrega/repartidor.

Actualización Fase 3.5.0: el diseño técnico quedó documentado en `docs/01-product/catalog-admin-design.md`. La recomendación MVP es crear backend/base de datos con seed desde `catalog-data.ts`, exponer endpoints público/admin, administrar precios/secciones/productos bajo `/app` y permitir solo selección de imágenes existentes al inicio. La carga de imágenes desde admin queda recomendada para una fase posterior con política de almacenamiento y backup.

Actualización Fase 3.5.1: el backend quedó implementado con `CatalogSection`, `CatalogProduct`, migración `20260705054221_AddCatalogManagement`, seed idempotente desde `catalog-data.ts`, `GET /api/catalog/public` sin autenticación y endpoints admin bajo `/api/admin/catalog/sections` y `/api/admin/catalog/products`. Los permisos reales son `catalog.view` y `catalog.manage`; Admin recibe ambos por baseline, `Repartidor` no recibe permisos de catálogo. No se creó UI admin, no se implementó upload y no se modificó `/catalogo`.

QA DEV Fase 3.5.1, 2026-07-05: commit `ebcf6e54b77ec6c5afaafdf8c21afc77213bf9d8` desplegado con GitHub Actions `success`; `/health` respondió `200`, `/api/catalog/public` sin sesión respondió `200`, `/catalogo` respondió `200` y los endpoints admin `/api/admin/catalog/sections` y `/api/admin/catalog/products` sin sesión respondieron `401`. Observación para Fase 3.5.2: revisar visualmente rutas heredadas con `yacket` y doble punto al seleccionar imágenes existentes.

## Propósito Futuro

Permitir que usuarios autorizados administren secciones, productos, precios e imágenes del catálogo desde la app privada, sin exponer edición en el sitio público.

## Alcance Tentativo

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

- Subir imagen.
- Reemplazar imagen.
- Eliminar imagen.
- Definir imagen específica de producto.
- Definir imagen representativa de sección.
- Validar peso, formato y tamaño.
- Preferir WebP cuando aplique.

## Seguridad

- Acceso solo para usuarios autorizados.
- Permisos implementados: `catalog.view` para lectura admin y `catalog.manage` para mutaciones.
- La edición no debe exponerse en el sitio público.
- Cualquier definición de permisos, guards, sesión o autorización debe revisar `docs/03-architecture/AUTH_FLOW.md` y `docs/03-architecture/ARCHITECTURE.md` antes de implementar.

## Arquitectura A Definir

Antes de implementar se debe definir:

- Migración del catálogo desde `catalog-data.ts` a backend/base de datos. Implementado en Fase 3.5.1 con seed inicial idempotente.
- Modelo de datos para secciones, productos, precios, imágenes, estados y ordenamiento. Implementado en Fase 3.5.1 con `CatalogSection` y `CatalogProduct` con `ImagePath` simple para MVP.
- Endpoints requeridos y reglas de autorización. Implementado en Fase 3.5.1: `GET /api/catalog/public` y endpoints privados bajo `/api/admin/catalog`.
- Almacenamiento de imágenes: local, cloud storage o CDN. Recomendación Fase 3.5.0: mantener assets estáticos existentes para MVP y diferir upload.
- Reglas de validación de imagen: peso, formato, dimensiones y nombres. Recomendación Fase 3.5.0: aplicar cuando se implemente Fase 3.5.4 de carga de imágenes.
- Historial de cambios de precios, si el cliente lo requiere.
- Flujo de aprobación antes de publicar cambios, si aplica.

## Publicación

- Evaluar si deben existir cambios guardados y cambios publicados.
- Documentar que los precios públicos requieren aprobación del cliente.
- Definir reglas para publicar, retirar o programar cambios visibles en `/catalogo`.

## Fuera De Alcance Actual

Después de Fase 3.5.1, sigue fuera de alcance:

- No se crean pantallas admin todavía.
- No se crea ruta `/app/admin/catalogo` todavía.
- No se modifica frontend funcional.
- No se modifica `AuthService`.
- No se modifican guards.
- No se implementa upload de imágenes.
- No se instalan dependencias.
- No se cambia deploy.
- No se modifica el catálogo público actual.
- No se elimina `catalog-data.ts`.
- No se mueve ningún asset.

## Prioridad Recomendada

La secuencia sugerida después de Fase 3.5.0 es:

1. Fase 3.5.1: backend catálogo administrable + migración + seed inicial desde `catalog-data.ts`. Implementada.
2. Fase 3.5.2: UI admin de catálogo/precios con selección de imagen existente.
3. Fase 3.5.3: `/catalogo` público consume API con manejo de error/fallback de transición.
4. Fase 3.5.4: carga/reemplazo de imágenes desde admin con política de almacenamiento y backup.

El catálogo requiere modelo de datos, endpoints, almacenamiento de imágenes, permisos y reglas de publicación; por eso no conviene mezclarlo con el MVP operativo de entrega.
