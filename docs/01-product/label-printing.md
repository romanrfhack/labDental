# Impresión De Etiquetas

Fuente funcional para el MVP de etiquetas de Laboratorio Dental Tláhuac. Fase 3.2 implementa impresión desde navegador/CSS para órdenes existentes; no implementa integración directa con impresoras, PDF, QR/barcode ni campos nuevos de entrega.

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

## MVP Fase 3.2 Implementado

Desde el detalle existente `/app/ordenes/:id`:

- Acción `Etiqueta interna`.
- Acción `Etiqueta entrega`.

Rutas privadas implementadas bajo `/app`:

- `/app/ordenes/:id/etiqueta-trabajo`
- `/app/ordenes/:id/etiqueta-entrega`

Estas rutas reutilizan la orden existente con `WorkOrderService.getById()`. No crean panel paralelo de órdenes, no agregan endpoints y quedan protegidas por la zona privada `/app` más `permissionGuard` con `orders.view`.

Cada pantalla de etiqueta incluye:

- Botón `Imprimir`, que ejecuta `window.print()`.
- Botón `Volver a la orden`.
- Estados de carga, error, no encontrado y sin permiso.
- CSS de impresión que oculta navegación, topbar y botones en `@media print`.

## QA Técnico Fase 3.2.1

Validado el 2026-07-02 como preparación de despliegue DEV desde rama `dev`.

Resultado:

- Las rutas de etiqueta siguen bajo `/app` y protegidas por sesión más `orders.view`.
- Las etiquetas reutilizan `WorkOrderService.getById()` y el contrato existente de órdenes.
- No se agregaron endpoints, migraciones, dependencias, QR/barcode, PDF ni integración directa con driver/SDK.
- `window.print()` sigue siendo el mecanismo de impresión.
- `@media print` oculta navegación, topbar, encabezados de pantalla y acciones.
- `@page` existe para 76 x 51 mm y 102 x 51 mm.
- El diseño es legible sin depender de color; usa texto negro, fondo blanco, bordes y jerarquía tipográfica.
- Textos largos se compactan o restringen para no romper layout.
- Dirección y contacto faltantes se muestran como textos seguros pendientes, no como datos inventados.
- No hay hallazgos técnicos bloqueantes para desplegar a DEV.

Limitación de ambiente local: no hay navegador/headless disponible sin instalar dependencias, por lo que la prueba visual automatizada y la impresión física quedan pendientes en DEV con navegador real e impresora térmica.

Validaciones ejecutadas: `npm run build`, `dotnet build`, `dotnet test`, búsquedas obligatorias y `git diff --check` final documentado en `docs/08-qa/label-printing-qa.md`.

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

## Datos Incluidos En Fase 3.2

### Etiqueta Interna 76 x 51 mm

Incluye:

- Texto de marca `LDT`.
- Texto claro `Etiqueta interna`.
- Folio / número de orden.
- Cliente.
- Doctor interno si existe.
- Paciente.
- Fecha de recepción.
- Fecha de entrega planeada.
- Estado.
- Color si existe.
- Trabajo solicitado resumido.
- Observaciones breves si existen.

### Etiqueta Entrega 102 x 51 mm

Incluye:

- Texto de marca `LDT`.
- Texto claro `Entrega`.
- Folio / número de orden.
- Cliente.
- Paciente o referencia.
- Trabajo solicitado resumido.
- Fecha de entrega planeada.
- Estado.
- `Dirección pendiente`.
- `Contacto pendiente`.
- `Recibe: __________________`.
- `Firma: __________________`.

## Datos Faltantes

La orden actual no expone en el detalle:

- Dirección de entrega.
- Contacto/teléfono/WhatsApp/email.
- Repartidor asignado.
- Salida a ruta.
- Persona real que recibe.
- Firma digital o evidencia.

Decisión Fase 3.2: no consultar `GET /api/customers/{id}` desde la etiqueta de entrega para no convertir una ruta protegida por `orders.view` en una pantalla que pueda fallar por falta de `customers.view`. La fase futura debe resolverlo con un DTO mínimo de impresión/entrega o ampliación controlada del contrato de orden.

## Limitaciones Del Enfoque Browser/CSS

- El navegador y el driver pueden agregar márgenes.
- La impresora puede requerir calibración de rollo/tamaño.
- `@page size` no siempre se respeta igual en todos los navegadores.
- La escala de impresión debe dejarse en 100% cuando sea posible.
- No hay confirmación automática de que la etiqueta se imprimió.
- No hay auditoría de impresión en el MVP si no se agrega registro explícito.
- No hay corte automático garantizado.
- No hay QR/barcode en el MVP si implica dependencia.
- No se valida desde código que la impresora haya respetado el tamaño físico; debe probarse con el equipo real.

## Cómo Probar Con Impresora Real

1. Entrar a DEV o local con Admin o usuario con `orders.view`.
2. Abrir `/app/ordenes`.
3. Abrir una orden existente.
4. Clic en `Etiqueta interna`.
5. Confirmar que la vista previa muestra una etiqueta 76 x 51 mm.
6. Clic en `Imprimir`.
7. En el diálogo del navegador, seleccionar la impresora térmica y el tamaño de etiqueta equivalente 3 x 2 in / 76 x 51 mm.
8. Usar escala 100% si el navegador lo permite y desactivar encabezados/pies del navegador.
9. Confirmar que navegación, topbar y botones no aparecen en impresión.
10. Volver a la orden.
11. Clic en `Etiqueta entrega`.
12. Repetir impresión con tamaño 4 x 2 in / 102 x 51 mm.
13. Revisar legibilidad física, cortes, márgenes, orientación y calibración del rollo.

## Fases Posteriores

- QR o barcode para identificar orden desde celular.
- PDF simple si el navegador presenta problemas de tamaño o si se requiere archivo.
- Plantillas configurables por tamaño.
- Registro de impresiones.
- Impresión directa mediante servicio local, solo después de validar modelo de impresora, sistema operativo, drivers y red local.
- Integración con flujo de entrega para que la etiqueta de reparto incluya repartidor y salida.
- Etiqueta chica 51 x 25 mm para folio/código.
- Etiquetas de entrega con repartidor asignado, salida a ruta y datos reales de contacto/dirección.

## Criterio De Aceptación Para Fase 3.2

- Se imprime una etiqueta interna desde una orden existente.
- Se imprime una etiqueta de entrega desde una orden existente.
- Las rutas viven bajo `/app`.
- Las rutas privadas respetan permisos.
- No se crea panel duplicado de órdenes.
- No se agregan dependencias.
- No se modifica base de datos.
- No se generan migraciones.
