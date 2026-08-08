# QA API Catálogo

Fase 3.5.1 - backend catálogo administrable + migración + seed inicial.

## Cierre QA DEV

Fecha: 2026-07-05.

- Commit desplegado: `ebcf6e54b77ec6c5afaafdf8c21afc77213bf9d8`.
- GitHub Actions: `success`.
- Working tree local previo reportado por responsable del proyecto: limpio.
- `GET /health`: `200`.
- `GET /api/catalog/public` sin sesión: `200`.
- `GET /api/catalog/public` devuelve secciones, productos, precios MXN e `imagePath`.
- `/catalogo` público en DEV: `200` por HTTP; sigue sirviendo el shell Angular público.
- `GET /api/admin/catalog/sections` sin sesión: `401`.
- `GET /api/admin/catalog/products` sin sesión: `401`.
- Resultado: Fase 3.5.1 cerrada en DEV sin bug bloqueante reportado.

Observación para siguiente fase: validar visualmente en la UI de selección de imágenes las rutas heredadas con nombres `yacket` y doble punto, especialmente `provisionales-yacket-*` y `protesis-removible-unidad-metalica..webp`. Se conservan como compatibilidad de assets existentes; no se normalizaron en esta fase.

Validación de cierre documental:

- `dotnet build`: correcto con 0 errores y 2 warnings `NU1903` conocidos por `SQLitePCLRaw.lib.e_sqlite3` 2.1.11 en tests.
- `dotnet test`: correcto; Domain 1/1, Application 1/1 y API 140/140.
- `npm run build`: correcto desde `src/LaboratorioTlahuac.Web`, initial total `314.59 kB`, sin warning de budget.
- `git diff --check`: correcto.

## Alcance Validado

- Entidades `CatalogSection` y `CatalogProduct`.
- Migración `20260705054221_AddCatalogManagement`.
- Seed idempotente desde `catalog-data.ts`.
- Permisos `catalog.view` y `catalog.manage`.
- Endpoint público `GET /api/catalog/public`.
- Endpoints admin bajo `/api/admin/catalog/sections` y `/api/admin/catalog/products`.
- Validación de precio no negativo.
- Validación de `ImagePath` como ruta relativa segura de assets.

## Endpoints

Público:

- `GET /api/catalog/public`: sin autenticación, devuelve solo secciones/productos activos y no expone campos administrativos.

Admin:

- `GET /api/admin/catalog/sections`: requiere `catalog.view` o `catalog.manage`.
- `POST /api/admin/catalog/sections`: requiere `catalog.manage`.
- `PUT /api/admin/catalog/sections/{id}`: requiere `catalog.manage`.
- `PATCH /api/admin/catalog/sections/{id}/status`: requiere `catalog.manage`.
- `GET /api/admin/catalog/products`: requiere `catalog.view` o `catalog.manage`; soporta `sectionId`.
- `POST /api/admin/catalog/products`: requiere `catalog.manage`.
- `PUT /api/admin/catalog/products/{id}`: requiere `catalog.manage`.
- `PATCH /api/admin/catalog/products/{id}/status`: requiere `catalog.manage`.
- `PATCH /api/admin/catalog/products/{id}/price`: requiere `catalog.manage`.

## Permisos

- Admin recibe `catalog.view` y `catalog.manage` por `Permissions.All` y sincronización baseline.
- `Repartidor` no recibe permisos `catalog.*`; conserva solo permisos mínimos de entregas.
- Los endpoints mutables quedan protegidos por XSRF centralizado para `/api`.

## Seed Inicial

- Fuente funcional: `src/LaboratorioTlahuac.Web/src/app/public/data/catalog-data.ts`.
- Resultado esperado: 12 secciones y 40 productos.
- Idempotencia: no duplica si ya existe `Key`.
- Backfill seguro: solo rellena `ImagePath`/`AltText` ausentes en registros existentes; no pisa precios, orden, nombres ni estado.
- No depende del filesystem y no copia archivos de imagen.

## Migración

- Nombre: `20260705054221_AddCatalogManagement`.
- Tablas creadas: `CatalogSections` y `CatalogProducts`.
- FK: `CatalogProducts.CatalogSectionId` hacia `CatalogSections.Id` con delete restrictivo.
- Índices: `CatalogSections.Key` único, `CatalogProducts.Key` único, `SortOrder`, `IsActive` y `CatalogSectionId`.
- No toca tablas ajenas y no elimina datos existentes.
- No se aplicó migración en VPS durante esta fase.

## Pruebas Automatizadas

Archivo: `tests/LaboratorioTlahuac.Api.Tests/CatalogIntegrationTests.cs`.

Cobertura:

- `GET /api/catalog/public` devuelve `200` sin autenticación.
- Seed inicial devuelve 12 secciones y 40 productos.
- Público devuelve solo secciones activas.
- Público devuelve solo productos activos.
- Público respeta orden por `SortOrder` y nombre como fallback.
- Público no expone `id`, `isActive`, `sortOrder`, `createdAtUtc` ni `updatedAtUtc`.
- Admin sections sin sesión devuelve `401`.
- Admin sections sin permiso devuelve `403`.
- Admin puede listar, crear, actualizar y activar/desactivar secciones.
- Admin puede listar, crear, actualizar, activar/desactivar y cambiar precio de productos.
- Precio negativo devuelve `400`.
- `ImagePath` externo/inseguro devuelve `400`.
- Usuario `Repartidor` no puede listar ni crear catálogo admin.

## Exclusiones Confirmadas

- No se modificó `/catalogo` para consumir API.
- La UI admin se implementó después, en Fase 3.5.2, consumiendo estos endpoints existentes.
- No se implementó upload de imágenes.
- No se borró `catalog-data.ts`.
- No se movieron assets.
- No se cambiaron rutas públicas.
- No se tocó `AuthService`, guards, cookies ni XSRF.
- No se tocó deploy.
- No se instalaron dependencias.

## Relación Con Fase 3.5.2

La UI privada `/app/admin/catalogo` usa los endpoints admin documentados aquí. La validación API de autorización sigue siendo la autoridad: `catalog.view` o `catalog.manage` para lectura y `catalog.manage` para mutaciones. La transición pública de `/catalogo` a `GET /api/catalog/public` se implementa en Fase 3.5.3 sin cambiar este contrato backend.

## Relación Con Fase 3.5.3

Fase 3.5.3 conecta `/catalogo` público a `GET /api/catalog/public` sin cambiar el contrato backend. El frontend valida la forma de la respuesta y usa `catalog-data.ts` como fallback si el endpoint devuelve error HTTP, tarda demasiado, responde nulo, no trae secciones, no trae ningún producto total o rompe el mapeo esperado.

La API sigue siendo pública y no requiere sesión. Los endpoints admin siguen protegidos y no se exponen desde la UI pública.

### Cierre QA DEV Fase 3.5.3 - 2026-08-08

- Commit desplegado: `8be9e14ec8cda5e8486770a77733a4413e456e96`.
- GitHub Actions: `success`.
- `GET /health`, `GET /catalogo` y `GET /api/catalog/public` sin sesión respondieron `200`.
- El responsable del proyecto activó y desactivó productos, cambió nombre y precio desde `/app/admin/catalogo`, y confirmó que esos cambios se reflejaron correctamente en `/catalogo`.
- No se modificó el contrato del endpoint, backend, migraciones, permisos, auth, cookies ni XSRF para este cierre.
