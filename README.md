# Laboratorio Dental Tláhuac

Plataforma web para Laboratorio Dental Tláhuac. El repositorio contiene un sistema administrativo privado para operar clientes, órdenes, pagos y dashboard, y el frente nuevo del sitio público institucional para `laboratoriodentaltlahuac.com`.

## Estado Actual

- Sistema privado / MVP administrativo: avanzado, con QA funcional, demo documentada, pase manual/técnico privado Fase 2.4 ejecutado y Fase 2.5 cerrada como pase visual humano privado completado.
- Sitio público institucional: primera versión mobile-first implementada en `/`, `/servicios`, `/catalogo` y `/contacto`; Fase 1.6 validada visualmente por el responsable del proyecto; contenido final del cliente pendiente.
- Deploy productivo: pendiente de plataforma, DNS, HTTPS, variables y base productiva.

La Fase 1 / Etapa 7 corresponde al MVP administrativo. La Fase 0/Fase 1 del sitio público corresponde a un frente separado.

## Stack

- Backend: .NET 10, ASP.NET Core Web API.
- Frontend: Angular 21 con routing y SCSS.
- Persistencia objetivo: SQL Server con Entity Framework Core.
- Auth: cookie segura HttpOnly con CSRF/XSRF.
- Autorización: permisos granulares por claims.
- Pruebas backend: xUnit y `Microsoft.AspNetCore.Mvc.Testing`.

## Estructura

```text
docs/
src/
  LaboratorioTlahuac.Api/
  LaboratorioTlahuac.Application/
  LaboratorioTlahuac.Domain/
  LaboratorioTlahuac.Infrastructure/
  LaboratorioTlahuac.Web/
tests/
```

## Comandos Principales

Backend desde la raíz:

```bash
dotnet restore
dotnet build
dotnet test
dotnet run --project src/LaboratorioTlahuac.Api/LaboratorioTlahuac.Api.csproj
```

Frontend desde `src/LaboratorioTlahuac.Web`:

```bash
npm ci
npm start
npm run build
```

Health check local:

```bash
curl http://localhost:5277/health
```

No ejecutar migraciones contra producción sin plan de despliegue y respaldo.

## Configuración Relevante

- Dashboard operativo: `Dashboard:BusinessTimeZone`.
- Valor default: `America/Mexico_City`.
- Esta zona define el "hoy" de negocio para `dueToday`, `overdue` y `upcomingDue`; `generatedAtUtc` sigue siendo UTC.
- El ID canónico es IANA; el backend contempla `Central Standard Time (Mexico)` como equivalente Windows.

## Rutas Principales

- Sitio público: `/`, `/catalogo`, `/servicios`, `/contacto`.
- Login: `/login`.
- Aplicación privada: `/app`.
- Dashboard privado real: `/app/dashboard`.
- API: `/api/auth`, `/api/customers`, `/api/work-orders`, `/api/payments`, `/api/dashboard/summary`.
- Health: `/health`.

## Documentación Canónica

- Índice general: [docs/README.md](docs/README.md).
- Estado del proyecto: [docs/PROJECT_STATUS.md](docs/PROJECT_STATUS.md).
- Roadmap global: [docs/ROADMAP.md](docs/ROADMAP.md).
- Bitácora: [docs/IMPLEMENTATION_LOG.md](docs/IMPLEMENTATION_LOG.md).
- Arquitectura: [docs/03-architecture/ARCHITECTURE.md](docs/03-architecture/ARCHITECTURE.md).
- Autenticación y autorización: [docs/03-architecture/AUTH_FLOW.md](docs/03-architecture/AUTH_FLOW.md).
- Sitio público: [docs/01-product/public-website.md](docs/01-product/public-website.md).
- Sistema privado: [docs/01-product/internal-system.md](docs/01-product/internal-system.md).
- Deploy: [docs/05-delivery/DEPLOYMENT.md](docs/05-delivery/DEPLOYMENT.md).
- QA responsive: [docs/08-qa/RESPONSIVE_CHECKLIST.md](docs/08-qa/RESPONSIVE_CHECKLIST.md).
- QA MVP administrativo: [docs/08-qa/mvp-qa-checklist.md](docs/08-qa/mvp-qa-checklist.md).
- Plan QA usuario limitado: [docs/08-qa/limited-user-qa-plan.md](docs/08-qa/limited-user-qa-plan.md).
- Documentación comercial: [docs/09-commercial/](docs/09-commercial/).

## Próximos Pasos

1. Implementar, si se autoriza tocar backend mínimo, el usuario QA limitado Development-only documentado en `docs/08-qa/limited-user-qa-plan.md`.
2. Confirmar contenido real pendiente del cliente: dirección, horarios, WhatsApp, precios y materiales visuales.
3. Definir plataforma de despliegue y pendientes de DNS para `laboratoriodentaltlahuac.com`.
