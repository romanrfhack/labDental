# Hallazgos Conocidos

Última actualización: **2026-09-07**.

## Críticos

- No hay hallazgos críticos abiertos conocidos.

## Altos

- No hay hallazgos altos abiertos conocidos.

## Medios

- No existe suite E2E de navegador completa para todos los flujos Angular; se conserva build automático y QA manual dirigido en DEV.
- La zona horaria formal de negocio para métricas de "hoy", vencidas y próximos 7 días sigue pendiente. Actualmente se usa fecha UTC del servidor.
- La cadena local por defecto con `Trusted_Connection=True` requiere adaptación explícita en Linux; QA local histórica usó SQL Server Docker con credenciales de desarrollo.

## Bajos

- La demo requiere captura manual de datos; no existe seed demo automático por decisión de alcance.
- El primer recorrido automatizado histórico de QA usó un dato inválido para una orden vencida; el dato de prueba se corrigió.

## Corregidos

- Dashboard: órdenes canceladas ya no alimentan conteos financieros pendientes.
- Clientes: durante `OPS-QA-1` se detectó que una segunda representación de la tabla se renderizaba sin formato debajo de la tabla desktop. La causa fue markup `<thead>/<tbody>` duplicado fuera de su contenedor. `SEC-PERM-1` elimina ese markup y separa correctamente tabla desktop/lista móvil. Pendiente sólo confirmación visual después del deploy DEV de la rama.

## Pendientes Operativos

- Prueba física de etiquetas `76 x 51 mm` y `102 x 51 mm` con impresora térmica real.
- QA manual en DEV de edición de permisos por rol y overrides por usuario después de integrar `SEC-PERM-1`.
- Force-change password o política equivalente antes de producción.

## Backlog No Bloqueante Del MVP

- fallback público de catálogo con API bloqueada/offline: prueba opcional;
- migración histórica de Excel;
- inventario/proveedores;
- reportes ampliados;
- automatizaciones/WhatsApp;
- entregas avanzadas y ciclo de vida avanzado de imágenes.
