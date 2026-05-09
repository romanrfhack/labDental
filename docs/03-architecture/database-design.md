# Diseño De Base De Datos

La Etapa 2 crea la primera migración real para seguridad. La Etapa 3 agrega las primeras tablas operativas para clientes y doctores internos.

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

## Tablas Sugeridas

- WorkOrders
- WorkOrderStatusHistory
- Payments
- Suppliers
- InventoryItems
- InventoryMovements

## Relaciones Conceptuales

- Customers tiene muchas WorkOrders.
- Customers puede tener muchos InternalDoctors cuando representa una clínica.
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
