# QA UI Admin Catálogo

Fase 3.5.2 - UI admin catálogo/precios con selección de imagen existente.

## Cierre QA DEV - 2026-07-05

Validación manual reportada en DEV para la UI admin de catálogo/precios. Este cierre documenta el reporte del responsable del proyecto; Codex no re-ejecutó login ni pruebas autenticadas con credenciales reales.

Deploy validado:

| Punto | Resultado |
| --- | --- |
| Commit desplegado | `e89d1f0b872d253838dc77f5df5fafb61522f9db` |
| GitHub Actions | `success` |
| `GET /health` | `200` |
| `GET /api/admin/catalog/sections` sin sesión | `401` esperado para endpoint protegido |
| `GET /api/admin/catalog/products` sin sesión | `401` esperado para endpoint protegido |

Resultados funcionales reportados:

| Caso | Resultado |
| --- | --- |
| Login Admin | OK |
| Navegación privada muestra `Catálogo` para Admin | OK |
| `/app/admin/catalogo` carga correctamente | OK |
| Lista secciones | OK |
| Lista productos | OK |
| Filtro por sección | OK |
| Crear sección | OK |
| Editar sección | OK |
| Activar/desactivar sección | OK |
| Crear producto | OK |
| Editar producto | OK |
| Actualizar precio | OK |
| Precio negativo bloqueado | OK |
| Activar/desactivar producto | OK |
| Selección de `imagePath` existente | OK |
| Preview de imagen | OK |
| Limpiar imagen | OK |
| `/catalogo` público sigue funcionando | OK |
| `Repartidor` no ve `Catálogo` | OK |
| `Repartidor` no accede a `/app/admin/catalogo` | OK |

Observaciones reportadas: sin hallazgos ni bug claro. No se modificó código, backend, migraciones, `AuthService`, guards, cookies, XSRF, deploy ni dependencias para este cierre documental.

## Revalidación En Cierre Fase 3.5.3 - 2026-08-08

Con el commit `8be9e14ec8cda5e8486770a77733a4413e456e96` desplegado mediante GitHub Actions `success`, el responsable del proyecto revalidó `/app/admin/catalogo` y confirmó:

- Activar un producto se refleja correctamente en `/catalogo`.
- Desactivar un producto se refleja correctamente en `/catalogo`.
- Cambiar el nombre de un producto se refleja correctamente en `/catalogo`.
- Cambiar el precio de un producto se refleja correctamente en `/catalogo`, conservando el formato MXN.

Resultado: integración admin → API pública → catálogo público OK, sin bug claro reportado. Esta revalidación no requirió cambios en UI admin, backend, migraciones, auth, guards, cookies, XSRF, permisos, deploy ni dependencias.

## Alcance Validado Por Código

- Ruta privada: `/app/admin/catalogo`.
- Navegación privada: `Catálogo` visible con `catalog.view` o `catalog.manage`.
- Ruta protegida con `catalog.view` por limitación actual de `permissionGuard` a un permiso por ruta.
- Acciones mutables visibles solo con `catalog.manage`.
- `Repartidor` no debe ver navegación de catálogo porque no tiene permisos `catalog.*`.
- Usuario sin sesión debe seguir redirigido a `/login`.
- Usuario autenticado sin permiso debe seguir redirigido a `/app/access-denied`.
- `/catalogo` público consume `GET /api/catalog/public` desde Fase 3.5.3 y conserva `catalog-data.ts` como fallback.
- No se implementó upload de imágenes.
- No se crearon migraciones.
- No se modificó backend, `AuthService`, guards, cookies, XSRF, deploy ni dependencias.

## UI Implementada

- Resumen de secciones/productos y activos.
- Listado responsive de secciones con tabla desktop y cards móviles.
- Crear/editar sección con nombre, clave técnica, descripción, orden, estado, `imagePath` y `altText`.
- Activar/desactivar sección.
- Listado responsive de productos con tabla desktop y cards móviles.
- Filtros de productos por sección y estado.
- Crear/editar producto con sección, nombre, clave técnica, descripción, precio, moneda MXN readonly, orden, estado, `imagePath` y `altText`.
- Activar/desactivar producto.
- Actualización rápida de precio mediante endpoint dedicado.
- Selección de imagen desde allowlist local `catalog-image-options.ts`.
- Preview cuando existe `imagePath`.
- Opción `Sin imagen` para limpiar `imagePath`.
- Observación visual para rutas heredadas con `yacket` y doble punto.

## Validaciones Locales

- `npm run build` desde `src/LaboratorioTlahuac.Web`: correcto; initial total `317.27 kB`, sin warning de budget.
- `dotnet build`: correcto con 0 errores y 2 warnings `NU1903` conocidos por `SQLitePCLRaw.lib.e_sqlite3` 2.1.11 en tests.
- `dotnet test`: correcto en ejecución serial; Domain 1/1, Application 1/1 y API 140/140.
- Primer `dotnet test` paralelo con `dotnet build` falló por bloqueo temporal de archivo en `obj`, patrón ya conocido en el repo; se repitió en serial correctamente.

## Checklist Manual DEV

- Admin ve `Catálogo` en navegación privada.
- Admin abre `/app/admin/catalogo` y lista secciones/productos.
- Admin filtra productos por sección y estado.
- Admin crea una sección con nombre, orden y estado activo.
- Admin edita sección, selecciona imagen existente, ve preview y limpia imagen.
- Admin desactiva y reactiva sección.
- Admin crea producto con sección, nombre, precio no negativo y moneda MXN.
- Admin edita producto, cambia sección/orden/imagen/alt text y ve preview.
- Admin actualiza precio desde acción rápida y ve el precio actualizado.
- Admin desactiva y reactiva producto.
- Capturar precio negativo en UI debe bloquear envío.
- Usuario con `catalog.view` sin `catalog.manage` abre `/app/admin/catalogo` en solo lectura y no ve acciones mutables.
- Usuario `Repartidor` no ve `Catálogo` y al abrir `/app/admin/catalogo` termina en `/app/access-denied`.
- Usuario sin sesión al abrir `/app/admin/catalogo` termina en `/login`.
- Confirmar que `/catalogo` público sigue mostrando el catálogo actual desde `catalog-data.ts`.

## Observaciones

- El asset `protesis-removible-unidad-acrilica.jpg` existe en carpeta fuente, pero la configuración Angular actual copia `src/assets/**/*.webp`; por eso la allowlist UI usa los assets `.webp` existentes.
- Las rutas `provisionales-yacket-*` y `protesis-removible-unidad-metalica..webp` se conservan por compatibilidad con assets existentes y se marcan visualmente como heredadas.
- Fase 3.5.3 se implementó localmente sin modificar esta UI admin: `/catalogo` público consume `GET /api/catalog/public` y conserva fallback a `catalog-data.ts`.
- Fase siguiente recomendada: Fase 3.5.4 - carga/reemplazo de imágenes desde admin, o pulido QA de catálogo público si DEV reporta hallazgos.
