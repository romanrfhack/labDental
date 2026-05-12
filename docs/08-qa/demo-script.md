# Guion De Demo MVP Administrativo

Objetivo: mostrar que el laboratorio ya puede operar clientes, órdenes, pagos, saldos y dashboard sin depender del Excel para registros nuevos.

## 1. Inicio De Sesión

1. Abrir `/login`.
2. Iniciar sesión como usuario Admin.
3. Confirmar entrada a `/app/dashboard`.

Mensaje clave: el sistema usa sesión privada con cookie HttpOnly, permisos por rol y protección XSRF para cambios.

## 2. Dashboard

1. Mostrar métricas de clientes activos.
2. Mostrar métricas de órdenes activas, entregadas, canceladas, vencidas y próximas.
3. Mostrar cobranza: total por cobrar, órdenes con saldo, pagadas, parciales y sin pago.
4. Mostrar últimas órdenes, próximas entregas y últimos pagos.

Mensaje clave: el dashboard da una vista rápida de operación y cobranza sin generar reportes avanzados todavía.

## 3. Clientes

1. Entrar a `/app/clientes`.
2. Mostrar doctores individuales.
3. Mostrar una clínica.
4. Entrar al detalle de la clínica.
5. Mostrar doctores internos asociados.
6. Crear o editar un doctor interno si el cliente desea ver captura.

Mensaje clave: el modelo distingue doctores independientes y clínicas con doctores internos.

## 4. Órdenes

1. Entrar a `/app/ordenes`.
2. Crear una orden para un doctor individual.
3. Crear una orden para una clínica seleccionando doctor interno.
4. Mostrar detalle de orden.
5. Cambiar estado operativo.
6. Mostrar historial de cambios.
7. Cancelar una orden con nota, si se decide mostrar la regla.

Mensaje clave: la orden concentra el trabajo operativo y deja trazabilidad de estados.

## 5. Pagos Y Saldos

1. Abrir una orden con total capturado.
2. Registrar un pago parcial.
3. Mostrar saldo pendiente y estado `Pago parcial`.
4. Registrar un segundo pago hasta cubrir total.
5. Mostrar estado `Pagada`.
6. Mostrar un caso de sobrepago con etiqueta `Saldo a favor / revisar`.
7. Cancelar un pago con motivo.
8. Confirmar que el saldo se recalcula.

Mensaje clave: los saldos se calculan desde pagos vigentes; no se editan manualmente.

## 6. Listado De Pagos

1. Entrar a `/app/pagos`.
2. Mostrar pagos recientes.
3. Buscar por cliente, paciente, orden o referencia.
4. Explicar que pagos cancelados no aparecen por defecto.

Mensaje clave: cobranza puede revisar abonos sin entrar orden por orden.

## 7. Cierre

Beneficios a comunicar:

- Menos dependencia del Excel para registros nuevos.
- Mejor trazabilidad de clientes, órdenes, pagos y estados.
- Saldos calculados automáticamente.
- Dashboard operativo para seguimiento diario.
- Base lista para decidir siguiente fase: sitio web público o módulos operativos como repartidores/etiquetas.

No prometer en esta demo:

- Inventario.
- Proveedores.
- Repartidores.
- Etiquetas.
- Migración automática del Excel.
- CFDI o facturación.
- Reportes avanzados.
