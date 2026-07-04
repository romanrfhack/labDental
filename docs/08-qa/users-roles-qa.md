# QA Usuarios Y Roles - Fase 3.3.1

## Estado

Fase 3.3.1 ejecutada como QA de seguridad, validacion tecnica y preparacion de despliegue DEV para la administracion MVP de usuarios y roles.

Resultado: sin hallazgos bloqueantes por codigo, pruebas, build ni HTTP local sin sesion. Queda pendiente la validacion visual/operativa completa en DEV con credenciales reales de Admin y usuario limitado.

## Alcance Revisado

Rutas privadas:

- `/app/admin/usuarios`
- `/app/admin/roles`

Ambas rutas viven bajo `/app`, heredan `authGuard` y usan `permissionGuard`.

Endpoints revisados:

| Endpoint | Permiso esperado | Estado QA |
| --- | --- | --- |
| `GET /api/admin/users` | `users.manage` | Protegido por backend; sin sesion devuelve `401`. |
| `GET /api/admin/users/{id}` | `users.manage` | Protegido por backend; sin sesion devuelve `401`. |
| `POST /api/admin/users` | `users.manage` | Protegido por backend y XSRF; sin sesion con XSRF valido devuelve `401`. |
| `PUT /api/admin/users/{id}` | `users.manage` | Protegido por backend y XSRF; sin sesion con XSRF valido devuelve `401`. |
| `PATCH /api/admin/users/{id}/status` | `users.manage` | Protegido por backend y XSRF; sin sesion con XSRF valido devuelve `401`. |
| `PATCH /api/admin/users/{id}/roles` | `users.manage` | Protegido por backend y XSRF; sin sesion con XSRF valido devuelve `401`. |
| `POST /api/admin/users/{id}/temporary-password` | `users.manage` | Protegido por backend y XSRF; sin sesion con XSRF valido devuelve `401`. |
| `GET /api/admin/roles` | `roles.manage` | Protegido por backend; sin sesion devuelve `401`. |
| `GET /api/admin/roles/{id}` | `roles.manage` | Protegido por backend; sin sesion devuelve `401`. |

## Seguridad

### Appsettings

- `appsettings.json` conserva `SecuritySeed:EnsureBaselineOnStartup=false` y `SecuritySeed:RunOnStartup=false`.
- `appsettings.Development.json` activa `SecuritySeed:EnsureBaselineOnStartup=true` solo para Development.
- La revision redaccionada de `ConnectionStrings` confirma cadenas locales sin `Password=` ni `User Id=`.
- `SecuritySeed:Admin:Password` en `appsettings.json` esta vacio.
- No se detectaron contrasenas reales guardadas en `appsettings.json` ni `appsettings.Development.json`.

### Permisos

- Usuarios requiere `users.manage` en frontend y backend.
- Roles requiere `roles.manage` en frontend y backend.
- Usuario sin sesion: validado por HTTP local contra los nueve endpoints admin con resultado `401`.
- Usuario autenticado sin permisos admin: cubierto por pruebas API existentes para `/api/admin/users` y `/api/admin/roles` con resultado `403`; validacion visual con usuario limitado real queda pendiente en DEV.
- Admin: pruebas API confirman que puede listar/crear usuarios, asignar roles, activar/desactivar usuarios y consultar roles.

### Password Temporal

- No se devuelve en respuestas de creacion ni detalle.
- No aparece en listados.
- No se registra en logs de aplicacion; la busqueda encontro uso en DTOs, hashing y pruebas, no en `Console.*` ni mensajes de `LoggerMessage`.
- La UI no genera ni muestra la contrasena temporal como resultado; el Admin la captura explicitamente en campos `type=password`.
- Riesgo DEV/UAT: el Admin debe comunicar la contrasena inicial por un canal seguro fuera del sistema.
- Pendiente obligatorio antes de operacion productiva amplia: implementar force-change password en el siguiente login o flujo equivalente.

### Admin Intacto

- El backend evita desactivar la propia cuenta.
- El backend evita dejar el sistema sin al menos un usuario activo con `users.manage`.
- La proteccion es por permiso, no por nombre de rol: si hay otro usuario activo con `users.manage`, el sistema permite reasignar roles.
- No existe delete de usuarios ni roles en esta fase.

### Rol Repartidor

- `Repartidor` queda preparado por baseline de seguridad en `Development`.
- En Fase 3.3.1 fue validado como rol base sin permisos activos en pruebas API.
- No recibe `orders.view`, `orders.edit` ni acceso amplio a ordenes completas.
- Los permisos reales de entregas se definiran en Fase 3.4. Sugeridos: `deliveries.view` y `deliveries.update`, todavia no implementados.

## QA Funcional DEV

Checklist para ejecutar en DEV despues de desplegar rama `dev`:

1. Iniciar sesion como Admin en `/login`.
2. Abrir `/app/admin/usuarios`.
3. Confirmar que carga sin errores de consola y lista usuarios.
4. Crear usuario QA con email, nombre, contrasena temporal y rol `Repartidor`.
5. Confirmar que la contrasena temporal no aparece en listado ni en detalle despues de guardar.
6. Editar nombre/email del usuario creado.
7. Cambiar roles del usuario creado.
8. Desactivar y reactivar el usuario creado.
9. Actualizar contrasena temporal desde accion explicita.
10. Abrir `/app/admin/roles`.
11. Confirmar que roles carga en modo solo lectura y comunica `Solo lectura`.
12. Confirmar que cada rol muestra permisos.
13. Confirmar que `Repartidor` aparece sin permisos activos.
14. Iniciar sesion con usuario sin `users.manage`/`roles.manage`.
15. Confirmar que `/app/admin/usuarios` y `/app/admin/roles` terminan en `/app/access-denied`.
16. Cerrar sesion y confirmar que entrar a `/app/admin/usuarios` sin sesion redirige a `/login?returnUrl=...`.

## Validacion Local Ejecutada

- `docker ps --filter name=ldt-labdental-sql`: `ldt-labdental-sql` activo en `14336`.
- `docker ps --filter name=codex-cobranza-sql`: sin contenedor activo; no se uso.
- API local levantada con `dotnet run --project src/LaboratorioTlahuac.Api/LaboratorioTlahuac.Api.csproj`.
- Angular local levantado con `npm start` en `http://localhost:4200/`.
- `GET /health`: `200`.
- `GET /api/auth/csrf`: `204` con token XSRF presente.
- Sin sesion, todos los endpoints admin revisados devolvieron `401`; para mutables se envio XSRF valido.
- `GET http://127.0.0.1:4200/login`: `200` shell Angular.
- `GET http://127.0.0.1:4200/app/admin/usuarios`: `200` shell Angular.
- `GET http://127.0.0.1:4200/app/admin/roles`: `200` shell Angular.

Nota: el `200` de rutas privadas por `curl` solo confirma entrega del shell SPA. La validacion real de guards y pantallas requiere navegador con Angular ejecutando y credenciales reales.

## Build/Test

- `npm run build` desde `src/LaboratorioTlahuac.Web`: correcto. Warning no bloqueante: bundle inicial `531.04 kB`, excede budget `500.00 kB` por `31.04 kB`.
- `dotnet build`: correcto con 0 errores y 2 warnings `NU1903` conocidos por `SQLitePCLRaw.lib.e_sqlite3` 2.1.11 en tests.
- `dotnet test`: correcto; Domain 1/1, Application 1/1, API 110/110.
- `git diff --check`: correcto.
- Busquedas obligatorias ejecutadas. Patrones sensibles revisados con salida limitada a archivos.

## Riesgos Y Pendientes

- Validacion visual real en DEV con Admin queda pendiente.
- Validacion de `/app/access-denied` con usuario limitado real sin `users.manage`/`roles.manage` queda pendiente en DEV; por pruebas API, una sesion sin permisos admin recibe `403`.
- Implementar force-change password antes de produccion.
- Definir si roles/permisos editables seran necesarios y bajo que reglas de seguridad.
- Resolver o ajustar el warning de budget inicial en una fase de optimizacion frontend; no bloquea DEV porque esta por debajo del `maximumError` de `1MB`.
- Disenar Fase 3.4 de entregas/repartidor con modelo, endpoints, permisos y UI mobile-first.

## Preparacion DEV

Si no aparecen bloqueantes en la validacion humana:

1. Hacer commit de Fase 3.3 + Fase 3.3.1.
2. Push a `dev`.
3. Desplegar DEV sin migraciones nuevas.
4. Validar los pasos del checklist QA funcional DEV.
5. Registrar evidencia de Admin, usuario limitado, rol `Repartidor` y password temporal antes de iniciar Fase 3.4.
