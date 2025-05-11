using GrpcCompression.Models;
using Microsoft.EntityFrameworkCore;

namespace GrpcCompression.Context;


public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
}