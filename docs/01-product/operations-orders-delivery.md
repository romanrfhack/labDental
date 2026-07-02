# Operación De Órdenes, Etiquetas Y Entrega

Fuente funcional para órdenes, etiquetas y reparto. Fase 3.1 documentó el análisis operativo; Fase 3.2 implementó el MVP de impresión de etiquetas desde órdenes existentes sin base de datos nueva, endpoints nuevos ni migraciones.

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

No existen permisos de reparto en el código actual. Permisos sugeridos para fases futuras:

- `delivery.view`: ver entregas asignadas o panel de entregas.
- `delivery.update`: avanzar estado de entrega y registrar recibido.
- `delivery.assign`: asignar repartidor y registrar salida, si se separa de administración.
- `labels.print`: imprimir etiquetas, si se decide separar de `orders.view`.

Para el MVP de etiquetas desde detalle de orden puede bastar inicialmente con `orders.view`, pero si imprimir etiquetas se considera acción sensible conviene agregar permiso explícito en una fase posterior.

### Usuarios Y Roles

Existe modelo de usuarios, roles, permisos y seed:

- Rol `Admin` con todos los permisos.
- Seed QA limitado solo `Development`, desactivado por default.
- Páginas `/app/admin/usuarios` y `/app/admin/roles` existen como placeholders.

No existe CRUD funcional de usuarios/roles en la app. Para rol repartidor se recomienda primero definir permisos y, si hace falta validar en DEV/local, usar mecanismo seguro de seed/QA antes de construir CRUD administrativo.

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
| `Failed` | No se pudo entregar. |
| `Cancelled` | Entrega cancelada. |

Alternativa mínima para MVP: usar `ReadyForDelivery` y `Delivered` en `WorkOrderStatus`, y agregar solo datos de asignación/recibido. La desventaja es que se pierde trazabilidad fina de intentos y salida.

## Campos Nuevos Posibles

Fase 3.2 de etiquetas quedó implementada sin campos nuevos al imprimir datos existentes y textos pendientes seguros para dirección/contacto.

Para Fase 3.3 de reparto sí se requerirá diseño de base de datos. Campos o entidades posibles:

- Entidad `Delivery` o `DeliveryAssignment`.
- `WorkOrderId`.
- `AssignedDriverUserId`.
- `AssignedAtUtc`.
- `AssignedByUserId`.
- `OutForDeliveryAtUtc`.
- `OutForDeliveryByUserId`.
- `DeliveredAtUtc`.
- `DeliveredByUserId`.
- `ReceivedByName`.
- `DeliveryNotes`.
- `FailureReason`.
- `DeliveryStatus`.
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

MVP reparto:

- `delivery.view`: ver entregas.
- `delivery.update`: marcar salida, no entregada o entregada según rol.
- `delivery.assign`: asignar repartidor, recomendado para administración.
- `orders.view`: consultar datos esenciales de orden.
- `customers.view`: solo si la ruta de entrega consulta datos completos del cliente; preferir DTO mínimo de entrega para no exponer de más al repartidor.

## Impacto En Base De Datos

Fase 3.2 etiquetas:

- Sin cambios de base si las etiquetas usan datos existentes.
- Sin migraciones si solo se agregan rutas privadas de impresión y CSS.

Fase 3.3 reparto:

- Probable migración nueva para entidad de entregas/asignaciones.
- Probable relación con `Users` para repartidor.
- Posible relación con `WorkOrders`.
- Posibles índices por repartidor, estado y fecha.
- Definir si dirección/contacto se lee vivo desde cliente o se guarda snapshot.

Fase 3.4 usuarios/roles:

- Puede apoyarse en tablas existentes, pero requiere endpoints, pantallas y reglas de seguridad.
- Evaluar si hace falta cambio de modelo antes de construir CRUD.

Fase 3.5 catálogo:

- Requerirá modelo propio, endpoints, migraciones y almacenamiento de imágenes si se migra desde `catalog-data.ts`.

## Prioridad Recomendada

1. Validar Fase 3.2 con impresora térmica real en DEV.
2. Fase 3.3: entrega/repartidor mobile-first.
3. Fase 3.4: administración de usuarios/roles.
4. Fase 3.5: administración de catálogo, precios e imágenes.

La siguiente fase implementable mayor es Fase 3.3, pero antes conviene cerrar prueba física de etiquetas para confirmar tamaño real, margen, escala del navegador y calibración de rollo.
