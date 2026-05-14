# Arquitectura Técnica Global

Fuente canónica de arquitectura para Laboratorio Dental Tláhuac.

## Stack Real

- Frontend: Angular 21 en `src/LaboratorioTlahuac.Web`.
- Estilos frontend: SCSS.
- Backend/API: ASP.NET Core Web API en .NET 10, proyecto `src/LaboratorioTlahuac.Api`.
- Capas backend: `Api`, `Application`, `Domain`, `Infrastructure`.
- Persistencia: Entity Framework Core con SQL Server como proveedor objetivo.
- Auth: cookie segura HttpOnly.
- Autorización: permisos granulares por claim `permission`.
- CSRF/XSRF: `IAntiforgery`, cookie `XSRF-TOKEN` y header `X-XSRF-TOKEN`.
- Pruebas backend: xUnit y `Microsoft.AspNetCore.Mvc.Testing`.

## Estructura Del Repo

```text
.
├── AGENTS.md
├── README.md
├── docs/
├── src/
│   ├── LaboratorioTlahuac.Api/
│   ├── LaboratorioTlahuac.Application/
│   ├── LaboratorioTlahuac.Domain/
│   ├── LaboratorioTlahuac.Infrastructure/
│   └── LaboratorioTlahuac.Web/
└── tests/
    ├── LaboratorioTlahuac.Api.Tests/
    ├── LaboratorioTlahuac.Application.Tests/
    └── LaboratorioTlahuac.Domain.Tests/
```

## Separación Sitio Público Vs App Privada

- Sitio público: rutas públicas dentro de `src/LaboratorioTlahuac.Web/src/app/public`.
- Catálogo público: ruta `/catalogo`, renderizada desde data tipada local.
- Login: ruta pública `/login`, fuera del layout privado.
- App privada: rutas bajo `/app`, renderizadas por `PrivateLayoutComponent`.
- Dashboard privado real: `/app/dashboard`.
- No crear otra app Angular ni otro repositorio para el sitio público.
- Fase 1 del sitio público reutiliza `PublicLayoutComponent` y páginas standalone bajo `public/pages`.
- El CTA de WhatsApp apunta temporalmente a `/contacto` porque el número real no está confirmado.

ADR relacionada: `docs/04-decisions/ADR-0002-single-public-site-private-app.md`.

## Rutas Públicas

Rutas existentes:

- `/`
- `/catalogo`
- `/servicios`
- `/contacto`
- `/login`

Rutas públicas futuras opcionales, solo si el contenido lo justifica:

- `/trabajos`
- `/ubicacion`
- `/privacidad`

## Rutas Privadas

Rutas existentes bajo `/app`:

- `/app/dashboard`
- `/app/ordenes`
- `/app/ordenes/nueva`
- `/app/ordenes/:id`
- `/app/ordenes/:id/editar`
- `/app/clientes`
- `/app/clientes/nuevo`
- `/app/clientes/:id`
- `/app/clientes/:id/editar`
- `/app/pagos`
- `/app/inventario`
- `/app/proveedores`
- `/app/admin/usuarios`
- `/app/admin/roles`
- `/app/access-denied`

`/dashboard` no es una ruta privada real del sistema actual.

## Backend / API

La API vive en `src/LaboratorioTlahuac.Api` y expone endpoints REST. Los controladores/endpoints deben mantenerse delgados y delegar reglas en servicios de aplicación/infraestructura.

Módulos API principales:

- `GET /health`
- `/api/auth`
- `/api/customers`
- `/api/work-orders`
- `/api/payments`
- `/api/dashboard/summary`

Detalle técnico backend: `docs/03-architecture/backend-architecture.md`.

## Frontend Angular

La app Angular vive en `src/LaboratorioTlahuac.Web`.

Estructura conceptual:

- `public/`: sitio institucional.
- `auth/`: login.
- `admin/`: layout y páginas administrativas.
- `features/`: módulos privados.
- `core/`: auth, guards y cliente HTTP.

Estructura pública implementada en Fase 1:

- `public/layout/public-layout.component.ts`: header, navegación pública y footer.
- `public/pages/home/home-page.component.ts`: landing de `/`.
- `public/pages/home/home-page.component.scss`: estilos mobile-first de la landing.
- `public/data/catalog-data.ts`: data tipada del catálogo público.
- `public/pages/catalog/catalog-page.component.ts`: página pública `/catalogo`.
- `public/pages/catalog/catalog-page.component.scss`: estilos mobile-first del catálogo.
- `public/pages/services/services-page.component.ts`: página pública `/servicios`.
- `public/pages/contact/contact-page.component.ts`: página pública `/contacto`.

Assets públicos del catálogo:

- Fuente local: `src/assets/catalog/products/`.
- Ruta servida por Angular: `/assets/catalog/products/`.
- `angular.json` copia `src/assets/**/*.webp` para el catálogo público.

Detalle frontend: `docs/03-architecture/frontend-architecture.md`.

## Persistencia

- Proveedor objetivo: SQL Server.
- ORM: Entity Framework Core.
- DbContext: `LaboratorioTlahuacDbContext`.
- Migraciones existentes: `InitialSecurityModel`, `AddCustomersAndInternalDoctors`, `AddWorkOrders`, `AddPayments`.
- No hay auto-migración al iniciar la aplicación.

Detalle de base de datos: `docs/03-architecture/database-design.md`.

## Pruebas

- Pruebas backend bajo `tests/`.
- QA funcional del MVP administrativo documentada en `docs/08-qa/`.
- Frontend no tiene runner no interactivo configurado como script npm; se valida con `npm run build` y revisión manual cuando aplique.
- QA responsive del sitio público: `docs/08-qa/RESPONSIVE_CHECKLIST.md`.

## Supuestos Técnicos

- Dominio principal: `laboratoriodentaltlahuac.com`.
- Sitio público y app privada se mantienen inicialmente en el mismo dominio.
- En producción, `src/environments/environment.ts` usa mismo origen con `apiBaseUrl: ''`.
- En desarrollo, Angular consume la API en `http://localhost:5277`.
- La autorización real siempre se valida en backend.
- Cualquier cambio de auth, rutas privadas, permisos, cookies o CSRF/XSRF debe revisar `docs/03-architecture/AUTH_FLOW.md`.
