# Checklist QA Funcional MVP Administrativo

Fecha de ejecución: 2026-05-11.

Alcance: Fase 1, Etapa 7. QA funcional del MVP administrativo, preparación de demo y documentación de hallazgos. No incluye inventario, proveedores, repartidores, etiquetas, sitio corporativo, migración del Excel, CFDI, facturación ni reportes avanzados.

## Validaciones Técnicas

- [x] Revisar `git status` antes de editar.
- [x] Confirmar que `src/LaboratorioTlahuac.Web/angular.json` no tiene cambios locales.
- [x] Ejecutar `dotnet restore`.
- [x] Ejecutar `dotnet build`.
- [x] Ejecutar `dotnet test`.
- [x] Ejecutar `npm install`.
- [x] Ejecutar `npm run build`.
- [x] Ejecutar `npm audit`.
- [x] Ejecutar `dotnet ef migrations list`.
- [x] Aplicar migraciones en SQL Server local aislado.
- [x] Confirmar que `/health` responde sin sesión.
- [x] Confirmar que endpoints protegidos sin sesión devuelven `401`.
- [x] Confirmar que usuario sin permiso requerido recibe `403`.
- [x] Confirmar que mutaciones autenticadas sin XSRF fallan.

## Validaciones Funcionales

- [x] Configurar seed Admin para QA local.
- [x] Iniciar sesión como Admin.
- [x] Entrar a dashboard.
- [x] Crear cliente tipo Doctor.
- [x] Crear cliente tipo Clinic.
- [x] Agregar doctores internos a la clínica.
- [x] Crear orden para Doctor.
- [x] Crear orden para Clinic con doctor interno.
- [x] Cambiar estado de orden.
- [x] Cancelar orden con nota.
- [x] Crear orden con `TotalAmount`.
- [x] Registrar pago parcial.
- [x] Verificar saldo parcial.
- [x] Registrar pago para cubrir total.
- [x] Verificar estado financiero `Paid`.
- [x] Registrar sobrepago y verificar `Overpaid`.
- [x] Cancelar pago con motivo.
- [x] Verificar recálculo de saldo.
- [x] Entrar a listado de pagos.
- [x] Verificar dashboard de operación y cobranza.
- [x] Confirmar que cancelados no cuentan indebidamente en métricas operativas ni financieras.

## Evidencia Resumida

- Backend build: correcto.
- Backend tests: correctos.
- Frontend build: correcto.
- `npm audit`: 0 vulnerabilidades.
- Migraciones EF listadas: `InitialSecurityModel`, `AddCustomersAndInternalDoctors`, `AddWorkOrders`, `AddPayments`.
- Migraciones aplicadas correctamente en base temporal `LaboratorioTlahuac_QA`.
- Angular respondió `200` en `/login` y `/app/dashboard` usando puerto local alterno `4300`.

## Observaciones

- La cadena local por defecto `Server=localhost;Trusted_Connection=True` no encontró SQL Server disponible en este entorno. Para QA se usó SQL Server local aislado en Docker con cadena explícita.
- Durante QA se detectó y corrigió que el dashboard excluía órdenes canceladas del monto por cobrar, pero no de todos los conteos financieros.
- La revisión visual completa con el cliente sigue pendiente.
