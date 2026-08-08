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
- Catálogo público: ruta `/catalogo`, renderizada desde `GET /api/catalog/public` con fallback a data tipada local.
- Diseño Fase 3.5.0: el catálogo administrable se documenta en `docs/01-product/catalog-admin-design.md`; Fase 3.5.1 ya implementa backend/API/seed, Fase 3.5.2 implementa UI admin y Fase 3.5.3 conecta `/catalogo` a la API pública con fallback.
- Logo público: asset local en `src/LaboratorioTlahuac.Web/src/assets/brand/logo-ldt.webp`, servido como `/assets/brand/logo-ldt.webp`.
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
- `/app/entregas`
- `/app/entregas/:id`
- `/app/ordenes`
- `/app/ordenes/nueva`
- `/app/ordenes/:id`
- `/app/ordenes/:id/editar`
- `/app/ordenes/:id/etiqueta-trabajo`
- `/app/ordenes/:id/etiqueta-entrega`
- `/app/clientes`
- `/app/clientes/nuevo`
- `/app/clientes/:id`
- `/app/clientes/:id/editar`
- `/app/pagos`
- `/app/inventario`
- `/app/proveedores`
- `/app/admin/usuarios`
- `/app/admin/catalogo`
- `/app/admin/roles`
- `/app/access-denied`

`/dashboard` no es una ruta privada real del sistema actual.

Fase 3.3 reemplaza los placeholders de `/app/admin/usuarios` y `/app/admin/roles`: Usuarios es funcional para CRUD administrativo mínimo y asignación de roles existentes; Roles es funcional en modo readonly para ver permisos.

Rutas privadas de Fase 3.4.3:

- `/app/entregas`
- `/app/entregas/:id`

Estas rutas implementan el flujo mobile-first de repartidor y requieren `deliveries.view`. Usan `GET /api/deliveries?assignedToMe=true` para el listado, no dan al rol `Repartidor` acceso amplio a `/app/ordenes`, `/app/clientes` ni `/app/pagos`, y muestran acciones de cierre solo con `deliveries.complete`.

## Backend / API

La API vive en `src/LaboratorioTlahuac.Api` y expone endpoints REST. Los controladores/endpoints deben mantenerse delgados y delegar reglas en servicios de aplicación/infraestructura.

Módulos API principales:

- `GET /health`
- `/api/auth`
- `/api/catalog/public`
- `/api/catalog/images/{fileName}`
- `/api/customers`
- `/api/work-orders`
- `/api/payments`
- `/api/dashboard/summary`
- `/api/admin/catalog`
- `/api/admin/users`
- `/api/admin/roles`
- `/api/deliveries`
- `/api/work-orders/{workOrderId}/delivery`

API de entregas implementada en Fase 3.4.1:

- `GET /api/deliveries`
- `GET /api/deliveries/{id}`
- `GET /api/work-orders/{workOrderId}/delivery`
- `POST /api/work-orders/{workOrderId}/delivery`
- `PATCH /api/deliveries/{id}/assign`
- `PATCH /api/deliveries/{id}/out-for-delivery`
- `PATCH /api/deliveries/{id}/complete`
- `PATCH /api/deliveries/{id}/failed`
- `PATCH /api/deliveries/{id}/retry`

Fase 3.4.1 crea una entidad separada `WorkOrderDelivery` con `DeliveryStatus` propio, en lugar de mezclar asignación, salida, entrega real y receptor dentro de `WorkOrder`. Fuente: `docs/01-product/delivery-mvp-design.md`.

Validación DEV 2026-07-04: commit `e4c28205c6b866ab0d71edb13c49164100340b0d` desplegado correctamente mediante GitHub Actions run `28712956106`; `GET /health` responde `200` y `GET /api/deliveries` sin sesión responde `401`. Esto confirma que la Delivery API está publicada y protegida en DEV.

QA DEV Fase 3.4.3.1, 2026-07-05: commit `59542efd4f57df7ba04a2444c5496040810d1702` desplegado con GitHub Actions `success`; `GET /health` responde `200`; `GET /api/deliveries` sin sesión responde `401`; Repartidor/Admin validaron redirect por permisos, access denied con `Ir a mi inicio`, retry de `FailedDelivery`, cierre posterior, `WorkOrder.Status` sin cambio por retry y refresco de grid sin observaciones reportadas.

Catálogo administrable Fase 3.5.1:

- Modelo implementado: `CatalogSection` y `CatalogProduct` con `Key` estable, `IsActive`, `SortOrder`, `ImagePath`, `AltText`, `CreatedAtUtc` y `UpdatedAtUtc`; `CatalogProduct` agrega `PriceAmount decimal(18,2)` y `Currency` default `MXN`.
- Migración: `20260705054221_AddCatalogManagement`, crea solo `CatalogSections` y `CatalogProducts`, con FK producto-sección, `Key` único, e índices `SortOrder` e `IsActive`.
- Seed: `CatalogSeed:RunOnStartup=true` siembra de forma idempotente 12 secciones y 40 productos desde `catalog-data.ts` cuando las tablas existen. No depende del filesystem ni copia imágenes.
- Permisos implementados: `catalog.view` y `catalog.manage`. Admin recibe ambos por `Permissions.All`; `Repartidor` no recibe permisos de catálogo.
- Endpoint público implementado: `GET /api/catalog/public`, sin auth, solo activos y sin campos administrativos.
- Endpoints privados implementados: `GET/POST/PUT/PATCH /api/admin/catalog/sections` y `GET/POST/PUT/PATCH /api/admin/catalog/products`; lectura requiere `catalog.view` o `catalog.manage`, mutaciones requieren `catalog.manage`.
- Estrategia MVP de imágenes: conservar assets estáticos existentes y guardar rutas relativas seguras en `ImagePath`; upload queda para Fase 3.5.4.

Imágenes persistentes Fase 3.5.4.1:

- `POST /api/admin/catalog/products/{id}/image` y `DELETE /api/admin/catalog/products/{id}/image` requieren `catalog.manage`; GET `/api/catalog/images/{fileName}` es público.
- API mapea `IFormFile` a `CatalogImageUploadRequest` con `Stream`; Application no depende de tipos ASP.NET.
- `CatalogService` confirma producto y coordina persistencia de `ImagePath`; `ICatalogImageStorage`/`CatalogImageStorage` encapsulan filesystem.
- `CatalogImagesOptions.StoragePath` se configura por `CatalogImages__StoragePath`; no deriva del content root ni de symlinks `current`.
- Escritura por streaming a temporal exclusivo, rename dentro de la misma raíz y compensación del archivo nuevo si falla la base.
- `ImagePath` acepta assets heredados y `/api/catalog/images/{fileName}` generado. No hay migración ni cambio de modelo.

UI catálogo administrable Fase 3.5.2:

- Ruta privada: `/app/admin/catalogo`.
- Navegación privada: `Catálogo`, visible con `catalog.view` o `catalog.manage`.
- Protección frontend: `permissionGuard` con `catalog.view`, porque el guard actual soporta un permiso por ruta.
- Acciones mutables en UI: visibles solo con `catalog.manage`; backend sigue siendo autoridad.
- Selección de imágenes: allowlist local de assets `.webp` existentes bajo `assets/catalog/products`, sin upload.
- Desde Fase 3.5.3, `/catalogo` público consume `GET /api/catalog/public` y conserva `catalog-data.ts` como fallback local si la API falla o responde con datos inválidos.

Detalle técnico backend: `docs/03-architecture/backend-architecture.md`.

## Configuración Operativa

Dashboard:

- Clave: `Dashboard:BusinessTimeZone`.
- Default: `America/Mexico_City`.
- Uso actual: calcular la fecha operativa del laboratorio para métricas `dueToday`, `overdue` y `upcomingDue`.
- `generatedAtUtc` se mantiene como fecha/hora UTC del resumen.
- `DeliveryDate` sigue representando la fecha de entrega capturada; no se convierte ni cambia de tipo.

Compatibilidad de zona horaria:

- El ID canónico del proyecto es IANA: `America/Mexico_City`.
- Para compatibilidad en Windows, el backend acepta el equivalente `Central Standard Time (Mexico)` si el sistema operativo no resuelve el ID IANA.
- Si se configura un ID inválido o no disponible, el dashboard debe fallar de forma explícita en vez de calcular métricas con una zona incorrecta.

Seed QA limitado:

- Clave baseline: `SecuritySeed:EnsureBaselineOnStartup`.
- Uso baseline Fase 3.4.1.1: asegurar catálogo de permisos existentes, sincronizar permisos faltantes al rol `Admin` existente y asegurar rol `Repartidor` en `Development`.
- Rol `Repartidor`: rol de sistema con `deliveries.view` y `deliveries.complete`; no otorga acceso amplio a órdenes completas, clientes, pagos, usuarios ni roles.
- Validación Fase 3.3.1: `appsettings.json` conserva baseline apagado por default general, `appsettings.Development.json` lo activa solo para Development, y no se guardan passwords reales en appsettings.
- Clave: `SecuritySeed:LimitedQaUser`.
- Bandera: `SecuritySeed:LimitedQaUser:RunOnStartup`.
- Campos: `Email`, `Password`, `FullName` y `Permissions`.
- Variables sensibles soportadas: `LT_QA_LIMITED_EMAIL`, `LT_QA_LIMITED_PASSWORD` y `LT_QA_LIMITED_FULL_NAME`.
- Uso actual: crear o sincronizar un usuario QA limitado local solo en `Development`.
- Permisos: allowlist contra `Permissions.All`; para validar access-denied se recomienda `customers.view` sin `reports.view`.
- Seguridad: desactivado por default, no corre fuera de `Development`, no imprime password, no usa SQL manual, no crea migraciones y no expone endpoints.

Seed catálogo:

- Clave: `CatalogSeed:RunOnStartup`.
- Valor default/configurado: `true`.
- Uso: sembrar `CatalogSections` y `CatalogProducts` desde el catálogo actual cuando las tablas existen.
- Seguridad: idempotente por `Key`, no imprime secretos, no depende de filesystem, no copia assets y no aplica migraciones automáticamente.

## Frontend Angular

La app Angular vive en `src/LaboratorioTlahuac.Web`.

Estructura conceptual:

- `public/`: sitio institucional.
- `auth/`: login.
- `admin/`: layout y páginas administrativas.
- `features/`: módulos privados.
- `core/`: auth, guards y cliente HTTP.

Routing:

- `src/app/app.routes.ts` carga layouts y páginas con `loadComponent` para evitar imports eager de features públicas/privadas en el bundle inicial.
- Imports eager actuales en rutas: `Routes`, `authGuard` y `permissionGuard`.
- `/login` se mantiene público; `/app` se mantiene protegido por `authGuard`; las rutas privadas conservan sus permisos con `permissionGuard`.
- `/dashboard` no existe como ruta privada real; el dashboard privado sigue siendo `/app/dashboard`.
- Optimización 2026-07-04: initial bundle bajó de `535.62 kB` con warning de budget a `304.19 kB` sin warning, sin cambiar `angular.json`.
- Deploy DEV 2026-07-04: la optimización lazy loading quedó desplegada en commit `e4c28205c6b866ab0d71edb13c49164100340b0d`; el warning de initial bundle queda resuelto en DEV.

Estructura pública implementada en Fase 1:

- `public/layout/public-layout.component.ts`: header, navegación pública y footer.
- `public/pages/home/home-page.component.ts`: landing de `/`.
- `public/pages/home/home-page.component.scss`: estilos mobile-first de la landing.
- `public/data/catalog-data.ts`: fallback local tipado del catálogo público.
- `public/pages/catalog/catalog-page.component.ts`: página pública `/catalogo`.
- `public/pages/catalog/catalog-page.component.scss`: estilos mobile-first del catálogo.
- `public/pages/services/services-page.component.ts`: página pública `/servicios`.
- `public/pages/contact/contact-page.component.ts`: página pública `/contacto`.

Assets públicos del catálogo:

- Fuente local: `src/assets/catalog/products/`.
- Ruta servida por Angular: `/assets/catalog/products/`.
- `angular.json` copia `src/assets/**/*.webp` para el catálogo público.
- Fase 3.5.1 mantiene estos assets como rutas `ImagePath` del MVP administrable y difiere upload desde `/app` hasta definir almacenamiento, validación y backup.
- Fase 3.5.2 agrega `features/catalog/catalog-image-options.ts` como allowlist frontend de imágenes `.webp` existentes para la UI admin.

Assets públicos de marca:

- Fuente local: `src/assets/brand/logo-ldt.webp`.
- Ruta servida por Angular: `/assets/brand/logo-ldt.webp`.
- `angular.json` ya copia `src/assets/**/*.webp`, por lo que no se requirió cambiar configuración para el logo.

Detalle frontend: `docs/03-architecture/frontend-architecture.md`.

## Persistencia

- Proveedor objetivo: SQL Server.
- ORM: Entity Framework Core.
- DbContext: `LaboratorioTlahuacDbContext`.
- Migraciones existentes: `InitialSecurityModel`, `AddCustomersAndInternalDoctors`, `AddWorkOrders`, `AddPayments`, `AddWorkOrderDeliveries`, `AddCatalogManagement`.
- Migración catálogo: `20260705054221_AddCatalogManagement`, crea `CatalogSections` y `CatalogProducts` sin tocar tablas ajenas.
- Validación local Fase 3.4.1.1: `AddWorkOrderDeliveries` aplica correctamente en SQL Server local `LaboratorioTlahuac_Dev`, creando tabla `WorkOrderDeliveries` con FK requerida a `WorkOrders`, FK opcional a `Security.Users`, índice único por `WorkOrderId` e índices por asignado, estado y creación.
- DEV Fase 3.4.1: la migración `WorkOrderDeliveries` ya está aplicada o la base DEV está al día; la evidencia publicada es `GET /api/deliveries` sin sesión respondiendo `401` en lugar de `404`.
- No hay auto-migración al iniciar la aplicación.

Detalle de base de datos: `docs/03-architecture/database-design.md`.

## Pruebas

- Pruebas backend bajo `tests/`.
- QA funcional del MVP administrativo documentada en `docs/08-qa/`.
- Frontend no tiene runner no interactivo configurado como script npm; se valida con `npm run build` y revisión manual cuando aplique.
- QA responsive del sitio público: `docs/08-qa/RESPONSIVE_CHECKLIST.md`.
- QA usuarios/roles Fase 3.3.1: `docs/08-qa/users-roles-qa.md` registra endpoints, permisos, matriz `401` sin sesión, cobertura `403` por pruebas API, riesgos de contraseña temporal y preparación DEV.
- QA API entregas Fase 3.4.1: `docs/08-qa/delivery-api-qa.md` registra endpoints, permisos, transiciones, migración y validación técnica.

## Supuestos Técnicos

- Dominio principal: `laboratoriodentaltlahuac.com`.
- Sitio público y app privada se mantienen inicialmente en el mismo dominio.
- En producción, `src/environments/environment.ts` usa mismo origen con `apiBaseUrl: ''`.
- En desarrollo, Angular consume la API en `http://localhost:5277`.
- La autorización real siempre se valida en backend.
- Cualquier cambio de auth, rutas privadas, permisos, cookies o CSRF/XSRF debe revisar `docs/03-architecture/AUTH_FLOW.md`.
