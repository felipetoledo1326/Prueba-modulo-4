namespace TaskOps.Api.Features.Tareas.Dtos;

public record TareaDto(
    Guid Id,
    string Titulo,
    string? Descripcion,
    string Estado,
    int Prioridad,
    DateTimeOffset FechaCreacion
);
