# Propuesta de sistema visual para Angular

## Objetivo

Mejorar la apariencia visual de la aplicación Angular de Laboratorio Dental Tláhuac sin caer en cambios aislados por componente. La intención es construir una base de diseño transversal que permita consistencia visual, mejor mantenimiento y evolución ordenada de la interfaz.

## Contexto observado

Estado actual detectado en `src/LaboratorioTlahuac.Web`:

- El sitio público ya tiene una identidad visual más trabajada y consistente.
- La aplicación privada y administrativa es funcional, pero visualmente más utilitaria.
- Existen estilos globales reutilizables, pero mezclados con reglas muy específicas por pantalla.
- Hay componentes con `styles` inline dentro de archivos `.ts`, lo que dificulta consolidar un lenguaje visual uniforme.
- Público y privado comparten parte de la paleta, pero todavía no operan como un solo sistema de diseño.

## Diagnóstico

Hoy la app presenta tres problemas visuales estructurales:

1. No existe un sistema visual formal unificado.
2. Varias decisiones de estilo viven acopladas a páginas concretas.
3. Mejorar una pantalla no garantiza mejorar la coherencia del resto del sistema.

## Dirección visual recomendada

### Concepto

La dirección recomendada es:

- clínica
- moderna
- confiable
- clara
- profesional
- ligeramente premium
- enfocada en operación y legibilidad

No conviene un estilo recargado ni demasiado llamativo. Tampoco uno excesivamente plano o genérico. Para este producto funciona mejor una interfaz sobria, limpia y precisa.

### Traducción visual

- fondos claros
- superficies blancas o azuladas suaves
- azul como color principal de marca
- navy para estructura y jerarquía
- sombras suaves y consistentes
- bordes discretos
- radios uniformes
- jerarquía tipográfica clara
- estados operativos muy distinguibles

## Identidad unificada para público y privado

Se recomienda usar una sola identidad visual para todo el producto, con dos niveles de expresión:

- **Público**: más expresivo, comercial y de marca
- **Privado/Admin**: más sobrio, operativo y denso

La paleta, tipografía, tokens y componentes base deben ser los mismos. Lo que cambia es la intensidad visual, no la identidad.

## Paleta sugerida

Se propone formalizar la base actual en tokens.

### Marca

- `primary-500`: azul principal de marca
- `primary-600`: azul de acción / hover
- `primary-700`: azul profundo
- `brand-navy`: navy estructural
- `brand-sky`: azul claro de apoyo

### Neutros

- `neutral-0`: blanco
- `neutral-25`: fondo muy claro
- `neutral-50`: fondo base de aplicación
- `neutral-100`: borde suave
- `neutral-300`: texto secundario
- `neutral-500`: texto normal
- `neutral-700`: texto fuerte
- `neutral-900`: títulos

### Estados

- `success`
- `warning`
- `danger`
- `info`

### Regla de uso

- El azul debe dominar acciones primarias y foco de producto.
- Danger solo debe usarse para acciones destructivas o alertas serias.
- Warning debe reservarse a estados operativos de atención.
- Los verdes no deben aparecer como color arbitrario fuera del sistema.

## Tipografía

### Recomendación

Usar **Inter** como tipografía principal.

### Razones

- excelente legibilidad en dashboards y formularios
- se comporta muy bien en tablas
- moderna sin exagerar personalidad
- adecuada para producto administrativo y sitio público

### Jerarquía sugerida

- título de página
- título de sección
- título de tarjeta
- cuerpo
- cuerpo pequeño
- label
- caption

La recomendación es mantener pocas variantes y aplicarlas con consistencia.

## Sistema de espaciado

Se recomienda una escala base corta y estable:

- 4
- 8
- 12
- 16
- 20
- 24
- 32
- 40
- 48

### Regla de uso

- controles y campos: 12 a 16
- bloques internos: 16 a 24
- separación entre secciones: 24 a 32
- respiración de página: 32+

## Radios, bordes y sombras

### Radios

- `sm`: 6px
- `md`: 8px
- `lg`: 12px
- `xl`: 16px

### Bordes

- suaves
- neutrales
- consistentes entre módulos

### Sombras

Escala corta sugerida:

- `shadow-sm`
- `shadow-md`
- `shadow-lg`

Las sombras deben ayudar a la jerarquía, no competir con ella.

## Arquitectura de estilos recomendada

Se recomienda reestructurar el SCSS global así:

```text
src/LaboratorioTlahuac.Web/src/styles/
  _tokens.scss
  _functions.scss
  _mixins.scss
  _reset.scss
  _typography.scss
  _base.scss
  _layout.scss
  _forms.scss
  _buttons.scss
  _tables.scss
  _cards.scss
  _badges.scss
  _alerts.scss
  _utilities.scss
```

Y dejar `src/styles.scss` como archivo orquestador:

```scss
@use './styles/tokens';
@use './styles/reset';
@use './styles/typography';
@use './styles/base';
@use './styles/layout';
@use './styles/buttons';
@use './styles/forms';
@use './styles/cards';
@use './styles/tables';
@use './styles/badges';
@use './styles/alerts';
@use './styles/utilities';
```

## Componentes visuales a normalizar

### Prioridad alta

#### Botones

Definir variantes estándar:

- primario
- secundario
- ghost
- danger

#### Formularios

Unificar:

- inputs
- selects
- textareas
- labels
- help text
- error text
- field groups
- fieldsets

#### Cards / Surfaces

Unificar:

- paneles administrativos
- tarjetas de dashboard
- bloques de detalle
- resúmenes operativos

#### Badges / Pills

Unificar:

- estados de orden
- estados de pago
- activo / inactivo
- etiquetas de contexto

#### Tablas

Unificar:

- headers
- paddings
- hover
- responsive fallback
- acciones por fila

#### Headers y toolbars

Unificar:

- page header
- section header
- filtros
- acciones principales

## Layout privado recomendado

La aplicación privada debe evolucionar a un shell más sólido visualmente, sin romper la navegación actual.

### Ajustes recomendados

- sidebar más robusta y coherente con la marca
- topbar más limpia y consistente
- contenedor principal con ancho y respiración definidos
- fondo de app suave
- superficies claras con buena separación visual
- jerarquía más fuerte entre navegación, contexto y contenido

### Dirección sugerida

- sidebar dark/navy
- acciones principales en azul marca
- contenido sobre fondos claros
- tarjetas blancas con borde suave y sombra ligera
- títulos con mayor jerarquía
- metadatos en tono secundario controlado

## Reglas de implementación

### Evitar

- rediseñar pantalla por pantalla sin base común
- seguir agregando estilos inline en `styles: []` de componentes
- introducir nuevas decisiones visuales locales sin token o patrón reutilizable

### Hacer

- mover lo reusable a la capa global
- dejar estilos específicos solo para necesidades reales de pantalla
- migrar progresivamente componentes existentes al sistema común

## Fases recomendadas

### Fase 1. Fundación visual

Entregables:

- tokens
- tipografía
- reset/base
- layout primitives
- botones
- formularios
- cards
- badges
- tablas

Resultado esperado:

- ya existe una guía visual real y reutilizable para todo el sistema

### Fase 2. Migración de shell y pantallas clave

Orden sugerido:

1. `private-layout`
2. `login`
3. `dashboard`
4. `órdenes`
5. `clientes`
6. `pagos`
7. `admin usuarios/roles`

Resultado esperado:

- la aplicación privada empieza a sentirse consistente como producto

### Fase 3. Consolidación pública/privada

- revisar coherencia entre home, catálogo, login y privado
- ajustar densidad visual
- afinar contraste
- uniformar estados vacíos, carga y error
- reforzar responsive fino

## Siguiente paso recomendado

Antes de rediseñar pantallas individuales, conviene implementar la base del sistema visual y usarla primero en:

- `private-layout`
- `login`
- `dashboard`

Estos tres puntos definen la percepción general del producto y pueden servir como patrón para el resto de módulos.

## Recomendación final

La mejora visual de labDental debe abordarse como un **sistema de diseño ligero y transversal**, no como una colección de ajustes por componente. Ese enfoque reducirá retrabajo, mejorará coherencia y hará más fácil escalar el producto sin degradación visual.
