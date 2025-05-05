using _6.GlobalExceptionMiddleware.Models;
using Microsoft.EntityFrameworkCore;

namespace _6.GlobalExceptionMiddleware.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<TodoItem> Todos { get; set; }
    }
}
