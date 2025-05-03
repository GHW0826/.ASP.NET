using HealthCheckEndpoint.Models;
using Microsoft.EntityFrameworkCore;

namespace HealthCheckEndpoint.Context;


public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
}