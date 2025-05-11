using _5.FluentValidation.Models;
using Microsoft.EntityFrameworkCore;

namespace _5.FluentValidation.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<TodoItem> Todos { get; set; }
    }
}
