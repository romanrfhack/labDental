# QA UI Upload De Imágenes De Catálogo

Fase 3.5.4.2 — UI para subir, reemplazar y desasociar imágenes de productos desde `/app/admin/catalogo`, 2026-08-08.

## Estado

Fase 3.5.4.2 desplegada en DEV y QA funcional parcial aprobado para upload, preview y render público. La persistencia entre releases y la desasociación final permanecen pendientes.

Commit UI desplegado: `f9acb0dfa973bd131ab2850c69105c4a90d84470`, con GitHub Actions `success`.

Backend DEV disponible y validado operativamente:

- Commit desplegado: `1b0384c414b54f541394dbe0e2f1e4a4d9329e93`.
- Release backend activo: `dev-42-1b0384c`.
- GitHub Actions: `success`.
- `/health`, `/api/catalog/public` y `/catalogo`: `200`.
- `GET` de una imagen inexistente: `404` esperado.
- Storage persistente: `/var/www/laboratorio-tlahuac-dev/shared/catalog-images`.
- Owner/group: `www-data:www-data`; permisos `0750`; el proceso `www-data` puede escribir.
- `CatalogImages__StoragePath` está configurado una sola vez en `/etc/laboratorio-tlahuac-dev/api.env`.

No se registran valores secretos en este documento.

## QA Funcional Parcial DEV — 2026-08-08

| Caso | Resultado |
| --- | --- |
| `GET /health` | `200` |
| `GET /api/catalog/public` | `200` |
| `GET /catalogo` | `200` |
| Admin crea producto | OK |
| Admin edita producto | OK |
| Upload desde `/app/admin/catalogo` | OK |
| Preview de imagen | OK |
| Producto público muestra la imagen cargada | OK |
| Archivo físico creado | `29b1d5f129af436a80b0c951555299d2.jpg` |
| Tamaño del archivo | `86688` bytes |
| Owner/group y modo del archivo | `www-data:www-data`, `0644` |
| Storage | `/var/www/laboratorio-tlahuac-dev/shared/catalog-images` |
| Owner/group y modo del directorio | `www-data:www-data`, `0750` |

La imagen continúa asociada al producto para ejecutar la prueba posterior a otro deploy. Esto confirma la asociación y lectura actuales, pero todavía no confirma persistencia entre releases.

## Cobertura Implementada

- `AdminCatalogService.uploadProductImage()` envía `FormData` con una sola parte `file`, XSRF y `withCredentials: true`; no establece `Content-Type` manualmente.
- `AdminCatalogService.clearProductImage()` usa DELETE con XSRF y `withCredentials: true`.
- El editor de producto existente muestra origen, preview, selector de asset heredado, file input y acciones de subir/reemplazar/quitar.
- Producto nuevo informa: `Guarda el producto antes de subir una imagen personalizada.` y no intenta upload sin id.
- File input limitado a WebP/JPG/JPEG/PNG, un archivo, máximo `2_097_152` bytes y coherencia básica extensión/MIME.
- Preview local usa `URL.createObjectURL()` y revoca la URL al cambiar, cancelar, completar o destruir el componente.
- Upload/DELETE actualizan el producto local y el `imagePath` del formulario sin requerir guardar otra vez el producto.
- El guardado acepta únicamente assets de `CATALOG_IMAGE_OPTIONS`, `null` o `/api/catalog/images/{32 hex}.{webp|jpg|jpeg|png}`; rechaza URLs y rutas arbitrarias.
- Controles mutables visibles solo con `catalog.manage`; `catalog.view` permanece readonly. Backend conserva la autoridad.
- La selección de imagen de sección continúa usando assets existentes; no existe upload de secciones.
- DELETE desasocia; no borra el archivo físico. Limpieza de huérfanos queda para una fase futura.

## Checklist Manual DEV — Admin

- [x] 1. Abrir `/app/admin/catalogo` con Admin.
- [x] 2. Editar un producto existente.
- [x] 3. Elegir PNG/JPG/WebP de hasta 2 MB.
- [x] 4. Confirmar preview local antes de enviar.
- [x] 5. Pulsar `Subir imagen` o `Reemplazar imagen`.
- [x] 6. Confirmar preview servido desde `/api/catalog/images/...` después del upload.
- [ ] 7. Recargar `/app/admin/catalogo`.
- [ ] 8. Confirmar que la imagen persiste.
- [x] 9. Abrir `/catalogo`.
- [x] 10. Confirmar que la imagen nueva aparece públicamente.
- [ ] 11. Reemplazarla con otra imagen válida.
- [ ] 12. Confirmar que `imagePath` cambia a otro GUID.
- [ ] 13. Confirmar que la imagen nueva aparece en admin y público.
- [ ] 14. Pulsar `Quitar imagen` y aceptar `¿Quitar la imagen de este producto?`.
- [ ] 15. Confirmar `Imagen desasociada.` y estado `Sin imagen`.
- [ ] 16. Confirmar que `/catalogo` actualiza su comportamiento visual/placeholder.

## Checklist Manual DEV — Errores

- [ ] 17. Archivo mayor a 2 MB: UI bloquea antes del POST con `La imagen no puede superar 2 MB.`.
- [ ] 18. PDF/TXT/SVG/GIF: UI bloquea con `Formato no permitido. Usa WebP, JPG o PNG.`.
- [ ] 19. MIME incoherente, si es reproducible en navegador: UI bloquea y backend sigue validando firma real.
- [ ] 20. Storage no disponible: solo documentar respuesta amigable `503`; no romper la configuración DEV para provocarla.

## Checklist Manual DEV — Seguridad

- [ ] 21. Usuario con `catalog.view` sin `catalog.manage` puede leer pero no ve upload, reemplazo ni quitar.
- [ ] 22. `Repartidor` no ve navegación de catálogo y no accede a `/app/admin/catalogo`.
- [ ] 23. Confirmar que POST/DELETE continúan rechazando falta de sesión/permiso y validaciones inválidas; la UI no sustituye la autoridad backend.

## Checklist Manual DEV — Persistencia

- [x] 24. Después de upload, verificar que el archivo existe en `/var/www/laboratorio-tlahuac-dev/shared/catalog-images` sin imprimir contenido sensible ni rutas adicionales.
- [ ] 25. Ejecutar otro deploy DEV y confirmar que la URL sigue sirviendo la imagen; si no se ejecuta ahora, conservar esta prueba como paso obligatorio posterior.

## Estados HTTP Esperados En UI

| Estado | Mensaje/acción |
| --- | --- |
| `400` | Archivo o datos inválidos. |
| `401` | El manejo global de sesión conserva la redirección; la página no duplica navegación. |
| `403` | `No tienes permiso para administrar el catálogo.` |
| `404` | `El producto ya no existe.` |
| `413` | `La imagen no puede superar 2 MB.` |
| `503` | `El almacenamiento de imágenes no está disponible temporalmente.` |
| Otro POST | `No fue posible subir la imagen.` |
| Otro DELETE | `No fue posible quitar la imagen.` |

## Validaciones Locales

- `npm run build`: correcto; initial total `318.75 kB`, sin warning de budget.
- El repositorio no tiene script `npm test`; no se agregó framework.
- `dotnet build`: correcto con 0 errores; permanecen los warnings `NU1903` conocidos en tests.
- `dotnet test`: correcto; 158/158 pruebas.
- `git diff --check`: correcto.
- Búsquedas obligatorias ejecutadas; patrones sensibles limitados a nombres de archivo.

## Siguiente Paso

Ejecutar otro deploy DEV y confirmar que `29b1d5f129af436a80b0c951555299d2.jpg` continúa asociado y servido. Después, validar por separado la desasociación/DELETE final y su comportamiento público. Ninguno de esos dos puntos se considera aprobado todavía.
