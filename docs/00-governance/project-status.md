# Estado del Proyecto

## Identificación

- Proyecto: Laboratorio Dental Tláhuac.
- Dominio principal: laboratoriodentaltlahuac.com.
- Fase actual: Fase 1 - MVP operativo.
- Etapa actual: Etapa 5 - Pagos, abonos y saldos.
- Estado: Etapa 5 implementada; Fase 1 en ejecución.

## Objetivo Inmediato

Revisar pagos y saldos calculados antes de implementar dashboard operativo básico.

## MVP Objetivo

Permitir operar órdenes de trabajo, clientes/doctores/clínicas y pagos sin depender del Excel para nuevos registros.

## Entregado En Etapas 1, 2, 2.1, 3, 4 Y 5

- Solución .NET 10 con proyectos `Api`, `Application`, `Domain` e `Infrastructure`.
- App Angular 21 en `src/LaboratorioTlahuac.Web`.
- Rutas públicas y privadas.
- Layout público y layout privado.
- Endpoint `GET /health`.
- Permisos base centralizados.
- ADR de autenticación por cookie segura HttpOnly para el MVP.
- Modelo inicial de seguridad: `User`, `Role`, `Permission`, `UserRole`, `RolePermission`.
- `LaboratorioTlahuacDbContext` con EF Core SQL Server.
- Migración inicial `InitialSecurityModel`.
- Seed Admin idempotente controlado por configuración.
- Endpoints `POST /api/auth/login`, `POST /api/auth/logout` y `GET /api/auth/me`.
- Cookie auth HttpOnly con respuestas `401`/`403` en `/api`.
- Protección CSRF/XSRF para requests mutables bajo `/api`.
- Endpoint `GET /api/auth/csrf` para emitir `XSRF-TOKEN`.
- Endpoint técnico `POST /api/security/csrf-check` solo en Development.
- Guards reales en Angular para sesión y permisos.
- Pruebas backend de integración para auth, cookie, CSRF, `/health` y permisos.
- `npm audit` revisado y corregido sin `--force`.
- Entidades `Customer`, `InternalDoctor` y `CustomerType`.
- DbSets y configuración EF Core para `Customers` e `InternalDoctors`.
- Migración `AddCustomersAndInternalDoctors`.
- Endpoints REST `/api/customers` protegidos por `customers.view`, `customers.create` y `customers.edit`.
- CRUD Angular en `/app/clientes`, `/app/clientes/nuevo`, `/app/clientes/:id` y `/app/clientes/:id/editar`.
- Gestión de doctores internos solo para clientes tipo clínica.
- Pruebas backend de integración para clientes, permisos, CSRF, soft deactivate y reglas de clínicas.
- Entidades `WorkOrder`, `WorkOrderStatus` y `WorkOrderStatusHistory`.
- DbSets y configuración EF Core para `WorkOrders` y `WorkOrderStatusHistory`.
- Migración `AddWorkOrders`.
- Endpoints REST `/api/work-orders` protegidos por `orders.view`, `orders.create`, `orders.edit` y `orders.changeStatus`.
- Generación de folio `OT-yyyyMMdd-XXXXXX` con índice único.
- UI Angular en `/app/ordenes`, `/app/ordenes/nueva`, `/app/ordenes/:id` y `/app/ordenes/:id/editar`.
- Cambio de estado operativo con historial y regla de cancelación con nota.
- Pruebas backend de integración para órdenes, permisos, CSRF, reglas de cliente/doctor interno, estados e historial.
- Entidad `Payment`, enum `PaymentMethod` y estado financiero calculado `PaymentStatus`.
- DbSet y configuración EF Core para `Payments`.
- Migración `AddPayments`.
- Endpoints REST de pagos protegidos por `payments.view`, `payments.create` y `payments.cancel`.
- Resumen financiero calculado por orden: total, pagado, saldo y estado financiero.
- UI Angular en `/app/pagos` y sección de pagos en `/app/ordenes/:id`.
- Cancelación de pagos con motivo, sin delete físico ni edición libre en MVP.
- Pruebas backend de integración para permisos, CSRF, creación, reglas de negocio, sobrepago, resumen, cancelación y listados.

## Principios De Trabajo

- Implementar cambios incrementales, seguros y no breaking.
- Mantener trazabilidad de reglas, decisiones y alcance.
- Actualizar documentación afectada después de cada implementación.
- Evitar migraciones o cambios masivos sin validación previa.

## Criterios De Validación

- La fase actual queda visible para cualquier integrante del proyecto.
- El objetivo del MVP está delimitado.
- Las decisiones importantes se documentan en ADRs.

## Próximos Pasos

- Revisar pagos y saldos.
- Implementar dashboard operativo básico.
- Preparar prueba con usuario.
- Preparar migración del Excel.
- Revisar UX y permisos.
