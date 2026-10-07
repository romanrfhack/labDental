# Laboratorio Dental Tláhuac

Plataforma web para Laboratorio Dental Tláhuac. El repositorio contiene un sistema administrativo privado para operar clientes, órdenes, pagos y dashboard, y el frente nuevo del sitio público institucional para `laboratoriodentaltlahuac.com`.

## Estado Actual

- MVP administrativo en DEV: clientes/doctores/clínicas, órdenes, pagos/saldos, dashboard, usuarios/roles, etiquetas, entregas y catálogo con imágenes persistentes.
- Sitio público aprobado en DEV: `/`, `/servicios`, `/catalogo`, `/contacto`.
- SEC-PERM-1: candidata única `codex/sec-perm-1-consolidated-20261006`, pendiente de revisión/integración y QA de su nueva UI en DEV. Incluye edición de permisos por rol, overrides `Allow/Deny` y refresco de sesiones existentes.
- QA usuario limitado: completado según evidencia del 2026-09-07. Pendiente impresión física de etiquetas y QA de permisos tras deploy.
- Inventario y proveedores: placeholders; funcionalidades futuras.
- Producción: pendiente de readiness. En comprobación HTTP del 2026-10-06 el dominio principal respondió 502 en los endpoints consultados.
- Fuente de estado y pendientes: [docs/PROJECT_STATUS.md](docs/PROJECT_STATUS.md) y [consolidación SEC-PERM-1](docs/08-qa/sec-perm-1-consolidation.md).

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
- Seed catálogo: `CatalogSeed:RunOnStartup=true` siembra de forma idempotente `CatalogSections` y `CatalogProducts` desde el catálogo actual cuando las tablas existen; no aplica migraciones automáticamente.
- Imágenes de catálogo: `CatalogImages__StoragePath` apunta en DEV a `/var/www/laboratorio-tlahuac-dev/shared/catalog-images`, fuera de releases; la carpeta usa `www-data:www-data`, permisos `0750` y escritura validada para el proceso `www-data`.

## Rutas Principales

- Sitio público: `/`, `/catalogo`, `/servicios`, `/contacto`.
- Login: `/login`.
- Aplicación privada: `/app`.
- Dashboard privado real: `/app/dashboard`.
- Entregas privadas de repartidor: `/app/entregas` y `/app/entregas/:id`.
- Etiquetas privadas de órdenes: `/app/ordenes/:id/etiqueta-trabajo` y `/app/ordenes/:id/etiqueta-entrega`.
- Admin privado: `/app/admin/usuarios`, `/app/admin/catalogo` y `/app/admin/roles`.
- API: `/api/auth`, `/api/catalog/public`, `/api/catalog/images/{fileName}`, `/api/customers`, `/api/work-orders`, `/api/work-orders/{id}/delivery`, `/api/deliveries`, `/api/deliveries/{id}/retry`, `/api/payments`, `/api/dashboard/summary`, `/api/admin/catalog/sections`, `/api/admin/catalog/products`, `/api/admin/catalog/products/{id}/image`, `/api/admin/users`, `/api/admin/roles`.
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
- QA API catálogo: [docs/08-qa/catalog-api-qa.md](docs/08-qa/catalog-api-qa.md).
- QA API upload de imágenes: [docs/08-qa/catalog-image-upload-api-qa.md](docs/08-qa/catalog-image-upload-api-qa.md).
- QA UI admin catálogo: [docs/08-qa/catalog-admin-ui-qa.md](docs/08-qa/catalog-admin-ui-qa.md).
- QA catálogo público API/fallback: [docs/08-qa/public-catalog-api-qa.md](docs/08-qa/public-catalog-api-qa.md).
- QA UI admin entregas: [docs/08-qa/delivery-admin-ui-qa.md](docs/08-qa/delivery-admin-ui-qa.md).
- QA UI repartidor: [docs/08-qa/driver-mobile-qa.md](docs/08-qa/driver-mobile-qa.md).
- Diseño MVP entregas/repartidor: [docs/01-product/delivery-mvp-design.md](docs/01-product/delivery-mvp-design.md).
- Diseño técnico catálogo administrable: [docs/01-product/catalog-admin-design.md](docs/01-product/catalog-admin-design.md).
- Documentación comercial: [docs/09-commercial/](docs/09-commercial/).

## Próximos Pasos

1. Validar el HEAD de la candidata con el check `SEC-PERM-1 candidate validation` y revisar el PR a `dev`.
2. Integrar/desplegar en DEV sólo después de revisar CI, migración y diff; ejecutar QA de roles/overrides/sesiones y Clientes.
3. Cerrar impresión física `76 x 51 mm` y `102 x 51 mm`.
4. Completar `PROD-READY-1` antes de promover a `main`.
