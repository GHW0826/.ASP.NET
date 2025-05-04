using GlobalRpcException.Models;
using Microsoft.EntityFrameworkCore;

namespace GlobalRpcException.Context;


public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
}