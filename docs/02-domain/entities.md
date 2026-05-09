# Entidades Conceptuales

Este documento describe entidades de dominio sin definir código, ORM ni esquema final.

## User

Usuario con acceso al sistema privado.

Campos iniciales:

- `Id`
- `Email`
- `NormalizedEmail`
- `FullName`
- `PasswordHash`
- `IsActive`
- `AccessFailedCount`
- `LockoutEndUtc`
- `LastLoginAtUtc`
- `CreatedAtUtc`
- `UpdatedAtUtc`

Reglas principales:

- `NormalizedEmail` se usa para búsquedas.
- Email es único.
- Usuario inactivo o bloqueado no puede iniciar sesión.
- `PasswordHash` nunca se devuelve en respuestas.

## Role

Agrupación de permisos asignable a usuarios.

Campos iniciales:

- `Id`
- `Name`
- `NormalizedName`
- `Description`
- `IsSystem`
- `CreatedAtUtc`
- `UpdatedAtUtc`

`NormalizedName` es único.

## Permission

Acción granular autorizable.

Campos iniciales:

- `Id`
- `Key`
- `Description`
- `CreatedAtUtc`

`Key` es único y corresponde a las constantes de `Permissions`.

## UserRole

Relación explícita many-to-many entre usuarios y roles.

Campos:

- `UserId`
- `RoleId`

## RolePermission

Relación explícita many-to-many entre roles y permisos.

Campos:

- `RoleId`
- `PermissionId`

## Customer

Cliente del laboratorio. Puede representar doctor, doctora, clínica u otro.

Campos implementados:

- `Id`
- `Type`
- `DisplayName`
- `LegalName`
- `ContactName`
- `Phone`
- `WhatsApp`
- `Email`
- `Address`
- `Notes`
- `IsActive`
- `CreatedAtUtc`
- `CreatedByUserId`
- `UpdatedAtUtc`
- `UpdatedByUserId`

Reglas principales:

- `DisplayName` y `Type` son obligatorios.
- Email es opcional, pero debe tener formato válido si se captura.
- No hay unicidad global estricta en `DisplayName`.
- Se desactiva con `IsActive`; no hay borrado físico.

## CustomerType

Tipo de cliente:

- `Doctor`
- `Clinic`
- `Other`

## InternalDoctor

Doctor asociado internamente a una clínica.

Campos implementados:

- `Id`
- `CustomerId`
- `FullName`
- `Phone`
- `WhatsApp`
- `Email`
- `Notes`
- `IsActive`
- `CreatedAtUtc`
- `CreatedByUserId`
- `UpdatedAtUtc`
- `UpdatedByUserId`

Reglas principales:

- Solo puede pertenecer a `Customer.Type = Clinic`.
- `FullName` es obligatorio.
- Se desactiva con `IsActive`; no hay borrado físico.

## WorkOrder

Orden de trabajo dental. Es la entidad central del flujo operativo del MVP.

Campos implementados:

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

Reglas principales:

- Pertenece a un `Customer` obligatorio.
- `InternalDoctor` es opcional y solo válido cuando el cliente es `Clinic`.
- `OrderNumber` es obligatorio, único y generado por el sistema.
- `PatientName`, `ReceivedDate` y `WorkDescription` son obligatorios.
- `TotalAmount` es opcional y no representa saldo.
- No hay delete físico.
- Una orden `Cancelled` no se edita en el MVP.

## WorkOrderStatus

Estado operativo de una orden. Valores internos estables:

- `Received`
- `InProcess`
- `FirstTrial`
- `SecondTrial`
- `ReadyForDelivery`
- `Delivered`
- `Cancelled`

## WorkOrderStatusHistory

Historial de cambios de estado de una orden.

Campos implementados:

- `Id`
- `WorkOrderId`
- `FromStatus`
- `ToStatus`
- `Notes`
- `ChangedAtUtc`
- `ChangedByUserId`

Reglas principales:

- Todo cambio real de estado crea un registro.
- El registro inicial usa `FromStatus = null` y `ToStatus = Received`.
- Cambiar al mismo estado no duplica historial.

## Payment

Movimiento financiero asociado obligatoriamente a una orden de trabajo.

Campos implementados:

- `Id`
- `WorkOrderId`
- `PaymentDate`
- `Amount`
- `Method`
- `Reference`
- `Notes`
- `IsCancelled`
- `CancelledAtUtc`
- `CancelledByUserId`
- `CancellationReason`
- `CreatedAtUtc`
- `CreatedByUserId`

Reglas principales:

- Pertenece a una `WorkOrder`.
- `Amount` debe ser mayor a 0.
- `PaymentDate` y `Method` son obligatorios.
- `Reference` y `Notes` son opcionales.
- No hay delete físico.
- No se editan pagos en el MVP.
- Un pago se cancela con motivo obligatorio.
- Un pago cancelado no cuenta para saldo.

## PaymentMethod

Método de pago con valores internos estables:

- `Cash`: Efectivo.
- `BankTransfer`: Transferencia.
- `Card`: Tarjeta.
- `Other`: Otro.

## PaymentStatus

Estado financiero calculado, no capturado manualmente:

- `TotalNotSet`: Total no definido.
- `Unpaid`: Sin pago.
- `Partial`: Pago parcial.
- `Paid`: Pagada.
- `Overpaid`: Saldo a favor / revisar.

## Supplier

Proveedor de materiales o servicios. Campos principales: nombre, contacto, teléfono, email, dirección, observaciones, estado activo.

## InventoryItem

Material o insumo inventariable. Campos principales: nombre, unidad, stock actual, stock mínimo, proveedor preferido opcional, estado activo.

## InventoryMovement

Movimiento de inventario. Campos principales: material, tipo de movimiento, cantidad, fecha, referencia, usuario y observaciones.

## Criterios De Validación

- Las entidades soportan el MVP sin forzar inventario avanzado.
- La orden conserva referencias a cliente, paciente, fechas, total, pagos y estado.
- Roles y permisos quedan preparados desde el diseño inicial.
