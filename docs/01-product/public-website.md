# Sitio Público Institucional

Última sincronización: **2026-08-22 — DOC-SYNC-1**.

Fuente funcional vigente del sitio público de Laboratorio Dental Tláhuac.

## Propósito

Publicar una presencia digital clara, profesional, accesible y mobile-first para Laboratorio Dental Tláhuac, manteniendo separado el sistema administrativo privado bajo `/app`.

## Estado Actual

Estado en DEV: **implementado, desplegado, optimizado y aprobado visualmente**.

Cierres recientes:

- `PUB-UX-2`: rediseño de `/catalogo` como workspace responsive — aprobado.
- `PUB-UX-3`: rediseño de home, servicios, contacto y header — aprobado en DEV.
- `PUB-UX-4`: accesibilidad, estabilidad visual, SEO y Lighthouse — integrado a `dev` mediante PR #8, merge `bfa07d0285ca66fab359c151b43ed9458a6b7727`.

Producción en `laboratoriodentaltlahuac.com`: **pendiente**.

## Rutas Públicas

- `/` — home institucional.
- `/servicios` — directorio visual hacia familias del catálogo.
- `/catalogo` — catálogo público administrable.
- `/contacto` — canales confirmados y guía para preparar consulta.
- `/login` — entrada al sistema privado.

Rutas privadas:

- `/app/**` requiere sesión y permisos.
- `/dashboard` no es una ruta privada real.

## Home

La home aprobada usa:

- Hero editorial en dos columnas.
- Mensaje comercial centrado en prótesis/restauraciones y consulta clara.
- Acciones compactas hacia catálogo y contacto.
- Imágenes reales del catálogo.
- Accesos a familias destacadas.
- Proceso resumido de consulta.
- CTA final de contacto.

No debe volver a mostrar copy interno sobre `/app`, rutas técnicas, seguimiento administrativo o implementación.

## Header Y Footer

Header público:

- Sticky y compacto.
- Marca/logotipo proporcionados.
- Navegación a Inicio, Servicios, Catálogo y Contacto.
- Acceso al sistema diferenciado.
- Menú móvil colapsable.
- Soporte de teclado/Escape y foco visible.
- Skip link hacia contenido principal.

Footer:

- Navegación pública.
- Teléfonos y correo confirmados.
- Acceso al sistema sin convertirlo en CTA comercial principal.

## Servicios

`/servicios` funciona como directorio hacia familias reales del catálogo, evitando duplicar productos/precios fuera de la fuente administrable.

Familias destacadas pueden enlazar con hash, por ejemplo:

- `/catalogo#zirconia`
- `/catalogo#emax`
- `/catalogo#prostodoncia-parcial-total`
- `/catalogo#servicios-prostodonticos`

Los nombres y categorías visibles deben seguir la data real disponible y no inventar servicios no administrados.

## Catálogo Público

Fuente primaria:

- `GET /api/catalog/public`.

Fallback:

- `src/LaboratorioTlahuac.Web/src/app/public/data/catalog-data.ts`.

Comportamiento vigente:

- Una categoría activa a la vez.
- Desktop con navegación lateral.
- Tablet con navegación horizontal manual.
- Móvil con selector compacto y tarjetas adaptadas.
- Sin autoplay.
- Sin galería duplicada.
- Selección estable mediante `key`.
- Hash compartible y compatible con atrás/adelante del navegador.
- Descripciones opcionales de sección/producto.
- Imagen de producto propia; sin reutilizar una imagen de sección como si fuera producto distinto.
- Placeholder visual cuando no existe imagen de producto.
- Precios formateados en MXN.
- Carga administrable desde `/app/admin/catalogo`.

### Imágenes

Se soportan dos orígenes válidos:

1. Assets heredados `assets/catalog/products/...`.
2. Imágenes persistentes `/api/catalog/images/{fileName}` cargadas desde administración.

Storage DEV validado:

- `${LDT_APP_ROOT}/shared/catalog-images`.

El upload/reemplazo/desasociación de imagen de producto está cerrado en DEV con QA end-to-end aprobado.

DELETE desasocia la imagen del producto y no elimina físicamente el archivo.

## Contacto

Datos confirmados y publicables:

- `55 3331 9445`
- `55 2161 2311`
- `55 9802 9816`
- `contacto@laboratoriodentaltlahuac.com`

Los teléfonos deben usar `tel:` y el correo `mailto:`.

Datos que siguen fuera de la interfaz principal por falta de confirmación final:

- Dirección.
- Horarios.
- WhatsApp institucional.
- Redes sociales.
- Mapa/ubicación pública.

## Condiciones Comerciales

Se conserva una nota prudente de precios de referencia sujetos a confirmación.

No publicar como definitivos sin aprobación explícita:

- Anticipo `50%`.
- Trabajos urgentes `+40%`.
- Tiempos de entrega específicos.
- Otras condiciones derivadas de carteles/documentos previos.

## Accesibilidad

Baseline vigente:

- Skip link.
- Navegación por teclado.
- Foco visible.
- Menú móvil compatible con Escape.
- Contraste corregido en páginas públicas.
- `prefers-reduced-motion` respetado.
- Imágenes con `alt` administrable o texto alternativo seguro.
- Dimensiones/aspect-ratio reservados para reducir layout shift.

Lighthouse de cierre PUB-UX-4:

| Ruta | Performance | Accesibilidad | Best Practices | SEO |
| --- | ---: | ---: | ---: | ---: |
| `/` | 91 | 100 | 100 | 100 |
| `/servicios` | 95 | 100 | 100 | 100 |
| `/catalogo` | 93 | 100 | 100 | 100 |
| `/contacto` | 96 | 100 | 100 | 100 |

La medición corresponde al árbol final de PUB-UX-4 antes del merge; `dev` contiene ese mismo árbol más el commit de merge.

## SEO

Implementado:

- Título/descripción por ruta pública.
- Descripción institucional actualizada.
- `robots.txt` válido.
- `/login` y `/app` desautorizados para rastreo mediante robots.

Pendiente para producción:

- Validar dominio canónico final.
- Decidir `www` o dominio raíz.
- Revisar indexación después de publicación productiva.
- Sitemap solo si se considera necesario; no es requisito del MVP actual.

## Rendimiento

Medidas vigentes:

- Lazy loading de rutas Angular.
- Lazy loading de imágenes below-the-fold cuando corresponde.
- Sin librería externa de carrusel.
- Sin nuevas dependencias para PUB-UX-2/3/4.
- Composición con CSS y Angular nativos.

## Estado De Aprobación

Aprobado por revisión humana en DEV:

- Catálogo desktop/móvil.
- Home, servicios, contacto y header.
- Dirección visual general posterior a PUB-UX-3.

`PUB-UX-4` cerró mejoras técnicas sin replantear el diseño visual aprobado.

## Próximo Hito

El sitio público no requiere otra fase de rediseño antes de producción.

Próximos pasos globales:

1. `OPS-QA-1` para QA operativo pendiente del sistema privado.
2. `PROD-READY-1` para preparar infraestructura, seguridad, backup/restore, DNS y HTTPS.
3. `PROD-RELEASE-1` para promover `dev -> main` y publicar producción.

Fuente de priorización: `docs/05-delivery/current-work-plan.md`.

## Alcance Protegido

Cualquier cambio futuro del sitio público debe preservar salvo decisión explícita:

- Rutas privadas y guards.
- Auth/cookies/XSRF.
- Contratos API de catálogo.
- Administración de catálogo e imágenes.
- Fallback de catálogo.
- Diseño visual aprobado como baseline.

Las modificaciones visuales posteriores deben ser incrementales y justificadas por feedback o métricas.