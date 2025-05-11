using ThreeTierLayer.Models;
using ThreeTierLayer.Models.Dtos;
using ThreeTierLayer.Repositories;

namespace ThreeTierLayer.Services;

public class TodoService : ITodoService
{
    private readonly ITodoRepository _repo;

    public TodoService(ITodoRepository repo)
    {
        _repo = repo;
    }

    public async Task<List<GetTodoDto>> GetAllAsync()
    {
        var todos = await _repo.GetAllAsync();
        return todos.Select(t => new GetTodoDto
        {
            Id = t.Id,
            Title = t.Title,
            IsComplete = t.IsComplete
        }).ToList();
    }

    public async Task<GetTodoDto?> GetByIdAsync(int id)
    {
        var todo = await _repo.GetByIdAsync(id);
        if (todo == null) return null;

        return new GetTodoDto
        {
            Id = todo.Id,
            Title = todo.Title,
            IsComplete = todo.IsComplete
        };
    }

    public async Task<GetTodoDto> CreateAsync(CreateTodoDto dto)
    {
        var todo = new TodoItem { Title = dto.Title, IsComplete = false };
        await _repo.AddAsync(todo);

        return new GetTodoDto
        {
            Id = todo.Id,
            Title = todo.Title,
            IsComplete = todo.IsComplete
        };
    }

    public async Task<bool> UpdateAsync(int id, UpdateTodoDto dto)
    {
        var todo = await _repo.GetByIdAsync(id);
        if (todo == null) return false;

        todo.Title = dto.Title ?? todo.Title;
        todo.IsComplete = dto.IsComplete;
        await _repo.UpdateAsync(todo);
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var todo = await _repo.GetByIdAsync(id);
        if (todo == null) return false;

        await _repo.DeleteAsync(todo);
        return true;
    }
}

