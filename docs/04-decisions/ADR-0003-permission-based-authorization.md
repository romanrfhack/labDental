# ADR-0003: Autorización Basada En Permisos

## Estado

Aceptada.

## Contexto

Inicialmente solo existirá el rol Admin, pero el sistema debe quedar preparado para roles futuros.

## Decisión

Usar autorización basada en permisos granulares.

## Motivo

Permite crecer a roles futuros sin romper contratos, rutas o endpoints. También evita codificar reglas rígidas basadas únicamente en nombres de roles.

## Consecuencias

- Admin tendrá todos los permisos.
- Las acciones protegidas deberán mapearse a permisos.
- El backend será la fuente autoritativa de autorización.
- El frontend usará permisos para navegación y visibilidad de acciones.
