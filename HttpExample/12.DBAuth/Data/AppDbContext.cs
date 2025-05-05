using DBAuth.Models;
using Microsoft.EntityFrameworkCore;

namespace DBAuth.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<TodoItem> Todos { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>().HasData(
                new User { Id = 1, Username = "admin", Password = "1234", Role = "Admin" },
                new User { Id = 2, Username = "user1", Password = "1111", Role = "User" }
            );
        }
    }
}
