# Guía Manual De Datos De Demo

Crear estos datos manualmente en una base local o de demo. No deben cargarse automáticamente en producción.

## Clientes

Doctores individuales:

- Dr. Ana Pérez.
- Dr. Bruno López.

Clínica:

- Clínica Sonrisa Tláhuac.

Doctores internos de la clínica:

- Dra. Carmen Ruiz.
- Dr. Diego Ramos.

## Órdenes Sugeridas

1. Orden para Dr. Ana Pérez.
   - Paciente: Paciente Uno.
   - Trabajo: Corona zirconia.
   - Estado: En proceso.
   - Total: sin definir.

2. Orden para Clínica Sonrisa Tláhuac con Dra. Carmen Ruiz.
   - Paciente: Paciente Dos.
   - Trabajo: Prótesis parcial.
   - Estado: Recibida o En proceso.
   - Total: 500.
   - Pago parcial: 200.
   - Saldo esperado: 300.

3. Orden para Dr. Bruno López.
   - Paciente: Paciente Tres.
   - Trabajo: Guarda oclusal.
   - Total: 1000.
   - Pagos: 400 y 600.
   - Estado financiero esperado: Pagada.
   - Estado operativo sugerido: Entregada.

4. Orden vencida para Dr. Ana Pérez.
   - Paciente: Paciente Cuatro.
   - Trabajo: Incrustación.
   - Fecha de entrega anterior a hoy.
   - Total: 800.
   - Pago de prueba: 900.
   - Estado esperado antes de cancelar pago: Saldo a favor / revisar.
   - Cancelar el pago con motivo.
   - Estado esperado después de cancelar pago: Sin pago, saldo 800.

5. Orden cancelada para Clínica Sonrisa Tláhuac con Dr. Diego Ramos.
   - Paciente: Paciente Cinco.
   - Trabajo: Carilla.
   - Total: 300.
   - Cambiar estado a Cancelada con nota.

## Pagos Sugeridos

- Pago parcial de 200 para la orden de 500.
- Pago de 400 para la orden de 1000.
- Pago de 600 para liquidar la orden de 1000.
- Pago de 900 para probar sobrepago y cancelación.

## Resultado Esperado Para Demo

- 2 doctores individuales activos.
- 1 clínica activa.
- 2 doctores internos activos.
- 5 órdenes con estados distintos.
- 3 pagos vigentes.
- 1 pago cancelado.
- 1 orden pagada.
- 1 orden con saldo pendiente.
- 1 orden vencida.
- 1 orden cancelada.

## Reglas De Uso

- Capturar estos datos solo en ambiente local, QA o demo.
- No usar información real de pacientes.
- No dejar credenciales demo en el repositorio.
- No activar seed demo automático por defecto.
