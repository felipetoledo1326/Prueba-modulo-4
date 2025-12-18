using System.ComponentModel.DataAnnotations;
using TaskOps.Api.Domain.Enums;

namespace TaskOps.Api.Domain.Entities;

public record Tarea
{
    public Guid Id { get; init; } = Guid.NewGuid();

    [MaxLength(60)]
    public required string Titulo { get; init; }

    public string? Descripcion { get; init; }

    [Range(1, 5)]
    public int Prioridad { get; init; }

    public TareaEstado Estado { get; init; } = TareaEstado.Pendiente;

    public DateTimeOffset FechaCreacion { get; init; } = DateTimeOffset.UtcNow;
}
