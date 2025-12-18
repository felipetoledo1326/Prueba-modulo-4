# Bonus: Middleware de Trazabilidad (X-Request-Id)

[Estado: Implementado | Tipo: Middleware | Valor: Observabilidad]

## 1. Descripción del Requerimiento
Para mejorar la observabilidad y el debugging en producción, se requiere que cada respuesta HTTP incluya un identificador único. Esto permite rastrear logs distribuidos (Distributed Tracing) correlacionando peticiones del cliente con logs del servidor.

## 2. Solución Técnica
Implementamos un **Middleware Inline** en el pipeline de ASP.NET Core (`Program.cs`).
Este middleware intercepta cada solicitud entrante, genera un `Guid`, y lo inyecta en los headers de la respuesta antes de pasar el control al siguiente componente (`next`).

### Código Implementado
```csharp
app.Use(async (context, next) =>
{
    // Generar ID único
    var requestId = Guid.NewGuid().ToString();
    
    // Agregar al Header de respuesta
    context.Response.Headers.TryAdd("X-Request-Id", requestId);
    
    // Continuar con el pipeline
    await next(context);
});
```

## 3. Beneficios
*   **Trazabilidad**: Permite al cliente reportar errores dando un ID específico ("Mi request ABC falló").
*   **Simplicidad**: Al ser un middleware global, aplica a **todos** los endpoints (Swagger, Health, CRUD) sin modificar cada controlador.
*   **Performance**: Operación O(1) con impacto despreciable en la latencia.
