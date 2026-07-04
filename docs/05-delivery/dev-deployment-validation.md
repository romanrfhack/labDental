# Validación De Despliegue DEV - Baseline UAT

## Resumen

- Fase: 3.0 - cierre formal del despliegue DEV y baseline UAT.
- Actualización 2026-07-04: cierre DEV de Fase 3.4.1 backend delivery MVP y optimización frontend lazy loading.
- Fecha de validación: 2026-07-02, America/Mexico_City.
- URL DEV: `https://dev.laboratoriodentaltlahuac.com`.
- Rama desplegada: `dev`.
- Upstream local verificado antes de documentar: `origin/dev`.
- Alcance: documentación y validación del ambiente DEV ya publicado.
- Resultado general: DEV queda registrado como baseline UAT inicial validado y, desde la actualización 2026-07-04, con Delivery API desplegada/protegida y lazy loading frontend desplegado.

Fase 3.0 no implementó funcionalidad nueva. La actualización 2026-07-04 es documental y registra un deploy ya ejecutado; no modifica frontend funcional, backend, auth, guards, cookies, XSRF, endpoints, base de datos, migraciones, dependencias ni despliegue real desde Codex.

## Actualización 2026-07-04 - Fase 3.4.1 DEV

Validación informada para el despliegue DEV de Fase 3.4.1 backend delivery MVP y optimización frontend lazy loading:

| Punto | Resultado |
| --- | --- |
| Commit desplegado | `e4c28205c6b866ab0d71edb13c49164100340b0d` |
| GitHub Actions run | `28712956106` |
| Resultado deploy DEV | `success` |
| URL DEV | `https://dev.laboratoriodentaltlahuac.com` |
| `GET /health` | `200` |
| `GET /api/deliveries` sin sesión | `401` |
| Delivery API | Desplegada y protegida por sesión/permisos. |
| Lazy loading frontend | Desplegado. |
| Initial bundle warning | Resuelto; build inicial pasó de `535.62 kB` a `304.19 kB` sin subir budgets. |

El cambio de `/api/deliveries` sin sesión de `404` anterior a `401` confirma que `DeliveryEndpoints` ya están publicados en DEV y protegidos. La migración `WorkOrderDeliveries` ya está aplicada o la base DEV está al día para este despliegue.

Pendiente específico posterior al deploy: validación manual Admin en DEV del flujo delivery. Siguiente fase técnica: Fase 3.4.2 - UI admin de entregas desde órdenes.

## Alcance Validado

Validación manual confirmada por el responsable del proyecto:

| Punto | Resultado |
| --- | --- |
| Sitio público `/` | OK. |
| Sitio público `/servicios` | OK. |
| Sitio público `/catalogo` | OK. |
| Sitio público `/contacto` | OK. |
| `/login` | OK. |
| Login con usuario QA | OK. |
| `/app/dashboard` autenticado | OK. |
| Usuario sin sesión navegando rutas públicas | OK. |
| Usuario sin sesión intentando `/app/dashboard` | Redirige a `/login`. |
| `/dashboard` raíz | OK: no es ruta privada real. |
| VPS DEV desplegado desde rama `dev` | OK. |

## Validación `curl` Sin Credenciales

Ejecución local desde el entorno de Codex, sin cookies ni credenciales:

| URL | Resultado `curl` |
| --- | --- |
| `https://dev.laboratoriodentaltlahuac.com/` | `200` |
| `https://dev.laboratoriodentaltlahuac.com/servicios` | `200` |
| `https://dev.laboratoriodentaltlahuac.com/catalogo` | `200` |
| `https://dev.laboratoriodentaltlahuac.com/contacto` | `200` |
| `https://dev.laboratoriodentaltlahuac.com/login` | `200` |
| `https://dev.laboratoriodentaltlahuac.com/app/dashboard` | `200` |

### Diferencia Entre `curl` Y Navegador

La aplicación pública/privada se sirve como SPA Angular. Por eso, `curl` puede recibir `200` y el shell HTML de Angular incluso para rutas privadas como `/app/dashboard`.

Ese `200` no prueba que el guard permita entrar sin sesión. La validación real de guards requiere navegador ejecutando Angular. En esta fase se registra como evidencia manual confirmada que un usuario sin sesión al intentar `/app/dashboard` es redirigido a `/login`.

## Rutas Públicas

- `/`
- `/servicios`
- `/catalogo`
- `/contacto`
- `/login`

`/login` sigue siendo una ruta pública de entrada al sistema.

## Rutas Privadas

- `/app`
- `/app/dashboard`

`/app/dashboard` sigue siendo el dashboard privado real. `/dashboard` raíz no es ruta privada real.

## Login QA

Resultado manual confirmado:

- Usuario QA puede autenticarse desde `/login`.
- Usuario QA autenticado puede entrar a `/app/dashboard`.
- No se documentaron ni imprimieron credenciales.

Este resultado valida una cuenta QA con permisos suficientes para dashboard. No cierra por sí mismo la validación formal del usuario QA limitado sin `reports.view`.

## Redirección Sin Sesión

Resultado manual confirmado:

- Usuario sin autenticar puede navegar rutas públicas.
- Usuario sin autenticar al intentar `/app/dashboard` es redirigido a `/login`.

La redirección exacta puede incluir `returnUrl` según el flujo del frontend, pero el resultado funcional esperado es conservar `/login` como entrada pública y mantener `/app/dashboard` privado.

## Estado De Base DEV

No se inspeccionó directamente la base DEV durante Fase 3.0 para evitar tocar base de datos, secretos o despliegue real.

Estado inferido por validación manual:

- Login QA funciona.
- `/app/dashboard` autenticado carga.
- DEV está operativo para baseline UAT inicial.
- Actualización 2026-07-04: `GET /api/deliveries` sin sesión responde `401` en DEV, por lo que la API de entregas está publicada y protegida. La migración `WorkOrderDeliveries` ya está aplicada o la base DEV está al día.

No se ejecutaron migraciones, seeds, consultas directas ni cambios de datos desde Codex en esta fase documental.

## Servicios Y VPS

Documentado en esta fase:

- DEV está publicado en VPS bajo `https://dev.laboratoriodentaltlahuac.com`.
- El despliegue DEV corresponde a la rama `dev`.
- Actualización 2026-07-04: GitHub Actions run `28712956106` desplegó correctamente el commit `e4c28205c6b866ab0d71edb13c49164100340b0d` a DEV.

No están documentados en el repositorio los nombres de servicios systemd, rutas del servidor, usuario del sistema, reverse proxy exacto ni comandos operativos del VPS. No se inspeccionaron ni modificaron servicios del VPS en esta fase.

## Pendientes Antes De Producción

- Confirmar dirección real del laboratorio.
- Confirmar horarios.
- Confirmar WhatsApp real.
- Aprobar precios finales 2026.
- Aprobar condición comercial `Anticipo 50%`.
- Aprobar condición comercial `Trabajos urgentes +40%`.
- Completar imágenes faltantes para `Servicios prostodónticos`.
- Cerrar validación de usuario QA limitado y `/app/access-denied` en DEV si aún no queda formalmente validada con cuenta limitada sin `reports.view`.
- Validar manualmente con Admin en DEV el flujo delivery desplegado.
- Definir el siguiente incremento funcional.
- Definir checklist productivo final: DNS, HTTPS productivo, variables, base productiva, respaldos, monitoreo, CORS, cookies seguras y rollback.

## Validaciones Técnicas De Cierre

- `npm run build` desde `src/LaboratorioTlahuac.Web`: correcto.
- `dotnet build`: correcto, 0 errores; reportó 2 warnings `NU1903` por vulnerabilidad conocida de `SQLitePCLRaw.lib.e_sqlite3` 2.1.11 en el proyecto de tests.
- `dotnet test`: correcto; Domain 1/1, Application 1/1 y API 101/101.
- `git diff --check`: correcto.
- Búsquedas obligatorias de rutas, URL DEV, variables sensibles, `ConnectionStrings` y `codex-cobranza-sql`: ejecutadas.

Actualización 2026-07-04:

- `npm run build` desde `src/LaboratorioTlahuac.Web`: correcto; initial total `304.19 kB`, sin warning de budget.
- `dotnet build`: correcto; 0 errores y 2 warnings `NU1903` conocidos por `SQLitePCLRaw.lib.e_sqlite3` 2.1.11 en tests.
- `dotnet test`: correcto; Domain 1/1, Application 1/1 y API 121/121.
- `git diff --check`: correcto.

## Confirmaciones De Alcance

- Solo documentación modificada.
- No se modificó código.
- No se instaló ninguna dependencia.
- No se crearon migraciones.
- No se imprimieron secretos.
- No se ejecutó `dotnet user-secrets list`.
- No se usó `codex-cobranza-sql`.
- No se hicieron commits.
- `/login` sigue público.
- `/app` y `/app/dashboard` siguen privados.
- `/dashboard` no es ruta privada real.

## Siguiente Fase Recomendada

Fase 3.4.2 - UI admin de entregas desde órdenes, con validación manual Admin en DEV como pendiente inmediato del cierre 3.4.1. En paralelo, cerrar usuario QA limitado y `/app/access-denied` en DEV si aún no queda formalmente validado con cuenta limitada real sin `reports.view`.
