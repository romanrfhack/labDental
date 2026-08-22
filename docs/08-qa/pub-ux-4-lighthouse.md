# QA PUB-UX-4 — Lighthouse Y Accesibilidad

Fecha de cierre técnico: **2026-08-22**.

## Alcance

Registrar la validación automatizada de la fase `PUB-UX-4` sobre las cuatro rutas públicas principales después de aplicar mejoras de accesibilidad, SEO, estabilidad visual y carga de imágenes.

Rutas auditadas:

- `/`
- `/servicios`
- `/catalogo`
- `/contacto`

## Integración

- Rama de trabajo: `agent/pub-ux-4-quality`.
- PR: `#8`.
- Head validado: `49ec8907f352b0b7cf7d7e24f9e6ddfbc77a6438`.
- Merge a `dev`: `bfa07d0285ca66fab359c151b43ed9458a6b7727`.
- El árbol de archivos validado por Lighthouse es el mismo árbol funcional integrado en `dev`; el merge agrega únicamente el commit de integración.

## Resultados Finales

| Ruta | Performance | Accesibilidad | Best Practices | SEO |
| --- | ---: | ---: | ---: | ---: |
| `/` | 91 | 100 | 100 | 100 |
| `/servicios` | 95 | 100 | 100 | 100 |
| `/catalogo` | 93 | 100 | 100 | 100 |
| `/contacto` | 96 | 100 | 100 | 100 |

Resultado global:

- Performance: `91–96`.
- Accesibilidad: `100` en todas las rutas.
- Best Practices: `100` en todas las rutas.
- SEO: `100` en todas las rutas.

## Hallazgo Corregido Durante La Fase

La primera compuerta obtuvo SEO `92` porque `/robots.txt` devolvía el shell de Angular en vez de un archivo de robots válido.

Corrección:

- Se agregó `src/LaboratorioTlahuac.Web/public/robots.txt`.
- Permite rastreo público general.
- Desautoriza `/login` y `/app`.
- La compuerta se repitió sin reducir umbrales y terminó en SEO `100` para todas las rutas.

## Mejoras Incluidas

- Skip link al contenido principal.
- Navegación móvil con mejor soporte de teclado/Escape.
- Foco visible.
- Contraste corregido donde aplicaba.
- Lazy loading en imágenes below-the-fold.
- Dimensiones/aspect ratio reservados para imágenes dinámicas del catálogo.
- Metadatos de título/descripción por ruta.
- `robots.txt` válido.
- Preservación de `prefers-reduced-motion`.

## Compuerta Automatizada

La rama incluyó una compuerta con:

- `dotnet restore`.
- `dotnet build`.
- `dotnet test`.
- `npm ci`.
- `npm run build`.
- `git diff --check` contra `dev`.
- Lighthouse para performance, accessibility, best-practices y SEO.

Umbrales mínimos configurados:

- Performance: `>= 80`.
- Accesibilidad: `>= 95`.
- Best Practices: `>= 95`.
- SEO: `>= 95`.

## Nota Operativa

Estos resultados no sustituyen un Lighthouse contra producción real después de `PROD-RELEASE-1`, donde red, CDN/proxy, TLS, caché y capacidad del VPS pueden alterar Performance.

Al publicar producción debe repetirse una medición equivalente como parte del smoke/post-release.

## Estado

`PUB-UX-4`: **CERRADA E INTEGRADA EN DEV**.
