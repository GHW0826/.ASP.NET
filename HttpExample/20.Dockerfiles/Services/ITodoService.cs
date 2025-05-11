using Dockerfiles.Models.Dtos;

namespace Dockerfiles.Services;

public interface ITodoService
{
    Task<List<GetTodoDto>> GetAllAsync();
    Task<GetTodoDto?> GetByIdAsync(int id);
    Task<GetTodoDto> CreateAsync(CreateTodoDto dto);
    Task<bool> UpdateAsync(int id, UpdateTodoDto dto);
    Task<bool> DeleteAsync(int id);
}
