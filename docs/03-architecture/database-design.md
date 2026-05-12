# Diseño De Base De Datos

La Etapa 2 crea la primera migración real para seguridad. La Etapa 3 agrega clientes y doctores internos. La Etapa 4 agrega órdenes de trabajo como núcleo operativo. La Etapa 5 agrega pagos como movimientos financieros. La Etapa 6 agrega dashboard operativo básico sin cambios de esquema.

## Tablas Implementadas En Seguridad

Esquema `Security`:

- `Users`
- `Roles`
- `Permissions`
- `UserRoles`
- `RolePermissions`

Índices únicos:

- `Users.NormalizedEmail`
- `Roles.NormalizedName`
- `Permissions.Key`

Relaciones:

- `Users` tiene muchos `Roles` mediante `UserRoles`.
- `Roles` tiene muchos `Permissions` mediante `RolePermissions`.

## Tablas Implementadas En Clientes

Esquema default:

- `Customers`
- `InternalDoctors`

Índices:

- `Customers.Type`
- `Customers.IsActive`
- `Customers.DisplayName`
- `InternalDoctors.CustomerId`
- `InternalDoctors.IsActive`

Relaciones:

- `Customers` tiene muchos `InternalDoctors`.
- `InternalDoctors.CustomerId` referencia `Customers.Id` con delete restrictivo.

## Tablas Implementadas En Órdenes

Esquema default:

- `WorkOrders`
- `WorkOrderStatusHistory`

Índices:

- `WorkOrders.OrderNumber` único
- `WorkOrders.CustomerId`
- `WorkOrders.InternalDoctorId`
- `WorkOrders.Status`
- `WorkOrders.ReceivedDate`
- `WorkOrders.DeliveryDate`
- `WorkOrders.PatientName`
- `WorkOrderStatusHistory.WorkOrderId`
- `WorkOrderStatusHistory.ChangedAtUtc`

Relaciones:

- `Customers` tiene muchas `WorkOrders`.
- `InternalDoctors` tiene muchas `WorkOrders` de forma opcional.
- `WorkOrders` tiene muchos `WorkOrderStatusHistory`.
- `WorkOrders.CreatedByUserId` y `WorkOrders.UpdatedByUserId` referencian `Security.Users`.
- `WorkOrderStatusHistory.ChangedByUserId` referencia `Security.Users`.
- Todas las relaciones operativas usan delete restrictivo.

## Tablas Implementadas En Pagos

Esquema default:

- `Payments`

Índices:

- `Payments.WorkOrderId`
- `Payments.PaymentDate`
- `Payments.Method`
- `Payments.IsCancelled`
- `Payments.CreatedAtUtc`

Relaciones:

- `WorkOrders` tiene muchos `Payments`.
- `Payments.WorkOrderId` referencia `WorkOrders.Id` con delete restrictivo.
- `Payments.CreatedByUserId` referencia `Security.Users`.
- `Payments.CancelledByUserId` referencia `Security.Users`.
- Los pagos cancelados se conservan y no se eliminan físicamente.

## Tablas Sugeridas

- Suppliers
- InventoryItems
- InventoryMovements

## Relaciones Conceptuales

- Customers tiene muchas WorkOrders.
- Customers puede tener muchos InternalDoctors cuando representa una clínica.
- InternalDoctors puede relacionarse opcionalmente con WorkOrders.
- WorkOrders tiene muchos Payments.
- WorkOrders tiene muchos WorkOrderStatusHistory.
- InventoryItems tiene muchos InventoryMovements.
- Suppliers puede relacionarse con InventoryItems.

## Reglas De Diseño

- Usar identificadores estables.
- Evitar borrar físicamente registros operativos o financieros sin política explícita.
- Mantener historial para cambios relevantes.
- Preparar migraciones incrementales y reversibles cuando sea posible.

## Criterios De Validación

- El modelo soporta el MVP.
- Los saldos pueden calcularse desde WorkOrders y Payments.
- Los permisos pueden crecer sin alterar usuarios existentes.
