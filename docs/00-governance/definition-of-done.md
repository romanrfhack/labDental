# Definition Of Done

Ninguna implementación se considera terminada si no actualiza la documentación afectada. La documentación forma parte del entregable, no es una actividad posterior opcional.

## Checklist De Código

- El cambio está limitado al alcance aprobado.
- No introduce cambios breaking sin ADR o decisión explícita.
- Sigue la arquitectura vigente.
- Mantiene nombres, estilos y convenciones del proyecto.

## Checklist De Pruebas

- Incluye pruebas acordes al riesgo del cambio.
- Cubre reglas de negocio modificadas.
- Verifica casos de error relevantes.
- No deja pruebas fallando o deshabilitadas sin justificación.

## Checklist De Documentación

- Actualiza documentos de producto, dominio, arquitectura u operaciones afectados.
- Actualiza roadmap si cambia alcance o fase.
- Actualiza changelog con cambios reales.
- No documenta funcionalidades no implementadas como si ya existieran.

## Checklist De Reglas De Negocio

- Toda regla nueva o modificada queda documentada.
- Los cálculos de saldos, pagos y estados se mantienen trazables.
- Las excepciones quedan descritas con validación manual cuando aplique.

## Checklist De Decisiones Técnicas

- Las decisiones relevantes se registran como ADR.
- Las alternativas descartadas quedan resumidas cuando afecten evolución futura.
- Las decisiones favorecen cambios incrementales, seguros y no breaking.

## Checklist De Validación Manual

- Se valida el flujo principal afectado.
- Se revisan permisos requeridos.
- Se revisa impacto en datos existentes.
- Se confirma que la documentación coincide con el comportamiento entregado.
