# QA UI Repartidor - Fase 3.4.3

## Alcance

Fase 3.4.3 agrega la UI mobile-first del repartidor bajo `/app/entregas` y `/app/entregas/:id`. Fase 3.4.3.1 agrega redirect post-login por permisos y reintento de entrega fallida.

Fase 3.4.3 no agregó backend, migraciones, endpoints, dependencias, deploy, cambios en `AuthService`, guards, cookies ni XSRF. Fase 3.4.3.1 agrega endpoint de retry y ajuste de redirect en frontend, sin migraciones, dependencias, deploy, guards, cookies ni XSRF. No permite asignar repartidor desde la UI mobile.

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
9. Confirmar que cada card muestra folio, cliente, paciente/referencia, trabajo, estado, fecha de entrega y dirección/contacto si existen.
10. Abrir `Ver detalle`.
11. Confirmar que el detalle muestra cliente, dirección, contacto, folio, paciente, referencia, trabajo, fecha de entrega, estado de orden y seguimiento.
12. Confirmar que no se muestran pagos, saldos ni datos financieros.
13. Confirmar que no existe acción para asignar o cambiar repartidor.
14. Con entrega `FailedDelivery` asignada al usuario, confirmar botón `Reintentar entrega` y texto `La entrega volverá a marcarse como En ruta.`.
15. Reintentar entrega y confirmar que el detalle refresca a `En ruta` / `OutForDelivery`.
16. Confirmar que `WorkOrder.Status` no cambió por el reintento.
17. Con entrega reintentada, capturar `recipientName` y marcar entregada.
18. Confirmar que el detalle refresca y muestra estado `Entregada`, timestamp y `Recibio`.
19. Con otra entrega reintentada, marcar no entregada con nuevo `failedReason`.
20. Confirmar que el detalle refresca y muestra estado `No entregada`, timestamp y motivo del último intento fallido.
21. Con entrega `OutForDelivery`, intentar marcar entregada con `Recibio` vacío y confirmar error controlado.
22. Con entrega `Assigned` u `OutForDelivery`, intentar marcar no entregada con motivo vacío y confirmar error controlado.
23. Iniciar sesión con usuario que tenga `deliveries.view` pero no `deliveries.complete`.
24. Confirmar que puede ver sus entregas asignadas pero no ve acciones de cierre ni reintento.
25. Intentar abrir una entrega de otro usuario por URL directa y confirmar que no se muestran datos sensibles.
26. Abrir `/app/access-denied` y confirmar que el enlace principal dice `Ir a mi inicio` y apunta a `/app/entregas`.
27. Confirmar que `/app/ordenes`, `/app/clientes`, `/app/pagos`, `/app/admin/usuarios` y `/app/admin/roles` no quedan disponibles para el rol `Repartidor`.
28. Confirmar que `/login` sigue público.
29. Confirmar que `/app` y `/app/dashboard` siguen protegidos.
30. Confirmar que `/dashboard` no se usa como ruta privada real.

## Responsive

Validar al menos:

- 360 x 740.
- 390 x 844.
- 414 x 896.
- 768 x 1024.
- Desktop.

Puntos visuales:

- Cards de listado sin scroll horizontal global.
- Botones táctiles de cierre con altura cómoda.
- Texto largo de cliente, dirección, referencia o trabajo sin desbordar.
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
- La validación final debe ejecutarse en DEV con usuario real `Repartidor`.
