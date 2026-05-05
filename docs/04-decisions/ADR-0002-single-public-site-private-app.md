# ADR-0002: Sitio Público Y App Privada En El Mismo Dominio

## Estado

Aceptada.

## Contexto

El proyecto necesita un sitio público institucional y una aplicación privada para operación interna.

## Decisión

Iniciar con sitio público y app privada en el mismo dominio.

- Ruta pública: `/`
- Login: `/login`
- Ruta privada: `/app`

## Motivo

Reducir complejidad inicial de despliegue, certificados, configuración DNS y operación.

## Alternativa Futura

Separar app y API por subdominios, por ejemplo:

- `www.laboratoriodentaltlahuac.com`
- `app.laboratoriodentaltlahuac.com`
- `api.laboratoriodentaltlahuac.com`

## Consecuencias

- El frontend debe separar layout público y privado.
- La seguridad de `/app` depende de autenticación y autorización.
- La separación por subdominios podrá hacerse después si el proyecto lo requiere.
