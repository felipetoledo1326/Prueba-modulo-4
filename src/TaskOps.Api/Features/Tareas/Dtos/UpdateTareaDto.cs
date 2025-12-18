using System.ComponentModel.DataAnnotations;
using TaskOps.Api.Domain.Enums;

namespace TaskOps.Api.Features.Tareas.Dtos;

public record UpdateTareaDto(
    [MaxLength(200)] string? Descripcion,
    [Range(1, 5)] int Prioridad,
    TareaEstado Estado
);
