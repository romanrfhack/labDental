# QA Catálogo Público API/Fallback

Fase 3.5.3 - `/catalogo` público consume `GET /api/catalog/public` con fallback local.

## Cierre QA DEV - 2026-08-08

Validación manual reportada por el responsable del proyecto y comprobación HTTP pública de cierre:

| Punto | Resultado |
| --- | --- |
| Commit desplegado | `8be9e14ec8cda5e8486770a77733a4413e456e96` |
| GitHub Actions | `success` |
| `GET /health` | `200` |
| `GET /catalogo` sin sesión | `200` |
| `GET /api/catalog/public` sin sesión | `200` |
| `/catalogo` carga sin login | OK |
| Secciones y productos visibles | OK |
| Precios con formato MXN | OK |
| Llamada a `/api/catalog/public` y consumo de datos administrados | OK |
| Activar/desactivar producto desde `/app/admin/catalogo` se refleja en `/catalogo` | OK |
| Cambiar nombre y precio desde `/app/admin/catalogo` se refleja en `/catalogo` | OK |
| `/app/admin/catalogo` mantiene edición de productos | OK |
| Imágenes | Sin falla reportada; no se documentó un pase visual exhaustivo por asset |
| Fallback con API bloqueada/offline en DEV | No probado de forma forzada |

Observaciones: el flujo principal admin → API pública → `/catalogo` quedó validado sin bug claro. La prueba forzada de degradación con API bloqueada/offline permanece como cobertura manual opcional; el fallback sigue implementado y fue validado localmente por build y revisión técnica. Esta limitación no bloquea el cierre QA DEV del camino principal de Fase 3.5.3.

El intento anterior correspondiente a `11ea0a296253d2e0a2660963430d49482dc4aaee` falló durante el health check posterior al restart, no por evidencia de falla funcional del catálogo. El siguiente commit, que incorporó el health check resiliente, desplegó correctamente.

Validación técnica repetida para el cierre documental:

- `npm run build`: correcto; initial total `317.77 kB`, sin warning de budget.
- `dotnet build`: correcto con 0 errores y 2 warnings `NU1903` conocidos.
- `dotnet test --no-build --verbosity normal`: correcto; Domain 1/1, Application 1/1 y API 140/140.

## Alcance

Validar que `/catalogo` use datos administrables cuando la API pública responde correctamente y que nunca quede vacío si la API no está disponible o devuelve una respuesta inválida.

## Comportamiento Esperado

- `/catalogo` consulta `GET /api/catalog/public` al cargar.
- Si la respuesta trae secciones y al menos un producto total con forma válida, la UI usa datos de API.
- Si hay error HTTP, timeout, respuesta nula, secciones vacías, catálogo sin productos o error de mapeo, la UI usa `catalog-data.ts`.
- Una sección sin productos puede mostrarse si la API la devuelve así y existe al menos otro producto total en el catálogo.
- Los precios se siguen formateando en MXN.
- Las imágenes siguen usando `imagePath` como ruta de asset.
- `altText` se conserva cuando viene de API.
- Si falta imagen, se mantiene el comportamiento de imagen de sección o placeholder visual.
- No se muestran errores técnicos al público.
- El aviso de fallback visible, si aparece, debe ser no alarmante: `Mostrando catálogo de referencia disponible.`.

## Validación Local

- `npm run build` desde `src/LaboratorioTlahuac.Web`: correcto; initial total `317.77 kB`, sin warning de budget.
- `dotnet build`: correcto con 0 errores y 2 warnings `NU1903` conocidos por `SQLitePCLRaw.lib.e_sqlite3` 2.1.11 en tests.
- `dotnet test`: correcto; Domain 1/1, Application 1/1 y API 140/140.
- `git diff --check`: correcto.
- Búsquedas obligatorias de catálogo, rutas públicas/privadas, permisos, upload, variables sensibles, `ConnectionStrings` y `codex-cobranza-sql`: ejecutadas; los patrones sensibles se revisaron con salida limitada a nombres de archivo.

No existe script frontend `test` en `src/LaboratorioTlahuac.Web/package.json`; por eso no se agregó prueba frontend automatizada en esta fase.

## Checklist Manual DEV

### API Correcta

- Abrir `https://dev.laboratoriodentaltlahuac.com/catalogo`.
- Confirmar que `GET /api/catalog/public` responde `200` sin sesión.
- Confirmar que se muestran secciones y productos administrados.
- Confirmar que cambios hechos en `/app/admin/catalogo` se reflejan en `/catalogo` después de recargar, si el backend DEV ya tiene esos cambios.
- Confirmar que precios se ven en MXN.
- Confirmar que `imagePath` conserva imágenes existentes.
- Confirmar que productos sin imagen usan imagen de sección o placeholder.
- Confirmar que no aparece mensaje técnico ni error visible.

### API Caída O Error HTTP

- Simular error HTTP de `GET /api/catalog/public` desde herramientas de navegador, proxy o ambiente controlado.
- Recargar `/catalogo`.
- Confirmar que se muestra el catálogo de referencia desde `catalog-data.ts`.
- Confirmar que la página no queda vacía y que el carrusel/galería/productos funcionan.
- Confirmar que el mensaje, si aparece, dice `Mostrando catálogo de referencia disponible.`.

### Timeout

- Simular que `GET /api/catalog/public` tarda más que el límite de carga del frontend.
- Confirmar que el estado `Actualizando catálogo...` no queda infinito.
- Confirmar que la UI vuelve a `catalog-data.ts`.

### Respuesta Vacía O Inválida

- Validar con una respuesta controlada como `{ "sections": [] }`.
- Validar con una respuesta con secciones pero sin ningún producto total.
- Validar con una respuesta sin `products` como arreglo.
- En todos los casos, confirmar fallback local y ausencia de errores técnicos visibles al usuario final.

### No Regresión De Admin Y Rutas

- Confirmar que `/app/admin/catalogo` mantiene su comportamiento existente.
- Confirmar que `/login` sigue cargando como entrada pública.
- Confirmar que `/app/dashboard` sigue siendo la ruta privada real.
- Confirmar que no existe `/dashboard` como ruta privada real.

## Exclusiones Confirmadas

- No se modificó backend.
- No se crearon migraciones.
- No se modificó UI admin.
- No se implementó upload de imágenes.
- No se eliminó `catalog-data.ts`.
- No se movieron assets.
- No se tocó `AuthService`.
- No se modificaron guards.
- No se tocaron cookies ni XSRF.
- No se tocó deploy.
- No se instalaron dependencias.

## Siguiente Fase Recomendada

Fase 3.5.4 - carga/reemplazo de imágenes desde admin, o pulido QA de catálogo público si DEV reporta hallazgos.
