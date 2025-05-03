using AutoMapper;
using EFCore.Models;
using EFCore.Models.Dto;

namespace EFCore.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<User, UserDto>().ReverseMap();
        CreateMap<UserDto, UserResponse>().ReverseMap();
    }
}