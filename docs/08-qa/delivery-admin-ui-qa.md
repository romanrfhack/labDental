# QA UI Admin Entregas - Fase 3.4.2, 3.4.2.1 y 3.4.3.1

## Alcance

Fase 3.4.2 agrega UI administrativa de entregas dentro del detalle de orden existente. Fase 3.4.2.1 agrega estado de entrega al listado `/app/ordenes`. Fase 3.4.3.1 agrega reintento de entregas fallidas desde la sección `Entrega`.

Esta pantalla admin no asigna alcance a la UI mobile-first del repartidor. La UI de repartidor quedó implementada después en Fase 3.4.3 bajo `/app/entregas` y tiene QA propio en `docs/08-qa/driver-mobile-qa.md`.

## Cierre QA DEV Fase 3.4.3.1 - 2026-07-05

Validación manual reportada en DEV para el flujo Admin de entregas.

Deploy validado:

| Punto | Resultado |
| --- | --- |
| Commit desplegado | `59542efd4f57df7ba04a2444c5496040810d1702` |
| GitHub Actions | `success` |
| `GET /health` | `200` |
| `GET /api/deliveries` sin sesión | `401` |

Resultados:

| Caso | Resultado |
| --- | --- |
| Login Admin lleva a `/app/dashboard` | OK |
| `/app/ordenes` carga correctamente | OK |
| Grid muestra `Estado` de orden y `Entrega` | OK |
| Detalle de orden muestra sección `Entrega` | OK |
| Admin puede reintentar entrega fallida | OK |
| Reintentar no cambia `WorkOrder.Status` | OK |
| Grid se actualiza después de cambios de entrega | OK |

Observaciones reportadas: sin hallazgos ni bug claro. No se modificó código, backend, migraciones, auth, guards, cookies, XSRF, deploy ni dependencias para este cierre documental.

## Alcance Fase 3.4.2.1

El listado/grid de `/app/ordenes` debe mostrar ambos estados:

- `Estado`: estado operativo de la orden (`WorkOrder.Status`).
- `Entrega`: estado logístico (`WorkOrderDelivery.Status`) cuando la entrega existe.

Reglas esperadas:

- Orden sin entrega: mostrar `Sin entrega`.
- `PendingAssignment`: mostrar `Pendiente de asignar`.
- `Assigned`: mostrar `Asignada`.
- `OutForDelivery`: mostrar `En ruta`.
- `Delivered`: mostrar `Entregada`.
- `FailedDelivery`: mostrar `No entregada`.

`No entregada` pertenece a `DeliveryStatus.FailedDelivery`, no a `WorkOrderStatus`. Marcar una entrega como fallida no debe cambiar `WorkOrder.Status`.

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

Este cierre no reemplazaba el checklist manual funcional de UI en ese momento. La validación DEV específica de grid, sección `Entrega`, retry y refresco quedó cerrada posteriormente en Fase 3.4.3.1.

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
- Reintentar entrega cuando está `FailedDelivery`.
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
- Reintentar entrega fallida: `deliveries.update`.
- Marcar entregada: `deliveries.complete`.
- Marcar no entregada: `deliveries.complete`.

Para cargar candidatos de repartidor se reutilizan endpoints admin existentes:

- `GET /api/admin/roles`: requiere `roles.manage`.
- `GET /api/admin/users`: requiere `users.manage`.

Si el usuario no puede listar roles o usuarios, la UI muestra error controlado y no expone contraseñas. Si no se puede filtrar por rol `Repartidor`, la UI muestra advertencia visual y usa selector controlado de usuarios activos.

## Checklist Manual

1. Iniciar sesión como Admin.
2. Abrir `/app/ordenes`.
3. Confirmar que el grid muestra columna/badge `Estado` y columna/badge `Entrega`.
4. Confirmar que una orden sin entrega muestra `Sin entrega`.
5. Abrir una orden en `/app/ordenes/:id`.
6. Confirmar que aparece la sección `Entrega`.
7. En una orden sin entrega, confirmar estado vacío claro.
8. Crear entrega.
9. Confirmar estado `Pendiente de asignar`.
10. Volver a `/app/ordenes` y confirmar `Entrega = Pendiente de asignar`.
11. Asignar usuario con rol `Repartidor`.
12. Confirmar estado `Asignada`, repartidor y timestamp de asignación.
13. Volver a `/app/ordenes` y confirmar `Entrega = Asignada`.
14. Marcar salida.
15. Confirmar estado `En ruta` y timestamp de salida.
16. Volver a `/app/ordenes` y confirmar `Entrega = En ruta`.
17. Intentar marcar entregada con `Recibió` vacío y confirmar error controlado.
18. Marcar entregada con un nombre en `Recibió`.
19. Confirmar estado `Entregada`, timestamp de entrega y nombre de quien recibió.
20. Volver a `/app/ordenes` y confirmar `Entrega = Entregada`.
21. Repetir en otra orden: crear entrega, asignar repartidor y marcar no entregada con motivo.
22. Confirmar estado `No entregada`, timestamp de falla y motivo.
23. Volver a `/app/ordenes` y confirmar `Entrega = No entregada`.
24. Confirmar que la columna `Estado` conserva `WorkOrder.Status`; si la orden estaba `Recibida`, debe seguir mostrando `Recibida`.
25. En esa entrega `FailedDelivery`, confirmar botón `Reintentar entrega` y texto `La entrega volverá a marcarse como En ruta.`.
26. Reintentar entrega y confirmar estado `En ruta`, timestamp de salida actualizado y mismo repartidor asignado.
27. Confirmar que `WorkOrder.Status` no cambia al reintentar.
28. Desde el reintento, marcar entregada con `Recibió` y confirmar `WorkOrder.Status = Delivered`.
29. Repetir otro reintento y volver a marcar no entregada con nuevo motivo; confirmar que motivo/timestamp de falla reflejan el último intento fallido.
30. Confirmar que pagos, historial, etiquetas y datos de orden siguen cargando.
31. Confirmar que `/login` sigue público.
32. Confirmar que `/app` y `/app/dashboard` siguen privados.
33. Confirmar que `/dashboard` no se usa como ruta privada real.

## Errores Esperados

- `400`: mostrar mensaje de validación entendible.
- `403`: mostrar mensaje local de falta de permiso, sin redirigir a login.
- `404`: mostrar entrega no encontrada o estado sin entrega cuando aplica.
- `409`: mostrar que la entrega no permite la acción en su estado actual.
- Retry fallido: mostrar `No se pudo reintentar la entrega.`.

## Limitaciones

- La UI de repartidor mobile-first bajo `/app/entregas` no se implementó dentro de Fase 3.4.2; quedó implementada posteriormente en Fase 3.4.3.
- La salida a entrega desde móvil sigue pendiente; el rol `Repartidor` no recibe `deliveries.update` en el MVP actual.
- La sección admin no agrega firma, foto, geolocalización, QR/barcode ni evidencia.
- La asignación depende de los endpoints admin existentes para listar usuarios/roles; no se agregó endpoint específico de repartidores.
- No hay historial completo de intentos de entrega; queda como fase futura.
- La validación DEV específica de listado, detalle, retry y actualización del grid quedó cerrada el 2026-07-05; el pulido UX operativo queda para Fase 3.4.4 si se prioriza.

## Relación Con Fase 3.4.3

- `/app/entregas` y `/app/entregas/:id` ya existen y requieren `deliveries.view`.
- El listado de repartidor usa `assignedToMe=true`.
- La UI de repartidor no permite asignar repartidor.
- La UI de repartidor muestra acciones de cierre solo con `deliveries.complete`.
- Decidir si `Repartidor` debe recibir `deliveries.update` para marcar salida desde móvil.
- Validación DEV de usuario `Repartidor` para redirect, retry, cierre y logout cerrada el 2026-07-05 en `docs/08-qa/driver-mobile-qa.md`.
