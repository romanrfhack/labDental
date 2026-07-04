# Operación De Órdenes, Etiquetas Y Entrega

Fuente funcional para órdenes, etiquetas y reparto. Fase 3.1 documentó el análisis operativo; Fase 3.2 implementó el MVP de impresión de etiquetas desde órdenes existentes sin base de datos nueva, endpoints nuevos ni migraciones. Fase 3.4.0 documentó el análisis técnico previo del flujo de entregas/repartidor, Fase 3.4.1 implementó el backend delivery MVP + permisos sin UI, Fase 3.4.2 implementó la UI admin de entregas desde `/app/ordenes/:id`, Fase 3.4.2.1 agregó estado de entrega al listado `/app/ordenes` y Fase 3.4.3 implementó la UI mobile-first de repartidor bajo `/app/entregas`.

Diseño técnico Fase 3.4.0: `docs/01-product/delivery-mvp-design.md`.

## Alcance De Fase 3.1

- Documentar qué existe hoy en órdenes, clientes, pagos, permisos, usuarios/roles y catálogo.
- Diseñar el flujo operativo futuro desde recepción hasta entrega.
- Confirmar que no debe crearse un panel de órdenes duplicado.
- Priorizar el siguiente incremento implementable.

Exclusiones de esta fase: código, migraciones, base de datos, endpoints, auth, guards, cookies, XSRF, deploy, dependencias y commits.

## Confirmación De Lo Existente

### Rutas Reales De Órdenes

El sistema privado ya tiene módulo real de órdenes bajo `/app/ordenes`:

- `/app/ordenes`: listado y filtros.
- `/app/ordenes/nueva`: creación.
- `/app/ordenes/:id`: detalle.
- `/app/ordenes/:id/editar`: edición.
- `/app/ordenes/:id/etiqueta-trabajo`: impresión privada de etiqueta interna.
- `/app/ordenes/:id/etiqueta-entrega`: impresión privada de etiqueta de entrega.

API existente:

- `GET /api/work-orders`
- `GET /api/work-orders/statuses`
- `GET /api/work-orders/{id}`
- `POST /api/work-orders`
- `PUT /api/work-orders/{id}`
- `PATCH /api/work-orders/{id}/status`

Decisión Fase 3.1 y ejecución Fase 3.2: no crear un "panel de órdenes" paralelo. El enfoque correcto es extender `/app/ordenes`, especialmente el detalle `/app/ordenes/:id`, porque ahí ya convergen estado, datos operativos, historial y pagos.

### Implementación Fase 3.2

El detalle de orden agrega acciones visibles:

- `Etiqueta interna`
- `Etiqueta entrega`

Las rutas de impresión viven bajo `/app`, heredan autenticación de la zona privada y requieren `orders.view` mediante `permissionGuard`. Usan `GET /api/work-orders/{id}` desde el servicio frontend existente; no se agregaron endpoints.

La etiqueta interna usa tamaño objetivo 76 x 51 mm y muestra LDT, folio, cliente, doctor interno si existe, paciente, recepción, entrega, estado, color si existe, trabajo y observaciones breves si existen.

La etiqueta de entrega usa tamaño objetivo 102 x 51 mm y muestra LDT, folio, cliente, paciente/referencia, trabajo, entrega, estado, `Dirección pendiente`, `Contacto pendiente`, `Recibe: __________________` y `Firma: __________________`.

Limitación vigente: el detalle actual de orden no incluye dirección, teléfono, WhatsApp ni email del cliente. Para no exigir `customers.view` en una ruta cuyo permiso de negocio es `orders.view`, la etiqueta de entrega imprime textos pendientes seguros hasta diseñar un DTO de impresión/entrega o ampliar el contrato de orden.

### Implementación Fase 3.4.2

El detalle de orden `/app/ordenes/:id` agrega sección `Entrega` sin duplicar el panel de órdenes.

Capacidades implementadas:

- Ver estado de entrega con etiquetas en español.
- Ver repartidor asignado, si existe.
- Ver timestamps de asignación, salida, entrega y falla.
- Ver `Recibió` o motivo de falla cuando aplica.
- Crear entrega si la orden no tiene entrega.
- Asignar repartidor.
- Marcar salida a entrega.
- Marcar entregada con `recipientName`.
- Marcar no entregada con `failedReason`.
- Refrescar estado después de cada acción.
- Mostrar errores controlados.

Permisos UI:

- `deliveries.view`: ver sección de seguimiento.
- `deliveries.assign`: crear entrega y asignar repartidor.
- `deliveries.update`: marcar salida.
- `deliveries.complete`: marcar entregada o no entregada.

La carga de repartidores reutiliza `/api/admin/users` y `/api/admin/roles` cuando el usuario tiene permisos administrativos suficientes. Si no se puede filtrar de forma segura por rol `Repartidor`, la UI muestra advertencia y selector controlado de usuarios activos. El backend conserva la validación de usuario activo y permiso `deliveries.view`.

Exclusiones:

- No se crea `/app/entregas`.
- No se implementa UI mobile-first de repartidor.
- No se cambian etiquetas de impresión.
- No se agregan endpoints, migraciones ni cambios de backend.

### Implementación Fase 3.4.2.1

El listado `/app/ordenes` muestra ahora dos estados separados:

- `Estado`: estado operativo de la orden, tomado de `WorkOrder.Status`.
- `Entrega`: estado logístico de entrega, tomado de `WorkOrderDelivery.Status` cuando existe.

El contrato existente `GET /api/work-orders` agrega un resumen opcional `delivery` por item de listado:

- `deliveryId`
- `deliveryStatus`
- `deliveryStatusLabel`
- `assignedToUserName`
- `deliveredAtUtc`
- `failedAtUtc`

Si una orden no tiene entrega, `delivery` regresa `null` y la UI muestra `Sin entrega`. Si la entrega está fallida, `deliveryStatus` es `FailedDelivery` y la UI muestra el badge `No entregada`.

Decisión funcional: marcar una entrega como `No entregada` no cambia `WorkOrder.Status`. `No entregada` es `DeliveryStatus`, no `WorkOrderStatus`. Esto evita mezclar producción/operación de laboratorio con logística de reparto. La orden puede seguir apareciendo como `Recibida`, `En proceso` u otro estado operativo mientras el intento logístico queda registrado aparte.

Exclusiones:

- No se crea otro panel de órdenes.
- No se crea endpoint nuevo; se amplía el listado actual.
- No se crea migración.
- No se cambian estados existentes de `WorkOrderStatus`.
- No se toca `/login`, `/app/dashboard`, auth, guards, cookies, XSRF ni deploy.

Validación DEV sugerida:

1. Abrir `/app/ordenes`.
2. Confirmar que la tabla/cards muestran `Estado` y `Entrega`.
3. En una orden sin entrega, confirmar badge `Sin entrega`.
4. Crear o ubicar una entrega marcada como `FailedDelivery`.
5. Confirmar que `/app/ordenes` muestra `No entregada` en `Entrega`.
6. Confirmar que `Estado` conserva el valor operativo de `WorkOrder.Status` y no cambia a `No entregada`.

### Implementación Fase 3.4.3

La ruta privada `/app/entregas` agrega la UI mobile-first para repartidor sin duplicar `/app/ordenes`.

Capacidades implementadas:

- Navegación privada `Entregas` visible con `deliveries.view`.
- Ruta `/app/entregas` protegida con `deliveries.view`.
- Ruta `/app/entregas/:id` protegida con `deliveries.view`.
- Listado de cards mobile-first usando `GET /api/deliveries?assignedToMe=true`.
- Detalle que valida en frontend que la entrega esté asignada al usuario autenticado antes de mostrar datos.
- Datos visibles: folio, cliente, paciente, referencia, trabajo, estado de entrega, fecha de entrega, dirección, contacto, estado de orden y seguimiento.
- Cierre con `recipientName` requerido cuando la entrega está `OutForDelivery`.
- No entrega con `failedReason` requerido cuando la entrega está `Assigned` u `OutForDelivery`.
- Lectura sin acciones si el usuario no tiene `deliveries.complete`.
- Loading, estado vacío y errores controlados.

Exclusiones:

- No se permite asignar repartidor desde la UI del repartidor.
- No se muestran pagos, saldos ni información financiera.
- No se otorga acceso amplio a órdenes, clientes, pagos, usuarios ni roles.
- No se crean endpoints, migraciones ni cambios backend.
- No se toca `AuthService`, guards, cookies, XSRF ni deploy.

### Modelo Actual De Orden

La orden de trabajo actual contiene:

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

En frontend, el detalle de orden expone folio, cliente, tipo de cliente, doctor interno, paciente, fechas, referencia, color, costo total, observaciones, historial de estados y sección de pagos si el usuario tiene `payments.view`.

`DeliveryDate` hoy significa fecha planeada/capturada de entrega. No representa fecha/hora real de entrega ni evidencia de recibido.

### Estados Actuales De Orden

Estados existentes:

| Estado técnico | Etiqueta actual |
| --- | --- |
| `Received` | Recibida |
| `InProcess` | En proceso |
| `FirstTrial` | En primera prueba |
| `SecondTrial` | En segunda prueba |
| `ReadyForDelivery` | Lista para entrega |
| `Delivered` | Entregada |
| `Cancelled` | Cancelada |

Una orden cancelada es terminal para el MVP actual. Todo cambio real de estado genera historial.

### Datos De Cliente Disponibles

El cliente actual tiene:

- Tipo: `Doctor`, `Clinic` u `Other`.
- Nombre visible.
- Razón social.
- Nombre de contacto.
- Teléfono.
- WhatsApp.
- Email.
- Dirección.
- Notas.
- Estado activo/inactivo.
- Doctores internos para clientes tipo clínica.

Limitación actual: el detalle de orden incluye `CustomerDisplayName`, `CustomerType`, `InternalDoctorFullName` y `CustomerId`, pero no incluye dirección, teléfono, WhatsApp, email ni notas del cliente. Para etiquetas de entrega y vista de repartidor se deberá ampliar el DTO de orden, consultar el cliente por `CustomerId` o crear un DTO específico de entrega.

### Datos De Entrega Disponibles O Faltantes

Disponible hoy:

- Folio de orden.
- Cliente.
- Doctor interno, si aplica.
- Paciente.
- Trabajo solicitado.
- Color.
- Fechas de recepción, pruebas y entrega planeada.
- Estado operativo.
- Observaciones.
- Historial de estados.

Faltante hoy:

- Repartidor asignado.
- Estado específico de entrega/reparto.
- Fecha/hora de salida a ruta.
- Usuario que registró salida.
- Nombre de quien recibió.
- Fecha/hora real de entrega tomada del servidor.
- Observaciones de entrega.
- Intento fallido/no entregado y motivo.
- Evidencia futura: foto, firma, ubicación o escaneo.
- Snapshot de dirección/contacto al momento de salida, si se requiere trazabilidad contra cambios posteriores del cliente.

Fase 3.4.0 confirma que estos datos no deben mezclarse sin análisis con `DeliveryDate`: `DeliveryDate` sigue siendo fecha planeada/capturada de entrega, mientras que salida, entrega real, receptor y no entrega pertenecen al flujo logístico.

### Pagos Relacionados

Los pagos ya están asociados a órdenes:

- Listado de pagos por orden.
- Resumen financiero por orden.
- Total de orden (`TotalAmount`).
- Monto pagado.
- Saldo calculado.
- Estado financiero calculado.
- Pagos activos/cancelados.

Para entrega, el saldo puede mostrarse como apoyo operativo si administración lo requiere, pero no debe bloquear el MVP de etiquetas ni reparto salvo decisión explícita del cliente.

### Roles Y Permisos Existentes

Permisos actuales relevantes:

- `orders.view`
- `orders.create`
- `orders.edit`
- `orders.changeStatus`
- `payments.view`
- `payments.create`
- `payments.cancel`
- `customers.view`
- `customers.create`
- `customers.edit`
- `users.manage`
- `roles.manage`
- `reports.view`

Permisos de reparto implementados en Fase 3.4.1:

- `deliveries.view`: ver entregas asignadas o panel de entregas.
- `deliveries.assign`: crear/asignar entrega desde administración.
- `deliveries.update`: marcar salida a ruta y actualizar notas.
- `deliveries.complete`: registrar entrega completada o no entregada.
- `labels.print`: imprimir etiquetas, si se decide separar de `orders.view`.

Para el MVP de etiquetas desde detalle de orden puede bastar inicialmente con `orders.view`, pero si imprimir etiquetas se considera acción sensible conviene agregar permiso explícito en una fase posterior.

### Usuarios Y Roles

Existe modelo de usuarios, roles, permisos y seed:

- Rol `Admin` con todos los permisos.
- Seed QA limitado solo `Development`, desactivado por default.
- Fase 3.3 implementa `/app/admin/usuarios` y `/app/admin/roles`.
- Rol `Repartidor` preparado con permisos mínimos de entregas (`deliveries.view` y `deliveries.complete`).

Usuarios ya permite CRUD administrativo mínimo y asignación de roles existentes; Roles queda readonly para ver permisos. Para el flujo repartidor ya existen permisos `deliveries.*`, modelo de entregas, endpoints, UI admin desde `/app/ordenes/:id` y UI mobile-first bajo `/app/entregas`.

### Catálogo Público Y Backlog

El catálogo público actual vive en `/catalogo` y usa `src/LaboratorioTlahuac.Web/src/app/public/data/catalog-data.ts`.

Estado actual:

- 12 secciones.
- 40 productos.
- Precios de referencia 2026 como números en frontend.
- Imágenes locales en `src/LaboratorioTlahuac.Web/src/assets/catalog/products/`.
- Placeholders intencionales para productos sin imagen.

La administración de catálogo, precios e imágenes sigue como backlog privado bajo `/app`; no debe bloquear el flujo operativo de órdenes, etiquetas y entregas.

## Flujo Operativo Futuro

### Recepción De Trabajo

1. Cliente entrega o solicita trabajo.
2. Usuario administrativo crea la orden en `/app/ordenes/nueva`.
3. La orden recibe folio único (`OrderNumber`).
4. Se imprime etiqueta interna desde el detalle de orden.
5. La etiqueta se pega al trabajo físico.
6. El trabajo físico queda identificado con folio, paciente, cliente y descripción mínima.

### Seguimiento Interno

1. Administración consulta `/app/ordenes`.
2. Se filtra por cliente, estado y fecha de entrega planeada.
3. Se actualiza estado operativo.
4. Se registran observaciones y fechas relevantes.
5. Se revisan pagos, abonos y saldo.
6. Si aplica en fase futura, se registra responsable interno.

Estados operativos sugeridos usando el modelo actual:

- `Received`: trabajo recibido.
- `InProcess`: producción interna.
- `FirstTrial`: primera prueba.
- `SecondTrial`: segunda prueba.
- `ReadyForDelivery`: listo para salida.
- `Delivered`: entregado.
- `Cancelled`: cancelado.

### Salida A Repartidor

1. Administración confirma que la orden está lista para entrega.
2. Se imprime etiqueta de entrega desde `/app/ordenes/:id`.
3. Se asigna repartidor.
4. Se registra salida con fecha/hora de servidor.
5. La entrega queda visible en la pantalla mobile-first del repartidor.

### Entrega

1. Repartidor entra a la ruta privada mobile-first.
2. Ve entregas asignadas.
3. Abre el detalle de entrega.
4. Revisa cliente, dirección, contacto, indicaciones y trabajos.
5. Marca como entregado.
6. Captura nombre de quien recibió.
7. El servidor registra fecha/hora real.
8. Administración ve estado actualizado en orden/entregas.
9. En fases futuras se puede agregar foto, firma, geolocalización o escaneo QR/código.

## Estados Sugeridos Para Entrega

La orden ya tiene estados operativos. Para reparto conviene evaluar un estado separado de entrega para no mezclar producción con logística:

| Estado sugerido | Uso |
| --- | --- |
| `PendingAssignment` | Lista para entregar, sin repartidor. |
| `Assigned` | Repartidor asignado. |
| `OutForDelivery` | Salió a ruta. |
| `Delivered` | Entrega completada. |
| `FailedDelivery` | No se pudo entregar. |
| `Cancelled` | Entrega cancelada. |

Alternativa mínima para MVP: usar `ReadyForDelivery` y `Delivered` en `WorkOrderStatus`, y agregar solo datos de asignación/recibido. La desventaja es que se pierde trazabilidad fina de intentos y salida.

Recomendación Fase 3.4.0: usar `DeliveryStatus` separado con los nombres anteriores. `FailedDelivery` se prefiere sobre `Failed` para evitar confundir fallos técnicos con intentos logísticos no entregados.

## Campos Nuevos Posibles

Fase 3.2 de etiquetas quedó implementada sin campos nuevos al imprimir datos existentes y textos pendientes seguros para dirección/contacto.

Fase 3.4.1 implementa base de datos para reparto. Fase 3.4.0 comparó dos alternativas:

- Extender `WorkOrder` con campos de entrega.
- Crear entidad `Delivery` o `WorkOrderDelivery`.

Se implementa `WorkOrderDelivery` para trazabilidad real y crecimiento futuro. Campos mínimos implementados:

- Entidad `WorkOrderDelivery`.
- `WorkOrderId`.
- `AssignedToUserId`.
- `Status`.
- `RecipientName`.
- `DeliveryNotes`.
- `FailedReason`.
- `AssignedAtUtc`.
- `OutForDeliveryAtUtc`.
- `DeliveredAtUtc`.
- `FailedAtUtc`.
- `CreatedAtUtc`.
- `UpdatedAtUtc`.

Campo no implementado en 3.4.1:

- Snapshot opcional: `CustomerDisplayName`, `DeliveryAddress`, `ContactPhone`, `ContactWhatsApp`.

Evidencia futura:

- `SignatureImagePath`.
- `PhotoEvidencePath`.
- `GeoLatitude`.
- `GeoLongitude`.
- `ScannedCodeAtUtc`.

## Permisos Sugeridos

MVP etiquetas:

- `orders.view` para ver detalle e imprimir datos de la orden.
- Evaluar `labels.print` si se quiere auditar o restringir impresión.

MVP reparto implementado:

- `deliveries.view`: ver entregas.
- `deliveries.assign`: crear/asignar entrega desde administración.
- `deliveries.update`: marcar salida a ruta y actualizar notas.
- `deliveries.complete`: marcar entregada o no entregada.
- `orders.view`: consultar datos esenciales de orden.
- `customers.view`: solo si la ruta de entrega consulta datos completos del cliente; preferir DTO mínimo de entrega para no exponer de más al repartidor.

Rol `Repartidor` implementado: `deliveries.view` y `deliveries.complete`, sin `deliveries.assign`, `deliveries.update`, `orders.view`, `customers.view` ni `payments.view`.

Administración/operación recomendada: `orders.view`, `orders.changeStatus`, `deliveries.view`, `deliveries.assign` y, si operará correcciones/cierres, `deliveries.update` y `deliveries.complete`.

## Endpoints Sugeridos Fase 3.4.1

MVP implementado:

- `GET /api/deliveries`: lista entregas; soporta `status`, `assignedToMe`, `page` y `pageSize`.
- `GET /api/deliveries/{id}`: detalle de entrega, filtrado por usuario asignado salvo permiso administrativo.
- `GET /api/work-orders/{workOrderId}/delivery`: seguimiento desde detalle de orden.
- `POST /api/work-orders/{workOrderId}/delivery`: crear entrega inicial `PendingAssignment`.
- `PATCH /api/deliveries/{id}/assign`: asignar repartidor.
- `PATCH /api/deliveries/{id}/out-for-delivery`: registrar salida.
- `PATCH /api/deliveries/{id}/complete`: registrar entrega, `RecipientName` obligatorio.
- `PATCH /api/deliveries/{id}/failed`: registrar no entrega, `FailedReason` obligatorio.

El endpoint de repartidor debe devolver un DTO mínimo de entrega con cliente, dirección/contacto necesarios, folio, paciente/referencia, trabajo e indicaciones, sin información financiera en MVP.

## Impacto En Base De Datos

Fase 3.2 etiquetas:

- Sin cambios de base si las etiquetas usan datos existentes.
- Sin migraciones si solo se agregan rutas privadas de impresión y CSS.

Fase 3.4.1 reparto:

- Migración nueva `20260704053734_AddWorkOrderDeliveries`.
- Tabla nueva `WorkOrderDeliveries`.
- Relación requerida con `WorkOrders`.
- Relación opcional con `Security.Users` para `AssignedToUser`.
- Índices por `WorkOrderId` único, `AssignedToUserId`, `Status` y `CreatedAtUtc`.
- Dirección/contacto se lee vivo desde `Customer` en el DTO de entrega; no se guarda snapshot todavía.
- Una entrega por orden en MVP, con posibilidad posterior de historial de intentos.

Fase 3.3 usuarios/roles:

- Implementada con tablas existentes, endpoints privados, pantallas y reglas de seguridad.
- Preparó rol `Repartidor` sin permisos activos ni acceso amplio a órdenes completas.
- Fase 3.4.1 sincroniza `Repartidor` con `deliveries.view` y `deliveries.complete`.

Fase 3.5 catálogo:

- Requerirá modelo propio, endpoints, migraciones y almacenamiento de imágenes si se migra desde `catalog-data.ts`.

## Prioridad Recomendada

1. Validar Fase 3.2 con impresora térmica real en DEV.
2. Fase 3.3: administración de usuarios/roles MVP. Implementada y subida a `origin/dev`.
3. Fase 3.4.0: análisis técnico previo de entrega/repartidor mobile-first. Documentado.
4. Fase 3.4.1: backend delivery MVP + permisos. Implementada.
5. Fase 3.4.2: UI admin desde órdenes. Implementada.
6. Fase 3.4.2.1: estado de entrega en listado `/app/ordenes`. Implementada.
7. Fase 3.4.3: UI repartidor mobile-first. Implementada.
8. Fase 3.4.4: QA DEV y ajustes.
9. Fase 3.5: administración de catálogo, precios e imágenes.

La siguiente fase implementable mayor es Fase 3.4.4 - QA DEV y ajustes del flujo de entregas. La prueba física de etiquetas puede seguir en paralelo porque no bloquea la UI de entregas.
