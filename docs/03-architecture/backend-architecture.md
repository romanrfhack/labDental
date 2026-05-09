# Arquitectura Backend

## Proyectos Reales

- `src/LaboratorioTlahuac.Api`: ASP.NET Core Web API .NET 10.
- `src/LaboratorioTlahuac.Application`: casos de uso y contratos.
- `src/LaboratorioTlahuac.Domain`: dominio y permisos base.
- `src/LaboratorioTlahuac.Infrastructure`: persistencia futura, seguridad e integraciones.

## Referencias

- Api referencia Application e Infrastructure.
- Application referencia Domain.
- Infrastructure referencia Application y Domain.
- Domain no referencia otros proyectos del sistema.

## Capas

### Api

Expone endpoints HTTP, autenticación, autorización, antiforgery y contratos de respuesta. Incluye `GET /health`, endpoints de auth, configuración de cookie, XSRF y policies de permisos.

### Application

Contiene casos de uso futuros, orquestación y contratos con infraestructura. Incluye `IPermissionChecker` e `IAuthSessionService` como contratos de seguridad.

### Domain

Contiene entidades, reglas de negocio, invariantes y conceptos centrales. Centraliza `Permissions`, `PermissionClaimTypes` y las entidades iniciales de seguridad.

### Infrastructure

Contiene persistencia, integraciones externas y adaptadores. Incluye EF Core, `LaboratorioTlahuacDbContext`, `ClaimsPermissionChecker`, `AuthSessionService` y `SecuritySeeder`.

## Reglas De Dependencia

- Domain no depende de Infrastructure.
- Domain no depende de Api.
- Application puede depender de Domain.
- Infrastructure implementa contratos definidos por Application.
- Api consume Application.

## Persistencia

Entity Framework Core SQL Server está configurado en Infrastructure.

DbContext:

- `LaboratorioTlahuacDbContext`

Entidades persistidas en esquema `Security`:

- `Users`
- `Roles`
- `Permissions`
- `UserRoles`
- `RolePermissions`

Índices únicos:

- `Users.NormalizedEmail`
- `Roles.NormalizedName`
- `Permissions.Key`

Relaciones many-to-many explícitas:

- `Users` a `Roles` mediante `UserRoles`.
- `Roles` a `Permissions` mediante `RolePermissions`.

Migración inicial:

- `InitialSecurityModel`

No hay auto-migración al iniciar la aplicación.

## Servicios De Seguridad

- `AuthSessionService`: valida credenciales, usuarios activos/bloqueados, rehidrata usuario actual y emite el resultado para claims.
- `ClaimsPermissionChecker`: valida claims de permiso.
- `SecuritySeeder`: crea permisos, rol Admin, permisos del Admin y usuario Admin inicial si existe configuración segura.
- `IAntiforgery`: valida tokens para requests mutables bajo `/api`.

El password se valida con `Microsoft.AspNetCore.Identity.PasswordHasher<User>`. No se guarda password plano ni se devuelve `PasswordHash` en respuestas.

## Antiforgery

Configuración:

- Header: `X-XSRF-TOKEN`.
- Cookie pública de token de request: `XSRF-TOKEN`, no HttpOnly.
- Cookie interna antiforgery: HttpOnly.
- `SameSite=Lax`.
- `Secure` obligatorio en Production.

La validación se aplica de forma centralizada en middleware para métodos mutables bajo `/api`: `POST`, `PUT`, `PATCH` y `DELETE`.

Exclusiones documentadas:

- `GET /health`.
- Métodos seguros bajo `/api`.
- `GET /api/auth/csrf`, que emite/renueva token.
- `GET /api/auth/me`, que solo consulta sesión.

Endpoints técnicos de seguridad solo en Development:

- `GET /api/security/permissions-check`.
- `POST /api/security/csrf-check`.

## Criterios De Validación

- Las reglas de saldo y estados no viven en controladores.
- La persistencia puede cambiar sin reescribir el dominio.
- Los casos de uso son testeables sin servidor HTTP.

## Próximos Pasos

- Revisar hardening de seguridad.
- Implementar CRUD de clientes/doctores/clínicas.
