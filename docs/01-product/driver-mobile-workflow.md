# Flujo Mobile-First Para Repartidor

Fuente funcional para la futura Fase 3.4. Este documento define el MVP de repartidor desde navegador móvil. No implementa código, permisos, base de datos ni endpoints.

Actualización Fase 3.4.0: el análisis técnico previo queda documentado en `docs/01-product/delivery-mvp-design.md`. La recomendación es crear una entidad separada `WorkOrderDelivery` para trazabilidad real, en lugar de extender `WorkOrder` salvo que se decida un MVP extremadamente rápido.

## Objetivo

Permitir que el repartidor consulte entregas asignadas desde celular, confirme la información esencial y registre quién recibió la entrega con fecha/hora de servidor.

## Rol Repartidor

Rol futuro sugerido: `Repartidor`.

Permisos sugeridos:

- `deliveries.view`: ver entregas asignadas.
- `deliveries.update`: actualizar salida a ruta o no entrega.
- `deliveries.complete`: registrar entrega completada y recibido.

Permisos administrativos opcionales:

- `deliveries.assign`: asignar repartidor y registrar salida.
- `deliveries.viewAll`: ver todas las entregas, si se separa de `deliveries.view`.

El sistema actual todavía no tiene estos permisos ni módulo de entregas. Fase 3.3 ya preparó administración MVP de usuarios/roles y rol `Repartidor` sin permisos activos.

Para el rol `Repartidor`, la combinación recomendada en MVP es `deliveries.view`, `deliveries.update` y `deliveries.complete`, sin `orders.view`, `customers.view`, `payments.view`, `users.manage` ni `roles.manage`.

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

MVP:

- Ver detalle.
- Marcar como entregado.
- Capturar `Recibió`.
- Guardar entrega.

Acciones posteriores:

- Marcar en ruta.
- Marcar no entregado.
- Capturar motivo de no entrega.
- Llamar/abrir WhatsApp desde contacto.
- Escanear QR/código.
- Capturar foto.
- Capturar firma.

## Validación De Entrega

Reglas mínimas:

- Solo usuario autenticado.
- Solo entregas asignadas al repartidor, salvo permiso administrativo.
- `Recibió` obligatorio para marcar entregado.
- Fecha/hora de entrega tomada del servidor, no del celular.
- No permitir marcar entregada una orden cancelada.
- No permitir modificar entrega ya cerrada salvo permiso administrativo futuro.
- Registrar usuario que realizó la acción.

Resultado esperado:

- Administración ve quién entregó.
- Administración ve cuándo se entregó.
- Administración ve a qué cliente se entregó.
- Administración ve quién recibió.
- La entrega queda en `Delivered`.
- La orden puede cambiar a `Delivered` en la misma operación para conservar tableros actuales, siempre que el backend lo haga con timestamp de servidor y reglas documentadas.

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
- `ReceivedByName`.
- `DeliveryNotes`.
- `FailureReason`.

Estados recomendados:

- `PendingAssignment`.
- `Assigned`.
- `OutForDelivery`.
- `Delivered`.
- `FailedDelivery`.
- `Cancelled`.

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
