using AutoMapper;
using Grpc.Core;
using FluentValidations.Models.Dto;
using FluentValidations.Repository;
using FluentValidations.Repositories;
using FluentValidation;

namespace FluentValidations.Services;

public class UserService : FluentValidations.UserService.UserServiceBase
{
    private readonly IUserRepository _repo;
    private readonly IMapper _mapper;
    private readonly IValidator<CreateUserRequest> _createValidator;
    private readonly IValidator<UpdateUserRequest> _updateValidator;

    public UserService(IUserRepository repo, IMapper mapper,
        IValidator<CreateUserRequest> createValidator,
        IValidator<UpdateUserRequest> updateValidator)
    {
        _repo = repo;
        _mapper = mapper;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    public override async Task<UserResponse> CreateUser(CreateUserRequest request, ServerCallContext context)
    {
        var validationResult = await _createValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
            throw new RpcException(new Status(StatusCode.InvalidArgument, validationResult.Errors[0].ErrorMessage));

        var entity = await _repo.CreateAsync(request.Name, request.Age);
        return _mapper.Map<UserResponse>(_mapper.Map<UserDto>(entity));
    }

    public override async Task<UserResponse> UpdateUser(UpdateUserRequest request, ServerCallContext context)
    {
        var validationResult = await _updateValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
            throw new RpcException(new Status(StatusCode.InvalidArgument, validationResult.Errors[0].ErrorMessage));

        var entity = await _repo.UpdateAsync(request.Id, request.Name, request.Age)
            ?? throw new RpcException(new Status(StatusCode.NotFound, "User not found"));

        return _mapper.Map<UserResponse>(_mapper.Map<UserDto>(entity));
    }


    public override async Task<UserResponse> GetUser(GetUserRequest request, ServerCallContext context)
    {
        var entity = await _repo.GetAsync(request.Id)
            ?? throw new RpcException(new Status(StatusCode.NotFound, "User not found"));
        return _mapper.Map<UserResponse>(_mapper.Map<UserDto>(entity));
    }


    public override async Task<DeleteUserResponse> DeleteUser(DeleteUserRequest request, ServerCallContext context)
    {
        var result = await _repo.DeleteAsync(request.Id);
        return new DeleteUserResponse { Success = result };
    }
}
