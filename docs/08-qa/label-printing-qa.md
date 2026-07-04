# QA Impresión De Etiquetas

Fuente QA para Fase 3.2 y Fase 3.2.1 - MVP de impresión de etiquetas desde órdenes existentes y preparación de despliegue DEV.

## Alcance

Validar que el usuario pueda abrir etiquetas privadas desde `/app/ordenes/:id`, imprimir desde navegador y volver a la orden sin romper rutas existentes.

Fase 3.2.1 no implementa funcionalidad nueva. El objetivo es confirmar técnicamente la implementación, documentar limitaciones visuales del entorno local y dejar listo el checklist para prueba física con impresora térmica real en DEV.

## Rutas Validadas

- `/app/ordenes/:id/etiqueta-trabajo`
- `/app/ordenes/:id/etiqueta-entrega`

Resultado Fase 3.2.1:

- Ambas rutas viven bajo `/app`.
- Ambas rutas heredan sesión requerida desde `authGuard` en la zona privada.
- Ambas rutas usan `permissionGuard` con `orders.view`.
- `/login` sigue público.
- `/app` y `/app/dashboard` siguen privados.
- `/dashboard` no es ruta privada real.

## Tamaños De Etiqueta Validados

- Etiqueta interna: 76 x 51 mm.
- Etiqueta entrega: 102 x 51 mm.

Confirmación técnica:

- Existe `@page { size: 76mm 51mm; margin: 0; }` para etiqueta interna.
- Existe `@page { size: 102mm 51mm; margin: 0; }` para etiqueta entrega.
- Los SCSS también declaran `@page ldt-work-label` y `@page ldt-delivery-label`.

## Datos Incluidos

### Etiqueta Interna

- LDT.
- Texto `Etiqueta interna`.
- Folio / número de orden.
- Cliente.
- Doctor interno si existe.
- Paciente.
- Fecha de recepción.
- Fecha de entrega planeada.
- Estado.
- Color si existe.
- Trabajo solicitado.
- Observaciones breves si existen.

### Etiqueta Entrega

- LDT.
- Texto `Entrega`.
- Folio / número de orden.
- Cliente.
- Paciente o referencia.
- Trabajo solicitado.
- Fecha de entrega planeada.
- Estado.
- `Dirección pendiente`.
- `Contacto pendiente`.
- `Recibe: __________________`.
- `Firma: __________________`.

## Validación Técnica Fase 3.2.1

Ejecutada el 2026-07-02 en esta rama:

- Las rutas nuevas están bajo `/app`.
- Las rutas requieren sesión y `orders.view`.
- Las pantallas usan `WorkOrderService.getById()` y el modelo existente `WorkOrderDetail`.
- No se agregaron endpoints.
- No se agregaron migraciones.
- No se instalaron dependencias.
- Los botones de impresión usan `window.print()`.
- `@media print` oculta navegación, topbar, encabezado de pantalla y acciones.
- La etiqueta no depende de colores para ser legible: usa fondo blanco, texto negro, bordes y jerarquía tipográfica.
- Los textos largos se compactan con `compact()` y se restringen con `overflow`, `text-overflow` o `-webkit-line-clamp`.
- Si faltan dirección/contacto, la etiqueta de entrega muestra textos seguros: `Dirección pendiente` y `Contacto pendiente`; no inventa datos.
- No se consultó `GET /api/customers/{id}` desde etiquetas para no exigir `customers.view`.
- No se tocaron `AuthService`, guards, cookies, XSRF, endpoints, rutas privadas, migraciones, base de datos, deploy ni dependencias.
- No se ejecutó `dotnet user-secrets list`.
- No se usó `codex-cobranza-sql`.
- No se hicieron commits.

## Resultado Build/Test

Ejecutado el 2026-07-02:

- `npm run build` desde `src/LaboratorioTlahuac.Web`: correcto.
- `dotnet build` desde raíz: correcto con 0 errores y 2 warnings `NU1903` conocidos por `SQLitePCLRaw.lib.e_sqlite3` 2.1.11 en tests.
- `dotnet test` desde raíz: correcto; Domain 1/1, Application 1/1 y API 101/101.
- Búsquedas obligatorias de rutas, etiquetas, `@page`, `window.print`, dashboard, login, patrones sensibles, `ConnectionStrings` y `codex-cobranza-sql`: ejecutadas.
- Las búsquedas de patrones sensibles se ejecutaron con salida limitada a archivos para no imprimir valores.
- `git diff --check`: correcto después de actualizar documentación.

## Validación Visual Local

No se ejecutó validación visual automatizada local porque el entorno no tiene navegador/headless disponible sin instalar dependencias:

- `chromium`: no disponible en `PATH`.
- `google-chrome`: no disponible en `PATH`.
- `chromium-browser`: no disponible en `PATH`.
- El frontend no declara Playwright ni Puppeteer en `package.json`/`package-lock.json`.

Por esta limitación, la revisión visual queda pendiente para navegador real en DEV.

## Checklist Manual En Navegador

1. Entrar con Admin o usuario con `orders.view`.
2. Abrir `/app/ordenes`.
3. Abrir una orden existente.
4. Confirmar que existen acciones `Etiqueta interna` y `Etiqueta entrega`.
5. Abrir `Etiqueta interna`.
6. Confirmar tamaño visual objetivo 76 x 51 mm y texto `Etiqueta interna`.
7. Confirmar datos: LDT, folio, cliente, paciente, recepción, entrega, estado, trabajo y observaciones si existen.
8. Clic en `Imprimir`.
9. Confirmar en vista previa de impresión que no aparecen navegación, topbar ni botones.
10. Volver a la orden.
11. Abrir `Etiqueta entrega`.
12. Confirmar tamaño visual objetivo 102 x 51 mm y texto `Entrega`.
13. Confirmar datos: LDT, folio, cliente, paciente/referencia, entrega, dirección pendiente, contacto pendiente, recibe y firma.
14. Clic en `Imprimir`.
15. Confirmar en vista previa de impresión que no aparecen navegación, topbar ni botones.

## Prueba Con Impresora Térmica En DEV

Pendiente de ejecución en DEV con el equipo real.

Checklist físico:

1. Confirmar que DEV está desplegado desde rama `dev`.
2. Entrar a `https://dev.laboratoriodentaltlahuac.com/login`.
3. Iniciar sesión con usuario autorizado con `orders.view`.
4. Abrir una orden existente en `/app/ordenes/:id`.
5. Imprimir etiqueta interna.
6. Confirmar que la etiqueta interna queda dentro del papel 76 x 51 mm sin cortar folio, cliente, paciente ni trabajo.
7. Imprimir etiqueta entrega.
8. Confirmar que la etiqueta entrega queda dentro del papel 102 x 51 mm sin cortar cliente, paciente/referencia, trabajo, recibe ni firma.
9. Revisar contraste físico, nitidez, corte, margen, orientación y alineación.
10. Confirmar que navegación, topbar, botones y texto de pantalla no aparecen en la etiqueta física.
11. Si hay desfase, ajustar primero navegador/driver/rollo antes de cambiar código.
12. Registrar evidencia manual y cualquier offset requerido por impresora.

## Ajustes Esperados En Navegador/Driver

- Escala: 100%.
- Márgenes: sin encabezado ni pie del navegador.
- Tamaño de papel personalizado:
  - 76 x 51 mm / 3 x 2 in para etiqueta interna.
  - 102 x 51 mm / 4 x 2 in para etiqueta entrega.
- Orientación: usar la que conserve ancho/alto físico correcto según driver; validar con una etiqueta de prueba.
- Calibración de offset: ajustar desde driver si el contenido sale corrido horizontal o verticalmente.
- Densidad/velocidad: ajustar desde driver si el texto sale tenue o borroso.
- Rollo: confirmar que el tamaño configurado coincide con la etiqueta instalada.

## Limitaciones

- Prueba física en DEV pendiente.
- No se agregó QR/barcode.
- No se agregó PDF.
- No se agregó impresión directa por driver/SDK.
- No se agregó etiqueta chica 51 x 25 mm.
- No se agregó repartidor asignado ni evidencia de entrega.
- El detalle actual de orden no incluye dirección/contacto completos del cliente; la etiqueta de entrega imprime textos pendientes seguros.

## Resultado De Fase 3.2.1

No hay hallazgos técnicos bloqueantes para preparar despliegue DEV desde rama `dev`.

Siguiente paso recomendado: commit/push a `dev`, desplegar en VPS DEV y ejecutar la prueba física con impresora térmica real en paralelo a la validación DEV de Fase 3.3. El flujo repartidor queda para Fase 3.4.
