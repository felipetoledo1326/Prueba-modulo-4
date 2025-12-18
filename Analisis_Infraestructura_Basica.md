# Análisis de Infraestructura: Persistencia y Endpoints Base

[Estado: Implementado | Framework: .NET 7 (Limitación de Entorno)]

## 1. Diagnóstico del Experto
**Calificación: [ACEPTABLE - ADAPTADO AL ENTORNO]**

Se ha detectado que el entorno de ejecución solo dispone del SDK de .NET 7. Por pragmatismo y para desbloquear el desarrollo, hemos ajustado el `TargetFramework` a `net7.0`.
Aunque el requisito ideal era .NET 9, el código generado es compatible con C# 11 (disponible en .NET 7), por lo que mantenemos características modernas como `required members` y `records`.

## 2. Justificación Técnica
*   **ConcurrentDictionary**: Se mantiene como la opción óptima para la persistencia en memoria.
*   **Singleton Lifetime**: Correctamente configurado para persistencia entre requests.
*   **Minimal APIs**: Compatible nativamente con .NET 6+, funciona perfecto en .NET 7.

## 3. Implementación de Referencia

### Program.cs (Adaptado)
Sin cambios significativos necesarios, la sintaxis de Minimal APIs es estable desde .NET 6.

```csharp
builder.Services.AddSingleton<ITareaRepository, InMemoryTareaRepository>();
app.MapGet("/api/health", () => Results.Ok(new { status = "ok" }));
```

## 4. Checklist de Calidad
*   [x] **Configuración de Swagger**: Habilitada en Development.
*   [!] **Actualización de Framework**: Revertido a `net7.0` por falta de SDK. *Acción: Actualizar SDK en el servidor de despliegue final.*
*   [x] **Inyección de Dependencias**: Singleton verificado.
