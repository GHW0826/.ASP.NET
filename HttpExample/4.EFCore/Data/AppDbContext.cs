using _4.EFCore.Models;
using Microsoft.EntityFrameworkCore;

namespace _4.EFCore.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<TodoItem> Todos { get; set; }
    }
}
