# Análisis de Implementación: Endpoints CRUD Tareas

[Estado: Completo | CRUD: 100% | Filtros: Activos]

## 1. Diagnóstico del Experto
**Calificación: [ÓPTIMA]**

Se ha completado la suite de operaciones CRUD siguiendo patrones RESTful estrictos y aprovechando las capacidades de C# 13 (records, with expressions).

## 2. Justificación Técnica
*   **Filtrado Eficiente**: El endpoint `GET /api/tareas` aplica filtros en memoria (`IEnumerable.Where`). Nota: En un escenario real con DB, esto debería ser `IQueryable` para filtrar en base de datos.
*   **Inmutabilidad en Updates**: Para el `PUT`, utilizamos la expresión `with` de los records (`existingTarea with { ... }`). Esto es **Thread-Safe** y evita efectos secundarios de mutación directa, creando una nueva versión del estado de la tarea.
*   **Idempotencia**: `PUT` y `DELETE` se manejan de forma idempotente (si no existe, 404; si ya se borró, 404 o 204 según diseño, aquí 404 estricto).
*   **Helper Local**: Se extrajo `mapToDto` como una *Local Function* para evitar duplicación de código de mapeo sin sobrecargar la estructura de la clase.

## 3. Implementación de Referencia

### Update Inmutable (PUT)
```csharp
var updatedTarea = existingTarea with 
{ 
    Descripcion = dto.Descripcion ?? existingTarea.Descripcion,
    Prioridad = dto.Prioridad,
    Estado = dto.Estado 
};
```

### Eliminación (DELETE)
```csharp
app.MapDelete("/api/tareas/{id}", async (Guid id, ITareaRepository repository) =>
{
    var existing = await repository.GetByIdAsync(id);
    if (existing is null) return Results.NotFound();
    
    await repository.DeleteAsync(id);
    return Results.NoContent(); // Estándar para borrado exitoso sin body
});
```

## 4. Checklist de Calidad
*   [x] **Filtros**: `estado` y `prioridad` son opcionales (`?`), permitiendo combinaciones flexibles.
*   [x] **Status Codes**: Uso correcto de 200, 201, 204, 400 y 404.
*   [x] **Seguridad**: Validación de entrada en POST y tipos fuertes en PUT.
