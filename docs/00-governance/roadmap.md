# Roadmap

## Fase 0 - Documentación Y Definición

- Objetivo: acordar alcance, reglas, arquitectura inicial y decisiones base.
- Alcance: documentación inicial, validación del flujo operativo, stack preliminar, estrategia de migración.
- Fuera de alcance: código, base de datos, deploy productivo, migración de datos.
- Criterio de salida: documentación revisada y backlog inicial aprobado.

## Fase 1 - MVP Operativo

- Objetivo: operar registros nuevos sin depender del Excel.
- Alcance: login, rol Admin, clientes/doctores/clínicas, órdenes, pagos, saldos calculados, dashboard básico y sitio público básico.
- Fuera de alcance: inventario automático, facturación, reportes avanzados, WhatsApp, migración perfecta del histórico.
- Criterio de salida: el laboratorio puede registrar órdenes nuevas, pagos y saldos desde la plataforma.

## Fase 2 - Migración Del Excel

- Objetivo: importar o consultar datos históricos con control de inconsistencias.
- Alcance: análisis de hojas, mapeo de clientes, mapeo de filas a órdenes, importación en modo revisión.
- Fuera de alcance: corrección automática de datos ambiguos, garantía de saldos históricos perfectos.
- Criterio de salida: registros históricos importados o marcados para revisión sin afectar operación nueva.

## Fase 3 - Inventario Y Proveedores

- Objetivo: controlar materiales, existencias y proveedores básicos.
- Alcance: proveedores, materiales, entradas, salidas, ajustes, mermas, stock mínimo y alertas.
- Fuera de alcance: costeo avanzado por orden, compras automatizadas, integración contable.
- Criterio de salida: el inventario básico puede auditar movimientos y niveles mínimos.

## Fase 4 - Reportes Administrativos

- Objetivo: dar visibilidad administrativa de operación, cobranza y productividad.
- Alcance: reportes por cliente, estado de órdenes, pagos, saldos, entregas y periodos.
- Fuera de alcance: inteligencia predictiva, BI externo, tableros contables completos.
- Criterio de salida: administración puede consultar métricas clave sin revisar el Excel manualmente.

## Fase 5 - Automatizaciones Y WhatsApp

- Objetivo: reducir seguimiento manual mediante notificaciones y automatizaciones.
- Alcance: recordatorios, mensajes operativos, posibles integraciones WhatsApp y plantillas.
- Fuera de alcance: campañas masivas, CRM completo, atención automática sin supervisión.
- Criterio de salida: automatizaciones probadas con consentimiento y trazabilidad.

## Regla De Actualización

Cada cierre de fase debe actualizar este roadmap, el changelog y los documentos afectados.
