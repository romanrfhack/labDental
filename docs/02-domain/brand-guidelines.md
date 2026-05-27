# Lineamientos De Marca

Fuente para identidad visual del sitio público de Laboratorio Dental Tláhuac.

## Logo

- Asset fuente: `src/LaboratorioTlahuac.Web/src/assets/brand/logo-ldt.webp`.
- Ruta pública esperada: `/assets/brand/logo-ldt.webp`.
- Formato: WebP.
- Uso: mostrar proporcionalmente, sin deformar, con `height: auto` y ancho controlado por CSS.
- Ubicación actual: header público, home y login visual.

## Tokens Visuales

```css
--ldt-navy: #032155;
--ldt-navy-soft: #0b3069;
--ldt-blue: #039ef7;
--ldt-blue-dark: #0781e6;
--ldt-sky: #54b1e8;
--ldt-sky-light: #a6dff7;
--ldt-gray: #65738e;
--ldt-white: #ffffff;
```

## Uso Recomendado

- Botones primarios: `--ldt-blue-dark` con texto blanco.
- Botones secundarios: fondo blanco, borde `--ldt-sky` y texto `--ldt-navy-soft`.
- Links activos: `--ldt-navy` con texto blanco cuando aplique.
- Bordes de tarjetas: variantes claras de `--ldt-sky`.
- Estados focus: halo azul claro basado en `--ldt-blue`.
- Footer: base `--ldt-navy`.
- Evitar verde, rojo u otros colores como base de marca; reservarlos solo para estados funcionales cuando aplique.

## Estilo Visual Fase 1.6

- Fondos premium: degradados institucionales con `--ldt-navy`, `--ldt-navy-soft`, `--ldt-blue` y tramas lineales suaves; evitar decoraciones saturadas.
- Botones activos y CTAs: usar degradado azul institucional, sombra controlada y microinteracción vertical máxima de 1px.
- Cards públicas: fondo blanco o azul muy claro, borde basado en `--ldt-sky`, sombra ligera y radio máximo de 8px.
- Catálogo: mantener precios en pastilla legible con contraste alto; imagen en frame uniforme sin deformación.
- Estados focus: conservar halo visible basado en `--ldt-focus` o `--ldt-sky`; no retirar outline sin reemplazo claro.
- Datos confirmados: destacar con azul institucional; datos pendientes: usar gris/neutral sin parecer error.

## Movimiento Y Animación

- En móvil, las animaciones deben ser breves y discretas: opacity y transform únicamente.
- No usar pinning largo, smooth scroll global ni scrub agresivo en esta fase.
- El parallax permitido es ligero, decorativo y no debe mover texto crítico.
- `prefers-reduced-motion: reduce` debe desactivar reveal, parallax y microinteracciones de transform.
- No depender del movimiento para comunicar estados, precios, CTAs o datos de contacto.

## Validación Visual Fase 1.6

- Validación manual confirmada el 2026-05-27 para rutas públicas, login visual, viewports obligatorios y ausencia de scroll horizontal.
- Estado de cierre: identidad visual aplicada y aceptada visualmente para esta etapa.
- Las animaciones se consideran sutiles y profesionales; el enfoque CSS + `IntersectionObserver` queda vigente.
- Reduced motion queda validado por implementación/código; no se reportaron hallazgos manuales bloqueantes.
- Los tokens LDT, criterios de logo y criterios de movimiento se mantienen como guía vigente.
- No se deben cambiar tokens, logo, movimiento ni composición de marca salvo observaciones concretas del cliente o una fase aprobada.

## Datos Del Cartel Incorporados

- Nombre comercial: Laboratorio Dental Tláhuac.
- Eslogan: Precisión • Estética • Confianza.
- Línea descriptiva: Prótesis, restauraciones y soluciones dentales.
- Teléfonos:
  - 55 3331 9445
  - 55 2161 2311
  - 55 9802 9816
- Correo: `contacto@laboratoriodentaltlahuac.com`.
- Catálogo de precios 2026.

## Condiciones Comerciales Del Cartel

Visibles en el cartel/catálogo:

- Anticipo 50%.
- Trabajos urgentes +40%.

Estas condiciones no deben publicarse como definitivas sin aprobación final del cliente. Si se muestran en el sitio, deben aparecer con texto prudente de confirmación pendiente.

## Datos Pendientes De Confirmar

- Dirección.
- Horarios.
- WhatsApp como canal real.
- Redes sociales.
- Mapa o ubicación pública.

No se debe inventar ninguno de estos datos.
