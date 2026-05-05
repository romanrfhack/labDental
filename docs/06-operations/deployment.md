# Despliegue

## Estrategia Inicial

- Servidor pendiente de definir.
- Dominio principal: laboratoriodentaltlahuac.com.
- HTTPS obligatorio recomendado desde el inicio.
- App pública y privada servidas desde el mismo frontend.
- Backend protegido detrás de reverse proxy.

## Consideraciones

- El despliegue debe ser reproducible.
- Las variables de entorno deben documentarse sin exponer secretos.
- Las migraciones de base de datos deben ejecutarse con respaldo previo cuando exista producción.
- La API debe quedar accesible solo por las rutas esperadas.

## Criterios De Validación

- El sitio público carga por HTTPS.
- `/app` requiere autenticación.
- El backend no expone información sensible.
- Existe procedimiento documentado para actualizar producción.
