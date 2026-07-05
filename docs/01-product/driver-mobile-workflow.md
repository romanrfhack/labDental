# Flujo Mobile-First Para Repartidor

Fuente funcional para la Fase 3.4 de entregas/repartidor. Este documento define el MVP de repartidor desde navegador móvil.

Actualización Fase 3.4.0: el análisis técnico previo queda documentado en `docs/01-product/delivery-mvp-design.md`. La recomendación es crear una entidad separada `WorkOrderDelivery` para trazabilidad real, en lugar de extender `WorkOrder` salvo que se decida un MVP extremadamente rápido.

Actualización Fase 3.4.1: backend/API implementado sin UI. Ya existen `WorkOrderDelivery`, permisos `deliveries.*`, migración `AddWorkOrderDeliveries` y endpoints privados de entregas.

Actualización Fase 3.4.2: UI admin de entregas implementada desde `/app/ordenes/:id` para crear entrega, asignar repartidor, marcar salida, entregar y marcar no entregada.

Actualización Fase 3.4.3: UI mobile-first de repartidor implementada bajo `/app/entregas` y `/app/entregas/:id`. El listado consume `GET /api/deliveries?assignedToMe=true`; el detalle valida que la entrega cargada esté asignada al usuario autenticado antes de mostrar datos; `deliveries.complete` habilita marcar entregada con `recipientName` o no entregada con `failedReason`.

Actualización Fase 3.4.3.1: el repartidor sin `returnUrl` interno válido queda redirigido a `/app/entregas` después del login si tiene `deliveries.view` y no tiene `reports.view`. El detalle mobile permite `Reintentar entrega` cuando la entrega propia está en `FailedDelivery`; la acción vuelve el estado a `OutForDelivery`, actualiza `OutForDeliveryAtUtc` con hora de servidor y mantiene el repartidor asignado.

## Objetivo

Permitir que el repartidor consulte entregas asignadas desde celular, confirme la información esencial y registre quién recibió la entrega con fecha/hora de servidor.

## Rol Repartidor

Rol futuro sugerido: `Repartidor`.

Permisos implementados para `Repartidor` en Fase 3.4.1:

- `deliveries.view`: ver entregas asignadas.
- `deliveries.complete`: registrar entrega completada o no entregada.

Permisos administrativos opcionales:

- `deliveries.assign`: asignar repartidor y registrar salida.
- `deliveries.viewAll`: ver todas las entregas, si se separa de `deliveries.view`.

El backend actual ya tiene estos permisos y módulo API de entregas. Fase 3.3 preparó administración MVP de usuarios/roles y Fase 3.4.1 activó el rol `Repartidor` con permisos mínimos.

Para el rol `Repartidor`, la combinación implementada en MVP es `deliveries.view` y `deliveries.complete`, sin `deliveries.assign`, `deliveries.update`, `orders.view`, `customers.view`, `payments.view`, `users.manage` ni `roles.manage`. La salida a ruta queda para Admin/operación con `deliveries.update` hasta que se defina si el repartidor debe registrar esa transición desde móvil.

## Ruta Recomendada

Ruta privada recomendada:

- `/app/entregas`
- `/app/entregas/:id`

Motivo: permite crecer a listado de entregas para administración y repartidor sin acoplar todo a una persona. Si se quiere una entrada más explícita para el rol, puede evaluarse `/app/repartidor` como alias o vista filtrada, pero no debe reemplazar ni duplicar `/app/ordenes`.

El texto de navegación puede cambiar por contexto: `Entregas` para administración y `Mis entregas` para repartidor, usando la misma ruta.

## Pantalla First-Mobile

La pantalla debe diseñarse primero para celular:

- Listado vertical de entregas asignadas.
- Acciones táctiles grandes.
- Estados visibles.
- Información priorizada para ruta.
- Evitar tablas anchas en móvil.
- Carga rápida y legible en pantallas pequeñas.

## Listado De Entregas Asignadas

Datos mínimos por entrega:

- Folio de orden.
- Cliente.
- Dirección corta o zona.
- Contacto principal.
- Fecha de entrega planeada.
- Estado de entrega.
- Número de trabajos/órdenes, si se agrupan en una entrega futura.

Filtros mínimos:

- Hoy.
- Pendientes.
- En ruta.
- Entregadas.

## Detalle De Entrega

Datos visibles:

- Folio.
- Cliente.
- Dirección completa.
- Contacto.
- Teléfono/WhatsApp disponible.
- Indicaciones.
- Paciente o referencia.
- Trabajo solicitado.
- Color, si existe.
- Estado de orden.
- Estado de entrega.
- Observaciones relevantes.

No mostrar información financiera al repartidor salvo decisión explícita del cliente. Si se muestra saldo, debe estar justificado por operación de cobranza y protegido por permiso.

## Acciones

Backend MVP implementado y UI admin disponible desde órdenes:

- Ver detalle.
- Marcar como entregado.
- Capturar `Recibió`.
- Marcar no entregado con motivo.
- Guardar entrega.

Acciones implementadas en UI mobile-first Fase 3.4.3:

- Listar entregas asignadas al usuario autenticado.
- Abrir detalle de entrega.
- Marcar entregada cuando la entrega está `OutForDelivery`.
- Capturar `recipientName`.
- Marcar no entregada cuando la entrega está `Assigned` u `OutForDelivery`.
- Capturar `failedReason`.
- Reintentar entrega fallida cuando la entrega propia está `FailedDelivery`.
- Mostrar lectura sin acciones si falta `deliveries.complete`.

Acciones pendientes de UI mobile-first:

- Marcar en ruta desde móvil si se otorga `deliveries.update` al rol.
- Llamar/abrir WhatsApp desde contacto.
- Escanear QR/código.
- Capturar foto.
- Capturar firma.
- Historial completo de intentos de entrega.

## Validación De Entrega

Reglas mínimas:

- Solo usuario autenticado.
- Solo entregas asignadas al repartidor, salvo permiso administrativo.
- `Recibió` obligatorio para marcar entregado.
- Fecha/hora de entrega tomada del servidor, no del celular.
- No permitir marcar entregada una orden cancelada.
- No permitir modificar entrega ya cerrada salvo permiso administrativo futuro.
- Registrar usuario que realizó la acción.

Resultado implementado en Fase 3.4.3:

- Administración ve quién entregó.
- Administración ve cuándo se entregó.
- Administración ve a qué cliente se entregó.
- Administración ve quién recibió.
- La entrega queda en `Delivered` cuando el backend acepta el cierre.
- La orden cambia a `Delivered` en la misma operación si el backend completa la entrega correctamente.
- Una no entrega queda como `FailedDelivery` sin convertir `WorkOrder.Status` a otro estado.
- Una entrega `FailedDelivery` puede reintentarse y volver a `OutForDelivery` sin cambiar `WorkOrder.Status`.
- Si el reintento se cierra como entregado, `WorkOrder.Status` pasa a `Delivered`.
- Si el reintento vuelve a fallar, `failedReason` y `failedAtUtc` se actualizan al último intento fallido.
- No existe historial completo de intentos todavía; queda como fase futura.

## Modelo Recomendado Fase 3.4.0

Recomendado: entidad separada `WorkOrderDelivery`.

Campos mínimos:

- `WorkOrderId`.
- `AssignedDriverUserId`.
- `AssignedAtUtc`.
- `AssignedByUserId`.
- `Status`.
- `OutForDeliveryAtUtc`.
- `OutForDeliveryByUserId`.
- `DeliveredAtUtc`.
- `DeliveredByUserId`.
- `RecipientName`.
- `DeliveryNotes`.
- `FailedReason`.

Estados implementados en backend:

- `PendingAssignment`.
- `Assigned`.
- `OutForDelivery`.
- `Delivered`.
- `FailedDelivery`.

El detalle completo de comparación contra extender `WorkOrder` queda en `docs/01-product/delivery-mvp-design.md`.

## Seguridad

- La ruta vive bajo `/app` y requiere sesión.
- No crear rutas públicas de entrega con datos sensibles.
- El repartidor no debe ver todas las órdenes si solo tiene entregas asignadas.
- No exponer dirección/contacto de clientes ajenos a sus entregas.
- No confiar en fecha/hora del dispositivo móvil para cierre.
- Evitar capturar evidencia sensible sin definir almacenamiento, permisos y retención.
- Si se agregan fotos o firmas, documentar almacenamiento, acceso y eliminación antes de implementar.

## Impacto Técnico Futuro

La Fase 3.4 requerirá:

- Modelo de entrega/asignación.
- Relación con orden.
- Relación con usuario repartidor.
- Permisos nuevos.
- Endpoints privados de entrega.
- Pantallas mobile-first.
- Pruebas de autorización.
- Migración de base de datos.

La Fase 3.2 de etiquetas no debe esperar este modelo; puede imprimir desde órdenes existentes.
