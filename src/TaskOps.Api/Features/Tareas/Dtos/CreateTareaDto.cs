using System.ComponentModel.DataAnnotations;

namespace TaskOps.Api.Features.Tareas.Dtos;

public record CreateTareaDto(
    [StringLength(60, MinimumLength = 3)] string Titulo,
    [MaxLength(200)] string? Descripcion,
    [Range(1, 5)] int Prioridad
);
