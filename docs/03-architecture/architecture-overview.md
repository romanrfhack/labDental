# Visión General De Arquitectura

## Arquitectura Implementada En Fase 1

- Frontend: Angular 21 en `src/LaboratorioTlahuac.Web`.
- Backend: .NET 10 ASP.NET Core Web API en `src/LaboratorioTlahuac.Api`.
- Comunicación: API REST.
- Base de datos objetivo: SQL Server.
- ORM objetivo: Entity Framework Core.
- Autenticación MVP: cookie segura HttpOnly.
- Autorización: permisos granulares.
- Protección CSRF/XSRF para endpoints mutables bajo `/api`.
- Dominio: laboratoriodentaltlahuac.com.
- Sitio público y app privada: mismo dominio inicialmente.

## Estructura Real

```text
src/
  LaboratorioTlahuac.Api/
  LaboratorioTlahuac.Application/
  LaboratorioTlahuac.Domain/
  LaboratorioTlahuac.Infrastructure/
  LaboratorioTlahuac.Web/
tests/
  LaboratorioTlahuac.Api.Tests/
  LaboratorioTlahuac.Application.Tests/
  LaboratorioTlahuac.Domain.Tests/
```

## Principios

- Arquitectura modular y preparada para crecimiento incremental.
- Separación clara entre dominio, casos de uso, infraestructura y API.
- Autorización por permisos desde el inicio.
- Cambios de esquema y contratos API deben ser compatibles cuando sea posible.
- La documentación se actualiza con cada cambio implementado.

## Vista Conceptual

Usuario -> Angular público/privado -> API REST .NET -> Base de datos relacional.

## Contratos Iniciales

- `GET /health` responde el estado de la API.
- `/` sirve sitio público.
- `/login` permite autenticación real contra `/api/auth/login`.
- `/app/*` existe con rutas protegidas por sesión y permisos.
- `POST /api/auth/logout` cierra sesión.
- `GET /api/auth/me` devuelve usuario, roles y permisos.
- `GET /api/auth/csrf` emite token XSRF.
- `/api/customers` expone CRUD de clientes y doctores internos protegido por permisos.
- `/api/work-orders` expone órdenes de trabajo protegido por permisos.
- `/api/payments` y `/api/work-orders/{id}/payments` exponen pagos y saldos protegidos por permisos financieros.
- `/api/dashboard/summary` expone dashboard básico protegido por `reports.view` y secciones condicionadas por permisos.
- Métodos mutables bajo `/api` requieren `X-XSRF-TOKEN`.

## Criterios De Validación

- `/` sirve sitio público.
- `/login` permite acceso privado.
- `/app/*` queda protegido.
- El backend expone API versionable y protegida.

## Pendientes

- Definir hosting y estrategia de despliegue.
- Preparar QA manual del MVP con SQL Server local.
- Preparar estrategia de migración del Excel.
