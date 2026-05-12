# Estado del Proyecto

## Identificación

- Proyecto: Laboratorio Dental Tláhuac.
- Dominio principal: laboratoriodentaltlahuac.com.
- Fase actual: Fase 1 - MVP operativo.
- Etapa actual: Etapa 7 - QA funcional y demo.
- Estado: QA funcional ejecutada, documentación de demo preparada y pendiente demo con cliente.

## Objetivo Inmediato

Ejecutar demo con cliente, capturar feedback y cerrar alcance comercial de la siguiente fase.

## MVP Objetivo

Permitir operar órdenes de trabajo, clientes/doctores/clínicas y pagos sin depender del Excel para nuevos registros.

## Entregado En Etapas 1, 2, 2.1, 3, 4, 5, 6 Y 7

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
- Endpoint `GET /api/dashboard/summary` protegido por `reports.view`.
- Dashboard con secciones condicionadas por `orders.view`, `payments.view` y `customers.view`.
- Métricas básicas de clientes, órdenes, pagos y saldos calculados usando datos existentes.
- UI Angular real en `/app/dashboard` con métricas, conteo por estado, últimas órdenes, próximas entregas y últimos pagos.
- Pruebas backend de integración para autorización, secciones por permiso, total por cobrar, vencidos, fecha fija, límites de listas y `/health`.
- QA funcional del MVP administrativo ejecutada contra SQL Server local aislado.
- Corrección de conteos financieros del dashboard para excluir órdenes canceladas.
- Documentación de QA en `docs/08-qa`.
- Guion de demo, guía de datos de demo, checklist de aceptación y hallazgos conocidos.

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

1. Ejecutar demo con cliente.
2. Capturar feedback.
3. Cerrar alcance comercial.
4. Definir prioridad entre sitio web y repartidores/etiquetas.
5. Planear siguiente fase contratada.
