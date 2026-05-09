# Estado del Proyecto

## Identificación

- Proyecto: Laboratorio Dental Tláhuac.
- Dominio principal: laboratoriodentaltlahuac.com.
- Fase actual: Fase 1 - MVP operativo.
- Etapa actual: Etapa 3 - Clientes / doctores / clínicas.
- Estado: Etapa 3 implementada; Fase 1 en ejecución.

## Objetivo Inmediato

Revisar el CRUD de clientes antes de iniciar órdenes de trabajo.

## MVP Objetivo

Permitir operar órdenes de trabajo, clientes/doctores/clínicas y pagos sin depender del Excel para nuevos registros.

## Entregado En Etapas 1, 2, 2.1 Y 3

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

- Revisar CRUD de clientes.
- Implementar órdenes de trabajo.
