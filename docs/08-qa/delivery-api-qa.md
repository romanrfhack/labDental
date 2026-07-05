# QA API Entregas - Fase 3.4.1

## Alcance

Fase 3.4.1 implementa backend delivery MVP + permisos. El alcance de implementación no incluye UI admin, UI repartidor, cambios visuales, impresión nueva, QR/barcode, evidencia, foto, firma digital, geolocalización, notificaciones ni dependencias nuevas.

Actualización 2026-07-04: la implementación backend delivery MVP fue desplegada correctamente a DEV en el commit `e4c28205c6b866ab0d71edb13c49164100340b0d` mediante GitHub Actions run `28712956106`. Esta actualización no agrega UI; solo registra el cierre de despliegue y validación técnica publicada.

Actualización Fase 3.4.2, 2026-07-04: la UI admin de entregas desde `/app/ordenes/:id` quedó implementada consumiendo estos endpoints existentes. El QA funcional/manual de UI queda documentado en `docs/08-qa/delivery-admin-ui-qa.md`.

Actualización operativa Fase 3.4.2, 2026-07-04: GitHub Actions para commit `97d46e9` falló durante health check con `502`, el rollback dejó activo `dev-23-eea8f39`, y el release nuevo `dev-24-97d46e9` fue validado manualmente y activado mediante ajuste de `backend/current` y restart del servicio. La validación final confirmó `GET /health` `200` y `GET /api/deliveries` sin sesión `401` en DEV. No se imprimieron secretos ni se usó `codex-cobranza-sql`.

Actualización Fase 3.4.3.1, 2026-07-05: se agrega `PATCH /api/deliveries/{id}/retry` para reintentar entregas en `FailedDelivery`. El endpoint vuelve la entrega a `OutForDelivery`, actualiza `OutForDeliveryAtUtc`, mantiene `AssignedToUserId`, no cambia `WorkOrder.Status` y permite cerrar después como `Delivered` o volver a `FailedDelivery`.

## Modelo Y Migración

- Entidad: `WorkOrderDelivery`.
- Migración: `20260704053734_AddWorkOrderDeliveries`.
- Tabla nueva: `WorkOrderDeliveries`.
- Relación requerida: `WorkOrderDelivery.WorkOrderId` -> `WorkOrders.Id`.
- Relación opcional: `WorkOrderDelivery.AssignedToUserId` -> `Security.Users.Id`.
- Regla MVP: una entrega por orden mediante índice único en `WorkOrderId`.
- `WorkOrder.DeliveryDate` no cambia; sigue siendo fecha planeada/capturada.
- Al completar entrega correctamente, el backend sincroniza `WorkOrder.Status` a `Delivered`.

Para el cierre DEV 2026-07-04, la migración ya está aplicada o la base DEV está al día. No aplicar a producción sin plan de despliegue y respaldo.

## Estados

- `PendingAssignment`: entrega creada sin repartidor.
- `Assigned`: entrega con repartidor asignado.
- `OutForDelivery`: salida a ruta registrada.
- `Delivered`: entrega cerrada correctamente.
- `FailedDelivery`: entrega no completada con motivo.

No se implementa `Cancelled` para entrega en este MVP.

## Transiciones

- Crear entrega: `PendingAssignment`.
- Asignar repartidor: `PendingAssignment` o `Assigned` -> `Assigned`.
- Registrar salida: `Assigned` -> `OutForDelivery`.
- Completar: `OutForDelivery` -> `Delivered`; requiere `recipientName`.
- No entregada: `Assigned` u `OutForDelivery` -> `FailedDelivery`; requiere `failedReason`.
- Reintento: `FailedDelivery` -> `OutForDelivery`; requiere `deliveries.update` para operación/Admin o `deliveries.complete` si el repartidor asignado reintenta su propia entrega.
- Transiciones fuera de regla devuelven `400`.

## Endpoints

- `GET /api/deliveries`
  - Permiso: `deliveries.view`.
  - Filtros opcionales: `status`, `assignedToMe`, `page`, `pageSize`.
- `GET /api/deliveries/{id}`
  - Permiso: `deliveries.view`.
- `GET /api/work-orders/{workOrderId}/delivery`
  - Permiso: `deliveries.view`.
- `POST /api/work-orders/{workOrderId}/delivery`
  - Permiso: `deliveries.assign`.
- `PATCH /api/deliveries/{id}/assign`
  - Permiso: `deliveries.assign`.
- `PATCH /api/deliveries/{id}/out-for-delivery`
  - Permiso: `deliveries.update`.
- `PATCH /api/deliveries/{id}/complete`
  - Permiso: `deliveries.complete`.
- `PATCH /api/deliveries/{id}/failed`
  - Permiso: `deliveries.complete`.
- `PATCH /api/deliveries/{id}/retry`
  - Permiso: `deliveries.update` para Admin/operación.
  - Permiso equivalente: `deliveries.complete` cuando el usuario autenticado es el repartidor asignado.

## Permisos Y Roles

- `deliveries.view`: listar/ver entregas permitidas.
- `deliveries.assign`: crear y asignar repartidor.
- `deliveries.update`: registrar salida y actualizar notas.
- `deliveries.complete`: marcar entregada o no entregada; también reintentar una entrega fallida propia cuando el usuario es el repartidor asignado.

Rol `Admin`:

- Recibe todos los permisos `deliveries.*` porque el seed Admin usa `Permissions.All`.
- Validación Fase 3.4.1.1: se detectó que un Admin local existente no recibía permisos nuevos cuando `SecuritySeed:RunOnStartup=false` y solo corría `SecuritySeed:EnsureBaselineOnStartup=true`. Se corrigió el seed baseline para sincronizar permisos faltantes de `Permissions.All` al rol `Admin` existente sin leer ni escribir contraseñas.

Rol `Repartidor`:

- Recibe `deliveries.view`.
- Recibe `deliveries.complete`.
- No recibe `deliveries.assign`, `deliveries.update`, `orders.view`, `customers.view`, `payments.view`, `users.manage` ni `roles.manage`.
- El backend limita listados y detalle a entregas asignadas cuando el usuario no tiene permisos administrativos.
- Puede reintentar una entrega fallida propia con `deliveries.complete`; no puede reintentar entregas ajenas.

## Pruebas Automatizadas

Agregadas:

- `DeliveryIntegrationTests`.
- Actualización de `AdminSecurityIntegrationTests`.
- Actualización de `SecuritySeederTests`.
- Actualización de `PermissionsTests`.

Cobertura:

- Sin sesión: endpoints delivery devuelven `401`.
- Usuario sin permisos: endpoints delivery devuelven `403`.
- Admin crea entrega para orden, lista, obtiene detalle, consulta por orden, asigna repartidor, marca salida y completa.
- Admin marca entrega como fallida con `failedReason`.
- Completar sin `recipientName` devuelve `400`.
- Fallida sin `failedReason` devuelve `400`.
- Transición inválida devuelve `400`.
- Repartidor ve sus entregas asignadas y puede completar.
- Repartidor no puede asignar otro repartidor.
- Admin puede reintentar una entrega fallida.
- Repartidor asignado puede reintentar su entrega fallida.
- Repartidor no asignado no puede reintentar una entrega ajena si no tiene permiso operativo.
- Reintentar una entrega no fallida devuelve `400`.
- Reintentar entrega de orden cancelada devuelve `409`.
- Reintento deja la entrega en `OutForDelivery` y no cambia `WorkOrder.Status`.
- Una entrega reintentada puede cerrarse como `Delivered` con `recipientName`.
- Permisos `deliveries.*` existen.
- Admin conserva permisos delivery.
- `Repartidor` recibe solo permisos esperados.

## Validación Local Ejecutada

- SQL local confirmado: contenedor `ldt-labdental-sql` activo y puerto `14336 -> 1433/tcp`; `codex-cobranza-sql` no se usó.
- `dotnet ef migrations list`: `20260704053734_AddWorkOrderDeliveries` aparecía pendiente antes de aplicar.
- `dotnet ef database update`: correcto contra `LaboratorioTlahuac_Dev` local; creó `WorkOrderDeliveries`, FK a `WorkOrders`, FK opcional a `Security.Users`, índices `AssignedToUserId`, `CreatedAtUtc`, `Status` e índice único `WorkOrderId`.
- `dotnet ef migrations list` posterior: `20260704053734_AddWorkOrderDeliveries` quedó aplicado.
- API local `http://localhost:5277`: `GET /health` respondió `200`.
- QA API real con Admin desde variables de entorno sin imprimir secretos: `401` sin sesión, permisos Admin delivery presentes, rol `Repartidor` con `deliveries.complete`/`deliveries.view`, creación de usuario Repartidor QA local, creación de orden/entrega, `GET /api/work-orders/{workOrderId}/delivery`, `GET /api/deliveries`, asignación, salida, completado con `recipientName`, `400` al completar sin `recipientName`, `400` al marcar fallida sin `failedReason`, Repartidor ve entregas asignadas, recibe `403` al asignar y al registrar salida, y completa entrega asignada.
- `WorkOrder.DeliveryDate` se mantuvo sin cambios en el flujo real; `WorkOrder.Status` se sincronizó a `Delivered` al completar entrega.
- Respuestas de delivery no expusieron email, password ni `passwordHash` del usuario asignado; solo `assignedToUserId` y `assignedToUserFullName`.
- Datos locales creados: usuarios/clientes/órdenes/entregas QA con prefijos de prueba; no se limpiaron.
- `dotnet build`: correcto; 0 errores y 2 warnings `NU1903` conocidos por `SQLitePCLRaw.lib.e_sqlite3` en tests.
- `dotnet test`: correcto; Domain 1/1, Application 1/1 y API 121/121. Actualización Fase 3.4.3.1: correcto con Domain 1/1, Application 1/1 y API 129/129.
- `npm run build` desde `src/LaboratorioTlahuac.Web`: correcto; warning de budget inicial excedido por 26.71 kB.
- `git diff --check`: correcto.
- Búsquedas finales solicitadas: correctas; patrones sensibles revisados con salida limitada a archivos para no imprimir valores.
- No se ejecutó `dotnet user-secrets list`; no se imprimieron secretos.

## Validación DEV Sugerida

Validación publicada del despliegue DEV:

| Punto | Resultado |
| --- | --- |
| Commit desplegado | `e4c28205c6b866ab0d71edb13c49164100340b0d` |
| GitHub Actions run | `28712956106` |
| Resultado deploy | `success` |
| `GET /health` | `200` |
| `GET /api/deliveries` sin sesión | `401` |
| Estado Delivery API | Desplegada y protegida. |

El `401` de `/api/deliveries` sin sesión reemplaza el `404` observado antes del deploy y confirma que `DeliveryEndpoints` están publicados en DEV con protección de sesión/permisos. La migración `WorkOrderDeliveries` ya está aplicada o la base DEV está al día.

Queda pendiente la validación manual Admin en DEV:

1. Iniciar sesión como Admin en DEV.
2. Confirmar en `/api/auth/me` que Admin recibe `deliveries.assign`, `deliveries.update` y `deliveries.complete`; si el Admin ya existía, Fase 3.4.1.1 cubre la sincronización baseline de permisos faltantes.
3. Confirmar en `/api/admin/roles` que `Repartidor` tiene solo `deliveries.view` y `deliveries.complete`.
4. Crear usuario con rol `Repartidor`.
5. Crear orden y entrega con Admin.
6. Asignar entrega al repartidor.
7. Registrar salida con Admin.
8. Iniciar sesión como repartidor y confirmar que solo ve sus entregas.
9. Completar entrega con `recipientName`.
10. Confirmar que la entrega queda `Delivered` y la orden queda `Delivered`.
11. Crear otra entrega, marcarla `FailedDelivery` y reintentar con Admin; confirmar `OutForDelivery`, mismo repartidor y `WorkOrder.Status` sin cambios.
12. Marcar otra entrega como `FailedDelivery`, iniciar sesión como repartidor asignado y reintentar; confirmar `OutForDelivery`.
13. Intentar retry con repartidor no asignado y confirmar `403`.
14. Intentar retry de entrega no fallida y confirmar `400`.
15. Intentar retry de entrega fallida cuya orden fue cancelada y confirmar `409`.

Para la validación visual/funcional de la UI admin, usar además `docs/08-qa/delivery-admin-ui-qa.md`.

## Nota Operativa DEV Fase 3.4.2

El fallo inicial de GitHub Actions no invalidó el release `dev-24-97d46e9`: la causa observada fue el health check `502` durante el despliegue. En la validación manual, el primer intento fue descartado porque se intentó sourcear `api.env` directamente en Bash; ese método no aplica para la connection string del ambiente porque contiene espacios/semicolons. Al cargar `/etc/laboratorio-tlahuac-dev/api.env` con parser seguro, el release arrancó correctamente en puerto alterno `5013`.

Después de activar manualmente `backend/current` hacia `dev-24-97d46e9` y reiniciar `laboratorio-tlahuac-dev-api.service`, DEV quedó con:

| Punto | Resultado |
| --- | --- |
| `http://127.0.0.1:5012/health` | `200` |
| `http://127.0.0.1:5012/api/deliveries` sin sesión | `401` |
| `https://dev.laboratoriodentaltlahuac.com/health` | `200` |
| `https://dev.laboratoriodentaltlahuac.com/api/deliveries` sin sesión | `401` |

Pendiente técnico: ajustar el workflow para esperar más tiempo o usar reintentos más tolerantes en health check después del restart.

## Pendientes

- Fase 3.4.2: UI admin de entregas desde órdenes. Implementada; pendiente validación manual DEV.
- Fase 3.4.3: UI repartidor mobile-first bajo `/app/entregas`. Implementada.
- Fase 3.4.3.1: redirect por permisos y retry de entrega fallida. Implementada; pendiente QA DEV.
- Fase 3.4.4: QA DEV y ajustes con celular real.
- Ajustar workflow DEV para reducir falsos negativos de health check `502` después del restart.
- Validación manual Admin en DEV del flujo delivery desplegado.
- Definir si `Repartidor` debe recibir `deliveries.update` para registrar salida desde móvil.
- Diseñar cancelación de entregas si operación la requiere.
- Diseñar firma, foto, geolocalización, QR/barcode, evidencia y retención antes de implementarlos.
