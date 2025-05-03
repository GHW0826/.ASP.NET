using AutoMapper;
using JWTAuth.Models;
using JWTAuth.Models.Dto;

namespace JWTAuth.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<User, UserDto>().ReverseMap();
        CreateMap<UserDto, UserResponse>().ReverseMap();
    }
}