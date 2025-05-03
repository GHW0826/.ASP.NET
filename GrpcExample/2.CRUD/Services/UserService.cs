using Grpc.Core;
using CRUD;

namespace CRUD.Services;

public class UserService : CRUD.UserService.UserServiceBase
{
    private readonly UserRepository _repo;

    public UserService(UserRepository repo)
    {
        _repo = repo;
    }

    public override Task<UserResponse> CreateUser(CreateUserRequest request, ServerCallContext context)
    {
        var user = _repo.Create(request.Name, request.Age);
        return Task.FromResult(new UserResponse { Id = user.Id, Name = user.Name, Age = user.Age });
    }

    public override Task<UserResponse> GetUser(GetUserRequest request, ServerCallContext context)
    {
        var user = _repo.Get(request.Id);
        if (user == null)
            throw new RpcException(new Status(StatusCode.NotFound, "User not found"));

        return Task.FromResult(new UserResponse { Id = user.Id, Name = user.Name, Age = user.Age });
    }

    public override Task<UserResponse> UpdateUser(UpdateUserRequest request, ServerCallContext context)
    {
        var user = _repo.Update(request.Id, request.Name, request.Age);
        if (user == null)
            throw new RpcException(new Status(StatusCode.NotFound, "User not found"));

        return Task.FromResult(new UserResponse { Id = user.Id, Name = user.Name, Age = user.Age });
    }

    public override Task<DeleteUserResponse> DeleteUser(DeleteUserRequest request, ServerCallContext context)
    {
        var success = _repo.Delete(request.Id);
        return Task.FromResult(new DeleteUserResponse { Success = success });
    }
}