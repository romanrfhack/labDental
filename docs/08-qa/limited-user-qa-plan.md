# Plan QA Usuario Limitado

## Objetivo

Definir un mecanismo seguro para validar `/app/access-denied` con una sesion autenticada sin permisos suficientes, sin alterar el Admin existente, sin SQL manual, sin migraciones y sin guardar contrasenas en archivos versionados.

## Riesgos

- Crear usuarios reales de QA sin controles puede dejar credenciales activas fuera de Development.
- Reutilizar el seed Admin para permisos limitados sin separacion clara puede reducir la confianza en el rol Admin.
- Crear datos por SQL manual puede saltarse reglas de dominio, hashing de password, normalizacion de email y auditoria basica.
- Versionar credenciales o imprimir secretos en consola filtraria acceso local.
- Un usuario limitado mal definido podria tener permisos suficientes y no validar realmente `/app/access-denied`.

## Opciones Evaluadas

### Opcion 1 - Seed QA limitado solo Development

Resumen: extender en una fase futura el mecanismo de seed para crear un usuario QA limitado solo cuando el entorno sea `Development`, este explicitamente habilitado y existan credenciales en user-secrets o variables de entorno.

- Seguridad: alta si queda desactivado por default, revisa `Environment=Development`, no imprime password y no corre en produccion.
- Esfuerzo: bajo a medio; reutiliza entidades, `PasswordHasher<User>`, permisos existentes y transaccion del seed.
- Utilidad QA: alta; permite validar login, `/app/access-denied`, diferencia entre `401` y `403`, y navegacion privada con sesion limitada.
- Riesgo residual: requiere tocar backend de forma controlada en una fase posterior y probar que no se activa fuera de Development.

### Opcion 2 - Esperar modulo de usuarios/roles

Resumen: no crear mecanismo especial ahora; validar `/app/access-denied` cuando exista administracion real de usuarios y roles.

- Seguridad: muy alta en el corto plazo porque evita codigo adicional y datos QA de seguridad.
- Esfuerzo: nulo ahora.
- Utilidad QA: baja para la fase actual; mantiene incompleta la evidencia manual de usuario sin permiso.
- Riesgo residual: posterga una validacion importante hasta una fase mayor.

### Opcion 3 - Script local de QA

Resumen: crear un comando local controlado que use servicios/repositorios existentes para crear un usuario limitado sin SQL directo ni credenciales versionadas.

- Seguridad: media a alta si usa servicios existentes, no imprime secretos y se limita al entorno local.
- Esfuerzo: medio; requiere disenar comando, parametros, validaciones, documentacion y pruebas.
- Utilidad QA: alta para entornos locales y repetibles.
- Riesgo residual: puede duplicar logica del seed o quedar fuera del flujo normal si no se mantiene.

## Opcion Recomendada

Recomendada: Opcion 1, seed QA limitado solo Development, como backlog tecnico inmediato y sin implementacion en Fase 2.5.

Motivo: es la opcion mas util para cerrar evidencia manual pronto y la mas alineada con el mecanismo existente de seed, siempre que se agreguen compuertas explicitas de entorno, habilitacion y credenciales externas. La Opcion 2 queda como fallback si se decide no tocar backend antes del modulo de usuarios/roles. La Opcion 3 solo conviene si se prefiere no ampliar el seed.

## Diseno Propuesto

- El usuario QA limitado solo puede crearse cuando `IWebHostEnvironment.IsDevelopment()` sea verdadero.
- El flujo debe estar desactivado por default.
- La habilitacion debe requerir una bandera explicita, por ejemplo `SecuritySeed:QaLimited:Enabled=true`.
- Las credenciales deben venir de user-secrets o variables de entorno, nunca de `appsettings*.json` versionados.
- La password nunca debe imprimirse, registrarse ni persistirse en texto plano.
- El usuario debe crearse o actualizarse usando `User.Create(...)`, `PasswordHasher<User>` y relaciones `UserRole`/`RolePermission`, no SQL directo.
- Para validar `/app/access-denied`, el perfil recomendado es un usuario activo sin permisos de producto, o con una allowlist minima que no incluya el permiso de la ruta a probar.
- La prueba manual recomendada es iniciar sesion con el usuario limitado e intentar abrir `/app/dashboard`; si el usuario no tiene `reports.view`, debe terminar en `/app/access-denied`.

## User-Secrets O Variables Propuestas

Nombres propuestos para una fase posterior:

```text
SecuritySeed:QaLimited:Enabled=true
LDT_QA_LIMITED_EMAIL=<email-local-qa>
LDT_QA_LIMITED_PASSWORD=<password-local-seguro>
LDT_QA_LIMITED_FULL_NAME=Usuario QA Limitado
SecuritySeed:QaLimited:Permissions=
```

Equivalentes de entorno cuando aplique:

```text
SecuritySeed__QaLimited__Enabled=true
LDT_QA_LIMITED_EMAIL=<email-local-qa>
LDT_QA_LIMITED_PASSWORD=<password-local-seguro>
LDT_QA_LIMITED_FULL_NAME=Usuario QA Limitado
SecuritySeed__QaLimited__Permissions=
```

`SecuritySeed:QaLimited:Permissions` debe ser opcional y aceptar solo una allowlist de claves existentes en `Permissions.All`. Para validar `/app/access-denied` contra `/app/dashboard`, debe quedar vacia o no incluir `reports.view`.

## Restricciones De Seguridad

- No activar en Production, Staging ni entornos no Development.
- No crear o modificar Admin.
- No usar SQL manual.
- No crear migraciones para este mecanismo.
- No guardar contrasenas en `appsettings`, documentos, scripts versionados ni archivos de ejemplo con valores reales.
- No ejecutar `dotnet user-secrets list` como evidencia.
- No imprimir secretos ni payloads con contrasenas.
- No usar `codex-cobranza-sql` ni bases de otros proyectos.
- No exponer endpoint HTTP para crear usuarios QA.
- No dejar la bandera activa despues de crear el usuario, salvo que se documente una razon local temporal.

## Criterio De Aceptacion

- Con `Environment=Development`, bandera habilitada y secretos locales presentes, el usuario QA limitado se crea o actualiza de forma idempotente.
- Sin bandera o fuera de Development, no se crea ni modifica el usuario QA limitado.
- El Admin existente conserva `Permissions.All` y no se altera.
- El usuario limitado puede iniciar sesion desde `/login`.
- `/api/auth/me` devuelve el usuario limitado sin password hash y con permisos esperados.
- Al abrir una ruta sin permiso, por ejemplo `/app/dashboard` sin `reports.view`, el frontend redirige a `/app/access-denied`.
- El endpoint backend correspondiente responde `403` si hay sesion sin permiso y `401` si no hay sesion.
- `npm run build`, `dotnet build`, `dotnet test` y `git diff --check` pasan.
- La documentacion de QA registra que no se imprimieron secretos y que no se uso SQL manual.

## Que NO Debe Hacerse

- No crear el usuario directamente en SQL Server.
- No alterar permisos del Admin para simular un usuario limitado.
- No versionar credenciales.
- No imprimir valores de `LDT_QA_LIMITED_PASSWORD`, `LT_ADMIN_PASSWORD` ni `LDT_SQL_SA_PASSWORD`.
- No crear migraciones o endpoints nuevos para esta validacion.
- No usar fixtures de pruebas como mecanismo para la base local real.
- No convertir `/dashboard` en ruta privada real.
