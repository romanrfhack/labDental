# Módulo De Repartidores Y Etiquetas

## Objetivo

Habilitar un flujo operativo para controlar entregas de órdenes dentales mediante repartidores, etiquetas QR o código, escaneo desde celular y evidencia de recibido.

El módulo busca conectar la orden física con el registro digital, permitiendo saber qué se entregó, quién lo entregó, quién recibió y qué evidencia respalda el cierre.

## Flujo Operativo

1. El administrador identifica una orden lista para entrega.
2. El sistema genera o muestra una etiqueta con QR o código.
3. La etiqueta se imprime desde navegador y se coloca en la orden o paquete.
4. El administrador asigna la entrega a un repartidor.
5. El repartidor consulta sus entregas desde celular.
6. El repartidor marca la entrega como en ruta.
7. El repartidor escanea el QR o código desde celular.
8. El repartidor registra el resultado de entrega.
9. Si la entrega fue exitosa, captura nombre de quien recibe y evidencia.
10. La evidencia puede ser firma o fotografía de recibido.
11. El sistema registra el evento en historial por orden y por repartidor.

## Estados De Entrega

- Pendiente de asignar.
- Asignada.
- En ruta.
- Entregada.
- No entregada.
- Cancelada.

## Etiquetas Con QR/Código

Cada orden podrá tener una etiqueta con QR o código para identificarla rápidamente. La etiqueta permitirá consultar o confirmar la orden durante el flujo de entrega, reduciendo errores de captura manual.

El formato final de etiqueta deberá validarse con el cliente antes de producción.

## Impresión Desde Navegador

La primera ronda incluye impresión básica de etiquetas desde navegador. Esto significa que el usuario podrá abrir la vista de etiqueta e imprimir usando las capacidades normales del navegador y la impresora configurada en el equipo.

## Servicio Local De Impresión

Un servicio local de impresión puede evaluarse como opción adicional. No se considera incluido de forma automática porque depende de validar:

- Modelo de impresora.
- Sistema operativo.
- Red local.
- Tamaño y tipo de etiqueta.
- Drivers disponibles.
- Flujo real de impresión del laboratorio.

## App Web Responsive/PWA Para Repartidores

El módulo de repartidores se propone como web responsive/PWA. El repartidor accederá desde el navegador del celular, sin requerir una app móvil nativa.

Funciones esperadas:

- Login privado.
- Consulta de entregas asignadas.
- Cambio de estado de entrega.
- Escaneo QR/código desde cámara del celular, sujeto a compatibilidad del dispositivo y navegador.
- Captura de evidencia.
- Consulta de historial básico.

## Escaneo Desde Celular

El escaneo permitirá identificar la orden mediante QR o código. El objetivo es reducir captura manual y confirmar que la entrega corresponde a la orden correcta.

## Evidencia Con Firma

La entrega podrá cerrarse con firma de recibido cuando el flujo operativo lo requiera. La firma quedará asociada al registro de entrega.

## Evidencia Con Fotografía

La entrega podrá cerrarse con fotografía de recibido o evidencia visual. El almacenamiento será razonable para la operación inicial y podrá ajustarse si el volumen crece.

## Nombre De Quien Recibe

El repartidor deberá capturar el nombre de quien recibe la entrega. Este dato formará parte del historial y servirá como respaldo operativo.

## Historial Por Orden

Cada orden podrá mostrar los eventos de entrega relacionados:

- Asignación.
- Inicio de ruta.
- Intento de entrega.
- Entrega exitosa.
- Entrega no realizada.
- Evidencia capturada.
- Usuario o repartidor responsable.

## Historial Por Repartidor

El sistema podrá consultar entregas asociadas a cada repartidor, permitiendo revisar trabajo asignado, entregado, no entregado o cancelado.

## Panel Administrativo De Entregas

El administrador podrá consultar y gestionar entregas:

- Ver entregas pendientes.
- Asignar repartidor.
- Consultar estado.
- Revisar evidencia.
- Consultar historial.
- Identificar entregas no realizadas.

## Exclusiones

- App móvil nativa.
- Optimización de rutas.
- Geolocalización avanzada.
- Seguimiento en tiempo real.
- Integración con mapas.
- Hardware.
- Impresoras.
- Etiquetas físicas.
- Lectores.
- Consumibles.
