# Plan QA Usuario Limitado

## Estado

Fase 2.6 implementada.

El repositorio ya tiene un mecanismo seguro para crear un usuario QA limitado local mediante el seed de seguridad existente. El mecanismo es solo para `Development`, esta desactivado por default, requiere habilitacion explicita y no guarda contrasenas en archivos versionados.

Validacion automatizada cubierta: seed directo con SQLite en memoria, login API con usuario limitado, `/api/auth/me` con permisos limitados, `/api/customers` permitido con `customers.view` y `/api/dashboard/summary` rechazado con `403` por falta de `reports.view`.

Validacion local real pendiente: no existen `LT_QA_LIMITED_EMAIL`, `LT_QA_LIMITED_PASSWORD` ni `LT_QA_LIMITED_FULL_NAME` en el proceso de Codex y no hay navegador/headless local disponible sin instalar dependencias.

Estado DEV Fase 3.0: el baseline UAT inicial en `https://dev.laboratoriodentaltlahuac.com` confirmó login QA y acceso autenticado a `/app/dashboard`, pero ese usuario tiene permisos suficientes para dashboard. Por lo tanto, la validación formal de usuario QA limitado y `/app/access-denied` en DEV sigue pendiente si aún no se prueba con una cuenta limitada real sin `reports.view`.

## Mecanismo Implementado

- Clase principal: `SecuritySeeder`.
- Configuracion: `SecuritySeed:LimitedQaUser`.
- Activacion de arranque: `SecuritySeed:LimitedQaUser:RunOnStartup=true`.
- Entorno permitido: solo `Development`.
- Rol local creado/actualizado: `Limited QA`.
- Usuario creado/actualizado: el email configurado para QA limitado.
- Password: se toma de user-secrets o variable de entorno, se hashea con `PasswordHasher<User>` y nunca se imprime.
- Permisos: se sincronizan exactamente contra una allowlist de claves existentes en `Permissions.All`.
- Admin: no se modifica cuando solo corre el seed QA limitado; si el email configurado pertenece a un usuario con rol Admin, el seed QA se omite de forma segura.
- Migraciones: no requiere migraciones.
- SQL manual: no se usa.

## Configuracion

User-secrets recomendados desde la raiz del repo:

```bash
dotnet user-secrets set SecuritySeed:LimitedQaUser:RunOnStartup true --project src/LaboratorioTlahuac.Api/LaboratorioTlahuac.Api.csproj
dotnet user-secrets set LT_QA_LIMITED_EMAIL "<email-local-qa>" --project src/LaboratorioTlahuac.Api/LaboratorioTlahuac.Api.csproj
dotnet user-secrets set LT_QA_LIMITED_PASSWORD "<password-local-seguro>" --project src/LaboratorioTlahuac.Api/LaboratorioTlahuac.Api.csproj
dotnet user-secrets set LT_QA_LIMITED_FULL_NAME "Usuario QA Limitado" --project src/LaboratorioTlahuac.Api/LaboratorioTlahuac.Api.csproj
dotnet user-secrets set SecuritySeed:LimitedQaUser:Permissions "customers.view" --project src/LaboratorioTlahuac.Api/LaboratorioTlahuac.Api.csproj
```

Equivalentes por configuracion jerarquica:

```text
SecuritySeed:LimitedQaUser:RunOnStartup=true
SecuritySeed:LimitedQaUser:Email=<email-local-qa>
SecuritySeed:LimitedQaUser:Password=<password-local-seguro>
SecuritySeed:LimitedQaUser:FullName=Usuario QA Limitado
SecuritySeed:LimitedQaUser:Permissions=customers.view
```

Equivalentes por variables de entorno para email, password y nombre:

```text
LT_QA_LIMITED_EMAIL=<email-local-qa>
LT_QA_LIMITED_PASSWORD=<password-local-seguro>
LT_QA_LIMITED_FULL_NAME=Usuario QA Limitado
SecuritySeed__LimitedQaUser__RunOnStartup=true
SecuritySeed__LimitedQaUser__Permissions=customers.view
```

No ejecutar `dotnet user-secrets list` como evidencia. No imprimir valores reales.

## Permisos Recomendados

Para validar `/app/access-denied` contra `/app/dashboard`, usar:

```text
SecuritySeed:LimitedQaUser:Permissions=customers.view
```

El usuario QA limitado debe poder iniciar sesion y consultar clientes, pero no debe tener:

```text
reports.view
```

Si `SecuritySeed:LimitedQaUser:Permissions` queda vacio o no existe, el usuario se crea sin permisos. El seed ignora claves desconocidas y solo aplica permisos existentes en `Permissions.All`.

## Validacion API

Resultado esperado con usuario QA limitado:

- `POST /api/auth/login`: `200`.
- `GET /api/auth/me`: `200`, sin `passwordHash`, con permisos limitados.
- `GET /api/customers`: `200` si se configuro `customers.view`.
- `GET /api/dashboard/summary`: `403` si no tiene `reports.view`.
- Sin sesion, `GET /api/dashboard/summary`: `401`.

La prueba automatizada `LimitedQaUserCanLoginAndIsForbiddenFromDashboardSummary` cubre este flujo sin usar secretos reales.

## Validacion Navegador

Pasos manuales con navegador real:

1. Confirmar que la API corre en `Development` y apunta a `ldt-labdental-sql` / `LaboratorioTlahuac_Dev`.
2. Configurar user-secrets sin imprimir valores reales.
3. Levantar la API para ejecutar el seed.
4. Apagar `SecuritySeed:LimitedQaUser:RunOnStartup`.
5. Entrar a `/login`.
6. Iniciar sesion con el usuario QA limitado.
7. Abrir `/app/dashboard`.
8. Confirmar redireccion a `/app/access-denied`.
9. Abrir `/app/clientes`.
10. Confirmar que carga si el usuario tiene `customers.view`.
11. Hacer logout.
12. Abrir `/app/dashboard` sin sesion.
13. Confirmar redireccion a `/login?returnUrl=%2Fapp%2Fdashboard`.

## Como Apagar El Seed

Despues de crear o sincronizar el usuario QA limitado local:

```bash
dotnet user-secrets set SecuritySeed:LimitedQaUser:RunOnStartup false --project src/LaboratorioTlahuac.Api/LaboratorioTlahuac.Api.csproj
```

Dejar el usuario limitado en la base local es aceptable para QA local si su password queda resguardada fuera de archivos versionados. No dejar la bandera encendida salvo que se este sincronizando de nuevo el usuario.

## Que NO Debe Hacerse

- No crear el usuario directamente en SQL Server.
- No usar `codex-cobranza-sql`.
- No alterar permisos del Admin para simular un usuario limitado.
- No versionar credenciales.
- No imprimir valores de `LT_QA_LIMITED_PASSWORD`, `LT_ADMIN_PASSWORD` ni `LDT_SQL_SA_PASSWORD`.
- No ejecutar `dotnet user-secrets list` como evidencia.
- No crear migraciones para este mecanismo.
- No exponer endpoint HTTP para crear usuarios QA.
- No convertir `/dashboard` en ruta privada real.
- No tocar `AuthService`, `auth.guard.ts`, `permission.guard.ts`, cookies o XSRF para esta validacion.

## Criterio De Aceptacion

- Con `Environment=Development`, bandera habilitada y secretos locales presentes, el usuario QA limitado se crea o actualiza de forma idempotente.
- Sin bandera, fuera de `Development` o con configuracion requerida incompleta, no se crea usuario QA limitado.
- El Admin existente no se altera.
- El usuario limitado puede iniciar sesion desde `/login`.
- `/api/auth/me` devuelve permisos esperados y no expone password hash.
- `/api/dashboard/summary` responde `403` con sesion limitada sin `reports.view`.
- `/api/dashboard/summary` responde `401` sin sesion.
- En navegador, `/app/dashboard` con usuario limitado debe terminar en `/app/access-denied`.
- `npm run build`, `dotnet build`, `dotnet test` y `git diff --check` pasan.
