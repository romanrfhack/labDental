# Checklist De Aceptación Del MVP Administrativo

Fecha base: 2026-05-11.

## Criterios Funcionales

- [x] Admin puede iniciar sesión.
- [x] Admin puede consultar dashboard.
- [x] Admin puede crear doctores individuales.
- [x] Admin puede crear clínicas.
- [x] Admin puede crear doctores internos para clínicas.
- [x] Admin puede crear órdenes para doctores.
- [x] Admin puede crear órdenes para clínicas con doctor interno.
- [x] Admin puede cambiar estados de órdenes.
- [x] Admin puede cancelar órdenes con nota.
- [x] Admin puede registrar pagos parciales.
- [x] Admin puede liquidar una orden.
- [x] Admin puede registrar sobrepago.
- [x] Admin puede cancelar pagos con motivo.
- [x] El saldo se recalcula desde pagos no cancelados.
- [x] El listado de pagos muestra pagos vigentes por defecto.
- [x] El dashboard muestra métricas operativas y financieras básicas.
- [x] Órdenes canceladas no cuentan indebidamente en métricas operativas ni financieras.

## Criterios Técnicos

- [x] `dotnet restore` correcto.
- [x] `dotnet build` correcto.
- [x] `dotnet test` correcto.
- [x] `npm install` correcto.
- [x] `npm run build` correcto.
- [x] `npm audit` con 0 vulnerabilidades.
- [x] Migraciones EF listan correctamente.
- [x] Migraciones aplican correctamente en SQL Server local aislado.
- [x] `/health` sigue público.
- [x] Endpoints sin sesión devuelven `401`.
- [x] Endpoints sin permiso devuelven `403`.
- [x] Mutaciones autenticadas sin XSRF fallan.

## Criterios De Demo

- [x] Guion de demo preparado.
- [x] Guía de datos de demo preparada.
- [x] Hallazgos conocidos priorizados.
- [x] Documentación de estado actualizada.
- [ ] Demo ejecutada con cliente.
- [ ] Feedback de cliente capturado.
- [ ] Alcance comercial siguiente cerrado.

## Fuera De Alcance Para Aceptación

- Inventario.
- Proveedores.
- Repartidores.
- Etiquetas.
- Sitio web corporativo.
- Migración del Excel.
- CFDI.
- Facturación.
- Reportes avanzados.
