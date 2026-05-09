# ADR-0007: Protección CSRF/XSRF Para Autenticación Basada En Cookies

## Estado

Aceptada para MVP.

## Contexto

El sistema usa cookie segura HttpOnly para autenticación. Los navegadores envían cookies automáticamente, por lo que los endpoints mutables requieren protección CSRF/XSRF.

## Decisión

Usar token antiforgery emitido por el backend, cookie `XSRF-TOKEN` legible por JavaScript y header `X-XSRF-TOKEN` enviado por Angular en requests mutables.

`POST /api/auth/login` también requiere CSRF. El cliente debe llamar primero a `GET /api/auth/csrf`. Después de login se renueva el token porque cambia la identidad de anónimo a usuario autenticado.

## Consecuencias Positivas

- Reduce riesgo CSRF en endpoints mutables.
- Es compatible con cookie auth.
- Aprovecha mecanismo estándar de Angular HttpClient.
- Mantiene la cookie de sesión protegida como HttpOnly.

## Consecuencias Negativas

- Requiere coordinación backend/frontend.
- Login/logout y futuros endpoints mutables deben considerar token XSRF.
- La cookie `XSRF-TOKEN` es legible por JavaScript por diseño, aunque no contiene la sesión.
- Los servicios mutables futuros deberán reutilizar el flujo XSRF.

## Alternativas Consideradas

- No usar CSRF por `SameSite=Lax` solamente.
- JWT en localStorage.
- Tokens manuales custom por endpoint.
- Proteger solo endpoints de negocio futuros.
