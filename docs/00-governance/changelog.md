# Changelog

Todos los cambios relevantes del proyecto deben registrarse aquí. No se deben inventar funcionalidades implementadas.

## 2026-05-12 - Fase 0.2 Consolidación Documental

- Se consolida la documentación para separar sistema privado, sitio público, control global, deploy, QA y documentación comercial.
- Se crea `docs/README.md` como índice general.
- Se crean fuentes canónicas: `docs/03-architecture/ARCHITECTURE.md`, `docs/03-architecture/AUTH_FLOW.md`, `docs/05-delivery/DEPLOYMENT.md` y `docs/08-qa/RESPONSIVE_CHECKLIST.md`.
- Se crean documentos funcionales: `docs/01-product/public-website.md` y `docs/01-product/internal-system.md`.
- Se actualizan `README.md`, `AGENTS.md`, `docs/PROJECT_STATUS.md`, `docs/ROADMAP.md` y `docs/IMPLEMENTATION_LOG.md`.
- Se agregan puentes temporales para rutas documentales anteriores.
- No se implementa código ni se modifican pantallas, rutas, auth, deploy, dependencias ni base de datos.

## 2026-05-12 - Paquete Comercial Primera Ronda

- Se crea documentación ejecutiva/comercial en `docs/09-commercial`.
- Se documenta alcance incluido, opcional y fuera de alcance para la primera ronda de implementación.
- Se agrega propuesta ejecutiva, matriz de alcance, fases comerciales, plan de entrega y aceptación, supuestos, exclusiones y plantilla económica sin importes.
- Se agregan documentos contractuales base: Statement of Work, control de cambios, responsabilidades del cliente y agenda de reunión de demo.
- Se documenta el módulo propuesto de repartidores, entregas y etiquetas como web responsive/PWA.
- Se aclara que el servicio local de impresión queda condicionado a validación de hardware.
- Se actualizan README y próximos pasos.
- No se implementan módulos nuevos ni cambios de arquitectura.

## 2026-05-11 - Fase 1 Etapa 7

- Se ejecuta QA funcional del MVP administrativo con SQL Server local aislado.
- Se valida login, clientes, doctores internos, órdenes, pagos, saldos, dashboard, CSRF, `401`, `403` y `/health`.
- Se corrige conteo financiero del dashboard para excluir órdenes canceladas de métricas de saldo pendiente y sin pago.
- Se agrega documentación de QA funcional en `docs/08-qa`.
- Se agrega guion de demo.
- Se agrega guía de datos de prueba.
- Se documentan hallazgos conocidos.
- Se actualizan estado del proyecto, próximos pasos, checklist de aceptación y README.
- No se implementan módulos nuevos.

## 2026-05-11 - Fase 1 Etapa 6

- Se implementa dashboard operativo básico.
- Se agregan métricas de clientes, órdenes y pagos usando datos existentes.
- Se agrega endpoint `GET /api/dashboard/summary`.
- Se agrega pantalla Angular de dashboard en `/app/dashboard`.
- Se agregan pruebas backend de integración para autorización, secciones por permiso, métricas financieras, métricas operativas y límites de listas.
- Se crea ADR-0011 para secciones de dashboard condicionadas por permisos.
- No se implementan inventario, proveedores, CFDI, facturación, cortes de caja avanzados, reportes avanzados, exportación Excel/PDF ni migración del Excel.

## 2026-05-09 - Fase 1 Etapa 5

- Se implementan pagos y abonos asociados a órdenes de trabajo.
- Se agrega entidad `Payment`.
- Se agregan `PaymentMethod` y `PaymentStatus` calculado.
- Se agregan endpoints de pagos bajo `/api/work-orders/{workOrderId}/payments` y `/api/payments`.
- Se agregan resúmenes financieros calculados: `PaidAmount`, `Balance` y estado financiero.
- Se agregan pantallas y secciones Angular de pagos en `/app/pagos` y `/app/ordenes/:id`.
- Se agregan pruebas backend de integración para permisos, CSRF, creación, cancelación, reglas de negocio, sobrepago y listados.
- Se agrega migración `AddPayments`.
- No se implementan inventario, proveedores, dashboard operativo real, facturación, CFDI ni reportes avanzados.

## 2026-05-09 - Fase 1 Etapa 4

- Se implementan órdenes de trabajo dental.
- Se agregan entidades `WorkOrder`, `WorkOrderStatus` y `WorkOrderStatusHistory`.
- Se agregan endpoints `/api/work-orders`.
- Se agregan pantallas Angular de órdenes.
- Se agregan pruebas backend de integración para permisos, CSRF, creación, edición, cambio de estado e historial.
- Se agrega migración `AddWorkOrders`.
- No se implementan pagos, abonos ni saldos en esta etapa.

## 2026-05-09 - Fase 1 Etapa 3

- Se implementa CRUD de clientes/doctores/clínicas.
- Se agregan entidades `Customer`, `InternalDoctor` y enum `CustomerType`.
- Se agregan endpoints `/api/customers`.
- Se agregan pantallas Angular de clientes.
- Se agregan pruebas backend de integración para clientes, permisos, CSRF y reglas de clínicas.
- Se agrega migración `AddCustomersAndInternalDoctors`.

## 2026-05-08 - Fase 1 Etapa 2.1

- Se agrega protección CSRF/XSRF para métodos mutables bajo `/api`.
- Se agrega endpoint `GET /api/auth/csrf` para emitir cookie `XSRF-TOKEN`.
- Se protege `POST /api/auth/login` y `POST /api/auth/logout` con header `X-XSRF-TOKEN`.
- Se agrega endpoint técnico `POST /api/security/csrf-check` solo en Development para validar antiforgery.
- Se validan cookies de auth y antiforgery en pruebas de integración.
- Se actualiza Angular para pedir token XSRF antes de login/logout y enviar `X-XSRF-TOKEN`.
- Se revisa `npm audit` y se corrigen vulnerabilidades transitivas con `npm audit fix` sin `--force`.

## 2026-05-08 - Fase 1 Etapa 2

- Se implementa autenticación con cookie HttpOnly.
- Se agregan entidades de seguridad: `User`, `Role`, `Permission`, `UserRole` y `RolePermission`.
- Se agrega `LaboratorioTlahuacDbContext` con EF Core SQL Server y migración inicial de seguridad.
- Se agrega seed Admin idempotente controlado por configuración.
- Se agregan endpoints `POST /api/auth/login`, `POST /api/auth/logout` y `GET /api/auth/me`.
- Se agrega endpoint técnico `GET /api/security/permissions-check` solo en Development para validar autorización por permisos.
- Se reemplazan guards placeholder por `AuthGuard` y `PermissionGuard` funcionales en Angular.
- Se agrega login real, logout y sesión en memoria en Angular sin `localStorage` ni `sessionStorage`.
- Se agregan pruebas backend de integración para health, login, cookie, `/me` y permisos.

## 2026-05-05 - Fase 1 Etapa 1

- Se crea arquitectura base backend/frontend.
- Se agregan proyectos .NET 10: Api, Application, Domain e Infrastructure.
- Se agrega app Angular 21.
- Se agregan rutas iniciales públicas y privadas placeholder.
- Se agrega endpoint `GET /health`.
- Se agregan permisos base centralizados.

## 2026-05-05 - Fase 0

- Se crea documentación inicial del proyecto.
