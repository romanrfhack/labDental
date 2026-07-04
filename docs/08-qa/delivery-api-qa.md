# QA API Entregas - Fase 3.4.1

## Alcance

Fase 3.4.1 implementa backend delivery MVP + permisos. No implementa UI admin, UI repartidor, cambios visuales, impresión nueva, QR/barcode, evidencia, foto, firma digital, geolocalización, notificaciones, deploy real ni dependencias nuevas.

## Modelo Y Migración

- Entidad: `WorkOrderDelivery`.
- Migración: `20260704053734_AddWorkOrderDeliveries`.
- Tabla nueva: `WorkOrderDeliveries`.
- Relación requerida: `WorkOrderDelivery.WorkOrderId` -> `WorkOrders.Id`.
- Relación opcional: `WorkOrderDelivery.AssignedToUserId` -> `Security.Users.Id`.
- Regla MVP: una entrega por orden mediante índice único en `WorkOrderId`.
- `WorkOrder.DeliveryDate` no cambia; sigue siendo fecha planeada/capturada.
- Al completar entrega correctamente, el backend sincroniza `WorkOrder.Status` a `Delivered`.

La migración debe aplicarse en DEV antes de validar el flujo por API. No aplicar a producción sin plan de despliegue y respaldo.

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

## Permisos Y Roles

- `deliveries.view`: listar/ver entregas permitidas.
- `deliveries.assign`: crear y asignar repartidor.
- `deliveries.update`: registrar salida y actualizar notas.
- `deliveries.complete`: marcar entregada o no entregada.

Rol `Admin`:

- Recibe todos los permisos `deliveries.*` porque el seed Admin usa `Permissions.All`.
- Validación Fase 3.4.1.1: se detectó que un Admin local existente no recibía permisos nuevos cuando `SecuritySeed:RunOnStartup=false` y solo corría `SecuritySeed:EnsureBaselineOnStartup=true`. Se corrigió el seed baseline para sincronizar permisos faltantes de `Permissions.All` al rol `Admin` existente sin leer ni escribir contraseñas.

Rol `Repartidor`:

- Recibe `deliveries.view`.
- Recibe `deliveries.complete`.
- No recibe `deliveries.assign`, `deliveries.update`, `orders.view`, `customers.view`, `payments.view`, `users.manage` ni `roles.manage`.
- El backend limita listados y detalle a entregas asignadas cuando el usuario no tiene permisos administrativos.

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
- `dotnet test`: correcto; Domain 1/1, Application 1/1 y API 121/121.
- `npm run build` desde `src/LaboratorioTlahuac.Web`: correcto; warning de budget inicial excedido por 26.71 kB.
- `git diff --check`: correcto.
- Búsquedas finales solicitadas: correctas; patrones sensibles revisados con salida limitada a archivos para no imprimir valores.
- No se ejecutó `dotnet user-secrets list`; no se imprimieron secretos.

## Validación DEV Sugerida

1. Aplicar migración `AddWorkOrderDeliveries` en base DEV.
2. Levantar API DEV con seed baseline habilitado para asegurar permisos y rol `Repartidor`.
3. Confirmar en `/api/auth/me` que Admin recibe `deliveries.assign`, `deliveries.update` y `deliveries.complete`; si el Admin ya existía, Fase 3.4.1.1 cubre la sincronización baseline de permisos faltantes.
4. Confirmar en `/api/admin/roles` que `Repartidor` tiene solo `deliveries.view` y `deliveries.complete`.
5. Crear usuario con rol `Repartidor`.
6. Crear orden y entrega con Admin.
7. Asignar entrega al repartidor.
8. Registrar salida con Admin.
9. Iniciar sesión como repartidor y confirmar que solo ve sus entregas.
10. Completar entrega con `recipientName`.
11. Confirmar que la entrega queda `Delivered` y la orden queda `Delivered`.

## Pendientes

- Fase 3.4.2: UI admin de entregas desde órdenes.
- Fase 3.4.3: UI repartidor mobile-first bajo `/app/entregas`.
- Fase 3.4.4: QA DEV y ajustes con celular real.
- Aplicar la migración en VPS DEV antes de usar endpoints delivery publicados.
- Definir si `Repartidor` debe recibir `deliveries.update` para registrar salida desde móvil.
- Diseñar cancelación de entregas si operación la requiere.
- Diseñar firma, foto, geolocalización, QR/barcode, evidencia y retención antes de implementarlos.
