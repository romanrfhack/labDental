# Plan De Entrega Y Aceptación

## Plan De Entrega

La primera ronda se entrega por fases para reducir riesgo y permitir validación progresiva con el cliente.

1. Confirmar alcance comercial y prioridades.
2. Preparar sitio web corporativo con materiales aprobados.
3. Completar módulo de repartidores, entregas y etiquetas.
4. Ejecutar QA funcional de la primera ronda.
5. Capacitar usuarios clave.
6. Desplegar a producción.
7. Obtener aceptación de cierre.

## Entregables Por Fase

| Fase | Entregables |
| --- | --- |
| Fase 0 - Planeación y documentación | Documentación base, alcance, roadmap y paquete comercial. |
| Fase 1 - Sistema administrativo MVP | Sistema privado con login, roles, clientes, órdenes, pagos, saldos, dashboard y QA local. |
| Fase 2 - Sitio web corporativo | Sitio público en dominio acordado, contenido institucional, servicios, ubicación y contacto. |
| Fase 3 - Repartidores y etiquetas | Etiquetas, impresión básica, escaneo desde celular, entregas, evidencia e historial. |
| Fase 4 - QA, capacitación y despliegue | Validación funcional, capacitación básica, puesta en producción y cierre. |

## Criterios De Aceptación Por Fase

### Fase 0

- El alcance está documentado.
- Las exclusiones están claras.
- El cliente puede revisar una propuesta ejecutiva.

### Fase 1

- Un Admin puede iniciar sesión.
- Se pueden registrar clientes, doctores, clínicas y doctores internos.
- Se pueden crear y consultar órdenes.
- Se pueden registrar pagos y abonos.
- Los saldos se calculan correctamente.
- El dashboard muestra información operativa y financiera básica.
- La QA local está documentada.

### Fase 2

- El sitio web carga correctamente.
- El contenido fue aprobado por el cliente.
- Los datos de contacto, ubicación y servicios son correctos.
- El sitio público está separado del sistema administrativo privado.

### Fase 3

- Una orden puede identificarse con etiqueta QR o código.
- La etiqueta puede imprimirse desde navegador.
- Una entrega puede asignarse a un repartidor.
- El repartidor puede consultar entregas desde celular.
- El repartidor puede escanear el QR o código.
- La entrega puede cerrarse con firma o fotografía de recibido.
- El historial queda registrado por orden y por repartidor.

### Fase 4

- Los flujos principales fueron validados con el cliente.
- No existen bloqueos críticos conocidos para producción.
- Los usuarios clave recibieron capacitación básica.
- El sistema quedó desplegado.
- El cliente acepta el cierre de la primera ronda.

## Condiciones Para Pasar A Producción

- Alcance contratado confirmado.
- Dominio/DNS disponibles o coordinados.
- Accesos técnicos necesarios entregados.
- Variables de entorno y credenciales productivas configuradas.
- Base de datos productiva preparada.
- Revisión de seguridad básica completada.
- QA funcional sin bloqueos críticos.
- Respaldos o plan de respaldo definido.
- Usuario Admin productivo definido por el cliente.
- Validación de hardware si se solicita impresión local más allá del navegador.

## Validación Con Cliente

La validación debe realizarse con usuarios reales o responsables operativos del laboratorio. La aceptación debe enfocarse en flujos de negocio, no solo en pantallas:

- Alta y consulta de cliente.
- Creación y seguimiento de orden.
- Registro de pago y revisión de saldo.
- Consulta de dashboard.
- Impresión de etiqueta.
- Asignación y cierre de entrega.
- Captura de evidencia.
- Consulta de historial.

## Capacitación

La capacitación básica incluye una sesión de operación para usuarios clave:

- Acceso al sistema.
- Clientes, doctores y clínicas.
- Órdenes de trabajo.
- Pagos, abonos y saldos.
- Dashboard.
- Etiquetas y entregas.
- Buenas prácticas de operación.

## Cierre De Primera Ronda

La primera ronda se considera cerrada cuando:

- Los entregables incluidos están disponibles.
- El cliente validó los flujos principales.
- El sistema está desplegado.
- Se entregó documentación básica.
- Se registran pendientes o mejoras como backlog posterior.
- Los cambios fuera de alcance quedan separados para una siguiente propuesta.
