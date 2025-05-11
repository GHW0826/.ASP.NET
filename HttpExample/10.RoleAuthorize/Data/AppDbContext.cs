using RoleAuthorize.Models;
using Microsoft.EntityFrameworkCore;

namespace RoleAuthorize.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<TodoItem> Todos { get; set; }
    }
}
