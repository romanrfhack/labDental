# Despliegue

Fuente canónica de build, deploy, dominio, DNS, variables de entorno y publicación.

## Dominio

- Dominio principal: `laboratoriodentaltlahuac.com`.
- URL DEV: `https://dev.laboratoriodentaltlahuac.com`.
- Estado de despliegue DEV: publicado en VPS y validado como baseline UAT inicial.
- Rama DEV desplegada: `dev`.
- Estado de despliegue productivo: pendiente.
- Plataforma/DNS/HTTPS productivos: pendientes por definir.

## Estrategia Actual

- Sitio público y app privada se sirven desde el mismo frontend Angular.
- La app privada vive bajo `/app`.
- El login vive en `/login`.
- La API .NET debe quedar protegida detrás de HTTPS y reverse proxy cuando exista producción.
- En producción, el frontend asume mismo origen con `apiBaseUrl: ''`, salvo decisión contraria.
- En desarrollo, Angular consume la API en `http://localhost:5277`.
- En DEV publicado, la validación funcional confirmada por navegador se documenta en `docs/05-delivery/dev-deployment-validation.md`.

## Ambiente DEV Publicado

- URL: `https://dev.laboratoriodentaltlahuac.com`.
- Rama: `dev`.
- Plataforma documentada: VPS.
- Fecha de baseline UAT inicial: 2026-07-02.
- Resultado: sitio público, `/login`, login QA, `/app/dashboard` autenticado y redirección sin sesión a `/login` validados manualmente por el responsable del proyecto.

Los nombres de servicios del VPS, rutas internas, usuario del sistema, configuración exacta de reverse proxy y comandos operativos del servidor no están documentados en este repositorio. Esta fase no inspeccionó ni modificó el VPS.

Validación con `curl` sin credenciales:

| URL | Resultado |
| --- | --- |
| `https://dev.laboratoriodentaltlahuac.com/` | `200` |
| `https://dev.laboratoriodentaltlahuac.com/servicios` | `200` |
| `https://dev.laboratoriodentaltlahuac.com/catalogo` | `200` |
| `https://dev.laboratoriodentaltlahuac.com/contacto` | `200` |
| `https://dev.laboratoriodentaltlahuac.com/login` | `200` |
| `https://dev.laboratoriodentaltlahuac.com/app/dashboard` | `200` |

Nota: como Angular se sirve como SPA, `curl` puede devolver shell `200` para rutas privadas. La protección real de `/app/dashboard` fue validada manualmente en navegador: sin sesión redirige a `/login`.

## Comandos De Build Y Validación

Backend desde la raíz:

```bash
dotnet restore
dotnet build
dotnet test
```

Frontend desde `src/LaboratorioTlahuac.Web`:

```bash
npm ci
npm run build
```

Ejecutar API local:

```bash
dotnet run --project src/LaboratorioTlahuac.Api/LaboratorioTlahuac.Api.csproj
```

## Variables De Entorno

Detectadas o documentadas:

- `ASPNETCORE_ENVIRONMENT`
- `ConnectionStrings__DefaultConnection` o `ConnectionStrings:DefaultConnection`
- `SecuritySeed__RunOnStartup`
- `LT_ADMIN_EMAIL`
- `LT_ADMIN_PASSWORD`
- `LT_ADMIN_FULL_NAME`
- `Cors__AllowedOrigins` si frontend y API quedan en orígenes distintos

Pendientes de producción:

- Cadena de conexión productiva.
- Política final de CORS si se separan subdominios.
- Secretos productivos para seed inicial, si se decide usarlo.
- Configuración de logs, monitoreo y respaldo.

## Migraciones Y Base De Datos

- Proveedor objetivo: SQL Server.
- Migraciones existentes: `InitialSecurityModel`, `AddCustomersAndInternalDoctors`, `AddWorkOrders`, `AddPayments`.
- No hay auto-migración en startup.
- No ejecutar migraciones contra producción sin plan de despliegue, respaldo previo y ventana de rollback.

Comando de migración local documentado:

```bash
dotnet ef database update \
  --project src/LaboratorioTlahuac.Infrastructure/LaboratorioTlahuac.Infrastructure.csproj \
  --startup-project src/LaboratorioTlahuac.Api/LaboratorioTlahuac.Api.csproj
```

## Pendientes De DNS

- Definir proveedor donde apuntará `laboratoriodentaltlahuac.com`.
- Definir si se usará `www.laboratoriodentaltlahuac.com`.
- Definir si API seguirá en mismo dominio o migrará después a subdominio.
- Configurar registros DNS según la plataforma elegida.
- Validar HTTPS y redirecciones canónicas.

## Checklist Antes De Publicar

- [x] DEV publicado en VPS.
- [x] DEV validado como baseline UAT inicial.
- [x] `/login` validado como público en DEV.
- [x] `/app/dashboard` validado como privado en DEV por navegador.
- [x] `/dashboard` confirmado como no ruta privada real.
- [ ] Plataforma de deploy definida.
- [ ] DNS configurado.
- [ ] HTTPS activo.
- [ ] Build frontend generado sin errores.
- [ ] Build backend generado sin errores.
- [ ] `dotnet test` ejecutado.
- [ ] Variables de entorno configuradas sin secretos en el repo.
- [ ] Base de datos productiva creada y respaldable.
- [ ] Migraciones revisadas antes de aplicar.
- [ ] Respaldo o plan de respaldo definido.
- [ ] Cookie de auth con `Secure` en producción.
- [ ] `/app` protegido por sesión.
- [ ] Endpoints `/api` responden `401`/`403` sin redirección HTML.
- [ ] Sitio público validado en móvil.
- [ ] Checklist responsive ejecutado.

## Documentos Relacionados

- Ambientes: `docs/06-operations/environments.md`.
- Backup y restore: `docs/06-operations/backup-and-restore.md`.
- Auth y seguridad: `docs/03-architecture/AUTH_FLOW.md`.
- Validación DEV baseline UAT: `docs/05-delivery/dev-deployment-validation.md`.
