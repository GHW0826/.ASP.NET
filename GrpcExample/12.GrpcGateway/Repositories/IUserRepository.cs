using GrpcGateway.Models;

namespace GrpcGateway.Repositories;

public interface IUserRepository
{
    Task<User> CreateAsync(string name, int age);
    Task<User?> GetAsync(int id);
    Task<User?> UpdateAsync(int id, string name, int age);
    Task<bool> DeleteAsync(int id);
}