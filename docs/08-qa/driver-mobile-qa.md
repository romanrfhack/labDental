# QA UI Repartidor - Fase 3.4.3

## Alcance

Fase 3.4.3 agrega la UI mobile-first del repartidor bajo `/app/entregas` y `/app/entregas/:id`.

No agrega backend, migraciones, endpoints, dependencias, deploy, cambios en `AuthService`, guards, cookies ni XSRF. No permite asignar repartidor desde la UI mobile.

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

El listado siempre usa `assignedToMe=true`. El detalle valida en frontend que `assignedToUserId` coincida con el usuario autenticado antes de mostrar datos.

## Checklist Manual

1. Iniciar sesión con usuario `Repartidor` que tenga `deliveries.view` y `deliveries.complete`.
2. Confirmar que la navegación privada muestra `Entregas`.
3. Abrir `/app/entregas`.
4. Confirmar que la lista carga sin quedar en loading infinito.
5. Confirmar en Network que el listado usa `assignedToMe=true`.
6. Confirmar que solo aparecen entregas asignadas al usuario autenticado.
7. Confirmar estado vacío cuando no hay entregas asignadas.
8. Confirmar que cada card muestra folio, cliente, paciente/referencia, trabajo, estado, fecha de entrega y dirección/contacto si existen.
9. Abrir `Ver detalle`.
10. Confirmar que el detalle muestra cliente, dirección, contacto, folio, paciente, referencia, trabajo, fecha de entrega, estado de orden y seguimiento.
11. Confirmar que no se muestran pagos, saldos ni datos financieros.
12. Confirmar que no existe acción para asignar o cambiar repartidor.
13. Con entrega `OutForDelivery`, intentar marcar entregada con `Recibio` vacío y confirmar error controlado.
14. Capturar `recipientName` y marcar entregada.
15. Confirmar que el detalle refresca y muestra estado `Entregada`, timestamp y `Recibio`.
16. Con otra entrega `Assigned` u `OutForDelivery`, intentar marcar no entregada con motivo vacío y confirmar error controlado.
17. Capturar `failedReason` y marcar no entregada.
18. Confirmar que el detalle refresca y muestra estado `No entregada`, timestamp y motivo.
19. Iniciar sesión con usuario que tenga `deliveries.view` pero no `deliveries.complete`.
20. Confirmar que puede ver sus entregas asignadas pero no ve acciones de cierre.
21. Intentar abrir una entrega de otro usuario por URL directa y confirmar que no se muestran datos sensibles.
22. Confirmar que `/app/ordenes`, `/app/clientes`, `/app/pagos`, `/app/admin/usuarios` y `/app/admin/roles` no quedan disponibles para el rol `Repartidor`.
23. Confirmar que `/login` sigue público.
24. Confirmar que `/app` y `/app/dashboard` siguen protegidos.
25. Confirmar que `/dashboard` no se usa como ruta privada real.

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
- `401`: el interceptor global debe redirigir a `/login`.

## Limitaciones

- El rol `Repartidor` no tiene `deliveries.update`, por lo que no registra salida a ruta desde móvil en esta fase.
- Marcar entregada requiere que la entrega esté `OutForDelivery`.
- Marcar no entregada está disponible para `Assigned` u `OutForDelivery`.
- No hay firma, foto, geolocalización, QR/barcode, offline/PWA ni evidencia adjunta.
- La validación final debe ejecutarse en DEV con usuario real `Repartidor`.
