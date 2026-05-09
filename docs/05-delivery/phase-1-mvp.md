# Fase 1 - MVP Operativo

## Objetivo

Permitir que el laboratorio registre operación nueva en la plataforma sin depender del Excel.

## Etapa 1 - Arquitectura Base

Estado: implementada.

Incluye:

- Solución .NET 10.
- Proyectos Api, Application, Domain e Infrastructure.
- App Angular 21.
- Rutas públicas y privadas placeholder.
- Layout público y layout privado.
- Health check.
- Permisos base.

Todavía faltan:

- Clientes.
- Órdenes.
- Pagos.
- Dashboard operativo.

## Etapa 2 - Autenticación Y Autorización

Estado: implementada.

Incluye:

- Modelo inicial de usuarios, roles y permisos.
- DbContext EF Core SQL Server.
- Migración inicial `InitialSecurityModel`.
- Seed Admin idempotente mediante configuración segura.
- Login/logout/me con cookie HttpOnly.
- Autorización por claims de permisos.
- Guards reales en Angular.
- Pruebas backend de integración.

Todavía faltan:

- Órdenes de trabajo.
- Pagos y saldos.
- Dashboard operativo básico.

## Etapa 2.1 - Hardening De Seguridad

Estado: implementada.

Incluye:

- Protección CSRF/XSRF para métodos mutables bajo `/api`.
- Endpoint `GET /api/auth/csrf`.
- Protección de login y logout con `X-XSRF-TOKEN`.
- Configuración Angular XSRF.
- Renovación de token XSRF después de login.
- Pruebas backend para cookies, CSRF, auth y no redirecciones 302.
- Revisión y corrección de `npm audit` sin `--force`.

Todavía faltan:

- Runner de pruebas frontend no interactivo.
- Pagos y saldos.
- Dashboard operativo básico.

## Etapa 3 - Clientes / Doctores / Clínicas

Estado: implementada.

Incluye:

- Entidades `Customer`, `InternalDoctor` y `CustomerType`.
- Migración `AddCustomersAndInternalDoctors`.
- Endpoints `/api/customers` protegidos por permisos.
- Protección XSRF en operaciones mutables.
- Listado, detalle, alta y edición Angular de clientes.
- Gestión de doctores internos para clientes tipo clínica.
- Desactivación lógica sin delete físico.
- Pruebas backend de integración.

Todavía faltan:

- Runner de pruebas frontend no interactivo.
- Órdenes de trabajo.
- Pagos y saldos.
- Dashboard operativo básico.

## Etapa 4 - Órdenes De Trabajo Dental

Estado: implementada.

Incluye:

- Entidades `WorkOrder`, `WorkOrderStatus` y `WorkOrderStatusHistory`.
- Migración `AddWorkOrders`.
- Endpoints `/api/work-orders` protegidos por permisos.
- Protección XSRF en operaciones mutables.
- Generación de `OrderNumber` único con formato MVP `OT-yyyyMMdd-XXXXXX`.
- Listado, detalle, alta y edición Angular de órdenes.
- Cambio de estado operativo con historial.
- Cancelación con nota y bloqueo de edición de órdenes canceladas.
- Pruebas backend de integración.

Todavía faltan:

- Runner de pruebas frontend no interactivo.
- Dashboard operativo básico.
- Migración del Excel.

## Etapa 5 - Pagos, Abonos Y Saldos

Estado: implementada.

Incluye:

- Entidad `Payment`.
- Enum `PaymentMethod`.
- Estado financiero calculado `PaymentStatus`.
- Migración `AddPayments`.
- Endpoints `/api/work-orders/{workOrderId}/payments` y `/api/payments` protegidos por permisos.
- Protección XSRF en registro y cancelación de pagos.
- Registro de pagos por orden.
- Cancelación de pagos con motivo.
- Cálculo de `PaidAmount`, `Balance` y `PaymentStatus`.
- Listado Angular en `/app/pagos`.
- Sección de pagos dentro de `/app/ordenes/:id`.
- Pruebas backend de integración.

Todavía faltan:

- Runner de pruebas frontend no interactivo.
- Dashboard operativo básico.
- Migración del Excel.
- Inventario y proveedores para fase posterior.

## Backlog MVP

- [x] Crear solución backend.
- [x] Crear app frontend.
- [x] Implementar login.
- [x] Implementar clientes.
- [x] Implementar órdenes.
- [x] Implementar pagos.
- [ ] Implementar dashboard.
- [x] Definir permisos base.
- [x] Implementar modelo de roles/permisos.
- [x] Implementar hardening CSRF/XSRF.
- [ ] Realizar deploy inicial.

## Criterios De Salida

- Admin puede iniciar sesión.
- Admin puede crear clientes y órdenes.
- Admin puede crear y cancelar pagos.
- Los saldos se calculan automáticamente desde pagos no cancelados.
- El dashboard muestra información básica operativa.
- El sitio público básico está disponible.
- Documentación y changelog quedan actualizados.

## Fuera De Alcance

- Inventario automático.
- Proveedores.
- Facturación.
- Reportes avanzados.
- Migración perfecta del histórico.
- Automatizaciones WhatsApp.
