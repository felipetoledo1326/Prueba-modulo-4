# Análisis de Arquitectura: Entidad Tarea

[Estado: Analizado | Memoria: Optimizada]

## 1. Diagnóstico del Experto
**Calificación: [ACEPTABLE PERO MEJORABLE]**

La propuesta inicial de usar `Id`, `Titulo`, `Descripcion`, `Prioridad`, `Estado` y `Fecha` es correcta funcionalmente, pero desde una perspectiva de **Arquitectura de Alto Rendimiento en .NET 9**, hay puntos críticos a refinar:

1.  **Primitivos vs Tipos Fuertes**: Usar `int` para `Prioridad` (1-5) es un "code smell" (Primitive Obsession). Permite asignar un `999` por error. Propongo usar un `Enum` o un Value Object para garantizar la integridad del dominio.
2.  **Validación**: La longitud de 3-60 caracteres debe asegurarse tanto en el DTO de entrada como en la Entidad (Validación defensiva).
3.  **Inmutabilidad**: La elección de `record` es **ÓPTIMA** para DTOs y Entidades de dominio en escenarios de lectura intensiva, ya que nos da inmutabilidad "out of the box" y comparaciones estructurales rápidas, reduciendo la superficie de errores por mutación accidental.

## 2. Justificación Técnica (.NET 9 / C# 13)
*   **Records y `init`**: Usaremos `public record Tarea` con propiedades `init`. Esto permite que EF Core materialice objetos eficientemente pero impide modificaciones arbitrarias post-creación, favoreciendo un diseño Thread-Safe.
*   **Enums optimizados**: Al usar Enums para Estado y Prioridad, evitamos *Magic Numbers* y reducimos el uso de memoria comparado con strings.
*   **Primary Constructors (C# 12/13)**: Simplifican la declaración, pero para EF Core, a veces es preferible la sintaxis estándar con propiedades explícitas para atributos de validación (`[Key]`, `[MaxLength]`). Usaremos propiedades explícitas para mayor claridad con DataAnnotations.
*   **DTO Separation**: Separar `CreateTareaDto` (Input) de `TareaDto` (Output) es crucial para evitar *Over-Posting* (donde un usuario malicioso inyecta un ID o fecha fraudulenta).

## 3. Implementación de Referencia

A continuación se presentan los modelos que se generarán en el proyecto.

### Enum: TareaEstado
```csharp
public enum TareaEstado
{
    Pendiente = 0,
    EnProgreso = 1,
    Resuelta = 2
}
```

### Entity: Tarea (Clean Domain)
Notar el uso de atributos para validación y propiedades `required` de C# 11+ para garantizar que no existan tareas "a medias".

```csharp
public record Tarea
{
    public Guid Id { get; init; } = Guid.NewGuid();
    
    [MaxLength(60)] 
    public required string Titulo { get; init; } // Required garantiza inicialización
    
    public string? Descripcion { get; init; } // Nullable explícito
    
    [Range(1, 5)] 
    public int Prioridad { get; init; }
    
    public TareaEstado Estado { get; init; } = TareaEstado.Pendiente;
    
    public DateTimeOffset FechaCreacion { get; init; } = DateTimeOffset.UtcNow; // DateTimeOffset es mejor para sistemas distribuidos que DateTime
}
```

### DTOs (Data Transfer Objects)

**Entrada (Create):** Solo lo necesario.
```csharp
// FluentValidation sería ideal aquí, pero DataAnnotations funcionan nativamente con Minimal APIs
public record CreateTareaDto(
    [StringLength(60, MinimumLength = 3)] string Titulo,
    string? Descripcion,
    [Range(1, 5)] int Prioridad
);
```

**Salida (Response):** Lo que ve el cliente.
```csharp
public record TareaDto(
    Guid Id,
    string Titulo,
    string? Descripcion,
    string Estado, // Devolvemos string para el frontend (más legible) o int según contrato. Usualmente string es mejor para serialización JSON legible.
    int Prioridad,
    DateTimeOffset FechaCreacion
);
```

## 4. Checklist de Calidad
*   [ ] **Validar Fechas**: Usar siempre `DateTimeOffset.UtcNow` en lugar de `DateTime.Now` para evitar problemas de zona horaria en la nube (Docker/Kubernetes).
*   [ ] **Manejo de Nulos**: `Descripcion` es opcional (`string?`), el compilador nos advertirá si intentamos accederlo sin chequear.
*   [ ] **Serialización**: Configurar `JsonStringEnumConverter` en `Program.cs` para que los Enums se vean como texto ("EnProgreso") y no números (1) en el API, mejorando la DX (Developer Experience).
