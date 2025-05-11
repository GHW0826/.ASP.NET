using GrpcHealthcheck.Models;

namespace GrpcHealthcheck.Repositories;

public interface IUserRepository
{
    Task<User> CreateAsync(string name, int age);
    Task<User?> GetAsync(int id);
    Task<User?> UpdateAsync(int id, string name, int age);
    Task<bool> DeleteAsync(int id);
}