# ADR-0006: Modelo Propio De Seguridad Con Roles, Permisos Y Seed Admin

## Estado

Aceptada para MVP.

## Contexto

El sistema requiere permisos granulares desde el inicio, aunque el único rol inicial sea Admin.

## Decisión

Crear modelo de `User`, `Role`, `Permission`, `UserRole` y `RolePermission`. El usuario Admin inicial se crea mediante configuración segura y seed idempotente.

## Consecuencias Positivas

- Permite crecer a roles futuros.
- Evita dependencias en nombres de rol para validar acceso.
- Facilita trazabilidad de permisos.
- Permite mantener Admin con todos los permisos iniciales.

## Consecuencias Negativas

- Requiere mantener correctamente permisos y claims.
- Cambios de permisos pueden requerir refrescar sesión o re-login.
- El seed debe operarse con cuidado en producción.

## Alternativas Consideradas

- Validar solo por rol Admin.
- Usar JWT en localStorage.
- Crear usuarios manualmente directo en base de datos.
