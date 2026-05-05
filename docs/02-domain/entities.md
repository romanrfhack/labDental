# Entidades Conceptuales

Este documento describe entidades de dominio sin definir código, ORM ni esquema final.

## User

Usuario con acceso al sistema privado. Campos principales: identificador, nombre, email, contraseña protegida, estado activo, fechas de creación y actualización.

## Role

Agrupación de permisos asignable a usuarios. Campos principales: identificador, nombre, descripción, estado activo.

## Permission

Acción granular autorizable. Campos principales: clave, descripción y módulo.

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
