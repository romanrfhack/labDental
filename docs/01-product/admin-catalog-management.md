# Administración De Catálogo, Precios E Imágenes

Backlog futuro para Laboratorio Dental Tláhuac.

## Estado

No pertenece a la fase actual y no está implementado.

Esta funcionalidad será una mejora futura de la app privada bajo `/app`. El catálogo público actual en `/catalogo` debe seguir funcionando con datos estructurados del frontend en `src/LaboratorioTlahuac.Web/src/app/public/data/catalog-data.ts` hasta que se diseñe, apruebe e implemente esta fase.

Actualización Fase 3.2: la administración de catálogo permanece como backlog. No debe bloquear el flujo operativo de órdenes, etiquetas y reparto. El MVP de etiquetas desde órdenes existentes ya quedó implementado; antes del catálogo sigue conviniendo validar impresión real y, si el cliente lo prioriza, avanzar flujo mobile-first de entrega/repartidor.

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
- Permiso futuro sugerido: `catalog.manage` o equivalente.
- La edición no debe exponerse en el sitio público.
- Cualquier definición de permisos, guards, sesión o autorización debe revisar `docs/03-architecture/AUTH_FLOW.md` y `docs/03-architecture/ARCHITECTURE.md` antes de implementar.

## Arquitectura A Definir

Antes de implementar se debe definir:

- Si el catálogo migra de `catalog-data.ts` a backend/base de datos.
- Modelo de datos para secciones, productos, precios, imágenes, estados y ordenamiento.
- Endpoints requeridos y reglas de autorización.
- Almacenamiento de imágenes: local, cloud storage o CDN.
- Reglas de validación de imagen: peso, formato, dimensiones y nombres.
- Historial de cambios de precios, si el cliente lo requiere.
- Flujo de aprobación antes de publicar cambios, si aplica.

## Publicación

- Evaluar si deben existir cambios guardados y cambios publicados.
- Documentar que los precios públicos requieren aprobación del cliente.
- Definir reglas para publicar, retirar o programar cambios visibles en `/catalogo`.

## Fuera De Alcance Actual

- No se crean pantallas.
- No se crean rutas.
- No se modifica backend.
- No se modifica frontend funcional.
- No se modifica auth.
- No se modifican guards.
- No se modifica base de datos.
- No se crean migraciones.
- No se crean endpoints.
- No se instalan dependencias.
- No se cambia deploy.
- No se modifica el catálogo público actual.

## Prioridad Recomendada

La secuencia sugerida después de Fase 3.2 es:

1. Validación real de etiquetas con impresora térmica.
2. Fase 3.3: entrega/repartidor mobile-first.
3. Fase 3.4: usuarios/roles.
4. Fase 3.5: administración de catálogo.

El catálogo requiere modelo de datos, endpoints, almacenamiento de imágenes, permisos y reglas de publicación; por eso no conviene mezclarlo con el MVP operativo de entrega.
