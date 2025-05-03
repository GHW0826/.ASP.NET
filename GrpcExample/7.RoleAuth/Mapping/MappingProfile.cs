using AutoMapper;
using RoleAuth.Models;
using RoleAuth.Models.Dto;

namespace RoleAuth.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<User, UserDto>().ReverseMap();
        CreateMap<UserDto, UserResponse>().ReverseMap();
    }
}