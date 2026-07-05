# Diseño Técnico Catálogo Administrable

Fase 3.5.0 - análisis y documentación para administración de catálogo, precios e imágenes bajo `/app`.

Actualización Fase 3.5.1, 2026-07-05: el backend del catálogo administrable ya quedó implementado con migración, seed inicial, permisos y endpoints. La UI admin, el consumo de `/catalogo` desde API y el upload de imágenes siguen fuera de esta fase.

## Resumen Ejecutivo

El catálogo público actual de `/catalogo` ya funciona y no debe romperse. Hoy se renderiza desde datos estructurados estáticos en `src/LaboratorioTlahuac.Web/src/app/public/data/catalog-data.ts` y usa assets locales en `src/LaboratorioTlahuac.Web/src/assets/catalog/products/`.

La recomendación para el MVP administrable es migrar secciones y productos a backend/base de datos, sembrar la información inicial desde `catalog-data.ts`, exponer un endpoint público de solo lectura y crear endpoints privados de administración bajo `/api/admin/catalog`. Para reducir riesgo, el MVP debe permitir seleccionar una imagen existente por `imagePath`/`assetPath`, sin carga de archivos desde UI todavía. La carga/reemplazo de imágenes debe quedar para una fase posterior con política explícita de almacenamiento, validación y backup.

Siguiente fase implementable recomendada: Fase 3.5.2 - UI admin catálogo/precios con selección de imagen existente.

## Estado Actual

### Confirmaciones

- `/catalogo` público usa datos frontend estáticos desde `catalog-data.ts`.
- Existe backend de catálogo desde Fase 3.5.1.
- Existen entidades `CatalogSection` y `CatalogProduct`, migración `20260705054221_AddCatalogManagement` y endpoints de catálogo.
- No existe todavía UI de administración de precios o imágenes bajo `/app`.
- No existe carga de imágenes desde `/app`.
- Las imágenes del catálogo viven como assets locales.
- La app privada actual vive bajo `/app`.
- Los módulos admin actuales son `/app/admin/usuarios` y `/app/admin/roles`.
- `/login` sigue siendo la entrada pública al sistema privado.
- `/app/dashboard` es la ruta privada real del dashboard; `/dashboard` no es ruta privada real.

### Fuentes Actuales

- Data: `src/LaboratorioTlahuac.Web/src/app/public/data/catalog-data.ts`.
- Página pública: `src/LaboratorioTlahuac.Web/src/app/public/pages/catalog/catalog-page.component.ts`.
- Assets: `src/LaboratorioTlahuac.Web/src/assets/catalog/products/`.
- Ruta pública de assets esperada: `/assets/catalog/products/...`.
- Configuración Angular: hoy copia `src/assets/**/*.webp` hacia `assets`.
- API pública implementada: `GET /api/catalog/public`.
- API admin implementada: `/api/admin/catalog/sections` y `/api/admin/catalog/products`.
- `/catalogo` público todavía no consume la API; sigue usando `catalog-data.ts`.

### Inventario Del Catálogo Actual

- Secciones: 12.
- Productos: 40.
- Productos con imagen específica: 19.
- Productos que usan imagen representativa de sección: 16.
- Productos con placeholder visual: 5.
- Assets referenciados por data: 20.
- Assets no referenciados detectados: `metal-porcelana-corona-sing-ivoclar-1.webp` y `protesis-removible-unidad-acrilica.jpg`.
- Referencias faltantes: ninguna; todas las rutas usadas por `catalog-data.ts` existen físicamente.

| Sección | Key actual | Productos | Imagen representativa |
| --- | --- | ---: | --- |
| Zirconia | `zirconia` | 4 | `assets/catalog/products/zirconia-corona-estratificada.webp` |
| E-MAX | `emax` | 3 | `assets/catalog/products/emax-corona-estratificada.webp` |
| SIGNUM | `signum` | 4 | `assets/catalog/products/signum-corona.webp` |
| Metal-porcelana | `metal-porcelana` | 2 | `assets/catalog/products/metal-porcelana-corona-sing-ivoclar.webp` |
| Metálicos y auxiliares | `metalicos-auxiliares` | 4 | `assets/catalog/products/metalicos-incrustacion-metalica.webp` |
| Provisionales y guardas | `provisionales-guardas` | 3 | `assets/catalog/products/provisionales-guarda-oclusal-acrilico.webp` |
| Totally Natural by tcs | `totally-natural` | 4 | `assets/catalog/products/totally-natural-dentadura-total.webp` |
| iFlex by tcs | `iflex` | 3 | `assets/catalog/products/iflex-protesis-bilateral.webp` |
| Prostodoncia parcial y total | `prostodoncia-parcial-total` | 2 | `assets/catalog/products/prostodoncia-dentadura-total-luciton.webp` |
| Servicios prostodónticos | `servicios-prostodonticos` | 5 | Sin imagen |
| Prótesis removible metal-acrílico | `protesis-removible-metal-acrilico` | 2 | `assets/catalog/products/protesis-removible-unidad-metalica..webp` |
| Prótesis inmediata provisional | `protesis-inmediata-provisional` | 4 | `assets/catalog/products/protesis-inmediata-provisional.webp` |

| Sección | Producto | Key actual | Precio | Imagen efectiva actual | Modo |
| --- | --- | --- | ---: | --- | --- |
| Zirconia | Corona estratificada | `zirconia-corona-estratificada` | 1800 | `assets/catalog/products/zirconia-corona-estratificada.webp` | específica |
| Zirconia | Corona monolítica | `zirconia-corona-monolitica` | 1600 | `assets/catalog/products/zirconia-corona-monolitica.webp` | específica |
| Zirconia | Carilla | `zirconia-carilla` | 1600 | `assets/catalog/products/zirconia-corona-estratificada.webp` | sección |
| Zirconia | Incrustación | `zirconia-incrustacion` | 1600 | `assets/catalog/products/zirconia-corona-estratificada.webp` | sección |
| E-MAX | Corona estratificada | `emax-corona-estratificada` | 1600 | `assets/catalog/products/emax-corona-estratificada.webp` | específica |
| E-MAX | Carilla | `emax-carilla` | 1500 | `assets/catalog/products/emax-corona-estratificada.webp` | sección |
| E-MAX | Incrustación | `emax-incrustacion` | 1500 | `assets/catalog/products/emax-incrustacion.webp` | específica |
| SIGNUM | Corona | `signum-corona` | 1100 | `assets/catalog/products/signum-corona.webp` | específica |
| SIGNUM | Carilla | `signum-carilla` | 950 | `assets/catalog/products/signum-corona.webp` | sección |
| SIGNUM | Unidad de puente con malla | `signum-unidad-puente-malla` | 1300 | `assets/catalog/products/signum-corona.webp` | sección |
| SIGNUM | Incrustación | `signum-incrustacion` | 850 | `assets/catalog/products/signum-incrustacion.webp` | específica |
| Metal-porcelana | Corona d. Sing Ivoclar | `metal-porcelana-corona-sing-ivoclar` | 1350 | `assets/catalog/products/metal-porcelana-corona-sing-ivoclar.webp` | específica |
| Metal-porcelana | Corona Safir Kulzer | `metal-porcelana-corona-safir-kulzer` | 1250 | `assets/catalog/products/metal-porcelana-corona-sing-ivoclar.webp` | sección |
| Metálicos y auxiliares | Incrustación metálica | `metalicos-incrustacion-metalica` | 750 | `assets/catalog/products/metalicos-incrustacion-metalica.webp` | específica |
| Metálicos y auxiliares | Corona total metal cerámico | `metalicos-corona-total-metal-ceramico` | 800 | `assets/catalog/products/metalicos-corona-total-metal-ceramico.webp` | específica |
| Metálicos y auxiliares | Acetato rígido | `metalicos-acetato-rigido` | 230 | `assets/catalog/products/metalicos-incrustacion-metalica.webp` | sección |
| Metálicos y auxiliares | Acetato flexible | `metalicos-acetato-flexible` | 280 | `assets/catalog/products/metalicos-incrustacion-metalica.webp` | sección |
| Provisionales y guardas | Jacket acrílico provisional | `provisionales-jacket-acrilico-provisional` | 280 | `assets/catalog/products/provisionales-yacket-acrilico-provisional.webp` | específica |
| Provisionales y guardas | Jacket acrílico termocurable | `provisionales-jacket-acrilico-termocurable` | 500 | `assets/catalog/products/provisionales-yacket-acrilico-termocurable.webp` | específica |
| Provisionales y guardas | Guarda oclusal de acrílico | `provisionales-guarda-oclusal-acrilico` | 1200 | `assets/catalog/products/provisionales-guarda-oclusal-acrilico.webp` | específica |
| Totally Natural by tcs | Dentadura total c/u | `totally-natural-dentadura-total` | 3200 | `assets/catalog/products/totally-natural-dentadura-total.webp` | específica |
| Totally Natural by tcs | Prótesis bilateral | `totally-natural-protesis-bilateral` | 2900 | `assets/catalog/products/totally-natural-protesis-bilateral.webp` | específica |
| Totally Natural by tcs | Prótesis unilateral de 1 a 2 unidades | `totally-natural-protesis-unilateral-1-2` | 1500 | `assets/catalog/products/totally-natural-dentadura-total.webp` | sección |
| Totally Natural by tcs | Prótesis unilateral 3 unidades | `totally-natural-protesis-unilateral-3` | 1700 | `assets/catalog/products/totally-natural-dentadura-total.webp` | sección |
| iFlex by tcs | Prótesis bilateral | `iflex-protesis-bilateral` | 2900 | `assets/catalog/products/iflex-protesis-bilateral.webp` | específica |
| iFlex by tcs | Prótesis unilateral de 1 a 2 unidades | `iflex-protesis-unilateral-1-2` | 1500 | `assets/catalog/products/iflex-protesis-unilateral-1-2.webp` | específica |
| iFlex by tcs | Prótesis unilateral 3 unidades | `iflex-protesis-unilateral-3` | 1700 | `assets/catalog/products/iflex-protesis-bilateral.webp` | sección |
| Prostodoncia parcial y total | Dentadura total acrílico Luciton 199 c/u | `prostodoncia-dentadura-total-luciton` | 2900 | `assets/catalog/products/prostodoncia-dentadura-total-luciton.webp` | específica |
| Prostodoncia parcial y total | Dentadura total en acrílico Kulzer c/u | `prostodoncia-dentadura-total-kulzer` | 2700 | `assets/catalog/products/prostodoncia-dentadura-total-kulzer.webp` | específica |
| Servicios prostodónticos | Reparación de dentadura por fractura | `servicios-reparacion-dentadura-fractura` | 650 | Sin imagen | placeholder |
| Servicios prostodónticos | Gancho volado | `servicios-gancho-volado` | 300 | Sin imagen | placeholder |
| Servicios prostodónticos | Descanso metálico c/u | `servicios-descanso-metalico` | 250 | Sin imagen | placeholder |
| Servicios prostodónticos | Rebase | `servicios-rebase` | 1100 | Sin imagen | placeholder |
| Servicios prostodónticos | Aumentar dientes c/u | `servicios-aumentar-dientes` | 350 | Sin imagen | placeholder |
| Prótesis removible metal-acrílico | Unidad acrílica | `protesis-removible-unidad-acrilica` | 180 | `assets/catalog/products/protesis-removible-unidad-metalica..webp` | sección |
| Prótesis removible metal-acrílico | Unidad metálica | `protesis-removible-unidad-metalica` | 240 | `assets/catalog/products/protesis-removible-unidad-metalica..webp` | específica |
| Prótesis inmediata provisional | Prótesis de 1 unidad | `protesis-inmediata-1-unidad` | 500 | `assets/catalog/products/protesis-inmediata-provisional.webp` | sección |
| Prótesis inmediata provisional | Prótesis de 1 a 4 unidades | `protesis-inmediata-1-4-unidades` | 900 | `assets/catalog/products/protesis-inmediata-provisional.webp` | sección |
| Prótesis inmediata provisional | Prótesis de 1 a 9 unidades | `protesis-inmediata-1-9-unidades` | 1300 | `assets/catalog/products/protesis-inmediata-provisional.webp` | sección |
| Prótesis inmediata provisional | A partir de 10 unidades | `protesis-inmediata-10-unidades` | 1450 | `assets/catalog/products/protesis-inmediata-provisional.webp` | sección |

### Limitaciones Actuales

- Cambiar precios requiere editar código, reconstruir y desplegar frontend.
- Cambiar nombres, orden o activación requiere editar `catalog-data.ts`.
- No hay auditoría de quién cambió precios.
- No hay flujo de aprobación/publicación.
- No hay validación backend de precios ni rutas de imagen.
- No hay UI privada para administrar catálogo.
- El archivo `.jpg` existente no se copia por la configuración actual de Angular, que solo incluye `.webp`.
- La ruta `protesis-removible-unidad-metalica..webp` conserva doble punto por compatibilidad con el asset actual; debe tratarse como valor heredado hasta normalizarlo en una fase controlada.

## Objetivo Funcional

Permitir que usuarios autorizados administren secciones, productos, precios, estado activo, orden e imágenes del catálogo desde `/app`, sin exponer edición en el sitio público y sin romper `/catalogo`.

El objetivo del MVP es:

- Administrar secciones y productos desde la app privada.
- Editar precios como valores decimales.
- Activar/desactivar secciones y productos.
- Ordenar secciones y productos.
- Mantener una `key`/`slug` estable para compatibilidad y seed.
- Seleccionar una imagen existente por ruta de asset.
- Exponer a `/catalogo` solo datos activos, sin campos internos.

## Modelo Propuesto

### CatalogSection

Campos recomendados:

- `Id`: `Guid`.
- `Key`: `string`, único, estable, derivado inicialmente del `id` actual de `catalog-data.ts`.
- `Slug`: `string`, opcional si se decide separar URL pública de key interna; para MVP puede reutilizarse `Key`.
- `Name`: `string`, requerido.
- `Description`: `string?`, opcional.
- `ImagePath`: `string?`, ruta relativa pública o asset path, por ejemplo `assets/catalog/products/zirconia-corona-estratificada.webp`.
- `IsActive`: `bool`.
- `SortOrder`: `int`.
- `CreatedAtUtc`: `DateTimeOffset`.
- `UpdatedAtUtc`: `DateTimeOffset`.
- `UpdatedByUserId`: `Guid?`, FK opcional a `Security.Users`.

Índices recomendados:

- Único por `Key`.
- Índice por `IsActive`.
- Índice por `SortOrder`.

### CatalogProduct

Campos recomendados:

- `Id`: `Guid`.
- `CatalogSectionId`: `Guid`, FK requerida.
- `Key`: `string`, estable, derivado inicialmente del `id` actual del producto.
- `Slug`: `string`, opcional si se decide separar URL pública de key interna; para MVP puede reutilizarse `Key`.
- `Name`: `string`, requerido.
- `Description`: `string?`, opcional.
- `Price`: `decimal(18, 2)`, requerido.
- `ImagePath`: `string?`, imagen específica opcional del producto.
- `IsActive`: `bool`.
- `SortOrder`: `int`.
- `CreatedAtUtc`: `DateTimeOffset`.
- `UpdatedAtUtc`: `DateTimeOffset`.
- `UpdatedByUserId`: `Guid?`, FK opcional a `Security.Users`.

Índices recomendados:

- Único por `Key` o único por `(CatalogSectionId, Key)`. Para preservar la data actual, `Key` global único es suficiente en MVP.
- Índice por `CatalogSectionId`.
- Índice por `IsActive`.
- Índice por `SortOrder`.

### CatalogProductImage Opcional

Para MVP se recomiendan campos simples `ImagePath` en sección y producto. Una tabla `CatalogProductImage` solo conviene cuando se implemente carga de imágenes, galería múltiple, metadata o limpieza de archivos.

Si se agrega después, campos tentativos:

- `Id`: `Guid`.
- `CatalogProductId`: `Guid?`.
- `CatalogSectionId`: `Guid?`.
- `ImagePath`: `string`, requerido.
- `AltText`: `string?`.
- `IsPrimary`: `bool`.
- `SortOrder`: `int`.
- `CreatedAtUtc`: `DateTimeOffset`.
- `UpdatedAtUtc`: `DateTimeOffset`.
- `UpdatedByUserId`: `Guid?`.

Regla: una imagen debe pertenecer a producto o sección, no a ambos al mismo tiempo, salvo que se defina una tabla de assets reutilizables.

### Seed Inicial

Fase 3.5.1 debe sembrar desde el inventario actual:

- Crear 12 secciones con `Key` igual al `id` actual.
- Crear 40 productos con `Key` igual al `id` actual.
- Preservar precios numéricos actuales como `decimal(18, 2)`.
- Preservar rutas `imageUrl` actuales como `ImagePath`.
- Asignar `SortOrder` por orden actual en `catalog-data.ts`.
- Marcar todo como activo inicialmente.
- Mantener placeholders como productos activos sin `ImagePath`.
- Ejecutar seed de forma idempotente para no duplicar datos.

Implementación Fase 3.5.1:

- Seed idempotente en `CatalogSeeder`.
- `CatalogSeed:RunOnStartup=true`.
- Crea datos faltantes por `Key`.
- No sobreescribe precios, nombres, orden ni estado de registros existentes para evitar pisar cambios administrativos futuros.
- Solo rellena `ImagePath`/`AltText` ausentes en registros existentes.
- No depende de filesystem y no copia archivos de imagen.
- Las rutas se guardan como paths relativos existentes de assets.

## Permisos

Permisos propuestos:

- `catalog.view`: permite entrar a la administración privada del catálogo en modo lectura y consultar endpoints admin de catálogo.
- `catalog.manage`: permite crear, editar, activar/desactivar, ordenar, actualizar precios y seleccionar imágenes existentes.
- `catalog.publish`: opcional futuro si se implementa flujo de borrador/aprobación/publicación.

Implementación Fase 3.5.1:

- Se implementaron `catalog.view` y `catalog.manage`.
- No se implementó `catalog.publish`.
- Admin recibe ambos permisos por `Permissions.All`.
- `Repartidor` no recibe permisos `catalog.*`.

Asignación inicial recomendada:

- `Admin`: debe recibir `catalog.view`, `catalog.manage` y, si existe, `catalog.publish` mediante sincronización baseline de `Permissions.All`.
- `Repartidor`: no debe recibir permisos de catálogo.
- `Limited QA`: solo recibiría permisos de catálogo si se declaran explícitamente en su allowlist local de Development.

Ruta privada futura recomendada:

- `/app/admin/catalogo`, protegida con `catalog.view`.
- Las acciones mutables de la UI deben requerir `catalog.manage`.
- Si se implementa `catalog.publish`, las acciones de publicar cambios deben requerirlo explícitamente.

## Endpoints Públicos

Endpoint recomendado:

```text
GET /api/catalog/public
```

Contrato esperado:

- No requiere autenticación si lo consume `/catalogo` público.
- Solo devuelve secciones activas.
- Solo devuelve productos activos dentro de secciones activas.
- Ordena por `SortOrder` y luego por nombre/key como respaldo estable.
- No expone `CreatedAtUtc`, `UpdatedAtUtc`, `UpdatedByUserId`, flags internos, datos de usuarios ni ids internos si no son necesarios para la UI pública.
- Puede exponer `key`, `name`, `description`, `price`, `imagePath` y `products`.
- Debe usar valores decimales; el frontend puede seguir formateando MXN.

Respuesta conceptual:

```json
{
  "sections": [
    {
      "key": "zirconia",
      "name": "Zirconia",
      "description": null,
      "imagePath": "assets/catalog/products/zirconia-corona-estratificada.webp",
      "products": [
        {
          "key": "zirconia-corona-estratificada",
          "name": "Corona estratificada",
          "description": null,
          "price": 1800.00,
          "imagePath": "assets/catalog/products/zirconia-corona-estratificada.webp"
        }
      ]
    }
  ]
}
```

## Endpoints Privados

Base recomendada:

```text
/api/admin/catalog
```

Implementación Fase 3.5.1:

- `GET /api/admin/catalog/sections`
- `POST /api/admin/catalog/sections`
- `PUT /api/admin/catalog/sections/{id}`
- `PATCH /api/admin/catalog/sections/{id}/status`
- `GET /api/admin/catalog/products`
- `POST /api/admin/catalog/products`
- `PUT /api/admin/catalog/products/{id}`
- `PATCH /api/admin/catalog/products/{id}/status`
- `PATCH /api/admin/catalog/products/{id}/price`
- `GET /api/admin/catalog/products` soporta filtro `sectionId`.
- No se implementaron endpoints de upload, assets allowlist, sort-order dedicado ni patch image dedicado en esta fase.

Lectura:

- `GET /api/admin/catalog/sections`
- `GET /api/admin/catalog/sections/{id}`
- `GET /api/admin/catalog/products`
- `GET /api/admin/catalog/products/{id}`

Mutación de secciones:

- `POST /api/admin/catalog/sections`
- `PUT /api/admin/catalog/sections/{id}`
- `PATCH /api/admin/catalog/sections/{id}/status`
- `PATCH /api/admin/catalog/sections/{id}/sort-order`
- `PATCH /api/admin/catalog/sections/reorder` si se prefiere ordenar varias secciones en una sola operación.

Mutación de productos:

- `POST /api/admin/catalog/products`
- `PUT /api/admin/catalog/products/{id}`
- `PATCH /api/admin/catalog/products/{id}/status`
- `PATCH /api/admin/catalog/products/{id}/sort-order`
- `PATCH /api/admin/catalog/products/reorder` si se prefiere ordenar varios productos en una sola operación.
- `PATCH /api/admin/catalog/products/{id}/price`
- `PATCH /api/admin/catalog/products/{id}/image`

Imágenes existentes en MVP:

- `GET /api/admin/catalog/assets` para listar rutas permitidas de assets existentes.
- `PATCH /api/admin/catalog/sections/{id}/image` para seleccionar imagen representativa existente.
- `PATCH /api/admin/catalog/products/{id}/image` para seleccionar imagen específica existente.

Upload futuro:

- `POST /api/admin/catalog/images`
- `DELETE /api/admin/catalog/images/{id}` o endpoint de desasociación si se implementa tabla de imágenes.

Autorización:

- `GET` privados: `catalog.view`.
- Mutaciones: `catalog.manage`.
- Publicación explícita futura: `catalog.publish`.
- Métodos mutables siguen requiriendo XSRF por la política global de `/api`.

## Estrategia De Imágenes

### Opción A - Seleccionar Assets Estáticos Existentes

Descripción:

- Mantener imágenes en `src/LaboratorioTlahuac.Web/src/assets/catalog/products/`.
- Guardar en base solo `ImagePath`.
- Admin elige una ruta existente desde una lista permitida.
- No subir archivos desde UI todavía.

Ventajas:

- Menor esfuerzo.
- Menor riesgo de seguridad.
- No cambia almacenamiento del VPS.
- No requiere política de backup nueva.
- Reduce el riesgo de romper deploy.
- Permite administrar precios/orden/estado antes de resolver uploads.

Riesgos y limitaciones:

- No permite subir imágenes nuevas desde la UI.
- Agregar assets nuevos seguirá requiriendo entrega técnica/deploy.
- Hay que validar que el `ImagePath` seleccionado pertenezca a una allowlist.
- El `.jpg` actual no se publica con la configuración actual; el MVP debe preferir `.webp`.

### Opción B - Subir Imágenes Desde Admin

Descripción:

- UI privada permite subir/reemplazar imágenes.
- Backend recibe `multipart/form-data`.
- Archivos se guardan en carpeta local controlada del VPS.
- El servidor los expone como assets públicos.
- Se validan extensión, peso, MIME real y dimensiones.
- Se prefiere WebP.

Ventajas:

- El cliente puede reemplazar imágenes sin deploy.
- Resuelve placeholders más rápido.
- Permite evolucionar a galería y assets administrados.

Riesgos y esfuerzo:

- Requiere política de almacenamiento.
- Requiere backup y restauración de archivos subidos.
- Requiere límites de peso y validación fuerte de tipo real.
- Requiere naming seguro, prevención de path traversal y deduplicación.
- Requiere limpieza de imágenes huérfanas.
- Requiere definir cómo se sirven archivos en DEV y producción.
- Requiere pruebas adicionales de seguridad, tamaño y concurrencia.

### Recomendación MVP

Usar Opción A en Fases 3.5.1 a 3.5.3.

La carga de imágenes desde admin debe quedar para Fase 3.5.4, cuando se defina:

- Carpeta real de almacenamiento en VPS.
- Ruta pública servida.
- Tamaño máximo.
- Formatos aceptados.
- Conversión o preferencia WebP.
- Política de backup.
- Limpieza de huérfanos.
- Permisos y auditoría.

## Fases Recomendadas

### Fase 3.5.1 - Backend Catálogo + Migración + Seed Inicial

Alcance:

- Crear entidades `CatalogSection` y `CatalogProduct`.
- Crear migración.
- Agregar `DbSet` y configuraciones EF.
- Agregar permisos `catalog.view` y `catalog.manage`; `catalog.publish` solo si se decide desde esta fase.
- Asegurar que Admin reciba permisos por baseline.
- Asegurar que `Repartidor` no reciba permisos de catálogo.
- Crear seed inicial idempotente desde el catálogo actual.
- Implementar `GET /api/catalog/public`.
- Implementar endpoints admin mínimos de lectura/mutación.
- Agregar pruebas backend.

Exclusiones:

- No cambiar `/catalogo` público todavía.
- No implementar upload de imágenes.
- No crear UI admin todavía.

### Fase 3.5.2 - UI Admin De Catálogo/Precios

Alcance:

- Agregar ruta `/app/admin/catalogo`.
- Proteger ruta con `catalog.view`.
- Mostrar secciones/productos/precios/estado.
- Permitir crear/editar secciones y productos con `catalog.manage`.
- Permitir activar/desactivar.
- Permitir ordenar.
- Permitir actualizar precios.
- Permitir seleccionar `imagePath` existente desde allowlist.

Exclusiones:

- No cambiar `/catalogo` público todavía.
- No implementar upload de imágenes.

### Fase 3.5.3 - `/catalogo` Público Consume API

Alcance:

- Cambiar `/catalogo` para consumir `GET /api/catalog/public`.
- Mantener layout y comportamiento visual actual.
- Manejar error de API con estado controlado.
- Evaluar fallback temporal a `catalog-data.ts` durante transición DEV, si se quiere reducir riesgo UAT.
- Validar DEV en móvil y desktop.

Exclusiones:

- No implementar upload.
- No eliminar `catalog-data.ts` hasta cerrar transición y rollback plan.

### Fase 3.5.4 - Carga/Reemplazo De Imágenes Desde Admin

Alcance:

- Definir almacenamiento local controlado en VPS.
- Implementar endpoint upload.
- Validar extensión, MIME real, peso y dimensiones.
- Preferir WebP.
- Servir imágenes subidas como assets públicos.
- Asociar imágenes a sección/producto.
- Definir backup.
- Definir limpieza de huérfanos.

Exclusiones:

- No mezclar con migración inicial ni con primer cambio de `/catalogo` público.

## Riesgos

- Romper `/catalogo` público al cambiar de data estática a API.
- Duplicar fuente de verdad entre `catalog-data.ts` y base de datos.
- Publicar precios sin aprobación del cliente.
- Asignar permisos de catálogo a roles incorrectos.
- Dar acceso de catálogo al rol `Repartidor` por error de seed.
- Dejar imágenes huérfanas cuando exista upload.
- Guardar rutas de imagen no permitidas o manipuladas.
- No respaldar imágenes subidas en VPS.
- Ejecutar migraciones en DEV/producción sin plan y sin respaldo.
- Romper assets por la diferencia actual entre `.webp` publicado y `.jpg` no publicado.
- Exponer campos internos en endpoint público.
- Introducir flujo de publicación demasiado complejo antes de validar necesidad real con el cliente.

Mitigaciones recomendadas:

- Mantener `/catalogo` con data estática hasta que backend y UI admin estén validados.
- Seed idempotente con keys estables.
- Endpoint público solo con activos y DTO público reducido.
- Admin recibe permisos por `Permissions.All`; `Repartidor` queda con allowlist explícita sin `catalog.*`.
- Fase upload separada con backup antes de activar.
- Validar DEV antes de producción.
- Documentar aprobación de precios 2026 y condiciones comerciales antes de publicación formal.

## Criterios De Aceptación

Para Fase 3.5.0:

- Documento de diseño técnico creado.
- Catálogo actual inventariado.
- Estado actual confirmado.
- Modelo propuesto definido.
- Permisos propuestos definidos.
- Endpoints públicos y privados propuestos.
- Estrategia de imágenes comparada y recomendada.
- Fases 3.5.1 a 3.5.4 propuestas.
- Riesgos documentados.
- No se modifica código funcional.
- No se crean migraciones.
- No se instalan dependencias.
- No se toca deploy.

Para Fase 3.5.1:

- Migración de catálogo creada y revisada: `20260705054221_AddCatalogManagement`.
- Seed inicial crea 12 secciones y 40 productos.
- Seed conserva precios, keys, sort order e imágenes actuales.
- `GET /api/catalog/public` devuelve solo activos.
- Endpoints admin requieren permisos correctos.
- Admin tiene `catalog.view` y `catalog.manage`.
- Repartidor no tiene permisos de catálogo.
- Pruebas backend cubren público, admin, sin sesión y sin permiso.
- `/catalogo` público sigue funcionando con data estática porque aún no se migra el frontend público.

## Qué No Implementar Todavía

- Upload de imágenes en MVP inicial.
- Flujo avanzado de borrador/publicación si el cliente no lo valida.
- Historial completo de precios.
- CDN o storage cloud.
- Eliminación física de productos/secciones.
- Eliminación de `catalog-data.ts` antes de cerrar la transición pública.
- Cambios de deploy.
- Cambios de auth/cookies/XSRF fuera de registrar nuevos permisos.

## Validaciones Con Cliente

Antes de exponer cambios administrables al público, validar:

- Precios 2026 definitivos.
- Vigencia del catálogo.
- Si `Anticipo 50%` se publica como condición definitiva.
- Si `Trabajos urgentes +40%` se publica como condición definitiva.
- Qué usuarios podrán administrar precios.
- Si cambios de precio se publican inmediatamente o requieren aprobación.
- Qué imágenes faltantes deben reemplazarse primero.
- Si el cliente acepta un MVP sin upload y con selección de imágenes existentes.

## Siguiente Fase Implementable

Fase 3.5.1 - backend catálogo administrable + migración + seed inicial.

La implementación debe empezar por backend y permisos, manteniendo `/catalogo` público sin cambios hasta que los endpoints privados y públicos estén validados.
