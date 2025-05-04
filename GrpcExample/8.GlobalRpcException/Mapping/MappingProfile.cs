using AutoMapper;
using GlobalRpcException.Models;
using GlobalRpcException.Models.Dto;

namespace GlobalRpcException.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<User, UserDto>().ReverseMap();
        CreateMap<UserDto, UserResponse>().ReverseMap();
    }
}