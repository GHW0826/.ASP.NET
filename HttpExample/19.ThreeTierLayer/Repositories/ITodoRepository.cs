using ThreeTierLayer.Models;

namespace ThreeTierLayer.Repositories;

public interface ITodoRepository
{
    Task<List<TodoItem>> GetAllAsync();
    Task<TodoItem?> GetByIdAsync(int id);
    Task AddAsync(TodoItem todo);
    Task UpdateAsync(TodoItem todo);
    Task DeleteAsync(TodoItem todo);
}
