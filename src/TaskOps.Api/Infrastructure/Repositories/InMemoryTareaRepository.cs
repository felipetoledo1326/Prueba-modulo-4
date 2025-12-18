using System.Collections.Concurrent;
using TaskOps.Api.Domain.Entities;
using TaskOps.Api.Domain.Repositories;

namespace TaskOps.Api.Infrastructure.Repositories;

public class InMemoryTareaRepository : ITareaRepository
{
    private readonly ConcurrentDictionary<Guid, Tarea> _tareas = new();

    public Task<IEnumerable<Tarea>> GetAllAsync()
    {
        return Task.FromResult(_tareas.Values.AsEnumerable());
    }

    public Task<Tarea?> GetByIdAsync(Guid id)
    {
        _tareas.TryGetValue(id, out var tarea);
        return Task.FromResult(tarea);
    }

    public Task AddAsync(Tarea tarea)
    {
        _tareas.TryAdd(tarea.Id, tarea);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Tarea tarea)
    {
        // For in-memory, since record is immutable, we replace the entry
        // In a real DB, we would attach/modify
        if (_tareas.ContainsKey(tarea.Id))
        {
            _tareas[tarea.Id] = tarea;
        }
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Guid id)
    {
        _tareas.TryRemove(id, out _);
        return Task.CompletedTask;
    }
}
