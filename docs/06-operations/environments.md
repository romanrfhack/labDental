# Ambientes

## Local

Ambiente de desarrollo en la máquina del desarrollador. Pendiente de definir puertos, variables de entorno y base de datos local.

## Development

Ambiente compartido para validación previa a producción. Pendiente de definir servidor, dominio o subdominio.

## Production

Ambiente productivo para operación real del laboratorio. Dominio principal: laboratoriodentaltlahuac.com.

## URLs Pendientes

Todavía no se definen URLs finales para API si se mantiene el mismo dominio. La decisión dependerá del hosting y reverse proxy.

## Criterios De Validación

- Cada ambiente debe tener configuración documentada antes de usarse.
- Producción debe usar HTTPS.
- Las credenciales no deben guardarse en repositorio.
