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
- El saldo no se captura manualmente; se calcula.
- El estado operativo de la orden no debe mezclarse con el estado financiero.
- Los pagos se registran como movimientos.
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

## Criterios De Validación

- Ningún saldo se guarda como valor manual autoritativo.
- Los pagos no modifican directamente el total de la orden.
- Las reglas nuevas se documentan antes o junto con su implementación.
