# Reglas De Negocio

## Reglas Iniciales

- La orden de trabajo es la entidad central.
- El cliente puede ser doctor, doctora, clínica u otro.
- Una clínica puede tener doctores internos.
- Doctor y Other no pueden tener doctores internos.
- Los clientes se desactivan; no se eliminan físicamente.
- Los doctores internos se desactivan; no se eliminan físicamente.
- Los clientes inactivos no aparecen por default en búsquedas.
- Cambiar una clínica con doctores internos activos a Doctor u Other se rechaza con conflicto.
- La orden de trabajo pertenece siempre a un `Customer`.
- Para crear una orden nueva, el `Customer` debe estar activo.
- `InternalDoctor` es opcional y solo válido para `Customer.Type = Clinic`.
- Si se selecciona `InternalDoctor`, debe pertenecer al `Customer` indicado.
- Para órdenes nuevas, el `InternalDoctor` seleccionado debe estar activo.
- Las fechas operativas de órdenes usan `DateOnly` o equivalente sin hora.
- Una orden `Cancelled` representa cancelación operativa terminal en el MVP.
- Una orden `Cancelled` no se edita ni cambia a otro estado en el MVP.
- No hay delete físico de órdenes.
- `TotalAmount` puede existir en la orden y es requerido para registrar pagos.
- El saldo no se captura manualmente; se calcula desde `TotalAmount` y pagos vigentes.
- El estado operativo de la orden no debe mezclarse con el estado financiero.
- Los pagos se registran como movimientos financieros asociados a órdenes.
- Los pagos no se editan en el MVP.
- Los pagos no se eliminan físicamente.
- Los pagos se cancelan con motivo.
- Los pagos cancelados no cuentan para saldo.
- No se puede registrar pago si `TotalAmount` no está definido.
- No se puede registrar pago en una orden `Cancelled`.
- Se permite sobrepago y se marca como `Overpaid`.
- El dashboard no crea datos ni modifica estado.
- El dashboard respeta permisos por sección: operación, cobranza y clientes.
- Los cambios relevantes deben dejar trazabilidad.
- Un usuario inactivo no puede iniciar sesión.
- Un usuario bloqueado no puede iniciar sesión.
- Los permisos controlan acciones y rutas privadas; no se debe depender solo del nombre de rol.
- El Admin inicial recibe todos los permisos definidos en el catálogo.
- `PasswordHash` nunca se expone en respuestas ni documentación operativa.
- No se permiten passwords vacíos para crear o validar usuarios.

## Separación Operativa Y Financiera

El estado operativo describe avance del trabajo dental. El estado financiero se deriva del total, pagos vigentes, cancelaciones y saldo. Una orden puede estar entregada y tener saldo pendiente.

## Trazabilidad

Debe existir historial para cambios relevantes, especialmente cambios de estado, pagos, cancelaciones y ajustes futuros de inventario.

## Órdenes De Trabajo

- `OrderNumber` lo genera el sistema y es único.
- El formato MVP del folio es `OT-yyyyMMdd-XXXXXX`; puede cambiar antes de producción si se requiere folio secuencial.
- `Status` inicial es `Received`.
- Cambiar a `Cancelled` requiere nota.
- Todo cambio real de estado crea `WorkOrderStatusHistory`.
- Cambiar al mismo estado devuelve éxito sin duplicar historial.
- En edición, no se permite cambiar a un cliente inactivo distinto del cliente actual de la orden.
- Si el cliente nuevo no es `Clinic`, `InternalDoctorId` debe quedar vacío.

## Pagos Y Saldos

- `PaidAmount` es la suma de pagos no cancelados.
- `Balance` es `TotalAmount - PaidAmount`.
- Si `TotalAmount` es `null`, `Balance` es `null` y `PaymentStatus` es `TotalNotSet`.
- Si `TotalAmount = 0` y no hay pagos vigentes, `PaymentStatus` es `Paid`.
- `PaymentStatus` se calcula; no se captura manualmente.
- El sobrepago queda visible como "Saldo a favor / revisar".

## Dashboard

- El dashboard usa datos existentes de clientes, órdenes y pagos.
- El dashboard no crea clientes, órdenes, pagos ni datos demo.
- Las métricas financieras usan pagos no cancelados.
- El saldo pendiente se calcula desde `TotalAmount` y pagos vigentes.
- La sección operativa requiere `orders.view`.
- La sección financiera requiere `payments.view`.
- La sección de clientes requiere `customers.view`.
- Acceder al dashboard requiere `reports.view`.
- La respuesta puede contener secciones `null` cuando faltan permisos.
- La fecha "hoy" del MVP se calcula con `DateOnly.FromDateTime(IClock.UtcNow.UtcDateTime)`; la zona horaria formal de negocio queda pendiente.

## Criterios De Validación

- Ningún saldo se guarda como valor manual autoritativo.
- Los pagos no modifican directamente el total de la orden.
- Las reglas nuevas se documentan antes o junto con su implementación.
