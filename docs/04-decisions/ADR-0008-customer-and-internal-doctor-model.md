# ADR-0008: Modelar clientes como Customer con tipo y doctores internos para clínicas

## Estado

Aceptada para MVP.

## Contexto

El Excel actual organiza la operación por doctor, doctora o clínica. Algunas clínicas pueden contener varios doctores internos. El sistema debe sustituir las hojas del Excel sin atarse a esa estructura.

## Decisión

Crear `Customer` como entidad flexible con `Type = Doctor`, `Clinic` u `Other`. Crear `InternalDoctor` como entidad hija únicamente para `Customers` tipo `Clinic`. No se usará borrado físico; los clientes y doctores internos se activan o desactivan.

## Consecuencias Positivas

- Permite representar doctores individuales y clínicas.
- Evita crear una hoja/tabla por doctor.
- Deja preparada la relación futura con órdenes de trabajo.
- Reduce riesgo de pérdida de histórico al evitar delete físico.

## Consecuencias Negativas

- Requiere validar reglas al cambiar tipo de cliente.
- Puede haber nombres duplicados; se manejarán con búsqueda y datos de contacto.
- Requiere criterio operativo para clínicas con doctores internos.

## Alternativas Consideradas

- Crear una entidad `Doctor` separada para todo.
- Modelar clínicas y doctores como tablas totalmente independientes.
- Forzar unicidad global de nombres.
- Permitir borrado físico.
