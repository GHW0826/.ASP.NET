using Microsoft.EntityFrameworkCore;
using Dockerfiles.Data;
using Dockerfiles.Models;

namespace Dockerfiles.Repositories;

public class TodoRepository : ITodoRepository
{
    private readonly AppDbContext _context;

    public TodoRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<TodoItem>> GetAllAsync() =>
        await _context.Todos.ToListAsync();

    public async Task<TodoItem?> GetByIdAsync(int id) =>
        await _context.Todos.FindAsync(id);

    public async Task AddAsync(TodoItem todo)
    {
        _context.Todos.Add(todo);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(TodoItem todo)
    {
        _context.Todos.Update(todo);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(TodoItem todo)
    {
        _context.Todos.Remove(todo);
        await _context.SaveChangesAsync();
    }
}
