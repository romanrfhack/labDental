# Consolidación SEC-PERM-1

Fecha de revisión: 2026-10-06 (America/Mexico_City).

## Decisión

Única candidata a DEV: `codex/sec-perm-1-consolidated-20261006`, derivada de `codex/sec-perm-1` SHA `31d4541129828028a244083a46c702899a72889e`. Base DEV revisada: `25e1ec41109fc5cbca470a64afcfa3ed782bc44d`.

La variante `sec-perm-1-admin-permissions` SHA `b26f04283a95c763de875ea8c70b0a1cfb96f914` queda como antecedente conservado. No se elimina ni reescribe ninguna rama original.

| Aspecto | Base elegida | Alternativa |
| --- | --- | --- |
| Integración | Amplía servicio administrativo existente | Agrega servicio paralelo |
| Escrituras nuevas | PUT rol/usuario; reemplazo completo | PATCH y GET adicionales |
| Sesiones abiertas | Refresca principal y cookie antes de autorización; Angular refresca auth/me | Checker consulta BD; mayor divergencia entre claims y permisos efectivos |
| Guardas | Admin protegido, herencia/Allow/Deny, último usuario activo con users.manage | Servicio nuevo no incorpora la misma guarda de último administrador |
| EF | Migración generada, Designer y snapshot | Migración alternativa; no combinar ambas |
| Evidencia anterior | CI exitosa en e7dcafb; commits documentales posteriores | Última CI b26f042 falla en tests; etapas posteriores omitidas |

No se incorpora código de la alternativa: no se identificó una capacidad necesaria ausente en la base elegida. Se preservan endpoints previos; los nuevos PUT son aditivos respecto a DEV. No se promete compatibilidad con endpoints de una rama nunca integrada.

## Validación Del HEAD

El check `SEC-PERM-1 candidate validation` ejecuta restore, build y tests .NET, build Angular, `has-pending-model-changes`, SQL idempotente y `git diff --check`. Conserva TRX/SQL por SHA. No tiene permisos de escritura ni sincroniza documentación automáticamente.

Se amplían pruebas de restauración de herencia en sesión existente, rechazo de overrides inválidos/desconocidos/duplicados y `401/403/XSRF` de las nuevas escrituras. Se conserva cobertura de grant/revoke, Allow/Deny, Admin y seed de Repartidor.

Los resultados anteriores no certifican el HEAD consolidado. La evidencia vigente es el check y los artefactos asociados al SHA del PR. Migración aplicada y despliegue DEV comprobados en 37555582224; QA autenticado/visual y aceptación final siguen pendientes.

## Migración Y Operación

Única migración candidata: `20260908031302_AddUserPermissionOverrides`, crea `Security.UserPermissionOverrides` con PK `(UserId, PermissionId)`, FKs e índice por permiso. No reemplaza ni borra datos existentes. La migración alternativa `20260908035000` no debe aplicarse junto con ésta.

Antes del deploy DEV confirmar `__EFMigrationsHistory` y ausencia de una tabla creada por la variante alternativa; si existe un estado no documentado, detener y reconciliar antes de aplicar SQL. Preflight DEV 37554079112 (2026-10-07 UTC): seis migraciones esperadas, sin migración alternativa ni tabla de overrides; estado previo compatible.

Respaldar BD antes de migrar. Aplicar con el flujo DEV existente; verificar health local/público y smoke. Un rollback sólo de binarios conserva la nueva tabla aditiva; no ejecutar Down automáticamente ni eliminar overrides capturados.

El refresco de principal consulta roles/overrides en cada solicitud autenticada: favorece revocación inmediata con mayor costo de BD. No se introdujo caché que difiera revocaciones. El cambio obligatorio de contraseña temporal permanece fuera del cierre SEC-PERM-1 y es requisito de readiness.

## Pendientes Registrados

| ID | Estado | Acción / criterio de salida |
| --- | --- | --- |
| SEC-PERM-1-CI | PASS en 949808f; 164 tests | Todos los checks verdes y evidencia TRX/SQL por SHA |
| SEC-PERM-1-DEV | PASS: PR #10, migración, deploy 37555582224 y smoke anónimo | Revisar PR, migración/estado BD, integrar a dev y health correcto |
| SEC-PERM-1-UAT | BLOCKED: acceso autenticado no disponible | QA manual roles, usuario Allow/Deny/Heredado, sesión abierta, Admin y Clientes desktop/móvil |
| OPS-QA-1-USER | Cerrado documentalmente | Evidencia del 2026-09-07; no repetir como tarea pendiente previa |
| OPS-QA-1-PRINT | Pendiente hardware | Etiquetas 76 x 51 mm y 102 x 51 mm en impresora real |
| PROD-READY-1-AUTH | Pendiente | Cambio obligatorio de contraseña temporal o política equivalente aprobada |
| PROD-READY-1-OPS | Pendiente | BD, migraciones, backup/restore BD + imágenes, configuración, DNS/HTTPS, rollback y smoke |
| PROD-READY-1-502 | Pendiente diagnóstico | Dominio principal devuelve 502 en health/catalog/auth; comprobar configuración y upstream antes de publicar |
| PUBLIC-CONTENT | Pendiente cliente | Confirmar dirección, horarios, WhatsApp, redes y condiciones |
| CATALOG-FALLBACK | Opcional | Bloquear API y verificar fallback público |
| POST-RELEASE | Backlog | Excel, inventario/proveedores, reportes, WhatsApp, entregas avanzadas y ciclo de imágenes |

Dashboard BusinessTimeZone está implementado: el pendiente histórico de usar UTC para hoy de negocio queda cerrado. Inventario/proveedores continúan como placeholders.

## Límites De Evidencia

HTTP comprobado el 2026-10-06: DEV health/catalog público 200 y auth/me sin sesión 401; mismos endpoints del dominio principal 502. Evidencia inicial anterior al preflight: no se comprobó qué SHA ejecutaba el VPS ni UI autenticada. El preflight posterior confirmó BD y creación de backup; las credenciales Admin no están configuradas y la verificación de restauración sigue pendiente. La prueba de usuario limitado se toma del registro de septiembre.

Secuencia: CI/revisión candidata -> integración y UAT DEV -> cierre impresión -> PROD-READY-1 -> promoción explícita dev/main. Esta preparación no despliega ni promueve producción.

## Flujo de comprobación DEV

`sec-perm-1-dev-preflight.yml` usa el environment DEV y el acceso SSH del deploy; su resultado es requisito operativo antes del merge. El script `.github/scripts/sec-perm-1-dev-check.py` restringe la BD a DEV, no muestra credenciales ni datos de Clientes. El backup queda en el volumen SQL bajo `/var/opt/mssql/data` y requiere retención posterior. El preflight 37554079112 creó backup COPY_ONLY con CHECKSUM; RESTORE VERIFYONLY quedó bloqueado por falta de identidad SQL privilegiada identificable. La verificación y restauración completa siguen pendientes de readiness.

`deploy.yml` ejecuta el mismo script en modo post después del deploy DEV. QA autenticado depende de credenciales Admin válidas ya configuradas; no inventa ni resetea contraseñas. El usuario temporal se conserva desactivado como rastro de QA; no existe endpoint de borrado. QA de edición de rol hace escritura idempotente para no alterar permisos operativos; grant/revoke por rol sigue cubierto automáticamente y pendiente de UAT dirigido en navegador.


## 2026-10-06 — PR #10 integrado y verificación DEV

Merge `16b22fa4ad84749bed5b0c252e91671637b8b352`; candidata `949808f`, checks 37554298114/37554293279 verdes (164 tests). Deploy 37555301511 aplicó `20260908031302_AddUserPermissionOverrides`; historia contiene las siete migraciones esperadas, tabla presente con UserId/PermissionId/Effect. Health local y público 200. Workflow terminó en failure por UnboundLocalError del import local urllib en helper post-QA; se mueve import al módulo y se valida GET + escritura XSRF con dobles HTTP, sin BD/credenciales. Corrección 13662f5 desplegada; nueva ejecución 37555582224 exitosa. Contraseña SQL se transmite sólo por SQLCMDPASSWORD.

QA autenticado BLOCKED: sin credenciales Admin configuradas; formulario seguro cancelado. No se crearon usuarios QA ni cambiaron permisos operativos. QA visual Clientes desktop/móvil y grant/revoke en sesión real siguen abiertos; cobertura automática no los sustituye. Backup COPY_ONLY/CHECKSUM creado antes del merge; VERIFYONLY/restore pendientes. Producción no cambia.


## 2026-10-06 — Resultado final DEV SEC-PERM-1

Workflow [37555582224](https://github.com/romanrfhack/labDental/actions/runs/37555582224) exitoso sobre `13662f557c013cf9e3e1c461219907e45bb54731`. 164 tests correctos, builds backend/frontend correctos, SQL idempotente y deploy VPS correctos. Health local/público 200. Consulta real post-deploy: siete migraciones esperadas, incluida `20260908031302_AddUserPermissionOverrides`; tabla presente y columnas UserId/PermissionId/Effect. Smoke anónimo health/catalog 200, auth/me y customers 401. Comprobación adicional roles/dashboard sin sesión 401; navegador Clientes redirige a login.

QA autenticado y visual: **BLOCKED**, no PASS; faltan Admin/usuario no Admin, grant/revoke por rol, Allow/Deny/Heredado con sesión abierta, logout y Clientes desktop/móvil en DEV. Sin credenciales Admin configuradas; solicitud segura de login cancelada. No se alteraron permisos operativos ni se crearon usuarios de prueba. Backup previo COPY_ONLY/CHECKSUM creado; VERIFYONLY y restauración completa pendientes. No se promueve producción. Evidencia documental final no cambia código ni requiere nuevo despliegue.
