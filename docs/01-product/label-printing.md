# Impresión De Etiquetas

Fuente funcional para el MVP de etiquetas de Laboratorio Dental Tláhuac. Este documento define tamaños, datos mínimos y estrategia inicial de impresión. No implementa código ni integración con impresoras.

## Objetivo

Reducir errores operativos conectando el trabajo físico con la orden digital mediante etiquetas impresas desde el navegador.

## Tamaños De Etiqueta

| Tamaño aproximado | Uso recomendado | Prioridad |
| --- | --- | --- |
| 51 x 25 mm | Etiqueta chica para folio/código y referencia rápida. | Posterior o complemento. |
| 76 x 51 mm / 3 x 2 in | Etiqueta interna de trabajo. | MVP recomendado. |
| 102 x 51 mm / 4 x 2 in | Etiqueta de entrega/repartidor. | MVP recomendado. |

La impresora esperada soporta etiquetas térmicas. El tamaño final debe validarse con la impresora real, el rollo disponible y la configuración del navegador.

## Uso Por Tamaño

### 51 x 25 mm

Uso: identificar piezas o bolsas pequeñas cuando solo se necesita folio visible.

Datos mínimos:

- Folio de orden.
- Paciente o iniciales, si cabe.
- Fecha de entrega planeada, si cabe.

No se recomienda como primera etiqueta única porque puede quedarse corta para trabajo, cliente y entrega.

### 76 x 51 mm / 3 x 2

Uso: etiqueta interna de trabajo recibida.

Datos mínimos:

- Folio de orden.
- Cliente.
- Paciente.
- Trabajo solicitado resumido.
- Color, si existe.
- Fecha de recepción.
- Fecha de entrega planeada, si existe.
- Estado actual.

Esta etiqueta va pegada al trabajo físico dentro del laboratorio.

### 102 x 51 mm / 4 x 2

Uso: etiqueta de entrega/repartidor.

Datos mínimos:

- Folio de orden.
- Cliente.
- Dirección de entrega.
- Contacto/teléfono/WhatsApp disponible.
- Paciente o referencia.
- Trabajo solicitado resumido.
- Fecha de entrega planeada.
- Repartidor asignado, cuando exista.

Dato opcional si administración lo requiere: saldo o indicación de pago pendiente. Debe validarse con el cliente antes de mostrarlo en etiqueta para evitar exponer información sensible innecesaria.

## MVP Mínimo Propuesto

Agregar desde el detalle existente `/app/ordenes/:id`:

- Botón `Imprimir etiqueta interna`.
- Botón `Imprimir etiqueta de entrega`.

Rutas privadas de impresión sugeridas bajo `/app`:

- `/app/ordenes/:id/etiqueta-trabajo`
- `/app/ordenes/:id/etiqueta-entrega`

Estas rutas deben reutilizar la orden existente. No deben crear un panel paralelo de órdenes.

## Estrategia Inicial De Impresión

La primera implementación debe usar impresión normal del navegador:

- Vista HTML dedicada por etiqueta.
- CSS `@media print`.
- Tamaños en milímetros.
- `@page` con `size` aproximado.
- Diseño de una sola etiqueta por vista.
- Botón o autoenfoque para llamar a `window.print()` solo si se decide en UX.

No usar todavía:

- Impresión directa a impresora.
- Servicio local de impresión.
- Drivers específicos.
- PDF obligatorio.
- Código QR o barcode si requiere dependencia nueva.

## Datos Desde El Sistema Actual

Datos disponibles de orden:

- Folio (`OrderNumber`).
- Cliente visible.
- Paciente.
- Doctor interno.
- Descripción de trabajo.
- Color.
- Fecha de recepción.
- Fechas de prueba.
- Fecha de entrega planeada.
- Estado.
- Observaciones.
- Total.

Datos disponibles de cliente:

- Dirección.
- Contacto.
- Teléfono.
- WhatsApp.
- Email.

Limitación actual: el detalle de orden no incluye dirección/contacto completo. La etiqueta de entrega deberá obtener esos datos ampliando el contrato de orden, usando un DTO de impresión o consultando el cliente por `CustomerId`.

## Limitaciones Del Enfoque Browser/CSS

- El navegador y el driver pueden agregar márgenes.
- La impresora puede requerir calibración de rollo/tamaño.
- `@page size` no siempre se respeta igual en todos los navegadores.
- La escala de impresión debe dejarse en 100% cuando sea posible.
- No hay confirmación automática de que la etiqueta se imprimió.
- No hay auditoría de impresión en el MVP si no se agrega registro explícito.
- No hay corte automático garantizado.
- No hay QR/barcode en el MVP si implica dependencia.

## Fases Posteriores

- QR o barcode para identificar orden desde celular.
- PDF simple si el navegador presenta problemas de tamaño o si se requiere archivo.
- Plantillas configurables por tamaño.
- Registro de impresiones.
- Impresión directa mediante servicio local, solo después de validar modelo de impresora, sistema operativo, drivers y red local.
- Integración con flujo de entrega para que la etiqueta de reparto incluya repartidor y salida.

## Criterio De Aceptación Para Fase 3.2

- Se imprime una etiqueta interna desde una orden existente.
- Se imprime una etiqueta de entrega desde una orden existente.
- Las rutas viven bajo `/app`.
- Las rutas privadas respetan permisos.
- No se crea panel duplicado de órdenes.
- No se agregan dependencias.
- No se modifica base de datos.
- No se generan migraciones.
