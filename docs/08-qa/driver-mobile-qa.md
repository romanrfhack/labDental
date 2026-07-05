# QA UI Repartidor - Fase 3.4.3 / 3.4.4

## Alcance

Fase 3.4.3 agrega la UI mobile-first del repartidor bajo `/app/entregas` y `/app/entregas/:id`. Fase 3.4.3.1 agrega redirect post-login por permisos y reintento de entrega fallida. Fase 3.4.4 agrega pulido UX operativo: filtros, contadores, cards más claras, jerarquía visual del detalle, contacto clicable y mapa condicional.

Fase 3.4.3 no agregó backend, migraciones, endpoints, dependencias, deploy, cambios en `AuthService`, guards, cookies ni XSRF. Fase 3.4.3.1 agrega endpoint de retry y ajuste de redirect en frontend, sin migraciones, dependencias, deploy, guards, cookies ni XSRF. Fase 3.4.4 no agrega backend, migraciones, endpoints, dependencias, deploy, auth, guards, cookies ni XSRF. No permite asignar repartidor desde la UI mobile.

## Cierre QA DEV Fase 3.4.3.1 - 2026-07-05

Validación manual reportada en DEV para el flujo de `Repartidor`.

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
| Login `Repartidor` sin `returnUrl` redirige a `/app/entregas` | OK |
| `/app/entregas` carga correctamente | OK |
| `/app/dashboard` redirige a `/app/access-denied` | OK |
| `/app/access-denied` muestra `Ir a mi inicio` | OK |
| `Ir a mi inicio` lleva a `/app/entregas` | OK |
| Entrega `FailedDelivery` muestra `Reintentar entrega` | OK |
| `Reintentar entrega` cambia la entrega a `En ruta` | OK |
| Después de reintentar permite marcar `Entregada` | OK |
| Validación de `recipientName` vacío | OK |
| Logout | OK |

Observaciones reportadas: sin hallazgos ni bug claro. No se modificó código, backend, migraciones, auth, guards, cookies, XSRF, deploy ni dependencias para este cierre documental.

## Validación Técnica Fase 3.4.4 - 2026-07-05

Validación local de implementación:

| Punto | Resultado |
| --- | --- |
| `npm run build` desde `src/LaboratorioTlahuac.Web` | OK, initial total `314.59 kB`, sin warning de budget |
| `dotnet build` | OK, 0 errores y 2 warnings `NU1903` conocidos |
| `dotnet test` | OK, Domain 1/1, Application 1/1, API 129/129 |
| `git diff --check` | OK |

Pendiente recomendado para UAT: revisión visual real en celular o DevTools móvil con usuario `Repartidor`.

## Cierre QA DEV Fase 3.4.4 - 2026-07-05

Validación manual reportada en DEV para el pulido UX operativo de entregas.

Deploy validado:

| Punto | Resultado |
| --- | --- |
| GitHub Actions | `success` |
| `GET /health` | `200` |
| `GET /api/deliveries` sin sesión | `401` |

Resultados:

| Caso | Resultado |
| --- | --- |
| Login `Repartidor` | OK |
| `/app/entregas` carga correctamente | OK |
| Filtros de estado | OK |
| Contadores | OK |
| Cards mobile-first | OK |
| Detalle de entrega | OK |
| Acciones contextuales | OK |
| Reintentar entrega | OK |
| Marcar entregada | OK |
| Marcar no entregada | OK |
| `tel:` aparece solo si hay teléfono | OK |
| WhatsApp aparece solo si existe dato | OK |
| `Abrir mapa` aparece solo si hay dirección | OK |
| Logout | OK |

Observaciones reportadas: sin hallazgos ni bug claro. No se modificó código, backend, migraciones, auth, guards, cookies, XSRF, deploy ni dependencias para este cierre documental.

## Rutas

- `/app/entregas`: listado de entregas asignadas.
- `/app/entregas/:id`: detalle y cierre de una entrega asignada.

Ambas rutas viven bajo `/app`, requieren sesión y están protegidas con `deliveries.view`.

## Permisos

- `deliveries.view`: permite entrar al listado/detalle.
- `deliveries.complete`: muestra acciones para marcar entregada o no entregada.

Si falta `deliveries.complete`, la pantalla queda en lectura y no muestra formularios de cierre.

## Contratos Usados

- `GET /api/deliveries?assignedToMe=true&page=1&pageSize=20`
- `GET /api/deliveries?assignedToMe=true&status={status}&page=1&pageSize=20`
- `GET /api/deliveries/{id}`
- `PATCH /api/deliveries/{id}/complete`
- `PATCH /api/deliveries/{id}/failed`
- `PATCH /api/deliveries/{id}/retry`

El listado siempre usa `assignedToMe=true`. El detalle valida en frontend que `assignedToUserId` coincida con el usuario autenticado antes de mostrar datos.

## Checklist Manual

1. Iniciar sesión con usuario `Repartidor` que tenga `deliveries.view` y `deliveries.complete`.
2. Confirmar que, sin `returnUrl` explícito, el login redirige a `/app/entregas` y no a `/app/dashboard`.
3. Confirmar que la navegación privada muestra `Entregas`.
4. Abrir `/app/entregas`.
5. Confirmar que la lista carga sin quedar en loading infinito.
6. Confirmar en Network que el listado usa `assignedToMe=true`.
7. Confirmar que solo aparecen entregas asignadas al usuario autenticado.
8. Confirmar estado vacío cuando no hay entregas asignadas.
9. Confirmar que aparecen filtros `Todas`, `En ruta`, `Asignadas`, `No entregadas` y `Entregadas`.
10. Confirmar que los contadores muestran asignadas, en ruta, no entregadas y entregadas.
11. Cambiar filtros y confirmar en Network que se usa `status` cuando corresponde.
12. Confirmar que cada card muestra folio, cliente, paciente/referencia, trabajo, estado, fecha de entrega y dirección/contacto si existen.
13. Confirmar que la card no muestra dirección/contacto cuando el dato no existe.
14. Abrir una entrega.
15. Confirmar que el detalle muestra cliente, folio, fecha de entrega, estado de entrega, estado de orden, paciente, referencia, trabajo y seguimiento.
16. Confirmar que `Llamar` aparece con `tel:` solo si existe teléfono.
17. Confirmar que `WhatsApp` aparece solo si existe dato WhatsApp.
18. Confirmar que `Abrir mapa` aparece solo si existe dirección y abre una URL de Google Maps.
19. Confirmar que no se muestran pagos, saldos ni datos financieros.
20. Confirmar que no existe acción para asignar o cambiar repartidor.
21. Con entrega `FailedDelivery` asignada al usuario, confirmar botón `Reintentar entrega` y texto `La entrega volvera a marcarse como En ruta.`.
22. Reintentar entrega y confirmar que el detalle refresca a `En ruta` / `OutForDelivery`.
23. Confirmar que `WorkOrder.Status` no cambió por el reintento.
24. Con entrega reintentada, capturar `recipientName` y marcar entregada.
25. Confirmar que el detalle refresca y muestra estado `Entregada`, timestamp y `Recibio`.
26. Con otra entrega reintentada, marcar no entregada con nuevo `failedReason`.
27. Confirmar que el detalle refresca y muestra estado `No entregada`, timestamp y motivo del último intento fallido.
28. Con entrega `OutForDelivery`, intentar marcar entregada con `Recibio` vacío y confirmar error controlado.
29. Con entrega `Assigned` u `OutForDelivery`, intentar marcar no entregada con motivo vacío y confirmar error controlado.
30. Iniciar sesión con usuario que tenga `deliveries.view` pero no `deliveries.complete`.
31. Confirmar que puede ver sus entregas asignadas pero no ve acciones de cierre ni reintento.
32. Intentar abrir una entrega de otro usuario por URL directa y confirmar que no se muestran datos sensibles.
33. Abrir `/app/access-denied` y confirmar que el enlace principal dice `Ir a mi inicio` y apunta a `/app/entregas`.
34. Confirmar que `/app/ordenes`, `/app/clientes`, `/app/pagos`, `/app/admin/usuarios` y `/app/admin/roles` no quedan disponibles para el rol `Repartidor`.
35. Confirmar que `/login` sigue público.
36. Confirmar que `/app` y `/app/dashboard` siguen protegidos.
37. Confirmar que `/dashboard` no se usa como ruta privada real.

## Responsive

Validar al menos:

- 360 x 740.
- 390 x 844.
- 414 x 896.
- 768 x 1024.
- Desktop.

Puntos visuales:

- Cards de listado sin scroll horizontal global.
- Filtros horizontales usables con dedo y sin cortar texto.
- Contadores legibles en 360px.
- Botones táctiles de cierre con altura cómoda.
- Texto largo de cliente, dirección, referencia o trabajo sin desbordar.
- Enlaces `Llamar`, `WhatsApp` y `Abrir mapa` no se enciman en móvil.
- Formularios de `Recibio` y `Motivo` legibles en una columna en móvil.
- En desktop, listado y detalle siguen usables sin depender de tabla.

## Errores Esperados

- `400`: mostrar validación local para `recipientName` o `failedReason`.
- `403`: mostrar falta de permiso sin redirigir a login.
- `404`: mostrar entrega no encontrada.
- `409`: mostrar estado no permitido.
- Retry fallido: mostrar `No se pudo reintentar la entrega.`.
- `401`: el interceptor global debe redirigir a `/login`.

## Limitaciones

- El rol `Repartidor` no tiene `deliveries.update`, por lo que no registra salida a ruta desde móvil en esta fase.
- Marcar entregada requiere que la entrega esté `OutForDelivery`.
- Marcar no entregada está disponible para `Assigned` u `OutForDelivery`.
- Reintentar entrega solo aplica para `FailedDelivery`.
- El historial completo de intentos queda pendiente para fase futura.
- No hay firma, foto, geolocalización, QR/barcode, offline/PWA ni evidencia adjunta.
- La validación DEV de redirect/retry con usuario real `Repartidor` quedó cerrada el 2026-07-05.
- Fase 3.4.4 quedó validada técnicamente en local y cerrada en DEV el 2026-07-05 sin observaciones reportadas.
