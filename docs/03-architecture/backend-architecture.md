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

Expone endpoints HTTP, autenticación, autorización, antiforgery y contratos de respuesta. Incluye `GET /health`, endpoints de auth, endpoints de clientes, endpoints de órdenes, configuración de cookie, XSRF y policies de permisos.

### Application

Contiene casos de uso, orquestación y contratos con infraestructura. Incluye `IPermissionChecker`, `IAuthSessionService`, `ICurrentUser`, `IClock`, `ICustomerService` e `IWorkOrderService`.

### Domain

Contiene entidades, reglas de negocio, invariantes y conceptos centrales. Centraliza `Permissions`, `PermissionClaimTypes`, entidades iniciales de seguridad, entidades de clientes y entidades de órdenes.

### Infrastructure

Contiene persistencia, integraciones externas y adaptadores. Incluye EF Core, `LaboratorioTlahuacDbContext`, `CustomerService`, `WorkOrderService`, `GuidWorkOrderNumberGenerator`, `ClaimsPermissionChecker`, `AuthSessionService`, `SystemClock` y `SecuritySeeder`.

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

Entidades operativas persistidas:

- `Customers`
- `InternalDoctors`
- `WorkOrders`
- `WorkOrderStatusHistory`

Índices únicos:

- `Users.NormalizedEmail`
- `Roles.NormalizedName`
- `Permissions.Key`

Relaciones many-to-many explícitas:

- `Users` a `Roles` mediante `UserRoles`.
- `Roles` a `Permissions` mediante `RolePermissions`.

Relaciones operativas:

- `Customers` 1:N `InternalDoctors`.
- `Customers` 1:N `WorkOrders`.
- `InternalDoctors` 1:N `WorkOrders` opcional.
- `WorkOrders` 1:N `WorkOrderStatusHistory`.
- `WorkOrders` y `WorkOrderStatusHistory` referencian `Security.Users` para auditoría.

Migraciones:

- `InitialSecurityModel`
- `AddCustomersAndInternalDoctors`
- `AddWorkOrders`

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

## Clientes

Endpoints principales:

- `GET /api/customers`
- `GET /api/customers/{id}`
- `POST /api/customers`
- `PUT /api/customers/{id}`
- `PATCH /api/customers/{id}/status`
- `GET /api/customers/{customerId}/internal-doctors`
- `POST /api/customers/{customerId}/internal-doctors`
- `PUT /api/customers/{customerId}/internal-doctors/{doctorId}`
- `PATCH /api/customers/{customerId}/internal-doctors/{doctorId}/status`

Implementación:

- `CustomerEndpoints` mantiene endpoints HTTP delgados.
- `ICustomerService` define casos de uso y contratos en Application.
- `CustomerService` implementa validación, consultas EF, auditoría y reglas de negocio en Infrastructure.
- `LaboratorioTlahuacDbContext` expone `Customers` e `InternalDoctors`.
- Los permisos usados son `customers.view`, `customers.create` y `customers.edit`.
- Los métodos mutables quedan cubiertos por el middleware XSRF centralizado.

## Órdenes De Trabajo

Endpoints principales:

- `GET /api/work-orders`
- `GET /api/work-orders/{id}`
- `GET /api/work-orders/statuses`
- `POST /api/work-orders`
- `PUT /api/work-orders/{id}`
- `PATCH /api/work-orders/{id}/status`

Implementación:

- `WorkOrderEndpoints` mantiene endpoints HTTP delgados.
- `IWorkOrderService` define casos de uso y contratos en Application.
- `WorkOrderService` implementa validación, consultas EF, auditoría, reglas Customer/InternalDoctor, generación de folio e historial.
- `GuidWorkOrderNumberGenerator` genera folio MVP `OT-yyyyMMdd-XXXXXX`.
- `LaboratorioTlahuacDbContext` expone `WorkOrders` y `WorkOrderStatusHistory`.
- Los permisos usados son `orders.view`, `orders.create`, `orders.edit` y `orders.changeStatus`.
- `orders.delete` queda reservado; no hay delete físico.
- Los métodos mutables quedan cubiertos por el middleware XSRF centralizado.

## Criterios De Validación

- Las reglas de saldo y estados no viven en controladores.
- La persistencia puede cambiar sin reescribir el dominio.
- Los casos de uso son testeables sin servidor HTTP.

## Próximos Pasos

- Revisar órdenes de trabajo.
- Implementar pagos, abonos y saldos.
