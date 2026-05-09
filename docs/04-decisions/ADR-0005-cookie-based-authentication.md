# ADR-0005: Adoptar Autenticación Basada En Cookie Segura HttpOnly Para El MVP

## Estado

Aceptada para MVP.

## Contexto

El sitio público y la app privada vivirán inicialmente en el mismo dominio. El sistema no requiere app móvil ni API pública en la primera etapa.

## Decisión

Usar autenticación basada en cookie segura HttpOnly para la sesión web.

## Consecuencias Positivas

- Reduce exposición de tokens a JavaScript.
- Simplifica operación en un mismo dominio.
- Es suficiente para `/login` y `/app`.
- Puede convivir con autorización por permisos.

## Consecuencias Negativas

- Requiere manejo correcto de SameSite, Secure, expiración y CSRF.
- Si en el futuro existe app móvil o API pública, podría agregarse OAuth/JWT.

## Alternativas Consideradas

- JWT en localStorage.
- JWT en memory storage.
- Subdominio separado con API token-based.
