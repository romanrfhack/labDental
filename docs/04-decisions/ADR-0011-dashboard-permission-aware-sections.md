# ADR-0011 - Dashboard Con Secciones Condicionadas Por Permisos

## Estado

Aceptada para MVP.

## Contexto

El dashboard combina información operativa, financiera y de clientes. En el futuro puede haber roles que tengan acceso parcial a operación, cobranza o administración.

## Decisión

El dashboard requiere `reports.view` para acceder, pero sus secciones internas respetan permisos adicionales: `orders.view` para operación, `payments.view` para cobranza y `customers.view` para clientes.

La convención de respuesta del MVP es devolver la sección como `null` cuando el usuario no tiene el permiso específico.

## Consecuencias Positivas

- Evita exponer saldos a usuarios sin permiso financiero.
- Permite crecimiento a roles parciales.
- Mantiene separación entre operación y cobranza.
- Permite un único dashboard adaptativo.

## Consecuencias Negativas

- La respuesta del dashboard puede variar según permisos.
- La UI debe manejar secciones ausentes.
- Las pruebas deben cubrir combinaciones de permisos.

## Alternativas Consideradas

- Un dashboard único visible completo solo con `reports.view`.
- Dashboards separados por módulo.
- Dar `reports.view` como permiso implícito para todo.
- Ocultar solo en frontend sin control backend.
