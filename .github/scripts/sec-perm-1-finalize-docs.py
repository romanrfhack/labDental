from pathlib import Path


def replace_section(path: Path, start_marker: str, end_marker: str, replacement: str) -> None:
    text = path.read_text(encoding="utf-8")
    start = text.index(start_marker)
    end = text.index(end_marker, start)
    path.write_text(text[:start] + replacement + text[end:], encoding="utf-8")


internal_path = Path("docs/01-product/internal-system.md")
internal_section = """## Usuarios, Roles Y Permisos

Estado: administración MVP de usuarios/roles operativa en DEV; `SEC-PERM-1` implementado en rama de trabajo y pendiente de integración/QA DEV.

- Rutas privadas:
  - `/app/admin/usuarios`: usuarios, roles asignados y overrides individuales.
  - `/app/admin/roles`: consulta y edición de permisos por rol.
- Endpoints adicionales SEC-PERM-1:
  - `PUT /api/admin/users/{id}/permissions`
  - `PUT /api/admin/roles/{id}/permissions`
- Herencia:
  - al crear o reasignar un usuario, los permisos no se copian; se resuelven desde sus roles;
  - permisos efectivos = unión de permisos de roles + `Allow` individuales - `Deny` individuales;
  - ausencia de override significa `Heredado`.
- UI de usuario:
  - muestra permiso efectivo, rol(es) de origen y clave técnica;
  - permite `Heredado / Permitir / Denegar` para usuarios no Admin.
- UI de rol:
  - permite editar permisos agrupados por módulo;
  - advierte cuántos usuarios heredan el cambio.
- Admin:
  - conserva todos los permisos;
  - el rol no puede reducirse desde UI/API;
  - usuarios con rol Admin no pueden degradarse mediante overrides individuales.
- Sesión:
  - los claims de rol/permisos se revalidan contra BD en cada solicitud autenticada;
  - si cambian, el principal y la cookie se renuevan antes de autorización;
  - el `permissionGuard` refresca `/api/auth/me` antes de navegación protegida.
- Seed:
  - Admin continúa recibiendo permisos nuevos;
  - `Repartidor` recibe `deliveries.view` y `deliveries.complete` sólo como baseline inicial y una ejecución posterior ya no pisa cambios administrativos;
  - Limited QA mantiene seed explícito Development-only.
- Persistencia:
  - `Security.UserPermissionOverrides` con PK `(UserId, PermissionId)` y `Effect` `Allow/Deny`.
- Seguridad heredada:
  - no existe delete de usuarios/roles;
  - no se expone `passwordHash`;
  - se conserva la regla de al menos un usuario activo con `users.manage`;
  - mutaciones continúan protegidas por XSRF.
- Pendiente `PROD-READY-1`:
  - force-change password en primer login o política equivalente aprobada.

Fuente funcional específica: `docs/01-product/user-roles-and-permissions.md`.
ADR: `docs/04-decisions/ADR-0012-role-user-permission-administration.md`.
QA: `docs/08-qa/users-roles-qa.md`.

"""
replace_section(
    internal_path,
    "## Usuarios Y Roles",
    "### Administración De Catálogo, Precios E Imágenes",
    internal_section,
)

auth_path = Path("docs/03-architecture/AUTH_FLOW.md")
auth = auth_path.read_text(encoding="utf-8")
old_baseline = "`SecuritySeed:EnsureBaselineOnStartup` queda activo en `Development` para asegurar el catálogo de permisos existentes, sincronizar permisos faltantes del rol `Admin` existente y asegurar el rol `Repartidor`. Ese baseline no lee ni escribe contraseñas y sincroniza `Repartidor` con `deliveries.view` y `deliveries.complete`."
new_baseline = "`SecuritySeed:EnsureBaselineOnStartup` queda activo en `Development` para asegurar el catálogo de permisos existentes, sincronizar permisos faltantes del rol `Admin` existente y asegurar que exista el rol `Repartidor`. El conjunto inicial de `Repartidor` es `deliveries.view` + `deliveries.complete`, pero desde `SEC-PERM-1` sólo se aplica al crear el rol; ejecuciones posteriores del baseline no sobrescriben permisos administrados desde la UI."
if old_baseline in auth:
    auth = auth.replace(old_baseline, new_baseline, 1)

auth_marker = "## SEC-PERM-1 — Permisos Dinámicos Por Rol Y Usuario"
if auth_marker not in auth:
    auth += """

## SEC-PERM-1 — Permisos Dinámicos Por Rol Y Usuario

Endpoints administrativos nuevos:

- `PUT /api/admin/roles/{id}/permissions`: requiere `roles.manage` y XSRF.
- `PUT /api/admin/users/{id}/permissions`: requiere `users.manage` y XSRF.

Modelo efectivo:

- permisos base = unión de `RolePermission` de todos los roles del usuario;
- `Security.UserPermissionOverrides` agrega `Allow` o `Deny` por `(UserId, PermissionId)`;
- `Deny` prevalece sobre herencia para usuarios no Admin;
- ausencia de override significa `Heredado`;
- Admin conserva todos los permisos y no admite degradación por edición de rol/override.

Actualización de sesión:

1. La cookie continúa transportando claims de rol y permiso.
2. Antes de autorización, `CookieAuthenticationEvents.OnValidatePrincipal` carga usuario, roles, permisos y overrides desde BD.
3. Usuario inexistente, inactivo o bloqueado invalida el principal.
4. Si los claims no coinciden con la seguridad persistida, la API reemplaza el principal y marca la cookie para renovación.
5. La policy de la misma solicitud usa ya el principal actualizado.
6. Angular ejecuta `/api/auth/me` desde `permissionGuard` antes de resolver una ruta protegida para mantener UI y backend alineados.

Implicación operativa: un grant/revoke por rol o un `Allow/Deny` individual surte efecto sin exigir logout/login. El costo aceptado en esta fase es una consulta del grafo de seguridad por request autenticado; puede evolucionarse a security-stamp/cache si el volumen futuro lo requiere sin cambiar el contrato funcional.

Evidencia automática: `tests/LaboratorioTlahuac.Api.Tests/PermissionAdministrationIntegrationTests.cs` y `SecuritySeederPermissionPreservationTests.cs`.
"""
auth_path.write_text(auth, encoding="utf-8")

log_path = Path("docs/IMPLEMENTATION_LOG.md")
log = log_path.read_text(encoding="utf-8")
log_marker = "## 2026-09-07 — SEC-PERM-1 — Administración De Roles Y Permisos"
if log_marker not in log:
    log += """

## 2026-09-07 — SEC-PERM-1 — Administración De Roles Y Permisos

Estado al registrar: **implementación de rama completada; pendiente integración y QA manual en DEV**.

### Decisión

- Mantener roles como fuente base; no copiar permisos al usuario.
- Permisos efectivos: roles + `Allow` individuales - `Deny` individuales.
- Admin protegido con todos los permisos.
- Cambios efectivos en sesiones existentes mediante revalidación de cookie contra BD.

### Backend / Persistencia

- Nueva entidad `UserPermissionOverride` y enum `UserPermissionOverrideEffect`.
- Nueva tabla `Security.UserPermissionOverrides` con PK compuesta y `Effect`.
- Migración EF `AddUserPermissionOverrides` generada con tooling, incluyendo Designer y snapshot.
- Nuevos endpoints `PUT /api/admin/roles/{id}/permissions` y `PUT /api/admin/users/{id}/permissions`.
- `AuthSessionService` calcula permisos efectivos con roles + overrides.
- `OnValidatePrincipal` refresca principal/cookie antes de autorización si la seguridad persistida cambió.
- Baseline seed deja de sobrescribir un `Repartidor` ya administrado; Admin conserva sincronización total.

### Frontend

- Roles deja de ser sólo lectura para roles no protegidos.
- Permisos agrupados por módulo y confirmación de usuarios afectados.
- Usuarios muestra permiso efectivo, origen y triestado `Heredado / Permitir / Denegar`.
- `permissionGuard` refresca `/api/auth/me` antes de navegación protegida.
- Corregido markup duplicado que renderizaba una segunda tabla de Clientes en desktop.

### QA Automático

- Build backend Release: correcto.
- Tests backend: correctos.
- Build Angular: correcto.
- `dotnet ef migrations has-pending-model-changes`: correcto.
- Cobertura nueva: grant/revoke por rol sin relogin, Allow/Deny por usuario sin relogin, Admin protegido y preservación de permisos de Repartidor frente al baseline seed.

### Estado Operativo

- `OPS-QA-1` Limited User real quedó validado previamente el 2026-09-07.
- Siguen pendientes las pruebas físicas de etiquetas 76x51 y 102x51.
- `SEC-PERM-1` requiere smoke manual en DEV después de merge/deploy.
- No se promovió `dev -> main` ni se tocó producción.
"""
    log_path.write_text(log, encoding="utf-8")

changelog_path = Path("docs/00-governance/changelog.md")
changelog = changelog_path.read_text(encoding="utf-8")
change_marker = "### 2026-09-07 — SEC-PERM-1"
if change_marker not in changelog:
    changelog += """

### 2026-09-07 — SEC-PERM-1

- Implementada administración de permisos por rol y overrides `Allow/Deny` por usuario en rama de trabajo.
- Agregada revalidación de permisos para sesiones ya abiertas.
- Admin queda protegido; Repartidor deja de ser sobrescrito por baseline una vez existente.
- Agregada migración `Security.UserPermissionOverrides` generada por EF.
- Corregida tabla duplicada de Clientes detectada durante OPS-QA-1.
- Pendiente integración y smoke DEV; sin promoción a `main`.
"""
    changelog_path.write_text(changelog, encoding="utf-8")
