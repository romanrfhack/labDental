# Plan De Trabajo Vigente

Última actualización: **2026-08-22 — DOC-SYNC-1**.

Este documento responde una sola pregunta: **¿qué sigue y en qué orden?**

No sustituye el historial técnico; concentra la secuencia vigente acordada para evitar abrir fases en paralelo sin cierre claro.

## Estado De Partida

- DEV publicado desde `dev` en `https://dev.laboratoriodentaltlahuac.com`.
- Sitio público aprobado visualmente.
- Catálogo administrable e imágenes persistentes validados end-to-end.
- Entregas/repartidor operativos en MVP.
- Usuarios/roles operativos en DEV.
- PUB-UX-4 integrado en `dev` mediante PR #8.
- Producción todavía no publicada.

## Secuencia Acordada

### 1. DOC-SYNC-1 — Reconciliación Documental

Estado: **se cierra con la integración de este cambio a `dev`**.

Objetivo:

- Sincronizar estado, roadmap, sitio público, fases comerciales, ambientes y backup/restore.
- Eliminar referencias que aún marcaban PUB-UX-2/3/4 como pendientes.
- Registrar un único siguiente paso.

Criterio de salida:

- `PROJECT_STATUS.md` y `ROADMAP.md` describen el estado real.
- El sitio público figura aprobado en DEV.
- Producción figura explícitamente pendiente.
- El siguiente paso queda identificado como `OPS-QA-1`.

### 2. OPS-QA-1 — QA Operativo Pendiente

Estado: **siguiente fase después de DOC-SYNC-1**.

Objetivo:

Cerrar evidencia manual que requiere hardware/usuarios reales antes de preparar producción.

Alcance obligatorio:

#### Impresora térmica

- Etiqueta interna `76 x 51 mm`.
- Etiqueta entrega `102 x 51 mm`.
- Escala 100%.
- Márgenes/headers/footers del navegador desactivados.
- Validar orientación, corte, offset, contraste y nitidez.
- Registrar evidencia y ajuste de driver si aplica.

#### Usuario limitado

- Cuenta DEV real sin `reports.view`.
- Login correcto.
- `/app/dashboard` termina en `/app/access-denied`.
- Un módulo permitido, por ejemplo `/app/clientes`, sigue disponible si tiene `customers.view`.
- Logout y acceso sin sesión conservan comportamiento esperado.

Cobertura opcional:

- Forzar falla/bloqueo de `GET /api/catalog/public` y confirmar fallback local de `/catalogo`.

Criterio de salida:

- Evidencia manual registrada.
- Cualquier hallazgo real corregido o clasificado.
- Sin deuda operativa bloqueante conocida para iniciar readiness productivo.

### 3. PROD-READY-1 — Preparación Para Producción

Estado: **pendiente**.

Objetivo:

Convertir `dev` en un release candidate seguro y operable.

Bloques:

1. **Seguridad de usuarios**
   - Definir/implementar cambio obligatorio de contraseña temporal en primer acceso, o política equivalente formalmente aprobada.
   - Revisar cookies `Secure`, sesión y permisos.

2. **Base de datos**
   - SQL Server productivo.
   - Cadena de conexión en secrets.
   - Migraciones revisadas.
   - Backup previo a migración.

3. **Backup y restore**
   - Backup de BD.
   - Backup de `shared/catalog-images`.
   - Restauración probada en ambiente no productivo.
   - Retención y ubicación protegida definidas.

4. **Infraestructura**
   - Environment `production` de GitHub.
   - Secrets/variables requeridos.
   - Root de aplicación y servicio API productivos.
   - Health local/público.
   - Rollback verificado.

5. **Dominio**
   - DNS de `laboratoriodentaltlahuac.com`.
   - Decisión `www`.
   - HTTPS.
   - Redirecciones canónicas.

6. **QA release candidate**
   - Sitio público.
   - Login/Admin.
   - Repartidor.
   - Catálogo administrable.
   - Upload/render de imagen.
   - `401/403` sin redirecciones HTML.

Criterio de salida:

- Checklist productivo sin P0/P1 abiertos.
- SHA de release candidate identificado.
- Rollback y restore conocidos.
- Autorización explícita para promover a `main`.

### 4. PROD-RELEASE-1 — Primera Publicación Productiva

Estado: **pendiente**.

Dependencia: `PROD-READY-1` cerrada.

Flujo:

- PR `dev -> main`.
- Revisión final del diff acumulado.
- Habilitar conscientemente deploy productivo.
- Deploy.
- Migraciones según procedimiento aprobado.
- Health checks.
- Smoke productivo.
- Confirmación DNS/HTTPS.
- Aceptación o rollback.

Criterio de salida:

- `https://laboratoriodentaltlahuac.com` operativo.
- Sistema privado accesible solo con sesión.
- Datos productivos respaldables.
- Evidencia de release registrada.

### 5. POST-PROD-1 — Estabilización

Estado: **pendiente**.

Alcance:

- Monitoreo inicial.
- Revisión de logs/errores.
- Confirmar backups automáticos.
- Primera prueba periódica de restore.
- Seguimiento de crecimiento de imágenes.
- Feedback de usuarios reales.
- Cierre de aceptación de primera ronda.

## Después De Producción

Priorizar una sola línea funcional a la vez:

- Migración Excel.
- Inventario/proveedores.
- Reportes administrativos.
- Automatizaciones/WhatsApp.
- Entregas avanzadas con QR/escaneo/evidencia.
- Ciclo de vida avanzado de imágenes.

## Regla De Control

- No fusionar `dev -> main` antes de cerrar `PROD-READY-1`.
- No abrir una fase funcional mayor mientras `OPS-QA-1` o readiness productivo tengan pendientes bloqueantes.
- Cada cierre debe actualizar `PROJECT_STATUS.md`, `ROADMAP.md` y la fuente específica afectada.
