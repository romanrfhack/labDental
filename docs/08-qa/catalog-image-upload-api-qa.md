# QA API Upload De Imágenes De Catálogo

Fase 3.5.4.1 — backend upload/reemplazo/desasociación, 2026-08-08.

## Cierre Operativo DEV

- Commit desplegado: `1b0384c414b54f541394dbe0e2f1e4a4d9329e93`.
- Release backend activo: `dev-42-1b0384c`.
- GitHub Actions: `success`.
- `/health`, `/api/catalog/public` y `/catalogo`: `200`.
- GET de imagen inexistente: `404` esperado.
- Storage: `/var/www/laboratorio-tlahuac-dev/shared/catalog-images`, owner/group `www-data:www-data`, permisos `0750`, escritura de `www-data` validada.
- `CatalogImages__StoragePath` configurado una sola vez en `/etc/laboratorio-tlahuac-dev/api.env`.

## QA Real Parcial Con UI Fase 3.5.4.2 — 2026-08-08

- Commit UI desplegado: `f9acb0dfa973bd131ab2850c69105c4a90d84470`; GitHub Actions `success`.
- `/health`, `/api/catalog/public` y `/catalogo`: `200`.
- POST real desde `/app/admin/catalogo`: OK; el producto público renderiza la imagen cargada.
- Archivo creado: `29b1d5f129af436a80b0c951555299d2.jpg`, `86688` bytes, `www-data:www-data`, modo `0644`.
- Storage: `/var/www/laboratorio-tlahuac-dev/shared/catalog-images`, `www-data:www-data`, modo `0750`.
- La imagen continúa asociada al producto para la prueba posterior a otro deploy.
- Estado: desplegada en DEV y QA funcional de upload/render público aprobado; persistencia entre releases y desasociación final pendientes.

Esta evidencia valida un POST/GET real y el almacenamiento físico actual. No valida todavía reemplazo, DELETE/desasociación ni supervivencia del archivo y su asociación después de cambiar de release.

## Alcance Automatizado

- `POST /api/admin/catalog/products/{id}/image` con `multipart/form-data` y una sola parte `file`.
- `DELETE /api/admin/catalog/products/{id}/image` para establecer `ImagePath = null` sin borrar archivo.
- `GET /api/catalog/images/{fileName}` público.
- Persistencia del producto y propagación a `GET /api/catalog/public`.
- Compatibilidad del validador con `assets/catalog/products/...` y `/api/catalog/images/{fileName}`.

## Matriz De Respuesta

| Caso | Esperado |
| --- | ---: |
| POST sin sesión, con XSRF válido | `401` |
| POST autenticado sin `catalog.manage` / Repartidor | `403` |
| POST producto inexistente | `404` |
| POST sin archivo, vacío o con múltiples archivos | `400` |
| Extensión/MIME/coherencia/firma inválida | `400` |
| Archivo mayor a 2,097,152 bytes | `413` |
| Storage vacío, inexistente o inaccesible | `503` genérico, sin ruta interna |
| Upload WebP/JPG/JPEG/PNG válido | `200` + producto actualizado |
| GET nombre inválido o archivo inexistente | `404` |
| GET imagen existente | `200`, MIME correcto y `nosniff` |
| DELETE sin sesión / sin permiso / producto inexistente | `401` / `403` / `404` |
| DELETE válido | `200`, `imagePath: null`; archivo aún legible por URL previa |

La decisión de la fase es usar `413 Payload Too Large` para el máximo de archivo y `503 Service Unavailable` para configuración/carpeta/permisos de almacenamiento no disponibles.

## Aislamiento De Pruebas

`TestApplicationFactory` crea una carpeta temporal con nombre GUID por instancia, sobreescribe `CatalogImages:StoragePath`, no usa assets del frontend ni `/var/www`, y elimina recursivamente solo esa carpeta aislada al disponer la factory. No se imprimen rutas sensibles en respuestas.

## Seguridad Cubierta

- Nombre físico independiente del original: GUID lowercase de 32 hex + extensión normalizada.
- Extensiones permitidas: `.webp`, `.jpg`, `.jpeg`, `.png`.
- MIME permitidos: `image/webp`, `image/jpeg`, `image/png`.
- Firma mínima: PNG, JPEG y RIFF/WEBP.
- Copia por streaming con comprobación de límite durante la escritura.
- Temporal exclusivo y rename final dentro del mismo `StoragePath`.
- Nombre público de un solo segmento y resolución canónica dentro de la raíz.
- URLs externas, otras rutas `/api`, query, traversal y nombres no generados rechazados.
- `catalog.manage` no se asigna a `Repartidor`; XSRF global se conserva para POST/DELETE.

## Checklist Operativo DEV Antes De Upload Real

- [ ] Confirmar `${LDT_APP_ROOT}` sin imprimir secretos.
- [x] Crear `/var/www/laboratorio-tlahuac-dev/shared/catalog-images` fuera de releases.
- [x] Confirmar usuario/grupo efectivo de la API y asignar permisos mínimos de lectura/escritura; no usar `777`.
- [x] Configurar `CatalogImages__StoragePath` fuera del repositorio.
- [x] Confirmar con el archivo probado que Nginx/proxy admite el multipart usado y enruta `/api/catalog/images/*`.
- [ ] Probar POST, GET, reemplazo y DELETE con Admin; confirmar `403` con Repartidor.
- [ ] Confirmar que el archivo sigue disponible tras otro deploy y rollback controlado.
- [ ] Respaldar/restaurar base y `shared/catalog-images` como un mismo punto temporal.

## Exclusiones

- La UI de Fase 3.5.4.2 está desplegada; su QA DEV parcial se documenta aparte.
- Sin migración, conversión WebP, recomprensión, limpieza de huérfanos ni borrado físico en DELETE.
- Sin cambios de `AuthService`, guards, cookies, política XSRF, deploy o dependencias.

## Resultado Local

- `CatalogIntegrationTests`: 27/27.
- Suite completa: Domain 1/1, Application 1/1 y API 156/156.
- `dotnet build`: 0 errores; permanecen 2 warnings `NU1903` conocidos en tests.
- `npm run build`: correcto, initial total `317.77 kB`.
- Backend y UI DEV desplegados; POST/GET real y render público aprobados. Persistencia entre releases, reemplazo y DELETE/desasociación final permanecen pendientes.
