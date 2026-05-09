# Changelog

Todos los cambios relevantes del proyecto deben registrarse aquí. No se deben inventar funcionalidades implementadas.

## 2026-05-09 - Fase 1 Etapa 4

- Se implementan órdenes de trabajo dental.
- Se agregan entidades `WorkOrder`, `WorkOrderStatus` y `WorkOrderStatusHistory`.
- Se agregan endpoints `/api/work-orders`.
- Se agregan pantallas Angular de órdenes.
- Se agregan pruebas backend de integración para permisos, CSRF, creación, edición, cambio de estado e historial.
- Se agrega migración `AddWorkOrders`.
- No se implementan pagos, abonos ni saldos en esta etapa.

## 2026-05-09 - Fase 1 Etapa 3

- Se implementa CRUD de clientes/doctores/clínicas.
- Se agregan entidades `Customer`, `InternalDoctor` y enum `CustomerType`.
- Se agregan endpoints `/api/customers`.
- Se agregan pantallas Angular de clientes.
- Se agregan pruebas backend de integración para clientes, permisos, CSRF y reglas de clínicas.
- Se agrega migración `AddCustomersAndInternalDoctors`.

## 2026-05-08 - Fase 1 Etapa 2.1

- Se agrega protección CSRF/XSRF para métodos mutables bajo `/api`.
- Se agrega endpoint `GET /api/auth/csrf` para emitir cookie `XSRF-TOKEN`.
- Se protege `POST /api/auth/login` y `POST /api/auth/logout` con header `X-XSRF-TOKEN`.
- Se agrega endpoint técnico `POST /api/security/csrf-check` solo en Development para validar antiforgery.
- Se validan cookies de auth y antiforgery en pruebas de integración.
- Se actualiza Angular para pedir token XSRF antes de login/logout y enviar `X-XSRF-TOKEN`.
- Se revisa `npm audit` y se corrigen vulnerabilidades transitivas con `npm audit fix` sin `--force`.

## 2026-05-08 - Fase 1 Etapa 2

- Se implementa autenticación con cookie HttpOnly.
- Se agregan entidades de seguridad: `User`, `Role`, `Permission`, `UserRole` y `RolePermission`.
- Se agrega `LaboratorioTlahuacDbContext` con EF Core SQL Server y migración inicial de seguridad.
- Se agrega seed Admin idempotente controlado por configuración.
- Se agregan endpoints `POST /api/auth/login`, `POST /api/auth/logout` y `GET /api/auth/me`.
- Se agrega endpoint técnico `GET /api/security/permissions-check` solo en Development para validar autorización por permisos.
- Se reemplazan guards placeholder por `AuthGuard` y `PermissionGuard` funcionales en Angular.
- Se agrega login real, logout y sesión en memoria en Angular sin `localStorage` ni `sessionStorage`.
- Se agregan pruebas backend de integración para health, login, cookie, `/me` y permisos.

## 2026-05-05 - Fase 1 Etapa 1

- Se crea arquitectura base backend/frontend.
- Se agregan proyectos .NET 10: Api, Application, Domain e Infrastructure.
- Se agrega app Angular 21.
- Se agregan rutas iniciales públicas y privadas placeholder.
- Se agrega endpoint `GET /health`.
- Se agregan permisos base centralizados.

## 2026-05-05 - Fase 0

- Se crea documentación inicial del proyecto.
