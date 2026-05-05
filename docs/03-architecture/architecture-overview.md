# Visión General De Arquitectura

## Propuesta Inicial

- Frontend: Angular.
- Backend: .NET.
- Comunicación: API REST.
- Base de datos: relacional.
- Autenticación: JWT o cookies seguras. La decisión final queda pendiente de validación técnica.
- Dominio: laboratoriodentaltlahuac.com.
- Sitio público y app privada: mismo dominio inicialmente.

## Principios

- Arquitectura modular y preparada para crecimiento incremental.
- Separación clara entre dominio, casos de uso, infraestructura y API.
- Autorización por permisos desde el inicio.
- Cambios de esquema y contratos API deben ser compatibles cuando sea posible.
- La documentación se actualiza con cada cambio implementado.

## Vista Conceptual

Usuario -> Angular público/privado -> API REST .NET -> Base de datos relacional.

## Criterios De Validación

- `/` sirve sitio público.
- `/login` permite acceso privado.
- `/app/*` queda protegido.
- El backend expone API versionable y protegida.

## Pendientes

- Definir autenticación final: JWT o cookies seguras.
- Definir motor de base de datos.
- Definir hosting y estrategia de despliegue.
