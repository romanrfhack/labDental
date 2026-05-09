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

Cliente del laboratorio. Puede representar doctor, doctora, clínica u otro. Campos principales: identificador, tipo, nombre comercial o profesional, teléfono, email, dirección, observaciones, estado activo.

## InternalDoctor

Doctor asociado internamente a una clínica. Campos principales: identificador, customer de clínica, nombre, contacto y observaciones.

## WorkOrder

Orden de trabajo dental. Campos principales: identificador, cliente, doctor interno opcional, paciente, trabajo, folio/nota, color, fecha de recepción, fechas de pruebas, fecha de entrega estimada o real, total, estado operativo, observaciones.

## WorkOrderStatusHistory

Historial de cambios de estado de una orden. Campos principales: orden, estado anterior, estado nuevo, usuario, fecha y motivo opcional.

## Payment

Movimiento de pago o abono asociado a una orden. Campos principales: orden, monto, método de pago, fecha, referencia, observaciones, estado vigente o cancelado, usuario.

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
