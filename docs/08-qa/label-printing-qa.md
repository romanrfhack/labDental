# QA Impresión De Etiquetas

Fuente QA para Fase 3.2 - MVP de impresión de etiquetas desde órdenes existentes.

## Alcance

Validar que el usuario pueda abrir etiquetas privadas desde `/app/ordenes/:id`, imprimir desde navegador y volver a la orden sin romper rutas existentes.

## Rutas

- `/app/ordenes/:id/etiqueta-trabajo`
- `/app/ordenes/:id/etiqueta-entrega`

Ambas rutas viven bajo `/app`, requieren sesión y usan permiso `orders.view`.

## Checklist Manual

1. Entrar con Admin o usuario con `orders.view`.
2. Abrir `/app/ordenes`.
3. Abrir una orden.
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

## Prueba Con Impresora Térmica

Pendiente de ejecución en DEV con el equipo real.

Recomendaciones:

- Configurar tamaño 3 x 2 in / 76 x 51 mm para etiqueta interna.
- Configurar tamaño 4 x 2 in / 102 x 51 mm para etiqueta entrega.
- Usar escala 100% si el navegador lo permite.
- Desactivar encabezados y pies del navegador.
- Revisar orientación, corte, margen físico, contraste y legibilidad.
- Ajustar configuración de driver/rollo antes de cambiar código.

## Validación Técnica Fase 3.2

Ejecutada el 2026-07-02 en esta rama:

- `npm run build` desde `src/LaboratorioTlahuac.Web`: correcto.
- `dotnet build` desde raíz: correcto con 0 errores y 2 warnings `NU1903` conocidos por `SQLitePCLRaw.lib.e_sqlite3` 2.1.11 en tests.
- `dotnet test` desde raíz: correcto; Domain 1/1, Application 1/1 y API 101/101.
- `git diff --check`: correcto.
- Búsquedas obligatorias de rutas, etiquetas, impresión, dashboard, login y patrones sensibles: ejecutadas.
- Las búsquedas de patrones sensibles se limitaron a nombres de archivo para no imprimir valores.

## Limitaciones

- No se probó impresión física desde este entorno.
- No se agregó QR/barcode.
- No se agregó PDF.
- No se agregó impresión directa por driver/SDK.
- No se agregó etiqueta chica 51 x 25 mm.
- No se agregó repartidor asignado ni evidencia de entrega.
- El detalle actual de orden no incluye dirección/contacto completos del cliente; la etiqueta de entrega imprime textos pendientes seguros.
