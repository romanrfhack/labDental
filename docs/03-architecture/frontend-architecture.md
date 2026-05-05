# Arquitectura Frontend

## Propuesta

Usar Angular con separación entre rutas públicas y privadas, componentes por feature y servicios dedicados para comunicación con la API.

## Estructura Conceptual

- Rutas públicas: sitio institucional y login.
- Rutas privadas: módulos bajo `/app`.
- Layout público: navegación y contenido institucional.
- Layout privado: navegación operativa, sesión y módulos internos.
- Features: clientes, órdenes, pagos, inventario, proveedores, administración.
- Servicios API: encapsulan llamadas HTTP por dominio funcional.

## Seguridad En Frontend

- Guards de autenticación para `/app/*`.
- Guards de permisos para pantallas o acciones sensibles.
- El frontend oculta acciones no permitidas, pero la autorización real debe validarse en backend.

## Criterios De Validación

- El sitio público puede navegarse sin sesión.
- Las rutas privadas redirigen a login si no hay sesión.
- Las acciones sensibles dependen de permisos, no solo de rol.

## Próximos Pasos

- Confirmar versión de Angular.
- Definir diseño base y componentes compartidos al iniciar Fase 1.
