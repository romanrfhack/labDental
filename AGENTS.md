# Reglas Para Codex

Este repositorio pertenece al proyecto Laboratorio Dental Tláhuac.

## Principios Permanentes

- El desarrollo del sitio público y de la aplicación privada debe ser mobile-first y responsive desde el inicio.
- No se deben hacer cambios grandes, cambios de arquitectura, cambios de base de datos ni cambios de despliegue sin documentarlos.
- Cada cambio de código debe actualizar `docs/PROJECT_STATUS.md` y `docs/IMPLEMENTATION_LOG.md`.
- Antes de tocar autenticación, rutas privadas, permisos, sesiones, cookies, CSRF/XSRF o datos sensibles, revisar `docs/03-architecture/AUTH_FLOW.md` y `docs/03-architecture/ARCHITECTURE.md`.
- Respetar los comandos existentes del proyecto antes de proponer comandos nuevos.
- No crear repositorios nuevos ni duplicar aplicaciones cuando la estructura actual ya soporte el cambio.
- Si falta información, documentar los supuestos claramente en el documento afectado.

## Comandos Detectados

- Backend:
  - `dotnet restore`
  - `dotnet build`
  - `dotnet test`
  - `dotnet run --project src/LaboratorioTlahuac.Api/LaboratorioTlahuac.Api.csproj`
- Frontend:
  - Ejecutar desde `src/LaboratorioTlahuac.Web`.
  - `npm install` o `npm ci`
  - `npm start`
  - `npm run build`
  - `npm run watch`

## Documentación Obligatoria

- `docs/README.md`: índice general y fuentes canónicas.
- `docs/PROJECT_STATUS.md`: estado vivo del proyecto por frente.
- `docs/ROADMAP.md`: roadmap global.
- `docs/IMPLEMENTATION_LOG.md`: bitácora cronológica de tareas.
- `docs/01-product/public-website.md`: definición funcional del sitio público.
- `docs/01-product/internal-system.md`: definición funcional del sistema privado.
- `docs/03-architecture/ARCHITECTURE.md`: estructura, stack, rutas y separación público/privado.
- `docs/03-architecture/AUTH_FLOW.md`: login, sesión, permisos, CSRF/XSRF y pendientes de seguridad.
- `docs/05-delivery/DEPLOYMENT.md`: dominio, build, variables, DNS y checklist de publicación.
- `docs/08-qa/RESPONSIVE_CHECKLIST.md`: validación mobile-first.

## Enfoque Del Sitio Público

- El cliente revisará primero desde celular.
- La navegación pública debe funcionar cómodamente con dedo.
- No introducir estilos o layouts que dependan de desktop.
- La entrada al sistema debe mantenerse mediante `/login`.
- La aplicación privada debe permanecer separada visual y funcionalmente bajo `/app`.
- La ruta privada real del dashboard es `/app/dashboard`; no documentar `/dashboard` como ruta real.
