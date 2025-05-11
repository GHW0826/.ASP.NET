using BIStreamingChat.Context;
using BIStreamingChat.Models;
using BIStreamingChat.Repositories;

namespace BIStreamingChat.Repository;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _db;

    public UserRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<User> CreateAsync(string name, int age)
    {
        var user = new User { Name = name, Age = age };
        _db.Users.Add(user);
        await _db.SaveChangesAsync();
        return user;
    }

    public async Task<User?> GetAsync(int id)
    {
        return await _db.Users.FindAsync(id);
    }

    public async Task<User?> UpdateAsync(int id, string name, int age)
    {
        var user = await _db.Users.FindAsync(id);
        if (user == null) return null;

        user.Name = name;
        user.Age = age;
        await _db.SaveChangesAsync();
        return user;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var user = await _db.Users.FindAsync(id);
        if (user == null) return false;

        _db.Users.Remove(user);
        await _db.SaveChangesAsync();
        return true;
    }
}
