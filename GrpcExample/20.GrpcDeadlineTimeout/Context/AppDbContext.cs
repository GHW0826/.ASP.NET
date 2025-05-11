using GrpcDeadlineTimeout.Models;
using Microsoft.EntityFrameworkCore;

namespace GrpcDeadlineTimeout.Context;


public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
}