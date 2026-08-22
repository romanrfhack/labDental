# Ambientes

Última sincronización: **2026-08-22 — DOC-SYNC-1**.

## Local

Ambiente de desarrollo del equipo.

Características:

- Frontend Angular.
- API .NET.
- SQL Server local/de desarrollo según configuración segura.
- Secrets fuera del repositorio mediante user-secrets o variables de entorno.
- No es fuente de verdad para aceptación final.

La configuración exacta de puertos locales puede variar; no debe hardcodearse en documentación operativa si no es necesaria.

## Development / UAT

Estado: **activo y publicado**.

- Rama: `dev`.
- Sitio: `https://dev.laboratoriodentaltlahuac.com`.
- Plataforma: VPS.
- Uso: integración, QA y UAT previa a producción.
- Deploy: GitHub Actions desde `dev`.
- Health público: `/health`.
- App privada: `/app`.
- Login: `/login`.
- API y frontend se sirven bajo el mismo origen público.

Storage persistente validado:

- `${LDT_APP_ROOT}/shared/catalog-images`.
- En DEV se validó escritura/lectura por el proceso de la API y persistencia entre releases.

DEV es actualmente el ambiente de referencia funcional del proyecto.

## Production

Estado: **pendiente de publicación**.

- Rama prevista: `main`.
- Dominio principal previsto: `https://laboratoriodentaltlahuac.com`.
- Deploy productivo existe como capacidad en el workflow, pero está deshabilitado hasta que `LDT_ENABLE_PROD_DEPLOY == true` y el environment `production` esté completo.

Antes de habilitar producción deben definirse/validarse:

- VPS/plataforma productiva definitiva.
- SQL Server productivo.
- Secrets y variables productivas.
- `LDT_LOCAL_HEALTH_URL` y `LDT_PUBLIC_HEALTH_URL`.
- DNS.
- Decisión sobre `www`.
- HTTPS y redirecciones canónicas.
- Servicio API y root de aplicación.
- `shared/catalog-images` productivo y permisos.
- Backup/restore de BD e imágenes.
- Monitoreo/logs mínimos.

## Separación De Datos

- Local, DEV y PROD no deben compartir base de datos.
- DEV no debe utilizar credenciales productivas.
- PROD no debe ejecutar seeds de QA Development-only.
- Los uploads de un ambiente deben vivir en su propio `shared/catalog-images`.

## Seguridad

- No guardar secretos en Git.
- Producción debe usar HTTPS.
- Cookies de autenticación deben usar configuración segura para producción.
- Endpoints privados deben responder `401/403` según corresponda, no HTML de login.
- El acceso al VPS debe mantenerse restringido a la operación necesaria.

## Flujo De Promoción

```text
local -> dev/UAT -> release candidate -> main -> production
```

No promover `dev -> main` hasta cerrar `PROD-READY-1`.

Fuente vigente: `docs/05-delivery/current-work-plan.md`.
