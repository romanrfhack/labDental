# Backup Y Restore

Última sincronización: **2026-08-22 — DOC-SYNC-1**.

## Estado Actual

El proyecto usa SQL Server y además mantiene archivos persistentes del catálogo fuera de los releases.

Unidad lógica de recuperación por ambiente:

```text
Base de datos SQL Server
+
shared/catalog-images
```

Un backup de solo uno de los dos componentes puede dejar referencias de imágenes inconsistentes después de una restauración. Siempre que existan uploads administrados, ambos deben respaldarse con una referencia temporal coherente.

## DEV

Estado conocido:

- SQL Server operativo para DEV.
- Storage persistente `${LDT_APP_ROOT}/shared/catalog-images` validado.
- Persistencia de imágenes entre releases validada.
- La desasociación no borra físicamente archivos.

El cierre funcional de Fase 3.5.4 no requiere limpieza automática de huérfanos.

## Producción

Estado: **pendiente de preparación**.

Antes del primer release productivo se requiere:

1. Base SQL Server productiva creada.
2. Ubicación protegida de backups definida.
3. `shared/catalog-images` productivo creado con permisos mínimos.
4. Backup de BD previo a migraciones.
5. Backup del storage de imágenes.
6. Restore probado en ambiente no productivo.
7. Política de frecuencia y retención acordada.
8. Procedimiento de verificación después del restore.

## Política Mínima Recomendada

Para la primera operación productiva:

- Backup diario de SQL Server.
- Backup diario de `shared/catalog-images` cuando existan cambios/uploads.
- Backup adicional antes de migraciones o cambios operativos relevantes.
- Conservar las copias fuera de los directorios de releases.
- Preferir una copia fuera del mismo VPS para tolerar pérdida total del servidor.

La frecuencia/retención definitiva debe ajustarse al volumen y capacidad reales.

## Restore De Base De Datos

El procedimiento exacto dependerá del mecanismo de backup elegido, pero debe comprobar al menos:

- La base restaura sin errores.
- Las migraciones esperadas están presentes.
- Se puede iniciar la API contra la copia restaurada.
- Clientes, órdenes, pagos y catálogo se consultan correctamente.
- Los permisos/autenticación continúan consistentes.

No ejecutar restore destructivo sobre producción como prueba.

## Restore De Imágenes

Reglas:

- Conservar los nombres de archivo originales.
- Restaurar dentro del `shared/catalog-images` del ambiente correspondiente.
- Conservar ownership/permisos requeridos por el servicio API.
- No restaurar dentro de `backend/releases` ni `frontend/releases`.
- Verificar una muestra de `ImagePath` mediante `GET /api/catalog/images/{fileName}`.

## Verificación De Consistencia

Después de un restore conjunto:

1. `/health` responde `200`.
2. `/api/catalog/public` responde correctamente.
3. Una muestra de productos con imagen administrada referencia archivos existentes.
4. Los GET de imágenes devuelven contenido y MIME esperado.
5. `/catalogo` renderiza esas imágenes.
6. Login y módulos privados principales funcionan.

## Rollback De Aplicación Vs Restore De Datos

Son operaciones diferentes:

- **Rollback de aplicación:** vuelve symlinks/backend/frontend a un release anterior; normalmente no revierte datos.
- **Restore de datos:** restaura SQL Server y, cuando aplica, imágenes persistentes.

No asumir que un rollback de código revierte migraciones o datos.

## Migraciones

Reglas:

- No aplicar migraciones productivas sin backup previo.
- Revisar el script idempotente generado por el pipeline.
- Conservar ventana de rollback operativa.
- Evitar migraciones destructivas sin plan específico de reversa.

## Retención Y Archivos Huérfanos

Actualmente DELETE de imagen desasocia pero no elimina el archivo físico.

Por tanto, pueden existir archivos huérfanos. Esto es backlog aceptado, no un defecto del MVP.

Antes de automatizar limpieza:

- Inventariar archivos no referenciados.
- Definir antigüedad mínima/retención.
- Tomar backup.
- Ejecutar dry-run/report primero.
- Nunca borrar basándose únicamente en nombre o fecha sin contrastar la BD.

## Criterio De Salida De PROD-READY-1

No se considera listo para producción hasta que exista evidencia de:

- backup real de BD;
- backup real de imágenes;
- restore de prueba exitoso;
- política de retención documentada;
- responsable/ubicación operativa definidos.

Fuente de prioridad: `docs/05-delivery/current-work-plan.md`.
