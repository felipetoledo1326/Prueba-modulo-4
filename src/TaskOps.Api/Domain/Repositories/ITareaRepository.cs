using TaskOps.Api.Domain.Entities;

namespace TaskOps.Api.Domain.Repositories;

public interface ITareaRepository
{
    Task<IEnumerable<Tarea>> GetAllAsync();
    Task<Tarea?> GetByIdAsync(Guid id);
    Task AddAsync(Tarea tarea);
    Task UpdateAsync(Tarea tarea);
    Task DeleteAsync(Guid id);
}
