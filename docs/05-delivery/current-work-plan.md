# Plan De Trabajo Vigente

Última actualización: **2026-09-07 — SEC-PERM-1 en implementación**.

Este documento responde una sola pregunta: **¿qué sigue y en qué orden?**

No sustituye el historial técnico; concentra la secuencia vigente acordada para evitar abrir fases en paralelo sin cierre claro.

## Estado De Partida

- DEV publicado desde `dev` en `https://dev.laboratoriodentaltlahuac.com`.
- Sitio público aprobado visualmente.
- Catálogo administrable e imágenes persistentes validados end-to-end.
- Entregas/repartidor operativos en MVP.
- Usuarios/roles operativos en DEV.
- Validación manual de usuario limitado de `OPS-QA-1` **completada el 2026-09-07**.
- Pruebas físicas de etiquetas térmicas todavía pendientes por disponibilidad de hardware.
- `SEC-PERM-1` autorizado para completarse antes del readiness productivo.
- Producción todavía no publicada.

## Secuencia Acordada

### 1. DOC-SYNC-1 — Reconciliación Documental

Estado: **cerrada**.

Cierre integrado a `dev` mediante PR #9.

### 2. OPS-QA-1 — QA Operativo Pendiente

Estado: **en curso; sólo queda pendiente la validación física de impresora**.

Objetivo:

Cerrar evidencia manual que requiere hardware/usuarios reales antes de preparar producción.

#### Usuario limitado — completado

Validación real en DEV ejecutada el **2026-09-07**:

- cuenta real sin `reports.view`;
- login correcto;
- `customers.view` efectivo;
- `/app/clientes` disponible;
- `/app/dashboard` termina en `/app/access-denied`;
- `GET /api/customers` autenticado devuelve `200`;
- `GET /api/dashboard/summary` autenticado devuelve `403`;
- el mismo endpoint sin credenciales devuelve `401`;
- `/api/auth/me` confirmó `customers.view=true` y `reports.view=false`;
- logout invalida la sesión;
- configuración temporal del seed Limited QA retirada del entorno después de la prueba.

#### Impresora térmica — pendiente

- Etiqueta interna `76 x 51 mm`.
- Etiqueta entrega `102 x 51 mm`.
- Escala 100%.
- Márgenes/headers/footers del navegador desactivados.
- Validar orientación, corte, offset, contraste y nitidez.
- Registrar evidencia y ajuste de driver si aplica.

Cobertura opcional:

- Forzar falla/bloqueo de `GET /api/catalog/public` y confirmar fallback local de `/catalogo`.

Criterio de salida:

- Evidencia física de las dos etiquetas registrada.
- Cualquier hallazgo real corregido o clasificado.
- Sin deuda operativa bloqueante conocida para iniciar readiness productivo.

### 3. SEC-PERM-1 — Administración De Roles Y Permisos

Estado: **autorizada y en implementación en rama de trabajo sobre `dev`**.

Objetivo:

Completar la administración segura de permisos antes del primer release productivo, conservando los roles como fuente base de herencia y permitiendo excepciones controladas por usuario.

Alcance:

1. **Permisos por rol**
   - edición desde `/app/admin/roles`;
   - usuario nuevo hereda automáticamente permisos de sus roles;
   - rol Admin protegido y con todos los permisos;
   - cambios de Repartidor no deben ser sobrescritos por el baseline seed.

2. **Permisos por usuario**
   - overrides individuales `Allow` / `Deny`;
   - estado normal `Heredado`;
   - permiso efectivo = permisos de roles + Allows - Denies;
   - mostrar rol/origen del permiso en UI;
   - usuarios con rol Admin no admiten overrides individuales.

3. **Sesión y autorización**
   - cambios de rol/override deben hacerse efectivos sin esperar un nuevo login;
   - cookie/principal se refresca contra el estado de seguridad persistido;
   - backend sigue siendo la fuente autoritativa;
   - frontend refresca `/api/auth/me` al proteger navegación por permiso.

4. **Persistencia**
   - tabla `Security.UserPermissionOverrides`;
   - migración EF revisada e idempotente;
   - modelo/snapshot sincronizados.

5. **QA**
   - herencia al crear usuarios;
   - grant/revoke por rol;
   - allow/deny individual;
   - protección Admin;
   - efecto en sesión existente;
   - `401/403` conservados.

Hallazgo QA asociado:

- corregir render duplicado de tabla desktop/responsive en Clientes detectado durante `OPS-QA-1`.

Criterio de salida:

- backend/frontend compilan;
- tests automáticos pasan;
- migración/snapshot EF coherentes;
- UI validada en DEV;
- Admin no puede quedar degradado por configuración accidental;
- no se promueve a `main` como parte de esta fase.

### 4. PROD-READY-1 — Preparación Para Producción

Estado: **pendiente**.

Dependencias:

- `SEC-PERM-1` cerrada.
- `OPS-QA-1` sin deuda operativa bloqueante; la prueba física de impresora debe cerrarse antes de autorizar producción.

Objetivo:

Convertir `dev` en un release candidate seguro y operable.

Bloques:

1. **Seguridad de usuarios**
   - definir/implementar cambio obligatorio de contraseña temporal en primer acceso, o política equivalente formalmente aprobada;
   - revisar cookies `Secure`, sesión y permisos.

2. **Base de datos**
   - SQL Server productivo;
   - cadena de conexión en secrets;
   - migraciones revisadas;
   - backup previo a migración.

3. **Backup y restore**
   - backup de BD;
   - backup de `shared/catalog-images`;
   - restauración probada en ambiente no productivo;
   - retención y ubicación protegida definidas.

4. **Infraestructura**
   - Environment `production` de GitHub;
   - secrets/variables requeridos;
   - root de aplicación y servicio API productivos;
   - health local/público;
   - rollback verificado.

5. **Dominio**
   - DNS de `laboratoriodentaltlahuac.com`;
   - decisión `www`;
   - HTTPS;
   - redirecciones canónicas.

6. **QA release candidate**
   - sitio público;
   - Login/Admin;
   - Repartidor;
   - catálogo administrable;
   - upload/render de imagen;
   - permisos por rol/usuario;
   - `401/403` sin redirecciones HTML.

Criterio de salida:

- checklist productivo sin P0/P1 abiertos;
- SHA de release candidate identificado;
- rollback y restore conocidos;
- autorización explícita para promover a `main`.

### 5. PROD-RELEASE-1 — Primera Publicación Productiva

Estado: **pendiente**.

Dependencia: `PROD-READY-1` cerrada.

Flujo:

- PR `dev -> main`;
- revisión final del diff acumulado;
- habilitar conscientemente deploy productivo;
- deploy;
- migraciones según procedimiento aprobado;
- health checks;
- smoke productivo;
- confirmación DNS/HTTPS;
- aceptación o rollback.

### 6. POST-PROD-1 — Estabilización

Estado: **pendiente**.

Alcance:

- monitoreo inicial;
- revisión de logs/errores;
- confirmar backups automáticos;
- primera prueba periódica de restore;
- seguimiento de crecimiento de imágenes;
- feedback de usuarios reales;
- cierre de aceptación de primera ronda.

## Después De Producción

Priorizar una sola línea funcional a la vez:

- Migración Excel.
- Inventario/proveedores.
- Reportes administrativos.
- Automatizaciones/WhatsApp.
- Entregas avanzadas con QR/escaneo/evidencia.
- Ciclo de vida avanzado de imágenes.

## Regla De Control

- **No fusionar `dev -> main` antes de cerrar `PROD-READY-1`.**
- `OPS-QA-1` permanece abierto hasta terminar las pruebas físicas de las dos etiquetas.
- `SEC-PERM-1` puede desarrollarse mientras se espera el hardware porque reduce deuda de seguridad previa a producción.
- No abrir otra fase funcional mayor mientras `OPS-QA-1`, `SEC-PERM-1` o readiness productivo tengan pendientes bloqueantes.
- Cada cierre debe actualizar `PROJECT_STATUS.md`, `ROADMAP.md` y la fuente específica afectada.
