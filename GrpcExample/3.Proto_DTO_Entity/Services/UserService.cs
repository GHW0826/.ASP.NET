using AutoMapper;
using Grpc.Core;
using Proto_DTO_Entity.Models.Dto;

namespace Proto_DTO_Entity.Services;

public class UserService : Proto_DTO_Entity.UserService.UserServiceBase
{
    private readonly UserRepository _repo;
    private readonly IMapper _mapper;

    public UserService(UserRepository repo, IMapper mapper)
    {
        _repo = repo;
        _mapper = mapper;
    }

    public override Task<UserResponse> CreateUser(CreateUserRequest request, ServerCallContext context)
    {
        var entity = _repo.Create(request.Name, request.Age);
        var dto = _mapper.Map<UserDto>(entity);
        return Task.FromResult(_mapper.Map<UserResponse>(dto));
    }

    public override Task<UserResponse> GetUser(GetUserRequest request, ServerCallContext context)
    {
        var entity = _repo.Get(request.Id) ?? throw new RpcException(new Status(StatusCode.NotFound, "User not found"));
        var dto = _mapper.Map<UserDto>(entity);
        return Task.FromResult(_mapper.Map<UserResponse>(dto));
    }

    public override Task<UserResponse> UpdateUser(UpdateUserRequest request, ServerCallContext context)
    {
        var entity = _repo.Update(request.Id, request.Name, request.Age) ?? throw new RpcException(new Status(StatusCode.NotFound, "User not found"));
        var dto = _mapper.Map<UserDto>(entity);
        return Task.FromResult(_mapper.Map<UserResponse>(dto));
    }

    public override Task<DeleteUserResponse> DeleteUser(DeleteUserRequest request, ServerCallContext context)
    {
        var result = _repo.Delete(request.Id);
        return Task.FromResult(new DeleteUserResponse { Success = result });
    }
}