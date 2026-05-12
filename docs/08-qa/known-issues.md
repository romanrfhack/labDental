# Hallazgos Conocidos

Fecha: 2026-05-11.

## Críticos

- No hay hallazgos críticos abiertos después de la corrección aplicada durante QA.

## Altos

- No hay hallazgos altos abiertos.

## Medios

- No existe runner frontend no interactivo. El frontend se validó con `npm run build` y smoke HTTP de rutas, pero no hay prueba automatizada de navegador para flujos Angular.
- La zona horaria formal de negocio para métricas de "hoy", vencidas y próximos 7 días sigue pendiente. Actualmente se usa fecha UTC del servidor.
- La cadena local por defecto con `Trusted_Connection=True` no conectó a SQL Server en este entorno Linux. La QA se ejecutó con SQL Server Docker y cadena explícita con usuario `sa`.

## Bajos

- La demo requiere captura manual de datos. No existe seed demo automático, por decisión de alcance y para evitar datos permanentes accidentales.
- El primer recorrido automatizado de QA usó un dato inválido para una orden vencida: fecha de recepción posterior a entrega. Se corrigió el dato de prueba y el flujo pasó.

## Corregidos Durante QA

- El dashboard excluía órdenes canceladas de `totalReceivable`, pero todavía las incluía en `ordersWithPendingBalanceCount` y `unpaidOrdersCount`. Se corrigió para que las órdenes `Cancelled` no alimenten conteos financieros.

## Pendientes No Bloqueantes

- Ejecutar demo con el cliente y capturar feedback real.
- Decidir prioridad comercial entre sitio web público y módulos operativos posteriores.
- Planear migración del Excel en fase separada.
- Definir alcance de repartidores y etiquetas si se priorizan.
- Mantener fuera de alcance inventario, proveedores, CFDI, facturación y reportes avanzados hasta nueva fase contratada.
