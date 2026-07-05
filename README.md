# Laboratorio Dental Tláhuac

Plataforma web para Laboratorio Dental Tláhuac. El repositorio contiene un sistema administrativo privado para operar clientes, órdenes, pagos y dashboard, y el frente nuevo del sitio público institucional para `laboratoriodentaltlahuac.com`.

## Estado Actual

- Sistema privado / MVP administrativo: avanzado, con QA funcional, demo documentada, pase manual/técnico privado Fase 2.4 ejecutado, Fase 2.5 cerrada como pase visual humano privado completado, Fase 2.6 implementada para usuario QA limitado Development-only, Fase 3.2 implementada para impresión MVP de etiquetas desde órdenes existentes, Fase 3.3 implementada para administración MVP de usuarios/roles, Fase 3.4.0 documentada como análisis técnico previo de entregas/repartidor mobile-first, Fase 3.4.1 implementada para backend delivery MVP + permisos, Fase 3.4.2 implementada para UI admin de entregas desde órdenes, Fase 3.4.3 implementada para UI mobile-first de repartidor bajo `/app/entregas` y Fase 3.4.3.1 implementada para redirect post-login por permisos y reintento de `FailedDelivery`.
- Sitio público institucional: primera versión mobile-first implementada en `/`, `/servicios`, `/catalogo` y `/contacto`; Fase 1.6 validada visualmente por el responsable del proyecto; contenido final del cliente pendiente.
- Ambiente DEV: publicado en `https://dev.laboratoriodentaltlahuac.com` desde rama `dev` y validado como baseline UAT inicial en Fase 3.0 para sitio público, `/login`, login QA, `/app/dashboard` autenticado y redirección sin sesión a `/login`.
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
- Usuario QA limitado local: `SecuritySeed:LimitedQaUser:RunOnStartup`, `SecuritySeed:LimitedQaUser:Permissions` y `LT_QA_LIMITED_EMAIL` / `LT_QA_LIMITED_PASSWORD` / `LT_QA_LIMITED_FULL_NAME`.
- El seed QA limitado solo corre en `Development`, esta desactivado por default y no debe guardar ni imprimir contrasenas.
- Baseline de seguridad Development: `SecuritySeed:EnsureBaselineOnStartup=true` asegura permisos existentes, sincroniza permisos faltantes al rol `Admin` existente y mantiene rol `Repartidor` con permisos mínimos de entregas (`deliveries.view` y `deliveries.complete`).

## Rutas Principales

- Sitio público: `/`, `/catalogo`, `/servicios`, `/contacto`.
- Login: `/login`.
- Aplicación privada: `/app`.
- Dashboard privado real: `/app/dashboard`.
- Entregas privadas de repartidor: `/app/entregas` y `/app/entregas/:id`.
- Etiquetas privadas de órdenes: `/app/ordenes/:id/etiqueta-trabajo` y `/app/ordenes/:id/etiqueta-entrega`.
- Admin privado: `/app/admin/usuarios` y `/app/admin/roles`.
- API: `/api/auth`, `/api/customers`, `/api/work-orders`, `/api/work-orders/{id}/delivery`, `/api/deliveries`, `/api/deliveries/{id}/retry`, `/api/payments`, `/api/dashboard/summary`, `/api/admin/users`, `/api/admin/roles`.
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
- Validación DEV: [docs/05-delivery/dev-deployment-validation.md](docs/05-delivery/dev-deployment-validation.md).
- QA responsive: [docs/08-qa/RESPONSIVE_CHECKLIST.md](docs/08-qa/RESPONSIVE_CHECKLIST.md).
- QA MVP administrativo: [docs/08-qa/mvp-qa-checklist.md](docs/08-qa/mvp-qa-checklist.md).
- QA impresión de etiquetas: [docs/08-qa/label-printing-qa.md](docs/08-qa/label-printing-qa.md).
- Plan QA usuario limitado: [docs/08-qa/limited-user-qa-plan.md](docs/08-qa/limited-user-qa-plan.md).
- QA usuarios y roles: [docs/08-qa/users-roles-qa.md](docs/08-qa/users-roles-qa.md).
- QA API entregas: [docs/08-qa/delivery-api-qa.md](docs/08-qa/delivery-api-qa.md).
- QA UI admin entregas: [docs/08-qa/delivery-admin-ui-qa.md](docs/08-qa/delivery-admin-ui-qa.md).
- QA UI repartidor: [docs/08-qa/driver-mobile-qa.md](docs/08-qa/driver-mobile-qa.md).
- Diseño MVP entregas/repartidor: [docs/01-product/delivery-mvp-design.md](docs/01-product/delivery-mvp-design.md).
- Documentación comercial: [docs/09-commercial/](docs/09-commercial/).

## Próximos Pasos

1. Ejecutar QA DEV de redirect post-login por permisos y reintento de entrega fallida con Admin y usuario `Repartidor`.
2. Después continuar con pulido mobile-first del flujo de reparto.
3. Validar Fase 3.2 en DEV con impresora térmica real y ajustar escala/márgenes del navegador si hace falta.
