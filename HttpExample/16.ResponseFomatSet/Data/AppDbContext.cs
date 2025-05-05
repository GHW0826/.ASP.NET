using ResponseFomatSet.Models;
using Microsoft.EntityFrameworkCore;

namespace ResponseFomatSet.Data
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
                 new User
                 {
                     Id = 1,
                     Username = "admin",
                     Password = "\"$2a$11$kM7GLqPSpXfr5A1MQ1gsPO9TnTuBlDkwFyMuTXElH6FSoTOINdwPq\"",
                     Role = "Admin",
                     RefreshToken = "dummy",
                     RefreshTokenExpireTime = new DateTime(2030, 1, 1)
                 },
                 new User
                 {
                     Id = 2,
                     Username = "user1",
                     Password = "\"$2a$11$kM7GLqPSpXfr5A1MQ1gsPO9TnTuBlDkwFyMuTXElH6FSoTOINdwPq\"",
                     Role = "User",
                     RefreshToken = "dummy2",
                     RefreshTokenExpireTime = new DateTime(2030, 1, 1)
                 }
            );
        }
    }
}
