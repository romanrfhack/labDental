# Diseño MVP De Entregas Y Repartidor - Fase 3.4.0

Fase 3.4.0 fue análisis técnico/documental. No implementó código, migraciones, endpoints, permisos reales, rutas frontend, auth, guards, cookies, XSRF, deploy ni dependencias.

Actualización Fase 3.4.1, 2026-07-04: backend delivery MVP + permisos quedó implementado sin UI. La implementación crea `WorkOrderDelivery`, `DeliveryStatus`, migración `AddWorkOrderDeliveries`, endpoints API mínimos y permisos `deliveries.*`. La UI admin y la UI mobile-first de repartidor quedan pendientes para Fase 3.4.2 y Fase 3.4.3.

Actualización Fase 3.4.1.1, 2026-07-04: QA técnico real aplicó la migración en SQL local `LaboratorioTlahuac_Dev`, validó endpoints delivery con Admin y Repartidor QA local, y corrigió el seed baseline para sincronizar permisos nuevos al rol `Admin` existente sin leer ni escribir contraseñas.

## Objetivo

Habilitar en la siguiente fase un flujo mobile-first para que el repartidor entre desde celular, vea entregas asignadas, consulte a dónde ir y a quién entregar, y registre la entrega con nombre de quien recibió y fecha/hora tomada del servidor.

Desde administración de órdenes se debe poder ver seguimiento de entrega, estatus logístico, repartidor asignado, momento de entrega y quién recibió.

## Inventario Actual

Ya existe:

- Módulo real de órdenes bajo `/app/ordenes`.
- Detalle real de orden en `/app/ordenes/:id`.
- Etiquetas privadas bajo `/app/ordenes/:id/etiqueta-trabajo` y `/app/ordenes/:id/etiqueta-entrega`.
- API actual de órdenes:
  - `GET /api/work-orders`
  - `GET /api/work-orders/statuses`
  - `GET /api/work-orders/{id}`
  - `POST /api/work-orders`
  - `PUT /api/work-orders/{id}`
  - `PATCH /api/work-orders/{id}/status`
- Administración MVP de usuarios y roles:
  - `/app/admin/usuarios`
  - `/app/admin/roles`
  - `/api/admin/users`
  - `/api/admin/roles`
- Rol `Repartidor` preparado por baseline Development como rol de sistema sin permisos activos.
- Permisos actuales:
  - `orders.view`
  - `orders.create`
  - `orders.edit`
  - `orders.delete`
  - `orders.changeStatus`
  - `payments.view`
  - `payments.create`
  - `payments.cancel`
  - `customers.view`
  - `customers.create`
  - `customers.edit`
  - `inventory.view`
  - `inventory.create`
  - `inventory.adjust`
  - `suppliers.view`
  - `suppliers.create`
  - `users.manage`
  - `roles.manage`
  - `reports.view`

Campos actuales de `WorkOrder`:

- `Id`
- `OrderNumber`
- `CustomerId`
- `InternalDoctorId`
- `PatientName`
- `ReceivedDate`
- `ReferenceNumber`
- `WorkDescription`
- `DentalColor`
- `FirstTrialDate`
- `SecondTrialDate`
- `DeliveryDate`
- `Status`
- `TotalAmount`
- `Notes`
- `CreatedAtUtc`
- `CreatedByUserId`
- `UpdatedAtUtc`
- `UpdatedByUserId`
- `StatusHistory`
- `Payments`

Estados actuales de `WorkOrder`:

| Valor técnico | Etiqueta UI |
| --- | --- |
| `Received` | Recibida |
| `InProcess` | En proceso |
| `FirstTrial` | En primera prueba |
| `SecondTrial` | En segunda prueba |
| `ReadyForDelivery` | Lista para entrega |
| `Delivered` | Entregada |
| `Cancelled` | Cancelada |

`DeliveryDate` es fecha planeada/capturada de entrega. No representa fecha/hora real de entrega, salida a ruta, recibido ni evidencia.

## Faltante Para Entregas

Falta implementar:

- Asignar repartidor a una orden lista para entrega.
- Registrar salida a entrega con fecha/hora de servidor.
- Registrar entrega completada.
- Registrar quién recibió.
- Registrar fecha/hora real de entrega.
- Registrar observaciones de entrega.
- Registrar no entrega o intento fallido con motivo.
- Relación persistente entre entrega y orden.
- Ruta mobile-first para repartidor.
- DTO mínimo de entrega que incluya datos necesarios de cliente sin exigir `customers.view` ni exponer toda la ficha del cliente.
- Permisos `deliveries.*`.
- Pruebas de autorización para repartidor: solo entregas asignadas salvo permiso administrativo.

## Opciones De Modelo

### Opción A: Extender `WorkOrder`

Agregar campos directamente a `WorkOrder`, por ejemplo:

- `AssignedDriverUserId`
- `DeliveryStatus`
- `AssignedAtUtc`
- `AssignedByUserId`
- `OutForDeliveryAtUtc`
- `OutForDeliveryByUserId`
- `DeliveredAtUtc`
- `DeliveredByUserId`
- `RecipientName`
- `DeliveryNotes`
- `FailedReason`

Ventajas:

- Menor cantidad de clases y endpoints al inicio.
- Consulta simple desde el detalle de orden.
- Menor esfuerzo inicial si solo existe una entrega por orden.

Desventajas:

- Mezcla ciclo operativo de producción con logística.
- Complica historial de intentos futuros.
- Hace crecer `WorkOrder` con campos que no aplican a todas las órdenes.
- Menor trazabilidad si hay reasignaciones, intentos fallidos o entregas parciales.
- Si se agregan firma, foto, QR o ubicación después, la entidad central queda más cargada.

### Opción B: Crear `Delivery` O `WorkOrderDelivery`

Crear una entidad separada para logística. En MVP puede existir una entrega activa por orden, con una restricción única por `WorkOrderId`. En una fase posterior puede evolucionar a múltiples intentos o historial.

Campos mínimos sugeridos:

- `Id`
- `WorkOrderId`
- `AssignedDriverUserId`
- `AssignedByUserId`
- `AssignedAtUtc`
- `Status`
- `OutForDeliveryAtUtc`
- `OutForDeliveryByUserId`
- `DeliveredAtUtc`
- `DeliveredByUserId`
- `RecipientName`
- `DeliveryNotes`
- `FailedReason`
- `CreatedAtUtc`
- `CreatedByUserId`
- `UpdatedAtUtc`
- `UpdatedByUserId`

Snapshot opcional recomendado si se quiere trazabilidad contra cambios posteriores del cliente:

- `CustomerDisplayName`
- `DeliveryAddress`
- `ContactName`
- `ContactPhone`
- `ContactWhatsApp`

Ventajas:

- Separa producción (`WorkOrderStatus`) de logística (`DeliveryStatus`).
- Mejor trazabilidad de asignación, salida, cierre y fallos.
- Permite crecer a historial de intentos, evidencia, firma, foto, QR y ubicación.
- Evita otorgar `orders.view` o `customers.view` amplio al repartidor.
- Facilita endpoints y DTOs específicos de entrega.

Desventajas:

- Requiere entidad, configuración EF, DbSet, migración y servicios nuevos.
- Requiere decidir regla de una entrega activa por orden para MVP.
- Requiere sincronizar estado final con `WorkOrder.Delivered` o mostrar ambos estados.

## Comparación

| Criterio | Opción A: extender `WorkOrder` | Opción B: `Delivery` / `WorkOrderDelivery` |
| --- | --- | --- |
| Esfuerzo inicial | Bajo | Medio |
| Trazabilidad | Baja/media | Alta |
| Escalabilidad | Limitada | Mejor |
| Cambios mínimos | Menos archivos | Más archivos |
| Impacto en migración | Columnas nuevas en `WorkOrders` | Tabla nueva, FKs e índices |
| Historial futuro | Difícil | Natural |
| Seguridad de datos | Tiende a reutilizar orden/cliente | Permite DTO mínimo específico |
| Ajuste al flujo real | MVP rápido | Operación más robusta |

## Recomendación

Recomendada: Opción B, entidad separada `WorkOrderDelivery`.

Motivo: el objetivo del flujo es trazabilidad real de entrega. Asignación, salida, entrega, recibido, no entrega y evidencia futura son conceptos logísticos distintos al ciclo de producción de la orden. Extender `WorkOrder` sirve para un MVP muy rápido, pero hace más caro agregar historial de intentos, evidencia o reportes por repartidor.

Regla MVP recomendada:

- Una orden puede tener cero o una entrega activa.
- La entrega se crea desde el detalle de orden cuando la orden está en `ReadyForDelivery` o cuando administración decide prepararla para entrega.
- El cierre exitoso de entrega marca `Delivery.Status = Delivered`.
- En la misma operación se puede cambiar `WorkOrder.Status` a `Delivered` para mantener el tablero actual consistente.
- Una orden `Cancelled` no puede crear, salir ni completar entrega.
- Una entrega `Delivered` no se modifica salvo permiso administrativo futuro.

## Estados De Entrega

Usar nombres técnicos en inglés, siguiendo el patrón actual de enums, con etiquetas en español en UI.

| Estado técnico | Etiqueta UI | Uso |
| --- | --- | --- |
| `PendingAssignment` | Pendiente de asignar | Orden lista para entrega sin repartidor asignado. |
| `Assigned` | Asignada | Repartidor asignado, aún sin salida. |
| `OutForDelivery` | En ruta | Repartidor salió a entregar. |
| `Delivered` | Entregada | Entrega cerrada correctamente. |
| `FailedDelivery` | No entregada | Intento fallido con motivo. |
| `Cancelled` | Cancelada | Entrega cancelada por administración. |

`FailedDelivery` es más explícito que `Failed` y evita confundir fallo técnico con intento logístico no entregado.

## Permisos Propuestos

Permisos recomendados para `Permissions.All` en Fase 3.4.1:

- `deliveries.view`: ver entregas asignadas y detalle permitido.
- `deliveries.assign`: crear/asignar/reasignar entrega desde administración.
- `deliveries.update`: marcar salida o no entregada según reglas.
- `deliveries.complete`: marcar entregada y capturar recibido.

Permisos del rol `Repartidor` implementados en Fase 3.4.1:

- `deliveries.view`
- `deliveries.complete`

El rol `Repartidor` no debe recibir en MVP:

- `orders.view`
- `orders.edit`
- `orders.changeStatus`
- `customers.view`
- `payments.view`
- `users.manage`
- `roles.manage`
- `deliveries.assign`
- `deliveries.update`

Permisos de administración/operación:

- `orders.view` para ver detalle de orden.
- `orders.changeStatus` si administración marcará `ReadyForDelivery`.
- `deliveries.view` para ver seguimiento de entrega.
- `deliveries.assign` para asignar repartidor.
- `deliveries.update` para registrar salida o no entregada si operación lo hace.
- `deliveries.complete` solo si administración podrá corregir/cerrar entregas.

Si se quiere un MVP más pequeño, `deliveries.complete` puede fusionarse en `deliveries.update`; la separación recomendada evita que cualquier actualización logística permita cerrar una entrega.

## Rutas Frontend Recomendadas

Ruta recomendada:

- `/app/entregas`
- `/app/entregas/:id`

Motivo:

- Es clara para administración y repartidor.
- Describe el recurso operativo, no solo el rol.
- Permite que un Admin use la misma sección para consultar entregas sin crear otra ruta.
- Evita duplicar `/app/ordenes` o crear un panel paralelo de órdenes.

Alternativa:

- `/app/repartidor`
- `/app/repartidor/:id`

No recomendada como ruta principal porque acopla la sección a un tipo de usuario y puede quedarse corta si administración necesita ver el mismo recurso.

La UI puede mostrar en navegación el texto `Entregas` para Admin/operación y `Mis entregas` para Repartidor, usando la misma ruta.

## Endpoints MVP Recomendados

Implementado para Fase 3.4.1:

- `GET /api/deliveries`
  - Lista entregas.
  - Filtros opcionales: `status`, `assignedToMe`, `page`, `pageSize`.
  - Requiere `deliveries.view`.
  - Admin/operación con permiso amplio puede ver todas; `Repartidor` queda limitado a sus asignadas.
- `GET /api/deliveries/{id}`
  - Detalle de entrega.
  - Requiere `deliveries.view`.
  - Debe validar que el usuario sea el repartidor asignado salvo permiso administrativo.
- `GET /api/work-orders/{workOrderId}/delivery`
  - Seguimiento de entrega desde detalle de orden.
  - Requiere `orders.view` y `deliveries.view` o regla equivalente.
- `POST /api/work-orders/{workOrderId}/delivery`
  - Crea entrega `PendingAssignment` si no existe.
  - Requiere `deliveries.assign`.
- `PATCH /api/deliveries/{id}/assign`
  - Cambia repartidor asignado.
  - Requiere `deliveries.assign`.
- `PATCH /api/deliveries/{id}/out-for-delivery`
  - Marca salida a ruta con timestamp de servidor.
  - Requiere `deliveries.update`.
- `PATCH /api/deliveries/{id}/complete`
  - Marca entregado, requiere `recipientName`.
  - Requiere `deliveries.complete`.
- `PATCH /api/deliveries/{id}/failed`
  - Marca no entregada, requiere `failedReason`.
  - Requiere `deliveries.complete`.

No se implementó `GET /api/deliveries/mine`; el equivalente MVP es `GET /api/deliveries?assignedToMe=true`.

DTO mínimo de lista:

- `id`
- `workOrderId`
- `orderNumber`
- `customerDisplayName`
- `patientName`
- `referenceNumber`
- `deliveryDate`
- `deliveryStatus`
- `deliveryStatusLabel`
- `deliveryAddress`
- `contactName`
- `contactPhone`
- `contactWhatsApp`

DTO mínimo de detalle:

- Datos de lista.
- `workDescription`
- `dentalColor`
- `internalDoctorFullName`
- `workOrderStatus`
- `workOrderStatusLabel`
- `assignedDriverUserId`
- `assignedDriverFullName`
- `assignedAtUtc`
- `outForDeliveryAtUtc`
- `deliveredAtUtc`
- `recipientName`
- `deliveryNotes`
- `failedReason`

No incluir información financiera para repartidor en el MVP.

## Flujo MVP

### Admin / Operación

1. Abrir `/app/ordenes/:id`.
2. Confirmar que la orden está lista para entrega.
3. Cambiar estado de orden a `ReadyForDelivery` si aún no lo está.
4. Crear/asignar entrega a un usuario con rol `Repartidor`.
5. Ver estatus de entrega en el detalle de orden.
6. Imprimir etiqueta de entrega.
7. Registrar salida si operación lo hace desde escritorio, o dejar que el repartidor marque salida desde móvil.
8. Consultar recibido, hora de entrega y observaciones después del cierre.

### Repartidor

1. Entrar desde celular a `/app/entregas`.
2. Ver lista vertical de entregas asignadas.
3. Abrir detalle.
4. Consultar cliente, dirección, contacto, folio, paciente/referencia, trabajo e indicaciones.
5. Marcar salida a ruta, si aplica.
6. Marcar entregado.
7. Capturar `Recibió`.
8. Capturar observaciones opcionales.
9. Guardar; el servidor registra fecha/hora y usuario.
10. Si no se entrega, marcar `No entregada` con motivo.

## UI Mobile-First

Listado móvil:

- Cards por entrega, sin tabla.
- Estado visible.
- Cliente y folio como primer nivel.
- Dirección/contacto visibles si existen; texto `pendiente` si faltan.
- Botones táctiles cómodos.
- Filtros simples: `Pendientes`, `En ruta`, `Entregadas`, `No entregadas`.

Detalle móvil:

- Datos esenciales arriba:
  - Cliente.
  - Dirección.
  - Contacto.
  - Folio.
  - Paciente/referencia.
  - Trabajo.
- Acciones al final o fijas si no tapan contenido:
  - `Marcar salida`
  - `Marcar entregado`
  - `No entregada`
- Campo `Recibió` obligatorio solo para entregar.
- Observaciones opcionales.

## Reglas De Seguridad Y Negocio

- La ruta vive bajo `/app` y requiere sesión.
- No hay rutas públicas de entrega.
- El repartidor solo ve entregas asignadas a su usuario.
- Admin/operación puede ver/asignar según permisos explícitos.
- La fecha/hora de salida y entrega la toma el servidor.
- `RecipientName` es obligatorio para `Delivered`.
- `FailedReason` es obligatorio para `FailedDelivery`.
- Una orden `Cancelled` no puede salir ni completarse.
- Una entrega `Delivered` no se modifica en MVP.
- El repartidor no ve pagos, saldos ni datos financieros.
- No capturar foto, firma, ubicación ni QR hasta documentar almacenamiento, retención y permisos.

## Fases Recomendadas

### Fase 3.4.1 - Backend Delivery MVP + Permisos

- Estado: implementada el 2026-07-04.
- Agrega permisos `deliveries.view`, `deliveries.assign`, `deliveries.update` y `deliveries.complete`.
- Define enum `DeliveryStatus` con `PendingAssignment`, `Assigned`, `OutForDelivery`, `Delivered` y `FailedDelivery`.
- Crea entidad `WorkOrderDelivery`.
- Agrega configuración EF y migración `20260704053734_AddWorkOrderDeliveries`.
- Crea contratos, servicio y endpoints mínimos.
- Agrega pruebas de autorización:
  - sin sesión `401`;
  - sin permiso `403`;
  - repartidor solo ve asignadas;
  - Admin asigna;
  - entrega requiere `recipientName`;
  - timestamp de servidor;
  - transiciones inválidas devuelven `400`.

Decisión 3.4.1: no se implementa estado `Cancelled` para entrega en este MVP. Si se requiere cancelar entrega como acción administrativa, debe diseñarse en una fase posterior.

### Fase 3.4.2 - UI Admin Desde Órdenes

- Mostrar panel de entrega en `/app/ordenes/:id`.
- Asignar repartidor.
- Ver estatus, salida, entregado, recibido y observaciones.
- Actualizar etiqueta de entrega con repartidor/dirección/contacto cuando el DTO esté listo.

### Fase 3.4.3 - UI Repartidor Mobile-First

- Crear `/app/entregas`.
- Crear `/app/entregas/:id`.
- Lista de entregas asignadas.
- Detalle con acciones táctiles.
- Captura de recibido/no entregado.

### Fase 3.4.4 - QA DEV Y Ajustes

- Validar con Admin.
- Crear usuario real con rol `Repartidor`.
- Asignar permisos y entrega.
- Validar desde celular real.
- Validar no acceso a órdenes/clientes/pagos completos.
- Validar impresión de etiqueta de entrega actualizada.

### Fase Posterior

- Firma.
- Foto.
- Ubicación.
- QR/barcode.
- Historial de múltiples intentos.
- Agrupación de varias órdenes por entrega.
- PWA/offline si el cliente lo confirma.

## Confirmación De Alcance 3.4.0

- Solo documentación.
- Sin cambios de código.
- Sin migraciones.
- Sin endpoints nuevos.
- Sin dependencias.
- Sin cambios de auth, guards, cookies, XSRF ni deploy.
- Fase siguiente implementada posteriormente: Fase 3.4.1 - backend delivery MVP + permisos.
- Siguiente fase recomendada actual: Fase 3.4.2 - UI admin de entregas desde órdenes.
