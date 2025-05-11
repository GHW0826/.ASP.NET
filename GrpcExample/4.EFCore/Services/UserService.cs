using AutoMapper;
using Grpc.Core;
using EFCore.Models.Dto;
using EFCore.Repository;
using EFCore.Repositories;

namespace EFCore.Services;

public class UserService : EFCore.UserService.UserServiceBase
{
    private readonly IUserRepository _repo;
    private readonly IMapper _mapper;

    public UserService(IUserRepository repo, IMapper mapper)
    {
        _repo = repo;
        _mapper = mapper;
    }

    public override async Task<UserResponse> CreateUser(CreateUserRequest request, ServerCallContext context)
    {
        var entity = await _repo.CreateAsync(request.Name, request.Age);
        return _mapper.Map<UserResponse>(_mapper.Map<UserDto>(entity));
    }

    public override async Task<UserResponse> GetUser(GetUserRequest request, ServerCallContext context)
    {
        var entity = await _repo.GetAsync(request.Id)
            ?? throw new RpcException(new Status(StatusCode.NotFound, "User not found"));
        return _mapper.Map<UserResponse>(_mapper.Map<UserDto>(entity));
    }

    public override async Task<UserResponse> UpdateUser(UpdateUserRequest request, ServerCallContext context)
    {
        var entity = await _repo.UpdateAsync(request.Id, request.Name, request.Age)
            ?? throw new RpcException(new Status(StatusCode.NotFound, "User not found"));
        return _mapper.Map<UserResponse>(_mapper.Map<UserDto>(entity));
    }

    public override async Task<DeleteUserResponse> DeleteUser(DeleteUserRequest request, ServerCallContext context)
    {
        var result = await _repo.DeleteAsync(request.Id);
        return new DeleteUserResponse { Success = result };
    }
}
