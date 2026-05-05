# Arquitectura Backend

## Propuesta

Usar una arquitectura limpia o modular con separación por responsabilidades.

## Capas

### Api

Expone endpoints HTTP, validación de entrada superficial, autenticación, autorización y contratos de respuesta.

### Application

Contiene casos de uso, orquestación, validaciones de aplicación, DTOs y contratos con infraestructura.

### Domain

Contiene entidades, reglas de negocio, invariantes y conceptos centrales como órdenes, pagos, clientes y permisos.

### Infrastructure

Contiene persistencia, integraciones externas, correo, archivos, configuración técnica y adaptadores.

## Reglas De Dependencia

- Domain no depende de Infrastructure.
- Domain no depende de Api.
- Application puede depender de Domain.
- Infrastructure implementa contratos definidos por Application.
- Api consume Application.

## Criterios De Validación

- Las reglas de saldo y estados no viven en controladores.
- La persistencia puede cambiar sin reescribir el dominio.
- Los casos de uso son testeables sin servidor HTTP.

## Próximos Pasos

- Confirmar solución .NET y versión.
- Definir estructura exacta de proyectos en Fase 1.
