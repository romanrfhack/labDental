# Diseño De Base De Datos

La Etapa 2 crea la primera migración real para seguridad. Las entidades operativas siguen en diseño conceptual hasta sus etapas correspondientes.

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

## Tablas Sugeridas

- Customers
- InternalDoctors
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
