# Sistema Privado / MVP Administrativo

Fuente canónica funcional del sistema privado de Laboratorio Dental Tláhuac.

## Propósito

Permitir que el laboratorio opere registros nuevos sin depender del Excel para el flujo principal: clientes, órdenes de trabajo, pagos, saldos y dashboard.

## Ruta Base

- App privada: `/app`.
- Dashboard real: `/app/dashboard`.
- Entrada pública de login: `/login`.

## Módulos Actuales

- Autenticación y sesión privada.
- Clientes, doctores, clínicas y doctores internos.
- Órdenes de trabajo dental.
- Estados e historial de órdenes.
- Pagos, abonos, cancelación de pagos y saldos calculados.
- Dashboard operativo y financiero básico.
- Páginas iniciales de inventario, proveedores, usuarios y roles.

## Clientes

- El cliente puede ser `Doctor`, `Clinic` u `Other`.
- Las clínicas pueden tener doctores internos.
- Clientes y doctores internos se desactivan; no hay delete físico en el MVP.
- La autorización usa `customers.view`, `customers.create` y `customers.edit`.

## Órdenes

- La orden de trabajo es la entidad central.
- Cada orden pertenece a un cliente.
- Una orden puede tener doctor interno solo si el cliente es clínica.
- Estados principales: recibida, en proceso, pruebas, lista para entrega, entregada y cancelada.
- Una orden cancelada es terminal en el MVP.
- La autorización usa `orders.view`, `orders.create`, `orders.edit` y `orders.changeStatus`.

## Pagos

- Los pagos son movimientos asociados a órdenes.
- Los saldos se calculan desde `TotalAmount` y pagos no cancelados.
- No hay edición libre ni delete físico de pagos en el MVP.
- Los pagos se cancelan con motivo.
- La autorización usa `payments.view`, `payments.create` y `payments.cancel`.

## Dashboard

- Ruta: `/app/dashboard`.
- API: `GET /api/dashboard/summary`.
- Acceso: `reports.view`.
- Secciones internas condicionadas:
  - operación con `orders.view`;
  - cobranza con `payments.view`;
  - clientes con `customers.view`.

## Permisos

El sistema autoriza por permisos, no por nombre de rol. El rol Admin inicial recibe todos los permisos mediante seed.

Fuente técnica de auth: `docs/03-architecture/AUTH_FLOW.md`.

## Exclusiones Actuales

- Inventario automático.
- Proveedores funcionales completos.
- CFDI/facturación.
- Reportes avanzados.
- Exportación Excel/PDF avanzada.
- Migración completa del Excel.
- WhatsApp automatizado.
- App móvil nativa.

## QA

La QA funcional del MVP administrativo está documentada en:

- `docs/08-qa/mvp-qa-checklist.md`
- `docs/08-qa/mvp-acceptance-checklist.md`
- `docs/08-qa/known-issues.md`
