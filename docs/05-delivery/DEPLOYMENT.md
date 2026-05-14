# Despliegue

Fuente canónica de build, deploy, dominio, DNS, variables de entorno y publicación.

## Dominio

- Dominio principal: `laboratoriodentaltlahuac.com`.
- Estado de despliegue: pendiente.
- Plataforma de deploy: pendiente por definir.
- HTTPS productivo: pendiente.
- DNS: pendiente.

## Estrategia Actual

- Sitio público y app privada se sirven desde el mismo frontend Angular.
- La app privada vive bajo `/app`.
- El login vive en `/login`.
- La API .NET debe quedar protegida detrás de HTTPS y reverse proxy cuando exista producción.
- En producción, el frontend asume mismo origen con `apiBaseUrl: ''`, salvo decisión contraria.
- En desarrollo, Angular consume la API en `http://localhost:5277`.

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
