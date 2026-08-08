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

### Health Check Y Rollback De DEV

- Después de reiniciar la API, el deploy valida primero `http://127.0.0.1:5012/health` y después `https://dev.laboratoriodentaltlahuac.com/health`.
- Cada validación exige HTTP `200` y permite hasta 30 intentos con 3 segundos entre intentos; cada `curl -fsS` limita la conexión a 2 segundos y la petición completa a 5 segundos.
- `systemctl is-active` se conserva como información de estado, pero no sustituye a `/health`: el servicio puede estar activo antes de que Kestrel acepte tráfico.
- Si el release nuevo no queda sano, antes del rollback se muestran el estado del servicio, las últimas 120 entradas de su journal y los destinos de `backend/current` y `frontend/current`. No se imprimen variables de entorno ni cadenas de conexión.
- El rollback restaura ambos symlinks, reinicia el servicio y vuelve a validar health local y público con los mismos reintentos. Si no puede restaurar, reiniciar o recuperar health, lo reporta explícitamente para intervención manual.
- Para un futuro deploy de `main`, deben configurarse las variables de GitHub `LDT_LOCAL_HEALTH_URL` y `LDT_PUBLIC_HEALTH_URL` con los endpoints de producción antes de habilitarlo.

El `502` observado al desplegar `dev-38-11ea0a2` se trata como un probable problema de timing: el workflow anterior esperaba solo 5 segundos y hacía una única petición pública, mientras que arranques observados de la API tardaron aproximadamente 15–20 segundos. El rollback mantuvo DEV estable en `dev-37-3dc0347`. Este ajuste modifica solo automatización y documentación; no cambia código funcional de backend o frontend, migraciones ni catálogo.

Cierre 2026-08-08: el ajuste quedó validado en un deploy real. El commit `8be9e14ec8cda5e8486770a77733a4413e456e96` terminó con GitHub Actions `success` y DEV respondió `200` en `/health`, `/catalogo` y `/api/catalog/public`. El intento anterior del commit `11ea0a296253d2e0a2660963430d49482dc4aaee` había fallado durante el health check posterior al restart; sin evidencia de crash del release y con el arranque más lento observado, se mantiene como causa probable la ventana demasiado agresiva del check anterior. El pendiente técnico de hacer resiliente el health check DEV queda cerrado.

## Layout De Releases Y Persistencia DEV

La revisión documental de Fase 3.5.4.0 confirma en `.github/workflows/deploy.yml` y `.github/scripts/deploy-lab.sh` el siguiente layout lógico:

```text
${LDT_APP_ROOT}/
├── backend/releases/{releaseId}/
├── backend/current -> releases/{releaseId}
├── frontend/releases/{releaseId}/
├── frontend/current -> releases/{releaseId}
├── migrations/releases/{releaseId}.sql
├── logs/
└── shared/
```

- Cada deploy instala backend y frontend en releases inmutables nuevos y mueve ambos symlinks `current`.
- El rollback restaura los symlinks anteriores y reinicia la API.
- La poda conserva cinco releases de backend/frontend y no elimina `shared`.
- El script crea `${LDT_APP_ROOT}/shared`, pero actualmente no crea una subcarpeta para catálogo, no asigna permisos específicos a ella y no ejecuta backup de archivos.
- No deben guardarse uploads en `backend/releases`, `frontend/releases` ni `frontend/current`: se perderían al cambiar o podar releases.

El valor efectivo de `LDT_APP_ROOT` se mantiene fuera del repositorio. No se leyó ni imprimió durante Fase 3.5.4.0.

## Almacenamiento De Imágenes De Catálogo

Backend implementado localmente en Fase 3.5.4.1; preparación operativa DEV pendiente:

- Carpeta persistente: `${LDT_APP_ROOT}/shared/catalog-images`.
- Ruta DEV esperada: `/var/www/laboratorio-tlahuac-dev/shared/catalog-images`, pendiente de confirmar contra el `LDT_APP_ROOT` real antes de activar upload.
- Configuración backend requerida: `CatalogImages__StoragePath`, ruta absoluta definida fuera del repositorio y sin hardcodear la ruta DEV. Si falta, está vacía, es relativa o la carpeta no existe/no es accesible, POST responde `503` controlado.
- Escritura: únicamente por el usuario efectivo del servicio API, con ownership/grupo y permisos mínimos; no usar permisos globales `777`.
- Lectura pública: `GET /api/catalog/images/{fileName}` desde ASP.NET Core.
- Persistencia en base: `ImagePath` igual a `/api/catalog/images/{fileName}`.
- Compatibilidad: los assets existentes `assets/catalog/products/...` continúan sirviéndose desde el frontend.

DEV ya publica `/api/catalog/public` en el mismo origen, por lo que se espera que el reverse proxy cubra el prefijo `/api`. La configuración exacta de Nginx no está versionada. Si el proxy cubre todo `/api`, el endpoint de imágenes no requiere cambio Nginx; este supuesto debe validarse en DEV antes del cierre de 3.5.4.1. Si existe una allowlist de rutas, el ajuste de proxy será un cambio separado, revisado y documentado.

El límite de request body del proxy debe admitir 2 MB más el overhead de `multipart/form-data`. No se requiere un alias/ruta Nginx nuevo, pero si el límite actual es menor debe ajustarse y documentarse antes de validar upload.

Preparación operativa requerida antes de habilitar upload:

1. Confirmar `LDT_APP_ROOT` y el usuario/grupo efectivos del servicio sin imprimir secretos ni el archivo de ambiente.
2. Crear `${LDT_APP_ROOT}/shared/catalog-images` si no existe.
3. Dar lectura/escritura al servicio API y retirar escritura a usuarios no requeridos.
4. Configurar `CatalogImages__StoragePath` fuera del repo.
5. Confirmar espacio libre, escritura controlada y lectura por API.
6. Confirmar que el proxy admite el tamaño máximo más overhead multipart.
7. Validar que una imagen siga disponible después de otro deploy y de un rollback de prueba controlado.

La implementación local de Fase 3.5.4.1 no ejecutó esos pasos ni modificó scripts, workflow, Nginx, systemd o VPS. Hasta completarlos, no debe habilitarse una prueba real de upload en DEV.

### Backup Manual/Recomendado

- Respaldar `${LDT_APP_ROOT}/shared/catalog-images` junto con la base de datos del mismo ambiente y punto temporal.
- Tomar backup manual antes de cambiar permisos/ruta, limpiar huérfanos o restaurar.
- Conservar el respaldo fuera de releases y preferentemente fuera del mismo VPS.
- Restaurar conservando nombres y permisos; después comprobar una muestra de `ImagePath` mediante el GET público.
- Antes de producción se requiere una restauración probada, destino protegido y política acordada de frecuencia/retención.
- Baseline recomendado cuando haya uploads reales: copia diaria más copia previa a cambios operativos/deploy relevantes. La automatización final queda pendiente.

Diseño canónico de validación, endpoints, autorización y consistencia: `docs/01-product/catalog-image-upload-design.md`.

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
- `CatalogImages__StoragePath` para el backend implementado en Fase 3.5.4.1

Pendientes de producción:

- Cadena de conexión productiva.
- Política final de CORS si se separan subdominios.
- Secretos productivos para seed inicial, si se decide usarlo.
- Configuración de logs, monitoreo y respaldo.
- Ruta, permisos y backup productivos para `shared/catalog-images`.

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
- [ ] `shared/catalog-images` creado fuera de releases con permisos mínimos para el servicio API.
- [ ] Backup y restore de imágenes del catálogo probados junto con la base de datos.
- [ ] Proxy `/api/catalog/images/{fileName}` validado sin exponer la carpeta del filesystem.
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
