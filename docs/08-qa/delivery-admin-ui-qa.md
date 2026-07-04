# QA UI Admin Entregas - Fase 3.4.2

## Alcance

Fase 3.4.2 agrega UI administrativa de entregas dentro del detalle de orden existente. No crea la pantalla mobile-first del repartidor, no agrega rutas nuevas, no modifica backend, no crea migraciones, no cambia auth, guards, cookies, XSRF ni deploy.

## Cierre Operativo DEV 2026-07-04

La Fase 3.4.2 quedó operativamente activa en DEV después de un ajuste manual del despliegue:

- GitHub Actions para commit `97d46e9` falló durante health check con `502`.
- El rollback dejó activo `dev-23-eea8f39`.
- El release nuevo `dev-24-97d46e9` quedó copiado en VPS.
- El release `dev-24-97d46e9` fue validado manualmente en puerto alterno `5013`.
- El primer intento manual fue inválido porque se intentó sourcear `/etc/laboratorio-tlahuac-dev/api.env` en Bash y la connection string contiene espacios/semicolons.
- La carga correcta de `api.env` con parser seguro permitió validar que el release nuevo arrancaba correctamente.
- Se cambió manualmente `backend/current` a `dev-24-97d46e9`.
- Se reinició `laboratorio-tlahuac-dev-api.service` y quedó `active`.
- Validación final: `/health` respondió `200`.
- Validación final: `/api/deliveries` sin sesión respondió `401`.
- No se imprimieron secretos.
- No se usó `codex-cobranza-sql`.

Este cierre no reemplaza el checklist manual funcional de UI. El siguiente paso recomendado es ejecutar QA manual DEV de esta pantalla con usuario Admin antes de iniciar Fase 3.4.3.

## Rutas Afectadas

- `/app/ordenes`
- `/app/ordenes/:id`

Rutas no modificadas:

- `/login`
- `/app`
- `/app/dashboard`
- `/app/ordenes/:id/etiqueta-trabajo`
- `/app/ordenes/:id/etiqueta-entrega`

`/dashboard` sigue sin ser ruta privada real.

## Acciones Disponibles

En `/app/ordenes/:id`, la sección `Entrega` permite:

- Ver estado de entrega.
- Crear entrega si la orden no tiene entrega.
- Asignar repartidor desde usuarios activos disponibles.
- Marcar salida a entrega.
- Marcar entregada con `recipientName`.
- Marcar no entregada con `failedReason`.
- Ver timestamps de asignación, salida, entrega y falla.
- Ver `Recibió` o motivo de falla cuando aplica.

Estados mostrados en UI:

- `PendingAssignment`: Pendiente de asignación.
- `Assigned`: Asignada.
- `OutForDelivery`: En reparto.
- `Delivered`: Entregada.
- `FailedDelivery`: No entregada.

## Permisos

- Ver entrega: `deliveries.view`.
- Crear entrega: `deliveries.assign`.
- Asignar repartidor: `deliveries.assign`.
- Marcar salida: `deliveries.update`.
- Marcar entregada: `deliveries.complete`.
- Marcar no entregada: `deliveries.complete`.

Para cargar candidatos de repartidor se reutilizan endpoints admin existentes:

- `GET /api/admin/roles`: requiere `roles.manage`.
- `GET /api/admin/users`: requiere `users.manage`.

Si el usuario no puede listar roles o usuarios, la UI muestra error controlado y no expone contraseñas. Si no se puede filtrar por rol `Repartidor`, la UI muestra advertencia visual y usa selector controlado de usuarios activos.

## Checklist Manual

1. Iniciar sesión como Admin.
2. Abrir `/app/ordenes`.
3. Abrir una orden en `/app/ordenes/:id`.
4. Confirmar que aparece la sección `Entrega`.
5. En una orden sin entrega, confirmar estado vacío claro.
6. Crear entrega.
7. Confirmar estado `Pendiente de asignación`.
8. Asignar usuario con rol `Repartidor`.
9. Confirmar estado `Asignada`, repartidor y timestamp de asignación.
10. Marcar salida.
11. Confirmar estado `En reparto` y timestamp de salida.
12. Intentar marcar entregada con `Recibió` vacío y confirmar error controlado.
13. Marcar entregada con un nombre en `Recibió`.
14. Confirmar estado `Entregada`, timestamp de entrega y nombre de quien recibió.
15. Repetir en otra orden: crear entrega, asignar repartidor y marcar no entregada con motivo.
16. Confirmar estado `No entregada`, timestamp de falla y motivo.
17. Confirmar que pagos, historial, etiquetas y datos de orden siguen cargando.
18. Confirmar que `/login` sigue público.
19. Confirmar que `/app` y `/app/dashboard` siguen privados.
20. Confirmar que `/dashboard` no se usa como ruta privada real.

## Errores Esperados

- `400`: mostrar mensaje de validación entendible.
- `403`: mostrar mensaje local de falta de permiso, sin redirigir a login.
- `404`: mostrar entrega no encontrada o estado sin entrega cuando aplica.
- `409`: mostrar que la entrega no permite la acción en su estado actual.

## Limitaciones

- La UI de repartidor mobile-first bajo `/app/entregas` no está implementada en esta fase.
- La salida a entrega desde móvil sigue pendiente; el rol `Repartidor` no recibe `deliveries.update` en el MVP actual.
- La sección admin no agrega firma, foto, geolocalización, QR/barcode ni evidencia.
- La asignación depende de los endpoints admin existentes para listar usuarios/roles; no se agregó endpoint específico de repartidores.
- La validación visual con navegador real/DEV queda pendiente si no se ejecuta manualmente.

## Pendientes Para Fase 3.4.3

- Completar primero QA manual DEV de Fase 3.4.2 con Admin.
- Crear `/app/entregas`.
- Crear `/app/entregas/:id`.
- Diseñar listado mobile-first para repartidor.
- Mostrar solo entregas asignadas al repartidor.
- Diseñar acciones táctiles para entregada/no entregada.
- Decidir si `Repartidor` debe recibir `deliveries.update` para marcar salida desde móvil.
- Validar en celular real con usuario `Repartidor`.
