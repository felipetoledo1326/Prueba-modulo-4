using TaskOps.Api.Domain.Repositories;
using TaskOps.Api.Infrastructure.Repositories;
using System.ComponentModel.DataAnnotations;
using TaskOps.Api.Domain.Entities;
using TaskOps.Api.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "TaskOps API", Version = "v1" });
});

builder.Services.AddSingleton<ITareaRepository, InMemoryTareaRepository>();

var app = builder.Build();

// Bonus: Middleware X-Request-Id
app.Use(async (context, next) =>
{
    context.Response.Headers.TryAdd("X-Request-Id", Guid.NewGuid().ToString());
    await next(context);
});

// Swagger enabled for all environments for Demo purposes
app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "TaskOps API v1"));

app.UseHttpsRedirection();

app.MapGet("/api/health", () => Results.Ok(new { status = "ok" }))
   .WithName("HealthCheck")
   .WithOpenApi();

// POST /api/tareas
app.MapPost("/api/tareas", async (
    TaskOps.Api.Features.Tareas.Dtos.CreateTareaDto dto, 
    ITareaRepository repository) =>
{
    var validationResults = new List<ValidationResult>();
    var validationContext = new ValidationContext(dto);
    
    if (!Validator.TryValidateObject(dto, validationContext, validationResults, true))
    {
        var errores = validationResults.Select(vr => vr.ErrorMessage).ToList();
        return Results.BadRequest(new { error = "Datos inválidos", detalles = errores });
    }

    var tarea = new Tarea
    {
        Titulo = dto.Titulo,
        Descripcion = dto.Descripcion,
        Prioridad = dto.Prioridad
    };

    await repository.AddAsync(tarea);

    var responseDto = mapToDto(tarea);
    return Results.Created($"/api/tareas/{tarea.Id}", responseDto);
})
.WithName("CreateTarea")
.WithOpenApi();

// GET /api/tareas/{id}
app.MapGet("/api/tareas/{id}", async (Guid id, ITareaRepository repository) =>
{
    var tarea = await repository.GetByIdAsync(id);
    return tarea is not null ? Results.Ok(mapToDto(tarea)) : Results.NotFound();
})
.WithName("GetTareaById")
.WithOpenApi();

// GET /api/tareas (Filters)
app.MapGet("/api/tareas", async (
    ITareaRepository repository, 
    [FromQuery] TareaEstado? estado, 
    [FromQuery] int? prioridad) =>
{
    var tareas = await repository.GetAllAsync();

    if (estado.HasValue)
    {
        tareas = tareas.Where(t => t.Estado == estado.Value);
    }

    if (prioridad.HasValue)
    {
        tareas = tareas.Where(t => t.Prioridad == prioridad.Value);
    }

    return Results.Ok(tareas.Select(mapToDto));
})
.WithName("GetTareas")
.WithOpenApi();

// PUT /api/tareas/{id}
app.MapPut("/api/tareas/{id}", async (
    Guid id, 
    TaskOps.Api.Features.Tareas.Dtos.UpdateTareaDto dto, 
    ITareaRepository repository) =>
{
    var existingTarea = await repository.GetByIdAsync(id);
    if (existingTarea is null) return Results.NotFound();

    // Immutable record update using "with" expression
    var updatedTarea = existingTarea with 
    { 
        Descripcion = dto.Descripcion ?? existingTarea.Descripcion,
        Prioridad = dto.Prioridad,
        Estado = dto.Estado 
    };

    await repository.UpdateAsync(updatedTarea);

    return Results.Ok(mapToDto(updatedTarea));
})
.WithName("UpdateTarea")
.WithOpenApi();

// DELETE /api/tareas/{id}
app.MapDelete("/api/tareas/{id}", async (Guid id, ITareaRepository repository) =>
{
    var existingTarea = await repository.GetByIdAsync(id);
    if (existingTarea is null) return Results.NotFound();

    await repository.DeleteAsync(id);
    return Results.NoContent();
})
.WithName("DeleteTarea")
.WithOpenApi();

// Helper for mapping
TaskOps.Api.Features.Tareas.Dtos.TareaDto mapToDto(Tarea t) => new(
    t.Id, t.Titulo, t.Descripcion, t.Estado.ToString(), t.Prioridad, t.FechaCreacion
);


app.Run();

public partial class Program { }
