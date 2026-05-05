# Diseño Conceptual De Base De Datos

No se crea base de datos en esta fase. Este documento define una propuesta conceptual inicial.

## Tablas Sugeridas

- Users
- Roles
- Permissions
- RolePermissions
- UserRoles
- Customers
- InternalDoctors
- WorkOrders
- WorkOrderStatusHistory
- Payments
- Suppliers
- InventoryItems
- InventoryMovements

## Relaciones Conceptuales

- Users tiene muchos Roles mediante UserRoles.
- Roles tiene muchos Permissions mediante RolePermissions.
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
