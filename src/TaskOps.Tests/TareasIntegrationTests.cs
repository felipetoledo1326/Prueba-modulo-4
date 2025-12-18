using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using TaskOps.Api.Features.Tareas.Dtos;
using TaskOps.Api.Domain.Enums;

namespace TaskOps.Tests;

public class TareasIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public TareasIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetHealth_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/health");
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("ok", content);
    }

    [Fact]
    public async Task PostTarea_Valid_ReturnsCreated()
    {
        var dto = new CreateTareaDto("Tarea Test", "Desc Test", 3);
        var response = await _client.PostAsJsonAsync("/api/tareas", dto);
        
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var tarea = await response.Content.ReadFromJsonAsync<TareaDto>();
        Assert.NotNull(tarea);
        Assert.Equal("Tarea Test", tarea.Titulo);
        Assert.NotEqual(Guid.Empty, tarea.Id);
    }

    [Fact]
    public async Task PostTarea_Invalid_ReturnsBadRequest()
    {
        // Título muy corto (<3) y Prioridad inválida (>5)
        var dto = new CreateTareaDto("A", "Desc", 99);
        var response = await _client.PostAsJsonAsync("/api/tareas", dto);
        
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        // Verificar formato custom
        Assert.Contains("error", content);
        Assert.Contains("detalles", content);
    }

    [Fact]
    public async Task GetTarea_NonExistent_ReturnsNotFound()
    {
        var response = await _client.GetAsync($"/api/tareas/{Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task PutTarea_Lifecycle_UpdatesAndPersists()
    {
        // 1. Crear
        var createDto = new CreateTareaDto("Para Editar", "Original", 1);
        var createRes = await _client.PostAsJsonAsync("/api/tareas", createDto);
        var created = await createRes.Content.ReadFromJsonAsync<TareaDto>();

        // 2. Actualizar (Cambiar estado a EnProgreso y Prioridad a 5)
        var updateDto = new UpdateTareaDto("Editado", 5, TareaEstado.EnProgreso);
        var updateRes = await _client.PutAsJsonAsync($"/api/tareas/{created!.Id}", updateDto);
        Assert.Equal(HttpStatusCode.OK, updateRes.StatusCode);

        // 3. Verificar persistencia con GET
        var getRes = await _client.GetAsync($"/api/tareas/{created.Id}");
        var final = await getRes.Content.ReadFromJsonAsync<TareaDto>();
        
        Assert.Equal("Editado", final!.Descripcion);
        Assert.Equal(5, final.Prioridad);
        Assert.Equal(TareaEstado.EnProgreso.ToString(), final.Estado);
    }

    [Fact]
    public async Task DeleteTarea_Lifecycle_Removes()
    {
        // 1. Crear
        var createDto = new CreateTareaDto("Para Borrar", null, 1);
        var createRes = await _client.PostAsJsonAsync("/api/tareas", createDto);
        var created = await createRes.Content.ReadFromJsonAsync<TareaDto>();

        // 2. Borrar
        var deleteRes = await _client.DeleteAsync($"/api/tareas/{created!.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteRes.StatusCode);

        // 3. Verificar que ya no existe
        var getRes = await _client.GetAsync($"/api/tareas/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getRes.StatusCode);
    }
}
